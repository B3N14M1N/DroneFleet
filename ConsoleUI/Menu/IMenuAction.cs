namespace DroneFleet.ConsoleUI.Menu;

internal interface IMenuAction
{
    /// <summary>
    /// The label to display in the menu.
    /// </summary>
    string Label { get; }

    /// <summary>
    /// The description of the action.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// The method to execute when the action is selected.
    /// </summary>
    /// <returns>Returns the action outcome <see cref="MenuActionOutcome"/></returns>
    MenuActionOutcome Execute();
}
