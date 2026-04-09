using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StitchTrack.Application.Commands;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;

namespace StitchTrack.Application.ViewModels;

/// <summary>
/// ViewModel for the Sessions / Stats page.
/// Calculates stats and recent sessions list based on the selected time filter.
/// </summary>
public class SessionsViewModel : INotifyPropertyChanged
{
    private readonly ISessionRepository _sessionRepository;
    private readonly SynchronizationContext? _syncContext;

    // All sessions loaded from DB — filtered in memory on tab switch
    private List<Session> _allSessions = new();

    private SessionFilter _activeFilter = SessionFilter.ThisWeek;
    private bool _isLoading;
    private bool _isEmpty;

    public event PropertyChangedEventHandler? PropertyChanged;

    // ─── Tab filter ──────────────────────────────────────────────

    public bool IsTodaySelected => _activeFilter == SessionFilter.Today;
    public bool IsThisWeekSelected => _activeFilter == SessionFilter.ThisWeek;
    public bool IsThisMonthSelected => _activeFilter == SessionFilter.ThisMonth;
    public bool IsAllSelected => _activeFilter == SessionFilter.All;

    // ─── Stats ───────────────────────────────────────────────────

    public string TotalTime { get; private set; } = "0h 0m";
    public string SessionCount { get; private set; } = "0";
    public string AvgSessionDuration { get; private set; } = "0m";
    public string TotalRowsCompleted { get; private set; } = "0";
    public string MostActiveDay { get; private set; } = string.Empty;
    public bool HasMostActiveDay => !string.IsNullOrEmpty(MostActiveDay);

    // ─── Recent sessions list ────────────────────────────────────

    public ObservableCollection<SessionDisplayItem> RecentSessions { get; } = new();

    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); }
    }

    // True when the filtered list has no sessions
    public bool IsEmpty
    {
        get => _isEmpty;
        private set { _isEmpty = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSessions)); }
    }

    public bool HasSessions => !IsEmpty;

    // ─── Commands ────────────────────────────────────────────────

    public ICommand ShowTodayCommand { get; }
    public ICommand ShowThisWeekCommand { get; }
    public ICommand ShowThisMonthCommand { get; }
    public ICommand ShowAllCommand { get; }
    public ICommand LoadCommand { get; }

    public SessionsViewModel(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _syncContext = SynchronizationContext.Current;

        ShowTodayCommand = new RelayCommand(() => SetFilter(SessionFilter.Today));
        ShowThisWeekCommand = new RelayCommand(() => SetFilter(SessionFilter.ThisWeek));
        ShowThisMonthCommand = new RelayCommand(() => SetFilter(SessionFilter.ThisMonth));
        ShowAllCommand = new RelayCommand(() => SetFilter(SessionFilter.All));
        LoadCommand = new RelayCommand(OnLoad);

        System.Diagnostics.Debug.WriteLine("✅ SessionsViewModel created");
    }

    // ─── Load ────────────────────────────────────────────────────

    public async Task LoadSessionsAsync()
    {
        try
        {
            IsLoading = true;
            System.Diagnostics.Debug.WriteLine("📂 Loading sessions...");

            // Load all sessions with Project included for display names
            _allSessions = (await _sessionRepository.GetAllWithProjectAsync()
                .ConfigureAwait(false)).ToList();

            System.Diagnostics.Debug.WriteLine($"✅ Loaded {_allSessions.Count} sessions");

            UpdateOnUiThread(ApplyFilter);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading sessions: {ex.Message}");
        }
        finally
        {
            UpdateOnUiThread(() => IsLoading = false);
        }
    }

    // ─── Filter ──────────────────────────────────────────────────

    private void SetFilter(SessionFilter filter)
    {
        _activeFilter = filter;

        // Notify all tab selection properties so the underline updates
        OnPropertyChanged(nameof(IsTodaySelected));
        OnPropertyChanged(nameof(IsThisWeekSelected));
        OnPropertyChanged(nameof(IsThisMonthSelected));
        OnPropertyChanged(nameof(IsAllSelected));

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var now = DateTime.Now; // local time
        var filtered = _activeFilter switch
        {
            SessionFilter.Today => _allSessions
                .Where(s => s.StartedAt.ToLocalTime().Date == now.Date),

            SessionFilter.ThisWeek => _allSessions
                .Where(s => s.StartedAt.ToLocalTime() >= StartOfWeek(now)),

            SessionFilter.ThisMonth => _allSessions
                .Where(s => s.StartedAt.ToLocalTime().Month == now.Month
                         && s.StartedAt.ToLocalTime().Year == now.Year),

            _ => _allSessions // All
        };

        var sessionList = filtered.OrderByDescending(s => s.StartedAt).ToList();

        CalculateStats(sessionList);
        BuildRecentList(sessionList);

        IsEmpty = RecentSessions.Count == 0;
    }

    // ─── Stats ───────────────────────────────────────────────────

    private void CalculateStats(List<Session> sessions)
    {
        var count = sessions.Count;
        SessionCount = count.ToString(System.Globalization.CultureInfo.InvariantCulture);

        var totalSeconds = sessions.Sum(s => s.DurationSeconds);
        TotalTime = FormatDuration(TimeSpan.FromSeconds(totalSeconds));

        AvgSessionDuration = count > 0
            ? FormatDuration(TimeSpan.FromSeconds(totalSeconds / count))
            : "0m";

        TotalRowsCompleted = sessions
            .Where(s => s.RowsCompleted.HasValue)
            .Sum(s => s.RowsCompleted!.Value)
            .ToString(System.Globalization.CultureInfo.InvariantCulture);

        // Most active day — day of week with the most sessions
        if (count > 0)
        {
            var mostActiveDay = sessions
                .GroupBy(s => s.StartedAt.ToLocalTime().DayOfWeek)
                .OrderByDescending(g => g.Count())
                .First();

            var dayName = mostActiveDay.Key.ToString();
            var sessionText = mostActiveDay.Count() == 1 ? "session" : "sessions";
            var duration = FormatDuration(
                TimeSpan.FromSeconds(mostActiveDay.Sum(s => s.DurationSeconds)));

            MostActiveDay = $"{dayName} ({mostActiveDay.Count()} {sessionText}, {duration})";
        }
        else
        {
            MostActiveDay = string.Empty;
        }

        OnPropertyChanged(nameof(TotalTime));
        OnPropertyChanged(nameof(SessionCount));
        OnPropertyChanged(nameof(AvgSessionDuration));
        OnPropertyChanged(nameof(TotalRowsCompleted));
        OnPropertyChanged(nameof(MostActiveDay));
        OnPropertyChanged(nameof(HasMostActiveDay));
    }

    // ─── Recent sessions list ────────────────────────────────────

    private void BuildRecentList(List<Session> sessions)
    {
        RecentSessions.Clear();

        foreach (var session in sessions)
        {
            var startLocal = session.StartedAt.ToLocalTime();
            var dateText = startLocal.Date == DateTime.Today
                ? $"Today, {startLocal:HH:mm}"
                : startLocal.ToString("dd MMM, HH:mm",
                    System.Globalization.CultureInfo.InvariantCulture);

            var rowsText = session.RowsCompleted.HasValue
                ? $"Rows: {session.StartingRowCount} → {session.EndingRowCount} (+{session.RowsCompleted} rows)"
                : string.Empty;

            RecentSessions.Add(new SessionDisplayItem
            {
                ProjectName = session.Project?.Name ?? "Unknown project",
                DateText = dateText,
                DurationText = $"Duration: {FormatDuration(TimeSpan.FromSeconds(session.DurationSeconds))}",
                RowsText = rowsText
            });
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private static DateTime StartOfWeek(DateTime date)
    {
        // Week starts on Monday
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours}h {duration.Minutes}m";
        if (duration.TotalMinutes >= 1)
            return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";
        return $"{duration.Seconds}s";
    }

    private void OnLoad() => _ = LoadSessionsAsync();

    private void UpdateOnUiThread(Action action)
    {
        if (_syncContext != null)
            _syncContext.Post(_ => action(), null);
        else
            action();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

/// <summary>
/// Display model for a single session row in the recent sessions list.
/// Keeps the ViewModel clean — no MAUI types, no formatting logic in XAML.
/// </summary>
public class SessionDisplayItem
{
    public string ProjectName { get; init; } = string.Empty;
    public string DateText { get; init; } = string.Empty;
    public string DurationText { get; init; } = string.Empty;
    public string RowsText { get; init; } = string.Empty;
    public bool HasRows => !string.IsNullOrEmpty(RowsText);
}

/// <summary>
/// Time range filter for the stats tabs.
/// </summary>
public enum SessionFilter
{
    Today,
    ThisWeek,
    ThisMonth,
    All
}
