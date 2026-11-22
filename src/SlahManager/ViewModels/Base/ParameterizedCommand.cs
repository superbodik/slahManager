using System.Windows.Input;

namespace SlahManager.ViewModels.Base;

public sealed class ParameterizedCommand<T> : ICommand
{
    private readonly Action<T> _execute;

    public ParameterizedCommand(Action<T> execute)
    {
        _execute = execute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => parameter is T;

    public void Execute(object? parameter)
    {
        if (parameter is T value)
        {
            _execute(value);
        }
    }
}
