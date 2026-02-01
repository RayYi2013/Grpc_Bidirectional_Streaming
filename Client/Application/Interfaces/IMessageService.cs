using Client.Application.DTOs;

namespace Client.Application.Interfaces;

/// <summary>
/// Event arguments for message received events.
/// </summary>
public class MessageReceivedEventArgs(ChatMessageDto message) : EventArgs
{
    public ChatMessageDto Message { get; } = message;
}

/// <summary>
/// Interface for sending and receiving chat messages (ISP, SRP).
/// Separated from connection management for better testability.
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// Event raised when a message is received from the server.
    /// </summary>
    event EventHandler<MessageReceivedEventArgs>? MessageReceived;

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    Task<Result> SendMessageAsync(string content, CancellationToken ct = default);

    /// <summary>
    /// Starts receiving messages from the server.
    /// </summary>
    void StartReceiving();

    /// <summary>
    /// Stops receiving messages from the server.
    /// </summary>
    void StopReceiving();
}
