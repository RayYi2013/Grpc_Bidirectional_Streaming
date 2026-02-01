# gRPC Bidirectional Streaming (WPF Client & Server)

Summary
- Two WPF applications demonstrating gRPC bidirectional streaming in .NET 10: a `Client` and a `Server`.
- Architecture: strict MVVM using CommunityToolkit.Mvvm, dependency injection via Microsoft.Extensions.DependencyInjection, and a modern Fluent-inspired WPF UI library.
- Purpose: lightweight chat-style demo where the Client opens a persistent duplex gRPC stream and the Server maintains active response streams to broadcast messages to connected clients.

Requirements (source)
See `Requirement.md` in the repository root for full project requirements, design notes, and the shared `.proto` specification used by both projects.

Key Features
- Client
  - Connect/Disconnect to server (default address: `http://localhost:5001`).
  - Persistent duplex stream (send messages and receive server/client broadcasts).
  - Messages list (ItemsControl/ListView) with timestamp and sender.
  - Send on Enter or `Send` button.
  - Window placement persisted between launches.

- Server
  - Start/Stop server from the WPF UI.
  - Maintains collection of active client response streams and broadcasts messages to all connected clients.
  - Messages list showing received and server-sent messages.
  - Broadcast messages to all clients via UI.
  - Window placement persisted between launches.

gRPC
- Shared `.proto` (see `Protos/chat.proto`) defines the `BidirectionalChat` service:

  - `rpc Chat (stream ChatMessage) returns (stream ChatMessage);`
  - `ChatMessage` contains `content`, `sender`, and `timestamp`.

- For development convenience the solution uses insecure http/2 (http://localhost:5001); the server hosts gRPC inside the WPF app rather than using an external ASP.NET Core web host.

Projects
- `Client/` — WPF client project, contains `App.xaml`, `MainWindow.xaml`, `ViewModels`, and a gRPC client wrapper/service.
- `Server/` — WPF server project, contains `App.xaml`, `MainWindow.xaml`, `ViewModels`, and gRPC service + connection manager.
- `Protos/chat.proto` — shared proto definition used by both projects.

Build & Run (simple)
Make sure you have .NET 10 SDK installed.

```powershell
# From repo root
dotnet build "Grpc_Bidirectional_Streaming.sln"
# Run the Server app (in a terminal or Visual Studio)
dotnet run --project Server/Server.csproj
# Run the Client app (in a separate terminal)
dotnet run --project Client/Client.csproj
```

Notes / Known behaviors
- The UI uses a Fluent-inspired WPF library (WPF UI). If the app uses custom titlebar/backdrop features they may be toggled in `MainWindow.xaml`. Standard system buttons are enabled by default.
- Window placement (Left/Top/Width/Height/WindowState) is stored in `%AppData%\GrpcBidirectionalStreaming` as JSON for each app (`server_window.json` / `client_window.json`).
- Message pop-ups are displayed for non-system messages; system informational messages (like "Server started") are appended to message history but do not pop-up.

Next actions you may want
- Run the solution and verify connect/send/broadcast flows.
- Add bounds-checking on window placement restore (multi-monitor safety).
- Replace JSON placement with user settings if you prefer `Properties.Settings`.

License & Credits
- This repo demonstrates patterns and references the `WPF UI` library (https://github.com/lepoco/wpfui). See project files for exact NuGet package versions.

Files to inspect first
- `Protos/chat.proto`
- `Server/Views/MainWindow.xaml` and `Server/ViewModels/MainViewModel.cs`
- `Client/Views/MainWindow.xaml` and `Client/ViewModels/MainViewModel.cs`

