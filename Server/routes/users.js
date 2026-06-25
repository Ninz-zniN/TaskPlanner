const express = require('express');
const router = express.Router();
const userController = require('../controllers/userController');
const { authenticateToken } = require('../middleware/auth');

router.get('/', authenticateToken, userController.getAll);
router.post('/', authenticateToken, userController.create);
router.patch('/:id', authenticateToken, userController.update);
router.delete('/:id', authenticateToken, userController.remove);

module.exports = router;