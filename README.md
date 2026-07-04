# ActiveSaga

ActiveSaga is a standalone Virtual Reality fitness game for the Meta Quest platform.  
The project turns home workouts into an interactive VR experience by using real body movements as gameplay controls.

Players can run in place, jump, squat, dodge, and attack inside a fantasy-inspired VR environment.  
The game includes two modes: **Run Game** and **Fight Game**, with progress saved through a backend server and MongoDB database.

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

---

## How to Run the Project

The final project is located in:

```text
Part2/
```

The system has two parts:

1. Backend API - `Part2/ActiveSaga-API`
2. Unity VR Game - `Part2/ActiveSaga-Game`

---

## 1. Backend Setup

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

Run the backend:

```bash
npm start
```

The backend should run locally on:

```text
http://localhost:3000
```

You can check that the server is running by opening:

```text
http://localhost:3000/health
```

---

## 2. Unity Project Setup

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

## 3. API URL Configuration

The Unity client communicates with the backend through API service scripts.

Main scripts to check:

```text
PlayerApiService
ApiGameResultSubmitter
```

For the deployed backend, use:

```text
https://active-saga-api.onrender.com/api/player
```

For local testing in the Unity Editor, use:

```text
http://localhost:3000/api/player
```

For testing on the Meta Quest headset with a local backend, do **not** use `localhost`.  
Use the computer's local network IP address instead:

```text
http://192.168.x.x:3000/api/player
```

Example:

```text
http://192.168.1.25:3000/api/player
```

---

## 4. Building the APK

To build the game for Meta Quest:

1. Open `Part2/ActiveSaga-Game` in Unity.
2. Open Build Settings / Build Profiles.
3. Select Android / Meta Quest as the target platform.
4. Make sure the required scenes are included.
5. Confirm that the API URL is correct.
6. Build the project as an APK.
7. Install the APK on the Meta Quest headset.

Optional installation with ADB:

```bash
adb install ActiveSaga.apk
```

To replace an existing version:

```bash
adb install -r ActiveSaga.apk
```

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

## Running the Game

1. Launch ActiveSaga on the Meta Quest headset.
2. Register or log in.
3. Review progress, daily missions, and weekly activity.
4. Select a game mode:
   - Run Game
   - Fight Game
5. Select a difficulty level.
6. Play the session using physical movements.
7. View the end-game results.
8. Return to the main menu and continue progressing.

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
