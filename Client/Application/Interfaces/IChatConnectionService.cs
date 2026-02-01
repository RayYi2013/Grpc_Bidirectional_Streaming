using Client.Application.DTOs;

namespace Client.Application.Interfaces;

/// <summary>
/// Interface for managing chat connection lifecycle (ISP, SRP).
/// Separated from messaging concerns for better testability and flexibility.
/// </summary>
public interface IChatConnectionService : IDisposable
{
    /// <summary>
    /// Current connection status.
    /// </summary>
    ConnectionStatus Status { get; }

    /// <summary>
    /// Observable stream that emits connection status updates.
    /// </summary>
    IObservable<ConnectionStatus> StatusChanged { get; }

    /// <summary>
    /// Establishes connection to the gRPC server.
    /// </summary>
    Task<Result> ConnectAsync(string serverAddress, CancellationToken ct = default);

    /// <summary>
    /// Disconnects from the gRPC server gracefully.
    /// </summary>
    Task DisconnectAsync();
}
