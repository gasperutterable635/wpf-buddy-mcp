using System.Text.Json;
using WpfBuddy.Mcp.Server.Services;
using WpfBuddy.Mcp.Server.Tools;

namespace WpfBuddy.Mcp.IntegrationTests;

/// <summary>
/// Tests that tools return proper error messages when no app is attached.
/// </summary>
public class UnattachedErrorTests
{
    private readonly SessionManager _session = new();
    private readonly AuditLog _audit = new();
    private readonly UiaAdapter _uia;
    private readonly ProbeClient _probe = new();
    private readonly RecordingService _recording;
    private readonly ExplorerService _explorer;
    private readonly DevWatcherService _devWatcher;

    public UnattachedErrorTests()
    {
        _uia = new UiaAdapter(_session);
        _recording = new RecordingService(_session);
        _explorer = new ExplorerService(_session, _uia, _audit);
        _devWatcher = new DevWatcherService(_session, _uia, _probe);
    }

    [Fact]
    public void ExploreScreen_ReturnsError_WhenNotAttached()
    {
        var tools = new ExplorerTools(_explorer, _uia, _audit);
        var result = tools.ExploreScreen();
        var json = JsonDocument.Parse(result);
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public void ExploreApp_ReturnsError_WhenNotAttached()
    {
        var tools = new ExplorerTools(_explorer, _uia, _audit);
        var result = tools.ExploreApp(maxSteps: 1);
        var json = JsonDocument.Parse(result);
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public void SuggestTestScenarios_ReturnsError_WhenNotAttached()
    {
        var tools = new ExplorerTools(_explorer, _uia, _audit);
        var result = tools.SuggestTestScenarios();
        var json = JsonDocument.Parse(result);
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public async Task WhyDisabled_ReturnsError_WhenNotAttached()
    {
        var tools = new WhyTools(_uia, _probe, _session, _audit);
        var result = await tools.WhyDisabled(name: "Save");
        var json = JsonDocument.Parse(result);
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public void WhyHidden_ReturnsError_WhenNotAttached()
    {
        var tools = new WhyTools(_uia, _probe, _session, _audit);
        var result = tools.WhyHidden(name: "Panel");
        var json = JsonDocument.Parse(result);
        Assert.True(json.RootElement.TryGetProperty("error", out _) ||
                    json.RootElement.TryGetProperty("analysis", out _));
    }

    [Fact]
    public async Task WhyValidationFailed_ReturnsError_WhenNotAttached()
    {
        var tools = new WhyTools(_uia, _probe, _session, _audit);
        var result = await tools.WhyValidationFailed();
        var json = JsonDocument.Parse(result);
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public async Task WhyEmpty_ReturnsError_WhenNotAttached()
    {
        var tools = new WhyTools(_uia, _probe, _session, _audit);
        var result = await tools.WhyEmpty(name: "TextBox1");
        var json = JsonDocument.Parse(result);
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public async Task ExplainScreen_ReturnsError_WhenNotAttached()
    {
        var tools = new WhyTools(_uia, _probe, _session, _audit);
        var result = await tools.ExplainScreen();
        var json = JsonDocument.Parse(result);
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public void GoalPlan_ReturnsError_WhenNotAttached()
    {
        var tools = new IntentTools(_uia, _session, _audit, _recording);
        var result = tools.GoalPlan("fill the form and save");
        var json = JsonDocument.Parse(result);
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public void GoalExecute_ReturnsError_WhenNotAttached()
    {
        var tools = new IntentTools(_uia, _session, _audit, _recording);
        var result = tools.GoalExecute("save the form");
        var json = JsonDocument.Parse(result);
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public void GoalVerify_ReturnsError_WhenNotAttached()
    {
        var tools = new IntentTools(_uia, _session, _audit, _recording);
        var result = tools.GoalVerify("no errors visible");
        var json = JsonDocument.Parse(result);
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public void SmartFill_ReturnsError_WhenNotAttached()
    {
        var tools = new IntentTools(_uia, _session, _audit, _recording);
        var result = tools.SmartFill("{\"Name\": \"Test\"}");
        var json = JsonDocument.Parse(result);
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public void NavigateTo_ReturnsError_WhenNotAttached()
    {
        var tools = new IntentTools(_uia, _session, _audit, _recording);
        var result = tools.NavigateTo("Settings");
        var json = JsonDocument.Parse(result);
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public async Task DevCheck_ReturnsError_WhenNotAttached()
    {
        var tools = new DevWatcherTools(_devWatcher, _uia, _audit);
        var result = await tools.DevCheck();
        var json = JsonDocument.Parse(result);
        // DevCheck reports issues when not attached
        Assert.True(json.RootElement.TryGetProperty("issues", out var issues) ||
                    json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public void DevSuggestIds_ReturnsError_WhenNotAttached()
    {
        var tools = new DevWatcherTools(_devWatcher, _uia, _audit);
        var result = tools.DevSuggestIds();
        var json = JsonDocument.Parse(result);
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public void DevAccessibilityQuick_ReturnsError_WhenNotAttached()
    {
        var tools = new DevWatcherTools(_devWatcher, _uia, _audit);
        var result = tools.DevAccessibilityQuick();
        var json = JsonDocument.Parse(result);
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }
}
