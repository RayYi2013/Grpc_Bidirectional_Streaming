using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Client.Application.DTOs;
using Client.Application.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace Client.ViewModels;

/// <summary>
/// Main ViewModel handling UI logic only (SRP, DIP).
/// Depends on service interfaces, not concrete implementations.
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IChatConnectionService _connectionService;
    private readonly IMessageService _messageService;

    [ObservableProperty]
    private string _serverAddress = "http://localhost:5001";

    [ObservableProperty]
    private string _currentMessage = string.Empty;

    [ObservableProperty]
    private string _connectionStatus = "Disconnected";

    [ObservableProperty]
    private Brush _statusColor = new SolidColorBrush(Color.FromRgb(156, 163, 175)); // Gray

    [ObservableProperty]
    private SymbolRegular _statusIcon = SymbolRegular.PlugDisconnected24;

    [ObservableProperty]
    private string _connectButtonText = "Connect";

    public ObservableCollection<ChatMessageDto> Messages { get; } = new ObservableCollection<ChatMessageDto>();

    public bool IsConnected => _connectionService.Status == Application.DTOs.ConnectionStatus.Connected;
    public bool IsDisconnected => _connectionService.Status == Application.DTOs.ConnectionStatus.Disconnected;

    public MainViewModel(IChatConnectionService connectionService, IMessageService messageService)
    {
        _connectionService = connectionService;
        _messageService = messageService;

        // Subscribe to events
        _connectionService.StatusChanged += OnConnectionStatusChanged;
        _messageService.MessageReceived += OnMessageReceived;
    }

    partial void OnCurrentMessageChanged(string value)
    {
        SendMessageCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task ToggleConnectionAsync()
    {
        if (IsConnected)
        {
            await DisconnectAsync();
        }
        else
        {
            await ConnectAsync();
        }
    }

    private async Task ConnectAsync()
    {
        if (string.IsNullOrWhiteSpace(ServerAddress))
            return;

        var result = await _connectionService.ConnectAsync(ServerAddress);
        
        if (result.IsSuccess)
        {
            // Start receiving messages
            _messageService.StartReceiving();

            Messages.Add(new ChatMessageDto
            {
                Content = $"Connected to {ServerAddress}",
                Sender = "System",
                Timestamp = DateTime.Now
            });
        }
        else
        {
            Messages.Add(new ChatMessageDto
            {
                Content = $"Connection failed: {result.ErrorMessage}",
                Sender = "System",
                Timestamp = DateTime.Now
            });
        }
    }

    public async Task DisconnectAsync()
    {
        _messageService.StopReceiving();

        await _connectionService.DisconnectAsync();

        Messages.Add(new ChatMessageDto
        {
            Content = "Disconnected from server",
            Sender = "System",
            Timestamp = DateTime.Now
        });
    }

    [RelayCommand(CanExecute = nameof(CanSendMessage))]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentMessage))
            return;

        var content = CurrentMessage;
        CurrentMessage = string.Empty;

        // Add to local messages immediately
        Messages.Add(new ChatMessageDto
        {
            Content = content,
            Sender = "Client",
            Timestamp = DateTime.Now
        });

        // Send to server
        var result = await _messageService.SendMessageAsync(content);
        
        if (!result.IsSuccess)
        {
            Messages.Add(new ChatMessageDto
            {
                Content = $"Failed to send: {result.ErrorMessage}",
                Sender = "System",
                Timestamp = DateTime.Now
            });
        }
    }

    private bool CanSendMessage() => IsConnected && !string.IsNullOrWhiteSpace(CurrentMessage);

    private void OnConnectionStatusChanged(object? sender, Application.DTOs.ConnectionStatus status)
    {
        ConnectionStatus = status.ToString();
        ConnectButtonText = status == Application.DTOs.ConnectionStatus.Connected ? "Disconnect" : "Connect";

        // Update status color and icon
        (StatusColor, StatusIcon) = status switch
        {
            Application.DTOs.ConnectionStatus.Connected => (new SolidColorBrush(Color.FromRgb(134, 239, 172)), SymbolRegular.PlugConnected24),
            Application.DTOs.ConnectionStatus.Connecting => (new SolidColorBrush(Color.FromRgb(250, 204, 21)), SymbolRegular.ArrowSync24),
            Application.DTOs.ConnectionStatus.Error => (new SolidColorBrush(Color.FromRgb(248, 113, 113)), SymbolRegular.ErrorCircle24),
            _ => (new SolidColorBrush(Color.FromRgb(156, 163, 175)), SymbolRegular.PlugDisconnected24)
        };

        // Notify property changes for computed properties
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(IsDisconnected));
        SendMessageCommand.NotifyCanExecuteChanged();
    }

    private void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        // Update UI on UI thread
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            Messages.Add(e.Message);
        });
    }

    public void Dispose()
    {
        _connectionService.StatusChanged -= OnConnectionStatusChanged;
        _messageService.MessageReceived -= OnMessageReceived;
        _connectionService.Dispose();
    }
}
