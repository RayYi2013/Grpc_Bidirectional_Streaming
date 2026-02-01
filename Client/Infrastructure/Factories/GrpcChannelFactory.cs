using Grpc.Net.Client;
using System.Net.Http;

namespace Client.Infrastructure.Factories;

/// <summary>
/// Factory implementation for creating gRPC channels (SRP).
/// Centralizes channel configuration.
/// </summary>
public class GrpcChannelFactory : IGrpcChannelFactory
{
    public GrpcChannel CreateChannel(string address)
    {
        // Configure for HTTP/2 unencrypted (development mode)
        var httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        return GrpcChannel.ForAddress(address, new GrpcChannelOptions
        {
            HttpHandler = httpHandler,
            MaxReceiveMessageSize = 5 * 1024 * 1024, // 5 MB
            MaxSendMessageSize = 5 * 1024 * 1024
        });
    }
}
