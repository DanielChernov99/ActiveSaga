# ActiveSaga

ActiveSaga is a standalone Virtual Reality fitness game for the Meta Quest platform.  
The project turns home workouts into an interactive VR experience by using real body movements as gameplay controls.

Players can run in place, jump, squat, dodge, and attack inside a fantasy-inspired VR environment.  
The game includes two modes: **Run Game** and **Fight Game**, with player progress saved through a deployed backend server and MongoDB database.

---

## Repository Structure

```text
ActiveSaga/
├── Part1/
│   └── POC, first project book, presentation, and demo videos
│
├── Part2/
│   ├── ActiveSaga-Game/      # Unity VR client
│   └── ActiveSaga-API/       # Node.js + Express backend
│
└── README.md
```

> **Part 1** contains the initial POC and documentation.  
> **Part 2** contains the final runnable project.

The final game is run from **Part2 / ActiveSaga-Game**.  
The backend is already deployed on Render, so running the backend locally is not required for normal use.

---

## Main Features

- Standalone VR game for Meta Quest
- Login and registration
- Run Game and Fight Game modes
- Easy, Medium, and Hard difficulty levels
- Real-time movement recognition:
  - Running in place
  - Jumping
  - Squatting
  - Dodging
  - Controller-based attacks
- XP, coins, levels, and saved progress
- Daily missions and weekly activity rewards
- Help screens, pause menu, and end-game results
- Backend synchronization with MongoDB

---

## Technologies

### Unity Client

- Unity
- C#
- Meta Quest
- XR / VR interaction tools
- Android build for APK deployment

### Backend

- Node.js
- Express
- MongoDB
- Mongoose
- JWT authentication
- bcrypt password hashing
- dotenv
- CORS
- Render deployment

---

## How to Run the Project

The final project is located in:

```text
Part2/
```

The system has two parts:

1. Unity VR Game - `Part2/ActiveSaga-Game`
2. Backend API - `Part2/ActiveSaga-API`

For regular use, only the Unity project or the final APK is needed.  
The backend already runs online on Render:

```text
https://active-saga-api.onrender.com
```

Health check:

```text
https://active-saga-api.onrender.com/health
```

---

## Unity Project Setup

Open the Unity project from:

```text
Part2/ActiveSaga-Game
```

Recommended Unity version:

```text
Unity 6000.3.13f1
```

Required Unity modules:

- Android Build Support
- OpenJDK
- Android SDK & NDK Tools

Required hardware:

- Meta Quest headset
- Meta Quest controllers
- Developer Mode enabled on the headset

---

## API URL Configuration

The Unity client communicates with the deployed backend through API service scripts.

Main scripts to check:

```text
PlayerApiService
ApiGameResultSubmitter
```

For the final APK version, the API URL should point to the deployed Render backend:

```text
https://active-saga-api.onrender.com/api/player
```

Local backend URLs are only needed for development or debugging.

---

## Running the Game on Meta Quest

1. Build or install the ActiveSaga APK on the Meta Quest headset.
2. Make sure the headset is connected to the internet.
3. Launch ActiveSaga.
4. Register a new account or log in with an existing account.
5. Review progress, daily missions, and weekly activity.
6. Select a game mode:
   - Run Game
   - Fight Game
7. Select a difficulty level.
8. Play the session using physical movements.
9. View the end-game results.
10. Return to the main menu and verify that progress was updated.

---

## Building the APK

To build the game for Meta Quest:

1. Open `Part2/ActiveSaga-Game` in Unity.
2. Open Build Settings / Build Profiles.
3. Select Android / Meta Quest as the target platform.
4. Make sure Android Build Support is installed.
5. Make sure the required scenes are included.
6. Confirm that the API URL points to Render:

```text
https://active-saga-api.onrender.com/api/player
```

7. Build the project as an APK.
8. Install the APK on the Meta Quest headset.

Optional installation with ADB:

```bash
adb install ActiveSaga.apk
```

To replace an existing version:

```bash
adb install -r ActiveSaga.apk
```

---

## Backend Information

The backend source code is included in:

```text
Part2/ActiveSaga-API
```

The backend is implemented with Node.js and Express and is already deployed on Render.

The Unity client uses the deployed backend for:

- User registration
- User login
- Player profile loading
- Daily missions
- Weekly activity progress
- Game session result submission
- XP, coins, levels, and saved statistics

---

## Main Backend Routes

| Method | Endpoint | Description |
|---|---|---|
| GET | `/health` | Check if the API is running |
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Log in and receive a JWT token |
| GET | `/api/player/me` | Load player profile |
| GET | `/api/player/daily-quests` | Load daily missions |
| GET | `/api/player/daily-streak` | Load weekly activity progress |
| POST | `/api/player/complete-game-session` | Submit game results and update progress |

Authenticated player routes require:

```http
Authorization: Bearer <token>
```

---

## Optional: Running the Backend Locally

Running the backend locally is not required for the final game.  
It is only needed if a developer wants to modify or debug the backend.

Go to the backend folder:

```bash
cd Part2/ActiveSaga-API
```

Install dependencies:

```bash
npm install
```

Create a `.env` file inside `Part2/ActiveSaga-API`:

```env
MONGO_URI=your_mongodb_connection_string
JWT_SECRET=your_jwt_secret_key
PORT=3000
```

Run the backend locally:

```bash
npm start
```

Local backend URL:

```text
http://localhost:3000
```

Local health check:

```text
http://localhost:3000/health
```

If testing a local backend from the Meta Quest headset, do **not** use `localhost`.  
Use the computer's local network IP address instead:

```text
http://192.168.x.x:3000/api/player
```

---

## Environment Variables

The deployed backend uses environment variables configured in Render.

Required backend variables:

```env
MONGO_URI=your_mongodb_connection_string
JWT_SECRET=your_jwt_secret_key
PORT=3000
```

Important:

- Do not commit `.env` to GitHub.
- Do not expose the MongoDB connection string.
- Do not expose the JWT secret.
- In production, environment variables should be configured in Render.

---

## Safety Notes

ActiveSaga requires physical movement.  
Before playing:

- Clear enough space around the player.
- Use the Meta Quest boundary system.
- Make sure there are no obstacles nearby.
- Take breaks when needed.
- Stop playing if discomfort occurs.

ActiveSaga is a fitness-oriented game, but it is not a medical or personalized training application.

---

## Important Notes

Do not commit sensitive or generated files such as:

```text
.env
node_modules/
Library/
Temp/
Logs/
Obj/
Build/
*.apk
*.keystore
```

The Unity client should never access MongoDB directly.  
All database operations are handled through the backend API.

---

## Authors

- Alexandra Belkind
- Daniel Chernov

## Advisor

Dr. Moshe Sulamy

## Project Context

Braude College of Engineering  
Software Engineering Department  
Final Project - Phase B  
Team Code: 26-1-D-15
