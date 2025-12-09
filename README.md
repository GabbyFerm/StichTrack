# StitchTrack

Cross-platform row counter for knitters & crocheters — .NET MAUI · SQLite · Local-First

[![CI status](https://img.shields.io/badge/ci-passing-brightgreen)](#) [![Platform](https://img.shields.io/badge/platform-Android%20%7C%20iOS-blue)](#) [![License](https://img.shields.io/badge/license-Unlicensed-lightgrey)](#)

---

## Overview

StitchTrack is a **local-first**, **privacy-focused** mobile app for knitters and crocheters to track row counts, projects, and sessions. No account required—your data stays on your device unless you choose to back it up to your own cloud storage.

### Core Philosophy
- **Start immediately** — no signup, no friction
- **Privacy by default** — data lives on your device
- **Own your data** — export anytime, sync to your cloud (optional)
- **Offline-first** — works perfectly without internet

---

## Key Features

### Phase 1 (Current - MVP) ✅
- ✅ **Quick Counter** — start counting immediately without creating a project
- ✅ **Project Management** — create, edit, delete projects with row counts
- ✅ **Undo Support** — undo counter changes with full history
- ✅ **Local Storage** — SQLite database, no cloud required
- ✅ **Haptic Feedback** — tactile response on button press
- ✅ **Bottom Navigation** — quick access to Counter, Projects, Sessions, Export, Settings

### Phase 2 (Enhanced Features) 🚧
- 📸 Project photos
- 📝 Notes and comments per project
- ⏱️ Session timer with history
- 📊 Progress tracking (X of Y rows)
- 📦 Archive completed projects
- 🌓 Dark mode

### Phase 3 (Cloud Sync) 📅
- ☁️ iCloud sync (iOS)
- ☁️ Google Drive sync (Android)  
- 📤 Conflict resolution UI
- 🔄 Sync status indicator
- 💾 Automatic backup reminders

### Phase 4 (Polish) 📅
- 🏷️ Project tags
- 🔍 Search and filters
- 📊 Multiple counters per project
- 🎨 Custom themes
- 📱 App Store & Play Store release

---

## Screenshots

> Coming soon — check `docs/assets/` for wireframes

---

## Getting Started (Developers)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022/2023 with .NET MAUI workload
- Android emulator or iOS simulator

### Quick Start
```bash
# Clone the repo
git clone https://github.com/GabbyFerm/StitchTrack.git
cd StitchTrack
git checkout develop

# Restore dependencies
dotnet restore

# Build the solution
dotnet build -c Release

# Run tests
dotnet test

# Format code (optional)
dotnet format
```

### Run the app
**Android (Visual Studio):**
1. Open `StitchTrack.sln`
2. Select Android emulator as target
3. Press F5 to run

**iOS (Mac only):**
1. Open `StitchTrack.sln`
2. Select iOS simulator as target
3. Press F5 to run

---

## Architecture

StitchTrack follows **Clean Architecture** principles:

```
StitchTrack/
├── src/
│   ├── StitchTrack.Domain/        # Core entities, business rules
│   ├── StitchTrack.Application/   # ViewModels, commands, use cases
│   ├── StitchTrack.Infrastructure/# Database, repositories, sync services
│   └── StitchTrack.MAUI/          # UI, views, platform-specific code
└── tests/
    ├── StitchTrack.Domain.Tests/
    ├── StitchTrack.Application.Tests/
    └── StitchTrack.IntegrationTests/
```

**Key Technologies:**
- .NET MAUI (UI framework)
- SQLite + EF Core (local database)
- MVVM pattern (ViewModels + data binding)
- NUnit + FluentAssertions (testing)

---

## Development Workflow

### Branching Strategy
- `main` — production releases only
- `develop` — active development (default PR target)
- `feature/*` — short-lived feature branches
- `bugfix/*` — bug fixes

See [BRANCHING.md](docs/BRANCHING.md) for detailed workflow.

### CI/CD
GitHub Actions automatically:
- ✅ Runs tests on all PRs
- ✅ Enforces code formatting (`.editorconfig`)
- ✅ Builds Android/iOS packages (on `develop` and `main`)
- 🔔 Posts Discord notifications on deploy (optional)

---

## Data & Privacy

### Where is my data stored?
- **Phase 1:** All data stored locally in SQLite on your device
- **Phase 2+:** Optional sync to **your own cloud** (iCloud, Google Drive, Dropbox)

### Do you collect my data?
**No.** StitchTrack does not:
- ❌ Require an account
- ❌ Send data to our servers (we don't have any!)
- ❌ Track your usage
- ❌ Sell your data

Your knitting projects are **yours**. We just help you count rows.

### Can I export my data?
**Yes!** You can export all projects as:
- JSON (full backup)
- CSV (for spreadsheets)

Export is available in Settings → Data Management → Export All Projects.

---

## Roadmap & Status

| Phase | Status | Features | Target |
|-------|--------|----------|--------|
| Phase 1 | ✅ Complete | Quick counter, projects, local storage | Jan 2025 |
| Phase 2 | 🚧 In Progress | Photos, notes, sessions | Mar 2025 |
| Phase 3 | 📅 Planned | Cloud sync (iCloud/Drive) | Jun 2025 |
| Phase 4 | 📅 Planned | Polish, app store release | Sep 2025 |

---

## Contributing

We're not accepting external contributions yet, but you can:
- 🐛 Report bugs via [GitHub Issues](https://github.com/GabbyFerm/StitchTrack/issues)
- 💡 Suggest features (use "Feature Request" template)
- ⭐ Star the repo if you find it useful!

If you want to contribute code, please reach out first: gabbzf@gmail.com

---

## License

This project is currently **unlicensed** — all rights reserved by Gabriella Frank Ferm. If you'd like to use or fork this code, please contact the author for permission.

---

## Contact & Support

**Author:** Gabriella Frank Ferm  
**Email:** gabbzf@gmail.com  
**GitHub:** [@GabbyFerm](https://github.com/GabbyFerm)

---

## Acknowledgments

Built with:
- [.NET MAUI](https://dotnet.microsoft.com/apps/maui) — Microsoft's cross-platform framework
- [SQLite](https://www.sqlite.org/) — Lightweight local database
- [Entity Framework Core](https://docs.microsoft.com/ef/core/) — ORM for database access
- [NUnit](https://nunit.org/) + [FluentAssertions](https://fluentassertions.com/) — Testing frameworks

Special thanks to the .NET community for excellent documentation and support! 🎉
