# multi_agent_planner.py

import inspect


SENSITIVE_FIELD_HINTS = {"secret", "internal", "policy", "heuristic", "strategy", "ip"}


# -------------------------------------------
# Parameter normalization
# -------------------------------------------

# common LLM naming mistakes. This maps the "wrong" parameter names to the expected ones for each tool.
PARAMETER_MAP = {
    "create_event": {"name": "title"},
    "create_task": {"name": "task", "description": "task"},
    "send_email": {"content": "body", "message": "body"},
}

def normalize_parameters(llm_gen_tool_name, llm_gen_tool_params, method):
    """
    Normalize parameters returned by LLM so they match
    the function signature of the skill method.
    """

    # Map common LLM names → expected parameter names
    if llm_gen_tool_name in PARAMETER_MAP:
        mapping = PARAMETER_MAP[llm_gen_tool_name]

        for wrong_param, correct_param in mapping.items():
            if wrong_param in llm_gen_tool_params and correct_param not in llm_gen_tool_params:
                llm_gen_tool_params[correct_param] = llm_gen_tool_params.pop(wrong_param)

    # Remove parameters not supported by method
    sig = inspect.signature(method)
    allowed = set(sig.parameters.keys())

    cleaned_params = {k: v for k, v in llm_gen_tool_params.items() if k in allowed}

    return cleaned_params


# -------------------------------------------
# Agent
# -------------------------------------------

class Agent:
    """
    An Agent owns a set of skills and uses the LLM
    to decide which skill to call.
    """

    def __init__(self, name, skills, llm_client):
        self.name = name
        self.skills = skills
        self.llm = llm_client
        self.api_contract = self._build_api_contract()

    def _build_api_contract(self):
        """
        Build an explicit IP-protected allowlist of tools exposed to the LLM.

        `skills` is expected to map: {"tool_name": skill_instance}
        Only `tool_name` is exposed, and only if the skill implements it.
        """
        contract = {}

        for tool_name, skill_obj in self.skills.items():
            if not hasattr(skill_obj, tool_name): # This checks if the skill actually implements the tool.
                continue

            tool_method = getattr(skill_obj, tool_name)
            if not callable(tool_method):
                continue

            sig = inspect.signature(tool_method)
            params = list(sig.parameters.keys())
            contract[tool_name] = {
                "method": tool_method,
                "description": getattr(skill_obj, "description", "No description"),
                "parameters": params,
            }

        return contract

    def _sanitize_output(self, payload):
        """
        ven if internal data accidentally leaks into the response, your code removes it.
        Remove fields that look like internal/IP-bearing data before returning
        results to the planner/user.
        """
        if not isinstance(payload, dict):
            return payload

        safe = {}
        for key, value in payload.items():
            lowered = key.lower()
            if any(hint in lowered for hint in SENSITIVE_FIELD_HINTS):
                continue
            safe[key] = value
        return safe

    def execute(self, user_query: str):

        tools_description = []

        for tool_name, tool_meta in self.api_contract.items():
            tools_description.append(f"{tool_name} → {tool_meta['description']}")

        tools_json_schema = {
            tool_name: tool_meta["parameters"] for tool_name, tool_meta in self.api_contract.items()
        }

        system_prompt = f"""
                    You are {self.name}.

                    You can use ONLY the following tool API surface:

                    {tools_description}

                    Allowed JSON parameter keys per tool:

                    {tools_json_schema}

                    Choose the BEST tool for the user request.

                    Return ONLY JSON like this:

                        {{
                                "tool": "tool_name",
                                "parameters": {{
                                                }}
                        }}

                    Examples:

                    User: Send an email
                    Output:
                    {{
                            "tool": "send_email",
                            "parameters": {{
                                            "to": "...",
                                            "body": "..."
                                            }}
                    }}

                    User: Schedule a meeting
                    Output:
                    {{
                            "tool": "create_event",
                            "parameters": {{
                                            "title": "...",
                                            "date": "...",
                                            "time": "...+
                                            "
                                            }}
                    }}
        """

        messages = [
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": user_query},
        ]

        response = self.llm.chat(messages)

        # -----------------------------------
        # Parse LLM response
        # -----------------------------------

        if not isinstance(response, dict):
            return response

        llm_gen_tool_name = response.get("tool")
        llm_gen_tool_params = response.get("parameters", {})

        if not llm_gen_tool_name:
            return response.get("response", "LLM did not return a tool")

        if llm_gen_tool_name not in self.api_contract:
            return {
                "error": (
                    f"Tool '{llm_gen_tool_name}' is not exposed by {self.name}. "
                    "Only explicitly allowlisted APIs are callable."
                )
            }

        # -----------------------------------
        # Execute skill
        # -----------------------------------

        tool_method = self.api_contract[llm_gen_tool_name]["method"]

        params = normalize_parameters(llm_gen_tool_name, llm_gen_tool_params, tool_method)

        # Fallback extraction if LLM forgot task/title
        if llm_gen_tool_name == "create_task" and "task" not in params:
            params["task"] = user_query

        if llm_gen_tool_name == "create_event" and "title" not in params:
            params["title"] = user_query

        try:
            raw_result = tool_method(**params)
            return self._sanitize_output(raw_result)
        except Exception as e:
            return {"error": str(e)}


# -------------------------------------------
# Multi-Agent Planner
# -------------------------------------------

class MultiAgentPlanner:
    """
    Routes user queries to the appropriate agent.
    """

    def __init__(self, agents, llm_client):
        self.agents = agents
        self.llm = llm_client

    def route_query(self, query: str):

        agent_names = [agent.name for agent in self.agents]

        router_prompt = f"""
                        You are a router in a multi-agent system.

                        Available agents:

                        {agent_names}

                        Rules:

                        WorkflowAgent → emails, calendar events, tasks
                        ResearchAgent → web search, research, information gathering

                        Return ONLY the agent name.

                        Example:
                        WorkflowAgent
                        """

        router_messages = [
            {"role": "system", "content": router_prompt},
            {"role": "user", "content": query},
        ]

        router_response = self.llm.chat(router_messages)

        pred_agent = ""

        if isinstance(router_response, dict):
            pred_agent = router_response.get("response", "")
        else:
            pred_agent = str(router_response)

        pred_agent = pred_agent.strip()

        # -----------------------------------
        # Try LLM routing
        # -----------------------------------

        for agent in self.agents:
            if agent.name.lower() in pred_agent.lower():
                return agent.execute(query)

        # -----------------------------------
        # Fallback routing (safe guard)
        # -----------------------------------

        q = query.lower()

        if any(k in q for k in ["email", "mail", "calendar", "meeting", "task"]):
            return self.agents[0].execute(query)

        if any(k in q for k in ["search", "find", "research"]):
            return self.agents[1].execute(query)

        return "No suitable agent found."