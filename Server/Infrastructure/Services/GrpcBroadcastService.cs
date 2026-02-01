using ChatGrpc;
using Server.Application.DTOs;
using Server.Application.Interfaces;
using Server.Infrastructure.GrpcServices;
using Server.Infrastructure.Managers;
using Server.Infrastructure.Mappers;

namespace Server.Infrastructure.Services;

/// <summary>
/// Broadcasting service implementation (SRP, DIP).
/// Depends on IClientConnectionManager and BidirectionalChatService.
/// </summary>
public class GrpcBroadcastService : IBroadcastService
{
    private readonly IClientConnectionManager _connectionManager;
    private readonly BidirectionalChatService _chatService;

    public int ConnectedClientsCount => _connectionManager.ConnectedClientsCount;

    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    public event EventHandler<int>? ConnectedClientsCountChanged;

    public GrpcBroadcastService(
        IClientConnectionManager connectionManager,
        BidirectionalChatService chatService)
    {
        _connectionManager = connectionManager;
        _chatService = chatService;

        // Subscribe to events
        _connectionManager.ConnectedClientsCountChanged += (s, count) =>
            ConnectedClientsCountChanged?.Invoke(this, count);

        _chatService.MessageReceivedFromClient += (s, message) =>
        {
            var dto = ChatMessageMapper.ToDto(message);
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(dto));
        };
    }

    public async Task<Result> BroadcastMessageAsync(string content, CancellationToken ct = default)
    {
        try
        {
            var message = new ChatMessage
            {
                Content = content,
                Sender = "Server",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            await _connectionManager.BroadcastToAllAsync(message, ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Broadcast failed: {ex.Message}");
        }
    }
}
