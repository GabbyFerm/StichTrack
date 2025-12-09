# StitchTrack — Technical Documentation

**Author:** Gabriella Frank Ferm  
**Date:** December 2024  
**Version:** 3.0  
**Project Type:** Cross-platform mobile app (.NET MAUI)

---

## Table of Contents
1. [Introduction](#1-introduction)
2. [Design Philosophy](#2-design-philosophy)
3. [Roadmap (Phases)](#3-roadmap-phases)
4. [Functional Requirements](#4-functional-requirements)
5. [Non-Functional Requirements](#5-non-functional-requirements)
6. [Technology Stack](#6-technology-stack)
7. [Architecture Overview](#7-architecture-overview)
8. [Data Model](#8-data-model)
9. [UI/UX Specifications](#9-uiux-specifications)
10. [Cloud Sync Strategy](#10-cloud-sync-strategy)
11. [Testing Strategy](#11-testing-strategy)
12. [Security & Privacy](#12-security--privacy)

---

## 1. Introduction

StitchTrack is a **local-first**, **privacy-focused** mobile application for knitters and crocheters. It helps users track row counts, manage projects, and measure session time—all without requiring an account or internet connection.

### Core Values
- **Immediate value** — users can start counting rows within seconds of opening the app
- **Privacy by default** — all data stays on the user's device unless they explicitly enable cloud backup
- **User ownership** — users can export their data at any time in standard formats (JSON, CSV)
- **Offline-first** — the app works perfectly without internet; sync is optional

---

## 2. Design Philosophy

### Why Local-First?

**Traditional approach (rejected):**
```
Open app → Create account → Verify email → Log in → Start using
```

**Our approach:**
```
Open app → Start counting rows (done!)
```

### Benefits of Local-First Architecture

1. **No Onboarding Friction**
   - No sign-up forms
   - No password requirements
   - No email verification
   - Users get value immediately

2. **Privacy by Default**
   - No server-side data collection
   - No tracking or analytics
   - Data never leaves device unless user enables sync
   - Complies with GDPR, CCPA without effort

3. **Offline-First = Reliable**
   - Works on planes, trains, remote areas
   - No "no internet connection" errors
   - Instant app responsiveness
   - No loading spinners for basic operations

4. **Reduced Complexity**
   - No backend API to maintain
   - No authentication system to secure
   - No database hosting costs
   - Fewer potential failure points

5. **User Trust**
   - Uses cloud storage users already trust (iCloud, Google Drive)
   - No "yet another account" problem
   - Clear data ownership

---

## 3. Roadmap (Phases)

### Phase 1: MVP — Local-Only (✅ Complete)
**Timeline:** Jan 2025  
**Status:** Complete

**Features:**
- Quick counter (unsaved session)
- Project CRUD (Create, Read, Update, Delete)
- Counter history & undo
- Local SQLite persistence
- Bottom navigation
- First-run onboarding

**Technical Scope:**
- SQLite database with EF Core
- MVVM architecture
- Unit tests for domain logic
- CI/CD with GitHub Actions

---

### Phase 2: Enhanced Local Features (🚧 In Progress)
**Timeline:** Feb–Mar 2025  
**Status:** Planning

**Features:**
- Upload project photos
- Add notes/comments to projects
- Session timer (start/stop/duration)
- Set total rows & show progress percentage
- Archive completed projects
- Dark mode support
- Manual JSON/CSV export

**Technical Scope:**
- File I/O for images
- Camera/photo library permissions
- Background timer service
- Export data models

---

### Phase 3: Cloud Sync (📅 Planned)
**Timeline:** Apr–Jun 2025  
**Status:** Not started

**Features:**
- iCloud sync (iOS)
- Google Drive sync (Android)
- Sync status indicator (header icon)
- Conflict resolution UI
- "Backup Now" prompts
- Last synced timestamp

**Technical Scope:**
- Platform-specific sync services
- Conflict detection algorithm
- Sync settings page
- Network status monitoring

---

### Phase 4: Polish & Release (📅 Planned)
**Timeline:** Jul–Sep 2025  
**Status:** Not started

**Features:**
- Dropbox sync (both platforms)
- Multiple counters per project
- Project tags
- Search & filter
- Share project as image
- App Store & Google Play submission

**Technical Scope:**
- App Store compliance (privacy manifest, etc.)
- Play Store compliance
- Beta testing program
- Analytics (if needed)

---

## 4. Functional Requirements (User Stories)

### Phase 1 — Core Functionality

| ID | User Story | Acceptance Criteria | Status |
|----|-----------|---------------------|--------|
| US-001 | As a user, I want to start counting rows immediately without login | Quick counter visible on launch | ✅ Done |
| US-002 | As a user, I want to increment my row count | +1 button increases count, haptic feedback | ✅ Done |
| US-003 | As a user, I want to decrement my row count | -1 button decreases count (min 0) | ✅ Done |
| US-004 | As a user, I want to reset the counter | Reset button sets count to 0, asks for confirmation | ✅ Done |
| US-005 | As a user, I want to undo my last change | Undo button reverts to previous value | ✅ Done |
| US-006 | As a user, I want to save my quick counter to a project | "Save to Project" button creates new project | ✅ Done |
| US-007 | As a user, I want to create a new project | Can create project with name | ✅ Done |
| US-008 | As a user, I want to see a list of my projects | Project list shows name and current count | ✅ Done |
| US-009 | As a user, I want to delete a project | Delete button with confirmation dialog | ✅ Done |
| US-010 | As a user, I want my data to persist between app launches | SQLite database saves all changes | ✅ Done |

### Phase 2 — Enhanced Features

| ID | User Story | Acceptance Criteria | Status |
|----|-----------|---------------------|--------|
| US-011 | As a user, I want to add a photo to my project | Can capture or upload photo | 📅 Planned |
| US-012 | As a user, I want to add notes to my project | Text field saves notes | 📅 Planned |
| US-013 | As a user, I want to track how long I work on a project | Session timer shows duration | 📅 Planned |
| US-014 | As a user, I want to see my session history | List of past sessions with durations | 📅 Planned |
| US-015 | As a user, I want to archive completed projects | Archived projects hidden from main list | 📅 Planned |
| US-016 | As a user, I want to export my data | JSON export contains all projects | 📅 Planned |

### Phase 3 — Cloud Sync

| ID | User Story | Acceptance Criteria | Status |
|----|-----------|---------------------|--------|
| US-017 | As a user, I want to back up my projects to iCloud | Enable iCloud in settings, projects sync | 📅 Planned |
| US-018 | As a user, I want to back up my projects to Google Drive | Enable Drive in settings, projects sync | 📅 Planned |
| US-019 | As a user, I want to see sync status | Cloud icon shows synced/syncing/error | 📅 Planned |
| US-020 | As a user, I want to resolve sync conflicts | Dialog shows local vs. cloud, choose version | 📅 Planned |

---

## 5. Non-Functional Requirements

### Performance
- App launch: < 2 seconds on mid-range device
- Counter tap response: < 100ms (haptic feedback instant)
- Project list load: < 500ms for 100 projects
- Database queries: < 50ms for common operations

### Reliability
- No data loss on app crash
- Graceful handling of low storage
- Offline operation guaranteed
- Sync conflicts resolved without data loss

### Usability
- One-handed operation for core features
- Large tap targets (minimum 44x44 pt)
- Accessible to users with vision impairments (VoiceOver/TalkBack)
- No more than 3 taps to common actions

### Security & Privacy
- No server-side data storage (Phase 1–2)
- Local database not encrypted (SQLite default) — consider encryption in Phase 3
- Cloud sync uses platform APIs (iCloud, Drive) with their security
- No analytics/tracking without explicit consent

### Compatibility
- iOS: 14.0+ (to support wide range of devices)
- Android: API 21+ (Android 5.0)
- Screen sizes: 4" phone to 12" tablet

---

## 6. Technology Stack

### Core Technologies
| Component | Technology | Justification |
|-----------|-----------|---------------|
| UI Framework | .NET MAUI | Single codebase, native performance, C# expertise |
| Architecture | MVVM | Industry standard for MAUI, testable |
| Local Database | SQLite + EF Core | Lightweight, built-in, excellent ORM |
| Language | C# 12 (.NET 8) | Modern features, strong typing, async/await |
| Dependency Injection | Microsoft.Extensions.DI | Built-in, familiar, well-documented |

### Testing Stack
| Component | Technology | Purpose |
|-----------|-----------|---------|
| Unit Tests | NUnit | Industry standard for .NET |
| Assertions | FluentAssertions | Readable test assertions |
| Mocking | Moq | Flexible mocking framework |
| Integration Tests | EF Core InMemory | Fast database testing |
| CI/CD | GitHub Actions | Automated builds & tests |

### Phase 3 Dependencies (Cloud Sync)
| Platform | Service | SDK |
|----------|---------|-----|
| iOS | iCloud Drive | CloudKit framework |
| Android | Google Drive | Google Drive API v3 |
| Both | Dropbox | Dropbox SDK |

### Development Tools
- **IDE:** Visual Studio 2022+ or VS Code with MAUI extensions
- **Emulators:** Android Studio AVD, Xcode iOS Simulator
- **Version Control:** Git + GitHub
- **Code Formatting:** .editorconfig + dotnet format
- **Documentation:** Markdown

---

## 7. Architecture Overview

StitchTrack follows **Clean Architecture** with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                  StitchTrack.MAUI                       │
│  (Views, ViewModels, Platform-Specific Code)           │
│  - GuestCounterPage.xaml / .cs                         │
│  - ProjectsPage.xaml / .cs                             │
│  - Platforms/ (iOS, Android)                           │
└────────────────────┬────────────────────────────────────┘
                     │ depends on
┌────────────────────▼────────────────────────────────────┐
│              StitchTrack.Application                    │
│  (ViewModels, Commands, Use Cases)                     │
│  - GuestCounterViewModel                               │
│  - ProjectsViewModel                                   │
│  - Commands (RelayCommand)                             │
└────────────────────┬────────────────────────────────────┘
                     │ depends on
┌────────────────────▼────────────────────────────────────┐
│             StitchTrack.Infrastructure                  │
│  (Data Access, Repositories, File I/O)                 │
│  - AppDbContext (EF Core)                              │
│  - Repositories                                        │
│  - Sync Services (Phase 3)                             │
└────────────────────┬────────────────────────────────────┘
                     │ depends on
┌────────────────────▼────────────────────────────────────┐
│               StitchTrack.Domain                        │
│  (Core Entities, Business Logic, Interfaces)           │
│  - Project, Session, CounterHistory, etc.              │
│  - Factory methods, business rules                     │
│  - No dependencies on other layers                     │
└─────────────────────────────────────────────────────────┘
```

### Dependency Flow
- **MAUI** depends on **Application** (ViewModels)
- **Application** depends on **Infrastructure** (repositories) and **Domain** (entities)
- **Infrastructure** depends on **Domain** (entities)
- **Domain** depends on **nothing** (pure business logic)

### Key Principles
1. **Domain is independent** — no EF Core, no MAUI, just business rules
2. **Interfaces in Domain** — Infrastructure implements them (Dependency Inversion)
3. **ViewModels in Application** — no XAML/UI concerns in ViewModels
4. **Platform code isolated** — iOS/Android specific code in MAUI/Platforms/

---

## 8. Data Model

### Core Entities

#### Project (Aggregate Root)
```csharp
public class Project
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; } // Always NULL in Phase 1
    
    public string Name { get; private set; }
    public int CurrentCount { get; private set; } // Current row count
    
    // Optional fields
    public string? ColorHex { get; private set; } // Yarn color (e.g., "#FF5733")
    public int? TotalRows { get; private set; } // Total rows in pattern
    public int? RowsPerRepeat { get; private set; } // Rows per pattern repeat
    public string? Notes { get; private set; } // User notes
    public bool IsArchived { get; private set; } // Soft delete
    
    // Media
    public string? ImagePath { get; private set; } // Local file path
    public string? ImageUrl { get; private set; } // Cloud URL (Phase 3)
    
    // Timestamps
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    // Cloud sync (Phase 3)
    public DateTime? LastSyncedAt { get; private set; }
    public string? CloudFileId { get; private set; }
    public int SyncVersion { get; private set; } = 0;
    
    // Relationships
    public ICollection<CounterHistory> CounterHistoryEntries { get; private set; }
    public ICollection<Session> Sessions { get; private set; }
    public ICollection<RowNote> RowNotes { get; private set; }
    // ... etc
    
    // Factory method
    public static Project CreateProject(string name, Guid? userId = null);
    
    // Business methods
    public void IncrementCount();
    public void DecrementCount();
    public void ResetCount();
    public bool UndoLastChange();
    public void ArchiveProject();
    public void MarkAsSynced(string cloudFileId);
}
```

#### CounterHistory
```csharp
public class CounterHistory
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    
    public int OldValue { get; private set; }
    public int NewValue { get; private set; }
    public DateTime ChangedAt { get; private set; }
    
    // Factory method (internal - only Project can create)
    internal static CounterHistory CreateCounterHistory(
        Guid projectId, int oldValue, int newValue);
}
```

#### Session
```csharp
public class Session
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public int DurationSeconds { get; private set; }
    
    public int? StartingRowCount { get; private set; }
    public int? EndingRowCount { get; private set; }
    
    // Computed properties
    public bool IsActive => !EndedAt.HasValue;
    public int? RowsCompleted => 
        StartingRowCount.HasValue && EndingRowCount.HasValue
            ? EndingRowCount - StartingRowCount
            : null;
    
    public static Session StartSession(Guid projectId, int? startingRowCount = null);
    public void EndSession(int? endingRowCount = null);
}
```

#### AppSettings (NEW in Phase 2)
```csharp
public class AppSettings
{
    public Guid Id { get; private set; } = 
        Guid.Parse("00000000-0000-0000-0000-000000000001");
    
    // First-run state
    public bool IsFirstRun { get; private set; } = true;
    public DateTime? FirstRunCompletedAt { get; private set; }
    
    // Sync settings (Phase 3)
    public bool SyncEnabled { get; private set; }
    public string? SyncProvider { get; private set; }
    public DateTime? LastSuccessfulSync { get; private set; }
    
    // App preferences
    public string Theme { get; private set; } = "Auto"; // "Light", "Dark", "Auto"
    public bool HapticFeedbackEnabled { get; private set; } = true;
    public int ProjectCreationCount { get; private set; } = 0;
    
    public static AppSettings CreateDefault();
    public void CompleteFirstRun();
    public void EnableSync(string provider);
    public void UpdateTheme(string theme);
}
```

### Database Schema (SQLite)

```sql
-- Projects table
CREATE TABLE Projects (
    Id TEXT PRIMARY KEY,
    UserId TEXT NULL,
    Name TEXT NOT NULL,
    CurrentCount INTEGER NOT NULL DEFAULT 0,
    ColorHex TEXT NULL,
    TotalRows INTEGER NULL,
    RowsPerRepeat INTEGER NULL,
    Notes TEXT NULL,
    IsArchived INTEGER NOT NULL DEFAULT 0,
    ImagePath TEXT NULL,
    ImageUrl TEXT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    LastSyncedAt TEXT NULL,
    CloudFileId TEXT NULL,
    SyncVersion INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX IX_Projects_UpdatedAt ON Projects(UpdatedAt);
CREATE INDEX IX_Projects_IsArchived ON Projects(IsArchived);

-- CounterHistory table
CREATE TABLE CounterHistory (
    Id TEXT PRIMARY KEY,
    ProjectId TEXT NOT NULL,
    OldValue INTEGER NOT NULL,
    NewValue INTEGER NOT NULL,
    ChangedAt TEXT NOT NULL,
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE
);

CREATE INDEX IX_CounterHistory_ProjectId_ChangedAt 
    ON CounterHistory(ProjectId, ChangedAt);

-- Sessions table
CREATE TABLE Sessions (
    Id TEXT PRIMARY KEY,
    ProjectId TEXT NOT NULL,
    StartedAt TEXT NOT NULL,
    EndedAt TEXT NULL,
    DurationSeconds INTEGER NOT NULL DEFAULT 0,
    StartingRowCount INTEGER NULL,
    EndingRowCount INTEGER NULL,
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Sessions_ProjectId ON Sessions(ProjectId);
CREATE INDEX IX_Sessions_StartedAt ON Sessions(StartedAt);

-- AppSettings table (single row)
CREATE TABLE AppSettings (
    Id TEXT PRIMARY KEY,
    IsFirstRun INTEGER NOT NULL DEFAULT 1,
    FirstRunCompletedAt TEXT NULL,
    SyncEnabled INTEGER NOT NULL DEFAULT 0,
    SyncProvider TEXT NULL,
    LastSuccessfulSync TEXT NULL,
    Theme TEXT NOT NULL DEFAULT 'Auto',
    HapticFeedbackEnabled INTEGER NOT NULL DEFAULT 1,
    ProjectCreationCount INTEGER NOT NULL DEFAULT 0
);

-- Insert default settings
INSERT INTO AppSettings (Id) VALUES ('00000000-0000-0000-0000-000000000001');
```

---

## 9. UI/UX Specifications

### Navigation Structure

```
App Launch
    ↓
Is First Run?
    ├─ YES → OnboardingCard
    │         ↓
    │    [Get Started] → QuickCounter
    │    [Enable Backup] → Settings (Sync)
    │         ↓
    └─ NO → QuickCounter (direct)

Bottom TabBar (always visible):
┌──────────┬──────────┬──────────┬──────────┬──────────┐
│ Counter  │ Projects │ Sessions │ Export   │ Settings │
│   (🏠)   │   (📂)   │   (⏱)   │   (📤)   │   (⚙️)   │
└──────────┴──────────┴──────────┴──────────┴──────────┘
```

### Screens

#### 1. Quick Counter (Default Landing)
```
┌─────────────────────────────────────────────────────┐
│  StitchTrack            ☁️ Not synced    [Menu]     │
├─────────────────────────────────────────────────────┤
│                                                     │
│           🧶 Quick Row Counter                      │
│      (Not saved to a project)                       │
│                                                     │
│               ┌──────────┐                          │
│               │   152    │                          │
│               └──────────┘                          │
│                                                     │
│     ┌─────────────┐  ┌─────────────┐               │
│     │ - DECREASE  │  │ + INCREASE  │               │
│     └─────────────┘  └─────────────┘               │
│                                                     │
│            ┌───────────┐                            │
│            │ ↻ Reset   │                            │
│            └───────────┘                            │
│                                                     │
│  ─────────────────────────────────────────────     │
│                                                     │
│  💡 Tip: Tap "Save to Project" to keep             │
│      this counter permanently                       │
│                                                     │
│       ┌──────────────────────┐                      │
│       │ 💾 Save to Project   │                      │
│       └──────────────────────┘                      │
│                                                     │
└─────────────────────────────────────────────────────┘
```

**Behaviors:**
- Counter persists across app restarts (temporary storage)
- "Save to Project" opens name dialog, creates project with current count
- Undo button appears after first change

---

#### 2. Onboarding (First Run Only)
```
┌─────────────────────────────────────────────────────┐
│                                                     │
│               🧶 Welcome to                         │
│                StitchTrack                          │
│                                                     │
│      Start counting rows immediately                │
│          No signup required                         │
│                                                     │
│  ┌───────────────────────────────────────────┐     │
│  │  Your projects are saved locally and      │     │
│  │  private by default. You own your data.   │     │
│  └───────────────────────────────────────────┘     │
│                                                     │
│       ┌──────────────────────┐                      │
│       │   Get Started 🚀     │                      │
│       └──────────────────────┘                      │
│                                                     │
│          Want automatic backup?                     │
│                                                     │
│       ┌──────────────────────┐                      │
│       │ Enable Backup & Sync │                      │
│       │        ☁️            │                      │
│       └──────────────────────┘                      │
│                                                     │
│            [ Maybe Later ]                          │
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

#### 3. Projects List
```
┌─────────────────────────────────────────────────────┐
│  Projects                         [+] [🔍]          │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ┌─────────────────────────────────────────────┐   │
│  │ 🧣 Cozy Scarf                          42   │   │
│  │ Updated 2 hours ago                    rows │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
│  ┌─────────────────────────────────────────────┐   │
│  │ 🧤 Winter Mittens                      18   │   │
│  │ Updated yesterday                      rows │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
│  ┌─────────────────────────────────────────────┐   │
│  │ 🧶 Baby Blanket                       156   │   │
│  │ Updated last week                      rows │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

#### 4. Settings
```
┌─────────────────────────────────────────────────────┐
│  Settings                                    [Done] │
├─────────────────────────────────────────────────────┤
│                                                     │
│  BACKUP & SYNC                                      │
│  ┌─────────────────────────────────────────────┐   │
│  │ Cloud Backup              [Toggle: OFF]     │   │
│  └─────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────┐   │
│  │ Backup Provider              Not set        │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
│  APP SETTINGS                                       │
│  ┌─────────────────────────────────────────────┐   │
│  │ Theme                        Auto            │   │
│  └─────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────┐   │
│  │ Haptic Feedback          [Toggle: ON]       │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
│  DATA MANAGEMENT                                    │
│  ┌─────────────────────────────────────────────┐   │
│  │ Export All Projects (JSON)            →     │   │
│  └─────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────┐   │
│  │ Import Projects                       →     │   │
│  └─────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────┐   │
│  │ Clear All Data                        →     │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
│  ABOUT                                              │
│  ┌─────────────────────────────────────────────┐   │
│  │ Version 1.0.0                                │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

### Design System

**Colors:**
```
Primary: #FE64A3 (Pink - brand color)
Secondary: #424B54 (Dark Gray - buttons)
Accent: #E1AD37 (Gold - highlight)
Background: #E8E8E8 (Light Gray)
Surface: #FFFFFF (White cards)
Text: #000000 (Black)
Text Secondary: #666666 (Gray)
```

**Typography:**
```
Font Family: Montserrat
Headline: ExtraBold, 32pt
Subheadline: SemiBold, 20pt
Body: Regular, 14pt
Button: SemiBold, 16pt
Caption: Regular, 12pt
```

**Spacing:**
```
XS: 4pt
S: 8pt
M: 16pt
L: 24pt
XL: 32pt
```

**Animations:**
```
Counter Change: Scale 1.0 → 1.1 → 1.0 (150ms)
Button Press: Scale 1.0 → 0.95 (100ms)
Card Tap: Scale 1.0 → 0.98 (100ms)
Screen Transition: Slide left/right (300ms)
```

---

## 10. Cloud Sync Strategy

### Why Delay Sync Until Phase 3?

1. **Ship faster** — Phase 1 provides immediate value
2. **Learn from users** — see if they actually want sync
3. **Avoid premature complexity** — sync is hard, get basics right first
4. **Keep costs low** — no server infrastructure needed yet

### Sync Architecture (Phase 3)

```
┌────────────────────────────────────────┐
│  StitchTrack.Infrastructure.Sync       │
├────────────────────────────────────────┤
│  ICloudSyncService (interface)         │
│      ↓ implements                      │
│  ┌──────────────────────────────────┐  │
│  │ iOS: ICloudSyncService           │  │
│  │   Uses: CloudKit framework       │  │
│  └──────────────────────────────────┘  │
│  ┌──────────────────────────────────┐  │
│  │ Android: GoogleDriveSyncService  │  │
│  │   Uses: Drive API v3             │  │
│  └──────────────────────────────────┘  │
│  ┌──────────────────────────────────┐  │
│  │ Both: MockSyncService (testing)  │  │
│  └──────────────────────────────────┘  │
└────────────────────────────────────────┘
```

### Sync Flow

1. **User Enables Sync**
   ```
   User taps "Enable Backup & Sync" in Settings
       ↓
   App checks for existing local projects
       ↓
   If projects exist:
       Show dialog: "Upload existing projects?"
           ├─ Yes, upload all
           ├─ No, start fresh
           └─ Let me choose (checkbox list)
       ↓
   Connect to cloud provider (iCloud/Drive)
       ↓
   Upload selected projects
       ↓
   Mark as synced in database
   ```

2. **Periodic Sync (Background)**
   ```
   Every 5 minutes (if sync enabled):
       ↓
   Check for local changes (SyncVersion incremented)
       ↓
   If changes exist:
       Upload to cloud
       Mark as synced
       ↓
   Check for remote changes
       ↓
   If remote changes exist:
       Compare SyncVersion
       If conflict:
           Show conflict resolution UI
       Else:
           Download and apply changes
   ```

3. **Conflict Resolution**
   ```
   Conflict detected
       ↓
   Show dialog:
       "Conflict on [Project Name]"
       
       Local Version:
       - Count: 42
       - Modified: 2 hours ago
       
       Cloud Version:
       - Count: 38
       - Modified: 1 hour ago
       
       [ Keep Local ]  [ Use Cloud ]  [ Keep Both ]
   ```

### Sync Data Format (JSON)

```json
{
  "project": {
    "id": "guid",
    "name": "Cozy Scarf",
    "currentCount": 42,
    "syncVersion": 5,
    "lastSyncedAt": "2025-01-15T14:30:00Z",
    "cloudFileId": "icloud-file-id-here"
  }
}
```

---

## 11. Testing Strategy

### Test Pyramid

```
                  ┌──────────────┐
                  │ Manual Tests │  (5%)
                  └──────────────┘
              ┌────────────────────┐
              │ Integration Tests  │  (15%)
              └────────────────────┘
          ┌──────────────────────────┐
          │      Unit Tests          │  (80%)
          └──────────────────────────┘
```

### Unit Tests (Domain & Application)

**What to test:**
- ✅ Business rules (counter floor at 0)
- ✅ Entity creation (factory methods)
- ✅ Entity behavior (increment, decrement, reset)
- ✅ Undo logic
- ✅ ViewModel commands
- ✅ Property change notifications

**What NOT to test:**
- ❌ Simple getters/setters
- ❌ EF Core (test with InMemory instead)
- ❌ XAML views

**Example:**
```csharp
[Test]
public void DecrementCount_WhenAtZero_ShouldStayAtZero()
{
    // Arrange
    var project = Project.CreateProject("Test");
    
    // Act
    project.DecrementCount();
    
    // Assert
    project.CurrentCount.Should().Be(0);
}
```

### Integration Tests (Infrastructure)

**What to test:**
- ✅ Database migrations apply cleanly
- ✅ Repository CRUD operations
- ✅ Complex queries (e.g., filter by date range)
- ✅ Cascade deletes work correctly

**Example:**
```csharp
[Test]
public async Task DeleteProject_ShouldCascadeDeleteCounterHistory()
{
    // Arrange
    var project = Project.CreateProject("Test");
    project.IncrementCount();
    await _context.Projects.AddAsync(project);
    await _context.SaveChangesAsync();
    
    // Act
    _context.Projects.Remove(project);
    await _context.SaveChangesAsync();
    
    // Assert
    var history = await _context.CounterHistory
        .Where(h => h.ProjectId == project.Id)
        .ToListAsync();
    history.Should().BeEmpty();
}
```

### Manual Testing Checklist

**Phase 1 MVP:**
- [ ] Quick counter increments/decrements
- [ ] Counter stays at 0 (doesn't go negative)
- [ ] Reset asks for confirmation
- [ ] Undo reverts last change
- [ ] Save to Project creates project
- [ ] Projects list shows all projects
- [ ] Delete project asks for confirmation
- [ ] Data persists after app restart
- [ ] Haptic feedback works on button press
- [ ] First-run onboarding shows once

---

## 12. Security & Privacy

### Data Storage

**Phase 1–2 (Local Only):**
- All data stored in SQLite on device
- Database location: `{AppDataDirectory}/stitchtrack.db3`
- Not encrypted by default (consider encryption in Phase 3)
- Backed up by device backup (iCloud Backup / Google Backup)

**Phase 3 (Cloud Sync):**
- iCloud: Data stored in user's iCloud Drive (encrypted by Apple)
- Google Drive: Data stored in user's Drive (encrypted by Google)
- No StitchTrack servers involved

### Privacy Policy

**What we collect: NOTHING**

- ❌ No analytics
- ❌ No crash reporting (unless user opts in)
- ❌ No user accounts
- ❌ No tracking
- ❌ No ads

**What we don't collect:**
- Your name, email, or any personal info
- Your location
- Your usage patterns
- Your project data (stays local or in your cloud)

### App Store Requirements

**iOS Privacy Manifest (Phase 4):**
```xml
<key>NSPrivacyTracking</key>
<false/>
<key>NSPrivacyTrackingDomains</key>
<array>
  <!-- Empty - no tracking -->
</array>
```

**Android Data Safety (Phase 4):**
```
Data collected: NONE
Data shared: NONE
Security practices:
  - Data encrypted in transit (cloud sync)
  - You can request data deletion (export → delete app)
```

---

## Appendix

### Export Format Example (JSON)

```json
{
  "exportVersion": "1.0",
  "exportedAt": "2025-01-15T14:30:00Z",
  "projects": [
    {
      "id": "guid",
      "name": "Cozy Scarf",
      "currentCount": 42,
      "colorHex": "#FF5733",
      "totalRows": 100,
      "notes": "Using worsted weight yarn",
      "isArchived": false,
      "createdAt": "2025-01-01T10:00:00Z",
      "updatedAt": "2025-01-15T14:00:00Z",
      "sessions": [
        {
          "id": "guid",
          "startedAt": "2025-01-15T13:00:00Z",
          "endedAt": "2025-01-15T14:00:00Z",
          "durationSeconds": 3600,
          "rowsCompleted": 10
        }
      ]
    }
  ]
}
```

---

**END OF DOCUMENTATION**
