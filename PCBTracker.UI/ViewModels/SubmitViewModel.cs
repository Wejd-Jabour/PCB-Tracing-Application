// PCBTracker.UI/ViewModels/SubmitViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
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

        public ObservableCollection<string> BoardTypes { get; }
        public ObservableCollection<Skid> Skids { get; }

        [ObservableProperty]
        string selectedBoardType;

        [ObservableProperty]
        string serialNumber;

        [ObservableProperty]
        string partNumber;

        [ObservableProperty]
        DateTime prepDate;

        [ObservableProperty]
        bool isShipped;

        // Manually-implemented SelectedSkid property
        private Skid _selectedSkid;
        public Skid SelectedSkid
        {
            get => _selectedSkid;
            set => SetProperty(ref _selectedSkid, value);
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            var types = await _boardService.GetBoardTypesAsync();
            BoardTypes.Clear();
            foreach (var t in types)
                BoardTypes.Add(t);


            // Load existing skids into the picker
            Skids.Clear();
            foreach (var s in await _boardService.GetSkidsAsync())
                Skids.Add(s);

            // If there are no skids yet, create the first one
            if (!Skids.Any())
            {
                var first = await _boardService.CreateNewSkidAsync();
                Skids.Add(first);
            }

            // Select the last skid by default
            SelectedSkid = Skids.Last();
        }

        [RelayCommand]
        async Task ChangeSkidAsync()
        {
            // Create & persist the next skid
            var next = await _boardService.CreateNewSkidAsync();
            Skids.Add(next);
            SelectedSkid = next;
        }

        [RelayCommand]
        async Task SubmitAsync()
        {
            if (string.IsNullOrWhiteSpace(SerialNumber)
             || string.IsNullOrWhiteSpace(PartNumber)
             || string.IsNullOrWhiteSpace(SelectedBoardType)
             || SelectedSkid == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    "Fill out all fields", "OK");
                return;
            }

            var board = new Board
            {
                SerialNumber = SerialNumber,
                PartNumber = PartNumber,
                BoardType = SelectedBoardType,
                PrepDate = PrepDate,
                ShipDate = IsShipped ? DateTime.Now : (DateTime?)null,
                IsShipped = IsShipped,
                SkidID = SelectedSkid.SkidID
            };
            await _boardService.SubmitBoardAsync(board);

            await Application.Current.MainPage
                .DisplayAlert("Success", "Board submitted", "OK");
        }
    }

}
