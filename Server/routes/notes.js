const express = require('express');
const router = express.Router();
const noteController = require('../controllers/noteController');
const { authenticateToken } = require('../middleware/auth');

router.get('/tasks/:id/notes', authenticateToken, noteController.getByTask);
router.post('/tasks/:id/notes', authenticateToken, noteController.create);
router.delete('/notes/:id', authenticateToken, noteController.delete);

module.exports = router;