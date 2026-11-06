# Releasing

The `Publish to NuGet` workflow builds and tests on pull requests, but only packs and publishes when a commit is pushed to `master`. A successful publish is followed by creation of the GitHub release and its `v<version>` tag.

## NuGet.org trusted publishing

Configure a trusted publishing policy in the NuGet.org account that owns these packages:

- **Repository owner:** `peterkovecses`
- **Repository:** `Result`
- **Workflow file:** `publish.yml` (filename only; do not enter `.github/workflows/`)
- **Environment:** leave blank; this workflow does not use a GitHub Actions environment.
- **Package scopes:** allow pushing new packages and new versions only, with exact package ID patterns `Kovecses.Result`, `Kovecses.Result.AspNetCore`, and `Kovecses.Result.FluentAssertions`. Do not grant unlisting or deletion permissions.

In the repository's GitHub Actions secrets, set `NUGET_USER` to the NuGet.org profile username (not the account email). The workflow exchanges GitHub's OIDC token for a short-lived NuGet API key immediately before pushing. No long-lived NuGet API key secret is needed. Login and push are both restricted to pushes to `master`; pull request runs do not request an OIDC token or attempt to publish.

The workflow also needs `contents: write` to create the GitHub release and `id-token: write` to request the OIDC token. Keep these permissions limited to this publishing job.

## Recovering the failed 3.0.0 release

The failed run [42300267429](https://github.com/peterkovecses/Result/actions/runs/42300267429) completed build, test, and pack, then failed at **Push to NuGet**. The version and GitHub release steps were skipped. At the time this guidance was written, the public NuGet version listings for all three packages did not include `3.0.0`, and the repository had no `v3.0.0` tag or GitHub release. Check those states again immediately before recovery because external state can change.

After setting up the policy and `NUGET_USER`, get this workflow change onto `master` to run the updated workflow. Do not rerun the old failed run: a rerun uses that run's original workflow revision and would still use the old publishing setup. A push to `master` starts a publishing run automatically.

All three project files currently declare version `3.0.0`; the GitHub release tag is derived from the version in `src/Kovecses.Result/Kovecses.Result.csproj`. The existing `--skip-duplicate` option makes a retry safe if NuGet accepted one or more packages before a prior push failed: already-existing package versions are skipped and the other packages can still be pushed. It does not replace or compare the contents of an existing package version. If a `3.0.0` package is present but is incorrect, do not try to republish that version; correct the package sources and release a new version by updating the package versions before publishing. The GitHub release step only runs after the push step succeeds.
