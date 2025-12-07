using Rndr.Navigation;

namespace Rndr.Extensions;

/// <summary>
/// Extension methods for navigation context to improve ergonomics.
/// </summary>
public static class NavigationExtensions
{
    /// <summary>
    /// Navigates to the home route ("/").
    /// </summary>
    /// <param name="navigation">The navigation context.</param>
    public static void NavigateHome(this NavigationContext navigation)
    {
        navigation.Navigate("/");
    }

    /// <summary>
    /// Navigates back if there is a previous route in the stack, otherwise navigates to home.
    /// </summary>
    /// <param name="navigation">The navigation context.</param>
    public static void BackOrHome(this NavigationContext navigation)
    {
        if (navigation.CanGoBack())
        {
            navigation.Back();
        }
        else
        {
            navigation.Navigate("/");
        }
    }

    /// <summary>
    /// Gets a value indicating whether navigation can go back.
    /// </summary>
    /// <param name="navigation">The navigation context.</param>
    /// <returns>True if there are routes in the navigation stack to go back to.</returns>
    public static bool CanGoBack(this NavigationContext navigation)
    {
        return navigation.State.Stack.Count > 1;
    }
}
