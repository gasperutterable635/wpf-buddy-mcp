using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace WpfBuddy.Mcp.Server.Services;

/// <summary>
/// Client for communicating with the in-process probe via named pipe.
/// </summary>
public sealed class ProbeClient : IDisposable
{
    private NamedPipeClientStream? _pipe;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private string? _pipeName;

    public bool IsConnected => _pipe?.IsConnected == true;
    public string? PipeName => _pipeName;

    public async Task<bool> ConnectAsync(int processId, int timeoutMs = 3000)
    {
        _pipeName = $"wpfbuddy-mcp-probe-{processId}";
        return await ConnectToPipeAsync(_pipeName, timeoutMs);
    }

    public async Task<bool> ConnectAsync(string pipeName, int timeoutMs = 3000)
    {
        _pipeName = pipeName;
        return await ConnectToPipeAsync(pipeName, timeoutMs);
    }

    private async Task<bool> ConnectToPipeAsync(string pipeName, int timeoutMs)
    {
        try
        {
            Disconnect();
            _pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await _pipe.ConnectAsync(timeoutMs);
            _reader = new StreamReader(_pipe, Encoding.UTF8);
            _writer = new StreamWriter(_pipe, Encoding.UTF8) { AutoFlush = true };
            return true;
        }
        catch
        {
            Disconnect();
            return false;
        }
    }

    public async Task<ProbeResponse?> SendAsync(string method, Dictionary<string, string>? parameters = null, int timeoutMs = 10000)
    {
        if (!IsConnected || _writer is null || _reader is null)
            return new ProbeResponse { Error = "Not connected to probe." };

        try
        {
            var request = JsonSerializer.Serialize(new { method, parameters });
            await _writer.WriteLineAsync(request);

            using var cts = new CancellationTokenSource(timeoutMs);
            var responseLine = await _reader.ReadLineAsync(cts.Token);
            if (responseLine is null)
                return new ProbeResponse { Error = "No response from probe." };
            return JsonSerializer.Deserialize<ProbeResponse>(responseLine);
        }
        catch (OperationCanceledException)
        {
            return new ProbeResponse { Error = "Probe request timed out." };
        }
        catch (Exception ex)
        {
            return new ProbeResponse { Error = ex.Message };
        }
    }

    public void Disconnect()
    {
        _reader?.Dispose();
        _writer?.Dispose();
        _pipe?.Dispose();
        _reader = null;
        _writer = null;
        _pipe = null;
    }

    public void Dispose() => Disconnect();
}

public class ProbeResponse
{
    public string? Result { get; set; }
    public string? Data { get; set; }
    public string? Error { get; set; }
}
