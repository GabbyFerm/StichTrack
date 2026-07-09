# StitchTrack

Row counter for knitters & crocheters — .NET MAUI · SQLite · Local-First

[![CI status](https://img.shields.io/badge/ci-passing-brightgreen)](#) [![Platform](https://img.shields.io/badge/platform-Android-blue)](#) [![License](https://img.shields.io/badge/license-Unlicensed-lightgrey)](#)

---

## Overview

StitchTrack is a **local-first**, **privacy-focused** mobile app for knitters and crocheters to track row counts, projects, and sessions. No account required — your data stays on your device.

### Core Philosophy
- **Start immediately** — no signup, no friction
- **Privacy by default** — data lives on your device, nowhere else
- **Own your data** — export and import anytime
- **Offline-first** — works perfectly without internet

---

## Features

### Counters
- ✅ **Quick Counter** — start counting immediately without creating a project
- ✅ **Multiple counters per project** — track different sections independently
- ✅ **Row notes** — add a note to any row on the main counter
- ✅ **Undo support** — full undo history for counter changes
- ✅ **Haptic feedback** — tactile response on button press (toggleable in Settings)

### Projects
- ✅ **Project management** — create, edit, and delete projects
- ✅ **Colour tags** — quickly identify projects at a glance
- ✅ **Project tags** — organise by type, status, or anything you like
- ✅ **Needle/hook size** — store the tools used per project
- ✅ **Total rows** — track overall project length
- ✅ **Notes** — freeform notes per project
- ✅ **Photos** — cover photo, pattern files, and inspiration images

### Sessions & Stats
- ✅ **Session tracking** — log crafting sessions per project
- ✅ **Statistics dashboard** — filter by today, this week, this month, or all time

### Data
- ✅ **Export to JSON** — full backup of projects, counters, and session history
- ✅ **Export to CSV** — for spreadsheets and external tools
- ✅ **Import from JSON** — fully restore projects, counters, and session history

### Settings
- ✅ **Theme** — light and dark mode
- ✅ **Haptic feedback toggle**
- ✅ **About section**

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
- .NET MAUI — cross-platform UI framework
- SQLite + Entity Framework Core — local database
- MVVM — ViewModels and data binding
- NUnit + FluentAssertions — testing

---

## Getting Started (Developers)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022/2023 with .NET MAUI workload
- Android emulator or physical device

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
2. Select an Android emulator or connected device as target
3. Press F5 to run

---

## Development Workflow

### Branching Strategy
- `main` — production releases only
- `develop` — active development (default PR target)
- `feature/*` — short-lived feature branches
- `bugfix/*` — bug fixes

### CI/CD
GitHub Actions automatically:
- ✅ Runs tests on all PRs
- ✅ Enforces code formatting (`.editorconfig`)
- ✅ Builds Android packages on `develop` and `main`

---

## Data & Privacy

### Where is my data stored?
All data is stored locally in SQLite on your device. Nothing is ever sent to a server.

### Do you collect my data?
**No.** StitchTrack does not:
- ❌ Require an account
- ❌ Send data to any server
- ❌ Track your usage
- ❌ Sell your data

Your projects are **yours**.

### Can I export my data?
**Yes.** Export all projects, counters, and session history from Settings → Data Management → Export. Import JSON to fully restore everything on a new device.

---

## Roadmap

| Phase | Status | Features | Released |
|-------|--------|----------|--------|
| Phase 1 | ✅ Complete | Quick counter, projects, local storage | Sept 2025 |
| Phase 2 | ✅ Complete | Photos, notes, sessions | Dec 2025 |
| Phase 3 | ✅ Complete | Multiple counters, enhancements, bugfixes | April 2026 |
| Phase 4 | ✅ Complete | Polish, Google Play release | May 2026 |

---

## Download

**[StitchTrack on Google Play](https://play.google.com/store/apps/details?id=com.gabbyferm.stitchtrack)**

---

## License

This project is currently **unlicensed** — all rights reserved by Gabriella Frank Ferm. If you'd like to use or fork this code, please get in touch.

---

## Contact

**Gabriella Frank Ferm**  
📧 gabbzf@gmail.com  
💻 [@GabbyFerm](https://github.com/GabbyFerm)

---

Built with ❤️ using .NET MAUI
