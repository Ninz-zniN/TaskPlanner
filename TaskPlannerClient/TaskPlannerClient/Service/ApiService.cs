using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TaskPlannerClient.Models;
using TaskPlannerClient.Models.Dto;

namespace TaskPlannerClient.Service
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            DateFormatString = "yyyy-MM-dd", //для всех datetime
            DateTimeZoneHandling = DateTimeZoneHandling.Local,
        };

        public ApiService()
        {
            _httpClient = new HttpClient();
            _baseUrl = LoadBaseUrl();
            JsonConvert.DefaultSettings = () => _jsonSettings;
        }

        public void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private static string LoadBaseUrl()
        {
            try
            {
                string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (System.IO.File.Exists(configPath))
                {
                    var json = System.IO.File.ReadAllText(configPath);
                    var config = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (config != null && config.TryGetValue("ApiBaseUrl", out var url))
                        return url;
                }
            }
            catch { /* любая ошибка — используем localhost */ }
            return "http://localhost:3001/api";
        }

        // <summary>
        /// Проверяет ответ сервера и выбрасывает исключение с русским текстом ошибки.
        /// </summary>
        private async Task HandleResponseAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = $"Ошибка {response.StatusCode}";
                try
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var errorObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);
                    if (errorObj != null && errorObj.TryGetValue("error", out var err) && err is JObject errDetails)
                    {
                        var msg = errDetails["message"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(msg))
                            errorMessage = msg;
                    }
                }
                catch { /* если не удалось распарсить, оставляем стандартное сообщение */ }
                throw new HttpRequestException(errorMessage);
            }
        }

        // ==================== Аутентификация ====================
        public async Task<LoginResponse> LoginAsync(string login, string password)
        {
            var request = new LoginRequest { Login = login, Password = password };
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/auth/login", content);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<LoginResponse>(body);
        }
        #region Задачи
        // ==================== Задачи ====================
        public async Task<List<TaskItem>> GetTasksAsync(
            int? assigneeId = null,
            int? teamId = null,
            bool includeUnassigned = true,
            int? previousAssigneeId = null)
        {
            var queryParams = new List<string>();
            if (assigneeId.HasValue) queryParams.Add($"assignee_id={assigneeId}");
            if (teamId.HasValue) queryParams.Add($"team_id={teamId}");
            if (!includeUnassigned) queryParams.Add($"include_unassigned=false");
            if (previousAssigneeId.HasValue) queryParams.Add($"previous_assignee_id={previousAssigneeId}");

            string url = $"{_baseUrl}/tasks";
            if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TaskListResponse>(body);
            return result.Items;
        }
        public async Task<TaskItem> UpdateTaskStatusAsync(int taskId, int newStatusId, DateTime? changedAt=null, decimal? actualHours=null)
        {
            var payload = new Dictionary<string, object>
            {
                { "id_task_status", newStatusId }
            };
            if (changedAt.HasValue)
                payload["changed_at"] = changedAt.Value.ToString("yyyy-MM-dd HH:mm:ss");
            if (actualHours.HasValue)
                payload["actual_hours"] = actualHours.Value;

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"{_baseUrl}/tasks/{taskId}/status", content);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TaskItem>(body);
        }

        public async Task<TaskItem> UpdateTaskDescriptionAsync(int taskId, string description)
        {
            var payload = new { description };
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"{_baseUrl}/tasks/{taskId}", content);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TaskItem>(body);
        }

        // ==================== Задачи (расширенные) ====================

        public async Task<TaskItem> CreateTaskAsync(CreateTaskDto dto)
        {
            var json = JsonConvert.SerializeObject(dto, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/tasks", content);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TaskItem>(body);
        }

        public async Task<TaskItem> UpdateTaskAsync(int taskId, TaskUpdateDto dto)
        {
            // 1. Обновляем поля задачи (без статуса)
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(dto, settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"{_baseUrl}/tasks/{taskId}", content);
            await HandleResponseAsync(response);

            // 2. Если в DTO был передан статус — отдельно вызываем метод, который пишет историю
            if (dto.IdTaskStatus.HasValue)
                await UpdateTaskStatusAsync(taskId, dto.IdTaskStatus.Value);

            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TaskItem>(body);
        }

        public async Task DeleteTaskAsync(int taskId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/tasks/{taskId}");
            await HandleResponseAsync(response);
        }

        public async Task<TaskItem> AssignTaskAsync(int taskId, int employeeId)
        {
            var payload = new { employee_id = employeeId };
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync($"{_baseUrl}/tasks/{taskId}/assign", content);
            await HandleResponseAsync(response);

            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TaskItem>(body);
        }
        #endregion

        #region Сотрудники
        // ==================== Сотрудники ====================
        public async Task<List<Employee>> GetEmployeesAsync(bool includeDismissed = false)
        {
            string url = $"{_baseUrl}/employees";
            if (includeDismissed) url += "?include_dismissed=true";
            var response = await _httpClient.GetAsync(url);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<EmployeeListResponse>(body);
            return result.Items;
        }

        public async Task<Employee> CreateEmployeeAsync(CreateEmployeeDto dto)
        {
            var json = JsonConvert.SerializeObject(dto, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/employees", content);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Employee>(body);
        }

        public async Task<Employee> UpdateEmployeeAsync(int employeeId, EmployeeUpdateDto dto)
        {
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(dto, settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"{_baseUrl}/employees/{employeeId}", content);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Employee>(body);
        }

        public async Task DismissEmployeeAsync(int employeeId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/employees/{employeeId}");
            await HandleResponseAsync(response);
        }
        #endregion

        #region Справочники
        // ==================== Справочники ====================
        public async Task<List<ReferenceItem>> GetTaskTypesAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/references/task-types");
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            var wrapper = JsonConvert.DeserializeObject<Dictionary<string, List<ReferenceItem>>>(body);
            return wrapper["items"];
        }

        public async Task<List<ReferenceItem>> GetTaskStatusesAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/references/task-statuses");
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            var wrapper = JsonConvert.DeserializeObject<Dictionary<string, List<ReferenceItem>>>(body);
            return wrapper["items"];
        }

        public async Task<List<PriorityItem>> GetPrioritiesAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/references/priorities");
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            var wrapper = JsonConvert.DeserializeObject<Dictionary<string, List<PriorityItem>>>(body);
            return wrapper["items"];
        }
        #endregion

        #region Спринты
        // ==================== Спринты ====================
        public async Task<List<Sprint>> GetSprintsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/sprints");
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<SprintListResponse>(body);
            return result.Items;
        }

        public async Task<Sprint> GetActiveSprintAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/sprints/active");
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Sprint>(body);
        }

        public async Task<Sprint> CreateSprintAsync(CreateSprintDto dto)
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/sprints", content);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Sprint>(body);
        }

        public async Task<Sprint> UpdateSprintAsync(int sprintId, SprintUpdateDto dto)
        {
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(dto, settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"{_baseUrl}/sprints/{sprintId}", content);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Sprint>(body);
        }

        public async Task ActivateSprintAsync(int sprintId)
        {
            var response = await _httpClient.PutAsync($"{_baseUrl}/sprints/{sprintId}/activate", null);
            await HandleResponseAsync(response);
        }

        public async Task CloseSprintAsync(int sprintId, string action)
        {
            var payload = new { action };
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/sprints/{sprintId}/close", content);
            await HandleResponseAsync(response);
        }

        public async Task DeleteSprintAsync(int sprintId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/sprints/{sprintId}");
            await HandleResponseAsync(response);
        }
        #endregion

        #region Проекты
        // ==================== Проекты ====================
        public async Task<List<Project>> GetProjectsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/projects");
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ProjectListResponse>(body);
            return result.Items;
        }

        public async Task<Project> CreateProjectAsync(CreateProjectDto dto)
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/projects", content);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Project>(body);
        }

        public async Task<Project> UpdateProjectAsync(int projectId, ProjectUpdateDto dto)
        {
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(dto, settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"{_baseUrl}/projects/{projectId}", content);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Project>(body);
        }

        public async Task<Project> CompleteProjectAsync(int projectId)
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/projects/{projectId}/complete", null);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Project>(body);
        }

        public async Task DeleteProjectAsync(int projectId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/projects/{projectId}");
            await HandleResponseAsync(response);
        }

        public async Task<ProjectProgressReport> GetProjectProgressReportAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/reports/project-progress");
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ProjectProgressReport>(body);
        }

        public async Task<ProjectSummaryReport> GetProjectSummaryReportAsync(int projectId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/reports/project-summary?project_id={projectId}");
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ProjectSummaryReport>(body);
        }
        #endregion

        #region Команды
        // ==================== Команды ====================

        public async Task<List<Team>> GetTeamsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/teams");
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            var wrapper = JsonConvert.DeserializeObject<dynamic>(body);
            return JsonConvert.DeserializeObject<List<Team>>(wrapper.items.ToString());
        }

        public async Task<Team> CreateTeamAsync(CreateTeamDto dto)
        {
            var payload = new
            {
                team_name = dto.TeamName,
                team_lead_id = dto.TeamLeadId
            };
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/teams", content);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Team>(body);
        }

        public async Task<Team> UpdateTeamAsync(int teamId, TeamUpdateDto dto)
        {
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(dto, settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"{_baseUrl}/teams/{teamId}", content);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Team>(body);
        }

        public async Task DeleteTeamAsync(int teamId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/teams/{teamId}");
            await HandleResponseAsync(response);
        }
        #endregion

        #region Пользователи
        // ==================== Пользователи ====================
        public async Task<List<UserAccount>> GetUsersAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/users");
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<UserAccountListResponse>(body);
            return result.Items;
        }

        public async Task<UserAccount> CreateUserAsync(CreateUserDto dto)
        {
            var payload = new
            {
                login = dto.Login,
                password = dto.Password,
                role = dto.Role,
                employee_id = dto.EmployeeId
            };
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/users", content);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UserAccount>(body);
        }

        public async Task<UserAccount> UpdateUserAsync(int userId, UserUpdateDto dto)
        {
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(dto, settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"{_baseUrl}/users/{userId}", content);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UserAccount>(body);
        }

        public async Task DeactivateUserAsync(int userId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/users/{userId}");
            await HandleResponseAsync(response);
        }
        #endregion

        #region Нагрузка
        // ==================== Нагрузка ====================
        public async Task<LoadReport> GetLoadReportAsync(int? employeeId = null, int? teamId = null)
        {
            var queryParams = new List<string>();
            if (employeeId.HasValue) queryParams.Add($"employee_id={employeeId}");
            if (teamId.HasValue) queryParams.Add($"team_id={teamId}");

            string url = $"{_baseUrl}/reports/load";
            if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<LoadReport>(body);
        }

        public async Task<OverdueReport> GetOverdueReportAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/reports/overdue");
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OverdueReport>(body);
        }

        public async Task<BurndownReport> GetBurndownReportAsync(int? sprintId = null)
        {
            string url = $"{_baseUrl}/reports/burndown";
            if (sprintId.HasValue) url += $"?sprint_id={sprintId.Value}";
            var response = await _httpClient.GetAsync(url);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<BurndownReport>(body);
        }

        public async Task<AccuracyReport> GetAccuracyReportAsync(int sprintId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/reports/accuracy?sprint_id={sprintId}");
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AccuracyReport>(body);
        }
        #endregion

        #region Заметки
        // ==================== Заметки ====================
        public async Task<List<TaskNote>> GetTaskNotesAsync(int taskId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/tasks/{taskId}/notes");
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TaskNoteListResponse>(body);
            return result.Items;
        }

        public async Task<TaskNote> AddTaskNoteAsync(int taskId, string content)
        {
            var payload = new { content };
            var json = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/tasks/{taskId}/notes", httpContent);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TaskNote>(body);
        }

        public async Task DeleteTaskNoteAsync(int noteId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/notes/{noteId}");
            await HandleResponseAsync(response);
        }
        #endregion

        #region Диограммы
        // ==================== Диограммы ====================
        public async Task<StatusDistributionResponse> GetProjectStatusDistributionAsync(int projectId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/reports/project-status-distribution?project_id={projectId}");
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<StatusDistributionResponse>(body);
        }

        public async Task<TopEmployeesResponse> GetTopEmployeesAsync(int limit = 5, int? teamId = null)
        {
            string url = $"{_baseUrl}/reports/top-employees?limit={limit}";
            if (teamId.HasValue)
                url += $"&team_id={teamId.Value}";

            var response = await _httpClient.GetAsync(url);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TopEmployeesResponse>(body);
        }

        public async Task<StatusHistoryResponse> GetStatusHistoryAsync(int sprintId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/reports/status-history?sprint_id={sprintId}");
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<StatusHistoryResponse>(body);
        }

        public async Task<OverdueByAssigneeReport> GetOverdueByAssigneeAsync(int? teamId = null)
        {
            string url = $"{_baseUrl}/reports/overdue-by-assignee";
            if (teamId.HasValue) url += $"?team_id={teamId.Value}";

            var response = await _httpClient.GetAsync(url);
            await HandleResponseAsync(response);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OverdueByAssigneeReport>(body);
        }
        #endregion
    }
}
