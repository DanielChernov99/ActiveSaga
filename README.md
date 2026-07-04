# ActiveSaga

ActiveSaga is a standalone Virtual Reality fitness game developed for the Meta Quest platform as a final Software Engineering capstone project.

The game turns home workouts into an interactive VR experience. Instead of following a regular workout routine, the player performs real physical movements inside a fantasy-inspired game environment. The system detects running in place, jumping, squatting, dodging, and controller-based attacks using the Meta Quest headset and controllers.

## Repository Overview

This repository contains both project phases.

```text
ActiveSaga/
├── Part1/
│   ├── ActiveSaga-POC/
│   ├── ActiveSaga.pdf
│   ├── ActiveSaga_208169805_323419416 .pptx
│   ├── DanielRuns (1).mp4
│   └── ScreenDemo.mp4
│
├── Part2/
│   ├── ActiveSaga-Game/
│   │   ├── Assets/
│   │   ├── Packages/
│   │   └── ProjectSettings/
│   │
│   └── ActiveSaga-API/
│       ├── controllers/
│       ├── middleware/
│       ├── models/
│       ├── routes/
│       ├── services/
│       ├── package.json
│       └── server.js
│
└── README.md
```

## Project Phases

### Part 1 - POC and Initial Documentation

`Part1` contains the first project phase, including the proof of concept, first project book, presentation, and early demo videos.

This part is mainly for documentation and historical reference.  
The final runnable version of the project is located in `Part2`.

### Part 2 - Final System

`Part2` contains the final implementation:

- `ActiveSaga-Game` - the Unity VR client for Meta Quest.
- `ActiveSaga-API` - the Node.js and Express backend connected to MongoDB.

The game should be run from `Part2`.

## Main Features

- Standalone VR game for Meta Quest
- Login and registration
- Two playable game modes:
  - Run Game
  - Fight Game
- Three difficulty levels
- Real-time movement recognition:
  - Running in place
  - Jumping
  - Squatting
  - Dodging
  - Controller-based attacks
- XP, coins, levels, and saved progress
- Daily missions
- Weekly activity rewards
- Help screens
- Pause and resume functionality
- End-game results screen
- Backend synchronization with MongoDB

## Technologies

### Unity Client

- Unity
- C#
- Meta Quest
- OpenXR / XR Interaction Toolkit
- Universal Render Pipeline
- Unity Input System

### Backend

- Node.js
- Express
- MongoDB
- Mongoose
- JWT authentication
- bcrypt password hashing
- dotenv
- CORS

### Deployment and Tools

- Render
- Git / GitHub
- MongoDB Atlas
- Unity Hub
- Meta Quest Developer tools / ADB

## System Architecture

ActiveSaga uses a client-server architecture.

The Unity client runs on the Meta Quest headset and handles real-time gameplay, movement recognition, VR interaction, UI, audio, pause logic, and results display.

The backend handles authentication, player profile loading, reward calculation, daily missions, weekly progress, and persistent data storage in MongoDB.

```text
Meta Quest Headset
        |
        v
Unity VR Client
        |
        | REST API Requests
        v
Node.js + Express Backend
        |
        v
MongoDB Database
```

## Backend Setup

The backend is located in:

```bash
Part2/ActiveSaga-API
```

### Requirements

Before running the backend locally, install:

- Node.js
- npm
- MongoDB Atlas account or local MongoDB database

### Installation

From the repository root:

```bash
cd Part2/ActiveSaga-API
npm install
```

### Environment Variables

Create a `.env` file inside:

```bash
Part2/ActiveSaga-API/.env
```

Example `.env` file:

```env
MONGO_URI=your_mongodb_connection_string
JWT_SECRET=your_jwt_secret_key
PORT=3000
```

Important:

- Do not commit `.env` to GitHub.
- Do not expose the MongoDB connection string.
- Do not expose the JWT secret.
- In production, define these variables in the Render dashboard instead of committing them to the repository.

### Running the Backend Locally

```bash
npm start
```

The backend should start on:

```text
http://localhost:3000
```

Health check:

```text
http://localhost:3000/health
```

Expected response:

```json
{
  "status": "ok",
  "message": "ActiveSaga API is running"
}
```

## Main Backend Routes

### General

| Method | Endpoint | Description |
|---|---|---|
| GET | `/` | Checks that the API is running |
| GET | `/health` | Health check endpoint |

### Authentication

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Log in an existing user |

### Player

Authenticated player routes require a JWT token in the request header:

```http
Authorization: Bearer <token>
```

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/player/me` | Load authenticated player data |
| GET | `/api/player/daily-quests` | Load daily missions |
| GET | `/api/player/daily-streak` | Load weekly/daily activity progress |
| POST | `/api/player/complete-game-session` | Submit a completed game session and update progress |
| POST | `/api/player/update-stats` | Legacy route that submits session stats |

## Unity Client Setup

The Unity project is located in:

```bash
Part2/ActiveSaga-Game
```

### Requirements

Install:

- Unity Hub
- Unity `6000.3.13f1`
- Android Build Support for Unity
- Meta Quest headset
- Meta Quest controllers
- USB cable or Meta Quest Link / developer connection
- Developer Mode enabled on the Meta Quest headset

### Opening the Unity Project

1. Open Unity Hub.
2. Click `Add`.
3. Select the folder:

```text
Part2/ActiveSaga-Game
```

4. Open the project with Unity `6000.3.13f1`.
5. Wait for Unity to import packages and assets.

### Important Scenes

The final build uses these main scenes:

```text
Assets/ActiveSaga/Scenes/Login.unity
Assets/ActiveSaga/Scenes/Main New.unity
Assets/ActiveSaga/Scenes/Run Game.unity
Assets/ActiveSaga/Scenes/Fight Game.unity
```

Make sure these scenes are included and enabled in Unity Build Settings.

## API Configuration in Unity

The Unity client communicates with the backend using API service scripts.

Important scripts to check when changing the backend URL:

```text
PlayerApiService
ApiGameResultSubmitter
```

For the final APK version, the Unity API URL should point to the deployed Render backend:

```text
https://active-saga-api.onrender.com/api/player
```

For local Unity Editor testing with a local backend:

```text
http://localhost:3000/api/player
```

For testing on the Meta Quest headset while the backend runs locally on the development computer, do not use `localhost`.

Use the computer local network IP address instead:

```text
http://192.168.x.x:3000/api/player
```

Example:

```text
http://192.168.1.25:3000/api/player
```

Why this matters:

- In the Unity Editor, `localhost` refers to the development computer.
- On the Meta Quest headset, `localhost` refers to the headset itself.
- Therefore, a Quest APK cannot reach a backend running on the computer through `localhost`.

## Running the Full Project Locally

### Option 1 - Local Backend + Unity Editor

Use this option during development.

1. Start the backend:

```bash
cd Part2/ActiveSaga-API
npm install
npm start
```

2. Confirm the backend is running:

```text
http://localhost:3000/health
```

3. Open Unity:

```text
Part2/ActiveSaga-Game
```

4. Make sure the Unity API URL points to:

```text
http://localhost:3000/api/player
```

5. Run the project from the Login scene or build it to the headset.

### Option 2 - Deployed Backend + Meta Quest APK

Use this option for the final standalone version.

1. Make sure the Unity API URL points to:

```text
https://active-saga-api.onrender.com/api/player
```

2. Build the Unity project as an Android APK.
3. Install the APK on the Meta Quest headset.
4. Launch ActiveSaga directly from the headset.
5. Register or log in.
6. Play either the Run Game or Fight Game.
7. Review the results screen.
8. Confirm that progress is saved after the session.

## Building an APK for Meta Quest

1. Open the Unity project.
2. Go to `File > Build Profiles` or `File > Build Settings`.
3. Select Android / Meta Quest as the target platform.
4. Make sure Android Build Support is installed.
5. Make sure the required scenes are enabled:
   - Login
   - Main New
   - Run Game
   - Fight Game
6. Confirm that the backend API URL is correct.
7. Build the project as an APK.
8. Install the APK on the Meta Quest headset.

Optional ADB installation command:

```bash
adb install ActiveSaga.apk
```

If replacing an existing installed version:

```bash
adb install -r ActiveSaga.apk
```

## Running the Game on Meta Quest

1. Put on the Meta Quest headset.
2. Make sure the headset is connected to the internet.
3. Launch ActiveSaga.
4. Register a new account or log in with an existing account.
5. Review the main menu, missions, and weekly progress.
6. Select a game mode:
   - Run Game
   - Fight Game
7. Select a difficulty level:
   - Easy
   - Medium
   - Hard
8. Perform the required physical movements during gameplay.
9. Finish the session and review the results.
10. Return to the main menu and verify that progress was updated.

## Safety Notes

ActiveSaga requires physical movement.

Before playing:

- Clear enough space around the player.
- Make sure there are no obstacles nearby.
- Use the Meta Quest guardian/boundary system.
- Avoid long sessions without breaks.
- Stop playing if the user feels discomfort.

ActiveSaga is a fitness-oriented game, but it is not a medical or personalized training application.

## Database Notes

MongoDB stores:

- User accounts
- Hashed passwords
- Player profiles
- XP
- Coins
- Levels
- Total play time
- Total distance
- Daily missions
- Weekly activity progress
- Game session statistics

The Unity client does not access MongoDB directly.  
All database operations are handled through the backend API.

## Deployment Notes

The backend can be deployed to Render.

For Render deployment:

1. Connect the backend repository/folder to Render.
2. Set the start command:

```bash
npm start
```

3. Add environment variables in Render:
   - `MONGO_URI`
   - `JWT_SECRET`
   - `PORT` if needed
4. Deploy the service.
5. Check the Render logs.
6. Test:

```text
https://active-saga-api.onrender.com/health
```

7. Update the Unity API URL if the deployed backend URL changes.
8. Rebuild the APK after changing the API URL.

## Troubleshooting

### Backend does not start

Check:

- `.env` file exists.
- `MONGO_URI` is defined.
- `JWT_SECRET` is defined.
- MongoDB connection string is valid.
- Dependencies were installed with `npm install`.

### Unity cannot connect to the backend

Check:

- Backend is running.
- API URL in Unity is correct.
- The headset has internet connection.
- If testing on Quest with a local backend, use the computer IP address instead of `localhost`.
- Render service is awake and running.

### Login or registration fails

Check:

- Backend `/health` route.
- MongoDB connection.
- API URL in Unity.
- Request payload fields.
- Render logs or local backend terminal output.

### Progress is not saved

Check:

- User is logged in.
- JWT token is sent in the `Authorization` header.
- Session result is sent to `/api/player/complete-game-session`.
- Backend receives the request successfully.
- MongoDB updates the player profile.

### Buttons do not respond in VR

Check:

- Controllers are connected.
- VR ray interaction is configured.
- Canvas interaction settings are correct.
- Button interaction areas are large enough.
- The screen was tested inside the headset, not only in the Unity Editor.

### Movement detection feels inaccurate

Check:

- Height calibration.
- Player standing position.
- Running/jumping/squatting thresholds.
- Testing on the actual Meta Quest headset.
- Possible false detections between squatting and jumping.

## What Not to Commit

Do not commit:

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

APK files, secrets, database credentials, and Unity temporary folders should not be pushed to GitHub.

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
