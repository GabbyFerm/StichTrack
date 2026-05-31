// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using StitchTrack.Application.Interfaces;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StitchTrack.Application.Commands;

namespace StitchTrack.Application.ViewModels;

/// <summary>
/// ViewModel for the Settings page.
/// Loads AppSettings on appear, persists changes immediately on toggle/select.
/// </summary>
public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly IAppSettingsRepository _settingsRepository;
    private readonly IHapticsService _hapticsService;
    private readonly IDialogService _dialogService;
    private AppSettings? _settings;
    private readonly SynchronizationContext? _syncContext;

    public event PropertyChangedEventHandler? PropertyChanged;

    // ─── Theme ───────────────────────────────────────────────────

    // Each property drives the visual selected state in the segmented control
    public bool IsLightTheme => _settings?.Theme == "Light";
    public bool IsAutoTheme => _settings?.Theme == "Auto";
    public bool IsDarkTheme => _settings?.Theme == "Dark";

    public string ThemeDisplayText => _settings?.Theme switch
    {
        "Light" => "Light",
        "Dark" => "Dark",
        _ => "Auto"
    };

    // ─── Haptics ─────────────────────────────────────────────────

    public bool HapticFeedbackEnabled => _settings?.HapticFeedbackEnabled ?? true;

    // ─── About ───────────────────────────────────────────────────

    public string AppVersion { get; set; } = string.Empty;
#pragma warning disable CA1822
    public string Author => "Gabriella Frank Ferm";
#pragma warning restore CA1822

    // ─── Commands ────────────────────────────────────────────────

    public ICommand SetLightThemeCommand { get; }
    public ICommand SetAutoThemeCommand { get; }
    public ICommand SetDarkThemeCommand { get; }
    public ICommand ToggleHapticFeedbackCommand { get; }
    public ICommand ResetOnboardingCommand { get; }

    /// <summary>
    /// Set by SettingsPage to apply the theme to the running app.
    /// Kept out of ViewModel because Application.Current is a MAUI API.
    /// </summary>
    public Action<string>? ApplyTheme { get; set; }

    public SettingsViewModel(IAppSettingsRepository settingsRepository, IHapticsService hapticsService, IDialogService dialogService)
    {
        _syncContext = SynchronizationContext.Current;
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _hapticsService = hapticsService ?? throw new ArgumentNullException(nameof(hapticsService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        SetLightThemeCommand = new RelayCommand(() => _ = SetThemeAsync("Light"));
        SetAutoThemeCommand = new RelayCommand(() => _ = SetThemeAsync("Auto"));
        SetDarkThemeCommand = new RelayCommand(() => _ = SetThemeAsync("Dark"));
        ToggleHapticFeedbackCommand = new RelayCommand(() => _ = ToggleHapticsAsync());
        ResetOnboardingCommand = new RelayCommand(() => _ = ResetOnboardingAsync());

    }

    // ─── Load ────────────────────────────────────────────────────

    public async Task LoadSettingsAsync()
    {
        try
        {
            _settings = await _settingsRepository.GetAppSettingsAsync().ConfigureAwait(false);

            // Sync haptics service with persisted setting
            _hapticsService.IsEnabled = _settings.HapticFeedbackEnabled;

            // Notify on UI thread so Switch binding updates correctly
            UpdateOnUiThread(() => OnPropertyChanged(string.Empty));

        }
#pragma warning disable CA1031
        catch (Exception)
#pragma warning restore CA1031
        {
        }
    }

    // ─── Theme ───────────────────────────────────────────────────

    private async Task SetThemeAsync(string theme)
    {
        if (_settings == null) return;
        if (_settings.Theme == theme) return;

        _settings.UpdateTheme(theme);

        // Apply immediately to the running app via Page callback
        ApplyTheme?.Invoke(theme);

        await SaveAsync().ConfigureAwait(false);

        // Notify on UI thread so Switch binding updates correctly
        UpdateOnUiThread(() =>
        {
            OnPropertyChanged(nameof(IsLightTheme));
            OnPropertyChanged(nameof(IsAutoTheme));
            OnPropertyChanged(nameof(IsDarkTheme));
            OnPropertyChanged(nameof(ThemeDisplayText));
        });

    }

    // ─── Haptics ─────────────────────────────────────────────────

    private async Task ToggleHapticsAsync()
    {
        if (_settings == null) return;

        var newValue = _settings.ToggleHapticFeedback();

        // Update service immediately so next tap already reflects the change
        _hapticsService.IsEnabled = newValue;

        await SaveAsync().ConfigureAwait(false);

        // Notify on UI thread so Switch binding updates correctly
        UpdateOnUiThread(() => OnPropertyChanged(string.Empty));

    }

    // ─── Onboarding ──────────────────────────────────────────────
    private async Task ResetOnboardingAsync()
    {
        if (_settings == null) return;

        _settings.ResetFirstRun();
        await SaveAsync().ConfigureAwait(false);

        await _dialogService
            .ShowToastAsync("Welcome screen will show on next launch")
            .ConfigureAwait(false);

    }

    // ─── Persist ─────────────────────────────────────────────────

    private async Task SaveAsync()
    {
        if (_settings == null) return;

        try
        {
            await _settingsRepository.SaveAppSettingsAsync(_settings).ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception)
#pragma warning restore CA1031
        {
        }
    }

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
