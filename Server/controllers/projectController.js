const pool = require('../config/db');

const projectController = {
    // GET /api/projects
    getAll: async (req, res) => {
        try {
            const query = `
                SELECT
                    p.id_project,
                    p.project_name,
                    p.description,
                    p.status,
                    p.completed_at,
                    CASE WHEN m.id_employee IS NOT NULL THEN
                        json_build_object('id', m.id_employee, 'last_name', m.last_name)
                    ELSE NULL END AS manager,
                    CASE WHEN t.id_team IS NOT NULL THEN
                        json_build_object('id_team', t.id_team, 'team_name', t.team_name)
                    ELSE NULL END AS team
                FROM Project p
                LEFT JOIN Employee m ON p.id_manager = m.id_employee
                LEFT JOIN Team t ON p.id_team = t.id_team
                ORDER BY p.project_name
            `;
            const result = await pool.query(query);
            const items = result.rows.map(row => ({
                id_project: row.id_project,
                project_name: row.project_name,
                description: row.description,
                status: row.status,
                completed_at: row.completed_at,
                manager: row.manager,
                team: row.team
            }));
            res.json({ items, total: items.length });
        } catch (error) {
            console.error('Error fetching projects:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // POST /api/projects (manager, admin)
    create: async (req, res) => {
        try {
            const { role } = req.user;
            if (role !== 'admin' && role !== 'project_manager') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const { project_name, description, id_manager, id_team, status } = req.body;
            if (!project_name) {
                return res.status(400).json({ error: { code: 400, message: 'Название проекта обязательно' } });
            }
            const result = await pool.query(
                `INSERT INTO Project (project_name, description, status, id_manager, id_team)
                VALUES ($1, $2, $3, $4, $5) RETURNING id_project`,
                [project_name, description || null, status || 'planning', id_manager || null, id_team || null]
            );
            const newId = result.rows[0].id_project;
            const project = await getProjectById(newId);
            res.status(201).json(project);
        } catch (error) {
            console.error('Error creating project:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // PATCH /api/projects/:id (manager, admin)
    update: async (req, res) => {
        try {
            const { role } = req.user;
            if (role !== 'admin' && role !== 'project_manager') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const projectId = parseInt(req.params.id);
            const { project_name, description, status, id_manager, id_team } = req.body;

            const existCheck = await pool.query('SELECT id_project FROM Project WHERE id_project = $1', [projectId]);
            if (existCheck.rows.length === 0) {
                return res.status(404).json({ error: { code: 404, message: 'Проект не найден' } });
            }

            const setClauses = [];
            const params = [projectId];
            const allowedFields = ['project_name', 'description', 'status', 'id_manager', 'id_team'];
            for (const field of allowedFields) {
                if (req.body[field] !== undefined) {
                    setClauses.push(`${field} = $${params.length + 1}`);
                    params.push(req.body[field]);
                }
            }
            if (setClauses.length === 0) {
                return res.status(400).json({ error: { code: 400, message: 'Нет полей для обновления' } });
            }

            await pool.query(`UPDATE Project SET ${setClauses.join(', ')} WHERE id_project = $1`, params);
            const project = await getProjectById(projectId);
            res.json(project);
        } catch (error) {
            console.error('Error updating project:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // POST /api/projects/:id/complete (manager, admin)
    complete: async (req, res) => {
        try {
            const { role } = req.user;
            if (role !== 'admin' && role !== 'project_manager') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const projectId = parseInt(req.params.id);
            const existCheck = await pool.query('SELECT status FROM Project WHERE id_project = $1', [projectId]);
            if (existCheck.rows.length === 0) {
                return res.status(404).json({ error: { code: 404, message: 'Проект не найден' } });
            }
            if (existCheck.rows[0].status === 'completed') {
                return res.status(400).json({ error: { code: 400, message: 'Проект уже завершён' } });
            }

            await pool.query("UPDATE Project SET status = 'completed', completed_at = NOW() WHERE id_project = $1", [projectId]);
            const project = await getProjectById(projectId);
            res.json(project);
        } catch (error) {
            console.error('Error completing project:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // DELETE /api/projects/:id (только admin)
    delete: async (req, res) => {
        try {
            if (req.user.role !== 'admin') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const projectId = parseInt(req.params.id);
            const existCheck = await pool.query('SELECT id_project FROM Project WHERE id_project = $1', [projectId]);
            if (existCheck.rows.length === 0) {
                return res.status(404).json({ error: { code: 404, message: 'Проект не найден' } });
            }
            await pool.query('DELETE FROM Project WHERE id_project = $1', [projectId]);
            res.json({ message: 'Проект удалён' });
        } catch (error) {
            console.error('Error deleting project:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    }
};

// Вспомогательная функция
async function getProjectById(projectId) {
    const query = `
        SELECT
            p.id_project,
            p.project_name,
            p.description,
            p.status,
            CASE WHEN m.id_employee IS NOT NULL THEN
                json_build_object('id', m.id_employee, 'last_name', m.last_name)
            ELSE NULL END AS manager,
            CASE WHEN t.id_team IS NOT NULL THEN
                json_build_object('id_team', t.id_team, 'team_name', t.team_name)
            ELSE NULL END AS team
        FROM Project p
        LEFT JOIN Employee m ON p.id_manager = m.id_employee
        LEFT JOIN Team t ON p.id_team = t.id_team
        WHERE p.id_project = $1
    `;
    const result = await pool.query(query, [projectId]);
    const row = result.rows[0];
    return {
        id_project: row.id_project,
        project_name: row.project_name,
        description: row.description,
        status: row.status,
        manager: row.manager,
        team: row.team
    };
}

module.exports = projectController;