[简体中文](GITHUB-RELEASE.zh-CN.md)

# Candidate and draft release workflow

The [repository](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher) is private. The active version is 3.4.1; the original 3.4.0 candidate is retained as an unpublished draft. See [current status](HANDOFF.md) for outstanding acceptance work.

## Build and identify a candidate

1. Complete repository, workflow, build, regression and package checks.
2. Commit and push to `main`. Windows CI uses the pinned SDK and uploads `SC2RegionSwitcher-win-x64-preview` after its checks pass.
3. Record the successful run ID and full source commit. Download that run's original artifact for installation and game tests.
4. Verify `SHA256SUMS.txt` and `release-manifest.json`. Schema 2 records the version, source commit, working-tree state, CI run ID, run attempt and package hashes. An accepted candidate comes from a clean `main` CI run.
5. Test the machine installation lifecycle against those files, including upgrade recovery and old-preview detection. Perform basic launch and bilingual UI checks. Retain the completed 3.4.0 full UI/online round as historical coverage; repeat wider application tests only when a changed behavior requires them. Record completed and blocked checks separately under ignored `artifacts/` directories.

The release payload has exactly four files: MSI, portable ZIP, `release-manifest.json` and `SHA256SUMS.txt`. The current ZIP contains four runtime files and the English and Chinese README files. Earlier candidates retain their original contents.

## Promote the original artifact

After acceptance, create a matching tag at the tested source commit, such as `v3.4.1-preview.1`. A tag alone does not create a release. Run **Promote tested candidate to draft prerelease** with:

- `version_tag`: the existing tag at the tested commit.
- `candidate_run_id`: the successful `main` CI run that generated the tested files.

The workflow verifies the source repository, workflow, event, branch, commit and run attempt, then downloads the original artifact. It neither builds nor repackages. It rejects another release or draft with the same numerical version, creates a draft prerelease, and downloads the four attachments to compare their hashes with the candidate.

The version-specific release notes describe changes and limits. Add the actual validation results, source commit, CI run and package hashes to the draft. Keep the draft on hold while acceptance checks remain incomplete.

## Version identity

Windows Installer compares three numerical fields. Each distributed preview therefore increments that version; changing only `preview.1` to `preview.2` is insufficient. Each numerical version has a new ProductCode; the product-family UpgradeCode remains stable.

Package creation rejects a numerical version that already has a local version tag; CI fetches tags before packaging. Promotion also rejects an existing draft or release for that version. The distributed 3.4.0 candidate must not be rebuilt, retagged or replaced.

Version 3.4.1 is the first per-machine package. Earlier current-user previews require a one-time uninstall/reinstall; the retained UpgradeCode does not enable upgrades across contexts. See [Installer](../tools/Installer/README.md).

The original 3.3.0 MSI lacks downgrade protection. Later packages cannot add protection to that already distributed file.

## Release scope

Packages are unsigned and depend on .NET 10 Desktop Runtime x64. No application license has been selected. Creating a draft does not publish it or change repository visibility. Build success alone does not establish installation, recovery or online operation. A new source revision or rebuilt MSI requires reassessment of affected checks.
