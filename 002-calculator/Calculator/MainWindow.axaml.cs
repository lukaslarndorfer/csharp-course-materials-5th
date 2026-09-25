using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Calculator;

public partial class MainWindow : Window
{

    private string _calculation = string.Empty; 
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: not null } button)
        {
            var input =  button.Tag.ToString();
            _calculation = CalculationLogic.HandleCalculationInput(_calculation, input ?? string.Empty);
            UpdateCalculation();
        }
    }

    private void UpdateCalculation()
    {
        Calculation.Text =  _calculation;
    }
    
    
}