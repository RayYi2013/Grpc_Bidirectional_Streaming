using ChatGrpc;
using Grpc.Core;
using Server.Infrastructure.Managers;
using Server.Infrastructure.Mappers;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace Server.Infrastructure.GrpcServices;

/// <summary>
/// gRPC service implementation for bidirectional chat (SRP, DIP).
/// Depends on IClientConnectionManager abstraction.
/// </summary>
public class BidirectionalChatService(IClientConnectionManager connectionManager) 
    : BidirectionalChat.BidirectionalChatBase
{
    private readonly IClientConnectionManager _connectionManager = connectionManager;
    private readonly Subject<ChatMessage> _messageSubject = new();

    public IObservable<ChatMessage> MessageReceivedFromClient => _messageSubject.AsObservable();

    /// <summary>
    /// Handles bidirectional streaming for a single client.
    /// </summary>
    public override async Task Chat(
        IAsyncStreamReader<ChatMessage> requestStream,
        IServerStreamWriter<ChatMessage> responseStream,
        ServerCallContext context)
    {
        // Register client
        var clientId = _connectionManager.RegisterClient(responseStream);

        try
        {
            // Read messages from client and broadcast to all
            await foreach (var message in requestStream.ReadAllAsync(context.CancellationToken))
            {
                // Broadcast to all connected clients (including sender)
                await _connectionManager.BroadcastToAllFromClientAsync(message, clientId, context.CancellationToken);

                // Publish message to subscribers (handled by BroadcastService)
                _messageSubject.OnNext(message);
            }
        }
        finally
        {
            // Unregister client when stream ends
            _connectionManager.UnregisterClient(clientId);
        }
    }

    /// <summary>
    /// Event raised when a message is received from any client.
    /// Used by BroadcastService to notify ViewModel.
    /// </summary>
    // Removed event in favor of reactive IObservable `MessageReceivedFromClient`.
}
