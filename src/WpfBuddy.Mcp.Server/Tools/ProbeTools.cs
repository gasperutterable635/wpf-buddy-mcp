using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using WpfBuddy.Mcp.Server.Services;

namespace WpfBuddy.Mcp.Server.Tools;

[McpServerToolType]
public sealed class ProbeTools
{
    private readonly ProbeClient _probe;
    private readonly SessionManager _session;
    private readonly AuditLog _audit;

    public ProbeTools(ProbeClient probe, SessionManager session, AuditLog audit)
    {
        _probe = probe;
        _session = session;
        _audit = audit;
    }

    [McpServerTool(Name = "wpf_probe_status"), Description("Check if probe is connected and responding.")]
    public async Task<string> ProbeStatus()
    {
        _audit.Record("wpf_probe_status");
        if (!_probe.IsConnected)
            return JsonSerializer.Serialize(new { connected = false, message = "Probe not connected. Use wpf_probe_connect first." }, JsonOptions.Default);

        var response = await _probe.SendAsync("ping");
        return JsonSerializer.Serialize(new
        {
            connected = true,
            pipeName = _probe.PipeName,
            responsive = response?.Result == "pong"
        }, JsonOptions.Default);
    }

    [McpServerTool(Name = "wpf_probe_connect"), Description("Connect to the in-process probe via named pipe.")]
    public async Task<string> ProbeConnect(string? pipeName = null)
    {
        _audit.Record("wpf_probe_connect");
        bool connected;
        if (!string.IsNullOrEmpty(pipeName))
        {
            connected = await _probe.ConnectAsync(pipeName);
        }
        else
        {
            var status = _session.GetStatus();
            if (!status.ProcessId.HasValue || status.ProcessId == 0)
                return JsonSerializer.Serialize(new { error = "No session attached. Attach to an app first." }, JsonOptions.Default);
            connected = await _probe.ConnectAsync(status.ProcessId.Value);
        }

        return JsonSerializer.Serialize(new { connected, pipeName = _probe.PipeName }, JsonOptions.Default);
    }

    [McpServerTool(Name = "wpf_probe_disconnect"), Description("Disconnect from the in-process probe.")]
    public string ProbeDisconnect()
    {
        _audit.Record("wpf_probe_disconnect");
        _probe.Disconnect();
        return JsonSerializer.Serialize(new { result = "disconnected" }, JsonOptions.Default);
    }

    [McpServerTool(Name = "wpf_probe_capabilities"), Description("List methods supported by connected probe.")]
    public string ProbeCapabilities()
    {
        _audit.Record("wpf_probe_capabilities");
        var methods = new[]
        {
            "ping", "get_datacontext", "get_viewmodel_properties", "get_binding_errors",
            "get_bindings", "get_command_state", "get_validation_state", "execute_command",
            "get_dispatcher_status"
        };
        return JsonSerializer.Serialize(new { connected = _probe.IsConnected, methods }, JsonOptions.Default);
    }

    [McpServerTool(Name = "wpf_probe_health"), Description("Run probe health check.")]
    public async Task<string> ProbeHealth()
    {
        _audit.Record("wpf_probe_health");
        if (!_probe.IsConnected)
            return JsonSerializer.Serialize(new { healthy = false, error = "Not connected." }, JsonOptions.Default);

        var ping = await _probe.SendAsync("ping");
        var dispatcher = await _probe.SendAsync("get_dispatcher_status");

        return JsonSerializer.Serialize(new
        {
            healthy = ping?.Result == "pong",
            pingOk = ping?.Result == "pong",
            dispatcherOk = dispatcher?.Error is null,
            dispatcherStatus = dispatcher?.Data
        }, JsonOptions.Default);
    }

    [McpServerTool(Name = "wpf_probe_install_instructions"), Description("Return instructions for installing the probe NuGet in a WPF app.")]
    public string ProbeInstallInstructions()
    {
        _audit.Record("wpf_probe_install_instructions");
        var instructions = @"
## Install the WpfBuddy MCP Probe

1. Add the NuGet package to your WPF project:
   ```
   dotnet add package WpfBuddy.Mcp.Probe
   ```

2. In your App.xaml.cs, add:
   ```csharp
   using WpfBuddy.Mcp.Probe;

   protected override void OnStartup(StartupEventArgs e)
   {
       base.OnStartup(e);
       ProbeHost.Start(); // pipe name auto-generated from PID
   }
   ```

3. The probe will listen on named pipe: `wpfbuddy-mcp-probe-{ProcessId}`

4. Connect from MCP tools using `wpf_probe_connect`.
";
        return JsonSerializer.Serialize(new { instructions }, JsonOptions.Default);
    }
}
