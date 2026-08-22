"""Consolidate a raw THIRD-PARTY-NOTICES.txt (as produced by the `thirdlicense`
dotnet tool) into THIRD-PARTY-NOTICES-clean.txt: entries with byte-identical
license text are grouped under one block listing every package (name + version)
that uses it, instead of repeating the same license text once per package.

Usage:
    python build_clean.py <path-to-raw-notices.txt> <path-to-output-clean.txt>
"""
import re
import sys
import io

src_path = sys.argv[1]
out_path = sys.argv[2]

with io.open(src_path, "r", encoding="utf-8-sig") as f:
    text = f.read()

# Blocks look like:
#   License notice for <name> (v<version>)
#   ------------------------------------
#   <body>
#
#   License notice for <next name> (v<next version>)
#   ...
pattern = re.compile(
    r"^License notice for (.+?) \(v(.+?)\)\r?\n------------------------------------\r?\n"
    r"(.*?)(?=\r?\n\r?\nLicense notice for |\Z)",
    re.DOTALL | re.MULTILINE,
)

entries = [(m.group(1), m.group(2), m.group(3).strip("\r\n")) for m in pattern.finditer(text)]
print(f"Parsed {len(entries)} entries", file=sys.stderr)

groups: dict[str, list[str]] = {}
order: list[str] = []
for name, version, body in entries:
    if body not in groups:
        groups[body] = []
        order.append(body)
    groups[body].append(f"{name} (v{version})")

print(f"Consolidated into {len(order)} unique license texts", file=sys.stderr)

out_lines = [
    "THIRD-PARTY-NOTICES (consolidated)",
    "=" * 60,
    "",
    "This file lists third-party NuGet packages used by gView GIS,",
    "grouped by identical license text to avoid repetition.",
    "",
    "",
]

for body in order:
    pkgs = groups[body]
    out_lines.append(f"Packages ({len(pkgs)}):")
    for p in sorted(pkgs):
        out_lines.append(f"  - {p}")
    out_lines.append("-" * 60)
    out_lines.append("")
    out_lines.append(body)
    out_lines.append("")
    out_lines.append("")

with io.open(out_path, "w", encoding="utf-8") as f:
    f.write("\n".join(out_lines))

print(f"Wrote {out_path}", file=sys.stderr)
