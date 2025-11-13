```markdown
# StitchTrack Branching Strategy

Purpose
This document explains the branch strategy used by StitchTrack. It is intentionally simple to keep the workflow easy to follow while supporting CI, PR-based reviews, and safe releases.

Branches
- main
  - Production-ready. Only merge tested, released code into main.
  - Protected: require passing CI, no direct pushes, require pull requests.
  - Tags/releases are created from main.

- develop
  - Integration branch for completed features and bug fixes.
  - Base for feature and bugfix branches.
  - Optional protection: require CI to pass.

- feature/<name>
  - Short-lived branches for individual features or user stories.
  - Branch from: develop
  - Merge back into: develop via PR.

- bugfix/<name>
  - For fixing a bug discovered during development.
  - Branch from: develop
  - Merge back into: develop via PR.

- hotfix/<name>
  - Urgent fixes to production.
  - Branch from: main
  - Merge back into: main and develop (or create a PR to merge into both).

- release/<version> (optional)
  - Prepare a release: final testing, bump version, changelog.
  - Branch from: develop
  - Merge into: main (release) and develop after release.

Naming conventions
- Use short, descriptive names:
  - feature/add-row-counter
  - bugfix/fix-crash-on-launch
  - hotfix/0.1.1-memory-leak
  - release/0.1.0

Workflow (example)
1. Create a feature branch
   git checkout -b feature/add-row-counter develop
2. Work locally, commit with clear messages (recommend Conventional Commits)
   feat(ui): add row counter control
3. Push and open a PR targeting develop
   git push -u origin feature/add-row-counter
   Create PR on GitHub (base: develop)
4. Run CI on the PR. Review, then merge (prefer squash merge for a clean history).
5. Once develop has completed features for a release:
   - Create release branch (optional), test, then merge into main and tag a release.

Pull Requests & Reviews
- Use pull requests for all merges into develop and main.
- Even if working alone: use PRs for CI runs, code/behavior review, and to keep a change record.
- Use clear PR titles and a short description with testing steps.

Merging Strategy
- Preferred: squash merges for feature/bugfix PRs to keep history compact.
- Release and hotfix merges into main can be merge commits to preserve context if desired.

Other recommendations
- Protect main (required): require passing CI, require PRs, block direct pushes.
- Optionally protect develop to require CI.
- Use tags and GitHub Releases for published versions.
- Use a CHANGELOG (keep entries per release).

This document can be updated as the project grows.
```