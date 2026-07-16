#!/usr/bin/env python3
"""Prepare, validate, and export Isekai Hero release metadata."""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
import uuid
import zipfile
from datetime import date
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "IsekaiHero.json"
README = ROOT / "README.md"
CHANGELOG = ROOT / "CHANGELOG.md"
NEXUS_DESCRIPTION = ROOT / "docs" / "nexus-mods-description.txt"

VERSION_PATTERN = (
    r"v(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)"
    r"(?:-[0-9A-Za-z](?:[0-9A-Za-z.-]*[0-9A-Za-z])?)?"
    r"(?:\+[0-9A-Za-z](?:[0-9A-Za-z.-]*[0-9A-Za-z])?)?"
)
VERSION_RE = re.compile(rf"^{VERSION_PATTERN}$")
README_VERSION_RE = re.compile(
    rf"^\*\*Current release:\*\* `(?P<version>{VERSION_PATTERN})`$", re.MULTILINE
)
NEXUS_VERSION_RE = re.compile(
    rf"^\[b\]Current version:\[/b\]\s+(?P<version>{VERSION_PATTERN})$",
    re.MULTILINE,
)


class ReleaseError(RuntimeError):
    pass


def read_text(path: Path) -> str:
    try:
        return path.read_text(encoding="utf-8-sig")
    except FileNotFoundError as exc:
        raise ReleaseError(f"Required file is missing: {path.relative_to(ROOT)}") from exc


def write_text(path: Path, text: str) -> None:
    path.write_text(text, encoding="utf-8", newline="\n")


def validate_version(version: str) -> None:
    if not VERSION_RE.fullmatch(version):
        raise ReleaseError(
            f"Invalid version {version!r}; expected a v-prefixed semantic version "
            "such as v0.6.0-alpha or v1.0.0."
        )


def manifest_version() -> str:
    try:
        manifest = json.loads(read_text(MANIFEST))
    except json.JSONDecodeError as exc:
        raise ReleaseError(f"IsekaiHero.json is not valid JSON: {exc}") from exc

    version = manifest.get("version")
    if not isinstance(version, str):
        raise ReleaseError("IsekaiHero.json must contain a string 'version'.")
    validate_version(version)
    return version


def release_section(changelog: str, version: str) -> tuple[str, str]:
    heading_re = re.compile(
        rf"^## \[{re.escape(version)}\] - (?P<date>\d{{4}}-\d{{2}}-\d{{2}})\s*$",
        re.MULTILINE,
    )
    match = heading_re.search(changelog)
    if not match:
        raise ReleaseError(
            f"CHANGELOG.md needs a '## [{version}] - YYYY-MM-DD' section. "
            f"Run: python scripts/release.py prepare {version}"
        )

    start = match.end()
    next_heading = re.search(r"^## \[", changelog[start:], re.MULTILINE)
    end = start + next_heading.start() if next_heading else len(changelog)
    return match.group("date"), changelog[start:end].strip()


def replace_single_marker(
    text: str, pattern: re.Pattern[str], replacement: str, source_name: str
) -> str:
    updated, count = pattern.subn(replacement, text)
    if count != 1:
        raise ReleaseError(
            f"Expected exactly one release-version marker in {source_name}; found {count}."
        )
    return updated


def prepare(version: str) -> None:
    validate_version(version)

    manifest_text = read_text(MANIFEST)
    manifest_text, count = re.subn(
        r'("version"\s*:\s*")[^"]+("\s*,?)',
        rf"\g<1>{version}\g<2>",
        manifest_text,
    )
    if count != 1:
        raise ReleaseError("Expected exactly one version field in IsekaiHero.json.")
    # Parse before writing so a malformed replacement can never corrupt the manifest.
    json.loads(manifest_text)
    write_text(MANIFEST, manifest_text)

    readme = replace_single_marker(
        read_text(README),
        README_VERSION_RE,
        f"**Current release:** `{version}`",
        "README.md",
    )
    write_text(README, readme)

    changelog = read_text(CHANGELOG)
    try:
        release_section(changelog, version)
    except ReleaseError:
        unreleased_re = re.compile(r"^## \[Unreleased\]\s*$", re.MULTILINE)
        if not unreleased_re.search(changelog):
            raise ReleaseError("CHANGELOG.md needs a '## [Unreleased]' section.")
        skeleton = (
            f"## [{version}] - {date.today().isoformat()}\n\n"
            "### Changed\n\n"
            "- TODO: replace this line with player-facing release notes."
        )
        changelog = unreleased_re.sub(
            lambda match: f"{match.group(0)}\n\n{skeleton}", changelog, count=1
        )
        write_text(CHANGELOG, changelog)

    print(f"Prepared release metadata for {version}.")
    print("Next: replace the CHANGELOG TODO with player-facing release notes.")


def validate(expected_version: str | None = None) -> str:
    version = manifest_version()
    if expected_version is not None:
        validate_version(expected_version)
        if version != expected_version:
            raise ReleaseError(
                f"Workflow requested {expected_version}, but IsekaiHero.json contains {version}."
            )

    readme_versions = README_VERSION_RE.findall(read_text(README))
    if readme_versions != [version]:
        raise ReleaseError(
            f"README.md current release must be {version}; found {readme_versions or 'no marker'}."
        )

    nexus_versions = NEXUS_VERSION_RE.findall(read_text(NEXUS_DESCRIPTION))
    if nexus_versions != [version]:
        raise ReleaseError(
            "docs/nexus-mods-description.txt current version must be "
            f"{version}; found {nexus_versions or 'no marker'}."
        )

    changelog = read_text(CHANGELOG)
    if not re.search(r"^## \[Unreleased\]\s*$", changelog, re.MULTILINE):
        raise ReleaseError("CHANGELOG.md needs a '## [Unreleased]' section.")

    release_date, notes = release_section(changelog, version)
    try:
        date.fromisoformat(release_date)
    except ValueError as exc:
        raise ReleaseError(f"Invalid changelog date for {version}: {release_date}") from exc

    if re.search(r"\b(?:TODO|TBD|CHANGEME)\b", notes, re.IGNORECASE):
        raise ReleaseError(f"The {version} changelog still contains a placeholder.")
    if not re.search(r"^[-*] \S.+$", notes, re.MULTILINE):
        raise ReleaseError(f"The {version} changelog needs at least one player-facing bullet.")

    print(f"Release metadata is valid for {version}.")
    return version


def write_github_output(path: Path, values: dict[str, str]) -> None:
    with path.open("a", encoding="utf-8", newline="\n") as output:
        for key, value in values.items():
            if "\n" not in value:
                output.write(f"{key}={value}\n")
                continue
            delimiter = f"isekai_{uuid.uuid4().hex}"
            output.write(f"{key}<<{delimiter}\n{value}\n{delimiter}\n")


def export(output: Path, github_output: Path | None) -> None:
    version = validate()
    _, notes = release_section(read_text(CHANGELOG), version)
    output.parent.mkdir(parents=True, exist_ok=True)
    write_text(output, f"# Isekai Hero {version}\n\n{notes}\n")

    nexus_notes = re.sub(r"^###\s+", "", notes, flags=re.MULTILINE)
    values = {
        "version": version,
        "nexus_version": version.removeprefix("v"),
        "display_name": f"Isekai Hero {version}",
        "archive": f"dist/IsekaiHero-{version}.zip",
        "release_notes": output.as_posix(),
        "prerelease": str("-" in version.split("+", 1)[0]).lower(),
        "nexus_changelog": nexus_notes,
    }

    if github_output is None and os.environ.get("GITHUB_OUTPUT"):
        github_output = Path(os.environ["GITHUB_OUTPUT"])

    if github_output:
        write_github_output(github_output, values)
    else:
        print(json.dumps(values, indent=2))


def check_environment() -> None:
    if os.environ.get("PUBLISH_TO_NEXUS", "").lower() == "true":
        if not os.environ.get("NEXUSMODS_API_KEY", "").strip():
            raise ReleaseError("Set the NEXUSMODS_API_KEY repository secret.")
        if not os.environ.get("NEXUSMODS_FILE_ID", "").strip():
            raise ReleaseError("Set the NEXUSMODS_FILE_ID repository variable.")

    print("Release runner configuration is present.")


def verify_tag(version: str, commit: str) -> None:
    validate_version(version)
    result = subprocess.run(
        ["git", "rev-list", "-n", "1", version],
        cwd=ROOT,
        check=False,
        capture_output=True,
        text=True,
    )
    tag_commit = result.stdout.strip()
    if result.returncode == 0 and tag_commit and tag_commit != commit:
        raise ReleaseError(
            f"Tag {version} already points to {tag_commit}, not this release commit."
        )
    if result.returncode not in (0, 128):
        raise ReleaseError(f"Could not inspect tag {version}: {result.stderr.strip()}")
    print(f"Tag {version} is available for commit {commit}.")


def build_project(configuration: str) -> None:
    command = ["dotnet", "build", "IsekaiHero.sln", "-c", configuration]

    try:
        result = subprocess.run(command, cwd=ROOT, check=False)
    except FileNotFoundError as exc:
        raise ReleaseError("dotnet is not installed or is not on PATH.") from exc
    if result.returncode != 0:
        raise ReleaseError("dotnet build failed.")


def package_build(configuration: str, output: Path | None = None) -> Path:
    version = manifest_version()
    build_directory = ROOT / ".godot" / "mono" / "temp" / "bin" / configuration
    sources = {
        "IsekaiHero/IsekaiHero.dll": build_directory / "IsekaiHero.dll",
        "IsekaiHero/IsekaiHero.pck": build_directory / "IsekaiHero.pck",
        "IsekaiHero/IsekaiHero.json": MANIFEST,
    }
    missing = [str(path.relative_to(ROOT)) for path in sources.values() if not path.is_file()]
    if missing:
        raise ReleaseError(
            "Release build outputs are missing: "
            f"{', '.join(missing)}. Run the Release configuration build first."
        )

    archive = output or ROOT / "dist" / f"IsekaiHero-{version}.zip"
    if not archive.is_absolute():
        archive = ROOT / archive
    archive.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(
        archive, "w", compression=zipfile.ZIP_DEFLATED, compresslevel=9
    ) as package:
        for archive_name, source in sources.items():
            package.write(source, archive_name)

    verify_package(archive, version)
    return archive


def verify_package(archive: Path, expected_version: str | None = None) -> None:
    version = expected_version or manifest_version()
    validate_version(version)
    if not archive.is_file():
        raise ReleaseError(f"Release archive does not exist: {archive}")

    expected_files = {
        "IsekaiHero/IsekaiHero.dll",
        "IsekaiHero/IsekaiHero.pck",
        "IsekaiHero/IsekaiHero.json",
    }
    with zipfile.ZipFile(archive) as package:
        package_files = {
            info.filename.replace("\\", "/")
            for info in package.infolist()
            if not info.is_dir()
        }
        if package_files != expected_files:
            missing = sorted(expected_files - package_files)
            extra = sorted(package_files - expected_files)
            raise ReleaseError(
                f"Unexpected package layout. Missing: {missing or 'none'}; "
                f"extra: {extra or 'none'}."
            )

        for required in expected_files:
            if package.getinfo(required).file_size == 0:
                raise ReleaseError(f"Packaged file is empty: {required}")

        packaged_manifest = json.loads(
            package.read("IsekaiHero/IsekaiHero.json").decode("utf-8-sig")
        )
        if packaged_manifest.get("version") != version:
            raise ReleaseError(
                "Packaged manifest version is "
                f"{packaged_manifest.get('version')!r}, expected {version!r}."
            )

    print(f"Verified {archive}: version {version}, layout and contents are valid.")


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    subparsers = parser.add_subparsers(dest="command", required=True)

    prepare_parser = subparsers.add_parser("prepare", help="Bump and synchronize release metadata")
    prepare_parser.add_argument("version")

    validate_parser = subparsers.add_parser("validate", help="Validate release metadata")
    validate_parser.add_argument("--expected-version")

    export_parser = subparsers.add_parser("export", help="Export release notes and workflow outputs")
    export_parser.add_argument("--output", type=Path, required=True)
    export_parser.add_argument("--github-output", type=Path)

    subparsers.add_parser("check-environment", help="Check release runner configuration")

    tag_parser = subparsers.add_parser("verify-tag", help="Prevent a release tag from moving")
    tag_parser.add_argument("version")
    tag_parser.add_argument("commit")

    build_parser = subparsers.add_parser("build", help="Build the mod with dotnet")
    build_parser.add_argument("--configuration", default="Release")

    package_parser = subparsers.add_parser("package", help="Create the installable release zip")
    package_parser.add_argument("--configuration", default="Release")
    package_parser.add_argument("--output", type=Path)

    verify_parser = subparsers.add_parser("verify-package", help="Verify the installable zip")
    verify_parser.add_argument("archive", type=Path)
    verify_parser.add_argument("--expected-version")
    return parser


def main() -> int:
    args = build_parser().parse_args()
    try:
        if args.command == "prepare":
            prepare(args.version)
        elif args.command == "validate":
            validate(args.expected_version)
        elif args.command == "export":
            export(args.output, args.github_output)
        elif args.command == "check-environment":
            check_environment()
        elif args.command == "verify-tag":
            verify_tag(args.version, args.commit)
        elif args.command == "build":
            build_project(args.configuration)
        elif args.command == "package":
            package_build(args.configuration, args.output)
        elif args.command == "verify-package":
            verify_package(args.archive, args.expected_version)
        else:
            raise AssertionError(f"Unhandled command: {args.command}")
    except (ReleaseError, json.JSONDecodeError, zipfile.BadZipFile) as exc:
        print(f"release error: {exc}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
