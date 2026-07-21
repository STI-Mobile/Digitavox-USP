// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Digitavox.Models;
using Digitavox.ViewModels;

namespace Digitavox.Views;

public partial class ThirdPartyLicensesView : ContentPage, IOnPageKeyPress
{
    public ThirdPartyLicensesView(ThirdPartyLicensesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public bool OnPageKeyDown(int keyCode)
    {
        return ((ThirdPartyLicensesViewModel)BindingContext).OnPageKeyDown(keyCode);
    }

    public bool OnPageKeyPress(int keyCode, int modifiers)
    {
        return ((ThirdPartyLicensesViewModel)BindingContext).OnPageKeyPress(keyCode, modifiers);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ((ThirdPartyLicensesViewModel)BindingContext).LoadAsync();
    }
}
