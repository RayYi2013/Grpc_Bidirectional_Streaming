using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Server.Application.DTOs;
using Server.Application.Interfaces;
using Server.Infrastructure.GrpcServices;

namespace Server.Infrastructure.Services;

/// <summary>
/// Hosts gRPC server using Kestrel within WPF application (SRP, DIP).
/// </summary>
public class KestrelServerHostService : IServerHostService
{
    private WebApplication? _app;
    private ServerStatus _status = ServerStatus.Stopped;
    private readonly BidirectionalChatService _chatService;
    private bool _disposed;

    public ServerStatus Status
    {
        get => _status;
        private set
        {
            if (_status != value)
            {
                _status = value;
                StatusChanged?.Invoke(this, value);
            }
        }
    }

    public event EventHandler<ServerStatus>? StatusChanged;

    public KestrelServerHostService(BidirectionalChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<Result> StartAsync(CancellationToken ct = default)
    {
        if (Status == ServerStatus.Running)
            return Result.Failure("Server is already running");

        Status = ServerStatus.Starting;

        try
        {
            var builder = WebApplication.CreateBuilder(Array.Empty<string>());

            // Configure Kestrel for HTTP/2
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(5001, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                });
            });

            // Add gRPC services
            builder.Services.AddGrpc();
            builder.Services.AddSingleton(_chatService);

            _app = builder.Build();

            // Map gRPC service
            _app.MapGrpcService<BidirectionalChatService>();

            // Start server in background
            _ = _app.RunAsync();

            // Wait a moment to ensure server started
            await Task.Delay(500, ct);

            Status = ServerStatus.Running;
            return Result.Success();
        }
        catch (Exception ex)
        {
            Status = ServerStatus.Error;
            return Result.Failure($"Failed to start server: {ex.Message}");
        }
    }

    public async Task<Result> StopAsync()
    {
        if (_app != null)
        {
            try
            {
                await _app.StopAsync();
                await _app.DisposeAsync();
                _app = null;
                Status = ServerStatus.Stopped;
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to stop server: {ex.Message}");
            }
        }

        Status = ServerStatus.Stopped;
        return Result.Success();
    }

    public void Dispose()
    {
        if (_disposed) return;

        StopAsync().GetAwaiter().GetResult();
        _disposed = true;
    }
}
