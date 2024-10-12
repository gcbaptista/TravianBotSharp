﻿using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;

namespace MainCore.UI.ViewModels
{
    [RegisterSingleton<MainViewModel>]
    public class MainViewModel : ViewModelBase
    {
        private MainLayoutViewModel _mainLayoutViewModel;
        private readonly IWaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IMediator _mediator;

        public MainViewModel(IMediator mediator, IWaitingOverlayViewModel waitingOverlayViewModel)
        {
            _mediator = mediator;
            _waitingOverlayViewModel = waitingOverlayViewModel;

            Load = ReactiveCommand.CreateFromTask(LoadHandler);
            Unload = ReactiveCommand.CreateFromTask(UnloadHandler);
        }

        private async Task LoadHandler()
        {
            await _waitingOverlayViewModel.Show();
            await _mediator.Publish(new MainWindowLoaded());

            await _waitingOverlayViewModel.ChangeMessage("loading program layout");
            MainLayoutViewModel = Locator.Current.GetService<MainLayoutViewModel>();
            await MainLayoutViewModel.Load();
            await _waitingOverlayViewModel.Hide();
        }

        private async Task UnloadHandler()
        {
            await _mediator.Publish(new MainWindowUnloaded());
        }

        public MainLayoutViewModel MainLayoutViewModel
        {
            get => _mainLayoutViewModel;
            set => this.RaiseAndSetIfChanged(ref _mainLayoutViewModel, value);
        }

        public ReactiveCommand<Unit, Unit> Load { get; }
        public ReactiveCommand<Unit, Unit> Unload { get; }
    }
}