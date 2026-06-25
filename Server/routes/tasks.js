const express = require('express');
const router = express.Router();
const taskController = require('../controllers/taskController');
const { authenticateToken } = require('../middleware/auth');

router.get('/', authenticateToken, taskController.getAll);
router.post('/', authenticateToken, taskController.create);
router.patch('/:id', authenticateToken, taskController.update);
router.patch('/:id/status', authenticateToken, taskController.updateStatus);

module.exports = router;