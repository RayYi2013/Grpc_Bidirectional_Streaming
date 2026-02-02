using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    private Task? _serverRunningTask;           // ← keep reference to the long-running task
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

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(5001, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                });
            });

            builder.Services.AddGrpc();
            builder.Services.AddSingleton(_chatService);

            // Optional: longer shutdown grace period (default is usually 5s)
            builder.Services.Configure<HostOptions>(opt => 
                opt.ShutdownTimeout = TimeSpan.FromSeconds(10));

            _app = builder.Build();

            _app.MapGrpcService<BidirectionalChatService>();

            // Start the host (non-blocking)
            await _app.StartAsync(ct);

            // Keep a reference to a task that completes when host actually shuts down
            _serverRunningTask = _app.WaitForShutdownAsync();

            // Small delay to ensure it's listening (optional but helpful)
            await Task.Delay(300, ct);

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
        if (_app == null)
        {
            Status = ServerStatus.Stopped;
            return Result.Success();
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));

            // Trigger graceful shutdown
            await _app.StopAsync(cts.Token);

            // Wait until the host has fully shut down (this replaces awaiting RunAsync)
            if (_serverRunningTask != null)
            {
                try
                {
                    await _serverRunningTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException) { }  // normal on timeout/cancel
            }

            _app = null;
            _serverRunningTask = null;
            Status = ServerStatus.Stopped;
            return Result.Success();
        }
        catch (Exception ex)
        {
            // log ex if needed
            return Result.Failure($"Stop failed: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        var r = await StopAsync();
        if (!r.IsSuccess)
        {
            // log error if needed
        }

        _disposed = true;
    }

    // Keep synchronous Dispose for compatibility – but prefer async dispose
    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
