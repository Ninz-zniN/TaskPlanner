const bcrypt = require('bcrypt');
const jwt = require('jsonwebtoken');
const pool = require('../config/db');
const { JWT_SECRET } = require('../middleware/auth');

const authController = {
    // POST /api/auth/login
    login: async (req, res) => {
        try {
            const { login, password } = req.body;

            // 1. Проверяем, что поля переданы
            if (!login || !password) {
                return res.status(400).json({ error: { code: 400, message: 'Логин и пароль обязательны' } });
            }

            // 2. Ищем пользователя вместе с ролью и сотрудником
            const userQuery = `
                SELECT u.id_user, u.login, u.password_hash, u.is_active,
                       ur.role_name as role,
                       e.id_employee, e.last_name, e.first_name, e.position,
                       t.id_team, t.team_name
                FROM Users u
                JOIN User_Role ur ON u.id_role = ur.id_role
                LEFT JOIN Employee e ON u.id_employee = e.id_employee
                LEFT JOIN Team t ON e.id_team = t.id_team
                WHERE u.login = $1
            `;
            const userResult = await pool.query(userQuery, [login]);
            const user = userResult.rows[0];

            if (!user) {
                return res.status(401).json({ error: { code: 401, message: 'Неверный логин или пароль' } });
            }

            // 3. Проверяем, активен ли пользователь
            if (!user.is_active) {
                return res.status(403).json({ error: { code: 403, message: 'Учётная запись заблокирована' } });
            }

            // 4. Проверяем пароль
            const validPassword = await bcrypt.compare(password, user.password_hash);
            if (!validPassword) {
                return res.status(401).json({ error: { code: 401, message: 'Неверный логин или пароль' } });
            }

            // 5. Генерируем JWT
            const tokenPayload = { id_user: user.id_user, login: user.login, role: user.role };
            const token = jwt.sign(tokenPayload, JWT_SECRET, { expiresIn: '24h' });
            
            await pool.query('UPDATE Users SET last_login = NOW() WHERE id_user = $1', [user.id_user]);

            // 6. Формируем ответ
            const response = {
                token: token,
                user: {
                    id_user: user.id_user,
                    login: user.login,
                    role: user.role,
                    employee: user.id_employee ? {
                        id_employee: user.id_employee,
                        last_name: user.last_name,
                        first_name: user.first_name,
                        position: user.position,
                        team: user.id_team ? {
                            id_team: user.id_team,
                            team_name: user.team_name
                        } : null
                    } : null
                }
            };

            res.json(response);
        } catch (error) {
            console.error('Login error:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    },
    // GET /api/auth/me (требует авторизации)
    me: async (req, res) => {
        try {
            const userId = req.user.id_user;

            const query = `
                SELECT u.id_user, u.login, ur.role_name as role,
                    e.id_employee, e.last_name, e.first_name, e.position,
                    t.id_team, t.team_name
                FROM Users u
                JOIN User_Role ur ON u.id_role = ur.id_role
                LEFT JOIN Employee e ON u.id_employee = e.id_employee
                LEFT JOIN Team t ON e.id_team = t.id_team
                WHERE u.id_user = $1
            `;
            const result = await pool.query(query, [userId]);
            const user = result.rows[0];

            if (!user) {
                return res.status(404).json({ error: { code: 404, message: 'Пользователь не найден' } });
            }

            const response = {
                id_user: user.id_user,
                login: user.login,
                role: user.role,
                employee: user.id_employee ? {
                    id_employee: user.id_employee,
                    last_name: user.last_name,
                    first_name: user.first_name,
                    position: user.position,
                    team: user.id_team ? {
                        id_team: user.id_team,
                        team_name: user.team_name
                    } : null
                } : null
            };

            res.json(response);
        } catch (error) {
            console.error('Me error:', error);
            res.status(500).json({ error: { code: 500, message: 'Внутренняя ошибка сервера' } });
        }
    }
};

module.exports = authController;