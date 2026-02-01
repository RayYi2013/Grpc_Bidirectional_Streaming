using ChatGrpc;
using Grpc.Core;

namespace Server.Infrastructure.Managers;

/// <summary>
/// Interface for managing client connections (ISP, SRP).
/// </summary>
public interface IClientConnectionManager
{
    /// <summary>
    /// Registers a new client connection.
    /// </summary>
    Guid RegisterClient(IServerStreamWriter<ChatMessage> responseStream);

    /// <summary>
    /// Unregisters a client connection.
    /// </summary>
    void UnregisterClient(Guid clientId);

    /// <summary>
    /// Broadcasts a message to all connected clients.
    /// </summary>
    Task BroadcastToAllAsync(ChatMessage message, CancellationToken ct = default);

    /// <summary>
    /// Number of currently connected clients.
    /// </summary>
    int ConnectedClientsCount { get; }

    /// <summary>
    /// Event raised when connected clients count changes.
    /// </summary>
    event EventHandler<int>? ConnectedClientsCountChanged;
}
