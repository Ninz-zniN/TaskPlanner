using System;
using System.Collections.Generic;
using System.Text;
using TaskPlannerClient.Models;

namespace TaskPlannerClient.Service
{
    public class DataCache
    {
        private static List<ReferenceItem> _taskTypes;
        private static List<ReferenceItem> _taskStatuses;
        private static List<PriorityItem> _priorities;

        // Кэшируемые "тяжёлые" списки
        private static List<Employee> _employees;
        private static List<Project> _projects;
        private static List<Sprint> _sprints;
        private static List<Team> _teams;

        // Флаги загрузки
        private static bool _referencesLoaded = false;
        private static bool _employeesLoaded = false;
        private static bool _projectsLoaded = false;
        private static bool _sprintsLoaded = false;
        private static bool _teamsLoaded = false;

        // ==================== Справочники (загружаются один раз) ====================
        public static async Task LoadReferencesAsync()
        {
            if (_referencesLoaded) return;
            var api = UserSession.Instance.Api;
            _taskTypes = await api.GetTaskTypesAsync();
            _taskStatuses = await api.GetTaskStatusesAsync();
            _priorities = await api.GetPrioritiesAsync();
            _referencesLoaded = true;
        }

        public static List<ReferenceItem> TaskTypes => _taskTypes;
        public static List<ReferenceItem> TaskStatuses => _taskStatuses;
        public static List<PriorityItem> Priorities => _priorities;

        // ==================== Сотрудники ====================
        public static async Task<List<Employee>> GetEmployeesAsync(bool forceReload = false)
        {
            if (!_employeesLoaded || forceReload)
            {
                _employees = await UserSession.Instance.Api.GetEmployeesAsync();
                _employeesLoaded = true;
            }
            return _employees;
        }

        public static void InvalidateEmployees() => _employeesLoaded = false;

        // ==================== Проекты ====================
        public static async Task<List<Project>> GetProjectsAsync(bool forceReload = false)
        {
            if (!_projectsLoaded || forceReload)
            {
                _projects = await UserSession.Instance.Api.GetProjectsAsync();
                _projectsLoaded = true;
            }
            return _projects;
        }

        public static void InvalidateProjects() => _projectsLoaded = false;

        // ==================== Спринты ====================
        public static async Task<List<Sprint>> GetSprintsAsync(bool forceReload = false)
        {
            if (!_sprintsLoaded || forceReload)
            {
                _sprints = await UserSession.Instance.Api.GetSprintsAsync();
                _sprintsLoaded = true;
            }
            return _sprints;
        }

        public static void InvalidateSprints() => _sprintsLoaded = false;

        // ==================== Команды ====================
        public static async Task<List<Team>> GetTeamsAsync(bool forceReload = false)
        {
            if (!_teamsLoaded || forceReload)
            {
                _teams = await UserSession.Instance.Api.GetTeamsAsync();
                _teamsLoaded = true;
            }
            return _teams;
        }

        public static void InvalidateTeams() => _teamsLoaded = false;
    }
}
