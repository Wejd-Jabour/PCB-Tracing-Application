using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using PCBTracker.Domain.DTOs;
using PCBTracker.Domain.Entities;

namespace PCBTracker.UI.Views
{
    public partial class ConfirmSkidPage : ContentPage
    {
        private readonly TaskCompletionSource<bool> _tcs = new();

        public Task<bool> WaitForResultAsync() => _tcs.Task;

        public ObservableCollection<BoardDto> Boards { get; } = new();

        public string HeaderText { get; private set; } = string.Empty;

        public string TotalsText { get; private set; } = string.Empty;

        public IRelayCommand ConfirmCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ConfirmSkidPage(Skid currentSkid, System.Collections.Generic.IEnumerable<BoardDto> boards)
        {
            InitializeComponent();

            foreach (var b in boards) Boards.Add(b);

            var count = Boards.Count;
            var shipped = Boards.Count(b => b.IsShipped);
            HeaderText = $"Skid {currentSkid?.SkidName ?? "-"} — Review";
            TotalsText = $"Total Boards: {count}    Shipped: {shipped}    Not Shipped: {count - shipped}";

            ConfirmCommand = new RelayCommand(() => _tcs.TrySetResult(true));
            CancelCommand = new RelayCommand(() => _tcs.TrySetResult(false));

            BindingContext = this;
        }

        protected override bool OnBackButtonPressed()
        {
            _tcs.TrySetResult(false);
            return base.OnBackButtonPressed();
        }
    }
}
