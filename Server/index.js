const express = require('express');
const cors = require('cors');

const app = express();
const PORT = 3001; // чтобы не мешать mock-серверу

const authRoutes = require('./routes/auth');
const referenceRoutes = require('./routes/references');
const employeeRoutes = require('./routes/employees');
const taskRoutes = require('./routes/tasks');
const teamRoutes = require('./routes/teams');
const projectRoutes = require('./routes/projects');
const sprintRoutes = require('./routes/sprints');
const reportRoutes = require('./routes/reports');
const userRoutes = require('./routes/users');
const noteRoutes = require('./routes/notes');

app.use(cors());
app.use(express.json());

app.use('/api/auth', authRoutes);
app.use('/api/references', referenceRoutes);
app.use('/api/employees', employeeRoutes);
app.use('/api/tasks', taskRoutes);
app.use('/api/teams', teamRoutes);
app.use('/api/projects', projectRoutes);
app.use('/api/sprints', sprintRoutes);
app.use('/api/reports', reportRoutes);
app.use('/api/users', userRoutes);
app.use('/api', noteRoutes);

// Подключаем маршруты (пока заглушки)
app.get('/', (req, res) => res.send('TaskPlanner API is running'));


app.listen(PORT, () => {
    console.log(`Real server running on http://localhost:${PORT}`);
});