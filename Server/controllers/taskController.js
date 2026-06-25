const pool = require('../config/db');

const taskController = {
    // GET /api/tasks
    getAll: async (req, res) => {
        try {
            const {
                assignee_id,
                team_id,
                project_id,
                sprint_id,
                status_id,
                previous_assignee_id,
                include_unassigned
            } = req.query;

            // Начинаем строить SQL
            let query = `
                SELECT
                    t.id_task,
                    t.title,
                    t.description,
                    t.estimate_hours,
                    t.actual_hours,
                    t.deadline,
                    t.created_at,
                    t.updated_at,
                    -- Исполнитель
                    CASE WHEN a.id_employee IS NOT NULL THEN
                        json_build_object('id', a.id_employee, 'last_name', a.last_name, 'first_name', a.first_name)
                    ELSE NULL END AS assignee,
                    -- Статус
                    json_build_object('id', ts.id_task_status, 'name', ts.status_name) AS status,
                    -- Приоритет
                    json_build_object('id', p.id_priority, 'name', p.priority_name, 'weight', p.priority_weight) AS priority,
                    -- Тип задачи
                    json_build_object('id', tt.id_task_type, 'name', tt.type_name) AS task_type,
                    -- Проект
                    json_build_object('id', pr.id_project, 'name', pr.project_name) AS project,
                    -- Спринт
                    CASE WHEN s.id_sprint IS NOT NULL THEN
                        json_build_object('id', s.id_sprint, 'name', s.sprint_name)
                    ELSE NULL END AS sprint
                FROM Task t
                LEFT JOIN Employee a ON t.id_assignee = a.id_employee
                LEFT JOIN Task_Status ts ON t.id_task_status = ts.id_task_status
                LEFT JOIN Priority p ON t.id_priority = p.id_priority
                LEFT JOIN Task_Type tt ON t.id_task_type = tt.id_task_type
                LEFT JOIN Project pr ON t.id_project = pr.id_project
                LEFT JOIN Sprint s ON t.id_sprint = s.id_sprint
                WHERE 1=1
            `;
            const params = [];

            // Фильтры
            if (assignee_id) {
                query += ` AND t.id_assignee = $${params.length + 1}`;
                params.push(assignee_id);
            }

            if (team_id) {
                // Задачи сотрудников из указанной команды ИЛИ неназначенные задачи проектов этой команды
                query += ` AND (
                    a.id_team = $${params.length + 1} 
                    OR (t.id_assignee IS NULL AND t.id_project IN (
                        SELECT id_project FROM Project WHERE id_team = $${params.length + 1}
                    ))
                )`;
                params.push(team_id);
            }

            if (project_id) {
                query += ` AND t.id_project = $${params.length + 1}`;
                params.push(project_id);
            }

            if (sprint_id) {
                query += ` AND t.id_sprint = $${params.length + 1}`;
                params.push(sprint_id);
            }

            if (status_id) {
                query += ` AND t.id_task_status = $${params.length + 1}`;
                params.push(status_id);
            }

            // Фильтр "включать неназначенные" (по умолчанию true)
            if (include_unassigned === 'false') {
                query += ` AND t.id_assignee IS NOT NULL`;
            }

            // Фильтр по предыдущему исполнителю (из истории)
            if (previous_assignee_id) {
                query += ` AND EXISTS (
                    SELECT 1 FROM Task_History th
                    WHERE th.id_task = t.id_task
                      AND th.assignee_id = $${params.length + 1}
                      AND t.id_task_status IN (3,4)  -- На ревью или Тестирование
                )`;
                params.push(previous_assignee_id);
            }

            query += ' ORDER BY t.updated_at DESC, t.created_at DESC';

            const result = await pool.query(query, params);

            
            const items = result.rows.map(row => ({
                id_task: row.id_task,
                title: row.title,
                description: row.description,
                estimate_hours: parseFloat(row.estimate_hours),
                actual_hours: row.actual_hours ? parseFloat(row.actual_hours) : null,
                deadline: row.deadline,
                created_at: row.created_at,
                updated_at: row.updated_at,
                assignee: row.assignee,          
                status: row.status,              
                priority: row.priority,          
                task_type: row.task_type,        
                project: row.project,            
                sprint: row.sprint               
            }));

            res.json({ items, total: items.length });
        } catch (error) {
            console.error('Error fetching tasks:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    create: async (req, res) => {
        try {
            const { role } = req.user;
            
            // Только менеджер и админ могут создавать задачи
            if (role !== 'admin' && role !== 'project_manager') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }

            const {
                title,
                description,
                id_task_type,
                id_priority,
                estimate_hours,
                deadline,
                id_assignee,
                id_project,
                id_sprint
            } = req.body;
            console.log(req.body);
            console.log(id_task_type);
            // Валидация обязательных полей
            if (!title || !estimate_hours) {
                return res.status(400).json({ error: { code: 400, message: 'Название и оценка обязательны' } });
            }
            if (id_assignee) {
                const load = await getEmployeeLoad(id_assignee);
                if (load.percent >= 150) {
                    return res.status(409).json({
                        error: {
                            code: 409,
                            message: 'Невозможно назначить задачу: нагрузка сотрудника уже превышает 150%'
                        }
                    });
                }
            }

            const result = await pool.query(
                `INSERT INTO Task (title, description, id_task_type, id_task_status, id_priority,
                    estimate_hours, deadline, id_assignee, id_project, id_sprint, created_by)
                VALUES ($1, $2, $3, 1, $4, $5, $6, $7, $8, $9, $10)
                RETURNING id_task`,
                [title, description || null, id_task_type || null, id_priority || null,
                estimate_hours, deadline || null, id_assignee || null,
                id_project || null, id_sprint || null, req.user.id_user]
            );

            const newTaskId = result.rows[0].id_task;

            // Возвращаем созданную задачу в полном формате (как в GET)
            const taskQuery = `
                SELECT
                    t.id_task,
                    t.title,
                    t.description,
                    t.estimate_hours,
                    t.actual_hours,
                    t.deadline,
                    t.created_at,
                    t.updated_at,
                    CASE WHEN a.id_employee IS NOT NULL THEN
                        json_build_object('id', a.id_employee, 'last_name', a.last_name)
                    ELSE NULL END AS assignee,
                    json_build_object('id', ts.id_task_status, 'name', ts.status_name) AS status,
                    json_build_object('id', p.id_priority, 'name', p.priority_name, 'weight', p.priority_weight) AS priority,
                    json_build_object('id', tt.id_task_type, 'name', tt.type_name) AS task_type,
                    json_build_object('id', pr.id_project, 'name', pr.project_name) AS project,
                    CASE WHEN s.id_sprint IS NOT NULL THEN
                        json_build_object('id', s.id_sprint, 'name', s.sprint_name)
                    ELSE NULL END AS sprint
                FROM Task t
                LEFT JOIN Employee a ON t.id_assignee = a.id_employee
                LEFT JOIN Task_Status ts ON t.id_task_status = ts.id_task_status
                LEFT JOIN Priority p ON t.id_priority = p.id_priority
                LEFT JOIN Task_Type tt ON t.id_task_type = tt.id_task_type
                LEFT JOIN Project pr ON t.id_project = pr.id_project
                LEFT JOIN Sprint s ON t.id_sprint = s.id_sprint
                WHERE t.id_task = $1
            `;
            const taskResult = await pool.query(taskQuery, [newTaskId]);
            await pool.query(
                `INSERT INTO Task_History (id_task, old_status, new_status, changed_by, assignee_id, changed_at, actual_hours)
                VALUES ($1, NULL, 1, $2, $3, NOW(), 0)`,
                [newTaskId, req.user.id_user, id_assignee || null]
            );
            const row = taskResult.rows[0];

            const task = {
                id_task: row.id_task,
                title: row.title,
                description: row.description,
                estimate_hours: parseFloat(row.estimate_hours),
                actual_hours: row.actual_hours ? parseFloat(row.actual_hours) : null,
                deadline: row.deadline,
                created_at: row.created_at,
                updated_at: row.updated_at,
                assignee: row.assignee,
                status: row.status,
                priority: row.priority,
                task_type: row.task_type,
                project: row.project,
                sprint: row.sprint
            };
            res.status(201).json(task);
        } catch (error) {
            console.error('Error creating task:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },
    // PATCH /api/tasks/:id
    update: async (req, res) => {
        try {
            const { id_user, role } = req.user;
            const taskId = req.params.id;
            let fields = req.body;   

            // 1. Проверяем существование задачи
            const taskCheck = await pool.query('SELECT id_assignee FROM Task WHERE id_task = $1', [taskId]);
            if (taskCheck.rows.length === 0) {
                return res.status(404).json({ error: { code: 404, message: 'Задача не найдена' } });
            }
            const currentAssigneeId = taskCheck.rows[0].id_assignee;

            // 2. Разрешаем только допустимые поля в зависимости от роли
            if (role === 'worker') {
                const myEmployeeId = await getEmployeeIdByUserId(id_user);
                if (currentAssigneeId !== myEmployeeId) {
                    return res.status(403).json({ error: { code: 403, message: 'Вы можете редактировать только свои задачи' } });
                }
                // Оставляем только поле description
                const allowed = {};
                if (fields.description !== undefined) allowed.description = fields.description;
                if (Object.keys(allowed).length === 0) {
                    return res.status(400).json({ error: { code: 400, message: 'Нет допустимых полей для изменения' } });
                }
                fields = allowed;   // теперь переменная let, ошибки не будет
            } else if (role !== 'admin' && role !== 'project_manager') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }

            // 3. Строим динамический UPDATE
            const setClauses = [];
            const params = [taskId];
            const allowedFields = [
                'title', 'description', 'id_task_type', 'id_priority',
                'estimate_hours', 'deadline', 'id_assignee', 'id_sprint'
            ];
            for (const field of allowedFields) {
                if (fields[field] !== undefined) {
                    setClauses.push(`${field} = $${params.length + 1}`);
                    params.push(fields[field]);
                }

                if (field === 'id_assignee') {
                    const load = await getEmployeeLoad(fields[field]);
                    if (load.percent >= 150) {
                        return res.status(409).json({
                            error: {
                                code: 409,
                                message: 'Невозможно назначить задачу: нагрузка сотрудника уже превышает 150%'
                            }
                        });
                    }
                }
            }
            if (setClauses.length === 0) {
                return res.status(400).json({ error: { code: 400, message: 'Нет полей для обновления' } });
            }
            setClauses.push(`updated_at = NOW()`);

            const query = `UPDATE Task SET ${setClauses.join(', ')} WHERE id_task = $1 RETURNING *`;
            await pool.query(query, params);

            // 4. Возвращаем обновлённую задачу
            const updatedTask = await getTaskById(taskId);
            res.json(updatedTask);
        } catch (error) {
            console.error('Error updating task:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // PATCH /api/tasks/:id/status
    updateStatus: async (req, res) => {
        try {
            const { id_user, role } = req.user;
            const taskId = req.params.id;
            const { id_task_status, changed_at, actual_hours } = req.body;

            if (!id_task_status) {
                return res.status(400).json({ error: { code: 400, message: 'Не указан новый статус' } });
            }

            // Проверяем существование задачи и текущего исполнителя
            const taskCheck = await pool.query(
                'SELECT id_task_status, id_assignee FROM Task WHERE id_task = $1', [taskId]
            );
            if (taskCheck.rows.length === 0) {
                return res.status(404).json({ error: { code: 404, message: 'Задача не найдена' } });
            }
            const oldStatusId = taskCheck.rows[0].id_task_status;
            const assigneeId = taskCheck.rows[0].id_assignee;

            // Worker может менять статус только своих задач
            if (role === 'worker') {
                const myEmployeeId = await getEmployeeIdByUserId(id_user);
                if (assigneeId !== myEmployeeId) {
                    return res.status(403).json({ error: { code: 403, message: 'Вы можете менять статус только своих задач' } });
                }
                // Если задача без исполнителя и Worker пытается её взять
                if (!assigneeId) {
                    const load = await getEmployeeLoad(myEmployeeId);
                    if (load.percent >= 150) {
                        return res.status(409).json({
                            error: {
                                code: 409,
                                message: 'Вы не можете взять задачу: ваша нагрузка уже превышает 150%'
                            }
                        });
                    }
                }
            } else if (role !== 'admin' && role !== 'project_manager') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            
            // Обновляем статус задачи
            await pool.query(
                'UPDATE Task SET id_task_status = $1, updated_at = NOW() WHERE id_task = $2',
                [id_task_status, taskId]
            );

            // Ручное указание даты и часов разрешено только администратору
            if (role === 'admin' && changed_at !== undefined && actual_hours !== undefined) {
                await pool.query(
                    `INSERT INTO Task_History (id_task, old_status, new_status, changed_by, assignee_id, changed_at, actual_hours)
                    VALUES ($1, $2, $3, $4, $5, $6, $7)`,
                    [taskId, oldStatusId, id_task_status, id_user, assigneeId, changed_at, actual_hours || 0]
                );
            }else {
                // Вставляем запись в историю (пока без actual_hours)
                const historyInsert = await pool.query(
                    `INSERT INTO Task_History (id_task, old_status, new_status, changed_by, assignee_id)
                    VALUES ($1, $2, $3, $4, $5) RETURNING id_log`,
                    [taskId, oldStatusId, id_task_status, id_user, assigneeId]
                );
                const historyId = historyInsert.rows[0].id_log;

                // Рассчитываем фактическое время для предыдущего этапа
                if (assigneeId && [3, 4, 5, 6].includes(id_task_status)) {
                    const prevHistory = await pool.query(
                        `SELECT changed_at FROM Task_History 
                        WHERE id_task = $1 AND assignee_id = $2 AND id_log != $3
                        ORDER BY changed_at DESC LIMIT 1`,
                        [taskId, assigneeId, historyId]
                    );

                    if (prevHistory.rows.length > 0) {
                        const prevTime = new Date(prevHistory.rows[0].changed_at);
                        const now = new Date();
                        const hoursPerDay = await getEmployeeHoursPerDay(assigneeId);
                        const hoursSpent = calculateWorkHours(prevTime, now, hoursPerDay);

                        await pool.query(
                            'UPDATE Task_History SET actual_hours = $1 WHERE id_log = $2',
                            [hoursSpent, historyId]
                        );
                    }
                }
            }

            // Возвращаем обновлённую задачу
            const updatedTask = await getTaskById(taskId);
            res.json(updatedTask);
        } catch (error) {
            console.error('Error updating task status:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    }
    
};

// Получить id_employee по id_user
async function getEmployeeIdByUserId(userId) {
    const res = await pool.query('SELECT id_employee FROM Users WHERE id_user = $1', [userId]);
    return res.rows[0]?.id_employee || null;
}

// Получить полную задачу по id (тот же запрос, что в GET /api/tasks/:id)
async function getTaskById(taskId) {
    const query = `
        SELECT
            t.id_task, t.title, t.description, t.estimate_hours, t.actual_hours, t.deadline,
            t.created_at, t.updated_at,
            CASE WHEN a.id_employee IS NOT NULL THEN
                json_build_object('id', a.id_employee, 'last_name', a.last_name)
            ELSE NULL END AS assignee,
            json_build_object('id', ts.id_task_status, 'name', ts.status_name) AS status,
            json_build_object('id', p.id_priority, 'name', p.priority_name, 'weight', p.priority_weight) AS priority,
            json_build_object('id', tt.id_task_type, 'name', tt.type_name) AS task_type,
            json_build_object('id', pr.id_project, 'name', pr.project_name) AS project,
            CASE WHEN s.id_sprint IS NOT NULL THEN
                json_build_object('id', s.id_sprint, 'name', s.sprint_name)
            ELSE NULL END AS sprint
        FROM Task t
        LEFT JOIN Employee a ON t.id_assignee = a.id_employee
        LEFT JOIN Task_Status ts ON t.id_task_status = ts.id_task_status
        LEFT JOIN Priority p ON t.id_priority = p.id_priority
        LEFT JOIN Task_Type tt ON t.id_task_type = tt.id_task_type
        LEFT JOIN Project pr ON t.id_project = pr.id_project
        LEFT JOIN Sprint s ON t.id_sprint = s.id_sprint
        WHERE t.id_task = $1
    `;
    const result = await pool.query(query, [taskId]);
    const row = result.rows[0];
    return {
        id_task: row.id_task,
        title: row.title,
        description: row.description,
        estimate_hours: parseFloat(row.estimate_hours),
        actual_hours: row.actual_hours ? parseFloat(row.actual_hours) : null,
        deadline: row.deadline,
        created_at: row.created_at,
        updated_at: row.updated_at,
        assignee: row.assignee,
        status: row.status,
        priority: row.priority,
        task_type: row.task_type,
        project: row.project,
        sprint: row.sprint
    };
}

// Получить количество рабочих часов в день для сотрудника
async function getEmployeeHoursPerDay(employeeId) {
    const res = await pool.query('SELECT hours_per_day FROM Employee WHERE id_employee = $1', [employeeId]);
    return res.rows[0] ? parseFloat(res.rows[0].hours_per_day) : 8; // по умолчанию 8
}

// Рассчитать рабочее время между двумя датами с учётом графика
function calculateWorkHours(start, end, hoursPerDay) {
    if (!start || !end || end <= start) return 0;
    const workStartHour = 9;
    const workEndHour = workStartHour + hoursPerDay;
    let totalHours = 0;
    let current = new Date(start);

    while (current < end) {
        const dayOfWeek = current.getDay();
        if (dayOfWeek !== 0 && dayOfWeek !== 6) { // будние дни
            let dayStart = new Date(current);
            dayStart.setHours(workStartHour, 0, 0, 0);
            let dayEnd = new Date(current);
            dayEnd.setHours(workEndHour, 0, 0, 0);

            const effectiveStart = current > dayStart ? current : dayStart;
            const effectiveEnd = end < dayEnd ? end : dayEnd;

            if (effectiveEnd > effectiveStart) {
                totalHours += (effectiveEnd - effectiveStart) / 3600000;
            }
        }
        // следующий день
        current.setDate(current.getDate() + 1);
        current.setHours(0, 0, 0, 0);
    }
    return Math.round(totalHours * 10) / 10;
}

// Рассчитать текущую загрузку сотрудника в активном спринте
async function getEmployeeLoad(employeeId) {
    // Находим активный спринт
    const sprintResult = await pool.query(
        'SELECT id_sprint, work_days FROM Sprint WHERE is_active = TRUE LIMIT 1'
    );
    if (sprintResult.rows.length === 0) {
        return { currentLoad: 0, capacity: 0, percent: 0 };
    }
    const sprint = sprintResult.rows[0];

    // Получаем часы сотрудника в день
    const empResult = await pool.query(
        'SELECT hours_per_day FROM Employee WHERE id_employee = $1',
        [employeeId]
    );
    if (empResult.rows.length === 0) {
        return { currentLoad: 0, capacity: 0, percent: 0 };
    }
    const hoursPerDay = parseFloat(empResult.rows[0].hours_per_day);
    const capacity = hoursPerDay * sprint.work_days;

    // Текущая нагрузка: сумма оценок задач сотрудника в активном спринте (исключая готовые и отменённые)
    const loadResult = await pool.query(
        `SELECT COALESCE(SUM(estimate_hours), 0) AS current_load
         FROM Task
         WHERE id_assignee = $1 AND id_sprint = $2 AND id_task_status NOT IN (5,6)`,
        [employeeId, sprint.id_sprint]
    );
    const currentLoad = parseFloat(loadResult.rows[0].current_load);
    const percent = capacity > 0 ? (currentLoad / capacity) * 100 : 0;

    return { currentLoad, capacity, percent };
}

module.exports = taskController;