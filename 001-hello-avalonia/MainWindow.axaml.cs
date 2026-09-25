using Avalonia.Controls;
using Avalonia.Interactivity;

namespace HelloAvalonia;

public partial class MainWindow : Window
{
    private int _count = 0;
    public MainWindow()
    {
        InitializeComponent();
        UpdateCountDisplay(); // für initialen Wert 0
    }

    private void HandleBtnClick(object? sender, RoutedEventArgs e) // an nullable object erkennt man das es alt ist (damals gabs noch keine generics)
    {
        _count++;
        UpdateCountDisplay();
    }

    private void UpdateCountDisplay()
    {
        CountDisplay.Text = $"{_count:000}"; // zero-padded auf 3 stellen
    }
}