using System.Reflection;
using MultiAgentFramework.Contracts;

namespace MultiAgentFramework.Discovery;

public sealed class ToolCatalogGenerator
{
    public IReadOnlyDictionary<string, ToolCatalogItem> BuildCatalog(IEnumerable<object> skills)
    {
        var catalog = new Dictionary<string, ToolCatalogItem>(StringComparer.OrdinalIgnoreCase);

        foreach (var skill in skills)
        {
            var methods = skill.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var tool = method.GetCustomAttribute<ToolAttribute>();
                if (tool is null)
                {
                    continue;
                }

                var version = method.GetCustomAttribute<ToolVersionAttribute>()?.Value
                    ?? throw new InvalidOperationException($"Tool '{tool.Name}' missing ToolVersionAttribute.");
                var tier = method.GetCustomAttribute<ToolTierAttribute>()?.Value
                    ?? throw new InvalidOperationException($"Tool '{tool.Name}' missing ToolTierAttribute.");
                var stability = method.GetCustomAttribute<ToolStabilityAttribute>()?.Value
                    ?? throw new InvalidOperationException($"Tool '{tool.Name}' missing ToolStabilityAttribute.");

                if (catalog.ContainsKey(tool.Name))
                {
                    throw new InvalidOperationException($"Duplicate tool name found: '{tool.Name}'.");
                }

                catalog[tool.Name] = new ToolCatalogItem(
                    Name: tool.Name,
                    Description: tool.Description,
                    Version: version,
                    Tier: tier,
                    Stability: stability,
                    Method: method,
                    SkillInstance: skill,
                    Parameters: method.GetParameters().Select(p => p.Name ?? string.Empty).ToArray());
            }
        }

        return catalog;
    }

    public string ToJson(IReadOnlyDictionary<string, ToolCatalogItem> catalog)
    {
        var payload = catalog.Values.Select(t => new
        {
            t.Name,
            t.Description,
            t.Version,
            t.Tier,
            t.Stability,
            parameters = t.Parameters
        });

        return System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
