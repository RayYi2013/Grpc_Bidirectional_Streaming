using Server.Application.DTOs;

namespace Server.Application.Interfaces;

/// <summary>
/// Event arguments for message received events.
/// </summary>
public class MessageReceivedEventArgs(ChatMessageDto message) : EventArgs
{
    public ChatMessageDto Message { get; } = message;
}

/// <summary>
/// Interface for broadcasting messages to all connected clients (ISP, SRP).
/// Separated from server hosting for better testability.
/// </summary>
public interface IBroadcastService
{
    /// <summary>
    /// Stream of messages received from clients (reactive).
    /// </summary>
    IObservable<ChatMessageDto> Messages { get; }

    /// <summary>
    /// Broadcasts a message to all connected clients.
    /// </summary>
    Task<Result> BroadcastMessageAsync(string content, CancellationToken ct = default);

    /// <summary>
    /// Number of currently connected clients.
    /// </summary>
    int ConnectedClientsCount { get; }

    /// <summary>
    /// Observable stream for connected clients count changes.
    IObservable<int> ConnectedClientsCountChanged { get; }
}
