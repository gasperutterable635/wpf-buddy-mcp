using FlaUI.Core.AutomationElements;
using WpfBuddy.Mcp.Server.Models;

namespace WpfBuddy.Mcp.Server.Services;

public sealed class SelectorBuilder
{
    private readonly UiaAdapter _uiaAdapter;

    public SelectorBuilder(UiaAdapter uiaAdapter)
    {
        _uiaAdapter = uiaAdapter;
    }

    public ElementSelector BuildSelector(AutomationElement element)
    {
        var automationId = element.Properties.AutomationId.ValueOrDefault;
        var name = element.Properties.Name.ValueOrDefault;
        var controlType = element.Properties.ControlType.ValueOrDefault.ToString();
        var className = element.Properties.ClassName.ValueOrDefault;

        var primary = new ElementCriteria();
        var fallbacks = new List<ElementCriteria>();

        // Best: AutomationId
        if (!string.IsNullOrEmpty(automationId))
        {
            primary.AutomationId = automationId;
            primary.ControlType = controlType;

            // Fallback: name + control type
            if (!string.IsNullOrEmpty(name))
            {
                fallbacks.Add(new ElementCriteria
                {
                    Name = name,
                    ControlType = controlType
                });
            }
        }
        else if (!string.IsNullOrEmpty(name))
        {
            // No AutomationId, use Name + ControlType
            primary.Name = name;
            primary.ControlType = controlType;

            if (!string.IsNullOrEmpty(className))
            {
                fallbacks.Add(new ElementCriteria
                {
                    ClassName = className,
                    ControlType = controlType
                });
            }
        }
        else
        {
            // Worst case: className + control type
            primary.ClassName = className;
            primary.ControlType = controlType;
        }

        return new ElementSelector
        {
            Element = primary,
            Fallbacks = fallbacks.Count > 0 ? fallbacks : null
        };
    }

    public bool ValidateSelector(ElementSelector selector)
    {
        var element = _uiaAdapter.ResolveSelector(selector);
        return element is not null;
    }

    public (bool isUnique, int matchCount) ValidateSelectorUniqueness(ElementCriteria criteria)
    {
        var elements = _uiaAdapter.FindElements(criteria);
        return (elements.Count == 1, elements.Count);
    }

    public List<(ElementSelector selector, string strategy, int stability)> RankSelectors(AutomationElement element)
    {
        var results = new List<(ElementSelector selector, string strategy, int stability)>();
        var automationId = element.Properties.AutomationId.ValueOrDefault;
        var name = element.Properties.Name.ValueOrDefault;
        var controlType = element.Properties.ControlType.ValueOrDefault.ToString();
        var className = element.Properties.ClassName.ValueOrDefault;

        if (!string.IsNullOrEmpty(automationId))
        {
            var sel = new ElementSelector { Element = new ElementCriteria { AutomationId = automationId } };
            results.Add((sel, "AutomationId", 95));
        }

        if (!string.IsNullOrEmpty(automationId) && !string.IsNullOrEmpty(controlType))
        {
            var sel = new ElementSelector { Element = new ElementCriteria { AutomationId = automationId, ControlType = controlType } };
            results.Add((sel, "AutomationId+ControlType", 98));
        }

        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(controlType))
        {
            var sel = new ElementSelector { Element = new ElementCriteria { Name = name, ControlType = controlType } };
            results.Add((sel, "Name+ControlType", 70));
        }

        if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(controlType))
        {
            var sel = new ElementSelector { Element = new ElementCriteria { ClassName = className, ControlType = controlType } };
            results.Add((sel, "ClassName+ControlType", 40));
        }

        return results.OrderByDescending(r => r.stability).ToList();
    }
}
