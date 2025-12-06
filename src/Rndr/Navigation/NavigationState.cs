namespace Rndr.Navigation;

/// <summary>
/// Default implementation of navigation state.
/// </summary>
internal sealed class NavigationState : INavigationState
{
    private readonly Stack<string> _stack;

    public NavigationState(Stack<string> stack)
    {
        _stack = stack;
    }

    /// <inheritdoc />
    public string CurrentRoute => _stack.Count > 0 ? _stack.Peek() : "/";

    /// <inheritdoc />
    public IReadOnlyList<string> Stack => _stack.ToArray().Reverse().ToList();
}

