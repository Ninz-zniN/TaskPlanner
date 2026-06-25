const express = require('express');
const router = express.Router();
const sprintController = require('../controllers/sprintController');
const { authenticateToken } = require('../middleware/auth');

router.get('/', authenticateToken, sprintController.getAll);
router.get('/active', authenticateToken, sprintController.getActive);
router.post('/', authenticateToken, sprintController.create);
router.patch('/:id', authenticateToken, sprintController.update);
router.put('/:id/activate', authenticateToken, sprintController.activate);
router.post('/:id/close', authenticateToken, sprintController.close);
router.delete('/:id', authenticateToken, sprintController.delete);

module.exports = router;