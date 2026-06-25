const express = require('express');
const router = express.Router();
const teamController = require('../controllers/teamController');
const { authenticateToken } = require('../middleware/auth');

router.get('/', authenticateToken, teamController.getAll);
router.post('/', authenticateToken, teamController.create);
router.patch('/:id', authenticateToken, teamController.update);
router.delete('/:id', authenticateToken, teamController.delete);

module.exports = router;