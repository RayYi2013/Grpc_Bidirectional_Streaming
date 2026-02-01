using ChatGrpc;
using Client.Application.DTOs;
using Grpc.Core;
using Grpc.Net.Client;
using System.Runtime.CompilerServices;

namespace Client.Infrastructure.Managers;

/// <summary>
/// Manages gRPC bidirectional stream lifecycle (SRP).
/// Handles request/response streams with thread-safe operations.
/// </summary>
public class GrpcStreamManager : IGrpcStreamManager
{
    private AsyncDuplexStreamingCall<ChatMessage, ChatMessage>? _streamingCall;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private bool _disposed;

    public bool IsStreamActive => _streamingCall != null;

    public async Task<Result> InitializeStreamAsync(GrpcChannel channel)
    {
        try
        {
            var client = new BidirectionalChat.BidirectionalChatClient(channel);
            _streamingCall = client.Chat();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to initialize stream: {ex.Message}");
        }
    }

    public async Task<Result> SendAsync(ChatMessage message)
    {
        if (_streamingCall == null)
            return Result.Failure("Stream not initialized");

        await _sendLock.WaitAsync();
        try
        {
            await _streamingCall.RequestStream.WriteAsync(message);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to send message: {ex.Message}");
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public async IAsyncEnumerable<ChatMessage> ReadMessagesAsync([EnumeratorCancellation] CancellationToken ct)
    {
        if (_streamingCall == null)
            yield break;

        await foreach (var message in _streamingCall.ResponseStream.ReadAllAsync(ct))
        {
            yield return message;
        }
    }

    public async Task CloseStreamAsync()
    {
        if (_streamingCall != null)
        {
            try
            {
                await _streamingCall.RequestStream.CompleteAsync();
            }
            catch
            {
                // Ignore errors during cleanup
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _streamingCall?.Dispose();
        _sendLock.Dispose();
        _disposed = true;
    }
}
