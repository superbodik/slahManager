using System.Windows;
using SlahManager.ViewModels;

namespace SlahManager.Views;

public partial class MainWindow : Window
{
    private const string MaximizeGlyph = "";
    private const string RestoreGlyph = "";

    public MainWindow()
    {
        InitializeComponent();
        StateChanged += (_, _) => UpdateMaximizedState();
        Loaded += async (_, _) =>
        {
            if (DataContext is MainViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        };
    }

    private void OnMinimize(object sender, RoutedEventArgs e) => SystemCommands.MinimizeWindow(this);

    private void OnMaximizeRestore(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            SystemCommands.RestoreWindow(this);
        }
        else
        {
            SystemCommands.MaximizeWindow(this);
        }
    }

    private void OnClose(object sender, RoutedEventArgs e) => SystemCommands.CloseWindow(this);

    private void UpdateMaximizedState()
    {
        var maximized = WindowState == WindowState.Maximized;
        RootGrid.Margin = maximized ? SystemParameters.WindowResizeBorderThickness : new Thickness(0);
        MaximizeButton.Content = maximized ? RestoreGlyph : MaximizeGlyph;
    }
}
