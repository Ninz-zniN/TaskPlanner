const pool = require('../config/db');

// Кэш статусов задач (загружается один раз)
let taskStatuses = [];

const loadStatuses = async () => {
    if (taskStatuses.length === 0) {
        const res = await pool.query('SELECT id_task_status AS id, status_name AS name FROM Task_Status');
        taskStatuses = res.rows;
    }
};

const reportController = {
    // GET /api/reports/load?team_id=&employee_id=
    getLoad: async (req, res) => {
        try {
            const { team_id, employee_id } = req.query;

            const activeSprint = await pool.query(
                'SELECT id_sprint, sprint_name, work_days FROM Sprint WHERE is_active = TRUE LIMIT 1'
            );
            const sprint = activeSprint.rows[0] || null;

            if (!sprint) {
                return res.json({ sprint: null, items: [] });
            }

            let empQuery = `
                SELECT e.id_employee, e.last_name, e.first_name, e.grade, e.hours_per_day
                FROM Employee e
                WHERE e.is_dismissed = FALSE
            `;
            const empParams = [];

            if (employee_id) {
                empQuery += ` AND e.id_employee = $${empParams.length + 1}`;
                empParams.push(employee_id);
            } else if (team_id) {
                empQuery += ` AND e.id_team = $${empParams.length + 1}`;
                empParams.push(team_id);
            }

            empQuery += ' ORDER BY e.last_name, e.first_name';
            const empResult = await pool.query(empQuery, empParams);
            const employees = empResult.rows;

            const items = [];
            for (const emp of employees) {
                const loadResult = await pool.query(
                    `SELECT COALESCE(SUM(estimate_hours), 0) AS current_load_hours
                     FROM Task
                     WHERE id_assignee = $1 AND id_sprint = $2 AND id_task_status NOT IN (5,6)`,
                    [emp.id_employee, sprint.id_sprint]
                );
                const currentLoadHours = parseFloat(loadResult.rows[0].current_load_hours);
                const capacityHours = parseFloat(emp.hours_per_day) * sprint.work_days;
                const loadPercent = capacityHours > 0 ? (currentLoadHours / capacityHours) * 100 : 0;

                items.push({
                    employee: {
                        id: emp.id_employee,
                        last_name: emp.last_name,
                        first_name: emp.first_name,
                        grade: emp.grade
                    },
                    current_load_hours: currentLoadHours,
                    capacity_hours: capacityHours,
                    load_percent: Math.round(loadPercent * 10) / 10
                });
            }

            res.json({
                sprint: { id: sprint.id_sprint, name: sprint.sprint_name, work_days: sprint.work_days },
                items
            });
        } catch (error) {
            console.error('Error in load report:', error);
            res.status(500).json({ error: { code: 500, message: 'Ошибка при формировании отчёта нагрузки' } });
        }
    },

    // GET /api/reports/overdue?project_id=
    getOverdue: async (req, res) => {
        try {
            const { project_id } = req.query;
            let query = `
                SELECT t.id_task, t.title, t.deadline,
                       CASE WHEN a.id_employee IS NOT NULL THEN
                           json_build_object('id', a.id_employee, 'last_name', a.last_name)
                       ELSE NULL END AS assignee,
                       json_build_object('id', p.id_project, 'name', p.project_name) AS project
                FROM Task t
                LEFT JOIN Employee a ON t.id_assignee = a.id_employee
                LEFT JOIN Project p ON t.id_project = p.id_project
                WHERE t.deadline < CURRENT_DATE
                  AND t.id_task_status NOT IN (5,6)
            `;
            const params = [];
            if (project_id) {
                query += ` AND t.id_project = $${params.length + 1}`;
                params.push(project_id);
            }
            query += ' ORDER BY t.deadline';

            const result = await pool.query(query, params);
            const items = result.rows.map(row => ({
                id_task: row.id_task,
                title: row.title,
                deadline: row.deadline,
                assignee: row.assignee,
                project: row.project
            }));

            res.json({ items, total: items.length });
        } catch (error) {
            console.error('Error in overdue report:', error);
            res.status(500).json({ error: { code: 500, message: 'Ошибка при формировании отчёта просроченных задач' } });
        }
    },

    // GET /api/reports/burndown
    getBurndown: async (req, res) => {
        try {
            const sprintId = req.query.sprint_id;
            let sprint;

            if (sprintId) {
                const result = await pool.query(
                    `SELECT id_sprint, sprint_name,
                            TO_CHAR(start_date, 'YYYY-MM-DD') AS start_date,
                            TO_CHAR(end_date, 'YYYY-MM-DD') AS end_date,
                            work_days
                    FROM Sprint WHERE id_sprint = $1`, [sprintId]);
                sprint = result.rows[0] || null;
            } else {
                const result = await pool.query(
                    `SELECT id_sprint, sprint_name,
                            TO_CHAR(start_date, 'YYYY-MM-DD') AS start_date,
                            TO_CHAR(end_date, 'YYYY-MM-DD') AS end_date,
                            work_days
                    FROM Sprint WHERE is_active = TRUE LIMIT 1`);
                sprint = result.rows[0] || null;
            }

            if (!sprint) return res.status(404).json({ error: { code: 404, message: 'Спринт не найден' } });

            const totalResult = await pool.query(
                'SELECT COALESCE(SUM(estimate_hours), 0) AS initial_hours FROM Task WHERE id_sprint = $1',
                [sprint.id_sprint]
            );
            const initialHours = parseFloat(totalResult.rows[0].initial_hours);

            const remainingResult = await pool.query(
                'SELECT COALESCE(SUM(estimate_hours), 0) AS remaining_hours FROM Task WHERE id_sprint = $1 AND id_task_status NOT IN (5,6)',
                [sprint.id_sprint]
            );
            const remainingHours = parseFloat(remainingResult.rows[0].remaining_hours);

            const idealBurn = [];
            for (let day = 1; day <= sprint.work_days; day++) {
                const idealRemaining = Math.max(0, initialHours - (initialHours / sprint.work_days) * day);
                idealBurn.push({ day, remaining: Math.round(idealRemaining * 10) / 10 });
            }

            const actualBurn = [];
            const today = new Date();
            //const sprintStart = new Date(sprint.start_date);
            //const sprintEnd = new Date(sprint.end_date);
            const [sYear, sMonth, sDay] = sprint.start_date.split('-').map(Number);
            const [eYear, eMonth, eDay] = sprint.end_date.split('-').map(Number);
            const sprintStart = new Date(sYear, sMonth - 1, sDay, 0, 0, 0);
            const sprintEnd = new Date(eYear, eMonth - 1, eDay, 0, 0, 0);

            const historyQuery = await pool.query(
                `SELECT th.id_task, th.new_status, th.changed_at
                 FROM Task_History th
                 JOIN Task t ON th.id_task = t.id_task
                 WHERE t.id_sprint = $1
                 ORDER BY th.changed_at`,
                [sprint.id_sprint]
            );
            const history = historyQuery.rows;

            // Вспомогательная проверка дня недели (0 – воскресенье, 6 – суббота)
            const isWeekend = (date) => date.getDay() === 0 || date.getDay() === 6;

            const tasksQuery = await pool.query(
                `SELECT id_task, estimate_hours, id_task_status, created_at FROM Task WHERE id_sprint = $1`,
                [sprint.id_sprint]
            );
            const tasks = tasksQuery.rows;
            const currentDate = new Date(sprintStart);
            while (currentDate <= today && currentDate <= sprintEnd) {
                const dayNumber = Math.floor((currentDate - sprintStart) / (1000 * 60 * 60 * 24)) + 1;
                const endOfDay = new Date(currentDate);
                endOfDay.setHours(23, 59, 59, 999);
                let remainingHoursForDay = 0;
                for (const task of tasks) {
                    if (new Date(task.created_at) > endOfDay) continue;
                    let lastChange;

                    // Проверяем, не последний ли это день спринта
                    if (currentDate.toDateString() === sprintEnd.toDateString()) {
                        // Для последнего дня — последнее изменение за всю историю
                        lastChange = history
                            .filter(h => h.id_task === task.id_task)
                            .pop();
                    } else {
                        // Для остальных дней — изменения до конца дня
                        lastChange = history
                            .filter(h => h.id_task === task.id_task && new Date(h.changed_at) <= endOfDay)
                            .pop();
                    }

                    const statusId = lastChange ? lastChange.new_status : task.id_task_status;
                    if (statusId !== 5 && statusId !== 6) {
                        remainingHoursForDay += parseFloat(task.estimate_hours);
                    }
                }

                actualBurn.push({
                    day: dayNumber,
                    remaining: Math.round(remainingHoursForDay * 10) / 10
                });

                currentDate.setDate(currentDate.getDate() + 1);
            }

            res.json({
                sprint: {
                    id: sprint.id_sprint,
                    name: sprint.sprint_name,
                    start_date: sprint.start_date,
                    end_date: sprint.end_date,
                    work_days: sprint.work_days,
                    initial_hours: initialHours,
                    remaining_hours: remainingHours
                },
                ideal_burn: idealBurn,
                actual_burn: actualBurn
            });
        } catch (error) {
            console.error('Error in burndown report:', error);
            res.status(500).json({ error: { code: 500, message: 'Ошибка при формировании burn‑down отчёта' } });
        }
    },

    // GET /api/reports/accuracy?sprint_id=
    getAccuracy: async (req, res) => {
        try {
            const { sprint_id } = req.query;
            if (!sprint_id) {
                return res.status(400).json({ error: { code: 400, message: 'Параметр sprint_id обязателен' } });
            }

            const tasksResult = await pool.query(
                `SELECT t.id_task, t.title, t.estimate_hours,
                        COALESCE(SUM(th.actual_hours), 0) AS total_actual_hours
                 FROM Task t
                 LEFT JOIN Task_History th ON t.id_task = th.id_task
                 WHERE t.id_sprint = $1 AND t.id_task_status = 5
                 GROUP BY t.id_task, t.title, t.estimate_hours
                 ORDER BY t.title`,
                [sprint_id]
            );

            const items = [];
            let totalEstimate = 0;
            let totalActual = 0;

            for (const task of tasksResult.rows) {
                const estimate = parseFloat(task.estimate_hours);
                const actual = parseFloat(task.total_actual_hours);
                const deviation = actual - estimate;
                const deviationPercent = estimate > 0 ? (deviation / estimate) * 100 : 0;

                items.push({
                    task: { id: task.id_task, title: task.title },
                    estimate_hours: estimate,
                    actual_hours: actual,
                    deviation: Math.round(deviation * 10) / 10,
                    deviation_percent: Math.round(deviationPercent * 10) / 10
                });

                totalEstimate += estimate;
                totalActual += actual;
            }

            const overallDeviationPercent = totalEstimate > 0
                ? ((totalActual - totalEstimate) / totalEstimate) * 100
                : 0;

            res.json({
                items,
                total_estimate: totalEstimate,
                total_actual: totalActual,
                overall_deviation_percent: Math.round(overallDeviationPercent * 10) / 10
            });
        } catch (error) {
            console.error('Error in accuracy report:', error);
            res.status(500).json({ error: { code: 500, message: 'Ошибка при формировании отчёта точности' } });
        }
    },

    // GET /api/reports/project-progress
    getProjectProgress: async (req, res) => {
        try {
            // Получаем список активных проектов
            const projectsResult = await pool.query(
                `SELECT id_project, project_name FROM Project WHERE status = 'active' ORDER BY project_name`
            );
            const projects = projectsResult.rows;

            const items = [];
            for (const proj of projects) {
                // Получаем все задачи проекта
                const tasksResult = await pool.query(
                    `SELECT id_task_status, estimate_hours, actual_hours FROM Task WHERE id_project = $1`,
                    [proj.id_project]
                );
                const tasks = tasksResult.rows;

                // Суммарный план (общий объём)
                const totalPlanned = tasks.reduce((sum, t) => sum + parseFloat(t.estimate_hours), 0);

                // Количество выполненных задач (статус = 5)
                const completedTasks = tasks.filter(t => t.id_task_status === 5).length;

                // Общий факт по всем задачам
                const totalActual = tasks.reduce((sum, t) => sum + (parseFloat(t.actual_hours) || 0), 0);
                
                // Суммарный план выполненных задач
                const completedPlannedHours = tasks
                    .filter(t => t.id_task_status === 5)
                    .reduce((sum, t) => sum + parseFloat(t.estimate_hours), 0);

                // Взвешенный прогресс
                let weightedProgress = 0;
                for (const task of tasks) {
                    const estimate = parseFloat(task.estimate_hours) || 0;
                    if (totalPlanned > 0 && estimate > 0) {
                        const weight = estimate / totalPlanned; // вес задачи в проекте
                        if (task.id_task_status === 5) {
                            // Завершённая задача = 100% прогресса
                            weightedProgress += weight;
                        } else {
                            // Незавершённая задача: прогресс = min(факт/план, 0.85)
                            const actual = parseFloat(task.actual_hours) || 0;
                            const progress = Math.min(actual / estimate, 0.85);
                            weightedProgress += weight * progress;
                        }
                    }
                }

                items.push({
                    id_project: proj.id_project,
                    project_name: proj.project_name,
                    status: 'active',
                    total_tasks: tasks.length,
                    completed_tasks: completedTasks,
                    planned_hours: Math.round(totalPlanned * 10) / 10,
                    actual_hours: Math.round(totalActual * 10) / 10,
                    completion_percent_tasks: tasks.length > 0
                        ? Math.round((completedTasks / tasks.length) * 1000) / 10
                        : 0,
                    completion_percent_hours: totalPlanned > 0
                        ? Math.round((completedPlannedHours / totalPlanned) * 1000) / 10
                        : 0,
                    completion_percent_weighted: Math.round(weightedProgress * 1000) / 10
                });
            }

            res.json({ items, total: items.length });
        } catch (error) {
            console.error('Error in project progress report:', error);
            res.status(500).json({ error: { code: 500, message: 'Ошибка при формировании отчёта о прогрессе проектов' } });
        }
    },

    // GET /api/reports/project-summary?project_id=...
    getProjectSummary: async (req, res) => {
        try {
            const { project_id } = req.query;
            if (!project_id) {
                return res.status(400).json({ error: { code: 400, message: 'Параметр project_id обязателен' } });
            }

            const projectQuery = await pool.query(
                `SELECT
                    p.id_project,
                    p.project_name,
                    p.description,
                    p.status,
                    p.completed_at,
                    COALESCE(SUM(t.estimate_hours), 0) AS total_planned,
                    COALESCE(SUM(t.actual_hours), 0) AS total_actual,
                    COUNT(t.id_task) AS task_count
                 FROM Project p
                 LEFT JOIN Task t ON p.id_project = t.id_project
                 WHERE p.id_project = $1
                 GROUP BY p.id_project`,
                [project_id]
            );

            if (projectQuery.rows.length === 0) {
                return res.status(404).json({ error: { code: 404, message: 'Проект не найден' } });
            }

            const project = projectQuery.rows[0];

            const tasksQuery = await pool.query(
                `SELECT t.id_task, t.title, t.estimate_hours, t.actual_hours, t.id_task_status,
                        COALESCE(SUM(th.actual_hours) FILTER (WHERE th.new_status = 3), 0) AS work_hours,
                        COALESCE(SUM(th.actual_hours) FILTER (WHERE th.new_status = 4), 0) AS review_hours,
                        COALESCE(SUM(th.actual_hours) FILTER (WHERE th.new_status = 5), 0) AS test_hours
                FROM Task t
                LEFT JOIN Task_History th ON t.id_task = th.id_task
                WHERE t.id_project = $1
                GROUP BY t.id_task, t.title, t.estimate_hours, t.actual_hours, t.id_task_status
                ORDER BY t.title`,
                [project_id]
            );

            const tasks = tasksQuery.rows.map(row => ({
                id_task: row.id_task,
                title: row.title,
                estimate_hours: parseFloat(row.estimate_hours),
                actual_hours: row.actual_hours ? parseFloat(row.actual_hours) : null,
                status_id: row.id_task_status,
                work_hours: parseFloat(row.work_hours),
                review_hours: parseFloat(row.review_hours),
                test_hours: parseFloat(row.test_hours)
            }));

            res.json({
                project: {
                    id: project.id_project,
                    name: project.project_name,
                    description: project.description,
                    status: project.status,
                    completed_at: project.completed_at
                },
                planned_hours: parseFloat(project.total_planned),
                actual_hours: parseFloat(project.total_actual),
                task_count: parseInt(project.task_count),
                tasks: tasks
            });
        } catch (error) {
            console.error('Error in project summary report:', error);
            res.status(500).json({ error: { code: 500, message: 'Ошибка при формировании сводки по проекту' } });
        }
    },

    // GET /api/reports/project-status-distribution?project_id=...
    getProjectStatusDistribution: async (req, res) => {
        try {
            const { project_id } = req.query;
            if (!project_id) {
                return res.status(400).json({ error: { code: 400, message: 'Параметр project_id обязателен' } });
            }

            const query = `
                SELECT ts.status_name, COUNT(t.id_task) AS count
                FROM Task t
                JOIN Task_Status ts ON t.id_task_status = ts.id_task_status
                WHERE t.id_project = $1
                GROUP BY ts.status_name
                ORDER BY count DESC
            `;
            const result = await pool.query(query, [project_id]);

            const items = result.rows.map(row => ({
                status: row.status_name,
                count: parseInt(row.count)
            }));

            res.json({ items, total: items.length });
        } catch (error) {
            console.error('Error in project status distribution:', error);
            res.status(500).json({ error: { code: 500, message: 'Ошибка при формировании распределения статусов' } });
        }
    },

    // GET /api/reports/status-history?sprint_id=...
    getStatusHistory: async (req, res) => {
        try {
            const { sprint_id } = req.query;
            if (!sprint_id) {
                return res.status(400).json({ error: { code: 400, message: 'Параметр sprint_id обязателен' } });
            }
            
            await loadStatuses();

            // Получаем данные спринта
            const sprintResult = await pool.query(
                `SELECT id_sprint, sprint_name,
                        TO_CHAR(start_date, 'YYYY-MM-DD') AS start_date,
                        TO_CHAR(end_date, 'YYYY-MM-DD') AS end_date,
                        work_days, is_active
                FROM Sprint WHERE id_sprint = $1`,
                [sprint_id]
            );
            if (sprintResult.rows.length === 0) {
                return res.status(404).json({ error: { code: 404, message: 'Спринт не найден' } });
            }
            const sprint = sprintResult.rows[0];
            // Все задачи спринта
            const tasksResult = await pool.query(
                'SELECT id_task, id_task_status, created_at FROM Task WHERE id_sprint = $1',
                [sprint_id]
            );
            const tasks = tasksResult.rows;

            // История изменений статусов для этих задач
            const historyResult = await pool.query(
                `SELECT th.id_task, th.new_status, th.changed_at
                FROM Task_History th
                JOIN Task t ON th.id_task = t.id_task
                WHERE t.id_sprint = $1
                ORDER BY th.changed_at`,
                [sprint_id]
            );
            const history = historyResult.rows;
            //console.log(history);
            // Формируем срезы по дням
            const startDate = new Date(sprint.start_date + 'T03:00:00');
            const endDate = new Date(sprint.end_date + 'T03:00:00');
            const days = [];
            const currentDate = new Date(startDate);

            // Фильтруем историю: только изменения с даты начала спринта
            const filteredHistory = history.filter(h => new Date(h.changed_at) >= startDate);

            while (currentDate <= endDate) {
                const dateStr = currentDate.toISOString().split('T')[0];
                const endOfDay = new Date(currentDate);
                endOfDay.setHours(23, 59, 59, 999);

                // Проверяем, не последний ли это день спринта
                const isLastDay = currentDate.toDateString() === endDate.toDateString();

                const counts = {};
                for (const task of tasks) {
                    if (new Date(task.created_at) > endOfDay) continue;

                    let lastChange;
                    if (isLastDay) {
                        // Для последнего дня берём последнее изменение за всё время
                        lastChange = filteredHistory
                            .filter(h => h.id_task === task.id_task)
                            .pop();
                    } else {
                        // Для остальных дней – изменения до конца дня
                        lastChange = filteredHistory
                            .filter(h => h.id_task === task.id_task && new Date(h.changed_at) <= endOfDay)
                            .pop();
                    }

                    const statusId = lastChange ? lastChange.new_status : task.id_task_status;
                    counts[statusId] = (counts[statusId] || 0) + 1;
                }

                for (const statusId of Object.keys(counts)) {
                    const statusName = taskStatuses.find(s => s.id === parseInt(statusId))?.name || 'Неизвестно';
                    days.push({
                        date: dateStr,
                        status: statusName,
                        count: counts[statusId]
                    });
                }
                currentDate.setDate(currentDate.getDate() + 1);
            }
            //console.log(days);
            res.json({ sprint: sprint.sprint_name, days });
        } catch (error) {
            console.error('Error in status history:', error);
            res.status(500).json({ error: { code: 500, message: 'Ошибка при формировании истории статусов' } });
        }
    },

    // GET /api/reports/overdue-by-assignee?team_id=
    getOverdueByAssignee: async (req, res) => {
        try {
            const { team_id } = req.query;

            let query = `
                SELECT 
                    a.id_employee,
                    a.last_name,
                    a.first_name,
                    COUNT(t.id_task) AS overdue_count
                FROM Task t
                JOIN Employee a ON t.id_assignee = a.id_employee
                WHERE t.deadline < CURRENT_DATE
                AND t.id_task_status NOT IN (5,6)
            `;
            const params = [];

            if (team_id) {
                query += ` AND a.id_team = $${params.length + 1}`;
                params.push(team_id);
            }

            query += `
                GROUP BY a.id_employee, a.last_name, a.first_name
                ORDER BY overdue_count DESC
            `;

            const result = await pool.query(query, params);

            const items = result.rows.map(row => ({
                employee: {
                    id: row.id_employee,
                    last_name: row.last_name,
                    first_name: row.first_name
                },
                overdue_count: parseInt(row.overdue_count)
            }));

            res.json({ items, total: items.length });
        } catch (error) {
            console.error('Error in overdue-by-assignee report:', error);
            res.status(500).json({ error: { code: 500, message: 'Ошибка при формировании отчёта просрочки по исполнителям' } });
        }
    },

    // GET /api/reports/top-employees?limit=5
    getTopEmployeesByHours: async (req, res) => {
        try {
            const limit = parseInt(req.query.limit) || 5;
            const { team_id } = req.query;

            // Активный спринт
            const sprintResult = await pool.query(
                'SELECT id_sprint, work_days FROM Sprint WHERE is_active = TRUE LIMIT 1'
            );
            const sprint = sprintResult.rows[0];
            if (!sprint) return res.json({ items: [] });

            let query = `
                SELECT e.id_employee, e.last_name, e.first_name, e.hours_per_day,
                    COALESCE(SUM(th.actual_hours), 0) AS total_hours
                FROM Employee e
                JOIN Task t ON e.id_employee = t.id_assignee
                LEFT JOIN Task_History th ON t.id_task = th.id_task AND th.assignee_id = e.id_employee
                WHERE t.id_sprint = $1
            `;
            const params = [sprint.id_sprint];

            if (team_id) {
                query += ` AND e.id_team = $${params.length + 1}`;
                params.push(team_id);
            }

            query += `
                GROUP BY e.id_employee, e.last_name, e.first_name, e.hours_per_day
                ORDER BY total_hours DESC
                LIMIT $${params.length + 1}
            `;
            params.push(limit);

            const result = await pool.query(query, params);
            const items = result.rows.map(row => ({
                employee: {
                    id: row.id_employee,
                    last_name: row.last_name,
                    first_name: row.first_name
                },
                total_hours: parseFloat(row.total_hours),
                capacity_hours: parseFloat(row.hours_per_day) * sprint.work_days,
                load_percent: Math.round((parseFloat(row.total_hours) / (parseFloat(row.hours_per_day) * sprint.work_days)) * 1000) / 10
            }));

            res.json({ items });
        } catch (error) {
            console.error('Error in top employees report:', error);
            res.status(500).json({ error: { code: 500, message: 'Ошибка при формировании топа сотрудников' } });
        }
    }
};

module.exports = reportController;