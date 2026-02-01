using System.Windows.Media;

namespace Server.Application.DTOs;

/// <summary>
/// Data Transfer Object for chat messages, decoupled from gRPC protobuf types.
/// </summary>
public class ChatMessageDto
{
    public required string Content { get; init; }
    public required string Sender { get; init; }
    public required DateTime Timestamp { get; init; }

    public string FormattedTimestamp => Timestamp.ToString("HH:mm:ss");

    /// <summary>
    /// Color for sender label based on sender type.
    /// </summary>
    public Brush SenderColor => Sender == "Server" 
        ? new SolidColorBrush(Color.FromRgb(134, 239, 172))  // Green
        : new SolidColorBrush(Color.FromRgb(96, 165, 250));  // Blue
}
