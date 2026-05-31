// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using StitchTrack.Application.ViewModels;

namespace StitchTrack.MAUI.Views;

/// <summary>
/// Export page — allows users to export project data as JSON or CSV files.
/// All business logic lives in ExportViewModel.
/// This code-behind only wires the BindingContext; the Page is presentation-only.
/// </summary>
public partial class ExportPage : ContentPage
{
    public ExportPage(ExportViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }
}
