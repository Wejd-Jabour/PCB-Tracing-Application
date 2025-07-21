// PCBTracker.UI/ViewModels/SubmitViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using PCBTracker.Domain.DTOs;
using PCBTracker.Domain.Entities;
using PCBTracker.Services.Interfaces;

namespace PCBTracker.UI.ViewModels
{
    public partial class SubmitViewModel : ObservableObject
    {
        private readonly IBoardService _boardService;

        public SubmitViewModel(IBoardService boardService)
        {
            _boardService = boardService;
            BoardTypes = new ObservableCollection<string>();
            Skids = new ObservableCollection<Skid>();
            PrepDate = DateTime.Today;
        }

        // lookup collections
        public ObservableCollection<string> BoardTypes { get; }
        public ObservableCollection<Skid> Skids { get; }

        // form fields
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        string serialNumber;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        string partNumber;

        [ObservableProperty]
        DateTime prepDate;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        string selectedBoardType;

        [ObservableProperty]
        bool isShipped;

        // this partial method auto-runs when IsShipped changes:
        partial void OnIsShippedChanged(bool value)
            => ShipDate = value ? PrepDate : (DateTime?)null;

        [ObservableProperty]
        DateTime? shipDate;

        // manually-implemented SelectedSkid so we can trigger CanExecute
        Skid _selectedSkid;
        public Skid SelectedSkid
        {
            get => _selectedSkid;
            set
            {
                SetProperty(ref _selectedSkid, value);
                SubmitCommand.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            // load board types
            var types = await _boardService.GetBoardTypesAsync();
            BoardTypes.Clear();
            foreach (var t in types)
                BoardTypes.Add(t);

            // load existing skids
            var skids = await _boardService.GetSkidsAsync();
            Skids.Clear();
            foreach (var s in skids)
                Skids.Add(s);

            // ensure at least one skid exists
            if (!Skids.Any())
                Skids.Add(await _boardService.CreateNewSkidAsync());

            // select the latest
            SelectedSkid = Skids.Last();
        }

        // command can only run when this is true
        public bool CanSubmit()
            => !string.IsNullOrWhiteSpace(SerialNumber)
               && !string.IsNullOrWhiteSpace(PartNumber)
               && !string.IsNullOrWhiteSpace(SelectedBoardType)
               && SelectedSkid != null;

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        public async Task SubmitAsync()
        {
            var dto = new BoardDto
            {
                SerialNumber = SerialNumber,
                PartNumber = PartNumber,
                BoardType = SelectedBoardType,
                PrepDate = PrepDate,
                IsShipped = IsShipped,
                ShipDate = ShipDate,
                SkidID = SelectedSkid.SkidID
            };

            await _boardService.CreateBoardAsync(dto);

            await Application.Current.MainPage
                .DisplayAlert("Success", "Board recorded!", "OK");

            // clear just the SN so scanner can fire again
            SerialNumber = string.Empty;
        }
    }
}
