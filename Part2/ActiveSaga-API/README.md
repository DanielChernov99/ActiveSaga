🎮 ActiveSaga API Reference
Base URL: http://localhost:3000
1. Register User
Endpoint: POST /api/auth/register

Request (Unity to Server):

JSON
{
  "email": "user@example.com",
  "username": "Player123",
  "password": "yourPassword",
  "firstName": "Sasha",
  "lastName": "A"
}
Response (Server to Unity):

Success (201): {"message": "Registration successful!", "accountId": "65f1..."}

Error (400): {"message": "Missing fields or User already exists"}

2. Login User (Unified)
Endpoint: POST /api/auth/login

Request (Unity to Server):

JSON
{
  "identifier": "Username OR Email",
  "password": "yourPassword"
}
Response (Server to Unity - Success 200):

JSON
{
  "message": "Login successful!",
  "accountId": "65f1abcd...",
  "username": "Player123",
  "playerStats": {
    "firstName": "Sasha",
    "lastName": "A",
    "level": 1,
    "xp": 0,
    "coins": 0,
    "inventory": []
  }
}