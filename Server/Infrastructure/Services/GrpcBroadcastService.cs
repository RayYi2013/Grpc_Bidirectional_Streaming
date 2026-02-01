using ChatGrpc;
using Server.Application.DTOs;
using Server.Application.Interfaces;
using Server.Infrastructure.GrpcServices;
using Server.Infrastructure.Managers;
using Server.Infrastructure.Mappers;
using System.Reactive.Linq;

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

    
    private readonly IObservable<ChatMessageDto> _messages;
    public IObservable<ChatMessageDto> Messages => _messages;
    
    public IObservable<int> ConnectedClientsCountChanged => _connectionManager.ConnectedClientsCountChanged;

    public GrpcBroadcastService(
        IClientConnectionManager connectionManager,
        BidirectionalChatService chatService)
    {
        _connectionManager = connectionManager;
        _chatService = chatService;

        // Map chat service messages to DTO stream
        _messages = _chatService.MessageReceivedFromClient.Select(m => ChatMessageMapper.ToDto(m));
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
