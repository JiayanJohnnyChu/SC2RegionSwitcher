# GitHub candidate and draft release workflow

The repository is private: [JiayanJohnnyChu/SC2RegionSwitcher](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher). The existing repository-local author and credential manager are used. Credentials and local user settings are never stored in source control.

## Build a candidate

1. Complete repository, workflow, build, regression and package checks.
2. Commit and push to main. CI runs on Windows using the pinned SDK, tests the source and packages, and uploads `SC2RegionSwitcher-win-x64-preview`.
3. Record the successful run ID and full source commit. Download that exact artifact before installation or game testing.
4. Verify `SHA256SUMS.txt` and `release-manifest.json`. Schema 2 records Version, SourceCommit, WorkingTreeDirty, CiRunId, CiRunAttempt and both package hashes. A release candidate must come from a clean main-branch CI run.
5. Perform the required desktop, upgrade and game checks against those downloaded files. Keep raw evidence under ignored artifacts directories. Record any blocked checks explicitly.

The candidate contains exactly four release files: MSI, portable ZIP, release-manifest.json and SHA256SUMS.txt. The ZIP contains README.md and four runtime files.

## Promote original files

For the 3.4.0 candidate, create `v3.4.0-preview.1` at the tested source commit and push the tag. A tag alone no longer creates a release.

Run the **Promote tested candidate to draft prerelease** workflow with:

- `version_tag`: the existing tag, for example `v3.4.0-preview.1`.
- `candidate_run_id`: the successful main CI run that generated the files actually tested.

The workflow checks out the tag, verifies the candidate's repository, workflow, event, branch, commit and run attempt, and downloads the existing artifact. It does not build or repackage. It refuses another release or draft with the same numerical version, creates Draft + Pre-release, then downloads all four attachments and checks their hashes against the original candidate.

The source's version-specific release notes describe changes and limits. Append actual test results, the source commit, candidate run and package hashes to the draft after verification. If a test is blocked, retain the draft and name the missing test rather than reporting it as passed.

## Version policy

Windows Installer compares three numerical fields. Every later distributed preview must increase that number; changing only preview.1 to preview.2 is insufficient. Package refuses a numerical version that already has a local version tag, and CI fetches tags before packaging. Promotion independently refuses an existing draft/release for the version.

The original 3.3.0 MSI has no downgrade protection. The new upgrade implementation removes it when upgrading, but cannot prevent someone from deliberately running that legacy package later.

## Boundaries

The repository remains private and no open-source license has been selected. Packages are unsigned and depend on .NET 10 Desktop Runtime x64. Publishing the draft or changing repository visibility is a separate decision. Installer, desktop and online checks are distinct from CI build success. A new source commit or rebuilt MSI requires a new assessment of the affected tests.