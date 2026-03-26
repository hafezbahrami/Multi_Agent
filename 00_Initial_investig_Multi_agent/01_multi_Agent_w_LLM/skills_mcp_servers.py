# skills_mcp_servers.py

class EmailSkill:
    def __init__(self):
        self.description = "Send, read, and manage emails"

    def send_email(self, to: str, body: str, subject: str = "No Subject") -> dict:
        print(f"\nEMAIL SENT!\nTo: {to}\nSubject: {subject}\nBody: {body}")
        return {"status": "success", "to": to, "subject": subject, "body": body[:50]}


class CalendarSkill:
    def __init__(self):
        self.description = "Create and manage calendar events"

    def create_event(self, title: str, date: str, time: str = "") -> dict:
        print(f"\nCALENDAR EVENT CREATED!\nTitle: {title}\nDate: {date}\nTime: {time}")
        return {"status": "success", "title": title, "date": date, "time": time}


class TaskSkill:
    def __init__(self):
        self.description = "Create and manage tasks"

    def create_task(self, task: str, priority: str = "medium") -> dict:
        print(f"\nTASK CREATED!\nTask: {task}\nPriority: {priority}")
        return {"status": "success", "task": task, "priority": priority}


class WebSearchSkill:
    def __init__(self):
        self.description = "Search the web for information"

    def search(self, query: str) -> dict:
        print(f"\nWEB SEARCH EXECUTED!\nQuery: {query}")
        return {"status": "success", "results": [f"Result 1 for {query}", f"Result 2 for {query}"]}