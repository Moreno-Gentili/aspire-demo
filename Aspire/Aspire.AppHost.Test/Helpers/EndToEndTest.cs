using Aspire.Hosting;
using Microsoft.Playwright.NUnit;

namespace MspOperator.All.Aspire.Test.Helpers;

public class EndToEndTest : PageTest
{
    private DistributedApplication? app = null;

    [OneTimeSetUp]
    public async Task StartAspireAsync()
    {
        if (SkipAspire)
        {
            return;
        }

        IDistributedApplicationTestingBuilder appHost =
            await DistributedApplicationTestingBuilder.CreateAsync<Projects.Aspire_AppHost>(
                args: [],
                configureBuilder: (appOptions, hostSettings) =>
                {
                    appOptions.DisableDashboard = false;
                });

        appHost.Configuration["ASPNETCORE_URLS"] = "http://localhost:18888"; // Dashboard URL
        appHost.Configuration["AppHost:BrowserToken"] = "";
        appHost.Configuration["DcpPublisher:RandomizePorts"] = "false";

        app = await appHost.BuildAsync();
        await app.StartAsync();

        /*await app.ResourceNotifications
            .WaitForResourceHealthyAsync(nameof(Projects.ProgettoAspNet));

        await app.ResourceNotifications
            .WaitForResourceHealthyAsync(nameof(Projects.ProgettoAspNetCore));*/
    }

    [OneTimeTearDown]
    public async Task StopAspireAsync()
    {
        if (app is not null)
        {
            await app.DisposeAsync();
        }
    }

    protected string BaseAspNetUrl =>
        TestContext.Parameters["BaseAspNetUrl"] 
        ?? "http://localhost:8881";

    protected string BaseAspNetCoreUrl =>
        TestContext.Parameters["BaseAspNetCoreUrl"]
        ?? "http://localhost:8880";

    protected bool SkipAspire =>
        TestContext.Parameters["SkipAspire"] == bool.TrueString;
}
