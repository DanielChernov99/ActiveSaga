# 🎮 ActiveSaga API Reference
**Base URL:** `http://localhost:3000`

---

## 1. Register User
**Endpoint:** `POST /api/auth/register`

### Request (Unity to Server)
```json
{
  "email": "user@example.com",
  "username": "Player123",
  "password": "yourPassword",
  "firstName": "Sasha",
  "lastName": "A"
}
### Response (Server to Unity)
```json
{
  "message": "Registration successful!",
  "accountId": "65f1abcd1234..."
}

---

## 2. Login User (Unified)
**Endpoint:** `POST /api/auth/login`

### Request (Unity to Server)
```json
{
  "identifier": "Username OR Email",
  "password": "yourPassword"
}
### Response (Server to Unity - Success 200)
```json
{
  "message": "Login successful!",
  "accountId": "65f1abcd1234...",
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

