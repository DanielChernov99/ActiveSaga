const jwt = require('jsonwebtoken');

module.exports = function (req, res, next) {
    // Get token from the header
    const token = req.header('Authorization');

    // Check if no token
    if (!token) {
        return res.status(401).json({ message: "No token, authorization denied" });
    }

    try {
        // Remove 'Bearer ' prefix if it exists
        const tokenString = token.startsWith('Bearer ') ? token.slice(7, token.length) : token;

        // Verify token
        const decoded = jwt.verify(tokenString, process.env.JWT_SECRET);

        // Add user from payload to request object
        req.user = decoded;
        next();
    } catch (err) {
        res.status(401).json({ message: "Token is not valid" });
    }
};