// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using StitchTrack.Domain.Entities;

namespace StitchTrack.Application.Models;

/// <summary>
/// Represents a file in the project form — either an existing DB record
/// being kept (ExistingId is set) or a new file to be added (ExistingId is null).
///
/// The ViewModel uses this to diff the current file list against the DB:
/// - Existing files not in the list → delete
/// - New files (ExistingId null)    → insert
/// - Existing files still in list   → keep, no action
/// </summary>
public record PendingProjectFile(
    Guid? ExistingId,       // null = new file to add
    string FileName,
    string? FilePath,       // local path — only needed for new files
    long FileSizeBytes,     // only needed for new files (0 for existing)
    string ContentType,
    ProjectFileType FileType
);
