# Changelog

All notable changes to this project will be documented in this file.
The changelog format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

For earlier versions, an original CHANGELOG is not available [at the original repo](https://github.com/grapefrukt/usfxr)

## [0.4.0] - 2026-11-13

### Fixed

- Unguarded `using UnityEditor;` in `TinySynthPlayer.cs` broke player builds. It is now inside `#if UNITY_EDITOR`.
- `Play` threw a `NullReferenceException` on the second play when no `TinySynthPlayer` was in the scene. It now logs the error and returns.
- `AudioClip.Create` clips leaked native memory when evicted or cleared from the cache. They are now destroyed.
- `OnValidate` delayed call used static `sources` and `gameObject` without null checks. It now returns when the component was destroyed, and uses its own AudioSources.
- The cache could grow without bound when the Inspector preview added clips. The least recently used clip (played or fetched with `GetClip`) is now destroyed when the cache is full.
- Static AudioSources went stale after the player was destroyed or replaced. `sourceIndex` could run past the end after polyphony shrank. Both are fixed.
- Static state is reset at `SubsystemRegistration` and when the active player is destroyed.
- `package.json` `unity` was `2019.1`, but the code needs `2022.3`.

### Added

- `TinySynthPlayer.MaxCacheSize` is public (32).
- `TinySynthPlayer.CacheCount` and `TinySynthPlayer.ClearCache()`.
- EditMode tests in `Tests/Editor/` (`SOSXR.TinySynth.Tests`).

## [0.2.1] - In Development

> ### Package Numbering Change
> #### Package will now be numbered starting with 0, to better reflect the current status in development (see the official semver information [here](https://semver.org/#spec-item-4)).
>
> If any issues arise when updating from previous (and higher numbered versions), please delete the old version before updating to this version.

## [2.0.0] - 2025-02-03

### Changed

- Changed from Apache license to MIT license

## [0.1.3] - 2025-01-31

### Changed

- Changed regular folder 'Examples' as a separate thing: download them as samples through the 'Samples' button in the
  package manager