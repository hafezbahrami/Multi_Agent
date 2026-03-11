# multi_agent_planner.py

import inspect


# -------------------------------------------
# Parameter normalization
# -------------------------------------------

PARAMETER_MAP = {
    "create_event": {"name": "title"},
    "create_task": {"name": "task", "description": "task"},
    "send_email": {"content": "body", "message": "body"},
}


def normalize_parameters(tool_name, params, method):
    """
    Normalize parameters returned by LLM so they match
    the function signature of the skill method.
    """

    # Map common LLM names → expected parameter names
    if tool_name in PARAMETER_MAP:
        mapping = PARAMETER_MAP[tool_name]

        for src, dst in mapping.items():
            if src in params and dst not in params:
                params[dst] = params.pop(src)

    # Remove parameters not supported by method
    sig = inspect.signature(method)
    allowed = set(sig.parameters.keys())

    cleaned_params = {k: v for k, v in params.items() if k in allowed}

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

    def execute(self, user_query: str):

        tools_description = []

        for skill_name, skill_obj in self.skills.items():

            for attr in dir(skill_obj):         # dir() returns all attributes and methods of an object as strings, like: ['_init_', '_str_', 'search', 'summarize', 'description'].
                if attr.startswith("_"):        # _ are usually private or internal attributes. We want to ignore those.   
                    continue
                fn = getattr(skill_obj, attr)
                if callable(fn):
                    tools_description.append(
                                            f"{attr} → {skill_obj.description}"
                                        )
        system_prompt = f"""
                    You are {self.name}.

                    You can use the following tools:

                    {tools_description}

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

        tool_name = response.get("tool")
        params = response.get("parameters", {})

        if not tool_name:
            return response.get("response", "LLM did not return a tool")

        # -----------------------------------
        # Execute skill
        # -----------------------------------

        for skill in self.skills.values():

            if hasattr(skill, tool_name):

                method = getattr(skill, tool_name)

                params = normalize_parameters(tool_name, params, method)
                
                # Fallback extraction if LLM forgot task/title
                if tool_name == "create_task" and "task" not in params:
                    params["task"] = user_query

                if tool_name == "create_event" and "title" not in params:
                    params["title"] = user_query

                try:
                    return method(**params)
                except Exception as e:
                    return {"error": str(e)}

        return {"error": f"Tool '{tool_name}' not found in {self.name}"}


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