---
name: dotnet-maui-testing
category: testing
subcategory: fundamentals
description: Tests .NET MAUI apps. Appium device automation, XHarness, platform validation.
license: MIT
targets: ['*']
tags: [testing, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for testing tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-maui-testing

Testing .NET MAUI applications using Appium for UI automation and XHarness for cross-platform test execution. Covers
device and emulator testing, platform-specific behavior validation, element location strategies for MAUI controls, and
test infrastructure for mobile/desktop apps.

**Version assumptions:** .NET 8.0+ baseline, Appium 2.x with UIAutomator2 (Android) and XCUITest (iOS) drivers, XHarness
1.x. Examples use the latest Appium .NET client (5.x+).

## Scope

- Appium 2.x UI automation for Android, iOS, and Windows
- XHarness cross-platform test execution
- Platform-specific behavior validation
- Element location strategies for MAUI controls
- Test infrastructure for mobile/desktop apps

## Out of scope

- Shared UI testing patterns (page object model, wait strategies) -- see [skill:dotnet-ui-testing-core]
- Browser-based testing -- see [skill:dotnet-playwright]
- Test project scaffolding -- see [skill:dotnet-add-testing]

**Prerequisites:** MAUI test project scaffolded via [skill:dotnet-add-testing]. Appium server installed
(`npm install -g appium`). For Android: Android SDK with emulator configured. For iOS: Xcode with simulator (macOS
only). For Windows: WinAppDriver installed.

Cross-references: [skill:dotnet-ui-testing-core] for page object model, test selectors, and async wait patterns,
[skill:dotnet-xunit] for xUnit fixtures and test organization, [skill:dotnet-maui-development] for MAUI project
structure, XAML/MVVM patterns, and platform services, [skill:dotnet-maui-aot] for Native AOT on iOS/Mac Catalyst and AOT
build testing considerations.

---

## Appium Setup for MAUI

### Packages

````xml

<PackageReference Include="Appium.WebDriver" Version="5.*" />
<PackageReference Include="xunit.v3" Version="3.2.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.5" />

```xml

### Driver Initialization

```csharp

public class AppiumFixture : IAsyncLifetime
{
    public AppiumDriver Driver { get; private set; } = null!;

    public ValueTask InitializeAsync()
    {
        var options = new AppiumOptions();

        if (OperatingSystem.IsAndroid() || TestConfig.TargetPlatform == "Android")
        {
            options.PlatformName = "Android";
            options.AutomationName = "UiAutomator2";
            options.App = TestConfig.AndroidApkPath;
            options.AddAdditionalAppiumOption("deviceName", "Pixel_7_API_34");
            options.AddAdditionalAppiumOption("avd", "Pixel_7_API_34");
        }
        else if (OperatingSystem.IsIOS() || TestConfig.TargetPlatform == "iOS")
        {
            options.PlatformName = "iOS";
            options.AutomationName = "XCUITest";
            options.App = TestConfig.iOSAppPath;
            options.AddAdditionalAppiumOption("deviceName", "iPhone 15");
            options.AddAdditionalAppiumOption("platformVersion", "17.2");
        }
        else if (OperatingSystem.IsWindows() || TestConfig.TargetPlatform == "Windows")
        {
            options.PlatformName = "Windows";
            options.AutomationName = "Windows";
            options.App = TestConfig.WindowsAppPath;
        }

        Driver = new AppiumDriver(
            new Uri("http://localhost:4723"), options);

        // Explicit waits only -- do not set ImplicitWait (it causes
        // additive timeout behavior when combined with WebDriverWait)
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;

        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        Driver?.Quit();
        return ValueTask.CompletedTask;
    }
}

```text

### Test Configuration

```csharp

public static class TestConfig
{
    // Set via environment variables or test runsettings
    public static string TargetPlatform =>
        Environment.GetEnvironmentVariable("TEST_PLATFORM") ?? "Android";

    public static string AndroidApkPath =>
        Environment.GetEnvironmentVariable("ANDROID_APK_PATH")
        ?? Path.Combine(SolutionDir, "bin", "Release", "net8.0-android", "com.myapp-Signed.apk");

    public static string iOSAppPath =>
        Environment.GetEnvironmentVariable("IOS_APP_PATH")
        ?? Path.Combine(SolutionDir, "bin", "Release", "net8.0-ios", "MyApp.app");

    public static string WindowsAppPath =>
        Environment.GetEnvironmentVariable("WINDOWS_APP_PATH")
        ?? "com.mycompany.myapp_1.0.0.0_x64__9a0dh7ch11qe4!App";

    private static string SolutionDir =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
}

```text

---

## Element Location with AutomationId

MAUI's `AutomationId` property maps to the platform-native accessibility identifier. This is the most reliable selector
for cross-platform tests.

### Setting AutomationId in XAML

```xml

<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">

    <VerticalStackLayout>
        <Entry AutomationId="username-input"
               Placeholder="Username" />

        <Entry AutomationId="password-input"
               Placeholder="Password"
               IsPassword="True" />

        <Button AutomationId="login-button"
                Text="Log In"
                Clicked="OnLoginClicked" />

        <Label AutomationId="error-message"
               TextColor="Red" />
    </VerticalStackLayout>
</ContentPage>

```text

### Finding Elements in Tests

```csharp

public class LoginTests : IClassFixture<AppiumFixture>
{
    private readonly AppiumDriver _driver;

    public LoginTests(AppiumFixture fixture)
    {
        _driver = fixture.Driver;
    }

    [Fact]
    public void Login_ValidCredentials_NavigatesToHome()
    {
        // Find by AutomationId (maps to accessibility ID on each platform)
        var usernameField = _driver.FindElement(MobileBy.AccessibilityId("username-input"));
        var passwordField = _driver.FindElement(MobileBy.AccessibilityId("password-input"));
        var loginButton = _driver.FindElement(MobileBy.AccessibilityId("login-button"));

        usernameField.Clear();
        usernameField.SendKeys("testuser");
        passwordField.Clear();
        // Use placeholder password in examples
        passwordField.SendKeys("<TEST_PASSWORD_PLACEHOLDER>");
        loginButton.Click();

        // Wait for navigation
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        var homeTitle = wait.Until(d =>
            d.FindElement(MobileBy.AccessibilityId("home-title")));

        Assert.Equal("Welcome", homeTitle.Text);
    }

    [Fact]
    public void Login_InvalidCredentials_ShowsError()
    {
        var usernameField = _driver.FindElement(MobileBy.AccessibilityId("username-input"));
        var passwordField = _driver.FindElement(MobileBy.AccessibilityId("password-input"));
        var loginButton = _driver.FindElement(MobileBy.AccessibilityId("login-button"));

        usernameField.Clear();
        usernameField.SendKeys("wrong");
        passwordField.Clear();
        passwordField.SendKeys("wrong");
        loginButton.Click();

        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        var errorLabel = wait.Until(d =>
            d.FindElement(MobileBy.AccessibilityId("error-message")));

        Assert.Contains("Invalid", errorLabel.Text);
    }
}

```text

---

## Page Object Model for MAUI

Apply the page object model pattern (see [skill:dotnet-ui-testing-core]) with Appium's driver:

```csharp

public class LoginPage
{
    private readonly AppiumDriver _driver;
    private readonly WebDriverWait _wait;

    public LoginPage(AppiumDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        WaitForPageLoaded();
    }

    private AppiumElement UsernameField =>
        _driver.FindElement(MobileBy.AccessibilityId("username-input"));
    private AppiumElement PasswordField =>
        _driver.FindElement(MobileBy.AccessibilityId("password-input"));
    private AppiumElement LoginButton =>
        _driver.FindElement(MobileBy.AccessibilityId("login-button"));
    private AppiumElement ErrorMessage =>
        _driver.FindElement(MobileBy.AccessibilityId("error-message"));

    public HomePage Login(string username, string password)
    {
        UsernameField.Clear();
        UsernameField.SendKeys(username);
        PasswordField.Clear();
        PasswordField.SendKeys(password);
        LoginButton.Click();

        return new HomePage(_driver);
    }

    public string GetErrorText()
    {
        _wait.Until(d =>
        {
            var el = d.FindElement(MobileBy.AccessibilityId("error-message"));
            return !string.IsNullOrEmpty(el.Text);
        });
        return ErrorMessage.Text;
    }

    private void WaitForPageLoaded()
    {
        _wait.Until(d => d.FindElement(MobileBy.AccessibilityId("login-button")));
    }
}

// Usage
[Fact]
public void Login_ValidUser_ReachesHomePage()
{
    var loginPage = new LoginPage(_driver);
    // Use placeholder password in examples
    var homePage = loginPage.Login("alice", "<TEST_PASSWORD_PLACEHOLDER>");

    Assert.True(homePage.IsLoaded);
}

```text

---

## Platform-Specific Behavior Testing

### Conditional Tests by Platform

```csharp

public class PlatformTests : IClassFixture<AppiumFixture>
{
    private readonly AppiumDriver _driver;

    public PlatformTests(AppiumFixture fixture)
    {
        _driver = fixture.Driver;
    }

    [Fact]
    [Trait("Platform", "Android")]
    public void BackButton_Android_NavigatesBack()
    {
        // xUnit v3 native skip support (no SkippableFact package needed)
        Assert.SkipWhen(TestConfig.TargetPlatform != "Android",
            "Android-only: hardware back button");

        // Navigate to details page
        _driver.FindElement(MobileBy.AccessibilityId("item-1")).Click();

        // Press Android back button
        _driver.Navigate().Back();

        // Verify we returned to the list
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        wait.Until(d => d.FindElement(MobileBy.AccessibilityId("item-list")));
    }

    [Fact]
    [Trait("Platform", "iOS")]
    public void SwipeToDelete_iOS_RemovesItem()
    {
        // xUnit v3 native skip support
        Assert.SkipWhen(TestConfig.TargetPlatform != "iOS",
            "iOS-only: swipe gesture");

        var item = _driver.FindElement(MobileBy.AccessibilityId("item-1"));

        // Swipe left to reveal delete action
        var swipe = new PointerInputDevice(PointerKind.Touch, "finger");
        var sequence = new ActionSequence(swipe);
        var itemLocation = item.Location;
        var itemSize = item.Size;

        sequence.AddAction(swipe.CreatePointerMove(
            item, itemSize.Width - 10, itemSize.Height / 2,
            TimeSpan.FromMilliseconds(0)));
        sequence.AddAction(swipe.CreatePointerDown(MouseButton.Left));
        sequence.AddAction(swipe.CreatePointerMove(
            item, 10, itemSize.Height / 2,
            TimeSpan.FromMilliseconds(300)));
        sequence.AddAction(swipe.CreatePointerUp(MouseButton.Left));

        _driver.PerformActions([sequence]);

        // Tap the delete button
        var deleteBtn = _driver.FindElement(MobileBy.AccessibilityId("delete-action"));
        deleteBtn.Click();

        // Verify item removed
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        wait.Until(d =>
        {
            var items = d.FindElements(MobileBy.AccessibilityId("item-1"));
            return items.Count == 0;
        });
    }
}

```text

### Screen Size and Orientation

```csharp

[Fact]
public void Dashboard_LandscapeMode_ShowsSidePanel()
{
    // Rotate to landscape
    _driver.Orientation = ScreenOrientation.Landscape;


## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
