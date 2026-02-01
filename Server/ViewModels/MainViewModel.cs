using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Server.Application.DTOs;
using Server.Application.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace Server.ViewModels;

/// <summary>
/// Main ViewModel handling UI logic only (SRP, DIP).
/// Depends on service interfaces, not concrete implementations.
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IServerHostService _serverHostService;
    private readonly IBroadcastService _broadcastService;

    [ObservableProperty]
    private string _currentMessage = string.Empty;

    [ObservableProperty]
    private string _serverStatus = "Stopped";

    [ObservableProperty]
    private Brush _statusColor = new SolidColorBrush(Color.FromRgb(156, 163, 175)); // Gray

    [ObservableProperty]
    private SymbolRegular _statusIcon = SymbolRegular.Stop24;

    [ObservableProperty]
    private string _serverButtonText = "Start Server";

    [ObservableProperty]
    private int _connectedClientsCount;

    public ObservableCollection<ChatMessageDto> Messages { get; } = new ObservableCollection<ChatMessageDto>();

    public bool IsServerRunning => _serverHostService.Status == Application.DTOs.ServerStatus.Running;
    public bool IsServerStopped => _serverHostService.Status == Application.DTOs.ServerStatus.Stopped;

    public MainViewModel(IServerHostService serverHostService, IBroadcastService broadcastService)
    {
        _serverHostService = serverHostService;
        _broadcastService = broadcastService;

        // Subscribe to events
        _serverHostService.StatusChanged += OnServerStatusChanged;
        _broadcastService.MessageReceived += OnMessageReceived;
        _broadcastService.ConnectedClientsCountChanged += OnConnectedClientsCountChanged;
    }

    partial void OnCurrentMessageChanged(string value)
    {
        BroadcastMessageCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task ToggleServerAsync()
    {
        if (IsServerRunning)
        {
            await StopServerAsync();
        }
        else
        {
            await StartServerAsync();
        }
    }

    private async Task StartServerAsync()
    {
        var result = await _serverHostService.StartAsync();

        if (result.IsSuccess)
        {
            Messages.Add(new ChatMessageDto
            {
                Content = "Server started on http://localhost:5001",
                Sender = "System",
                Timestamp = DateTime.Now
            });
        }
        else
        {
            Messages.Add(new ChatMessageDto
            {
                Content = $"Failed to start server: {result.ErrorMessage}",
                Sender = "System",
                Timestamp = DateTime.Now
            });
        }
    }

    public async Task StopServerAsync()
    {
        var result = await _serverHostService.StopAsync();

        if (result.IsSuccess)
        {
            Messages.Add(new ChatMessageDto
            {
                Content = "Server stopped",
                Sender = "System",
                Timestamp = DateTime.Now
            });
        }
    }

    [RelayCommand(CanExecute = nameof(CanBroadcastMessage))]
    private async Task BroadcastMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentMessage))
            return;

        var content = CurrentMessage;
        CurrentMessage = string.Empty;

        // Add to local messages immediately
        Messages.Add(new ChatMessageDto
        {
            Content = content,
            Sender = "Server",
            Timestamp = DateTime.Now
        });

        // Broadcast to all clients
        var result = await _broadcastService.BroadcastMessageAsync(content);

        if (!result.IsSuccess)
        {
            Messages.Add(new ChatMessageDto
            {
                Content = $"Failed to broadcast: {result.ErrorMessage}",
                Sender = "System",
                Timestamp = DateTime.Now
            });
        }
    }

    private bool CanBroadcastMessage() => IsServerRunning && !string.IsNullOrWhiteSpace(CurrentMessage);

    private void OnServerStatusChanged(object? sender, Application.DTOs.ServerStatus status)
    {
        ServerStatus = status.ToString();
        ServerButtonText = status == Application.DTOs.ServerStatus.Running ? "Stop Server" : "Start Server";

        // Update status color and icon
        (StatusColor, StatusIcon) = status switch
        {
            Application.DTOs.ServerStatus.Running => (new SolidColorBrush(Color.FromRgb(134, 239, 172)), SymbolRegular.Play24),
            Application.DTOs.ServerStatus.Starting => (new SolidColorBrush(Color.FromRgb(250, 204, 21)), SymbolRegular.ArrowSync24),
            Application.DTOs.ServerStatus.Error => (new SolidColorBrush(Color.FromRgb(248, 113, 113)), SymbolRegular.ErrorCircle24),
            _ => (new SolidColorBrush(Color.FromRgb(156, 163, 175)), SymbolRegular.Stop24)
        };

        // Notify property changes for computed properties
        OnPropertyChanged(nameof(IsServerRunning));
        OnPropertyChanged(nameof(IsServerStopped));
        BroadcastMessageCommand.NotifyCanExecuteChanged();
    }

    private void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        // Update UI on UI thread
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            Messages.Add(e.Message);
        });
    }

    private void OnConnectedClientsCountChanged(object? sender, int count)
    {
        // Update UI on UI thread
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            ConnectedClientsCount = count;
        });
    }

    public void Dispose()
    {
        _serverHostService.StatusChanged -= OnServerStatusChanged;
        _broadcastService.MessageReceived -= OnMessageReceived;
        _broadcastService.ConnectedClientsCountChanged -= OnConnectedClientsCountChanged;
        _serverHostService.Dispose();
    }
}
