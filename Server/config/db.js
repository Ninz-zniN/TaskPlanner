const { Pool } = require('pg');

const pool = new Pool({
    host: 'localhost',
    port: 5433,
    database: 'taskplanner',    // ваше имя БД
    user: 'postgres',
    password: '137'
});

module.exports = pool;