import os
import sys

CURRENT_DIR = os.path.dirname(__file__)
PROJECT_DIR = os.path.dirname(CURRENT_DIR)
if PROJECT_DIR not in sys.path:
    sys.path.insert(0, PROJECT_DIR)

from multi_agent_planner import Agent
from skills_mcp_servers import EmailSkill


class DummyLLM:
    def __init__(self, canned_response):
        self.canned_response = canned_response

    def chat(self, messages, tools=None):
        return self.canned_response


class SensitiveEchoSkill:
    description = "Echo skill that returns sensitive fields for test"

    def echo(self, text: str):
        return {
            "status": "success",
            "text": text,
            "internal_policy_version": "v99",
            "strategy_notes": "confidential",
        }


class TestIPProtection:
    def test_private_helpers_not_exposed_in_api_contract(self):
        email_skill = EmailSkill()
        llm = DummyLLM({"tool": "send_email", "parameters": {"to": "a@b.com", "body": "hello"}})
        agent = Agent(name="WorkflowAgent", skills={"send_email": email_skill}, llm_client=llm)

        # Public API is allowlisted.
        assert "send_email" in agent.api_contract

        # Private methods/attributes are never exposed as tools.
        assert "_sanitize_body" not in agent.api_contract
        assert "_select_mail_channel" not in agent.api_contract
        assert "_routing_policy_version" not in agent.api_contract

    def test_unexposed_tool_is_blocked(self):
        email_skill = EmailSkill()
        llm = DummyLLM({"tool": "_sanitize_body", "parameters": {"body": "x"}})
        agent = Agent(name="WorkflowAgent", skills={"send_email": email_skill}, llm_client=llm)

        result = agent.execute("try calling private method")
        assert "error" in result
        assert "not exposed" in result["error"]

    def test_internal_fields_are_sanitized_from_output(self):
        llm = DummyLLM({"tool": "echo", "parameters": {"text": "hello"}})
        agent = Agent(name="ResearchAgent", skills={"echo": SensitiveEchoSkill()}, llm_client=llm)

        result = agent.execute("echo hello")

        assert result.get("status") == "success"
        assert result.get("text") == "hello"
        assert "internal_policy_version" not in result
        assert "strategy_notes" not in result

    def test_email_skill_uses_private_sanitizer_but_returns_safe_contract(self):
        skill = EmailSkill()

        # Body has extra spaces; private sanitizer should trim it.
        result = skill.send_email(to="a@b.com", body="   top secret draft   ", subject="Hi")

        assert result["status"] == "success"
        assert result["to"] == "a@b.com"
        assert result["subject"] == "Hi"
        assert result["preview"] == "top secret draft"

        # Internal IP-bearing internals should not be in the public response.
        assert "routing_policy_version" not in result
        assert "_routing_policy_version" not in result
