using CommunityToolkit.Mvvm.ComponentModel;

public partial class ConnectionStatusViewModel : ObservableObject
{
    private readonly ConnectionStatusService _service;

    [ObservableProperty]
    private string statusText = "Checking connection...";

    public ConnectionStatusViewModel(ConnectionStatusService service)
    {
        _service = service;
        _ = UpdateStatusAsync();
    }

    public async Task UpdateStatusAsync()
    {
        bool isConnected = await _service.CheckConnectionAsync();
        StatusText = isConnected ? "Connected to DB ✅" : "Not Connected ❌";
    }
}
