using System.Drawing;
using System.Drawing.Imaging;
using FlaUI.Core.AutomationElements;

namespace WpfBuddy.Mcp.Server.Services;

public sealed class ScreenshotService
{
    private readonly SessionManager _session;

    public ScreenshotService(SessionManager session)
    {
        _session = session;
    }

    public byte[] CaptureWindow()
    {
        if (!_session.IsAttached || _session.ActiveWindow is null)
            throw new InvalidOperationException("No window attached.");

        var window = _session.ActiveWindow;
        var bounds = window.BoundingRectangle;

        if (bounds.Width <= 0 || bounds.Height <= 0)
            throw new InvalidOperationException("Window has no visible bounds (is it minimized?).");

        using var bitmap = new Bitmap((int)bounds.Width, (int)bounds.Height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen((int)bounds.X, (int)bounds.Y, 0, 0, new Size((int)bounds.Width, (int)bounds.Height));

        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    public byte[] CaptureElement(AutomationElement element)
    {
        var bounds = element.BoundingRectangle;
        if (bounds.Width <= 0 || bounds.Height <= 0)
            throw new InvalidOperationException("Element has no visible bounds.");

        using var bitmap = new Bitmap((int)bounds.Width, (int)bounds.Height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen((int)bounds.X, (int)bounds.Y, 0, 0, new Size((int)bounds.Width, (int)bounds.Height));

        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    public Rectangle GetWindowBounds()
    {
        if (!_session.IsAttached || _session.ActiveWindow is null)
            throw new InvalidOperationException("No window attached.");

        var bounds = _session.ActiveWindow.BoundingRectangle;
        return new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
    }
}
