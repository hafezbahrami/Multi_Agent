using System.Reflection;

namespace MultiAgentFramework.Contracts;

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
    public string Value { get; }

    public ToolVersionAttribute(string value)
    {
        Value = value;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class ToolTierAttribute : Attribute
{
    public string Value { get; }

    public ToolTierAttribute(string value)
    {
        Value = value;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class ToolStabilityAttribute : Attribute
{
    public string Value { get; }

    public ToolStabilityAttribute(string value)
    {
        Value = value;
    }
}

public sealed record ToolCatalogItem(
    string Name,
    string Description,
    string Version,
    string Tier,
    string Stability,
    MethodInfo Method,
    object SkillInstance,
    IReadOnlyList<string> Parameters);
