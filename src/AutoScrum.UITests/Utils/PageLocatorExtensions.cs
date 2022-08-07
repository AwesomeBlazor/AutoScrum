namespace AutoScrum.UITests.Utils;

public static class PageLocatorExtensions
{
    public static async Task ShouldHaveValue(this ILocator locator, string expectedValue)
    {
        var value = await locator.GetAttributeAsync("value");
        value.Should().Be(expectedValue);
    }
}
