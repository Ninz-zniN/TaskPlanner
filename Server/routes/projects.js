const express = require('express');
const router = express.Router();
const projectController = require('../controllers/projectController');
const { authenticateToken } = require('../middleware/auth');

router.get('/', authenticateToken, projectController.getAll);
router.post('/', authenticateToken, projectController.create);
router.patch('/:id', authenticateToken, projectController.update);
router.post('/:id/complete', authenticateToken, projectController.complete);
router.delete('/:id', authenticateToken, projectController.delete);

module.exports = router;