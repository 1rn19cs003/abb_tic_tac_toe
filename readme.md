# Tic Tac Toe - Full-Stack Senior Software Engineer Assessment

A production-grade, browser-based Tic Tac Toe application built with an **Angular** frontend and a **.NET 8 Web API** backend.

---

## 📋 Table of Contents
1. [Project Overview](#project-overview)
2. [Technology Stack](#technology-stack)
3. [Architecture & Design Decisions](#architecture--design-decisions)
4. [Features Implemented](#features-implemented)
5. [API Endpoint Contract](#api-endpoint-contract)
6. [Local Setup & Run Instructions](#local-setup--run-instructions)
7. [Running Tests](#running-tests)
8. [AI-Assisted Workflow & Prompts Summary](#ai-assisted-workflow--prompts-summary)
9. [Clarifications, Assumptions & Trade-offs](#clarifications-assumptions--trade-offs)
10. [Known Limitations & Future Improvements](#known-limitations--future-improvements)

---

## 1. Project Overview

This project implements a full-featured Tic Tac Toe game designed to showcase Senior Software Engineering principles:
- **Backend-Driven Single Source of Truth**: All game state transitions, rule evaluations, win/draw detections, and move histories are strictly computed and owned by the .NET Web API.
- **Modern Layered Architecture**: Clean separation between presentation (`Controllers`), business logic (`Services`), domain modeling (`Models`), data access (`Repositories`), and AI decision strategies (`Strategies`).
- **Concurrent & Thread-Safe Design**: Thread-safe in-memory session and scoreboard management utilizing `ConcurrentDictionary` and `Interlocked` primitives.
- **Mode-Specific Undo & Scoreboard Semantics**: Implements required dual-mode undo mechanics (single-move undo for two-player; atomic dual-move undo for computer mode).
- **Rigorous Test Coverage**: 28 automated tests (23 xUnit backend tests covering rules, edge cases, and heuristics + 5 Jasmine/Karma frontend component tests).

---

## 2. Technology Stack

- **Backend**:
  - **Framework**: .NET 8.0 (C# 12) ASP.NET Core Web API
  - **Documentation**: Swagger / OpenAPI (Swashbuckle)
  - **Testing**: xUnit with fluent assertions
  - **Concurrency**: Thread-safe `ConcurrentDictionary` & atomic `Interlocked` counters
- **Frontend**:
  - **Framework**: Angular 16+ / TypeScript
  - **Styling**: Pure responsive CSS3 with CSS variables and keyframe animations
  - **HTTP**: Angular `HttpClient` reactive streams
  - **Testing**: Karma + Jasmine (Chrome Headless)
- **Version Control**: Git feature branching with descriptive semantic commit history.

---

## 3. Architecture & Design Decisions

### A. Repository & Directory Structure
```
abb-tic-tac-toe/
├── BE/
│   ├── TicTacToe.Api/            # ASP.NET Core 8 Web API
│   │   ├── Controllers/          # REST API endpoints (GamesController, ScoreboardController)
│   │   ├── Models/
│   │   │   ├── Domain/           # GameSession, Scoreboard, MoveRecord, CellPosition, Exception
│   │   │   ├── DTOs/             # Requests & Responses (GameStateResponse, MakeMoveRequest, etc.)
│   │   │   └── Enums/            # GameMode, GameStatus, PlayerType
│   │   ├── Repositories/         # IGameRepository & InMemoryGameRepository
│   │   ├── Services/             # IGameService, GameService, IAiStrategy, AiStrategy, ScoreboardService
│   │   └── Program.cs            # Dependency Injection, CORS, Swagger Configuration
│   ├── TicTacToe.Tests/          # xUnit Test Suite
│   │   ├── GameServiceTests.cs   # 16 domain rule & state transition unit tests
│   │   └── AiStrategyTests.cs    # 7 heuristic priority & boundary tests
│   └── TicTacToe.sln             # Visual Studio / .NET Solution
│
└── FE/
    ├── src/
    │   ├── app/
    │   │   ├── models/           # TypeScript interfaces matching backend DTOs
    │   │   ├── services/         # GameService HTTP REST client
    │   │   ├── app.component.ts  # Main reactive game controller
    │   │   ├── app.component.html# Accessible game board, scoreboard & history layout
    │   │   ├── app.component.css # Responsive modern styling & winning cell glow
    │   │   └── app.component.spec.ts # Angular Karma unit tests
    │   └── environments/         # API endpoint configuration
    └── package.json              # Angular dependencies & scripts
```

### B. Single Source of Truth
The Angular frontend never calculates game rules, winners, or AI moves client-side. Every user click dispatches an HTTP request to the backend, which validates the move against the stored session, evaluates state transitions, updates history/scoreboard, and returns an authoritative `GameStateResponse`.

### C. Undo & Scoreboard Policy (Option A Selected)
In accordance with the assessment clarification options:
- **Option A Chosen**: Once a game is completed (`Won` or `Draw`), the `Undo` action is disabled. The game result and scoreboard remain immutable and final for that match.
- **Rationale**: Eliminates scoreboard rollback ambiguity, prevents competitive exploitation, and provides a clear, deterministic state machine.

### D. AI Heuristic Hierarchy (5-Step Strict Priority)
When playing in **Computer Mode** (`AgainstComputer`), the backend `AiStrategy` evaluates moves using the exact priority hierarchy mandated by the specification:
1. **Winning Move**: If `O` can win immediately, play the winning cell.
2. **Block Opponent**: If `X` can win on the next move, block `X`.
3. **Center**: Take center `(1, 1)` if available.
4. **Corner**: Take any unoccupied corner (`(0,0)`, `(0,2)`, `(2,0)`, `(2,2)`).
5. **Fallback**: Take any remaining available cell.

---

## 4. Features Implemented

1. **3 × 3 Interactive Grid**:
   - Empty cells are clickable; occupied cells are locked.
   - Distinct, vibrant styling for Player X (Cyan/Blue) and Player O (Coral/Red).
   - Real-time winning line highlight with glow animation.
2. **Turn Management & Indicators**:
   - Alternates between Player X and Player O.
   - Shows "Computer is calculating move..." during AI processing.
   - Invalid moves are rejected with descriptive messages and without turn loss.
3. **Session-Level Scoreboard**:
   - Tracks `X Wins`, `O Wins`, and `Draws`.
   - Increments exactly once per completed game.
   - Preserved across "Reset Game".
   - Separate "Reset Scoreboard" action to reset tallies to zero.
4. **Move History Ledger**:
   - Displays real-time move number, player (`X`/`O`), and board position (e.g., `Row 1, Column 1`).
   - Highlights the latest move.
5. **Mode-Specific Undo Mechanics**:
   - **Two Player Mode**: Undoes the single most recent move and restores that player's turn.
   - **Computer Mode**: Atomically undoes both the computer's move (`O`) and human's preceding move (`X`), returning turn to Player X.
   - Automatically disabled when no moves are available or when game is completed.
6. **Dual Game Modes**:
   - **Two Player Mode**: Two users playing locally.
   - **Play Against Computer**: Single-player vs backend AI heuristic engine.

---

## 5. API Endpoint Contract

Base URL: `http://localhost:5000/api`

| Method | Endpoint | Description | Request Body | Response |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/games` | Initializes a new game session | `{ "mode": "TwoPlayer" \| "AgainstComputer" }` | `GameStateResponse` (201 Created) |
| `GET` | `/api/games/{id}` | Fetches current game state | *None* | `GameStateResponse` (200 OK) |
| `POST` | `/api/games/{id}/moves` | Submits a player move | `{ "player": "X" \| "O", "row": 0..2, "col": 0..2 }` | `GameStateResponse` (200 OK) |
| `POST` | `/api/games/{id}/undo` | Undoes move(s) based on mode | *None* | `GameStateResponse` (200 OK) |
| `POST` | `/api/games/{id}/reset` | Resets board & moves for session | *None* | `GameStateResponse` (200 OK) |
| `GET` | `/api/scoreboard` | Retrieves current session scoreboard | *None* | `ScoreboardDto` (200 OK) |
| `POST` | `/api/scoreboard/reset` | Resets scoreboard tallies to 0 | *None* | `ScoreboardDto` (200 OK) |

### Sample GameStateResponse JSON:
```json
{
  "gameId": "c8a49301-443b-486a-8b18-b0a3f81e3271",
  "board": [
    ["X", "O", null],
    [null, "X", null],
    [null, null, null]
  ],
  "currentPlayer": "O",
  "gameMode": "TwoPlayer",
  "gameStatus": "InProgress",
  "winner": null,
  "winningCells": [],
  "moveHistory": [
    {
      "moveNumber": 1,
      "player": "X",
      "row": 0,
      "col": 0,
      "position": "Row 1, Column 1",
      "timestamp": "2026-10-24T18:40:00Z"
    },
    {
      "moveNumber": 2,
      "player": "O",
      "row": 0,
      "col": 1,
      "position": "Row 1, Column 2",
      "timestamp": "2026-10-24T18:40:02Z"
    },
    {
      "moveNumber": 3,
      "player": "X",
      "row": 1,
      "col": 1,
      "position": "Row 2, Column 2",
      "timestamp": "2026-10-24T18:40:05Z"
    }
  ],
  "scoreboard": {
    "xWins": 2,
    "oWins": 1,
    "draws": 0
  },
  "canUndo": true,
  "message": "Player O's turn"
}
```

---

## 6. Local Setup & Run Instructions

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js (v18+ or v20+)](https://nodejs.org/) & npm

### Running the Backend (.NET Web API)
```bash
# Navigate to the backend directory
cd BE/TicTacToe.Api

# Restore and run the API
dotnet run
```
- API will start on: `http://localhost:5000`
- Interactive Swagger UI: `http://localhost:5000/swagger`

### Running the Frontend (Angular)
```bash
# Navigate to the frontend directory
cd FE

# Install dependencies (if not already installed)
npm install

# Start the Angular development server
npm start
```
- Open browser at: `http://localhost:4200`

---

## 7. Running Tests

### Backend Unit Tests (xUnit)
```bash
cd BE
dotnet test
```
**Results Summary**:
- **Total Tests**: 23
- **Status**: Passed (0 Failed, 0 Skipped)
- **Coverage**:
  - Valid move & board updates
  - Out of bounds move rejection (400)
  - Occupied cell rejection (400)
  - Wrong player turn rejection (400)
  - Move after game completion rejection (400)
  - Row wins (Rows 0, 1, 2)
  - Column wins (Cols 0, 1, 2)
  - Diagonal wins (Main and Anti-diagonals)
  - Draw detection after 9 moves with no winner
  - Two-Player Undo (single move removal + turn restore)
  - Computer Mode Undo (dual move removal + turn restore)
  - Undo rejection on empty history
  - Undo rejection on completed game (Option A)
  - Reset board preserves scoreboard
  - Scoreboard single increment guarantee
  - AI Heuristic Priority 1 (Win)
  - AI Heuristic Priority 2 (Block)
  - AI Heuristic Priority 3 (Center)
  - AI Heuristic Priority 4 (Corners)
  - AI Heuristic Priority 5 (Any free cell)

### Frontend Unit Tests (Karma / Jasmine)
```bash
cd FE
npm test -- --watch=false --browsers=ChromeHeadless
```
**Results Summary**:
- **Total Tests**: 5
- **Status**: Passed (0 Failed)
- **Coverage**: Component creation, mode toggling, move dispatch, undo triggers, and winning cell calculations.

---

## 8. AI-Assisted Workflow & Prompts Summary

Per the assessment guidelines on AI-assisted development transparency:

### Conversion of Requirements into Specification
- Decomposed the prompt into distinct domain abstractions:
  1. Immutable domain models (`MoveRecord`, `CellPosition`).
  2. Thread-safe persistence abstraction (`IGameRepository` with `ConcurrentDictionary`).
  3. AI heuristic strategy isolation (`IAiStrategy`).
  4. Mode-aware undo state machine.

### Key Prompts & Iterations
1. **Domain & API Architecture**:
   - *Prompt*: "Generate a .NET 8 Web API domain model and service for Tic Tac Toe with thread-safe in-memory storage, supporting TwoPlayer and Computer modes, with validation for occupied cells, out-of-bounds, out-of-turn play, and game status."
2. **AI Priority Heuristics**:
   - *Prompt*: "Implement the AI move selection with strict precedence: 1. Win immediate line, 2. Block opponent line, 3. Center, 4. Any Corner, 5. Any open cell. Keep it purely functional and unit testable."
3. **Angular Reactive UI**:
   - *Prompt*: "Build an Angular component with clean CSS that presents a 3x3 grid, mode selector, move history table, scoreboard, and status banner, connecting to the backend via REST."

### Manual Refinements & Engineering Judgment
- **Provider Injection in Specs**: Corrected Jasmine test module provider syntax from `{ value: mockService }` to `{ useValue: mockService }`.
- **Budget Tuning**: Adjusted Angular production budget in `angular.json` for component styling to ensure production bundle generation succeeds without arbitrary style splitting.
- **Atomic Undo Logic**: Engineered mode-specific undo logic on the backend to guarantee that undoing in computer mode rolls back the human move and AI move atomically.

---

## 9. Clarifications, Assumptions & Trade-offs

1. **Option A for Completed Games**:
   - Once a game is marked `Won` or `Draw`, Undo is disabled. This guarantees scoreboard immutability and avoids complicated rollbacks.
2. **In-Memory Thread Safety**:
   - Used `ConcurrentDictionary<Guid, GameSession>` and `Interlocked` operations on the scoreboard to ensure the server remains resilient under concurrent calls without database overhead.
3. **First Move Advantage**:
   - Player `X` always makes the first move of any new game. In Computer Mode, the human is always `X` and the computer is always `O`.

---

## 10. Known Limitations & Future Improvements

1. **Persistent Database**:
   - The current repository stores sessions in server memory. An EF Core SQLite or PostgreSQL provider could be swapped in behind `IGameRepository` for production resilience across server restarts.
2. **Multiplayer via WebSockets (SignalR)**:
   - While REST APIs satisfy the take-home requirements cleanly, real-time multiplayer between two separate remote browsers can be enabled by introducing ASP.NET Core SignalR hubs.
3. **Player Customization**:
   - Allowing human players to choose symbols (`O` plays first) or configurable grid dimensions ($N \times N$).

---

*Production-ready Full-Stack Implementation for Senior Software Engineer Panel Review.*
