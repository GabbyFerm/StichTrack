# StitchTrack - Task List

**Status:** Updated for local-first architecture redesign  
**Date:** December 2024  
**Current Phase:** Phase 1 - MVP (Local Only)

---

## 🎯 Overview

This document tracks all development tasks for StitchTrack across 4 phases:
- **Phase 1:** MVP - Local-only functionality (no auth, no cloud)
- **Phase 2:** Enhanced features (photos, notes, sessions, dark mode)
- **Phase 3:** Cloud sync (iCloud, Google Drive, Dropbox)
- **Phase 4:** Polish & app store release

---

## ✅ Phase 1: MVP - Local Only

**Goal:** Ship a working row counter app with local storage, no authentication required.

**Status:** ~60% complete

---

### 1.1 Database & Infrastructure ✅

#### TASK-001: Create AppSettings entity
**Status:** ✅ Done  
**Priority:** High  
**Labels:** `phase-1`, `domain`, `database`

**Description:**
Create AppSettings entity to store user preferences and first-run state.

**Acceptance Criteria:**
- [x] Entity created with well-known GUID
- [x] Properties: IsFirstRun, Theme, HapticFeedbackEnabled, SyncEnabled, etc.
- [x] Factory method: CreateDefault()
- [x] Business methods: CompleteFirstRun(), EnableSync(), UpdateTheme()
- [x] Unit tests for all business methods

**Technical Notes:**
```csharp
// Single-row table with ID: 00000000-0000-0000-0000-000000000001
public static AppSettings CreateDefault();
public void CompleteFirstRun();
public void EnableSync(string provider);
public void UpdateTheme(string theme);
```

---

#### TASK-002: Add sync fields to Project entity
**Status:** ✅ Done  
**Priority:** High  
**Labels:** `phase-1`, `domain`, `database`

**Description:**
Add cloud sync fields to Project entity for Phase 3 preparation.

**Acceptance Criteria:**
- [x] Add LastSyncedAt (DateTime?)
- [x] Add CloudFileId (string?)
- [x] Add SyncVersion (int, default 0)
- [x] Add MarkAsSynced(string cloudFileId) method
- [x] Unit tests for MarkAsSynced()

**Technical Notes:**
Fields are nullable and unused in Phase 1. Will be populated in Phase 3.

---

#### TASK-003: Create database migration
**Status:** 🚧 In Progress  
**Priority:** Critical  
**Labels:** `phase-1`, `database`, `migration`

**Description:**
Create and test EF Core migration for AppSettings table and Project sync fields.

**Acceptance Criteria:**
- [ ] Migration file created: `AddSyncFieldsAndAppSettings`
- [ ] Migration includes AppSettings table creation
- [ ] Migration includes 3 new Project columns
- [ ] Migration tested on clean database (new install)
- [ ] Migration tested on existing database (upgrade scenario)
- [ ] Default AppSettings row inserted automatically

**Commands:**
```bash
cd src/StitchTrack.Infrastructure
dotnet ef migrations add AddSyncFieldsAndAppSettings --startup-project ../StitchTrack.MAUI
dotnet ef database update --startup-project ../StitchTrack.MAUI
```

**Testing:**
1. Delete existing database
2. Run app → database should be created with AppSettings
3. Verify AppSettings row exists with default values

---

#### TASK-004: Create AppSettings repository methods
**Status:** 📅 To Do  
**Priority:** Medium  
**Labels:** `phase-1`, `infrastructure`, `repository`

**Description:**
Add methods to repository to load/save AppSettings (single-row table pattern).

**Acceptance Criteria:**
- [ ] Method: GetAppSettingsAsync() → returns single AppSettings
- [ ] Method: SaveAppSettingsAsync(AppSettings settings)
- [ ] If no AppSettings exists, create default
- [ ] Unit tests with InMemory database

**Technical Notes:**
```csharp
public async Task<AppSettings> GetAppSettingsAsync()
{
    var settings = await _context.AppSettings.FirstOrDefaultAsync();
    if (settings == null)
    {
        settings = AppSettings.CreateDefault();
        _context.AppSettings.Add(settings);
        await _context.SaveChangesAsync();
    }
    return settings;
}
```

---

### 1.2 Bottom Navigation & Placeholder Pages

#### TASK-005: Update AppShell with 5-tab bottom navigation
**Status:** ✅ Done  
**Priority:** High  
**Labels:** `phase-1`, `ui`, `navigation`

**Description:**
Replace existing navigation with 5-tab bottom bar: Counter, Projects, Sessions, Export, Settings.

**Acceptance Criteria:**
- [x] Counter tab (default landing)
- [x] Projects tab
- [x] Sessions tab
- [x] Export tab
- [x] Settings tab
- [x] Custom header with sync status icon
- [x] All tabs navigate correctly

**Technical Notes:**
Use `<TabBar>` with `<ShellContent>` for each tab. Set Counter as default route.

---

#### TASK-006: Create ProjectsPage placeholder
**Status:** ✅ Done  
**Priority:** Medium  
**Labels:** `phase-1`, `ui`, `placeholder`

**Description:**
Create basic ProjectsPage with "Coming soon" message for Phase 1.

**Acceptance Criteria:**
- [x] Page created with XAML and code-behind
- [x] Shows "Projects List" header
- [x] Shows "Coming soon in Phase 2" message
- [x] Navigates from bottom tab

**Note:**
Full implementation in TASK-015.

---

#### TASK-007: Create SessionsPage placeholder
**Status:** ✅ Done  
**Priority:** Low  
**Labels:** `phase-1`, `ui`, `placeholder`

**Description:**
Create basic SessionsPage with "Coming soon" message.

**Acceptance Criteria:**
- [x] Page created with XAML and code-behind
- [x] Shows "Sessions" header
- [x] Shows "Coming soon in Phase 2" message
- [x] Navigates from bottom tab

---

#### TASK-008: Create ExportPage placeholder
**Status:** ✅ Done  
**Priority:** Low  
**Labels:** `phase-1`, `ui`, `placeholder`

**Description:**
Create basic ExportPage with "Coming soon" message.

**Acceptance Criteria:**
- [x] Page created with XAML and code-behind
- [x] Shows "Export Data" header
- [x] Shows "Coming soon in Phase 2" message
- [x] Navigates from bottom tab

---

#### TASK-009: Create SettingsPage placeholder
**Status:** ✅ Done  
**Priority:** Medium  
**Labels:** `phase-1`, `ui`, `placeholder`

**Description:**
Create basic SettingsPage with placeholder sections.

**Acceptance Criteria:**
- [x] Page created with XAML and code-behind
- [x] Shows "Settings" header
- [x] Shows section headers: Backup & Sync, App Settings, About
- [x] Navigates from bottom tab

**Note:**
Full implementation in TASK-020.

---

### 1.3 First-Run Onboarding

#### TASK-010: Create OnboardingCard component
**Status:** 📅 To Do  
**Priority:** High  
**Labels:** `phase-1`, `ui`, `onboarding`

**Description:**
Create reusable onboarding card component shown on first app launch.

**Acceptance Criteria:**
- [ ] Card component with welcome message
- [ ] "Get Started" button → dismisses onboarding
- [ ] "Enable Backup & Sync" button → navigates to Settings
- [ ] "Maybe Later" link → dismisses onboarding
- [ ] Card uses brand colors and Montserrat font
- [ ] Card has subtle shadow/elevation

**Design:**
```
┌───────────────────────────────────────┐
│       🧶 Welcome to StitchTrack       │
│                                       │
│  Start counting rows immediately      │
│      No signup required               │
│                                       │
│  Your projects are saved locally      │
│      and private by default           │
│                                       │
│     [ Get Started 🚀 ]                │
│                                       │
│    Want automatic backup?             │
│  [ Enable Backup & Sync ☁️ ]         │
│                                       │
│        [ Maybe Later ]                │
└───────────────────────────────────────┘
```

---

#### TASK-011: Add onboarding logic to app startup
**Status:** 📅 To Do  
**Priority:** High  
**Labels:** `phase-1`, `ui`, `onboarding`

**Description:**
Show onboarding card on first app launch, then never again.

**Acceptance Criteria:**
- [ ] Load AppSettings on app start
- [ ] If IsFirstRun == true, show OnboardingCard
- [ ] "Get Started" → call CompleteFirstRun(), dismiss card
- [ ] "Enable Backup & Sync" → navigate to Settings, then CompleteFirstRun()
- [ ] "Maybe Later" → call CompleteFirstRun(), dismiss card
- [ ] After first run, app goes directly to Counter tab
- [ ] Tested: onboarding only shows once

**Technical Notes:**
Add to App.xaml.cs or MainPage startup logic.

---

### 1.4 Quick Counter Enhancements

#### TASK-012: Add "Save to Project" button to GuestCounterPage
**Status:** 📅 To Do  
**Priority:** High  
**Labels:** `phase-1`, `ui`, `counter`

**Description:**
Allow users to save their quick counter to a permanent project.

**Acceptance Criteria:**
- [ ] "Save to Project" button added below counter
- [ ] Button has icon (💾) and clear label
- [ ] Button is visually distinct (outlined style)
- [ ] Positioned below Reset button
- [ ] Tapping button shows project name dialog

**Design:**
Place after Reset button with some vertical spacing.

---

#### TASK-013: Implement SaveToProjectCommand in GuestCounterViewModel
**Status:** 📅 To Do  
**Priority:** High  
**Labels:** `phase-1`, `viewmodel`, `counter`

**Description:**
Implement business logic to save quick counter as a new project.

**Acceptance Criteria:**
- [ ] Command: SaveToProjectCommand
- [ ] Shows dialog: "Enter project name"
- [ ] Validates name (not empty, max 200 chars)
- [ ] Creates new Project with Name and CurrentCount
- [ ] Transfers CounterHistory entries to new project
- [ ] Saves project to database
- [ ] Navigates to Projects tab
- [ ] Shows success toast: "Project saved!"
- [ ] Unit tests for command logic

**Edge Cases:**
- Empty name → show validation error
- Duplicate name → allow (show warning?)
- No history entries → still create project

**Technical Notes:**
```csharp
public ICommand SaveToProjectCommand { get; }

private async Task SaveToProjectAsync()
{
    string name = await ShowProjectNameDialogAsync();
    if (string.IsNullOrWhiteSpace(name)) return;
    
    var project = Project.CreateProject(name);
    project.CurrentCount = CurrentCount;
    
    // Transfer history
    foreach (var entry in _counterHistory)
    {
        project.AddCounterHistoryEntry(entry);
    }
    
    await _projectRepository.AddAsync(project);
    await Shell.Current.GoToAsync("//Projects");
}
```

---

#### TASK-014: Add visual hint to Quick Counter
**Status:** 📅 To Do  
**Priority:** Low  
**Labels:** `phase-1`, `ui`, `counter`

**Description:**
Add subtle text hint that quick counter is not saved.

**Acceptance Criteria:**
- [ ] Label: "This counter is not saved to a project"
- [ ] Positioned above counter number
- [ ] Gray text, small font (12pt)
- [ ] Optional: Add info icon (ℹ️) with tooltip

**Design:**
Center-aligned, subtle, not too prominent.

---

### 1.5 Projects List (Basic Implementation)

#### TASK-015: Create ProjectsViewModel
**Status:** 📅 To Do  
**Priority:** High  
**Labels:** `phase-1`, `viewmodel`, `projects`

**Description:**
Create ViewModel to load and display projects.

**Acceptance Criteria:**
- [ ] ObservableCollection<Project> Projects
- [ ] LoadProjectsCommand (async)
- [ ] Load active projects only (IsArchived = false)
- [ ] Sort by UpdatedAt descending (most recent first)
- [ ] Handle empty state (no projects)
- [ ] Show loading indicator while loading
- [ ] Unit tests with mock repository

**Technical Notes:**
```csharp
public class ProjectsViewModel : BaseViewModel
{
    public ObservableCollection<Project> Projects { get; }
    public ICommand LoadProjectsCommand { get; }
    public bool IsEmpty => !Projects.Any();
    
    private async Task LoadProjectsAsync()
    {
        IsLoading = true;
        var projects = await _repository.GetActiveProjectsAsync();
        Projects.Clear();
        foreach (var p in projects.OrderByDescending(x => x.UpdatedAt))
        {
            Projects.Add(p);
        }
        IsLoading = false;
    }
}
```

---

#### TASK-016: Implement ProjectsPage UI
**Status:** 📅 To Do  
**Priority:** High  
**Labels:** `phase-1`, `ui`, `projects`

**Description:**
Replace placeholder ProjectsPage with functional list view.

**Acceptance Criteria:**
- [ ] CollectionView bound to Projects
- [ ] Each item shows: project name, current count, "rows" label
- [ ] Each item shows: last updated time (relative, e.g. "2 hours ago")
- [ ] Tap project → navigate to project detail (Phase 2, show toast for now)
- [ ] Empty state: "No projects yet. Tap + to create one."
- [ ] Loading state: ActivityIndicator
- [ ] Pull-to-refresh
- [ ] Swipe actions: Delete (with confirmation)

**Design:**
```
┌─────────────────────────────────────────────┐
│  Projects                   [+] [🔍]        │
├─────────────────────────────────────────────┤
│                                             │
│  ┌─────────────────────────────────────┐   │
│  │ 🧣 Cozy Scarf                  42   │   │
│  │ Updated 2 hours ago           rows  │   │
│  └─────────────────────────────────────┘   │
│                                             │
│  ┌─────────────────────────────────────┐   │
│  │ 🧤 Winter Mittens              18   │   │
│  │ Updated yesterday             rows  │   │
│  └─────────────────────────────────────┘   │
│                                             │
└─────────────────────────────────────────────┘
```

---

#### TASK-017: Add "New Project" button to ProjectsPage
**Status:** 📅 To Do  
**Priority:** Medium  
**Labels:** `phase-1`, `ui`, `projects`

**Description:**
Add FAB (Floating Action Button) or header button to create new project.

**Acceptance Criteria:**
- [ ] Button visible in header or as FAB
- [ ] Icon: + or "New"
- [ ] Tapping button shows "Create Project" dialog
- [ ] Dialog has text input for project name
- [ ] Validates name (not empty)
- [ ] Creates project with CurrentCount = 0
- [ ] Adds to list immediately
- [ ] Scrolls to new project

**Note:**
Consider FAB (bottom-right corner) for better UX on mobile.

---

#### TASK-018: Implement Delete Project functionality
**Status:** 📅 To Do  
**Priority:** Medium  
**Labels:** `phase-1`, `projects`, `crud`

**Description:**
Allow users to delete projects with confirmation.

**Acceptance Criteria:**
- [ ] Swipe left on project → shows Delete button
- [ ] Tap Delete → shows confirmation dialog
- [ ] Dialog: "Delete [Project Name]? This cannot be undone."
- [ ] Confirm → deletes from database (cascade deletes history)
- [ ] Removes from list immediately
- [ ] Shows success toast: "Project deleted"
- [ ] Cancel → no action
- [ ] Unit test: delete removes project and history

**Technical Notes:**
Use SwipeView or ContextActions for swipe-to-delete.

---

#### TASK-019: Add "last updated" relative time
**Status:** 📅 To Do  
**Priority:** Low  
**Labels:** `phase-1`, `ui`, `helper`

**Description:**
Create helper to show relative time strings (e.g., "2 hours ago", "yesterday").

**Acceptance Criteria:**
- [ ] Helper class: TimeAgoConverter or extension method
- [ ] Converts DateTime to string:
  - < 1 min → "just now"
  - < 60 min → "X minutes ago"
  - < 24 hrs → "X hours ago"
  - Yesterday → "yesterday"
  - < 7 days → "X days ago"
  - Older → "Jan 15, 2025"
- [ ] Used in ProjectsPage item template
- [ ] Unit tests for all time ranges

---

### 1.6 Settings Page (Basic)

#### TASK-020: Implement basic Settings page
**Status:** 📅 To Do  
**Priority:** Medium  
**Labels:** `phase-1`, `ui`, `settings`

**Description:**
Create functional Settings page with app preferences.

**Acceptance Criteria:**
- [ ] Section: Backup & Sync (disabled in Phase 1)
  - [ ] Label: "Cloud Backup (Coming in Phase 3)"
  - [ ] Toggle disabled, set to OFF
- [ ] Section: App Settings
  - [ ] Theme picker: Light / Dark / Auto (default Auto)
  - [ ] Haptic Feedback toggle (default ON)
- [ ] Section: About
  - [ ] App version (e.g., "Version 1.0.0")
  - [ ] Author: Gabriella Frank Ferm
  - [ ] Link: GitHub repo (optional)
- [ ] All settings persist to AppSettings entity
- [ ] Settings apply immediately (theme change, haptic toggle)

**Design:**
```
┌─────────────────────────────────────────────┐
│  Settings                            [Done] │
├─────────────────────────────────────────────┤
│  BACKUP & SYNC                              │
│  ┌─────────────────────────────────────┐   │
│  │ Cloud Backup       [Toggle: OFF 🔒] │   │
│  │ (Coming in Phase 3)                 │   │
│  └─────────────────────────────────────┘   │
│                                             │
│  APP SETTINGS                               │
│  ┌─────────────────────────────────────┐   │
│  │ Theme              Auto          →  │   │
│  └─────────────────────────────────────┘   │
│  ┌─────────────────────────────────────┐   │
│  │ Haptic Feedback    [Toggle: ON]    │   │
│  └─────────────────────────────────────┘   │
│                                             │
│  ABOUT                                      │
│  ┌─────────────────────────────────────┐   │
│  │ Version 1.0.0                       │   │
│  │ Made by Gabriella Frank Ferm        │   │
│  └─────────────────────────────────────┘   │
└─────────────────────────────────────────────┘
```

---

#### TASK-021: Create SettingsViewModel
**Status:** 📅 To Do  
**Priority:** Medium  
**Labels:** `phase-1`, `viewmodel`, `settings`

**Description:**
Create ViewModel to manage settings state and persistence.

**Acceptance Criteria:**
- [ ] Load AppSettings on init
- [ ] Property: SelectedTheme (Light/Dark/Auto)
- [ ] Property: IsHapticEnabled (bool)
- [ ] Command: SaveSettingsCommand
- [ ] Update AppSettings entity on change
- [ ] Save to database immediately
- [ ] Unit tests for setting updates

---

#### TASK-022: Implement theme switching
**Status:** 📅 To Do  
**Priority:** Medium  
**Labels:** `phase-1`, `ui`, `theme`

**Description:**
Allow users to switch between Light, Dark, and Auto themes.

**Acceptance Criteria:**
- [ ] Theme picker in Settings (3 options)
- [ ] Apply theme immediately on selection
- [ ] "Auto" follows system theme
- [ ] Theme persists across app restarts
- [ ] Test on Android (light/dark modes)
- [ ] Test on iOS (light/dark modes)

**Technical Notes:**
```csharp
// In App.xaml.cs
Application.Current.UserAppTheme = theme switch
{
    "Light" => AppTheme.Light,
    "Dark" => AppTheme.Dark,
    _ => AppTheme.Unspecified // Auto
};
```

**Note:**
Full dark theme styling in Phase 2. For now, just enable switching.

---

#### TASK-023: Implement haptic feedback toggle
**Status:** 📅 To Do  
**Priority:** Low  
**Labels:** `phase-1`, `settings`, `haptic`

**Description:**
Allow users to disable haptic feedback if they prefer.

**Acceptance Criteria:**
- [ ] Toggle in Settings
- [ ] Default: ON
- [ ] When OFF, no haptic calls in GuestCounterViewModel
- [ ] Setting persists
- [ ] Test on real device (Android/iOS)

**Technical Notes:**
Add check in ViewModel before calling Vibration.Default.Vibrate():
```csharp
if (_appSettings.HapticFeedbackEnabled)
{
    Vibration.Default.Vibrate();
}
```

---

### 1.7 Sync Status Indicator (UI Only)

#### TASK-024: Add cloud icon to AppShell header
**Status:** 📅 To Do  
**Priority:** Low  
**Labels:** `phase-1`, `ui`, `header`

**Description:**
Add cloud icon to app header to show sync status (hardcoded "not synced" in Phase 1).

**Acceptance Criteria:**
- [ ] Icon in top-right of header
- [ ] Icon: cloud with slash (not synced)
- [ ] Icon size: 24x24 pt
- [ ] Tapping icon shows tooltip: "Sync disabled (enable in Settings)"
- [ ] Icon visible on all tabs

**Note:**
In Phase 3, this will update dynamically (synced, syncing, error).

---

### 1.8 Testing & Polish

#### TASK-025: Write integration tests for Phase 1
**Status:** 📅 To Do  
**Priority:** Medium  
**Labels:** `phase-1`, `testing`, `integration`

**Description:**
Write integration tests for core Phase 1 flows.

**Test Cases:**
- [ ] Test: Create project → saves to database
- [ ] Test: Delete project → removes from database + cascade deletes history
- [ ] Test: Quick counter save to project → transfers count and history
- [ ] Test: AppSettings persists across restarts
- [ ] Test: Theme change persists
- [ ] Test: First-run onboarding shows only once

---

#### TASK-026: Manual testing on Android emulator
**Status:** 📅 To Do  
**Priority:** High  
**Labels:** `phase-1`, `testing`, `manual`

**Testing Checklist:**
- [ ] App launches successfully
- [ ] First-run onboarding shows
- [ ] Onboarding dismisses correctly
- [ ] Quick counter increments/decrements/resets
- [ ] Counter doesn't go below 0
- [ ] Undo works correctly
- [ ] Save to project creates project
- [ ] Projects list loads correctly
- [ ] Delete project works with confirmation
- [ ] Settings theme change works
- [ ] Haptic feedback toggle works
- [ ] App restarts with data intact
- [ ] No crashes or ANR (App Not Responding)

---

#### TASK-027: Manual testing on iOS simulator
**Status:** 📅 To Do  
**Priority:** Medium (if Mac available)  
**Labels:** `phase-1`, `testing`, `manual`, `ios`

**Testing Checklist:**
- [ ] Same as Android checklist
- [ ] Test on iPhone 14 simulator
- [ ] Test on iPad simulator (different layout)
- [ ] Verify Safe Area insets
- [ ] Test haptic feedback on real device

---

#### TASK-028: Fix any layout issues
**Status:** 📅 To Do  
**Priority:** Medium  
**Labels:** `phase-1`, `ui`, `bugfix`

**Common Issues to Check:**
- [ ] Text truncation on small screens
- [ ] Button tap targets too small (<44pt)
- [ ] Overlapping elements
- [ ] Incorrect spacing
- [ ] Safe area violations (notch, home indicator)
- [ ] Landscape mode issues

---

#### TASK-029: Review color usage and branding
**Status:** 📅 To Do  
**Priority:** Low  
**Labels:** `phase-1`, `ui`, `design`

**Description:**
Ensure all UI uses brand colors consistently.

**Brand Colors:**
- Primary: #FE64A3 (Pink)
- Secondary: #424B54 (Dark Gray)
- Accent: #E1AD37 (Gold)
- Background: #E8E8E8 (Light Gray)
- Surface: #FFFFFF (White)

**Check:**
- [ ] All buttons use Primary or Secondary color
- [ ] Headers use correct font weight
- [ ] Spacing is consistent (8pt, 16pt, 24pt increments)
- [ ] Font is Montserrat throughout

---

#### TASK-030: Code review and formatting
**Status:** 📅 To Do  
**Priority:** Medium  
**Labels:** `phase-1`, `code-quality`

**Description:**
Final code cleanup before Phase 1 release.

**Checklist:**
- [ ] Run `dotnet format` on entire solution
- [ ] Fix any StyleCop warnings
- [ ] Remove commented-out code
- [ ] Add XML doc comments to public APIs
- [ ] Ensure all files have copyright header
- [ ] Remove unused usings
- [ ] Check for TODOs and FIXMEs

---

#### TASK-031: Update CHANGELOG.md
**Status:** 📅 To Do  
**Priority:** Low  
**Labels:** `phase-1`, `documentation`

**Description:**
Document Phase 1 changes in CHANGELOG.

**Format:**
```markdown
## [1.0.0] - 2025-01-XX

### Added
- Quick row counter with increment, decrement, reset, undo
- Save quick counter to permanent project
- Projects list with create, view, delete
- Basic settings page (theme, haptic feedback)
- First-run onboarding
- Local SQLite database with EF Core
- Bottom navigation (5 tabs)
- Haptic feedback on button press

### Changed
- Redesigned to local-first architecture (no authentication required)
- Bottom navigation replaces side menu

### Removed
- User authentication (moved to Phase 3)
```

---

#### TASK-032: Add screenshots to documentation
**Status:** 📅 To Do  
**Priority:** Low  
**Labels:** `phase-1`, `documentation`, `assets`

**Description:**
Take and add screenshots to docs/assets/ folder.

**Screenshots Needed:**
- [ ] Onboarding card
- [ ] Quick counter (default state)
- [ ] Quick counter with high count
- [ ] Projects list (with 3-4 projects)
- [ ] Projects list (empty state)
- [ ] Settings page
- [ ] Bottom navigation

**Instructions:**
1. Take screenshots on Android emulator (Pixel 6)
2. Save to docs/assets/screenshots/
3. Update README.md with links
4. Update documentation with links

---

#### TASK-033: Write Phase 1 completion summary
**Status:** 📅 To Do  
**Priority:** Low  
**Labels:** `phase-1`, `documentation`

**Description:**
Write blog post or summary of Phase 1 development (good for portfolio).

**Topics to Cover:**
- Why local-first architecture
- TDD approach and learnings
- Clean architecture benefits
- Challenges faced (e.g., EF Core with MAUI)
- What's next (Phase 2)

**Optional:** Post on dev.to, Medium, or personal blog.

---

## 📸 Phase 2: Enhanced Features

**Goal:** Add photos, notes, sessions, and dark mode to make the app more feature-rich.

**Status:** Not started

---

### 2.1 Project Photos

#### TASK-034: Add camera/photo library permissions
**Priority:** High  
**Labels:** `phase-2`, `permissions`, `media`

**Acceptance Criteria:**
- [ ] Add permissions to AndroidManifest.xml
- [ ] Add permissions to Info.plist (iOS)
- [ ] Request permissions at runtime
- [ ] Handle permission denied gracefully
- [ ] Show rationale dialog if needed

**Permissions:**
- Android: CAMERA, READ_EXTERNAL_STORAGE
- iOS: NSCameraUsageDescription, NSPhotoLibraryUsageDescription

---

#### TASK-035: Create image picker service
**Priority:** High  
**Labels:** `phase-2`, `service`, `media`

**Acceptance Criteria:**
- [ ] Interface: IImagePickerService
- [ ] Method: PickFromCameraAsync() → returns image path
- [ ] Method: PickFromGalleryAsync() → returns image path
- [ ] Platform-specific implementations (Android, iOS)
- [ ] Handle cancellation (user backs out)
- [ ] Compress image to max 1024x1024 px
- [ ] Save to app's local storage
- [ ] Unit tests with mock service

---

#### TASK-036: Add photo to project detail page
**Priority:** Medium  
**Labels:** `phase-2`, `ui`, `projects`

**Acceptance Criteria:**
- [ ] "Add Photo" button on project detail page
- [ ] Dialog: "Take Photo" or "Choose from Gallery"
- [ ] Display photo thumbnail (150x150)
- [ ] Tap photo → full screen view
- [ ] Long-press photo → delete option
- [ ] Photo saves to Project.ImagePath
- [ ] Photo displays in project list (small thumbnail)

---

#### TASK-037: Display project photo in list
**Priority:** Low  
**Labels:** `phase-2`, `ui`, `projects`

**Acceptance Criteria:**
- [ ] Small circular thumbnail (40x40) left of project name
- [ ] Placeholder icon if no photo (🧶)
- [ ] Thumbnail loads asynchronously
- [ ] Cached to avoid re-loading

---

### 2.2 Project Notes

#### TASK-038: Add Notes field to project detail
**Priority:** Medium  
**Labels:** `phase-2`, `ui`, `projects`

**Acceptance Criteria:**
- [ ] Multi-line text editor for notes
- [ ] Max 4000 characters
- [ ] Character counter: "X / 4000"
- [ ] Auto-save on blur or after 1 sec idle
- [ ] Placeholder: "Add notes about your project..."

---

### 2.3 Session Timer

#### TASK-039: Create SessionService
**Priority:** High  
**Labels:** `phase-2`, `service`, `session`

**Acceptance Criteria:**
- [ ] Method: StartSession(Guid projectId)
- [ ] Method: StopSession() → returns Session
- [ ] Track elapsed time in memory
- [ ] Handle app backgrounding (pause timer)
- [ ] Handle app termination (save session)
- [ ] Unit tests for timer logic

---

#### TASK-040: Add Start/Stop timer to counter page
**Priority:** High  
**Labels:** `phase-2`, `ui`, `counter`

**Acceptance Criteria:**
- [ ] "Start Timer" button below counter
- [ ] Button changes to "Stop Timer" when active
- [ ] Display elapsed time while running (e.g., "15:32")
- [ ] Timer continues in background
- [ ] Notification shows timer is running (Android/iOS)
- [ ] Stop timer → saves session to database

---

#### TASK-041: Implement SessionsPage
**Priority:** Medium  
**Labels:** `phase-2`, `ui`, `sessions`

**Acceptance Criteria:**
- [ ] Load all sessions from database
- [ ] Group by project
- [ ] Display: project name, duration, rows completed, date
- [ ] Filter by project (dropdown)
- [ ] Filter by date range (last 7 days, last 30 days, all time)
- [ ] Empty state: "No sessions yet. Start a timer!"

---

### 2.4 Archive Projects

#### TASK-042: Add Archive swipe action
**Priority:** Medium  
**Labels:** `phase-2`, `projects`, `archive`

**Acceptance Criteria:**
- [ ] Swipe right on project → Archive button
- [ ] Tap Archive → project.IsArchived = true
- [ ] Archived projects hidden from main list
- [ ] Show toast: "Project archived"

---

#### TASK-043: Add "Show Archived" toggle
**Priority:** Low  
**Labels:** `phase-2`, `ui`, `projects`

**Acceptance Criteria:**
- [ ] Toggle in header of ProjectsPage
- [ ] ON → shows archived projects (grayed out)
- [ ] Swipe archived project → Unarchive action
- [ ] Unarchive → project.IsArchived = false

---

### 2.5 Progress Tracking

#### TASK-044: Add TotalRows field to project form
**Priority:** Low  
**Labels:** `phase-2`, `projects`, `progress`

**Acceptance Criteria:**
- [ ] Optional field: "Total Rows (optional)"
- [ ] Number input, min 1, max 9999
- [ ] If set, calculate progress: CurrentCount / TotalRows * 100
- [ ] Display progress percentage in list and detail

---

#### TASK-045: Display progress bar
**Priority:** Low  
**Labels:** `phase-2`, `ui`, `projects`

**Acceptance Criteria:**
- [ ] Progress bar in project list item
- [ ] Bar fills based on percentage
- [ ] Color: green for 0-75%, blue for 76-99%, gold for 100%
- [ ] Text: "42 / 100 rows (42%)"

---

### 2.6 Manual Export

#### TASK-046: Implement JSON export
**Priority:** High  
**Labels:** `phase-2`, `export`, `data`

**Acceptance Criteria:**
- [ ] Button: "Export All Projects (JSON)"
- [ ] Serialize all projects to JSON (include sessions, history)
- [ ] Save to device storage
- [ ] Open system share sheet
- [ ] User can save to Files app or share via email/Drive
- [ ] Test: JSON is valid and contains all data

---

#### TASK-047: Implement CSV export
**Priority:** Medium  
**Labels:** `phase-2`, `export`, `data`

**Acceptance Criteria:**
- [ ] Button: "Export All Projects (CSV)"
- [ ] CSV columns: Name, CurrentCount, TotalRows, Notes, CreatedAt
- [ ] Save to device storage
- [ ] Open system share sheet
- [ ] Test: CSV opens correctly in Excel/Sheets

---

#### TASK-048: Implement import projects
**Priority:** Medium  
**Labels:** `phase-2`, `import`, `data`

**Acceptance Criteria:**
- [ ] Button: "Import Projects"
- [ ] File picker for JSON files
- [ ] Deserialize JSON
- [ ] Handle duplicate IDs (generate new GUIDs)
- [ ] Ask: "Merge with existing projects or replace all?"
- [ ] Import into database
- [ ] Show success: "X projects imported"
- [ ] Test with sample JSON file

---

### 2.7 Dark Mode

#### TASK-049: Create Dark theme resource dictionary
**Priority:** Medium  
**Labels:** `phase-2`, `ui`, `theme`

**Acceptance Criteria:**
- [ ] Create App.Dark.xaml with dark colors
- [ ] Define: Background, Surface, Text, Primary, etc.
- [ ] Apply to all pages
- [ ] Test all screens in dark mode
- [ ] Ensure contrast ratios meet accessibility standards

---

#### TASK-050: Update all pages for dark mode
**Priority:** Medium  
**Labels:** `phase-2`, `ui`, `theme`

**Acceptance Criteria:**
- [ ] Use dynamic colors (not hardcoded hex values)
- [ ] Test each page in light and dark mode
- [ ] Fix any contrast issues
- [ ] Update icons if needed (some may need dark variants)

---

## ☁️ Phase 3: Cloud Sync

**Goal:** Add optional cloud sync to iCloud (iOS) and Google Drive (Android).

**Status:** Not started

---

### 3.1 iCloud Sync (iOS)

#### TASK-051: Research CloudKit API
**Priority:** High  
**Labels:** `phase-3`, `research`, `ios`

**Description:**
Research Apple's CloudKit API and how to integrate with .NET MAUI.

**Deliverables:**
- [ ] Document: iCloud sync architecture
- [ ] Proof of concept: Upload/download single file
- [ ] Cost analysis (CloudKit pricing)

---

#### TASK-052: Create ICloudSyncService
**Priority:** High  
**Labels:** `phase-3`, `sync`, `ios`

**Acceptance Criteria:**
- [ ] Interface: ICloudSyncService
- [ ] Method: IsAvailableAsync() → checks if iCloud is enabled
- [ ] Method: UploadProjectAsync(Project project)
- [ ] Method: DownloadProjectAsync(Guid projectId)
- [ ] Method: SyncAllProjectsAsync()
- [ ] Handle network errors gracefully
- [ ] Unit tests with mock

---

#### TASK-053: Implement conflict resolution for iCloud
**Priority:** High  
**Labels:** `phase-3`, `sync`, `conflict`

**Acceptance Criteria:**
- [ ] Compare SyncVersion of local vs. cloud
- [ ] If conflict, show dialog (see TASK-060)
- [ ] User chooses: Keep Local, Use Cloud, Keep Both
- [ ] "Keep Both" → creates duplicate with new GUID

---

### 3.2 Google Drive Sync (Android)

#### TASK-054: Research Google Drive API
**Priority:** High  
**Labels:** `phase-3`, `research`, `android`

**Description:**
Research Google Drive API v3 and OAuth flow for Android.

**Deliverables:**
- [ ] Document: Google Drive sync architecture
- [ ] Proof of concept: OAuth flow + upload/download file
- [ ] Cost analysis (Drive API pricing - likely free for personal use)

---

#### TASK-055: Create GoogleDriveSyncService
**Priority:** High  
**Labels:** `phase-3`, `sync`, `android`

**Acceptance Criteria:**
- [ ] Same interface as ICloudSyncService
- [ ] OAuth 2.0 flow for user consent
- [ ] Upload/download projects as JSON files
- [ ] Handle token refresh
- [ ] Unit tests with mock

---

#### TASK-056: Implement conflict resolution for Drive
**Priority:** High  
**Labels:** `phase-3`, `sync`, `conflict`

**Acceptance Criteria:**
- [ ] Same logic as iCloud (TASK-053)
- [ ] Use same conflict resolution UI (TASK-060)

---

### 3.3 Sync UI

#### TASK-057: Update Settings with sync toggle
**Priority:** High  
**Labels:** `phase-3`, `ui`, `settings`

**Acceptance Criteria:**
- [ ] "Backup & Sync" section enabled
- [ ] Toggle: ON/OFF (default OFF)
- [ ] Provider picker: iCloud (iOS) or Google Drive (Android)
- [ ] "Last Synced" timestamp
- [ ] "Sync Now" button (manual sync)

---

#### TASK-058: Create sync setup wizard
**Priority:** Medium  
**Labels:** `phase-3`, `ui`, `sync`

**Description:**
First-time sync setup flow.

**Steps:**
1. Welcome: "Enable Cloud Backup"
2. Ask: "Upload existing projects?" (Yes / No / Choose)
3. Show progress: "Uploading X of Y projects..."
4. Done: "Backup enabled!"

---

#### TASK-059: Update cloud icon with sync status
**Priority:** Medium  
**Labels:** `phase-3`, `ui`, `sync`

**Acceptance Criteria:**
- [ ] Icon changes based on status:
  - ☁️✓ Synced (green)
  - ☁️↻ Syncing (animated)
  - ☁️⚠ Error (red)
  - ☁️✗ Not synced (gray)
- [ ] Tap icon → shows sync details popup
- [ ] Updates in real-time

---

#### TASK-060: Create ConflictResolutionPage
**Priority:** High  
**Labels:** `phase-3`, `ui`, `sync`

**Acceptance Criteria:**
- [ ] Show local vs. cloud differences:
  - Current count
  - Last modified timestamp
  - Notes (if different)
- [ ] Button: "Keep Local"
- [ ] Button: "Use Cloud"
- [ ] Button: "Keep Both" (creates duplicate)
- [ ] Test with sample conflict

---

### 3.4 Background Sync

#### TASK-061: Implement periodic background sync
**Priority:** Low  
**Labels:** `phase-3`, `sync`, `background`

**Acceptance Criteria:**
- [ ] Sync runs every 5 minutes (when sync enabled)
- [ ] Only syncs if changes detected (SyncVersion incremented)
- [ ] Pauses when device is low on battery
- [ ] Pauses when on cellular (optional setting)
- [ ] Shows notification if conflict needs resolution

---

## 🎨 Phase 4: Polish & Release

**Goal:** Final polish, multi-device testing, app store submission.

**Status:** Not started

---

### 4.1 Dropbox Sync

#### TASK-062: Add Dropbox as sync provider
**Priority:** Medium  
**Labels:** `phase-4`, `sync`, `dropbox`

**Acceptance Criteria:**
- [ ] Dropbox SDK integration
- [ ] OAuth flow
- [ ] Same sync logic as iCloud/Drive
- [ ] Works on both iOS and Android

---

### 4.2 Multiple Counters

#### TASK-063: Allow multiple counters per project
**Priority:** Low  
**Labels:** `phase-4`, `feature`, `counter`

**Acceptance Criteria:**
- [ ] Add/remove counters to project
- [ ] Label each counter (e.g., "Main body", "Sleeves")
- [ ] Each counter has independent count and history
- [ ] Switch between counters in project detail

---

### 4.3 Project Tags

#### TASK-064: Add tags to projects
**Priority:** Low  
**Labels:** `phase-4`, `feature`, `tags`

**Acceptance Criteria:**
- [ ] Create Tag entity
- [ ] Tag picker UI (multi-select)
- [ ] Filter projects by tag
- [ ] Display tags in project list

---

### 4.4 Search & Filter

#### TASK-065: Add search to projects
**Priority:** Medium  
**Labels:** `phase-4`, `feature`, `search`

**Acceptance Criteria:**
- [ ] Search bar in ProjectsPage header
- [ ] Fuzzy search by project name
- [ ] Filter by tag
- [ ] Filter by status (active/archived)
- [ ] Clear filters button

---

### 4.5 Share Progress

#### TASK-066: Create shareable project image
**Priority:** Low  
**Labels:** `phase-4`, `feature`, `share`

**Acceptance Criteria:**
- [ ] Generate image with project name, count, photo
- [ ] Add app branding
- [ ] Share via system share sheet (social media, messaging)

---

### 4.6 App Store Submission

#### TASK-067: Prepare iOS app for App Store
**Priority:** Critical  
**Labels:** `phase-4`, `release`, `ios`

**Checklist:**
- [ ] Create App Store Connect account
- [ ] Prepare app icon (1024x1024)
- [ ] Screenshots (6.5" and 5.5" displays)
- [ ] App description and keywords
- [ ] Privacy policy URL
- [ ] Support URL
- [ ] Age rating questionnaire
- [ ] Submit for review

---

#### TASK-068: Prepare Android app for Play Store
**Priority:** Critical  
**Labels:** `phase-4`, `release`, `android`

**Checklist:**
- [ ] Create Google Play Console account ($25 one-time fee)
- [ ] Prepare app icon and feature graphic
- [ ] Screenshots (phone and tablet)
- [ ] App description and keywords
- [ ] Privacy policy URL
- [ ] Data safety form
- [ ] Content rating questionnaire
- [ ] Submit for review

---

## 📝 Notes

### Task Naming Convention
- TASK-XXX: Each task has a unique ID for easy reference in commits and PRs
- Use format: `git commit -m "feat: implement X (TASK-015)"`

### Priority Levels
- **Critical:** Blocks other work or release
- **High:** Important for current phase
- **Medium:** Nice to have in current phase
- **Low:** Can be deferred to later phase

### Labels
Use GitHub labels to organize issues:
- `phase-1`, `phase-2`, `phase-3`, `phase-4`
- `domain`, `ui`, `viewmodel`, `infrastructure`
- `bug`, `enhancement`, `documentation`
- `testing`, `research`

### Creating Issues from This File
Use this template when creating GitHub issues:

```markdown
**Phase:** Phase X
**Priority:** High/Medium/Low
**Labels:** phase-X, ui, enhancement

## Description
[Copy from TASK-XXX description]

## Acceptance Criteria
- [ ] Criterion 1
- [ ] Criterion 2

## Technical Notes
[Copy any technical notes]

## Related Tasks
- Depends on: TASK-XXX
- Blocks: TASK-XXX
```

---

**Last Updated:** December 2024  
**Total Tasks:** 68 (Phase 1: 33, Phase 2: 17, Phase 3: 11, Phase 4: 7)
