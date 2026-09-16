#!/usr/bin/env python3
"""Accumulate benchmark results per release and draw them for the README.

Two subcommands:

  ingest   read BenchmarkDotNet's JSON reports and append one entry to the history file
  render   draw the history as SVG, one file per colour scheme

The history file is committed, so a release only ever measures itself and the chart keeps
everything measured before it. Nothing here imports a third-party package: CI runs it on a stock
runner with no pip install step, and the SVG it writes is text that reviews like source.
"""

from __future__ import annotations

import argparse
import glob
import json
import os
import re
import sys
from datetime import datetime, timezone

SCHEMA_VERSION = 1

# The benchmarks drawn in the README, in the order they appear. Everything measured is stored;
# this only decides what the picture shows, so it can change without re-running anything.
HEADLINE = [
    ("ArithmeticBenchmarks.Add", "30", "Add"),
    ("ArithmeticBenchmarks.Multiply", "30", "Multiply"),
    ("ArithmeticBenchmarks.Divide", "30", "Divide"),
    ("ComparisonBenchmarks.CompareTo", "30", "CompareTo"),
    ("ConstructionBenchmarks.Sanitizing", "30", "Construct"),
    ("TextBenchmarks.Parse", "30", "Parse"),
    ("ConversionBenchmarks.ToDouble", None, "ToDouble"),
]

BASELINE_KEY = "BaselineBenchmarks.ReferenceWork"

# Validated against scripts/validate_palette.js in both modes: every check passes, worst adjacent
# CVD dE 24.7 light / 26.8 dark.
THEMES = {
    "light": {
        "surface": "#fcfcfb",
        "ink": "#0b0b0b",
        "muted": "#52514e",
        "grid": "#e4e3df",
        "alloc": "#2a78d6",
        "time": "#eb6834",
    },
    "dark": {
        "surface": "#1a1a19",
        "ink": "#ffffff",
        "muted": "#c3c2b7",
        "grid": "#333330",
        "alloc": "#3987e5",
        "time": "#d95926",
    },
}


# --------------------------------------------------------------------------- ingest


def _benchmark_key(full_name: str) -> str:
    """Reduces a fully qualified benchmark name to Class.Method."""
    bare = full_name.split("(", 1)[0]
    parts = bare.split(".")
    return ".".join(parts[-2:]) if len(parts) >= 2 else bare


def _params(report: dict) -> str:
    """The parameter values for one case, as BenchmarkDotNet writes them."""
    return (report.get("Parameters") or "").strip()


def read_reports(results_dir: str) -> dict:
    """Reads every BenchmarkDotNet JSON report below a directory."""
    measured: dict[str, dict] = {}
    host = {}
    paths = sorted(glob.glob(os.path.join(results_dir, "**", "*-report-full.json"), recursive=True))
    if not paths:
        raise SystemExit(f"No *-report-full.json under {results_dir}")

    for path in paths:
        with open(path, encoding="utf-8-sig") as handle:
            document = json.load(handle)

        environment = document.get("HostEnvironmentInfo") or {}
        if not host:
            host = {
                "cpu": (environment.get("ProcessorName") or "").strip(),
                "runtime": (environment.get("RuntimeVersion") or "").strip(),
                "dotnetSdk": (environment.get("DotNetSdkVersion") or "").strip(),
            }

        for report in document.get("Benchmarks") or []:
            statistics = report.get("Statistics") or {}
            mean = statistics.get("Mean")
            if mean is None:
                continue
            memory = report.get("Memory") or {}
            key = _benchmark_key(report.get("FullName") or report.get("MethodTitle") or "")
            parameters = _params(report)
            measured.setdefault(key, {})[parameters] = {
                "meanNs": round(float(mean), 4),
                "allocatedBytes": int(memory.get("BytesAllocatedPerOperation") or 0),
            }

    return {"host": host, "benchmarks": measured}


def ingest(args: argparse.Namespace) -> None:
    gathered = read_reports(args.results)
    measured = gathered["benchmarks"]

    baseline_ns = None
    if BASELINE_KEY in measured:
        baseline_ns = next(iter(measured[BASELINE_KEY].values()))["meanNs"]
    elif args.baseline_ns is not None:
        baseline_ns = args.baseline_ns

    if baseline_ns is None:
        print(
            f"warning: no {BASELINE_KEY} measurement and no --baseline-ns; "
            "this entry's times will not be comparable across runners",
            file=sys.stderr,
        )

    entry = {
        "version": args.version,
        "commit": args.commit,
        "date": args.date or datetime.now(timezone.utc).strftime("%Y-%m-%d"),
        "cpu": gathered["host"].get("cpu", ""),
        "runtime": gathered["host"].get("runtime", ""),
        "baselineNs": baseline_ns,
        "runId": args.run_id or "",
        "benchmarks": {
            key: values for key, values in sorted(measured.items()) if key != BASELINE_KEY
        },
    }

    history = load_history(args.history)
    # A version is measured once. Re-running a release replaces its entry rather than doubling it.
    history["entries"] = [e for e in history["entries"] if e.get("version") != entry["version"]]
    history["entries"].append(entry)
    history["entries"].sort(key=version_sort_key)

    os.makedirs(os.path.dirname(args.history) or ".", exist_ok=True)
    with open(args.history, "w", encoding="utf-8", newline="\n") as handle:
        json.dump(history, handle, indent=2, sort_keys=False)
        handle.write("\n")

    print(
        f"ingested {entry['version']}: {len(entry['benchmarks'])} benchmarks, "
        f"baseline {baseline_ns} ns, cpu {entry['cpu'] or 'unknown'}"
    )


def load_history(path: str) -> dict:
    if not os.path.exists(path):
        return {"schemaVersion": SCHEMA_VERSION, "entries": []}
    with open(path, encoding="utf-8") as handle:
        history = json.load(handle)
    history.setdefault("schemaVersion", SCHEMA_VERSION)
    history.setdefault("entries", [])
    return history


def version_sort_key(entry: dict):
    """Orders versions numerically, keeping anything unparseable at the front in name order."""
    text = str(entry.get("version", ""))
    numbers = [int(part) for part in re.findall(r"\d+", text)]
    return (1, numbers, text) if numbers else (0, [], text)


# --------------------------------------------------------------------------- render


def series_for(history: dict, key: str, parameters: str | None):
    """Pulls one benchmark's points out of every entry, in release order."""
    points = []
    for entry in history["entries"]:
        cases = (entry.get("benchmarks") or {}).get(key)
        if not cases:
            points.append(None)
            continue
        if parameters is None:
            case = next(iter(cases.values()))
        else:
            case = next(
                (value for name, value in cases.items() if parameters in name),
                None,
            )
        points.append(case)
    return points


def nice_bytes(value: float) -> str:
    if value <= 0:
        return "0 B"
    if value >= 1024:
        return f"{value / 1024:.1f} KB"
    return f"{value:.0f} B"


def ratio_label(value: float) -> str:
    """Three significant figures, so a 0.0331x and a 15.3x are both legible."""
    if value <= 0:
        return "0×"
    if value >= 100:
        return f"{value:.0f}×"
    if value >= 10:
        return f"{value:.1f}×"
    if value >= 1:
        return f"{value:.2f}×"
    if value >= 0.1:
        return f"{value:.3f}×"
    return f"{value:.4f}×"


def escape(text: str) -> str:
    return (
        str(text)
        .replace("&", "&amp;")
        .replace("<", "&lt;")
        .replace(">", "&gt;")
        .replace('"', "&quot;")
    )


def panel(x0, y0, width, height, title, labels, values, fmt, colour, theme):
    """One small multiple: a single series, so colour carries no identity of its own."""
    out = []
    plot_top = y0 + 22
    plot_bottom = y0 + height - 20
    plot_left = x0 + 6
    plot_right = x0 + width - 10

    out.append(
        f'<text x="{x0}" y="{y0 + 10}" class="panel-title">{escape(title)}</text>'
    )

    present = [(i, v) for i, v in enumerate(values) if v is not None]
    if not present:
        out.append(
            f'<text x="{x0}" y="{(plot_top + plot_bottom) / 2:.1f}" class="muted">not measured</text>'
        )
        return out

    highest = max(v for _, v in present)
    # Zero-based: these are magnitudes, and a clipped axis would exaggerate every wobble.
    top = highest * 1.25 if highest > 0 else 1.0

    def px(index):
        if len(labels) == 1:
            return (plot_left + plot_right) / 2
        return plot_left + (plot_right - plot_left) * index / (len(labels) - 1)

    def py(value):
        return plot_bottom - (plot_bottom - plot_top) * (value / top)

    out.append(
        f'<line x1="{plot_left}" y1="{plot_bottom:.1f}" x2="{plot_right}" '
        f'y2="{plot_bottom:.1f}" class="axis" />'
    )

    segments = []
    for index, value in present:
        segments.append(f"{px(index):.1f},{py(value):.1f}")
    if len(segments) > 1:
        out.append(
            f'<polyline points="{" ".join(segments)}" fill="none" '
            f'stroke="{colour}" stroke-width="2" stroke-linejoin="round" stroke-linecap="round" />'
        )

    for index, value in present:
        # A 2px surface ring keeps markers legible where the line passes behind them.
        out.append(
            f'<circle cx="{px(index):.1f}" cy="{py(value):.1f}" r="4" fill="{colour}" '
            f'stroke="{theme["surface"]}" stroke-width="2" />'
        )

    last_index, last_value = present[-1]
    anchor = "end" if last_index == len(labels) - 1 else "middle"
    out.append(
        f'<text x="{px(last_index):.1f}" y="{py(last_value) - 9:.1f}" '
        f'class="value" text-anchor="{anchor}">{escape(fmt(last_value))}</text>'
    )

    first_index, first_value = present[0]
    if first_index != last_index:
        out.append(
            f'<text x="{px(first_index):.1f}" y="{py(first_value) - 9:.1f}" '
            f'class="muted-value" text-anchor="middle">{escape(fmt(first_value))}</text>'
        )

    return out


def render_svg(history: dict, theme_name: str) -> str:
    theme = THEMES[theme_name]
    entries = history["entries"]
    labels = [e.get("version", "?") for e in entries]

    columns, cell_w, cell_h = 4, 228, 132
    left, right = 56, 24
    width = left + columns * cell_w + right
    rows = (len(HEADLINE) + columns - 1) // columns
    section_h = 34 + rows * cell_h
    height = 72 + section_h * 2 + 54

    parts = [
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" '
        f'viewBox="0 0 {width} {height}" role="img" '
        f'aria-label="PreciseNumber allocation and relative time per release">',
        "<style>",
        f"  text {{ font-family: ui-sans-serif, -apple-system, 'Segoe UI', Roboto, sans-serif; "
        f"fill: {theme['ink']}; }}",
        "  .title { font-size: 15px; font-weight: 600; }",
        f"  .section {{ font-size: 12.5px; font-weight: 600; }}",
        f"  .panel-title {{ font-size: 11px; font-weight: 600; fill: {theme['ink']}; }}",
        f"  .muted, .muted-value, .caption {{ font-size: 9.5px; fill: {theme['muted']}; }}",
        f"  .value {{ font-size: 10px; font-weight: 600; fill: {theme['ink']}; }}",
        f"  .tick {{ font-size: 9px; fill: {theme['muted']}; }}",
        f"  .axis {{ stroke: {theme['grid']}; stroke-width: 1; }}",
        "</style>",
        f'<rect width="{width}" height="{height}" fill="{theme["surface"]}" />',
        f'<text x="{left}" y="28" class="title">PreciseNumber performance by release</text>',
    ]

    latest = entries[-1] if entries else {}
    parts.append(
        f'<text x="{left}" y="45" class="caption">'
        f'{escape(len(entries))} releases · newest {escape(latest.get("version", "?"))}'
        f'{" · " + escape(latest.get("date", "")) if latest.get("date") else ""}</text>'
    )

    sections = [
        (
            "Allocated bytes per operation",
            theme["alloc"],
            lambda case: float(case["allocatedBytes"]),
            nice_bytes,
            "Deterministic: the same code allocates the same bytes on any machine.",
        ),
        (
            "Time, as a multiple of a fixed reference workload",
            theme["time"],
            None,  # filled in below; needs the entry's baseline
            ratio_label,
            "Divided by a reference loop measured in the same job, which cancels most of the "
            "difference between CI runners. Lower is faster.",
        ),
    ]

    y = 72
    for title, colour, _value_of, fmt, note in sections:
        is_time = colour == theme["time"]
        parts.append(f'<rect x="{left}" y="{y - 10}" width="9" height="9" rx="2" fill="{colour}" />')
        parts.append(f'<text x="{left + 15}" y="{y - 2}" class="section">{escape(title)}</text>')
        parts.append(f'<text x="{left + 15}" y="{y + 12}" class="caption">{escape(note)}</text>')

        grid_top = y + 26
        for position, (key, parameters, label) in enumerate(HEADLINE):
            cases = series_for(history, key, parameters)
            if is_time:
                values = [
                    (case["meanNs"] / entry["baselineNs"])
                    if case and entry.get("baselineNs")
                    else None
                    for case, entry in zip(cases, entries)
                ]
            else:
                values = [float(case["allocatedBytes"]) if case else None for case in cases]

            column, row = position % columns, position // columns
            parts.extend(
                panel(
                    left + column * cell_w,
                    grid_top + row * cell_h,
                    cell_w - 16,
                    cell_h - 12,
                    label + ("" if parameters is None else f" ({parameters.split('=')[-1]} digits)"),
                    labels,
                    values,
                    fmt,
                    colour,
                    theme,
                )
            )

        y += 34 + rows * cell_h

    # One shared x axis caption: every panel uses the same release order.
    axis_y = y - 4
    ticks = []
    for index, label in enumerate(labels):
        if len(labels) > 6 and 0 < index < len(labels) - 1 and index % 2:
            continue
        ticks.append(escape(label))
    parts.append(
        f'<text x="{left}" y="{axis_y}" class="tick">releases, oldest to newest: '
        f'{escape(" → ".join(ticks))}</text>'
    )

    cpus = sorted({e.get("cpu", "") for e in entries if e.get("cpu")})
    parts.append(
        f'<text x="{left}" y="{axis_y + 16}" class="caption">'
        f'Measured on {escape(", ".join(cpus) or "an unrecorded CPU")}. '
        f'Full tables: PreciseNumber.Benchmarks.</text>'
    )
    parts.append("</svg>")
    return "\n".join(parts) + "\n"


def render(args: argparse.Namespace) -> None:
    history = load_history(args.history)
    if not history["entries"]:
        raise SystemExit(f"{args.history} has no entries to draw")

    base, extension = os.path.splitext(args.out)
    written = []
    for theme_name in ("light", "dark"):
        path = args.out if theme_name == "light" else f"{base}-dark{extension}"
        os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
        with open(path, "w", encoding="utf-8", newline="\n") as handle:
            handle.write(render_svg(history, theme_name))
        written.append(path)
    print(f"rendered {len(history['entries'])} releases to {', '.join(written)}")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    sub = parser.add_subparsers(dest="command", required=True)

    take = sub.add_parser("ingest", help="append one release's results to the history")
    take.add_argument("--history", required=True)
    take.add_argument("--results", required=True, help="directory holding BenchmarkDotNet output")
    take.add_argument("--version", required=True)
    take.add_argument("--commit", default="")
    take.add_argument("--date", default="")
    take.add_argument("--run-id", default="")
    take.add_argument(
        "--baseline-ns",
        type=float,
        default=None,
        help="reference time to record when this version predates BaselineBenchmarks",
    )
    take.set_defaults(func=ingest)

    draw = sub.add_parser("render", help="draw the history as SVG")
    draw.add_argument("--history", required=True)
    draw.add_argument("--out", required=True, help="light-mode path; the dark file sits beside it")
    draw.set_defaults(func=render)

    args = parser.parse_args()
    args.func(args)


if __name__ == "__main__":
    main()
