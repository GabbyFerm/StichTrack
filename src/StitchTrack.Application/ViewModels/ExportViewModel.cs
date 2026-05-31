using StitchTrack.Application.Commands;
using StitchTrack.Application.Interfaces;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace StitchTrack.Application.ViewModels;

/// <summary>
/// ViewModel for the Export page.
/// Delegates file writing and share sheet to IExportService (MAUI layer).
/// </summary>
public class ExportViewModel : INotifyPropertyChanged
{
    private readonly IExportService _exportService;
    private readonly IDialogService _dialogService;
    private readonly IImportService _importService;
    private readonly SynchronizationContext? _syncContext;
    private bool _includeArchived;
    private bool _isExporting;
    private bool _isImporting;

    public event PropertyChangedEventHandler? PropertyChanged;

    // ─── Properties ──────────────────────────────────────────────

    /// <summary>
    /// Whether to include archived projects in the export.
    /// Bound to the checkbox on the export page.
    /// </summary>
    public bool IncludeArchived
    {
        get => _includeArchived;
        set
        {
            if (_includeArchived == value) return;
            _includeArchived = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// True while an export is in progress — disables buttons to prevent double-tap.
    /// </summary>
    public bool IsExporting
    {
        get => _isExporting;
        private set
        {
            if (_isExporting == value) return;
            _isExporting = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotExporting));
        }
    }
    public bool IsNotExporting => !_isExporting;

    /// <summary>
    /// True while an import is in progress — disables buttons to prevent double-tap and shows "Importing..." status.
    /// </summary>
    public bool IsImporting
    {
        get => _isImporting;
        private set
        {
            if (_isImporting == value) return;
            _isImporting = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotImporting));
        }
    }
    public bool IsNotImporting => !_isImporting;

    // ─── Commands ────────────────────────────────────────────────

    public ICommand ExportJsonCommand { get; }
    public ICommand ExportCsvCommand { get; }
    public ICommand ImportJsonCommand { get; }

    public ExportViewModel(IExportService exportService, IImportService importService, IDialogService dialogService)
    {
        _syncContext = SynchronizationContext.Current;

        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        ExportJsonCommand = new RelayCommand(OnExportJson);
        ExportCsvCommand = new RelayCommand(OnExportCsv);
        ImportJsonCommand = new RelayCommand(OnImportJson);
    }

    // ─── Export actions ──────────────────────────────────────────

    private void OnExportJson() => _ = ExportJsonAsync();
    private void OnExportCsv() => _ = ExportCsvAsync();

    private async Task ExportJsonAsync()
    {
        if (_isExporting) return;
        try
        {
            IsExporting = true;
            var count = await _exportService.ExportJsonAsync(_includeArchived).ConfigureAwait(false);
            if (count == 0)
                await _dialogService.ShowToastAsync("No projects to export").ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ JSON export error: {ex.Message}");
            await _dialogService.ShowAlertAsync(
                "Export Failed",
                "Could not export projects. Please try again."
            ).ConfigureAwait(false);
        }
        finally
        {
            UpdateOnUiThread(() => IsExporting = false);
        }
    }

    private async Task ExportCsvAsync()
    {
        if (_isExporting) return;
        try
        {
            IsExporting = true;
            var count = await _exportService.ExportCsvAsync(_includeArchived).ConfigureAwait(false);
            if (count == 0)
                await _dialogService.ShowToastAsync("No projects to export").ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ CSV export error: {ex.Message}");
            await _dialogService.ShowAlertAsync(
                "Export Failed",
                "Could not export projects. Please try again."
            ).ConfigureAwait(false);
        }
        finally
        {
            UpdateOnUiThread(() => IsExporting = false);
        }
    }

    // ─── Import action ──────────────────────────────────────────
    private void OnImportJson() => _ = ImportJsonAsync();

    private async Task ImportJsonAsync()
    {
        if (_isImporting) return;
        try
        {
            IsImporting = true;
            var count = await _importService.ImportJsonAsync().ConfigureAwait(false);

            if (count == -1) return; // user cancelled — no toast

            await _dialogService.ShowToastAsync(
                count == 0
                    ? "No projects found in file"
                    : $"Imported {count} project{(count == 1 ? "" : "s")}!")
                .ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ Import error: {ex.Message}");
            await _dialogService.ShowAlertAsync(
                "Import Failed",
                "Could not read the file. Make sure it's a valid StitchTrack JSON export.")
                .ConfigureAwait(false);
        }
        finally
        {
            UpdateOnUiThread(() => IsImporting = false);
        }
    }

    // Helper to raise PropertyChanged on the UI thread if needed

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void UpdateOnUiThread(Action action)
    {
        if (_syncContext != null)
            _syncContext.Post(_ => action(), null);
        else
            action();
    }
}
