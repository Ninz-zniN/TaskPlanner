const express = require('express');
const router = express.Router();
const referenceController = require('../controllers/referenceController');

router.get('/task-types', referenceController.getTaskTypes);
router.get('/task-statuses', referenceController.getTaskStatuses);
router.get('/priorities', referenceController.getPriorities);

module.exports = router;