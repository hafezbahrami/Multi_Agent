"""Small Linux/WSL process-inspection helpers.

These utilities read from `/proc` to identify a process's PPID and to walk the
process tree under a given root PID. They are best-effort: processes can exit
between reads, and `/proc` may be unavailable on non-Linux platforms.
"""

from pathlib import Path
from typing import Dict, List, Optional, Set, Tuple


def _read_proc_stat(pid: int) -> Optional[str]:
    """Read `/proc/<pid>/stat` as text; return None if the process is gone."""
    try:
        return Path(f"/proc/{pid}/stat").read_text(encoding="utf-8", errors="replace")
    except OSError:
        return None


def pid_ppid(pid: int) -> Optional[Tuple[int, int]]:
    """Return (pid, ppid) for a running process, or None if unavailable."""

    stat = _read_proc_stat(pid)
    if not stat:
        return None
    parts = stat.split()
    if len(parts) < 5:
        return None
    return int(parts[0]), int(parts[3])


def _read_cmdline(pid: int) -> str:
    """Read `/proc/<pid>/cmdline` and return a shell-like command string."""
    try:
        raw = Path(f"/proc/{pid}/cmdline").read_bytes()
        if not raw:
            return ""
        return " ".join([p.decode("utf-8", errors="replace") for p in raw.split(b"\x00") if p])
    except OSError:
        return ""


def _read_comm(pid: int) -> str:
    """Extract the short process name (comm) from `/proc/<pid>/stat`."""
    stat = _read_proc_stat(pid)
    if not stat:
        return ""
    l = stat.find("(")
    r = stat.rfind(")")
    if l == -1 or r == -1 or r <= l:
        return ""
    return stat[l + 1 : r]


def list_descendants(root_pid: int, max_depth: int = 3) -> List[Tuple[int, int, int, str]]:
    """Return [(pid, ppid, depth, cmd), ...] for root_pid's descendants (Linux /proc best-effort)."""

    children: Dict[int, List[int]] = {}
    for p in Path("/proc").iterdir():
        if not p.name.isdigit():
            continue
        pid = int(p.name)
        info = pid_ppid(pid)
        if not info:
            continue
        _, ppid = info
        children.setdefault(ppid, []).append(pid)

    results: List[Tuple[int, int, int, str]] = []
    queue: List[Tuple[int, int]] = [(root_pid, 0)]
    visited: Set[int] = {root_pid}
    while queue:
        current_pid, depth = queue.pop(0)
        if depth >= max_depth:
            continue
        for child_pid in sorted(children.get(current_pid, [])):
            if child_pid in visited:
                continue
            visited.add(child_pid)
            child_ppid_info = pid_ppid(child_pid)
            child_ppid = child_ppid_info[1] if child_ppid_info else current_pid
            cmd = _read_cmdline(child_pid) or _read_comm(child_pid) or "<exited>"
            results.append((child_pid, child_ppid, depth + 1, cmd))
            queue.append((child_pid, depth + 1))
    return results
