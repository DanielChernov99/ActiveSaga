const express = require('express');
const mongoose = require('mongoose');
const cors = require('cors');
require('dotenv').config();

const authRoutes = require('./routes/authRoutes');
const playerRoutes = require('./routes/playerRoutes');

const app = express();

app.use(cors());
app.use(express.json());

const mongoURI = process.env.MONGO_URI;
const PORT = process.env.PORT || 3000;

if (!mongoURI) {
    console.error('❌ MONGO_URI is missing');
    process.exit(1);
}

if (!process.env.JWT_SECRET) {
    console.error('❌ JWT_SECRET is missing');
    process.exit(1);
}

app.get('/', (req, res) => {
    res.send('ActiveSaga API is running!');
});

app.get('/health', (req, res) => {
    res.json({
        status: 'ok',
        message: 'ActiveSaga API is running'
    });
});

app.use('/api/auth', authRoutes);
app.use('/api/player', playerRoutes);

mongoose.connect(mongoURI)
    .then(() => {
        console.log('✅ Connected to MongoDB successfully!');

        app.listen(PORT, '0.0.0.0', () => {
            console.log(`🚀 Server running on port ${PORT}`);
        });
    })
    .catch((err) => {
        console.error('❌ Error connecting to MongoDB:', err.message);
        process.exit(1);
    });