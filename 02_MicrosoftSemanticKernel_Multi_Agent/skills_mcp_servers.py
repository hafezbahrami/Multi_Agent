# skills_mcp_servers.py

class EmailSkill:
    def __init__(self):
        self.description = "Send, read, and manage emails"
        # Internal/IP logic stays private to the agent and is never exposed as a tool.
        self._routing_policy_version = "email-router-v3"

    def _sanitize_body(self, body: str) -> str:
        return body.strip()

    def _select_mail_channel(self) -> str:
        # Internal routing policy is intentionally private company IP.
        if self._routing_policy_version == "email-router-v3":
            return "primary"
        return "fallback"

    def send_email(self, to: str, body: str, subject: str = "No Subject") -> dict:
        clean_body = self._sanitize_body(body)
        _channel = self._select_mail_channel()
        print(f"\nEMAIL SENT!\nTo: {to}\nSubject: {subject}\nBody: {clean_body}")
        # Return only a public response contract; never leak internal routing details.
        return {
            "status": "success",
            "to": to,
            "subject": subject,
            "preview": clean_body[:50],
        }


class CalendarSkill:
    def __init__(self):
        self.description = "Create and manage calendar events"
        self._scheduling_heuristics = {"focus_block_minutes": 90, "buffer_minutes": 15}

    def _normalize_time(self, time: str) -> str:
        return time.strip()

    def create_event(self, title: str, date: str, time: str = "") -> dict:
        normalized_time = self._normalize_time(time)
        print(f"\nCALENDAR EVENT CREATED!\nTitle: {title}\nDate: {date}\nTime: {normalized_time}")
        return {"status": "success", "title": title, "date": date, "time": normalized_time}


class TaskSkill:
    def __init__(self):
        self.description = "Create and manage tasks"
        self._priority_policy = {
            "high": "executive/escalation",
            "medium": "team-backlog",
            "low": "icebox",
        }

    def _normalize_priority(self, priority: str) -> str:
        p = (priority or "medium").lower().strip()
        if p not in {"high", "medium", "low"}:
            return "medium"
        return p

    def create_task(self, task: str, priority: str = "medium") -> dict:
        normalized_priority = self._normalize_priority(priority)
        print(f"\nTASK CREATED!\nTask: {task}\nPriority: {normalized_priority}")
        return {"status": "success", "task": task, "priority": normalized_priority}


class WebSearchSkill:
    def __init__(self):
        self.description = "Search the web for information"
        self._ranking_profile = "research-ranking-v2"

    def search(self, query: str) -> dict:
        print(f"\nWEB SEARCH EXECUTED!\nQuery: {query}")
        return {
            "status": "success",
            "results": [f"Result 1 for {query}", f"Result 2 for {query}"],
        }