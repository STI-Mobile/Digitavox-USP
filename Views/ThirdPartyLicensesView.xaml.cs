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
