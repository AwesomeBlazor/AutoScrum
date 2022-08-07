namespace AutoScrum.UITests.Utils;

public static class PageExtensions
{
    public static async Task ShouldBeVisible(this IPage page, string selector)
    {
        var profileForm = await page.QuerySelectorAsync(selector);
        profileForm.Should().NotBeNull();
        (await profileForm!.IsVisibleAsync()).Should().BeTrue();
    }

    public static async Task ShouldBeHiddenOrNull(this IPage page, string selector)
    {
        // If null or not visible, assertion passes.
        var profileForm = await page.QuerySelectorAsync(selector);
        if (profileForm != null)
        {
            (await profileForm!.IsVisibleAsync()).Should().BeFalse();
        }
    }
}
