const pool = require('../config/db');
const bcrypt = require('bcrypt');

const userController = {
    // GET /api/users (только admin)
    getAll: async (req, res) => {
        try {
            if (req.user.role !== 'admin') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const query = `
                SELECT u.id_user, u.login, ur.role_name AS role, u.is_active,
                       CASE WHEN e.id_employee IS NOT NULL THEN
                           json_build_object('id_employee', e.id_employee, 'last_name', e.last_name, 'first_name', e.first_name)
                       ELSE NULL END AS employee
                FROM Users u
                JOIN User_Role ur ON u.id_role = ur.id_role
                LEFT JOIN Employee e ON u.id_employee = e.id_employee
                ORDER BY u.login
            `;
            const result = await pool.query(query);
            const items = result.rows.map(row => ({
                id_user: row.id_user,
                login: row.login,
                role: row.role,
                is_active: row.is_active,
                employee: row.employee
            }));
            res.json({ items, total: items.length });
        } catch (error) {
            console.error('Error fetching users:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // POST /api/users (только admin)
    create: async (req, res) => {
        try {
            if (req.user.role !== 'admin') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const { login, password, role, employee_id } = req.body;
            if (!login || !password) {
                return res.status(400).json({ error: { code: 400, message: 'Логин и пароль обязательны' } });
            }

            // Проверка уникальности логина
            const dup = await pool.query('SELECT id_user FROM Users WHERE login = $1', [login]);
            if (dup.rows.length > 0) {
                return res.status(400).json({ error: { code: 400, message: 'Пользователь с таким логином уже существует' } });
            }

            const passwordHash = await bcrypt.hash(password, 10);
            const roleResult = await pool.query('SELECT id_role FROM User_Role WHERE role_name = $1', [role || 'worker']);
            const idRole = roleResult.rows[0]?.id_role || (await pool.query("SELECT id_role FROM User_Role WHERE role_name = 'worker'")).rows[0].id_role;

            const result = await pool.query(
                'INSERT INTO Users (login, password_hash, id_role, id_employee, is_active) VALUES ($1, $2, $3, $4, TRUE) RETURNING id_user',
                [login, passwordHash, idRole, employee_id || null]
            );
            const newId = result.rows[0].id_user;

            // Возвращаем созданного пользователя
            const user = await getUserById(newId);
            res.status(201).json(user);
        } catch (error) {
            console.error('Error creating user:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // PATCH /api/users/:id (только admin)
    update: async (req, res) => {
        try {
            if (req.user.role !== 'admin') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const userId = parseInt(req.params.id);
            const { login, password, role, employee_id, is_active } = req.body;

            const existCheck = await pool.query('SELECT id_user FROM Users WHERE id_user = $1', [userId]);
            if (existCheck.rows.length === 0) {
                return res.status(404).json({ error: { code: 404, message: 'Пользователь не найден' } });
            }

            const setClauses = [];
            const params = [userId];
            if (login !== undefined) {
                setClauses.push(`login = $${params.length + 1}`);
                params.push(login);
            }
            if (password !== undefined) {
                const passwordHash = await bcrypt.hash(password, 10);
                setClauses.push(`password_hash = $${params.length + 1}`);
                params.push(passwordHash);
            }
            if (role !== undefined) {
                const roleResult = await pool.query('SELECT id_role FROM User_Role WHERE role_name = $1', [role]);
                if (roleResult.rows.length > 0) {
                    setClauses.push(`id_role = $${params.length + 1}`);
                    params.push(roleResult.rows[0].id_role);
                }
            }
            if (employee_id !== undefined) {
                setClauses.push(`id_employee = $${params.length + 1}`);
                params.push(employee_id);
            }
            if (is_active !== undefined) {
                setClauses.push(`is_active = $${params.length + 1}`);
                params.push(is_active);
            }
            if (setClauses.length === 0) {
                return res.status(400).json({ error: { code: 400, message: 'Нет полей для обновления' } });
            }

            await pool.query(`UPDATE Users SET ${setClauses.join(', ')} WHERE id_user = $1`, params);
            const user = await getUserById(userId);
            res.json(user);
        } catch (error) {
            console.error('Error updating user:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },

    // DELETE /api/users/:id (только admin, блокировка)
    remove: async (req, res) => {
        try {
            if (req.user.role !== 'admin') {
                return res.status(403).json({ error: { code: 403, message: 'Недостаточно прав' } });
            }
            const userId = parseInt(req.params.id);
            await pool.query('UPDATE Users SET is_active = FALSE WHERE id_user = $1', [userId]);
            res.json({ message: 'Пользователь заблокирован' });
        } catch (error) {
            console.error('Error deactivating user:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    }
};

async function getUserById(userId) {
    const query = `
        SELECT u.id_user, u.login, ur.role_name AS role, u.is_active,
               CASE WHEN e.id_employee IS NOT NULL THEN
                   json_build_object('id_employee', e.id_employee, 'last_name', e.last_name, 'first_name', e.first_name)
               ELSE NULL END AS employee
        FROM Users u
        JOIN User_Role ur ON u.id_role = ur.id_role
        LEFT JOIN Employee e ON u.id_employee = e.id_employee
        WHERE u.id_user = $1
    `;
    const result = await pool.query(query, [userId]);
    const row = result.rows[0];
    return {
        id_user: row.id_user,
        login: row.login,
        role: row.role,
        is_active: row.is_active,
        employee: row.employee
    };
}

module.exports = userController;