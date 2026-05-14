using System.Reflection;
using McpTaskExample.Contracts;

namespace McpTaskExample.Discovery;

public sealed class ToolCatalogGenerator
{
    public IReadOnlyDictionary<string, ToolMetadata> BuildCatalog(IEnumerable<object> skills)
    {
        var catalog = new Dictionary<string, ToolMetadata>(StringComparer.OrdinalIgnoreCase);

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

                var version = method.GetCustomAttribute<ToolVersionAttribute>()?.Version
                    ?? throw new InvalidOperationException($"Tool '{tool.Name}' missing ToolVersionAttribute.");
                var tier = method.GetCustomAttribute<ToolTierAttribute>()?.Tier
                    ?? throw new InvalidOperationException($"Tool '{tool.Name}' missing ToolTierAttribute.");
                var stability = method.GetCustomAttribute<ToolStabilityAttribute>()?.Stability
                    ?? throw new InvalidOperationException($"Tool '{tool.Name}' missing ToolStabilityAttribute.");

                if (catalog.ContainsKey(tool.Name))
                {
                    throw new InvalidOperationException($"Duplicate tool name detected: {tool.Name}");
                }

                var parameters = method.GetParameters().Select(p => p.Name ?? string.Empty).ToArray();
                catalog[tool.Name] = new ToolMetadata(
                    tool.Name,
                    tool.Description,
                    version,
                    tier,
                    stability,
                    method,
                    skill,
                    parameters);
            }
        }

        return catalog;
    }

    public string ToJson(IReadOnlyDictionary<string, ToolMetadata> catalog)
    {
        var document = catalog.Values
            .Select(t => new
            {
                t.Name,
                t.Description,
                t.Version,
                t.Tier,
                t.Stability,
                parameters = t.ParameterNames
            });

        return System.Text.Json.JsonSerializer.Serialize(document, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
