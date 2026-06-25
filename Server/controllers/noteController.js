const pool = require('../config/db');

const noteController = {
    // GET /api/tasks/:id/notes
    getByTask: async (req, res) => {
        try {
            const taskId = req.params.id;

            // Проверяем существование задачи
            const taskCheck = await pool.query('SELECT id_assignee FROM Task WHERE id_task = $1', [taskId]);
            if (taskCheck.rows.length === 0) {
                return res.status(404).json({ error: { code: 404, message: 'Задача не найдена' } });
            }

            const result = await pool.query(
                `SELECT tn.id_note, tn.id_task, tn.content, tn.created_at,
                        u.id_user, u.login,
                        e.last_name, e.first_name
                 FROM Task_Note tn
                 JOIN Users u ON tn.author_id = u.id_user
                 LEFT JOIN Employee e ON u.id_employee = e.id_employee
                 WHERE tn.id_task = $1
                 ORDER BY tn.created_at`,
                [taskId]
            );

            const items = result.rows.map(row => ({
                id_note: row.id_note,
                id_task: row.id_task,
                content: row.content,
                created_at: row.created_at,
                author: {
                    id_user: row.id_user,
                    login: row.login,
                    last_name: row.last_name,
                    first_name: row.first_name
                }
            }));

            res.json({ items, total: items.length });
        } catch (error) {
            console.error('Error fetching notes:', error);
            res.status(500).json({ error: { code: 500, message: 'Ошибка при получении заметок' } });
        }
    },

    // POST /api/tasks/:id/notes
    create: async (req, res) => {
        try {
            const taskId = req.params.id;
            const { content } = req.body;
            const userId = req.user.id_user;
            const role = req.user.role;

            if (!content || !content.trim()) {
                return res.status(400).json({ error: { code: 400, message: 'Текст заметки обязателен' } });
            }

            // Проверяем задачу и права
            const taskCheck = await pool.query('SELECT id_assignee FROM Task WHERE id_task = $1', [taskId]);
            if (taskCheck.rows.length === 0) {
                return res.status(404).json({ error: { code: 404, message: 'Задача не найдена' } });
            }

            if (role === 'worker') {
                const myEmployeeId = await getEmployeeIdByUserId(userId);
                if (taskCheck.rows[0].id_assignee !== myEmployeeId) {
                    return res.status(403).json({ error: { code: 403, message: 'Вы можете добавлять заметки только к своим задачам' } });
                }
            } else if (role !== 'admin' && role !== 'project_manager') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }

            const result = await pool.query(
                `INSERT INTO Task_Note (id_task, author_id, content)
                 VALUES ($1, $2, $3) RETURNING id_note, created_at`,
                [taskId, userId, content.trim()]
            );

            const note = result.rows[0];
            res.status(201).json({
                id_note: note.id_note,
                id_task: parseInt(taskId),
                content: content.trim(),
                created_at: note.created_at,
                author: { id_user: userId, login: req.user.login }
            });
        } catch (error) {
            console.error('Error creating note:', error);
            res.status(500).json({ error: { code: 500, message: 'Ошибка при добавлении заметки' } });
        }
    },

    // DELETE /api/notes/:id
    delete: async (req, res) => {
        try {
            const noteId = req.params.id;
            const userId = req.user.id_user;
            const role = req.user.role;

            const noteCheck = await pool.query('SELECT * FROM Task_Note WHERE id_note = $1', [noteId]);
            if (noteCheck.rows.length === 0) {
                return res.status(404).json({ error: { code: 404, message: 'Заметка не найдена' } });
            }

            const note = noteCheck.rows[0];
            if (role !== 'admin' && role !== 'project_manager' && note.author_id !== userId) {
                return res.status(403).json({ error: { code: 403, message: 'Нет прав на удаление этой заметки' } });
            }

            await pool.query('DELETE FROM Task_Note WHERE id_note = $1', [noteId]);
            res.json({ message: 'Заметка удалена' });
        } catch (error) {
            console.error('Error deleting note:', error);
            res.status(500).json({ error: { code: 500, message: 'Ошибка при удалении заметки' } });
        }
    }
};

async function getEmployeeIdByUserId(userId) {
    const res = await pool.query('SELECT id_employee FROM Users WHERE id_user = $1', [userId]);
    return res.rows[0]?.id_employee || null;
}

module.exports = noteController;