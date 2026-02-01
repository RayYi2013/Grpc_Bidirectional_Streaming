using ChatGrpc;
using Client.Application.DTOs;

namespace Client.Infrastructure.Mappers;

/// <summary>
/// Maps between gRPC protobuf types and application DTOs (SRP).
/// Centralizes conversion logic for maintainability.
/// </summary>
public static class ChatMessageMapper
{
    /// <summary>
    /// Maps from DTO to gRPC protobuf message.
    /// </summary>
    public static ChatMessage ToProto(ChatMessageDto dto)
    {
        return new ChatMessage
        {
            Content = dto.Content,
            Sender = dto.Sender,
            Timestamp = new DateTimeOffset(dto.Timestamp).ToUnixTimeMilliseconds()
        };
    }

    /// <summary>
    /// Maps from gRPC protobuf message to DTO.
    /// </summary>
    public static ChatMessageDto ToDto(ChatMessage proto)
    {
        return new ChatMessageDto
        {
            Content = proto.Content,
            Sender = proto.Sender,
            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(proto.Timestamp).LocalDateTime
        };
    }
}
