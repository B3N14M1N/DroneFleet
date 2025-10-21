using System.Collections.ObjectModel;

namespace DroneFleet.App.ConsoleUI.Menu;

internal class MenuActionRegistry
{
    private readonly List<IMenuAction> _actions = [];
    private readonly ReadOnlyCollection<IMenuAction> _readOnlyActions;

    public MenuActionRegistry()
    {
        _readOnlyActions = _actions.AsReadOnly();
    }

    public IReadOnlyList<IMenuAction> Actions => _readOnlyActions;

    public void Register(IMenuAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (_actions.Contains(action))
        {
            return;
        }

        _actions.Add(action);
    }

    public bool Unregister(IMenuAction action)
    {
        if (action is null)
        {
            return false;
        }

        return _actions.Remove(action);
    }

    public void Clear() => _actions.Clear();
}
