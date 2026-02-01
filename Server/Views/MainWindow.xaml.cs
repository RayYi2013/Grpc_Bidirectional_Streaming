using Server.ViewModels;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace Server.Views;

/// <summary>
/// Main window with minimal code-behind following MVVM pattern.
/// </summary>
public partial class MainWindow
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Load saved window placement
        Loaded += (s, e) => LoadWindowPlacement();

        // Show a simple notification when new messages arrive
        viewModel.Messages.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is Server.Application.DTOs.ChatMessageDto msg && !string.Equals(msg.Sender, "System", StringComparison.OrdinalIgnoreCase))
                    {
                        // Show a brief info box on UI thread for non-system messages
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"{msg.Sender}: {msg.Content}", "Server Get New Message", MessageBoxButton.OK, MessageBoxImage.Information);
                        });
                    }
                }
            }
        };

        // Handle window closing to cleanup resources
        Closing += async (s, e) =>
        {
            SaveWindowPlacement();
            await viewModel.StopServerAsync();
            viewModel.Dispose();
        };
    }

    private string GetPlacementFilePath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GrpcBidirectionalStreaming", "server_window.json");

    private void LoadWindowPlacement()
    {
        try
        {
            var file = GetPlacementFilePath();
            if (!File.Exists(file))
                return;

            var json = File.ReadAllText(file);
            var obj = JsonSerializer.Deserialize<WindowPlacement>(json);
            if (obj == null) return;

            if (!double.IsNaN(obj.Left) && !double.IsNaN(obj.Top))
            {
                Left = obj.Left;
                Top = obj.Top;
            }

            if (obj.Width > 0) Width = obj.Width;
            if (obj.Height > 0) Height = obj.Height;

            WindowState = obj.State;
        }
        catch { }
    }

    private void SaveWindowPlacement()
    {
        try
        {
            var file = GetPlacementFilePath();
            var dir = Path.GetDirectoryName(file);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var obj = new WindowPlacement
            {
                Left = this.Left,
                Top = this.Top,
                Width = this.Width,
                Height = this.Height,
                State = this.WindowState
            };

            File.WriteAllText(file, JsonSerializer.Serialize(obj));
        }
        catch { }
    }

    private class WindowPlacement
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public WindowState State { get; set; }
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }
}
