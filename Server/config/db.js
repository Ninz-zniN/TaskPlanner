const { Pool } = require('pg');

const pool = new Pool({
    host: process.env.DB_HOST || 'localhost',
    port: process.env.DB_PORT || 5433,
    database: process.env.DB_NAME || 'taskplanner',
    user: process.env.DB_USER || 'postgres',
    password: process.env.DB_PASSWORD || 137
});

module.exports = pool;