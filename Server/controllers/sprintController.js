const pool = require('../config/db');

const sprintController = {
    // GET /api/sprints
    getAll: async (req, res) => {
        try {
            const query = `
                SELECT id_sprint, sprint_name, start_date, end_date, work_days, is_active
                FROM Sprint ORDER BY start_date DESC
            `;
            const result = await pool.query(query);
            const items = result.rows.map(row => ({
                id_sprint: row.id_sprint,
                sprint_name: row.sprint_name,
                start_date: row.start_date,
                end_date: row.end_date,
                work_days: row.work_days,
                is_active: row.is_active
            }));
            res.json({ items, total: items.length });
        } catch (error) {
            console.error('Error fetching sprints:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // GET /api/sprints/active
    getActive: async (req, res) => {
        try {
            const query = 'SELECT * FROM Sprint WHERE is_active = TRUE LIMIT 1';
            const result = await pool.query(query);
            if (result.rows.length === 0) {
                return res.status(404).json({ error: { code: 404, message: 'Нет активного спринта' } });
            }
            const row = result.rows[0];
            res.json({
                id_sprint: row.id_sprint,
                sprint_name: row.sprint_name,
                start_date: row.start_date,
                end_date: row.end_date,
                work_days: row.work_days,
                is_active: row.is_active
            });
        } catch (error) {
            console.error('Error fetching active sprint:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // POST /api/sprints (manager, admin)
    create: async (req, res) => {
        try {
            const { role } = req.user;
            if (role !== 'admin' && role !== 'project_manager') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const { sprint_name, start_date, end_date, work_days } = req.body;
            if (!sprint_name || !start_date || !end_date || !work_days) {
                return res.status(400).json({ error: { code: 400, message: 'Все поля обязательны' } });
            }

            const result = await pool.query(
                `INSERT INTO Sprint (sprint_name, start_date, end_date, work_days, is_active)
                 VALUES ($1, $2, $3, $4, FALSE) RETURNING id_sprint`,
                [sprint_name, start_date, end_date, work_days]
            );
            const newId = result.rows[0].id_sprint;
            const sprint = await getSprintById(newId);
            res.status(201).json(sprint);
        } catch (error) {
            console.error('Error creating sprint:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // PATCH /api/sprints/:id (manager, admin)
    update: async (req, res) => {
        try {
            const { role } = req.user;
            if (role !== 'admin' && role !== 'project_manager') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const sprintId = parseInt(req.params.id);
            const { sprint_name, start_date, end_date, work_days } = req.body;

            const existCheck = await pool.query('SELECT id_sprint FROM Sprint WHERE id_sprint = $1', [sprintId]);
            if (existCheck.rows.length === 0) {
                return res.status(404).json({ error: { code: 404, message: 'Спринт не найден' } });
            }

            const setClauses = [];
            const params = [sprintId];
            const allowedFields = ['sprint_name', 'start_date', 'end_date', 'work_days'];
            for (const field of allowedFields) {
                if (req.body[field] !== undefined) {
                    setClauses.push(`${field} = $${params.length + 1}`);
                    params.push(req.body[field]);
                }
            }
            if (setClauses.length === 0) {
                return res.status(400).json({ error: { code: 400, message: 'Нет полей для обновления' } });
            }

            await pool.query(`UPDATE Sprint SET ${setClauses.join(', ')} WHERE id_sprint = $1`, params);
            const sprint = await getSprintById(sprintId);
            res.json(sprint);
        } catch (error) {
            console.error('Error updating sprint:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // PUT /api/sprints/:id/activate
    activate: async (req, res) => {
        try {
            const { role } = req.user;
            if (role !== 'admin' && role !== 'project_manager') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const sprintId = parseInt(req.params.id);

            // Проверяем, есть ли активный спринт с незавершёнными задачами
            const activeCheck = await pool.query(
                `SELECT s.id_sprint, s.sprint_name,
                        COUNT(t.id_task) FILTER (WHERE t.id_task_status NOT IN (5,6)) AS unfinished
                FROM Sprint s
                LEFT JOIN Task t ON s.id_sprint = t.id_sprint
                WHERE s.is_active = TRUE
                GROUP BY s.id_sprint`
            );

            if (activeCheck.rows.length > 0 && parseInt(activeCheck.rows[0].unfinished) > 0) {
                return res.status(409).json({
                    error: {
                        code: 409,
                        message: 'В активном спринте есть незавершённые задачи. Закройте его перед активацией нового.'
                    },
                    activeSprint: {
                        id_sprint: activeCheck.rows[0].id_sprint,
                        sprint_name: activeCheck.rows[0].sprint_name,
                        unfinished_tasks: parseInt(activeCheck.rows[0].unfinished)
                    }
                });
            }

            // Деактивируем все спринты
            await pool.query('UPDATE Sprint SET is_active = FALSE');
            // Активируем выбранный
            await pool.query('UPDATE Sprint SET is_active = TRUE WHERE id_sprint = $1', [sprintId]);

            const sprint = await getSprintById(sprintId);
            res.json(sprint);
        } catch (error) {
            console.error('Error activating sprint:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // POST /api/sprints/:id/close
    close: async (req, res) => {
        try {
            const { role } = req.user;
            if (role !== 'admin' && role !== 'project_manager') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const sprintId = parseInt(req.params.id);
            const { action } = req.body; // move_to_next | move_to_backlog | cancel_tasks

            const existCheck = await pool.query('SELECT * FROM Sprint WHERE id_sprint = $1', [sprintId]);
            if (existCheck.rows.length === 0) {
                return res.status(404).json({ error: { code: 404, message: 'Спринт не найден' } });
            }

            if (action === 'move_to_next') {
                // Перенести незавершённые задачи в следующий спринт (самый ранний по дате)
                const nextSprint = await pool.query(
                    "SELECT id_sprint FROM Sprint WHERE start_date > (SELECT end_date FROM Sprint WHERE id_sprint = $1) AND id_sprint != $1 ORDER BY start_date LIMIT 1",
                    [sprintId]
                );
                if (nextSprint.rows.length > 0) {
                    await pool.query(
                        "UPDATE Task SET id_sprint = $1 WHERE id_sprint = $2 AND id_task_status NOT IN (5,6)",
                        [nextSprint.rows[0].id_sprint, sprintId]
                    );
                }
            } else if (action === 'move_to_backlog') {
                // Убрать спринт у незавершённых задач (вернуть в бэклог)
                await pool.query(
                    "UPDATE Task SET id_sprint = NULL WHERE id_sprint = $1 AND id_task_status NOT IN (5,6)",
                    [sprintId]
                );
            } else if (action === 'cancel_tasks') {
                // 1. Получить список задач, которые будут отменены, вместе с текущим статусом и исполнителем
                const tasksToCancel = await pool.query(
                    `SELECT id_task, id_task_status, id_assignee 
                    FROM Task 
                    WHERE id_sprint = $1 AND id_task_status NOT IN (5,6)`,
                    [sprintId]
                );

                // 2. Обновить статус на "Отменена" и убрать привязку к спринту
                await pool.query(
                    `UPDATE Task SET id_task_status = 6, id_sprint = NULL, updated_at = NOW() 
                    WHERE id_sprint = $1 AND id_task_status NOT IN (5,6)`,
                    [sprintId]
                );

                // 3. Вставить записи в историю для каждой затронутой задачи
                for (const task of tasksToCancel.rows) {
                    await pool.query(
                        `INSERT INTO Task_History (id_task, old_status, new_status, changed_by, assignee_id, changed_at, actual_hours)
                        VALUES ($1, $2, 6, $3, $4, NOW(), 0)`,
                        [task.id_task, task.id_task_status, req.user.id_user, task.id_assignee]
                    );
                }
            }

            await pool.query('UPDATE Sprint SET is_active = FALSE WHERE id_sprint = $1', [sprintId]);
            res.json({
                closed_sprint: { id_sprint: sprintId, sprint_name: existCheck.rows[0].sprint_name },
                affected_tasks: (await pool.query("SELECT COUNT(*) FROM Task WHERE id_sprint = $1 AND id_task_status NOT IN (5,6)", [sprintId])).rows[0].count
            });
        } catch (error) {
            console.error('Error closing sprint:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // DELETE /api/sprints/:id (manager, admin)
    delete: async (req, res) => {
        try {
            const { role } = req.user;
            if (role !== 'admin' && role !== 'project_manager') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const sprintId = parseInt(req.params.id);
            await pool.query('DELETE FROM Sprint WHERE id_sprint = $1', [sprintId]);
            res.json({ message: 'Спринт удалён' });
        } catch (error) {
            console.error('Error deleting sprint:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    }
};

async function getSprintById(sprintId) {
    const result = await pool.query('SELECT * FROM Sprint WHERE id_sprint = $1', [sprintId]);
    const row = result.rows[0];
    return {
        id_sprint: row.id_sprint,
        sprint_name: row.sprint_name,
        start_date: row.start_date,
        end_date: row.end_date,
        work_days: row.work_days,
        is_active: row.is_active
    };
}

module.exports = sprintController;