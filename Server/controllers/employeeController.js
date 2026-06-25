const pool = require('../config/db');

const employeeController = {
    getAll: async (req, res) => {
        try {
            const { role, id_user } = req.user;
            const { team_id, position, grade, include_dismissed } = req.query;

            let query = `
                SELECT e.id_employee, e.last_name, e.first_name, e.patronymic, e.position,
                    e.grade, e.hours_per_day, e.is_dismissed,
                    t.id_team, t.team_name
                FROM Employee e
                LEFT JOIN Team t ON e.id_team = t.id_team
                WHERE 1=1
            `;
            const params = [];
            
            // Скрываем уволенных ото всех, кроме admin и явного параметра include_dismissed=true
            if (role !== 'admin' && include_dismissed !== 'true') {
                query += ` AND e.is_dismissed = FALSE`;
            }

            if (role === 'worker') {
                const userQuery = `
                    SELECT e.id_team FROM Users u
                    JOIN Employee e ON u.id_employee = e.id_employee
                    WHERE u.id_user = $1
                `;
                const userResult = await pool.query(userQuery, [id_user]);
                const myTeamId = userResult.rows[0]?.id_team;

                if (myTeamId) {
                    query += ` AND e.id_team = $${params.length + 1}`;
                    params.push(myTeamId);
                } else {
                    query += ` AND e.id_employee = (SELECT e2.id_employee FROM Users u2 JOIN Employee e2 ON u2.id_employee = e2.id_employee WHERE u2.id_user = $${params.length + 1})`;
                    params.push(id_user);
                }
            }

            if ((role === 'admin' || role === 'project_manager') && team_id) {
                query += ` AND e.id_team = $${params.length + 1}`;
                params.push(team_id);
            }
            if (position) {
                query += ` AND e.position ILIKE $${params.length + 1}`;
                params.push(`%${position}%`);
            }
            if (grade) {
                query += ` AND e.grade = $${params.length + 1}`;
                params.push(grade);
            }

            query += ' ORDER BY e.last_name, e.first_name';

            const result = await pool.query(query, params);

            const items = result.rows.map(row => ({
                id_employee: row.id_employee,
                last_name: row.last_name,
                first_name: row.first_name,
                patronymic: row.patronymic,
                position: row.position,
                grade: row.grade,
                hours_per_day: row.hours_per_day,
                is_dismissed: row.is_dismissed,
                team: row.id_team ? {
                    id_team: row.id_team,
                    team_name: row.team_name
                } : null
            }));

            res.json({ items, total: items.length });
        } catch (error) {
            console.error('Error fetching employees:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // POST /api/employees (только admin)
    create: async (req, res) => {
        try {
            if (req.user.role !== 'admin') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const { last_name, first_name, patronymic, position, grade, id_team, hours_per_day } = req.body;
            if (!last_name || !first_name) {
                return res.status(400).json({ error: { code: 400, message: 'Фамилия и имя обязательны' } });
            }

            const result = await pool.query(
                `INSERT INTO Employee (last_name, first_name, patronymic, position, grade, id_team, hours_per_day)
                 VALUES ($1, $2, $3, $4, $5, $6, $7) RETURNING id_employee`,
                [last_name, first_name, patronymic || null, position || null,
                 grade || null, id_team || null, hours_per_day || 8]
            );
            const newId = result.rows[0].id_employee;

            // Возвращаем созданного сотрудника (используем вспомогательную функцию)
            const employee = await getEmployeeById(newId);
            res.status(201).json(employee);
        } catch (error) {
            console.error('Error creating employee:', error);
            res.status(500).json({ error: { code: 500, message: 'Ошибка при создании сотрудника' } });
        }
    },

    // PATCH /api/employees/:id (только admin)
    update: async (req, res) => {
        try {
            if (req.user.role !== 'admin') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const empId = parseInt(req.params.id);
            const { last_name, first_name, patronymic, position, grade, id_team, hours_per_day, is_dismissed } = req.body;

            const setClauses = [];
            const params = [empId];
            const fields = { last_name, first_name, patronymic, position, grade, id_team, hours_per_day, is_dismissed };
            for (const [field, value] of Object.entries(fields)) {
                if (value !== undefined) {
                    setClauses.push(`${field} = $${params.length + 1}`);
                    params.push(value);
                }
            }
            if (setClauses.length === 0) {
                return res.status(400).json({ error: { code: 400, message: 'Нет полей для обновления' } });
            }

            await pool.query(
                `UPDATE Employee SET ${setClauses.join(', ')} WHERE id_employee = $1`,
                params
            );

            const employee = await getEmployeeById(empId);
            res.json(employee);
        } catch (error) {
            console.error('Error updating employee:', error);
            res.status(500).json({ error: { code: 500, message: 'Ошибка при обновлении сотрудника' } });
        }
    },

    // DELETE /api/employees/:id (только admin) — мягкое увольнение
    dismiss: async (req, res) => {
        try {
            if (req.user.role !== 'admin') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const empId = parseInt(req.params.id);
            await pool.query('UPDATE Employee SET is_dismissed = TRUE WHERE id_employee = $1', [empId]);
            res.json({ message: 'Сотрудник уволен' });
        } catch (error) {
            console.error('Error dismissing employee:', error);
            res.status(500).json({ error: { code: 500, message: 'Ошибка при увольнении сотрудника' } });
        }
    }
};

// Вспомогательная функция для получения сотрудника по id (возвращает объект с командой)
async function getEmployeeById(id) {
    const query = `
        SELECT e.id_employee, e.last_name, e.first_name, e.patronymic, e.position,
               e.grade, e.hours_per_day, e.is_dismissed,
               t.id_team, t.team_name
        FROM Employee e
        LEFT JOIN Team t ON e.id_team = t.id_team
        WHERE e.id_employee = $1
    `;
    const result = await pool.query(query, [id]);
    const row = result.rows[0];
    if (!row) return null;
    return {
        id_employee: row.id_employee,
        last_name: row.last_name,
        first_name: row.first_name,
        patronymic: row.patronymic,
        position: row.position,
        grade: row.grade,
        hours_per_day: row.hours_per_day,
        is_dismissed: row.is_dismissed,
        team: row.id_team ? { id_team: row.id_team, team_name: row.team_name } : null
    };
}

module.exports = employeeController;