namespace AutoScrum.UITests.ProjectManagement;

public sealed class CreateProfileFromScratchTest : PageTest
{
    public override float? SlowMo { get; protected set; } = null;

    [Theory]
    [InlineData("Simple profile", "test@email.com", "PAT", "SimpleOrg", "Simple", null)]
    [InlineData("Team profile", "team@email.com", "PAT123", "TeamOrg", "TeamProject", true)]
    [InlineData("Me profile", "me@email.com", "PAT@me", "TheOne", "LoneWolf", false)]
    public async Task Should_CreateProjectWhenNoProjects(string profileName, string email, string pat, string org, string projectName, bool? filterByTeam)
    {
        await Page.RouteAsync("**", async route =>
        {
            if (route.Request.Url.Contains("visualstudio.com"))
            {
                await route.FulfillAsync(new RouteFulfillOptions
                {
                    Status = 200,
                    Body = @"{ ""count"":0, ""value"":[] }"
                });
            }
            else
            {
                await route.ContinueAsync();
            }
        });

        await Page.WaitForSelectorAsync("#is-page-loading", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden });
        await Page.Locator("#ProfileName").FillAsync(profileName);
        await Page.Locator("#ProfileEmail").FillAsync(email);
        await Page.Locator("#ProfilePat").FillAsync(pat);
        await Page.Locator("#ProfileOrg").FillAsync(org);
        await Page.Locator("#ProfileProjName").FillAsync(projectName);

        if (filterByTeam == true)
        {
            await Page.Locator("#filter-by-team").ClickAsync();
        }
        else if (filterByTeam == false)
        {
            await Page.Locator("#filter-by-me").ClickAsync();
        }

        await Page.Locator("#ProfileSave").ClickAsync();

        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync();

        await Page.WaitForSelectorAsync("#is-page-loading", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden });

        await Page.ShouldBeHiddenOrNull("#ProfileForm");

        await Page.Locator("#connection-info-header >> button[role=\"switch\"]").ClickAsync();

        await Page.ShouldBeVisible("#ProfileForm");

        await Page.Locator("#connection-info-profile-id").ShouldHaveValue("1");
        await Page.Locator("#connection-info-profile-name").ShouldHaveValue(profileName);
        await Page.Locator("#connection-info-profile-email").ShouldHaveValue(email);
        await Page.Locator("#connection-info-profile-org").ShouldHaveValue(org);
        await Page.Locator("#connection-info-profile-project-name").ShouldHaveValue(projectName);
        await Page.Locator("#connection-info-profile-project-type").ShouldHaveValue("AzureDevOps");

        if (filterByTeam != null)
        {
            string filterBy = filterByTeam == true ? "Team" : "Me";
            await Page.Locator("#connection-info-profile-team-filter-by").ShouldHaveValue(filterBy);
        }

        await Page.ShouldBeVisible($".ant-select-selection-item:has-text(\"{profileName}\")");
    }
}
