using FlowHub.Main.ViewModels;

namespace FlowHub.Main.Views.Desktop;

public partial class HomePageD : ContentPage
{
    public readonly HomePageVM viewModel;
    public HomePageD(HomePageVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;
        
    }
    bool _isInitialized;
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if(!_isInitialized)
        {
            await viewModel.DisplayInfo();
            await viewModel.incomeRepo.SynchronizeIncomesAsync();
            _isInitialized = true;
        }
    }
}