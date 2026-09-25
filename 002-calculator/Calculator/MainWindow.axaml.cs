using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Calculator;

public partial class MainWindow : Window
{
    private string _calculation = string.Empty;
    private const string EqualKey = "=";

    private double? _simpleResult;
    private string? _simpleError;
    private double? _expressionResult;
    private string? _expressionError;

    private bool IsExpressionMode => CalculationModeSwitch.IsChecked == true;


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

        var input = button.Tag.ToString();
        if (input == EqualKey)
        {
            var success = CalculationLogic.TryEvaluate(_calculation, out var value, out _simpleError);
            _simpleResult = success ? value : null;
        }
        else
        {
            _calculation = CalculationLogic.HandleInput(_calculation, input ?? string.Empty);
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        var result = IsExpressionMode ? _expressionResult : _simpleResult;
        var error = IsExpressionMode ? _expressionError : _simpleError;

        Calculation.Text = _calculation;
        Result.Text = error ??
                      // f stands for fixed-point: rounds and pads with zeros to the slider's decimal places
                      result?.ToString($"F{(int)DecimalSlider.Value}", CultureInfo.InvariantCulture);
    }


    private void OnModeChanged(object? sender, RoutedEventArgs e)
    {
        SimpleCalculationModePanel.IsVisible = !IsExpressionMode;
        ExpressionEvaluationModePanel.IsVisible = IsExpressionMode;
        UpdateDisplay();
    }

    private void OnEvaluateExpressionClick(object? sender, RoutedEventArgs e)
    {
        var success = CalculationLogic.TryEvaluate(ExpressionText.Text ?? "", out var value, out _expressionError);
        _expressionResult = success ? value : null;
        UpdateDisplay();
    }
}