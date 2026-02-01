using ChatGrpc;
using Grpc.Core;
using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace Server.Infrastructure.Managers;

/// <summary>
/// Manages client connections with thread-safe operations (SRP).
/// </summary>
public class ClientConnectionManager : IClientConnectionManager
{
    private readonly ConcurrentDictionary<Guid, IServerStreamWriter<ChatMessage>> _clients = new();

    public int ConnectedClientsCount => _clients.Count;
    private readonly BehaviorSubject<int> _countSubject = new(0);
    public IObservable<int> ConnectedClientsCountChanged => _countSubject.AsObservable();

    public Guid RegisterClient(IServerStreamWriter<ChatMessage> responseStream)
    {
        var clientId = Guid.NewGuid();
        _clients.TryAdd(clientId, responseStream);
        _countSubject.OnNext(ConnectedClientsCount);
        return clientId;
    }

    public void UnregisterClient(Guid clientId)
    {
        _clients.TryRemove(clientId, out _);
        _countSubject.OnNext(ConnectedClientsCount);
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

    public async Task BroadcastToAllFromClientAsync(ChatMessage message, Guid fromClientId, CancellationToken ct = default)
    {
        var failedClients = new List<Guid>();

        foreach (var (clientId, stream) in _clients)
        {
            if (clientId == fromClientId)
                continue;

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
