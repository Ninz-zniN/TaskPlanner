const express = require('express');
const router = express.Router();
const employeeController = require('../controllers/employeeController');
const { authenticateToken } = require('../middleware/auth');

router.get('/', authenticateToken, employeeController.getAll);
router.post('/', authenticateToken, employeeController.create);
router.patch('/:id', authenticateToken, employeeController.update);
router.delete('/:id', authenticateToken, employeeController.dismiss);

module.exports = router;