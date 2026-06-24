namespace Memory_Storage;

public static class UiAnimations
{
    public static async Task PressAsync(object? sender)
    {
        if (sender is not VisualElement element)
        {
            return;
        }

        await element.ScaleToAsync(0.96, 70, Easing.CubicOut);
        await element.ScaleToAsync(1.03, 90, Easing.CubicOut);
        await element.ScaleToAsync(1, 80, Easing.CubicOut);
    }
}
