import subprocess
import requests
import time

OLLAMA_URL = "http://localhost:11434"
MODEL = "llama3.2:3b"


def is_ollama_running():
    """Check if Ollama server is already running."""
    try:
        r = requests.get(f"{OLLAMA_URL}/api/tags", timeout=2)
        return r.status_code == 200
    except requests.exceptions.RequestException:
        return False


def start_ollama():
    """Start Ollama server."""
    print("Starting Ollama server...")
    subprocess.Popen(
        ["ollama", "serve"],
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL
    )

    # Wait for server to become ready
    for _ in range(10):
        if is_ollama_running():
            print("Ollama server started.")
            return
        time.sleep(1)

    raise RuntimeError("Ollama server failed to start")


def generate(prompt):
    """Generate text from Ollama."""
    response = requests.post(
        f"{OLLAMA_URL}/api/generate",
        json={
            "model": MODEL,
            "prompt": prompt,
            "stream": False
        }
    )

    response.raise_for_status()
    return response.json()["response"]


def main():
    if not is_ollama_running():
        start_ollama()
    else:
        print("Ollama server already running.")

    prompt = "Explain reinforcement learning in 2 sentences."

    result = generate(prompt)

    print("\nModel Output:\n")
    print(result)


if __name__ == "__main__":
    main()