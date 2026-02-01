using ChatGrpc;
using Client.Application.DTOs;
using Grpc.Core;
using Grpc.Net.Client;

namespace Client.Infrastructure.Managers;

/// <summary>
/// Interface for managing gRPC bidirectional stream lifecycle (ISP, SRP).
/// </summary>
public interface IGrpcStreamManager : IDisposable
{
    /// <summary>
    /// Initializes the bidirectional streaming call.
    /// </summary>
    Task<Result> InitializeStreamAsync(GrpcChannel channel);

    /// <summary>
    /// Sends a message through the stream.
    /// </summary>
    Task<Result> SendAsync(ChatMessage message);

    /// <summary>
    /// Reads messages from the response stream asynchronously.
    /// </summary>
    IAsyncEnumerable<ChatMessage> ReadMessagesAsync(CancellationToken ct);

    /// <summary>
    /// Closes the stream gracefully.
    /// </summary>
    Task CloseStreamAsync();

    /// <summary>
    /// Indicates if the stream is currently active.
    /// </summary>
    bool IsStreamActive { get; }
}
