# StitchTrack — Documentation

Author: Gabriella Frank Ferm  
Date: 2025-11-13  
Version: 2.0  
Project: Cross-platform mobile app (.NET MAUI)

---

## Table of contents
- Introduction
- Roadmap (Phases)
- Functional requirements (user stories)
  - Phase 1 (MVP)
  - Phase 2 (Enhanced)
  - Phase 3 (Cloud & Advanced)
  - Phase 4 (Polish & Optional)
- Edge cases & special behaviours
- Non-functional requirements
- Technology stack
- Architecture overview
- Data model (core entities)
- Example JSON payloads
- GUID strategy
- Testing (unit + manual)
- UI / UX notes
- Assets & images
- Appendix: storing original Word docs

---

## 1. Introduction
StitchTrack is a cross-platform mobile app (built with .NET MAUI) that helps knitters and crocheters track row counts, projects, sessions, timers, reminders and pattern files. The app is designed for immediate usability (guest mode), works offline using SQLite, and can optionally sync to the cloud.

---

## 2. Roadmap (Phases)
- Phase 1 — MVP: local, offline-first, essential counter and project CRUD.
- Phase 2 — Enhanced: photos, notes, session tracking, archiving.
- Phase 3 — Cloud & Advanced: auth, sync, PDF patterns, reminders.
- Phase 4 — Polish: multi-device sync, profile management, exports, UI polish.

Short, testable scope for Phase 1 is recommended before advancing.

---

## 3. Functional requirements (user stories)

### Phase 1 — MVP (core)
- As a user I want to increment/decrement the row counter (guest mode) so I can start immediately.
- As a user I want to reset the counter.
- As a user I want to undo the last change.
- As a user I want to create, read, update and delete projects.
- As a user I want to see a list of projects with their current row counts.

### Phase 2 — Enhanced
- Upload/display a photo per project.
- Add comments/notes and archive/unarchive projects.
- Set total rows for progress percentage.
- Session timer: start/stop and view session history.
- Duplicate a project to start a similar one quickly.

### Phase 3 — Cloud & Advanced
- User authentication and cloud synchronization across devices.
- Upload and view PDF pattern files.
- Reminders/notifications and conflict resolution UI.
- Export/backup data (JSON/CSV).
- Account management (profile update, delete account & data).

### Phase 4 — Polish & Optional
- Dark/light theme.
- Search and filter projects (active/archived).
- Tags per project.
- Multiple counters per project for complex patterns.
- Share/export project progress (image or share sheet).

---

## 4. Edge cases & special behaviours
- Counter cannot go below 0.
- Duplicate project names allowed, but warn user.
- Offline edits queue for sync when online; show sync status.
- Undo should only affect the most recent change in the current session (configurable).

---

## 5. Non-functional requirements
- Must operate offline (SQLite local storage).
- UI must be accessible with large, tappable controls.
- Target Android and iOS (cross-platform).
- Efficient media storage and handle camera/storage permissions gracefully.
- Securely handle authentication tokens and personal data.

---

## 6. Technology stack
Core:
- .NET MAUI (single codebase for Android + iOS)
- MVVM pattern (XAML + ViewModels)
- Local DB: SQLite (EF Core, or a lightweight wrapper)
- Backend (optional): ASP.NET Core Web API (.NET 8)
- Auth: ASP.NET Core Identity + JWT (Phase 3)

Dev/testing:
- xUnit, Moq, FluentAssertions
- EF Core InMemory for tests

Hosting / cloud (future):
- Database: PostgreSQL (Supabase) or Azure SQL
- Media: Cloudinary or Supabase storage
- Hosting: Railway/Fly.io or a low-cost provider

Notes:
- Use Microsoft.Extensions.DependencyInjection for DI.
- Keep third-party services optional and behind feature flags.

---

## 7. Architecture overview
Follow Clean Architecture / layered design:
- Presentation: MAUI Views & ViewModels (UI, navigation)
- Application: Use cases, services, validation, DTOs
- Infrastructure: EF Core, repositories, API clients, file I/O
- Cross-cutting: Logging, error handling, telemetry, DI

Keep layers independent and testable. UI layer should be thin — move business logic to services/use-cases.

---

## 8. Data model (core entities)
Below is a compact summary. Use EF Core conventions (GUID PKs) and proper indexing for queries.

Entity | Key fields (examples)
---|---
User | Id (GUID, PK), Email, PasswordHash, DisplayName, CreatedAt
Project | Id, UserId (nullable for guest), Name, CurrentCount, TotalRows, IsArchived, ImagePath, ImageUrl, CreatedAt, UpdatedAt
Session | Id, ProjectId, StartedAt, EndedAt, DurationSeconds, StartingRowCount
Reminder | Id, ProjectId, IntervalMinutes, IsEnabled, LastTriggeredAt
PatternFile | Id, ProjectId, FileName, FilePath, FileUrl, UploadedAt
RowNote | Id, ProjectId, RowNumber, NoteText, CreatedAt
CounterHistory | Id, ProjectId, OldValue, NewValue, ChangedAt

Constraints:
- CounterHistory.ProjectId -> Project.Id (ON DELETE CASCADE desired for history cleanup)
- Unique index on User.Email
- Consider indices on Project.UserId and Project.IsArchived for fast queries.

---

## 9. Example JSON payloads
Project (upload)
```json
{
  "id": "GUID",
  "userId": "GUID",
  "name": "Socks",
  "currentCount": 42,
  "yarnColor": "018-purple",
  "imageUrl": null,
  "createdAt": "2025-09-16T12:00:00Z",
  "updatedAt": "2025-09-16T12:30:00Z"
}
```

Reminder (upload)
```json
{
  "id": "GUID",
  "projectId": "GUID",
  "intervalMinutes": 30,
  "isEnabled": true,
  "lastTriggeredAt": null
}
```

---

## 10. GUID strategy
- Use GUIDs (System.Guid) as primary keys to enable offline creation and later sync.
- Mobile: generate via Guid.NewGuid() on create.
- Backend: accept GUID strings, validate via Guid.TryParse.
- Storage: SQLite store GUIDs as TEXT; in SQL Server use uniqueidentifier (NEWSEQUENTIALID for clustered indexes if desired).

When to use GUIDs: when offline creation or multi-client writes are expected; otherwise auto-increment integers may be simpler for small single-node systems.

---

## 11. Testing
Principle: test behaviour, not implementation. Focus tests on business logic and sync-critical code.

Must test:
- Counter increment/decrement, undo, and floor at zero
- Project create/read/update/delete and persistence to SQLite
- GUID generation and validity
- Data export (JSON/CSV) format and contents
- Auth token validation & password hashing (backend)

Should test:
- Sync conflict resolution
- Session timer calculations
- File upload validation

Nice-to-test:
- UI toggles (dark mode), search & filter (manual + automated where feasible)

Unit test tools:
- xUnit, Moq, FluentAssertions, Microsoft.EntityFrameworkCore.InMemory

CI: run unit tests for core libraries on each PR.

Manual testing checklist (Phase 1 short list):
- Guest counter works (inc/dec/reset/undo)
- Create & list projects persist across app restarts
- Delete confirms and removes project
- Basic UI flows on Android and iOS simulators/emulators

---

## 12. UI / UX notes
- Use large primary buttons for + / − / reset / undo.
- Subtle animations (number scale on change, small button press scale).
- Provide haptic feedback on change when available.
- Loading & error states: clear toasts/dialogs and retry options.
- Accessibility: support large fonts, voiceover/ TalkBack labels.

---

## 13. Assets & images
Stored images under:
- docs/assets/wireframes/
- docs/assets/diagrams/
