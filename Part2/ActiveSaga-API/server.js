const express = require('express');
const mongoose = require('mongoose');
require('dotenv').config();

const app = express();
app.use(express.json());

const mongoURI = process.env.MONGO_URI;
const PORT = process.env.PORT || 3000;

mongoose.connect(mongoURI)
    .then(() => console.log('✅ Connected to MongoDB successfully!'))
    .catch((err) => console.error('❌ Error connecting to MongoDB:', err.message));

app.get('/', (req, res) => {
    res.send('ActiveSaga API is running!');
});

app.listen(PORT, () => {
    console.log(`🚀 Server running on http://localhost:${PORT}`);
});