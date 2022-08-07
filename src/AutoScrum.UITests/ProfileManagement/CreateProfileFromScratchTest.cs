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

        //// Click span:has-text("Test profile")
        //await page.Locator("span:has-text(\"Test profile\")").ClickAsync();

        //// Click text=Test profile >> nth=1
        //await page.Locator("text=Test profile").Nth(1).ClickAsync();

        //// Click button:has-text("➕ Add Project")
        //await page.Locator("button:has-text(\"➕ Add Project\")").ClickAsync();

        //// Click text=Profile name
        //await page.Locator("text=Profile name").ClickAsync();

        //// Click [placeholder="Profile name\.\.\."]
        //await page.Locator("[placeholder=\"Profile name\\.\\.\\.\"]").ClickAsync();

        //// Press a with modifiers
        //await page.Locator("[placeholder=\"Profile name\\.\\.\\.\"]").PressAsync("Control+a");

        //// Fill [placeholder="Profile name\.\.\."]
        //await page.Locator("[placeholder=\"Profile name\\.\\.\\.\"]").FillAsync("Test project 2");

        //// Press Tab
        //await page.Locator("[placeholder=\"Profile name\\.\\.\\.\"]").PressAsync("Tab");

        //// Fill [placeholder="Your email\.\.\."]
        //await page.Locator("[placeholder=\"Your email\\.\\.\\.\"]").FillAsync("second@email.com");

        //// Press Tab
        //await page.Locator("[placeholder=\"Your email\\.\\.\\.\"]").PressAsync("Tab");

        //// Fill [placeholder="Your Azure DevOps Personal Access Token \(PAT\)"]
        //await page.Locator("[placeholder=\"Your Azure DevOps Personal Access Token \\(PAT\\)\"]").FillAsync("PAR2");

        //// Press Tab
        //await page.Locator("[placeholder=\"Your Azure DevOps Personal Access Token \\(PAT\\)\"]").PressAsync("Tab");

        //// Fill [placeholder="Azure DevOps Organization Name or URL"]
        //await page.Locator("[placeholder=\"Azure DevOps Organization Name or URL\"]").FillAsync("Org2");

        //// Press Tab
        //await page.Locator("[placeholder=\"Azure DevOps Organization Name or URL\"]").PressAsync("Tab");

        //// Fill [placeholder="Azure DevOps Project"]
        //await page.Locator("[placeholder=\"Azure DevOps Project\"]").FillAsync("ProjName2");

        //// Click button:has-text("Save")
        //await page.Locator("button:has-text(\"Save\")").ClickAsync();

        //// Click span:has-text("New Project")
        //await page.Locator("span:has-text(\"New Project\")").ClickAsync();

        //// Click text=Test profile
        //await page.Locator("text=Test profile").ClickAsync();

        //// Click span:has-text("Test profile")
        //await page.Locator("span:has-text(\"Test profile\")").ClickAsync();

        //// Click text=New Project
        //await page.Locator("text=New Project").ClickAsync();
    }
}
