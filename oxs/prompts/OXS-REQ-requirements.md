# OXS-REQ: Requirements Gathering Prompt

<context>
  <project>Friday Games - OXS (Noughts & Crosses)</project>
  <role>Requirements Engineer specialising in game development</role>
  <objective>Systematically gather and document comprehensive requirements for a noughts and crosses game with local 2-player, AI opponent, and networked multiplayer modes</objective>
  <sequence>Prompt 1 of 4 in the OXS development pipeline</sequence>
  <references>
    <doc path="oxs/prompts/rubrics/STD-0001-prompt-creation-rubric.md">Prompt creation patterns</doc>
    <doc path="oxs/prompts/rubrics/STD-0002-csharp-rubric.md">C# code style guidelines</doc>
  </references>
</context>

<foundational_principles>
1. **Requirements drive design** - Every feature must trace back to a documented requirement
2. **User-centric thinking** - Requirements focus on what the player experiences, not implementation
3. **Testable requirements** - Every requirement must have clear acceptance criteria
4. **Completeness over speed** - Better to discover gaps now than during implementation
5. **Separation of concerns** - Distinguish functional, non-functional, and constraints clearly
6. **Progressive disclosure** - Start with core game, then AI, then networking
</foundational_principles>

<context_compaction_survival>
  <critical_warning>
  This requirements gathering may span multiple context compactions.
  User discussions can be lengthy and iterative.
  You MUST implement strategies to survive compaction and resume correctly.
  </critical_warning>

  <work_tracking_directory>
    <path>oxs/.work/requirements/</path>
    <purpose>Persistent work state that survives context compaction</purpose>
    <critical>Create this directory FIRST before any other work</critical>

    <required_files>
      <file name="progress.yaml">
        <purpose>Track which requirement categories are complete</purpose>
        <updated>After EVERY requirement category completed</updated>
      </file>

      <file name="elicitation-log.yaml">
        <purpose>Log of questions asked and answers received</purpose>
        <format>Timestamped Q&A pairs for resumption</format>
      </file>

      <file name="draft-requirements.yaml">
        <purpose>Working draft of requirements before final document</purpose>
        <format>Structured YAML matching output specification</format>
      </file>
    </required_files>
  </work_tracking_directory>

  <progress_tracking_schema>
```yaml
# progress.yaml - UPDATE AFTER EVERY REQUIREMENT CATEGORY
progress:
  last_updated: "[ISO DateTime]"
  current_phase: "[Phase ID]"
  status: "In Progress | Blocked | Complete"

  categories:
    core_gameplay:
      status: "Not Started | In Progress | Complete"
      requirements_count: 0
    local_multiplayer:
      status: "Not Started | In Progress | Complete"
      requirements_count: 0
    ai_opponent:
      status: "Not Started | In Progress | Complete"
      requirements_count: 0
    networked_multiplayer:
      status: "Not Started | In Progress | Complete"
      requirements_count: 0
    user_interface:
      status: "Not Started | In Progress | Complete"
      requirements_count: 0
    non_functional:
      status: "Not Started | In Progress | Complete"
      requirements_count: 0
    constraints:
      status: "Not Started | In Progress | Complete"
      requirements_count: 0

  questions_pending: []
  blockers: []
  next_action: "[Exactly what to do next]"
```
  </progress_tracking_schema>

  <resumption_protocol>
  WHEN CONTEXT IS COMPACTED OR SESSION RESUMES:

  1. IMMEDIATELY check for existing progress:
     ```bash
     cat oxs/.work/requirements/progress.yaml 2>/dev/null || echo "NO_PROGRESS_FILE"
     ```

  2. IF progress file exists:
     - Read current_phase, status
     - Review draft-requirements.yaml for completed work
     - Review elicitation-log.yaml for conversation history
     - Resume from next_action

  3. IF no progress file:
     - Initialize .work/requirements/ directory
     - Begin with Phase 1 (Core Gameplay Requirements)
  </resumption_protocol>
</context_compaction_survival>

<requirements_categories>
  <category id="CORE" name="Core Gameplay">
    <description>Fundamental noughts and crosses game rules and mechanics</description>
    <elicitation_questions>
      <question>Standard 3x3 grid, or support for larger boards (4x4, 5x5)?</question>
      <question>Standard win condition (3-in-a-row), or configurable (N-in-a-row)?</question>
      <question>How should draws be handled?</question>
      <question>Should the game track statistics (wins, losses, draws)?</question>
      <question>Should there be a match system (best of 3, 5, etc.)?</question>
    </elicitation_questions>
  </category>

  <category id="LOCAL" name="Local 2-Player">
    <description>Two players on the same device</description>
    <elicitation_questions>
      <question>How do players indicate their turn (click, keyboard)?</question>
      <question>Should players be able to customise their symbols (X/O, colours)?</question>
      <question>Should there be player names or profiles?</question>
      <question>Undo/redo functionality?</question>
      <question>How is the starting player determined?</question>
    </elicitation_questions>
  </category>

  <category id="AI" name="AI Opponent">
    <description>Single-player mode against computer opponent</description>
    <elicitation_questions>
      <question>What difficulty levels are needed (Easy, Medium, Hard)?</question>
      <question>Should AI be "perfect" at highest difficulty (never loses)?</question>
      <question>Should AI have visible "thinking" time or respond instantly?</question>
      <question>Should AI difficulty be adjustable mid-game?</question>
      <question>Any AI "personality" (aggressive, defensive, random)?</question>
    </elicitation_questions>
  </category>

  <category id="NET" name="Networked Multiplayer">
    <description>Two players over network connection</description>
    <elicitation_questions>
      <question>Direct IP connection, or lobby/matchmaking system?</question>
      <question>Host-client or peer-to-peer architecture?</question>
      <question>How to handle disconnections mid-game?</question>
      <question>Latency compensation requirements?</question>
      <question>Chat functionality needed?</question>
      <question>Spectator mode?</question>
    </elicitation_questions>
  </category>

  <category id="UI" name="User Interface">
    <description>Visual presentation and user interaction</description>
    <elicitation_questions>
      <question>Target platforms (PC only, mobile, cross-platform)?</question>
      <question>Visual style preference (minimal, animated, themed)?</question>
      <question>Sound effects and music?</question>
      <question>Accessibility features (colourblind modes, screen reader)?</question>
      <question>Resolution and aspect ratio support?</question>
    </elicitation_questions>
  </category>

  <category id="NFR" name="Non-Functional Requirements">
    <description>Quality attributes and constraints</description>
    <elicitation_questions>
      <question>Target frame rate?</question>
      <question>Maximum acceptable network latency?</question>
      <question>Localisation/internationalisation needs?</question>
      <question>Save game/resume functionality?</question>
      <question>Analytics or telemetry?</question>
    </elicitation_questions>
  </category>

  <category id="CON" name="Technical Constraints">
    <description>Fixed technical boundaries</description>
    <known_constraints>
      <constraint>Godot 4.5 game engine</constraint>
      <constraint>C# with .NET 8</constraint>
      <constraint>Following STD-0002 code style rubric</constraint>
    </known_constraints>
    <elicitation_questions>
      <question>Target Godot export platforms?</question>
      <question>Any specific networking library preferences?</question>
      <question>Minimum hardware specifications?</question>
    </elicitation_questions>
  </category>
</requirements_categories>

<elicitation_methodology>
  <approach>
  Use structured questioning to systematically gather requirements:

  1. **Start with context** - Confirm understanding of the project scope
  2. **Category by category** - Work through each requirement category in order
  3. **Ask, don't assume** - Never assume requirements; always confirm with user
  4. **Clarify ambiguity** - If an answer is unclear, ask follow-up questions
  5. **Summarise frequently** - After each category, summarise what was captured
  6. **Validate completeness** - Before moving on, confirm nothing is missing
  </approach>

  <questioning_techniques>
    <technique name="Open questions">Start broad: "What should happen when..."</technique>
    <technique name="Closed questions">Confirm specifics: "Should X support Y?"</technique>
    <technique name="Scenario-based">Explore edge cases: "What if a player..."</technique>
    <technique name="Priority clarification">Establish importance: "Is this essential or nice-to-have?"</technique>
  </questioning_techniques>

  <default_assumptions>
  When user explicitly defers to your judgement, apply these sensible defaults:

  - Grid size: 3x3 standard
  - Win condition: 3-in-a-row
  - AI levels: Easy (random), Medium (basic strategy), Hard (minimax)
  - Network: Host-client model with Godot's built-in networking
  - UI: Clean, minimal, responsive
  - Platform: PC (Windows/Linux/Mac)
  </default_assumptions>
</elicitation_methodology>

<output_specifications>
  <primary_output>
    <file>oxs/docs/requirements/OXS-REQUIREMENTS.md</file>
    <format>Markdown with structured requirement tables</format>
  </primary_output>

  <document_structure>
```markdown
# OXS Requirements Specification

## Document Information
- **Version:** 1.0
- **Date:** [Date]
- **Status:** Draft | Under Review | Approved

## 1. Introduction
### 1.1 Purpose
### 1.2 Scope
### 1.3 Definitions and Acronyms

## 2. Overall Description
### 2.1 Product Perspective
### 2.2 User Classes and Characteristics
### 2.3 Operating Environment
### 2.4 Assumptions and Dependencies

## 3. Functional Requirements

### 3.1 Core Gameplay (CORE)
| ID | Requirement | Priority | Acceptance Criteria |
|----|-------------|----------|---------------------|
| CORE-001 | [Description] | Must/Should/Could | [Testable criteria] |

### 3.2 Local 2-Player (LOCAL)
| ID | Requirement | Priority | Acceptance Criteria |
|----|-------------|----------|---------------------|
| LOCAL-001 | [Description] | Must/Should/Could | [Testable criteria] |

### 3.3 AI Opponent (AI)
| ID | Requirement | Priority | Acceptance Criteria |
|----|-------------|----------|---------------------|
| AI-001 | [Description] | Must/Should/Could | [Testable criteria] |

### 3.4 Networked Multiplayer (NET)
| ID | Requirement | Priority | Acceptance Criteria |
|----|-------------|----------|---------------------|
| NET-001 | [Description] | Must/Should/Could | [Testable criteria] |

### 3.5 User Interface (UI)
| ID | Requirement | Priority | Acceptance Criteria |
|----|-------------|----------|---------------------|
| UI-001 | [Description] | Must/Should/Could | [Testable criteria] |

## 4. Non-Functional Requirements

| ID | Category | Requirement | Target | Measurement |
|----|----------|-------------|--------|-------------|
| NFR-001 | Performance | [Description] | [Target value] | [How to measure] |

## 5. Technical Constraints

| ID | Constraint | Rationale |
|----|------------|-----------|
| CON-001 | [Description] | [Why this constraint exists] |

## 6. Appendices
### A. Glossary
### B. References
### C. Revision History
```
  </document_structure>

  <requirement_format>
  Each requirement MUST have:
  - **Unique ID**: Category prefix + sequential number (e.g., CORE-001)
  - **Description**: Clear, unambiguous statement of what is required
  - **Priority**: Must (essential), Should (important), Could (nice-to-have)
  - **Acceptance Criteria**: How to verify the requirement is met

  Good example:
  | AI-003 | The AI opponent shall provide three difficulty levels: Easy, Medium, and Hard | Must | Player can select difficulty from menu; each level exhibits distinct behaviour |

  Bad example:
  | AI-003 | Good AI | Must | Works well |
  </requirement_format>
</output_specifications>

<examples>
  <example name="Good functional requirement">
```markdown
| CORE-002 | The game shall detect and announce a win when a player places three of their symbols in a horizontal, vertical, or diagonal line | Must | Given a board state with three X's in a row, the game displays "X wins" within 500ms |
```
  </example>

  <example name="Good non-functional requirement">
```markdown
| NFR-002 | Performance | The game shall maintain 60 FPS during gameplay | 60 FPS minimum | FPS counter shows no drops below 60 during 100 consecutive games |
```
  </example>

  <example name="Good AI requirement">
```markdown
| AI-001 | The Easy AI shall make random valid moves with no strategic consideration | Must | Over 100 games against optimal play, Easy AI wins less than 10% of games |
| AI-002 | The Medium AI shall block opponent winning moves and take winning moves when available | Must | Medium AI never loses in one move when block is possible; always takes immediate win |
| AI-003 | The Hard AI shall play optimally using the Minimax algorithm, never losing a game | Must | Over 100 games, Hard AI loses 0 games (draws or wins only) |
```
  </example>

  <example name="Good networking requirement">
```markdown
| NET-001 | The game shall support networked play between two players over TCP/IP | Must | Two players on different machines can complete a full game |
| NET-002 | The game shall synchronise board state within 100ms of a move being made | Should | Latency measurement shows state sync < 100ms on local network |
| NET-003 | The game shall gracefully handle player disconnection by pausing and offering reconnection | Must | When player disconnects, other player sees "Opponent disconnected" and game pauses |
```
  </example>
</examples>

<critical_reminders>
================================================================================
                    CRITICAL REMINDERS
================================================================================

1. **ASK, DON'T ASSUME**
   - Never fill in requirements without user input
   - If something is unclear, ask
   - Only use defaults when user explicitly defers

2. **CHECKPOINT FREQUENTLY**
   - Update progress.yaml after each category
   - Save draft-requirements.yaml incrementally
   - Log all Q&A in elicitation-log.yaml

3. **TESTABLE CRITERIA**
   - Every requirement needs acceptance criteria
   - "Works correctly" is not testable
   - Include specific values, counts, or observable behaviours

4. **PRIORITY DISCIPLINE**
   - Must: Game cannot ship without this
   - Should: Important but can be descoped if needed
   - Could: Nice-to-have, implement if time permits

5. **COMPLETE BEFORE MOVING ON**
   - Finish one category fully before starting another
   - Confirm with user before marking complete
   - Document any deferred decisions

6. **OUTPUT GOES TO docs/, NOT .work/**
   - .work/ is for internal tracking only
   - Final requirements document goes to oxs/docs/requirements/
   - This is a deliverable for human review

</critical_reminders>

<begin>
=====================================
CRITICAL: CHECK FOR EXISTING PROGRESS FIRST
=====================================
This requirements gathering may have been started before context compaction.

FIRST ACTION - Check for existing progress:
```bash
cat oxs/.work/requirements/progress.yaml 2>/dev/null || echo "NO_PROGRESS_FILE"
```

IF progress file exists:
- Read current category and next_action
- Review elicitation-log.yaml for conversation history
- Resume gathering from where you left off

IF no progress file (fresh start):
- Create oxs/.work/requirements/ directory
- Initialise progress.yaml
- Begin with introductory questions to establish scope

=====================================
STARTING WORKFLOW
=====================================

1. **Greet and establish context**
   - Confirm the user wants to define requirements for OXS
   - Briefly explain the process (category by category)

2. **Work through categories in order:**
   - Core Gameplay (CORE)
   - Local 2-Player (LOCAL)
   - AI Opponent (AI)
   - Networked Multiplayer (NET)
   - User Interface (UI)
   - Non-Functional Requirements (NFR)
   - Technical Constraints (CON)

3. **For each category:**
   - Ask elicitation questions from the category
   - Capture answers in draft-requirements.yaml
   - Summarise what was captured
   - Confirm completeness before moving on
   - Update progress.yaml

4. **After all categories complete:**
   - Generate final OXS-REQUIREMENTS.md
   - Review with user for any gaps
   - Mark complete

BEGIN NOW by checking for existing progress.
</begin>
