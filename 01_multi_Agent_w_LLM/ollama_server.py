import subprocess
import requests
import time

from dotenv import load_dotenv
import os
load_dotenv()
OLLAMA_URL = os.getenv("OLLAMA_URL", "http://localhost:11434")
MODEL_NAME = os.getenv("OLLAMA_MODEL", "llama3.2:3b")


# ============================================================================
# OLLAMA SERVER MANAGER
# ============================================================================

class OllamaManager:
    """Manages Ollama server lifecycle and health checks"""
    
    def __init__(self, url: str = OLLAMA_URL, model: str = MODEL_NAME):
        self.url = url
        self.model = model
    
    def is_running(self) -> bool:
        """Check if Ollama server is already running."""
        try:
            r = requests.get(f"{self.url}/api/tags", timeout=2)
            return r.status_code == 200
        except requests.exceptions.RequestException:
            return False
    
    def start(self):
        """Start Ollama server."""
        print(" Starting Ollama server...")
        subprocess.Popen(
            ["ollama", "serve"],
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL
        )
        
        # Wait for server to become ready
        for i in range(10):
            if self.is_running():
                print("Ollama server started successfully!")
                return True
            print(f" Waiting for Ollama server... ({i+1}/10)")
            time.sleep(1)
        
        raise RuntimeError(" Ollama server failed to start")
    
    def ensure_running(self):
        """Ensure Ollama server is running, start if needed."""
        if not self.is_running():
            self.start()
        else:
            print(" Ollama server already running.")
    
    def test_model(self):
        """Test if the model is available and working."""
        try:
            response = requests.post(
                f"{self.url}/api/generate",
                json={
                    "model": self.model,
                    "prompt": "Say 'OK'",
                    "stream": False
                },
                timeout=10
            )
            response.raise_for_status()
            print(f" Model '{self.model}' is ready!")
            return True
        except Exception as e:
            print(f" Warning: Model test failed - {e}")
            return False

    def generate(self, prompt):
        """Generate text from Ollama."""
        response = requests.post(
            f"{self.url}/api/generate",
            json={
                "model": self.model,
                "prompt": prompt,
                "stream": False
            }
        )

        response.raise_for_status()
        return response.json()["response"]



def main():
    ollamaManager = OllamaManager()
    if not ollamaManager.is_running():
        ollamaManager.start()
    else:
        print("Ollama server already running.")

    prompt = "Explain reinforcement learning in 2 sentences."

    result = ollamaManager.generate(prompt)

    print("\nModel Output:\n")
    print(result)


if __name__ == "__main__":
    main()