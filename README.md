# Modern Full-Stack Tic Tac Toe 🎮

A full-stack, browser-based Tic Tac Toe application built with an **Angular** frontend and a **.NET 8 Web API** backend with **SQLite persistence via EF Core**. Supports two-player local play, an intelligent computer opponent with rule-based heuristics, multi-level move undo, live move history tracking, and a session-level scoreboard.

---

## 📋 Table of Contents
- [Project Overview](#project-overview)
- [Tech Stack](#tech-stack)
- [Architecture & Design Decisions](#architecture--design-decisions)
- [Features Implemented](#features-implemented)
- [API Contract & Documentation](#api-contract--documentation)
- [How to Run Locally](#how-to-run-locally)
  - [Prerequisites](#prerequisites)
  - [Backend Setup (.NET 8)](#backend-setup-net-8)
  - [Frontend Setup (Angular)](#frontend-setup-angular)
- [Running Tests](#running-tests)
  - [Backend Tests (xUnit)](#backend-tests-xunit)
  - [Frontend Tests (Vitest)](#frontend-tests-vitest)
- [Computer Opponent AI Strategy](#computer-opponent-ai-strategy)
- [Clarifications & Assumptions](#clarifications--assumptions)
- [AI Workflow & Prompt Summary](#ai-workflow--prompt-summary)
- [Known Limitations & Future Improvements](#known-limitations--future-improvements)

---

## 🌟 Project Overview

This solution demonstrates pair programming and full-stack software engineering practices:
- **True Backend State Ownership**: The .NET Web API is the authoritative single source of truth for the board, move history, turn sequencing, win/draw detection, and scoreboard. The Angular frontend reacts to and renders server state.
- **Persistent SQLite Storage**: Uses Entity Framework Core with an SQLite database file (`tictactoe.db`), automatically created and seeded on startup.
- **Intelligent Heuristic AI**: In Computer Mode, the computer player (O) evaluates winning moves, defensive blocking, center control, and corner dominance.
- **Smart Undo**: Mode-aware undo functionality (single move for Two Player; move pair for Computer Mode) with automatic scoreboard adjustment.
- **Modern Glassmorphic UI**: Built with custom CSS design tokens, responsive layout, smooth micro-animations, color-coded player marks (Indigo for X, Rose for O), and accessible elements.

---

## 🛠 Tech Stack

| Layer | Technology | Details |
|---|---|---|
| **Frontend** | Angular 19 + TypeScript | Standalone components, signals, Angular control flow (`@for`, `@if`), CSS variables |
| **Backend** | .NET 8 Web API (C#) | Minimal dependency injection, Controller pattern, RESTful endpoints |
| **ORM / Storage** | Entity Framework Core 8 | SQLite database (`Microsoft.EntityFrameworkCore.Sqlite`) |
| **API Docs** | Swagger / OpenAPI | Interactive UI at `http://localhost:5000/swagger` |
| **Testing** | xUnit, FluentAssertions, Vitest | 30 backend unit tests + 9 frontend unit tests |

---

## 🏛 Architecture & Design Decisions

### 1. Backend as Single Source of Truth
To prevent desynchronization, client-side cheating, or rule discrepancies, all state transitions (move validation, win/draw checking, computer moves, and undo operations) are executed synchronously on the backend. The frontend sends user intents (`POST /moves`, `POST /undo`, `POST /reset`) and renders the returned `GameResponse`.

### 2. SQLite Database Schema
The database uses EF Core with SQLite:
- **`Games` table**: Stores `Id` (GUID), `Board` (9-char string `"---------"` representing rows 0-2 and cols 0-2), `CurrentPlayer`, `GameMode`, `Status` (`InProgress`, `Won`, `Draw`), `Winner`, `WinningCells`, and `CreatedAt`.
- **`Moves` table**: Relational child table with `MoveNumber`, `Player`, `Row`, `Col`, and foreign key `GameId` (cascade delete).
- **`Scoreboards` table**: Singleton record (`Id = 1`) tracking `XWins`, `OWins`, and `Draws`.

### 3. Undo Strategy: Mode-Specific Completion Rules
- **Two Player Mode (Option A after completion)**: Once a game is won or drawn in Two Player mode, Undo is disabled. The result is final and the scoreboard remains unchanged.
- **Computer Mode (Option B after completion)**: In Computer Mode, Undo is allowed even after game completion. If the user undos, both the computer's last move and the player's winning/finishing move are reverted, the game status returns to `InProgress`, and the scoreboard count is automatically decremented.

### 4. Computer Move Mechanics
When in Computer Mode:
1. Human submits move as `X`.
2. The server records X's move and evaluates game state.
3. If the game is still `InProgress`, the server automatically calculates and applies the computer's move as `O` before returning the response.
4. The client receives the complete updated state in a single atomic HTTP roundtrip.

---

## ✨ Features Implemented

1. **Interactive 3x3 Game Board**
   - Clickable empty cells with immediate tactile feedback.
   - Locked once clicked or when game finishes.
   - Neon glow color schemes for X (Indigo `#818cf8`) and O (Rose `#fb7185`).

2. **Player Turns & Turn Indicator**
   - Clear visual status banner showing whose turn it is.
   - Alternates automatically on valid moves.
   - Invalid move protection (occupied cells, wrong turn, out of bounds).

3. **Win & Draw Detection**
   - Real-time row, column, and diagonal evaluation.
   - Winning cells pulse with glowing green border animation.
   - Game completion prevents any subsequent cell clicks.

4. **Session-Level Scoreboard**
   - Tracks X Wins, O Wins, and Draws persisted in SQLite.
   - Increments once per completed game.
   - Independent "Reset Scoreboard" button to reset stats without affecting current board.

5. **Move History Table**
   - Chronological move list with Move #, Player badge, and human-friendly position (`Row 1, Column 1`).
   - Smooth entry animations as moves are registered.

6. **Smart Undo Last Move**
   - **Two Player Mode**: Reverts the single most recent move and restores the previous player's turn.
   - **Computer Mode**: Reverts both the computer's move and the user's preceding move together, giving the human their turn back immediately.
   - Disabled when no moves exist to undo.

7. **Reset Game**
   - Clears board and move history.
   - Resets winner/draw status and sets current player back to X.
   - Preserves scoreboard statistics.

8. **Game Mode Toggle**
   - Seamlessly switch between **Two Player Mode** and **Play Against Computer**.

9. **Past Games History Section**
   - Expandable section beneath the board displaying match history from SQLite.
   - Shows timestamp, mode badge (2-Player or AI), match outcome badge, and total move count.
   - "Inspect" button to load and review the completed board of any previous match, with a "Back to Live Game" button to resume.
   - Live "Refresh" button to fetch recent games from the database on demand.

---

## 📡 API Contract & Documentation

Interactive Swagger documentation is available at `http://localhost:5000/swagger`.

### Endpoints Summary

| Method | Endpoint | Request Body | Description |
|---|---|---|---|
| `GET` | `/api/games` | *None* | Retrieves list of past game sessions (history) |
| `POST` | `/api/games` | `{"gameMode": "TwoPlayer" \| "Computer"}` | Creates a new game session |
| `GET` | `/api/games/{id}` | *None* | Retrieves current game state |
| `POST` | `/api/games/{id}/moves` | `{"player": "X", "row": 0, "col": 0}` | Submits a player move (triggers AI if Computer mode) |
| `POST` | `/api/games/{id}/undo` | *None* | Undoes the last move (or pair in Computer mode) |
| `POST` | `/api/games/{id}/reset` | *None* | Resets the game board and move history |
| `GET` | `/api/scoreboard` | *None* | Retrieves session scoreboard |
| `POST` | `/api/scoreboard/reset` | *None* | Resets session scoreboard to all zeroes |

### Sample Game Response
```json
{
  "id": "a27b5f8f-b3f9-473e-badf-c149cde97e81",
  "board": [
    ["X", "", ""],
    ["", "O", ""],
    ["", "", ""]
  ],
  "currentPlayer": "X",
  "gameMode": "Computer",
  "status": "InProgress",
  "winner": null,
  "winningCells": null,
  "moveHistory": [
    { "moveNumber": 1, "player": "X", "row": 0, "col": 0 },
    { "moveNumber": 2, "player": "O", "row": 1, "col": 1 }
  ],
  "scoreboard": {
    "xWins": 0,
    "oWins": 0,
    "draws": 0
  }
}
```

---

## 🚀 How to Run Locally

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js (v18+) & npm](https://nodejs.org/)

---

### Backend Setup (.NET 8)

1. Open a terminal and navigate to the backend directory:
   ```bash
   cd backend
   ```
2. Restore NuGet dependencies and build:
   ```bash
   dotnet restore
   dotnet build
   ```
3. Run the API:
   ```bash
   dotnet run --project TicTacToe.Api/TicTacToe.Api.csproj
   ```
4. The server will start on **`http://localhost:5000`**.  
   Swagger UI is accessible at: **`http://localhost:5000/swagger`**.

---

### Frontend Setup (Angular)

1. Open a second terminal and navigate to the frontend directory:
   ```bash
   cd frontend
   ```
2. Install npm dependencies:
   ```bash
   npm install
   ```
3. Start the Angular dev server:
   ```bash
   npm start
   ```
4. Open your browser and navigate to: **`http://localhost:4200`**.

---

## 🧪 Running Tests

### Backend Tests (xUnit)
The backend test suite (`TicTacToe.Tests`) includes **30 comprehensive unit tests** covering:
- Valid and invalid move validation
- Out-of-bounds coordinates
- Turn alternations
- Row, column, and diagonal wins
- Draw detection
- Game reset and scoreboard retention
- Mode-aware undo (Two Player vs Computer mode)
- Scoreboard increment on win/draw and decrement on undo
- Computer AI move selection priorities

Run backend tests:
```bash
cd backend
dotnet test --verbosity normal
```

### Frontend Tests (Vitest)
The frontend test suite (`app.spec.ts`) verifies:
- Component rendering and structure
- Initial game load in Two Player mode
- Turn message formatting
- Move click event emission to API
- Undo, Reset Game, and Reset Scoreboard triggers
- Mode switching to Computer mode

Run frontend tests:
```bash
cd frontend
npm test -- --watch=false
```

---

## 🧠 Computer Opponent AI Strategy

The computer player (`O`) selects moves using the priority rules outlined in the requirements:
1. **Winning Move**: If `O` has two in any row, column, or diagonal, take the winning cell.
2. **Defensive Block**: If `X` has two in any line and can win next turn, block `X`.
3. **Center Cell**: Take the center `(1, 1)` if unoccupied.
4. **Corner Cells**: Take any available corner `(0, 0)`, `(0, 2)`, `(2, 0)`, or `(2, 2)`.
5. **Any Available Cell**: Take the first open cell.

---

## 🔍 Clarifications & Assumptions

1. **Storage Choice**: SQLite via Entity Framework Core was chosen instead of in-memory storage to ensure durability across server restarts. The database file `tictactoe.db` is stored locally in the backend directory.
2. **Undo After Completion**: In Two Player Mode, Undo is disabled after game completion (Option A). In Computer Mode, Undo is permitted after game completion (Option B), reversing the final moves and decrementing the scoreboard.
3. **Session vs Persistent Scoreboard**: The scoreboard is maintained in SQLite as a singleton entity. It persists across games and can be reset at any time via the "Reset Scoreboard" button.
4. **Computer Mode Human Mark**: In Computer Mode, the human is always Player `X` (moves first), and the computer is Player `O`.

---

## 🤖 AI Workflow & Prompt Summary

### Conversion of Requirements into Specification
1. Analyzed functional requirements and mapped them into discrete domain models (`Game`, `Move`, `Scoreboard`).
2. Drafted a contract-first REST API specification matching the required endpoints.
3. Determined that game logic and state transitions should be encapsulated in `GameService` for high unit-test coverage.

### Prompts & AI Assistance Used
- Architecture scaffolding: Scaffolded standard .NET 8 Web API project structure with EF Core SQLite and xUnit test harness.
- AI Logic Implementation: Generated the heuristic priority tree for the computer opponent and winning pattern matrix.
- Modern CSS Design: Designed a dark glassmorphic UI using CSS custom properties, neon box-shadows, and micro-interactions without relying on heavy CSS frameworks.
- Automated Testing: Generated comprehensive unit tests for both backend (xUnit) and frontend (Vitest) ensuring 100% pass rates.

### Manual Reviews & Refinements
- **Angular 19 compatibility**: Adapted tests to Vitest runner (`vi.fn()`) matching Angular 19+ project defaults.
- **Port synchronization**: Explicitly configured Kestrel and launch profiles to bind to port 5000 and configured Angular CORS policy.
- **End-to-end browser verification**: Tested the complete gameplay loop using a browser subagent and confirmed visual correctness.

---

## 🔮 Known Limitations & Future Improvements

- **Difficulty Levels**: Currently, the computer opponent uses a fixed heuristic. Future improvements could include configurable difficulty (Easy / Medium / Unbeatable Minimax).
- **Multiplayer Online**: Introduce WebSockets / SignalR for real-time multiplayer over the network instead of local hot-seat play.
- **Sound Effects**: Add optional Web Audio API sound effects for cell placement, undo, victory, and draw.
- **Custom Player Names**: Allow players to customize display names for X and O.
