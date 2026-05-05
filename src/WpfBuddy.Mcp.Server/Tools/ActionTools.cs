using System.ComponentModel;
using System.Text.Json;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using ModelContextProtocol.Server;
using WpfBuddy.Mcp.Server.Models;
using WpfBuddy.Mcp.Server.Services;

namespace WpfBuddy.Mcp.Server.Tools;

[McpServerToolType]
public sealed class ActionTools
{
    private readonly UiaAdapter _uia;
    private readonly AuditLog _audit;
    private readonly RecordingService _recording;

    public ActionTools(UiaAdapter uia, AuditLog audit, RecordingService recording)
    {
        _uia = uia;
        _audit = audit;
        _recording = recording;
    }

    [McpServerTool(Name = "wpf_invoke"), Description("Invoke Button, MenuItem, Hyperlink through UIA InvokePattern.")]
    public string Invoke(string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_invoke", criteria);

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        if (!element.Patterns.Invoke.IsSupported)
            return Error("Element does not support Invoke pattern.");

        element.Patterns.Invoke.Pattern.Invoke();
        RecordAction("invoke", criteria);
        return Ok("invoked");
    }

    [McpServerTool(Name = "wpf_click"), Description("Click element using pattern if available, coordinates as fallback.")]
    public string Click(string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_click", criteria);

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        if (element.Patterns.Invoke.IsSupported)
        {
            element.Patterns.Invoke.Pattern.Invoke();
        }
        else
        {
            element.Click();
        }

        RecordAction("click", criteria);
        return Ok("clicked");
    }

    [McpServerTool(Name = "wpf_set_value"), Description("Set text/value via ValuePattern.")]
    public string SetValue(string value, string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_set_value", criteria, new() { ["value"] = "***" });

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        if (!element.Patterns.Value.IsSupported)
            return Error("Element does not support Value pattern.");

        element.Patterns.Value.Pattern.SetValue(value);
        RecordAction("set_value", criteria, value);
        return Ok("value_set");
    }

    [McpServerTool(Name = "wpf_clear_value"), Description("Clear text/value from element.")]
    public string ClearValue(string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_clear_value", criteria);

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        if (element.Patterns.Value.IsSupported)
        {
            element.Patterns.Value.Pattern.SetValue(string.Empty);
        }

        RecordAction("clear_value", criteria);
        return Ok("cleared");
    }

    [McpServerTool(Name = "wpf_type_text"), Description("Type text into focused or selected element via keyboard input.")]
    public string TypeText(string text, string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_type_text", criteria, new() { ["text"] = "***" });

        if (!string.IsNullOrEmpty(automationId) || !string.IsNullOrEmpty(name))
        {
            var element = _uia.FindElement(criteria);
            if (element is null)
                return Error("Element not found.");
            element.Focus();
        }

        Keyboard.Type(text);
        RecordAction("type_text", criteria, text);
        return Ok("typed");
    }

    [McpServerTool(Name = "wpf_send_keys"), Description("Send keyboard shortcuts (e.g., Ctrl+S, Enter, Tab).")]
    public string SendKeys(string keys)
    {
        _audit.Record("wpf_send_keys", parameters: new() { ["keys"] = keys });

        // Parse common key names
        var keyParts = keys.Split('+').Select(k => k.Trim().ToLowerInvariant()).ToArray();
        var modifiers = new List<VirtualKeyShort>();
        VirtualKeyShort? mainKey = null;

        foreach (var part in keyParts)
        {
            switch (part)
            {
                case "ctrl" or "control": modifiers.Add(VirtualKeyShort.CONTROL); break;
                case "alt": modifiers.Add(VirtualKeyShort.ALT); break;
                case "shift": modifiers.Add(VirtualKeyShort.SHIFT); break;
                case "enter" or "return": mainKey = VirtualKeyShort.ENTER; break;
                case "tab": mainKey = VirtualKeyShort.TAB; break;
                case "escape" or "esc": mainKey = VirtualKeyShort.ESCAPE; break;
                case "delete" or "del": mainKey = VirtualKeyShort.DELETE; break;
                case "backspace": mainKey = VirtualKeyShort.BACK; break;
                case "space": mainKey = VirtualKeyShort.SPACE; break;
                case "home": mainKey = VirtualKeyShort.HOME; break;
                case "end": mainKey = VirtualKeyShort.END; break;
                case "up": mainKey = VirtualKeyShort.UP; break;
                case "down": mainKey = VirtualKeyShort.DOWN; break;
                case "left": mainKey = VirtualKeyShort.LEFT; break;
                case "right": mainKey = VirtualKeyShort.RIGHT; break;
                default:
                    if (part.Length == 1 && char.IsLetterOrDigit(part[0]))
                    {
                        mainKey = (VirtualKeyShort)char.ToUpperInvariant(part[0]);
                    }
                    else if (part.StartsWith("f") && int.TryParse(part[1..], out var fNum) && fNum >= 1 && fNum <= 12)
                    {
                        mainKey = (VirtualKeyShort)((int)VirtualKeyShort.F1 + fNum - 1);
                    }
                    break;
            }
        }

        if (mainKey is null)
            return Error("Could not parse key combination.");

        foreach (var mod in modifiers) Keyboard.Press(mod);
        Keyboard.Press(mainKey.Value);
        Keyboard.Release(mainKey.Value);
        foreach (var mod in modifiers) Keyboard.Release(mod);

        return Ok("keys_sent");
    }

    [McpServerTool(Name = "wpf_focus"), Description("Move focus to element.")]
    public string Focus(string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_focus", criteria);

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        element.Focus();
        return Ok("focused");
    }

    [McpServerTool(Name = "wpf_select"), Description("Select list/grid/tree/combo item by automation id or name.")]
    public string Select(string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_select", criteria);

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        if (!element.Patterns.SelectionItem.IsSupported)
            return Error("Element does not support SelectionItem pattern.");

        element.Patterns.SelectionItem.Pattern.Select();
        RecordAction("select", criteria);
        return Ok("selected");
    }

    [McpServerTool(Name = "wpf_select_by_text"), Description("Select item in a list/combo by visible text.")]
    public string SelectByText(string text, string? parentAutomationId = null, string? parentName = null)
    {
        _audit.Record("wpf_select_by_text", parameters: new() { ["text"] = text });

        FlaUI.Core.AutomationElements.AutomationElement? parent = null;
        if (!string.IsNullOrEmpty(parentAutomationId) || !string.IsNullOrEmpty(parentName))
        {
            parent = _uia.FindElement(new ElementCriteria { AutomationId = parentAutomationId, Name = parentName });
        }

        var criteria = new ElementCriteria { Name = text };
        var element = _uia.FindElement(criteria, parent);
        if (element is null)
            return Error($"Item with text '{text}' not found.");

        if (element.Patterns.SelectionItem.IsSupported)
        {
            element.Patterns.SelectionItem.Pattern.Select();
        }
        else
        {
            element.Click();
        }

        RecordAction("select", new ElementCriteria { Name = text });
        return Ok("selected");
    }

    [McpServerTool(Name = "wpf_toggle"), Description("Toggle checkbox, toggle button, or expander.")]
    public string Toggle(string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_toggle", criteria);

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        if (!element.Patterns.Toggle.IsSupported)
            return Error("Element does not support Toggle pattern.");

        element.Patterns.Toggle.Pattern.Toggle();
        RecordAction("toggle", criteria);
        return Ok("toggled");
    }

    [McpServerTool(Name = "wpf_check"), Description("Ensure checkbox is checked.")]
    public string Check(string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_check", criteria);

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        if (!element.Patterns.Toggle.IsSupported)
            return Error("Element does not support Toggle pattern.");

        // Loop to handle 3-state checkboxes (Off → On → Indeterminate → Off)
        for (int i = 0; i < 3; i++)
        {
            var state = element.Patterns.Toggle.Pattern.ToggleState.ValueOrDefault;
            if (state == FlaUI.Core.Definitions.ToggleState.On)
                break;
            element.Patterns.Toggle.Pattern.Toggle();
        }

        return Ok("checked");
    }

    [McpServerTool(Name = "wpf_uncheck"), Description("Ensure checkbox is unchecked.")]
    public string Uncheck(string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_uncheck", criteria);

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        if (!element.Patterns.Toggle.IsSupported)
            return Error("Element does not support Toggle pattern.");

        // Loop to handle 3-state checkboxes (Off → On → Indeterminate → Off)
        for (int i = 0; i < 3; i++)
        {
            var state = element.Patterns.Toggle.Pattern.ToggleState.ValueOrDefault;
            if (state == FlaUI.Core.Definitions.ToggleState.Off)
                break;
            element.Patterns.Toggle.Pattern.Toggle();
        }

        return Ok("unchecked");
    }

    [McpServerTool(Name = "wpf_expand"), Description("Expand combo/tree/expander/menu.")]
    public string Expand(string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_expand", criteria);

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        if (!element.Patterns.ExpandCollapse.IsSupported)
            return Error("Element does not support ExpandCollapse pattern.");

        element.Patterns.ExpandCollapse.Pattern.Expand();
        RecordAction("expand", criteria);
        return Ok("expanded");
    }

    [McpServerTool(Name = "wpf_collapse"), Description("Collapse combo/tree/expander/menu.")]
    public string Collapse(string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_collapse", criteria);

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        if (!element.Patterns.ExpandCollapse.IsSupported)
            return Error("Element does not support ExpandCollapse pattern.");

        element.Patterns.ExpandCollapse.Pattern.Collapse();
        RecordAction("collapse", criteria);
        return Ok("collapsed");
    }

    [McpServerTool(Name = "wpf_scroll_into_view"), Description("Scroll element into view.")]
    public string ScrollIntoView(string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_scroll_into_view", criteria);

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        if (element.Patterns.ScrollItem.IsSupported)
        {
            element.Patterns.ScrollItem.Pattern.ScrollIntoView();
            return Ok("scrolled_into_view");
        }

        return Error("Element does not support ScrollItem pattern.");
    }

    [McpServerTool(Name = "wpf_double_click"), Description("Double-click element.")]
    public string DoubleClick(string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_double_click", criteria);

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        element.DoubleClick();
        RecordAction("double_click", criteria);
        return Ok("double_clicked");
    }

    [McpServerTool(Name = "wpf_right_click"), Description("Right-click element to open context menu.")]
    public string RightClick(string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_right_click", criteria);

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        element.RightClick();
        RecordAction("right_click", criteria);
        return Ok("right_clicked");
    }

    [McpServerTool(Name = "wpf_select_by_index"), Description("Select item by index in a list/combo. Marked as brittle.")]
    public string SelectByIndex(int index, string? parentAutomationId = null, string? parentName = null)
    {
        _audit.Record("wpf_select_by_index", parameters: new() { ["index"] = index });

        FlaUI.Core.AutomationElements.AutomationElement? parent = null;
        if (!string.IsNullOrEmpty(parentAutomationId) || !string.IsNullOrEmpty(parentName))
        {
            parent = _uia.FindElement(new ElementCriteria { AutomationId = parentAutomationId, Name = parentName });
            if (parent is null)
                return Error("Parent element not found.");
        }

        var items = _uia.FindElements(new ElementCriteria { ControlType = "ListItem" }, parent);
        if (items.Count == 0)
            items = _uia.FindElements(new ElementCriteria { ControlType = "TreeItem" }, parent);
        if (items.Count == 0)
            items = _uia.FindElements(new ElementCriteria { ControlType = "DataItem" }, parent);

        if (index < 0 || index >= items.Count)
            return Error($"Index {index} out of range. Found {items.Count} items.");

        var target = items[index];
        if (target.Patterns.SelectionItem.IsSupported)
            target.Patterns.SelectionItem.Pattern.Select();
        else
            target.Click();

        return Ok($"selected_index_{index}");
    }

    [McpServerTool(Name = "wpf_scroll"), Description("Scroll container by direction and amount.")]
    public string Scroll(string direction, double amount = 1.0, string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_scroll", criteria, new() { ["direction"] = direction, ["amount"] = amount });

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        if (!element.Patterns.Scroll.IsSupported)
            return Error("Element does not support Scroll pattern.");

        var scrollPattern = element.Patterns.Scroll.Pattern;
        switch (direction.ToLowerInvariant())
        {
            case "up":
                scrollPattern.Scroll(FlaUI.Core.Definitions.ScrollAmount.NoAmount, FlaUI.Core.Definitions.ScrollAmount.SmallDecrement);
                break;
            case "down":
                scrollPattern.Scroll(FlaUI.Core.Definitions.ScrollAmount.NoAmount, FlaUI.Core.Definitions.ScrollAmount.SmallIncrement);
                break;
            case "left":
                scrollPattern.Scroll(FlaUI.Core.Definitions.ScrollAmount.SmallDecrement, FlaUI.Core.Definitions.ScrollAmount.NoAmount);
                break;
            case "right":
                scrollPattern.Scroll(FlaUI.Core.Definitions.ScrollAmount.SmallIncrement, FlaUI.Core.Definitions.ScrollAmount.NoAmount);
                break;
            default:
                return Error($"Unknown direction: {direction}. Use up, down, left, right.");
        }

        return Ok("scrolled");
    }

    [McpServerTool(Name = "wpf_open_menu_path"), Description("Open menu path such as 'File > Export > PDF'.")]
    public string OpenMenuPath(string menuPath)
    {
        _audit.Record("wpf_open_menu_path", parameters: new() { ["path"] = menuPath });

        var parts = menuPath.Split(new[] { ">", " > ", " → " }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
            return Error("Menu path is empty.");

        foreach (var part in parts)
        {
            var criteria = new ElementCriteria { Name = part, ControlType = "MenuItem" };
            var menuItem = _uia.FindElement(criteria);
            if (menuItem is null)
            {
                // Try by automation id
                criteria = new ElementCriteria { AutomationId = part };
                menuItem = _uia.FindElement(criteria);
            }
            if (menuItem is null)
                return Error($"Menu item '{part}' not found.");

            if (menuItem.Patterns.ExpandCollapse.IsSupported)
                menuItem.Patterns.ExpandCollapse.Pattern.Expand();
            else if (menuItem.Patterns.Invoke.IsSupported)
                menuItem.Patterns.Invoke.Pattern.Invoke();
            else
                menuItem.Click();

            Thread.Sleep(100);
        }

        RecordAction("open_menu_path", null, menuPath);
        return Ok("menu_opened");
    }

    [McpServerTool(Name = "wpf_open_context_menu_item"), Description("Right-click target and invoke context menu item.")]
    public string OpenContextMenuItem(string menuItemName, string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_open_context_menu_item", criteria, new() { ["menuItem"] = menuItemName });

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        element.RightClick();
        Thread.Sleep(200);

        // Find the context menu item
        var menuCriteria = new ElementCriteria { Name = menuItemName, ControlType = "MenuItem" };
        var menuItem = _uia.FindElement(menuCriteria);
        if (menuItem is null)
            return Error($"Context menu item '{menuItemName}' not found.");

        if (menuItem.Patterns.Invoke.IsSupported)
            menuItem.Patterns.Invoke.Pattern.Invoke();
        else
            menuItem.Click();

        return Ok("context_menu_item_invoked");
    }

    [McpServerTool(Name = "wpf_drag_drop"), Description("Drag source element to target element.")]
    public string DragDrop(string sourceAutomationId, string targetAutomationId)
    {
        _audit.Record("wpf_drag_drop", parameters: new() { ["source"] = sourceAutomationId, ["target"] = targetAutomationId });

        var source = _uia.FindElement(new ElementCriteria { AutomationId = sourceAutomationId });
        if (source is null)
            return Error("Source element not found.");

        var target = _uia.FindElement(new ElementCriteria { AutomationId = targetAutomationId });
        if (target is null)
            return Error("Target element not found.");

        var srcBounds = source.BoundingRectangle;
        var tgtBounds = target.BoundingRectangle;
        var srcCenter = new System.Drawing.Point((int)(srcBounds.X + srcBounds.Width / 2), (int)(srcBounds.Y + srcBounds.Height / 2));
        var tgtCenter = new System.Drawing.Point((int)(tgtBounds.X + tgtBounds.Width / 2), (int)(tgtBounds.Y + tgtBounds.Height / 2));

        Mouse.MoveTo(srcCenter);
        Mouse.Down(MouseButton.Left);
        Thread.Sleep(100);
        Mouse.MoveTo(tgtCenter);
        Thread.Sleep(100);
        Mouse.Up(MouseButton.Left);

        return Ok("drag_drop_completed");
    }

    [McpServerTool(Name = "wpf_set_slider"), Description("Set Slider/RangeBase value.")]
    public string SetSlider(double value, string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_set_slider", criteria, new() { ["value"] = value });

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        if (!element.Patterns.RangeValue.IsSupported)
            return Error("Element does not support RangeValue pattern.");

        element.Patterns.RangeValue.Pattern.SetValue(value);
        return Ok("slider_set");
    }

    [McpServerTool(Name = "wpf_set_date"), Description("Set DatePicker/Calendar date by typing text value.")]
    public string SetDate(string date, string? automationId = null, string? name = null)
    {
        var criteria = new ElementCriteria { AutomationId = automationId, Name = name };
        _audit.Record("wpf_set_date", criteria, new() { ["date"] = date });

        var element = _uia.FindElement(criteria);
        if (element is null)
            return Error("Element not found.");

        if (element.Patterns.Value.IsSupported)
        {
            element.Patterns.Value.Pattern.SetValue(date);
            return Ok("date_set");
        }

        // Fallback: focus and type
        element.Focus();
        Keyboard.Type(date);
        return Ok("date_typed");
    }

    [McpServerTool(Name = "wpf_accept_dialog"), Description("Click OK/Yes/Accept on current modal dialog.")]
    public string AcceptDialog()
    {
        _audit.Record("wpf_accept_dialog");

        // Try common accept button names
        string[] acceptNames = ["OK", "Ok", "Yes", "Accept", "Confirm", "Save"];
        foreach (var buttonName in acceptNames)
        {
            var criteria = new ElementCriteria { Name = buttonName, ControlType = "Button" };
            var element = _uia.FindElement(criteria);
            if (element is not null)
            {
                if (element.Patterns.Invoke.IsSupported)
                    element.Patterns.Invoke.Pattern.Invoke();
                else
                    element.Click();
                return Ok($"accepted_via_{buttonName}");
            }
        }

        return Error("No accept/OK button found in current window.");
    }

    [McpServerTool(Name = "wpf_cancel_dialog"), Description("Click Cancel/No/Close on current modal dialog.")]
    public string CancelDialog()
    {
        _audit.Record("wpf_cancel_dialog");

        string[] cancelNames = ["Cancel", "No", "Close", "Abort"];
        foreach (var buttonName in cancelNames)
        {
            var criteria = new ElementCriteria { Name = buttonName, ControlType = "Button" };
            var element = _uia.FindElement(criteria);
            if (element is not null)
            {
                if (element.Patterns.Invoke.IsSupported)
                    element.Patterns.Invoke.Pattern.Invoke();
                else
                    element.Click();
                return Ok($"cancelled_via_{buttonName}");
            }
        }

        return Error("No cancel/close button found in current window.");
    }

    private void RecordAction(string action, ElementCriteria? selector, string? value = null)
    {
        if (_recording.IsRecording)
        {
            _recording.AddStep(new RecordingStep
            {
                Action = action,
                Selector = selector,
                Value = value
            });
        }
    }

    private static string Ok(string result) =>
        JsonSerializer.Serialize(new { result }, JsonOptions.Default);

    private static string Error(string message) =>
        JsonSerializer.Serialize(new { error = message }, JsonOptions.Default);
}
