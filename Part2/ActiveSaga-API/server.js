const express = require('express');
const mongoose = require('mongoose');
require('dotenv').config();

const authRoutes = require('./routes/authRoutes');

const app = express();
app.use(express.json());

const mongoURI = process.env.MONGO_URI;
const PORT = process.env.PORT || 3000;

//mongoDb connection
mongoose.connect(mongoURI)
    .then(() => console.log('✅ Connected to MongoDB successfully!'))
    .catch((err) => console.error('❌ Error connecting to MongoDB:', err.message));

app.use('/api/auth', authRoutes);

app.get('/', (req, res) => {
    res.send('ActiveSaga API is running!');
});

app.listen(PORT, () => {
    console.log(`🚀 Server running on http://localhost:${PORT}`);
});