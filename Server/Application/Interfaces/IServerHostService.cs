using Server.Application.DTOs;

namespace Server.Application.Interfaces;

/// <summary>
/// Interface for hosting gRPC server within WPF (ISP, SRP).
/// Handles server lifecycle management only.
/// </summary>
public interface IServerHostService : IDisposable
{
    /// <summary>
    /// Current server status.
    /// </summary>
    ServerStatus Status { get; }

    /// <summary>
    /// Event raised when server status changes.
    /// </summary>
    event EventHandler<ServerStatus>? StatusChanged;

    /// <summary>
    /// Starts the gRPC server.
    /// </summary>
    Task<Result> StartAsync(CancellationToken ct = default);

    /// <summary>
    /// Stops the gRPC server gracefully.
    /// </summary>
    Task<Result> StopAsync();
}
