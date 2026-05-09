using StitchTrack.Application.Interfaces;
using StitchTrack.Application.Commands;
using System.ComponentModel;
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
    private bool _includeArchived;
    private bool _isExporting;

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

    // Convenience inverse for button IsEnabled binding
    public bool IsNotExporting => !_isExporting;

    // ─── Commands ────────────────────────────────────────────────

    public ICommand ExportJsonCommand { get; }
    public ICommand ExportCsvCommand { get; }

    public ExportViewModel(IExportService exportService, IDialogService dialogService)
    {
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        ExportJsonCommand = new RelayCommand(OnExportJson);
        ExportCsvCommand = new RelayCommand(OnExportCsv);
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
            await _exportService.ExportJsonAsync(_includeArchived).ConfigureAwait(false);
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
            IsExporting = false;
        }
    }

    private async Task ExportCsvAsync()
    {
        if (_isExporting) return;

        try
        {
            IsExporting = true;
            await _exportService.ExportCsvAsync(_includeArchived).ConfigureAwait(false);
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
            IsExporting = false;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
