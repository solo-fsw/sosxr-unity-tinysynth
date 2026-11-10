# UPM release workflow

This repo uses a GitHub Action (`.github/workflows/upm-release.yml`) to publish Unity Package Manager–ready branches.

## Why

Unity hides a package's samples only when the folder is named `Samples~`. Unity's packing step does that rename automatically, but Git URL installs skip packing: UPM clones the repo as-is. Without the rename, `Samples/` is imported as regular package content and its scripts get compiled in every consuming project.

During development we want to keep `Samples/` (no tilde) so the samples stay visible and editable in the dev project. The workflow does the rename on a separate branch instead.

## What the workflow does

1. Renames `Samples/` to `Samples~/` and removes the root `Samples.meta`. The `.meta` files inside are kept, so GUIDs stay stable.
2. Rewrites every `samples[].path` (and `samples[].images`) in `package.json` from `Samples/...` to `Samples~/...`.
3. Force-pushes the result to a UPM branch.

| Trigger | Output |
| --- | --- |
| Push to `dev` | Branch `upm-dev` |
| Push tag `vX.Y.Z` | Branch `upm` + tag `upm/vX.Y.Z` |

For tags, the workflow first checks that `vX.Y.Z` matches `version` in `package.json` and fails if not.

## Installing in another project

In Package Manager, use **+ → Install package from git URL…**:

| Channel | URL |
| --- | --- |
| Dev (latest `dev` push) | `https://github.com/solo-fsw/<repo>.git#upm-dev` |
| Latest release | `https://github.com/solo-fsw/<repo>.git#upm` |
| Pinned release | `https://github.com/solo-fsw/<repo>.git#upm/vX.Y.Z` |

Samples then show up under the package's **Samples** tab and are only copied into `Assets/` when you click **Import**.

## Releasing

```bash
# 1. Bump "version" in package.json and commit to main
git commit -am "Release 1.2.3"
git push

# 2. Tag and push
git tag v1.2.3
git push origin v1.2.3
```

For dev builds, just push to `dev`.

## Conventions on `main` / `dev`

- Keep the samples folder named `Samples` (no tilde).
- Keep `package.json` paths as `"path": "Samples/<SampleName>"`.
- Don't edit the `upm` or `upm-dev` branches by hand. They are overwritten on every run.

## Notes

- The workflow file must exist on the branch being pushed. GitHub runs the version from the pushed commit.
- The repo must allow Actions to write: **Settings → Actions → General → Workflow permissions → Read and write**.
- `upm` and `upm-dev` must not be protected branches, because the workflow force-pushes to them.
- UPM locks Git packages to a commit hash in `packages-lock.json`. To get a newer build, click **Update** in Package Manager.
- The `upm` branch has no history of its own. Past releases stay reachable through the `upm/vX.Y.Z` tags.
