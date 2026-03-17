namespace MultiAgentFramework.Skills;

/// <summary>
/// Base interface for all skills. Each skill exposes a Description
/// and one or more public tool methods.
/// </summary>
public interface ISkill
{
    string Description { get; }
}
