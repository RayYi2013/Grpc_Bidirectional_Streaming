using Client.Application.DTOs;
using Client.Application.Interfaces;
using Client.Infrastructure.Factories;
using Client.Infrastructure.Managers;
using Grpc.Net.Client;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace Client.Infrastructure.Services;

/// <summary>
/// Connection management implementation using gRPC (SRP, DIP).
/// Depends on abstractions (IGrpcChannelFactory, IGrpcStreamManager).
/// </summary>
public class GrpcChatConnectionService : IChatConnectionService
{
    private readonly IGrpcChannelFactory _channelFactory;
    private readonly IGrpcStreamManager _streamManager;
    private GrpcChannel? _channel;
    private ConnectionStatus _status = ConnectionStatus.Disconnected;
    private bool _disposed;
    private readonly BehaviorSubject<ConnectionStatus> _statusSubject;

    public ConnectionStatus Status
    {
        get => _status;
        private set
        {
            if (_status != value)
            {
                _status = value;
                _statusSubject.OnNext(_status);
            }
        }
    }

    public IObservable<ConnectionStatus> StatusChanged => _statusSubject.AsObservable();

    public GrpcChatConnectionService(IGrpcChannelFactory channelFactory, IGrpcStreamManager streamManager)
    {
        _channelFactory = channelFactory;
        _streamManager = streamManager;
        _statusSubject = new BehaviorSubject<ConnectionStatus>(_status);
    }

    public async Task<Result> ConnectAsync(string serverAddress, CancellationToken ct = default)
    {
        if (Status == ConnectionStatus.Connected)
            return Result.Failure("Already connected");

        Status = ConnectionStatus.Connecting;

        try
        {
            // Create channel
            _channel = _channelFactory.CreateChannel(serverAddress);

            // Initialize stream
            var result = await _streamManager.InitializeStreamAsync(_channel);
            if (!result.IsSuccess)
            {
                Status = ConnectionStatus.Error;
                return result;
            }

            Status = ConnectionStatus.Connected;
            return Result.Success();
        }
        catch (Exception ex)
        {
            Status = ConnectionStatus.Error;
            return Result.Failure($"Connection failed: {ex.Message}");
        }
    }

    public async Task DisconnectAsync()
    {
        if (_channel != null)
        {
            await _streamManager.CloseStreamAsync();
            await _channel.ShutdownAsync();
            _channel.Dispose();
            _channel = null;
        }

        Status = ConnectionStatus.Disconnected;
    }

    public void Dispose()
    {
        if (_disposed) return;

        DisconnectAsync().GetAwaiter().GetResult();
        _statusSubject.Dispose();
        _disposed = true;
    }
}
