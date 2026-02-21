#!/usr/bin/env python3
"""build_unitypackage.py

Generates a .unitypackage file from the Editor/ directory without Unity.

The .unitypackage format is a gzipped tar archive where each asset
is stored in a directory named after its GUID, containing:
  - asset       (the file content; omitted for directories)
  - asset.meta  (the Unity .meta file)
  - pathname    (the Unity asset path as a text file)
"""

import io
import os
import re
import sys
import tarfile
from pathlib import Path
from typing import Optional


UNITY_PREFIX = "Assets/MochiFitterNotifier"


def get_guid(meta_path: Path) -> str:
    """Extract GUID from a .meta file."""
    content = meta_path.read_text(encoding="utf-8")
    m = re.search(r"^guid:\s*(\w+)", content, re.MULTILINE)
    if not m:
        raise ValueError(f"No GUID found in {meta_path}")
    return m.group(1)


def add_entry(
    tar: tarfile.TarFile,
    guid: str,
    asset_bytes: Optional[bytes],
    meta_bytes: bytes,
    pathname: str,
) -> None:
    """Add one asset entry to the tar archive."""
    prefix = guid + "/"

    # pathname
    pn = pathname.encode("utf-8")
    ti = tarfile.TarInfo(name=prefix + "pathname")
    ti.size = len(pn)
    tar.addfile(ti, io.BytesIO(pn))

    # asset.meta
    ti = tarfile.TarInfo(name=prefix + "asset.meta")
    ti.size = len(meta_bytes)
    tar.addfile(ti, io.BytesIO(meta_bytes))

    # asset (files only; directories have no asset)
    if asset_bytes is not None:
        ti = tarfile.TarInfo(name=prefix + "asset")
        ti.size = len(asset_bytes)
        tar.addfile(ti, io.BytesIO(asset_bytes))


def build(repo_root: Path, output_path: Path) -> None:
    editor_dir = repo_root / "Editor"
    if not editor_dir.is_dir():
        print(f"Error: Editor/ not found under {repo_root}", file=sys.stderr)
        sys.exit(1)

    with tarfile.open(output_path, "w:gz") as tar:
        for current_dir, dirs, files in os.walk(editor_dir):
            dirs.sort()  # deterministic order
            current_path = Path(current_dir)
            rel_dir = current_path.relative_to(repo_root)

            # Directory entry: meta file lives one level up as "<dirname>.meta"
            dir_meta = current_path.parent / (current_path.name + ".meta")
            if dir_meta.exists():
                guid = get_guid(dir_meta)
                unity_path = UNITY_PREFIX + "/" + rel_dir.as_posix()
                add_entry(tar, guid, None, dir_meta.read_bytes(), unity_path)

            # File entries
            for filename in sorted(files):
                if filename.endswith(".meta"):
                    continue  # handled together with their asset

                file_path = current_path / filename
                meta_path = current_path / (filename + ".meta")

                if not meta_path.exists():
                    print(
                        f"Warning: no .meta for {file_path}, skipping",
                        file=sys.stderr,
                    )
                    continue

                guid = get_guid(meta_path)
                rel_file = file_path.relative_to(repo_root)
                unity_path = UNITY_PREFIX + "/" + rel_file.as_posix()
                add_entry(
                    tar,
                    guid,
                    file_path.read_bytes(),
                    meta_path.read_bytes(),
                    unity_path,
                )

    print(f"Created: {output_path}")


def main() -> None:
    if len(sys.argv) != 2:
        print(f"Usage: {sys.argv[0]} <output.unitypackage>", file=sys.stderr)
        sys.exit(1)

    output_path = Path(sys.argv[1])
    repo_root = Path(__file__).resolve().parent.parent.parent  # .github/scripts/ → .github/ → repo root
    build(repo_root, output_path)


if __name__ == "__main__":
    main()
