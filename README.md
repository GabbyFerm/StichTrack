# StitchTrack

Cross-platform row counter for knitters & crocheters — .NET MAUI · SQLite · Clean Architecture

[![CI status](https://img.shields.io/badge/ci-pending-lightgrey)](#) [![Platform](https://img.shields.io/badge/platform-Android%20%7C%20iOS-lightgrey)](#) [![License](https://img.shields.io/badge/license-Unlicensed-lightgrey)](#)

Short description
-----------------
StitchTrack is a small, offline-first mobile app to help knitters and crocheters track row counts, sessions and project progress. It’s built with .NET MAUI and uses SQLite for local storage (optional cloud sync is planned for later phases).

Key features (Phase 1 / MVP)
- Quick guest counter with + / − / reset and undo.
- Create / edit / delete projects and switch between them.
- Persist projects locally with SQLite.
- Simple, accessible UI optimized for one-handed operation.

Roadmap highlights
- Phase 1 (MVP): Counter, projects, local persistence (done first).
- Phase 2: Images, notes, session timers, archive.
- Phase 3: Authentication + cloud sync, PDF patterns, reminders.
- Phase 4: Polishing, multi-device sync, app store publishing.

Screenshots / wireframes
- See docs/assets/ for wireframes and UI mockups (add images to docs/assets and link them here).

Getting started (developer)
---------------------------
Prerequisites:
- .NET SDK (8.0 or change to the target SDK)
- Visual Studio 2022/2023 with MAUI workloads (or use dotnet CLI + appropriate dev tools)

Quick local dev commands
1. Clone:
   git clone https://github.com/GabbyFerm/StitchTrack.git
   git checkout develop

2. Restore, build and test:
   dotnet restore
   dotnet build -c Release
   dotnet test

3. Format (if you use the repo formatting rules):
   dotnet tool restore
   dotnet format

Development branches
--------------------
- main — production releases
- develop — active development (default PR target)
- feature/* — short-lived feature branches

See BRANCHING.md for full branching strategy and contribution notes.

CI / Workflows
--------------
This repository includes GitHub Actions to:
- Run build & tests on PRs
- Enforce code formatting
- MacOS MAUI build skeleton (unsigned)
- CD workflow to simulate deploys and post Discord notifications

License / visibility
--------------------
This repository currently does not include an open-source license. Copyright remains with the author (Gabriella Frank Ferm). If you'd like permission to reuse or contribute, please contact the author first.

Contributing
------------
- Read docs/BRANCHING.md before opening PRs.
- Run tests and format locally before creating a PR.
- Open small PRs with clear titles and testing steps.

Contact
-------
Author: Gabriella Frank Ferm gabbzf@gmail.com

Acknowledgements
----------------
- Built with .NET MAUI and community packages. See docs for more details.
