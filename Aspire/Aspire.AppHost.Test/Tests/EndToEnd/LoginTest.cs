using Microsoft.Playwright;
using MspOperator.All.Aspire.Test.Helpers;

namespace MspOperator.All.Aspire.Test.Tests.EndToEnd;

[TestFixture]
public class LoginTests : EndToEndTest
{
    [Test]
    public async Task Message_ShouldBeSentFromApp1ToApp2(CancellationToken cancellationToken)
    {
        // Arrange
        IPage page1 = await Context.NewPageAsync();
        await page1.GotoAsync(BaseAspNetUrl);

        IPage page2 = await Context.NewPageAsync();
        await page2.GotoAsync(BaseAspNetCoreUrl);

        const string message = "Mellon";

        // Act
        ILocator messageTextField = page1.GetByTestId("message-text");
        await messageTextField.FillAsync(message);

        ILocator sendButton = page1.GetByTestId("send-message");
        await sendButton.ClickAsync();

        // Assert
        ILocator messageSpan = page2.GetByTestId("message-text");
        await Expect(messageSpan).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(messageSpan).ToHaveTextAsync(message);
    }
}