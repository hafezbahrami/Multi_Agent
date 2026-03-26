using System.Reflection;

namespace McpTaskExample.Contracts;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ToolAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public ToolAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class ToolVersionAttribute : Attribute
{
    public string Version { get; }

    public ToolVersionAttribute(string version)
    {
        Version = version;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class ToolTierAttribute : Attribute
{
    public string Tier { get; }

    public ToolTierAttribute(string tier)
    {
        Tier = tier;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class ToolStabilityAttribute : Attribute
{
    public string Stability { get; }

    public ToolStabilityAttribute(string stability)
    {
        Stability = stability;
    }
}

public sealed record ToolMetadata(
    string Name,
    string Description,
    string Version,
    string Tier,
    string Stability,
    MethodInfo Method,
    object SkillInstance,
    IReadOnlyList<string> ParameterNames);
