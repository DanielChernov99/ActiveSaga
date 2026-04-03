const Account = require('../models/Account');
const PlayerProfile = require('../models/PlayerProfile');

exports.registerUser = async (req, res) => {
    try {
        const { email, username, password, firstName, lastName } = req.body;

        if (!email || !username || !password || !firstName || !lastName) {
             return res.status(400).send({ message: "Missing required fields (email, username, password, firstName, lastName)" });
        }

        const existingAccount = await Account.findOne({ $or: [{ email }, { username }] });
        if (existingAccount) {
            return res.status(400).send({ message: "Email or Username already exists!" });
        }

        const newAccount = new Account({ email, username, password });
        const savedAccount = await newAccount.save();

        const newPlayerProfile = new PlayerProfile({
            accountId: savedAccount._id,
            firstName: firstName,
            lastName: lastName
        });
        await newPlayerProfile.save();

        console.log(`🎮 New player registered: ${username}`);
        res.status(201).send({ message: "Registration successful!", accountId: savedAccount._id });

    } catch (error) {
        console.error("❌ Registration error:", error);
        res.status(500).send({ message: "Registration failed", error: error.message });
    }
};