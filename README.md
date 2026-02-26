---
media_link:
MOC:
related:
tags: []
date_created: 2025-01-31
date_modified: 2026-02-26
---

# TinySynth: Procedural Sound Effects for Unity

- By: grapefrukt
- Fully open source: Feel free to add to, or modify, anything you see fit.

TinySynth is a fork of [usfxr](https://github.com/grapefrukt/usfxr) (which is itself a port of the AS3 library
[sfxr](http://www.drpetter.se/project_sfxr.html)). It was renamed because spelling _usfxr_ from memory is a tax on
working memory.

The synthesiser lets you generate placeholder (or final) sound effects entirely inside the Unity
Editor — no external tools required.

## Installation

### Via Package Manager (UPM) (recommended)

1. Open the Unity project you want to install this package in.
2. Open the Package Manager window.
3. Click on the `+` button and select `Add package from git URL…`.
4. Paste the URL of this repo into the text field and press `Add`. Make sure it ends with `.git`.

### Via Unity Package

1. Download the latest release from the release page.
2. Import the `.unitypackage` into your Unity project.

### Via Git (to contribute)

1. Fork this repository, then clone it into your Unity project's `Assets` folder.
2. Make your changes and open a pull request back to the main repository.

### Dev Release

For the latest (possibly unstable) version, append `#dev` to the UPM git URL to track the `dev` branch.

## Architecture

TinySynth is built around three runtime types and one editor type:

| Type                            | Kind                           | Role                                                                                                                                                              |
| ------------------------------- | ------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **`TinySynthSound`**            | `struct` (Serializable)        | A bag of ~32 normalised synthesis parameters (envelope, frequency, filters, effects). Assign as a public field — it gets a full property drawer in the Inspector. |
| **`TinySynthPlayer`**           | `MonoBehaviour`                | Scene singleton that owns one or more `AudioSource` components (one per polyphony slot). Provides the `Play` and `PreCache` static API.                           |
| **`TinySynthRenderer`**         | Plain C# class                 | Converts a `TinySynthSound` into a Unity `AudioClip` or a raw WAV `byte[]`. All synthesis math lives here.                                                        |
| **`TinySynthPreset`**           | Static class                   | Factory methods that return ready-to-use `TinySynthSound` values for common game archetypes (laser, explosion, jump, etc.).                                       |
| **`TinySynthWaveType`**         | `enum`                         | Selects the oscillator: Square, Sawtooth, Sine, Noise, Triangle, PinkNoise, Tan, Whistle, Breaker.                                                                |
| **`TinySynthDefaultAttribute`** | `Attribute`                    | Marks each `TinySynthSound` field with its default value, powering the 'R' reset button in the Editor drawer.                                                     |
| **`TinySynthEditor`**           | `PropertyDrawer` (Editor-only) | Draws the waveform preview, preset row, per-parameter sliders, lock toggles, and WAV export for any `TinySynthSound` field in the Inspector.                      |

**Data flow:**

```
TinySynthSound (parameters)
  └─► TinySynthRenderer.GenerateClip()
        └─► AudioClip ─► TinySynthPlayer (polyphonic AudioSources) ─► AudioListener
```

Sound effects are **generated on first play and cached** by `TinySynthPlayer`. The cache holds up to 32 clips; the
oldest entry is evicted when the limit is reached.

## Quick Start

### 1 — Add TinySynthPlayer to the scene

Add `TinySynthPlayer` to any persistent GameObject (the main camera is a common choice). It requires an
`AudioSource`; additional sources are added/removed automatically when you change the **Polyphony** setting.

### 2 — Declare a sound effect

In any `MonoBehaviour`, add a public `TinySynthSound` field:

```csharp
using SOSXR.TinySynth;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public TinySynthSound sfxJump;
    public TinySynthSound sfxCoin;
}
```

Each field gets a rich property drawer in the Inspector: a live waveform preview, preset buttons, sliders for all
synthesis parameters, and a reset / lock button per parameter.

### 3 — Play at runtime

```csharp
private void OnJump()
{
    TinySynthPlayer.Play(sfxJump);
}
```

`Play` is static — no component reference needed. It retrieves (or generates) the cached `AudioClip` and routes it
through the next available polyphony slot.

### 4 — Pre-cache on scene load

Generating a clip takes a few milliseconds. To avoid any latency on first play, pre-cache everything at startup:

```csharp
private void Start()
{
    TinySynthPlayer.PreCache(this);
}
```

`PreCache` reflects over the entire assembly, finds every `TinySynthSound` field on every active `MonoBehaviour`,
and generates their clips up front.

## Usage

### Presets

`TinySynthPreset` provides seven randomised presets as a starting point:

```csharp
TinySynthSound laser     = TinySynthPreset.LaserShoot();
TinySynthSound coin      = TinySynthPreset.PickupCoin();
TinySynthSound explosion = TinySynthPreset.Explosion();
TinySynthSound powerUp   = TinySynthPreset.PowerUp();
TinySynthSound hit       = TinySynthPreset.HitHurt();
TinySynthSound jump      = TinySynthPreset.Jump();
TinySynthSound blip      = TinySynthPreset.BlipSelect();
```

Each call returns a freshly randomised variant within the archetype — re-call to get a different flavour.

In the Inspector, the preset row in the property drawer exposes the same seven buttons with a single click.

### Mutating and Randomizing

```csharp
sfxJump.Mutate();          // Nudges each parameter by up to ±0.05 (default)
sfxJump.Mutate(0.1f);      // Nudge by up to ±0.10
sfxJump.Randomize();       // Fully re-randomise all parameters
```

Individual parameters can be **locked** via the lock button in the Inspector drawer so they are excluded from
preset application and mutation.

### Copy / Paste settings strings

`TinySynthSound` supports serialisation to a comma-separated string compatible with SFXR (24 params) and BFXR
(32 params), enabling sharing between projects or with external tools:

```csharp
// Export
string settings = sfxJump.GetSettingsString();       // BFXR format (32 params)
string legacy   = sfxJump.GetSettingsStringLegacy(); // SFXR format (24 params)

// Import
sfxJump.SetSettingsString(settings);
```

### Exporting to WAV

Right-click (or use the `⋮` context menu) on any `TinySynthSound` field header in the Inspector and choose
**Export WAV** to save the current clip as a `.wav` file on disk.

### Getting the raw AudioClip

```csharp
AudioClip clip = TinySynthPlayer.GetClip(sfxJump);
audioSource.clip = clip;
audioSource.Play();
```

### Polyphony and retriggering

| Setting            | Inspector header                          | Default |
| ------------------ | ----------------------------------------- | ------- |
| `polyphony`        | Max simultaneous sounds                   | 1       |
| `minRetriggerTime` | Min seconds before re-triggering same sfx | 0.017 s |

Increasing polyphony adds `AudioSource` components automatically. `minRetriggerTime` prevents the same effect from
stacking too rapidly (e.g. rapid button mashing).

## Wave Types

| Value | Name      | Character                           |
| ----- | --------- | ----------------------------------- |
| 0     | Square    | Buzzy, retro; duty cycle adjustable |
| 1     | Sawtooth  | Bright, brassy                      |
| 2     | Sine      | Soft, pure                          |
| 3     | Noise     | White noise; explosions, static     |
| 4     | Triangle  | Warm, flute-like                    |
| 5     | PinkNoise | Warmer noise with 1/f spectrum      |
| 6     | Tan       | Harsh, distorted                    |
| 7     | Whistle   | Sine + high overtone; thin whistle  |
| 8     | Breaker   | Folded quadratic; rich harmonics    |

## Samples

Import the **Samples** package via the Package Manager window (`Samples` tab on the TinySynth entry) to get a
scene with pre-configured `TinySynthSound` values demonstrating each preset archetype.

## Origin and Licence

TinySynth is a fork of [usfxr](https://github.com/grapefrukt/usfxr) by Martin Jonasson (grapefrukt), which is
itself a port of [sfxr](http://www.drpetter.se/project_sfxr.html) by DrPetter and
[as3sfxr](http://superflashbros.net/as3sfxr/) / [bfxr](https://www.bfxr.net/) extensions.

Released under the MIT licence — see `LICENSE.md`.
