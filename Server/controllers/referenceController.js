const pool = require('../config/db');

const referenceController = {
    // GET /api/references/task-types
    getTaskTypes: async (req, res) => {
        try {
            const result = await pool.query('SELECT id_task_type AS id, type_name AS name FROM Task_Type ORDER BY id');
            res.json({ items: result.rows });
        } catch (error) {
            console.error('Error fetching task types:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // GET /api/references/task-statuses
    getTaskStatuses: async (req, res) => {
        try {
            const result = await pool.query('SELECT id_task_status AS id, status_name AS name FROM Task_Status ORDER BY id');
            res.json({ items: result.rows });
        } catch (error) {
            console.error('Error fetching task statuses:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // GET /api/references/priorities
    getPriorities: async (req, res) => {
        try {
            const result = await pool.query('SELECT id_priority AS id, priority_name AS name, priority_weight AS weight FROM Priority ORDER BY id');
            res.json({ items: result.rows });
        } catch (error) {
            console.error('Error fetching priorities:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    }
};

module.exports = referenceController;