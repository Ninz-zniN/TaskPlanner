const express = require('express');
const router = express.Router();
const reportController = require('../controllers/reportController');
const { authenticateToken } = require('../middleware/auth');

router.get('/load', authenticateToken, reportController.getLoad);
router.get('/overdue', authenticateToken, reportController.getOverdue);
router.get('/burndown', authenticateToken, reportController.getBurndown);
router.get('/accuracy', authenticateToken, reportController.getAccuracy);
router.get('/project-progress', authenticateToken, reportController.getProjectProgress);
router.get('/project-summary', authenticateToken, reportController.getProjectSummary);
router.get('/project-status-distribution', authenticateToken, reportController.getProjectStatusDistribution);
router.get('/top-employees', authenticateToken, reportController.getTopEmployeesByHours);
router.get('/status-history', authenticateToken, reportController.getStatusHistory);
router.get('/overdue-by-assignee', authenticateToken, reportController.getOverdueByAssignee);

module.exports = router;