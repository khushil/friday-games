# OXS Requirements Specification

## Document Information

- **Version:** 1.0
- **Date:** 2026-01-16
- **Status:** Draft

---

## 1. Introduction

### 1.1 Purpose

This document specifies the software requirements for OXS, a noughts and crosses (tic-tac-toe) game developed as part of the Friday Games project. It defines the functional and non-functional requirements that will guide the design, implementation, and testing of the game.

### 1.2 Scope

OXS is a noughts and crosses game supporting three play modes:

1. **Local 2-Player** - Two players on the same device taking turns
2. **AI Opponent** - Single player against computer-controlled opponent
3. **Networked Multiplayer** - Two players over network connection

The game features variable board sizes (3x3, 4x4, 5x5), configurable win conditions, match play, and persistent statistics. It targets PC platforms (Windows, Linux, macOS) with a polished, animated visual style.

### 1.3 Definitions and Acronyms

| Term | Definition |
|------|------------|
| OXS | Project codename for the noughts and crosses game |
| N-in-a-row | Win condition where N consecutive symbols in a line wins |
| Match | A series of games (Best of 3, 5, or 7) |
| Round | A single game within a match |
| Host | The authoritative game instance in networked play |
| Client | The connecting player instance in networked play |
| FPS | Frames Per Second |
| ENet | Reliable UDP networking library used by Godot |

---

## 2. Overall Description

### 2.1 Product Perspective

OXS is a standalone game application built with Godot 4.5 and C#. It extends the classic noughts and crosses concept with:

- Variable board sizes beyond the traditional 3x3
- Configurable win conditions independent of board size
- Multiple AI difficulty levels with adaptive adjustment
- Network multiplayer via direct IP connection

### 2.2 User Classes and Characteristics

| User Class | Description | Technical Skill |
|------------|-------------|-----------------|
| Casual Player | Plays locally against AI or friend | Low |
| Competitive Player | Uses match play, tracks statistics | Low-Medium |
| Network Player | Connects to friends via IP address | Medium |

### 2.3 Operating Environment

- **Platforms:** Windows, Linux, macOS
- **Minimum Hardware:** GTX 750 / Intel HD 620, 8GB RAM (5-year-old mid-range PC)
- **Display:** Any resolution from 720p to 4K, any aspect ratio (4:3, 16:9, 16:10, 21:9)
- **Network:** TCP/IP for multiplayer (no NAT traversal required)

### 2.4 Assumptions and Dependencies

1. Players have a working keyboard and mouse/touchpad
2. Network players can exchange IP addresses out-of-band
3. Network players have stable internet connections with <200ms latency
4. Godot 4.5 export templates are available for target platforms

---

## 3. Functional Requirements

### 3.1 Core Gameplay (CORE)

| ID | Requirement | Priority | Acceptance Criteria |
|----|-------------|----------|---------------------|
| CORE-001 | The game shall support variable board sizes: 3x3, 4x4, and 5x5 | Must | Player can select board size from menu; game correctly renders selected grid |
| CORE-002 | The game shall allow configurable win conditions (N-in-a-row) independent of board size | Must | Player can select required line length (3, 4, or 5); game correctly detects wins for selected N |
| CORE-003 | The game shall detect and announce a win when a player places N symbols in a horizontal, vertical, or diagonal line | Must | Given board state with N symbols in line, game displays winner within 500ms |
| CORE-004 | The game shall detect and announce a draw when the board is full with no winner | Must | Given full board with no winner, game displays 'Draw' within 500ms |
| CORE-005 | The game shall offer two draw handling modes: half-point scoring or sudden death rematch | Should | Player can select draw mode; half-point awards 0.5 to each; sudden death continues until winner |
| CORE-006 | The game shall persist player statistics (wins, losses, draws) across sessions | Must | After closing and reopening game, previous statistics are displayed correctly |
| CORE-007 | The game shall support match play with Best of 3, 5, or 7 rounds | Must | Player selects match length; game tracks round wins; match ends when majority reached |
| CORE-008 | The game shall alternate starting player between rounds in match play | Should | In a 3-round match, starting player alternates X, O, X |

### 3.2 Local 2-Player (LOCAL)

| ID | Requirement | Priority | Acceptance Criteria |
|----|-------------|----------|---------------------|
| LOCAL-001 | The game shall support mouse/touch input for placing symbols by clicking/tapping cells | Must | Clicking empty cell places current player's symbol; occupied cells cannot be clicked |
| LOCAL-002 | The game shall support keyboard input with arrow keys for navigation and Enter to select | Must | Arrow keys move cell highlight; Enter places symbol in highlighted cell |
| LOCAL-003 | The game shall allow each player to choose their symbol colour from a palette | Should | Players can select from at least 8 colours; selections persist during session |
| LOCAL-004 | The game shall prompt players to enter their names at the start of a session | Should | Name entry screen appears; names display during gameplay and in results |
| LOCAL-005 | The game shall give the first move to the loser of the previous game | Must | After Game 1 loss, losing player moves first in Game 2; first game uses random selection |
| LOCAL-006 | Moves shall be final once placed with no undo functionality | Must | No undo button exists; placed symbols cannot be removed or moved |

### 3.3 AI Opponent (AI)

| ID | Requirement | Priority | Acceptance Criteria |
|----|-------------|----------|---------------------|
| AI-001 | The game shall provide three fixed AI difficulty levels: Easy, Medium, and Hard | Must | Difficulty selection available; each level exhibits distinct behaviour patterns |
| AI-002 | The Easy AI shall make random valid moves with minimal strategic consideration | Must | Over 50 games, Easy AI wins less than 20% against competent play |
| AI-003 | The Medium AI shall block opponent winning moves and take winning moves when available | Must | Medium AI never misses immediate win; always blocks when opponent has 2-in-a-row |
| AI-004 | The Hard AI shall play near-optimally but remain beatable with strategic play | Must | Hard AI wins majority of games; skilled players can win occasionally (5-15%) |
| AI-005 | The game shall provide an adaptive AI mode that adjusts difficulty based on player performance | Should | After 3+ consecutive wins, AI plays harder; after 3+ losses, AI plays easier |
| AI-006 | The AI shall have selectable personalities: Aggressive, Defensive, or Balanced | Should | Aggressive prefers attacks; Defensive prefers blocks; Balanced weighs both equally |
| AI-007 | The AI shall display a brief thinking delay of 0.5-1.0 seconds before each move | Should | Visual indicator shows AI is 'thinking'; move appears after delay |
| AI-008 | AI difficulty shall be locked once a game begins | Must | No difficulty change option available during active gameplay |

### 3.4 Networked Multiplayer (NET)

| ID | Requirement | Priority | Acceptance Criteria |
|----|-------------|----------|---------------------|
| NET-001 | The game shall support networked play via direct IP connection | Must | Player can host game; other player joins by entering host IP address |
| NET-002 | The game shall use a host-client architecture where the host is authoritative | Must | Game logic executes on host; client sends inputs and receives state updates |
| NET-003 | The game shall synchronise board state within 200ms of a move being made | Should | Network latency tests show state sync under 200ms on typical connections |
| NET-004 | The game shall pause and offer reconnection when a player disconnects | Must | Disconnection shows 'Waiting for opponent' with reconnect option |
| NET-005 | The game shall allow 60 seconds for reconnection before declaring forfeit | Must | Countdown timer visible; forfeit occurs automatically at 0 seconds |
| NET-006 | The waiting player shall be able to forfeit the disconnected opponent before timeout | Should | Forfeit button available during reconnection wait period |
| NET-007 | Networked games shall not include text chat functionality | Must | No chat UI present during networked gameplay |
| NET-008 | Networked games shall be limited to two players with no spectator support | Must | Only host and one client can connect; additional connections rejected |

### 3.5 User Interface (UI)

| ID | Requirement | Priority | Acceptance Criteria |
|----|-------------|----------|---------------------|
| UI-001 | The game shall target PC platforms: Windows, Linux, and macOS | Must | Game exports and runs correctly on all three desktop platforms |
| UI-002 | The game shall feature a polished animated visual style with smooth transitions | Must | Symbol placement, win line, and screen transitions include animations |
| UI-003 | The game shall include particle effects for win celebrations and key moments | Should | Winning move triggers visible particle effect; celebrations appear on match win |
| UI-004 | The game shall include sound effects for moves, wins, losses, and UI interactions | Must | Distinct sounds for symbol placement, game end, and button clicks |
| UI-005 | The game shall not include background music | Must | No music plays during gameplay or menus |
| UI-006 | The game shall use a colour-blind friendly palette with sufficient contrast | Must | Colours pass WCAG AA contrast requirements; symbols distinguishable without colour alone |
| UI-007 | The game UI shall scale appropriately for different display sizes | Must | UI remains usable from 720p to 4K; no elements clip or overlap |
| UI-008 | The game shall support any window size and aspect ratio | Must | Game renders correctly in 4:3, 16:9, 16:10, and 21:9 aspect ratios |

---

## 4. Non-Functional Requirements

| ID | Category | Requirement | Target | Measurement |
|----|----------|-------------|--------|-------------|
| NFR-001 | Performance | The game shall maintain a minimum of 60 FPS during gameplay | 60 FPS | FPS counter shows no sustained drops below 60 FPS |
| NFR-002 | Performance | The game shall load to the main menu within 10 seconds | < 10 seconds | Timed from executable launch to interactive main menu |
| NFR-003 | Reliability | The game shall handle unexpected errors gracefully without crashing | Zero crashes during normal operation | No crashes during 100 consecutive games |
| NFR-004 | Privacy | The game shall not collect any user data or telemetry | Zero data transmission | Network monitoring shows no analytics calls |
| NFR-005 | Localisation | The game shall be English-only with no localisation support | English text only | All UI text appears in English |
| NFR-006 | Persistence | The game shall not support save/resume for incomplete games | No game save feature | No save button or auto-save behaviour present |
| NFR-007 | Network | Network latency should not exceed 200ms for acceptable gameplay | < 200ms round-trip | Ping tests show acceptable play at 200ms latency |

---

## 5. Technical Constraints

| ID | Constraint | Priority | Acceptance Criteria | Rationale |
|----|------------|----------|---------------------|-----------|
| CON-001 | The game shall be developed using Godot 4.5 game engine | Must | Project uses Godot 4.5 or compatible version | Project technical constraint |
| CON-002 | The game shall be developed using C# with .NET 8 | Must | All game logic implemented in C# targeting .NET 8 | Project technical constraint |
| CON-003 | Code shall follow the STD-0002 C# code style rubric | Must | Code review confirms adherence to STD-0002 | Project code quality standard |
| CON-004 | Network multiplayer shall use Godot's built-in high-level multiplayer API | Must | No third-party networking libraries; uses ENet via Godot API | Simplicity and Godot ecosystem alignment |
| CON-005 | The game shall target mid-range hardware (5-year-old PC equivalent) | Should | Runs at 60 FPS on GTX 750 / Intel HD 620 with 8GB RAM | Balance between visual quality and accessibility |

---

## 6. Appendices

### A. Glossary

| Term | Definition |
|------|------------|
| Noughts and Crosses | British name for tic-tac-toe |
| Minimax | Algorithm for optimal play in two-player zero-sum games |
| Adaptive AI | AI that adjusts difficulty based on player performance |
| Host-client | Network architecture where one machine is authoritative |
| ENet | Reliable UDP library providing connection-based communication |
| WCAG | Web Content Accessibility Guidelines |

### B. References

| Document | Description |
|----------|-------------|
| STD-0001 | Prompt Creation Rubric |
| STD-0002 | C# Code Style Rubric for Godot 4.5 |
| Godot Docs | Godot 4.5 Official Documentation |

### C. Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 0.1 | 2026-01-16 | Requirements Engineer | Initial draft |
| 1.0 | 2026-01-16 | Requirements Engineer | Complete requirements after elicitation |

---

## Summary

This specification defines **50 requirements** across 7 categories:

| Category | Count | Must | Should | Could |
|----------|-------|------|--------|-------|
| Core Gameplay (CORE) | 8 | 6 | 2 | 0 |
| Local 2-Player (LOCAL) | 6 | 4 | 2 | 0 |
| AI Opponent (AI) | 8 | 4 | 4 | 0 |
| Networked Multiplayer (NET) | 8 | 6 | 2 | 0 |
| User Interface (UI) | 8 | 6 | 2 | 0 |
| Non-Functional (NFR) | 7 | 7 | 0 | 0 |
| Technical Constraints (CON) | 5 | 4 | 1 | 0 |
| **Total** | **50** | **37** | **13** | **0** |

All requirements have testable acceptance criteria and follow the MoSCoW prioritisation scheme.
