using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Naming", "CA1724:Type names should not match namespaces",
    Justification = "App is a standard name for MAUI applications")]

[assembly: SuppressMessage("Design", "CA1515:Consider making public types internal",
    Justification = "MAUI requires public classes for XAML binding")]

// Suppress platform-specific warnings
[assembly: SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix",
    Justification = "AppDelegate is required by iOS/MacCatalyst platform",
    Scope = "type",
    Target = "~T:StitchTrack.App.AppDelegate")]

[assembly: SuppressMessage("Performance", "CA1852:Seal internal types",
    Justification = "Program class is platform entry point and cannot be sealed",
    Scope = "type",
    Target = "~T:StitchTrack.App.Program")]
