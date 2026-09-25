using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Calculator;

public partial class MainWindow : Window
{

    private string _calculation = string.Empty; 
    private const string EqualKey = "=";
    private string _result = string.Empty; 

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: not null } button)
        {
            return;
        }
        
        var input =  button.Tag.ToString();
        if (input == EqualKey)
        {
            _result = CalculationLogic.Evaluate(_calculation);
        } else
        {
            _calculation = CalculationLogic.HandleInput(_calculation, input ?? string.Empty);
        }
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        Calculation.Text =  _calculation;
        Result.Text = _result;
    }
    
    
}