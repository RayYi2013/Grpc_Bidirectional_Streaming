using ChatGrpc;
using Grpc.Core;
using System.Collections.Concurrent;

namespace Server.Infrastructure.Managers;

/// <summary>
/// Manages client connections with thread-safe operations (SRP).
/// </summary>
public class ClientConnectionManager : IClientConnectionManager
{
    private readonly ConcurrentDictionary<Guid, IServerStreamWriter<ChatMessage>> _clients = new();

    public int ConnectedClientsCount => _clients.Count;

    public event EventHandler<int>? ConnectedClientsCountChanged;

    public Guid RegisterClient(IServerStreamWriter<ChatMessage> responseStream)
    {
        var clientId = Guid.NewGuid();
        _clients.TryAdd(clientId, responseStream);
        ConnectedClientsCountChanged?.Invoke(this, ConnectedClientsCount);
        return clientId;
    }

    public void UnregisterClient(Guid clientId)
    {
        _clients.TryRemove(clientId, out _);
        ConnectedClientsCountChanged?.Invoke(this, ConnectedClientsCount);
    }

    public async Task BroadcastToAllAsync(ChatMessage message, CancellationToken ct = default)
    {
        var failedClients = new List<Guid>();

        foreach (var (clientId, stream) in _clients)
        {
            try
            {
                await stream.WriteAsync(message, ct);
            }
            catch
            {
                // Mark client for removal if write fails
                failedClients.Add(clientId);
            }
        }

        // Remove failed clients
        foreach (var clientId in failedClients)
        {
            UnregisterClient(clientId);
        }
    }
}
