const pool = require('../config/db');

const teamController = {
    // GET /api/teams
    getAll: async (req, res) => {
        try {
            const query = `
                SELECT
                    t.id_team,
                    t.team_name,
                    CASE WHEN tl.id_employee IS NOT NULL THEN
                        json_build_object('id_employee', tl.id_employee, 'last_name', tl.last_name, 'first_name', tl.first_name)
                    ELSE NULL END AS team_lead
                FROM Team t
                LEFT JOIN Employee tl ON t.team_lead_id = tl.id_employee
                ORDER BY t.team_name
            `;
            const result = await pool.query(query);

            const items = result.rows.map(row => ({
                id_team: row.id_team,
                team_name: row.team_name,
                team_lead: row.team_lead
            }));

            res.json({ items, total: items.length });
        } catch (error) {
            console.error('Error fetching teams:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // POST /api/teams (только admin)
    create: async (req, res) => {
        try {
            if (req.user.role !== 'admin') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const { team_name, team_lead_id } = req.body;
            if (!team_name) {
                return res.status(400).json({ error: { code: 400, message: 'Название команды обязательно' } });
            }

            const result = await pool.query(
                'INSERT INTO Team (team_name, team_lead_id) VALUES ($1, $2) RETURNING id_team',
                [team_name, team_lead_id || null]
            );
            const newId = result.rows[0].id_team;

            // Возвращаем созданную команду
            const team = await getTeamById(newId);
            res.status(201).json(team);
        } catch (error) {
            console.error('Error creating team:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // PATCH /api/teams/:id (только admin)
    update: async (req, res) => {
        try {
            if (req.user.role !== 'admin') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const teamId = parseInt(req.params.id);   // ← явно в число
            const { team_name, team_lead_id } = req.body;

            // Проверяем, существует ли команда
            const existCheck = await pool.query('SELECT id_team FROM Team WHERE id_team = $1', [teamId]);
            if (existCheck.rows.length === 0) {
                return res.status(404).json({ error: { code: 404, message: 'Команда не найдена' } });
            }

            const setClauses = [];
            const params = [teamId];
            if (team_name !== undefined) {
                setClauses.push(`team_name = $${params.length + 1}`);
                params.push(team_name);
            }
            if (team_lead_id !== undefined) {
                setClauses.push(`team_lead_id = $${params.length + 1}`);
                params.push(team_lead_id);
            }
            if (setClauses.length === 0) {
                return res.status(400).json({ error: { code: 400, message: 'Нет полей для обновления' } });
            }

            await pool.query(
                `UPDATE Team SET ${setClauses.join(', ')} WHERE id_team = $1`,
                params
            );

            const team = await getTeamById(teamId);
            res.json(team);
        } catch (error) {
            console.error('Error updating team:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // DELETE /api/teams/:id (только admin, только если нет сотрудников)
    delete: async (req, res) => {
        try {
            if (req.user.role !== 'admin') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const teamId = req.params.id;

            // Проверяем, есть ли сотрудники в команде
            const empCheck = await pool.query('SELECT COUNT(*) FROM Employee WHERE id_team = $1', [teamId]);
            if (parseInt(empCheck.rows[0].count) > 0) {
                return res.status(400).json({ error: { code: 400, message: 'Нельзя удалить команду, в которой есть сотрудники' } });
            }

            await pool.query('DELETE FROM Team WHERE id_team = $1', [teamId]);
            res.json({ message: 'Команда удалена' });
        } catch (error) {
            console.error('Error deleting team:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    }
};

// Вспомогательная функция
async function getTeamById(teamId) {
    const query = `
        SELECT
            t.id_team,
            t.team_name,
            CASE WHEN tl.id_employee IS NOT NULL THEN
                json_build_object('id_employee', tl.id_employee, 'last_name', tl.last_name, 'first_name', tl.first_name)
            ELSE NULL END AS team_lead
        FROM Team t
        LEFT JOIN Employee tl ON t.team_lead_id = tl.id_employee
        WHERE t.id_team = $1
    `;
    const result = await pool.query(query, [teamId]);
    const row = result.rows[0];
    return {
        id_team: row.id_team,
        team_name: row.team_name,
        team_lead: row.team_lead
    };
}

module.exports = teamController;