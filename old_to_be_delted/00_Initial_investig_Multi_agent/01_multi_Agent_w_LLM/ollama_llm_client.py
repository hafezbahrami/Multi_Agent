# ollama_llm_client.py

import json
import requests
import os
from dotenv import load_dotenv

load_dotenv()
OLLAMA_URL = os.getenv("OLLAMA_URL", "http://localhost:11434")
MODEL_NAME = os.getenv("OLLAMA_MODEL", "llama3.2:3b")


class OllamaLLMClient:
    """LLM client for Ollama with tool-calling support."""

    def __init__(self, url: str = OLLAMA_URL, model: str = MODEL_NAME):
        self.url = url
        self.model = model

    def chat(self, messages, tools=None):
        """Send messages to Ollama and optionally receive tool calls."""
        prompt = self._build_prompt(messages, tools)
        try:
            resp = requests.post(
                f"{self.url}/api/generate",
                json={"model": self.model, "prompt": prompt, "stream": False, "temperature": 0.7},
                timeout=30
            )
            resp.raise_for_status()
            text = resp.json().get("response", "")
            return self._parse_response(text)
        except Exception as e:
            print(f"Ollama error: {e}")
            return self._mock_response(messages)

    def _build_prompt(self, messages, tools=None):
        parts = []
        for msg in messages:
            role = msg.get("role")
            content = msg.get("content")
            parts.append(f"{role.upper()}: {content}\n")
        if tools:
            parts.append("\nAvailable tools:\n")
            for t in tools:
                parts.append(f"- {t['name']}: {t.get('description', '')}\n")
        parts.append("\nASSISTANT: ")
        return "".join(parts)

    def _parse_response(self, text):
        """Try to parse JSON tool call from LLM."""
        try:
            start = text.find("{")
            end = text.rfind("}") + 1
            if start >= 0 and end > start:
                return json.loads(text[start:end])
        except:
            pass
        return {"response": text}

    def _mock_response(self, messages):
        user_msg = messages[-1]["content"]
        return {"response": f"LLM received: {user_msg}"}