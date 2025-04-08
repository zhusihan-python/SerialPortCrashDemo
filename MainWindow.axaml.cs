using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace SerialPortCrashDemo;

public partial class MainWindow : Window
{
    private ComPort _comPort;
    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = this;
        var parser = new FrameParser();
        this._comPort = new ComPort(parser);
        Task.Run(async () => await InitializeComPortAsync()).ConfigureAwait(false);
    }

    private async Task InitializeComPortAsync()
    {
        await this._comPort.InitializeAsync("COM2", 115200);
    }

    public string Text { get; set; } = string.Join(Environment.NewLine, Enumerable.Range(0, 1000).Select(i => $"Line {i}"));

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var scroll = text1.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (scroll is not null)
        {
            scroll.ScrollChanged += OnScrollChanged;
        }
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        var scroll2 = text2.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (scroll2 is not null)
        {
            scroll2.Offset = new Vector(scroll2.Offset.X, scroll2.Offset.Y + e.OffsetDelta.Y);
        }
    }
}