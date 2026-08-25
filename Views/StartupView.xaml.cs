// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

namespace Digitavox.Views;

public partial class StartupView : ContentPage
{
    public StartupView()
    {
        InitializeComponent();
    }

    public void ShowInitializationError()
    {
        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
        StatusLabel.Text = "Não foi possível iniciar o aplicativo.";
        SemanticScreenReader.Announce(StatusLabel.Text);
    }
}
