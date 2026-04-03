const bcrypt = require('bcrypt');
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

        const saltRounds = 10; 
        const hashedPassword = await bcrypt.hash(password, saltRounds);

        const newAccount = new Account({ email, username, password: hashedPassword });
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


exports.loginUser = async (req, res) => {
    try {
        const { identifier, password } = req.body;

        if (!identifier || !password) {
            return res.status(400).send({ message: "Please provide email/username and password" });
        }

        const user = await Account.findOne({ 
            $or: [{ email: identifier }, { username: identifier }] 
        });

        if (!user) {
            return res.status(404).send({ message: "User not found" });
        }

        const isPasswordMatch = await bcrypt.compare(password, user.password);

        if (!isPasswordMatch) {
            return res.status(401).send({ message: "Invalid password" });
        }

        const profile = await PlayerProfile.findOne({ accountId: user._id });

        if (!profile) {
            return res.status(404).send({ message: "Account exists but player profile was not found" });
        }

        console.log(`🎮 Player logged in: ${user.username}`);

        res.status(200).send({ 
            message: "Login successful!", 
            accountId: user._id,
            username: user.username,
            playerStats: profile 
        });

    } catch (error) {
        console.error("❌ Login error:", error);
        res.status(500).send({ message: "Login failed", error: error.message });
    }
};