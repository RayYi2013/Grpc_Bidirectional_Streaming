You are an expert in modern .NET desktop development using C# and clean architecture.

use solid skill.

Create **two separate WPF applications** — one client and one server — that communicate via **gRPC bidirectional streaming** in .NET.


Target framework and tools (early 2026):
• .NET 10.0 (use <TargetFramework>net10.0-windows</TargetFramework>)
• WPF
• C# 14 (use primary constructors, file-scoped namespaces, required members, collection expressions, etc.)
• **CommunityToolkit.Mvvm** (latest stable version — use [ObservableProperty], [RelayCommand], ObservableObject, ObservableRecipient, etc.)
• **Strict MVVM** — no ReactiveUI; rely on CommunityToolkit.Mvvm source generators for observable properties & commands, and System.Reactive / DynamicData only if truly needed for advanced scenarios (avoid if possible)
• Use **Microsoft.Extensions.DependencyInjection** for DI
• Follow SOLID principles (SRP, DIP especially)
• gRPC: Grpc.Net.Client + Google.Protobuf + Grpc.Tools (client); Grpc.AspNetCore.Server (server — hosted inside WPF, not ASP.NET Core web host)
• Modern, clean, Windows 11 / Fluent-inspired look: good spacing, subtle borders, rounded corners, system theme awareness (light/dark) if possible

Third-party UI library requirement:
• Use one modern free/open-source WPF UI styling library to achieve a contemporary look (Windows 11 / Fluent Design style preferred).
• Strong recommendation: **WPF UI** (https://github.com/lepoco/wpfui or https://wpfui.lepo.co/) — it provides Navigation, Snackbar, Dialog, modern controls, and Fluent/Win11 aesthetics with minimal overhead.
• Alternatives to consider (choose one primary):
  - ModernWpf[](https://github.com/Kinnara/ModernWpf) — clean Win10/11 emulation, lightweight
  - MahApps.Metro — very popular, Metro/Modern style, battle-tested
  - MaterialDesignInXamlToolkit — if Material Design is desired
• Prefer WPF UI as default unless there's a clear reason to choose another. Include the necessary NuGet references and App.xaml ResourceDictionary merges to apply the theme globally.
• Keep additional third-party libs minimal — no heavy commercial suites (Telerik, Syncfusion, etc.) unless explicitly better for this simple chat-like app.

Shared .proto (place in both projects or shared):
syntax = "proto3";

option csharp_namespace = "ChatGrpc";

package chat;

service BidirectionalChat {
  rpc Chat (stream ChatMessage) returns (stream ChatMessage);
}

message ChatMessage {
  string content = 1;
  string sender = 2;     // "Client" or "Server"
  int64 timestamp = 3;
}

Application requirements — Client (WPF app)

MainWindow layout (single main view):
• Top section: TextBox (server address, default "https://localhost:5001") + Button "Connect"
• Status indicator (TextBlock or colored icon/label): Disconnected / Connecting / Connected / Error
• Middle: Multi-line ReadOnly TextBox (or better: ItemsControl/ListView with messages) showing conversation history (prefix with [Client] or [Server], timestamp optional)
• Bottom: TextBox (single line, supports Enter to send) + Button "Send"
  - On send: transmit text via open stream, append to local history as [Client]
  - Clear input after send
• On connect: establish bidirectional gRPC call (AsyncDuplexStreamingCall), read responses continuously in background, append received messages as [Server]
• Graceful cleanup on disconnect / window close

Server (separate WPF app)

MainWindow layout:
• Top: Button "Start Server" + status (Stopped / Running / Error) + optional connected clients count
  - Fixed address e.g. https://*:5001 (or http2 unencrypted for dev: http://localhost:5001)
• Middle: Multi-line ReadOnly TextBox / ItemsControl showing messages received from clients
• Bottom: TextBox + Button "Send"
  - Send message to **all** connected clients (broadcast)
  - Append sent message to local display as [Server]

gRPC notes:
• Client: open persistent duplex stream on connect; use background task/WhenAnyValue to read responses reactively
• Server: implement BidirectionalChat service; maintain collection of active response streams (e.g. ConcurrentBag or list with lock); broadcast server messages to all; echo or forward client messages as desired (simple broadcast style is fine)
• Use insecure http/2 for dev simplicity (AppContext switch or server options)

Technical requirements (both apps):
• File-scoped namespaces
• Primary constructors where sensible
• DI setup in App.xaml.cs (register ViewModels, gRPC channel/server lifetime, etc.)
• Use [ObservableProperty], [RelayCommand] heavily
• No code-behind except minimal window events (e.g. Closing → cleanup streams/server)
• IViewFor not needed (CommunityToolkit doesn't require it)
• Modern C# style, clear comments on gRPC lifetime, stream management, disposal

Deliverables — output structure:

1. Shared .proto file

Then Client:

1. Client.csproj (include PackageReferences: CommunityToolkit.Mvvm, Grpc.Net.Client, etc. + chosen UI lib)
2. App.xaml & App.xaml.cs (DI + theme ResourceDictionary)
3. MainWindow.xaml & MainWindow.xaml.cs (minimal)
4. MainViewModel.cs
5. Grpc client wrapper/service (interface + impl)

Then Server: similar structure (Server.csproj, App.xaml, MainWindow, MainViewModel, Grpc service impl + hosting logic)

Aim for clean, professional, maintainable code that would pass review in 2026 for a WPF + gRPC + CommunityToolkit.Mvvm app.

Start output with .proto, then Client project files, then Server.