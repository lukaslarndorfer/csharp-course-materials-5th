using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Calculator;

public partial class MainWindow : Window
{

    private string _calculation = string.Empty; 
    private const string EqualKey = "=";
    private double? _result; 
    private string? _error;


    public MainWindow()
    {
        InitializeComponent();
        DecimalSlider.ValueChanged += (_, _) => UpdateDisplay();
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
            var success = CalculationLogic.TryEvaluate(_calculation, out var value, out _error);
            _result = success ? value : null;
        } else
        {
            _calculation = CalculationLogic.HandleInput(_calculation, input ?? string.Empty);
        }
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        Calculation.Text =  _calculation;
        Result.Text = _error ?? 
                      // f stands for fixed-point: rounds and pads with zeros to the slider's decimal places
                      _result?.ToString($"F{(int)DecimalSlider.Value}", CultureInfo.InvariantCulture); 

    }
    
    
}