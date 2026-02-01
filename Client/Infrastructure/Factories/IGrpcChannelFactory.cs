using Grpc.Net.Client;

namespace Client.Infrastructure.Factories;

/// <summary>
/// Factory interface for creating gRPC channels (SRP, OCP).
/// Allows easy testing and configuration changes.
/// </summary>
public interface IGrpcChannelFactory
{
    /// <summary>
    /// Creates a configured gRPC channel for the specified address.
    /// </summary>
    GrpcChannel CreateChannel(string address);
}
