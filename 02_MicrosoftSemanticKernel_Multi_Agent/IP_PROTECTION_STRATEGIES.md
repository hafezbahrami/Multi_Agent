# IP Protection Strategies for MCP-Style Multi-Agent Systems

This project now demonstrates a simple pattern for keeping company IP inside agents while still allowing LLM-driven orchestration.

## Threat Model

If an LLM can discover and call arbitrary class methods, it can accidentally reach internal behavior or metadata.

Examples of risk:
- LLM sees internal method names and tries to invoke them.
- Internal fields (policy versions, heuristics, ranking rules) are returned in tool outputs.
- Tool outputs become future prompts and leak internal implementation details.

## Strategy 1: Explicit API Boundary (Allowlist)

Implement one public API contract per agent and expose only those tools.

In this project:
- `skills` are passed as a mapping of `tool_name -> skill_instance`.
- The planner only exposes `tool_name` if the skill instance has a callable method with the same name.
- The LLM prompt lists only these allowlisted tools.

Result:
- Internal helper methods are not discoverable by the LLM.
- The public interface is stable and auditable.

## Strategy 2: Parameter Contract Enforcement

Tool calls are normalized and filtered by the method signature before execution.

In this project:
- Common LLM naming mistakes are mapped (`content -> body`, etc.).
- Unknown parameters are removed before calling the tool.

Result:
- LLM cannot inject extra fields to influence hidden behavior.
- Each tool receives only parameters it is designed to handle.

## Strategy 3: Output Sanitization

Treat every tool response as potentially prompt-visible data.

In this project:
- The agent removes fields that look internal/sensitive (`internal`, `secret`, `policy`, `heuristic`, `strategy`, `ip`).

Result:
- Accidental leakage from skill output is reduced.

## Strategy 4: Keep Internal Logic Private in Skills

Skills should contain private state and helper logic that is never part of the API.

In this project:
- Skills include private attributes like `_priority_policy`, `_scheduling_heuristics`, `_ranking_profile`.
- Only public methods (`send_email`, `create_event`, `create_task`, `search`) return safe output contracts.

Result:
- Business logic/IP remains in agent internals.
- The framework exposes only business-safe outcomes.

## Minimal Design Rule

Use this simple rule:

1. Agent internals: private, hidden, mutable.
2. Tool API: explicit, allowlisted, versioned.
3. Tool I/O: validated and sanitized.
4. Prompts: never include private data, only API docs.

This gives you an educational and practical baseline that can scale to stricter controls (RBAC, audit logs, policy engines, and signed tool manifests).
