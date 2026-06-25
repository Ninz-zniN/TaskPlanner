const express = require('express');
const router = express.Router();
const authController = require('../controllers/authController');
// GET /api/auth/me (защищённый маршрут)
const { authenticateToken } = require('../middleware/auth');

// POST /api/auth/login
router.post('/login', authController.login);
router.get('/me', authenticateToken, authController.me);

module.exports = router;