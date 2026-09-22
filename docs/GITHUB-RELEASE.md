[简体中文](GITHUB-RELEASE.zh-CN.md)

# Candidate and draft release workflow

The [repository](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher) is private. The active version is 3.4.2; original 3.4.1 preview artifacts remain immutable. Outstanding acceptance work is documented in [current status](HANDOFF.md).

## Candidate generation and identification

1. Candidate generation requires successful repository, workflow, build, regression and package checks.
2. A commit pushed to `main` triggers Windows CI, which uses the pinned SDK and uploads `SC2RegionSwitcher-win-x64-preview` after its checks pass.
3. The candidate record includes the successful run ID and full source commit. Installation and any required game tests use that run's downloaded original artifact.
4. Candidate identification includes verification of `SHA256SUMS.txt` and `release-manifest.json`. Schema 2 records the version, source commit, working-tree state, CI run ID, run attempt and package hashes. An accepted candidate comes from a clean `main` CI run.
5. Validation of the original artifact follows the affected changes. One comprehensive pass precedes a major application release; subsequent small revisions receive targeted checks. Documentation, licensing and package-content changes require relevant content/package verification and the existing build/package/static CI checks, without a repeated installation, UI or online matrix. Completed and blocked checks are recorded separately under ignored `artifacts/` directories.

The release payload has exactly four files: MSI, portable ZIP, `release-manifest.json` and `SHA256SUMS.txt`. The ZIP contains four runtime files, English and Chinese README files, and six licensing/source files identified in [third-party notices](../THIRD-PARTY-NOTICES.md). Earlier candidates retain their original contents.

## Promotion of the original artifact

Promotion requires acceptance and a matching tag at the tested source commit, such as `v3.4.2-preview.1`. A tag alone does not create a release. The **Promote tested candidate to draft prerelease** workflow takes the following inputs:

- `version_tag`: the existing tag at the tested commit.
- `candidate_run_id`: the successful `main` CI run that generated the tested files.

The workflow verifies the source repository, workflow, event, branch, commit and run attempt, then downloads the original artifact. It neither builds nor repackages. It rejects another release or draft with the same numerical version, creates a draft prerelease, and downloads the four attachments to compare their hashes with the candidate.

The version-specific release notes describe changes and limits. The draft must include the actual validation results, source commit, CI run and package hashes. It remains on hold while acceptance checks are incomplete.

## Version identity

Windows Installer compares three numerical fields. Each distributed preview therefore increments that version; changing only `preview.1` to `preview.2` is insufficient. Each numerical version has a new ProductCode; the product-family UpgradeCode remains stable.

Package creation rejects a numerical version that already has a local version tag; CI fetches tags before packaging. Promotion also rejects an existing draft or release for that version. Previously distributed candidates, including 3.4.1, must not be rebuilt, retagged or replaced.

Version 3.4.1 is the first per-machine package. Earlier current-user previews require a one-time uninstall/reinstall; the retained UpgradeCode does not enable upgrades across contexts. The migration procedure is documented in [Installer](../tools/Installer/README.md).

The original 3.3.0 MSI lacks downgrade protection. Later packages cannot add protection to that already distributed file.

## Release scope

Packages are unsigned and depend on .NET 10 Desktop Runtime x64. Original project material is licensed under [MIT](../LICENSE); [third-party notices](../THIRD-PARTY-NOTICES.md) identify retained upstream licenses. Creating a draft does not publish it or change repository visibility. Build success alone does not establish installation, recovery or online operation. A new source revision or rebuilt MSI requires reassessment of affected checks.
