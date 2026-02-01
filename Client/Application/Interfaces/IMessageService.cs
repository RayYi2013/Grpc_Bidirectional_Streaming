using Client.Application.DTOs;

namespace Client.Application.Interfaces;

/// <summary>
/// Event arguments for message received events.
/// </summary>
/// <summary>
/// Interface for sending and receiving chat messages (ISP, SRP).
/// Exposes a reactive `IObservable` stream for incoming messages.
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// Stream of incoming messages from the server.
    /// </summary>
    IObservable<ChatMessageDto> Messages { get; }

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
