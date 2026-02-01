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
    /// Event raised when a message is received from any client.
    /// </summary>
    event EventHandler<MessageReceivedEventArgs>? MessageReceived;

    /// <summary>
    /// Broadcasts a message to all connected clients.
    /// </summary>
    Task<Result> BroadcastMessageAsync(string content, CancellationToken ct = default);

    /// <summary>
    /// Number of currently connected clients.
    /// </summary>
    int ConnectedClientsCount { get; }

    /// <summary>
    /// Event raised when connected clients count changes.
    /// </summary>
    event EventHandler<int>? ConnectedClientsCountChanged;
}
