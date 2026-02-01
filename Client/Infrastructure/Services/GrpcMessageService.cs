using ChatGrpc;
using Client.Application.DTOs;
using Client.Application.Interfaces;
using Client.Infrastructure.Managers;
using Client.Infrastructure.Mappers;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace Client.Infrastructure.Services;

/// <summary>
/// Message handling implementation using gRPC (SRP, DIP).
/// Depends on IGrpcStreamManager abstraction.
/// </summary>
public class GrpcMessageService : IMessageService
{
    private readonly IGrpcStreamManager _streamManager;
    private CancellationTokenSource? _readCts;
    private Task? _readTask;

    private readonly Subject<ChatMessageDto> _messageSubject = new();

    public IObservable<ChatMessageDto> Messages => _messageSubject.AsObservable();

    public GrpcMessageService(IGrpcStreamManager streamManager)
    {
        _streamManager = streamManager;
    }

    /// <summary>
    /// Starts background task to read messages from the stream.
    /// Should be called after successful connection.
    /// </summary>
    public void StartReceiving()
    {
        if (_readTask != null)
            return;

        _readCts = new CancellationTokenSource();
        _readTask = Task.Run(async () => await ReadMessagesAsync(_readCts.Token));
    }

    /// <summary>
    /// Stops receiving messages.
    /// </summary>
    public void StopReceiving()
    {
        _readCts?.Cancel();
        _readTask = null;
        // optionally signal completion to subscribers
        //_messageSubject.OnCompleted();
    }

    public async Task<Result> SendMessageAsync(string content, CancellationToken ct = default)
    {
        if (!_streamManager.IsStreamActive)
            return Result.Failure("Not connected");

        var message = new ChatMessage
        {
            Content = content,
            Sender = "Client",
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        return await _streamManager.SendAsync(message);
    }

    private async Task ReadMessagesAsync(CancellationToken ct)
    {
        try
        {
            await foreach (var message in _streamManager.ReadMessagesAsync(ct))
            {
                var dto = ChatMessageMapper.ToDto(message);
                _messageSubject.OnNext(dto);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
        catch (Exception)
        {
            // Connection lost or error
        }
    }
}
