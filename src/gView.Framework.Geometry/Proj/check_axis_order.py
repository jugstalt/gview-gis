"""
Checks the axis order stored in the proj.db WKT table against the official EPSG axis order

gView only uses the WKT table for the axis order (SpatialReference.FromID_: AXIS["X",..] = first axis,
AXIS["Y",..] = second axis). For every EPSG code the axis order gView derives is compared with PROJ
(pyproj). Differences are printed (and written to <output> as a list); exit code 1 if there are any.

Usage (needs: pip install pyproj):
    python check_axis_order.py <path-to-proj.db> <output.txt>
"""
import re
import sqlite3
import sys

from pyproj import CRS

DIRS = {'north': 'North', 'east': 'East', 'south': 'South', 'west': 'West'}


def gview_axes(wkt):
    """replicates SpatialReference.FromID_ (axis part)"""
    if not wkt:
        return 'East', 'North'
    x = re.search(r'AXIS\["X",(\w+)\]', wkt)
    y = re.search(r'AXIS\["Y",(\w+)\]', wkt)
    if not x and not y:
        if wkt.startswith('GEOGCS'):
            return 'North', 'East'
        return 'East', 'North'
    ax, ay = 'East', 'North'
    if x and x.group(1).capitalize() in DIRS.values():
        ax = x.group(1).capitalize()
    if y and y.group(1).capitalize() in DIRS.values():
        ay = y.group(1).capitalize()
    return ax, ay


def main(db_path, out_path):
    c = sqlite3.connect(f'file:{db_path}?mode=ro', uri=True)
    # ids in proj.db can carry whitespace: normalize before comparing/joining
    wkts = {k.strip().upper(): v for k, v in c.execute("select PROJ_ID, WKT from wkt").fetchall()}
    ids = {r[0].strip().upper() for r in c.execute("select PROJ_ID from projs where PROJ_ID like 'EPSG:%'")}
    ids = sorted(ids | {i for i in wkts if i.startswith('EPSG:')}, key=lambda s: int(s.split(':')[1]))

    entries = []
    for pid in ids:
        code = int(pid.split(':')[1])
        try:
            axes = CRS.from_epsg(code).axis_info
        except Exception:
            continue
        if len(axes) < 2 or axes[0].direction not in DIRS or axes[1].direction not in DIRS:
            continue
        ref = (DIRS[axes[0].direction], DIRS[axes[1].direction])
        if ref != gview_axes(wkts.get(pid)):
            entries.append((code, ref))

    with open(out_path, 'w', encoding='utf-8') as f:
        for code, (a, b) in entries:
            print(f'EPSG:{code} should be {a},{b}', file=f)
    print(len(entries), 'mismatches')
    sys.exit(1 if entries else 0)


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2])
