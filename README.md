# Isekai Hero

Isekai Hero is an alpha playable character mod for Slay the Spire 2.

The character is a genre-parody power fantasy built around outsider logic:
gain a **Job**, find ways to **Exploit** the Spire's systems, and earn absurd
**Cheat Skills** without turning every combat into an automatic win.

## Alpha Status

The `v0.1.0-alpha` release is an early public test build. It currently includes:

- The Isekai Hero playable character.
- A starter deck with Isekai Hero Defends and the starter relic Veil of the
  Unseen.
- A small custom card set with early cards such as Status Appraisal, Seen it
  Coming, Truck-kun, Return by Death, Megiddo, and I Am Atomic.

Expect the card pool, balance, visuals, and mechanics to change while the
character moves toward its Jobs, Exploits, and Cheat Skills identity.

## Requirements

- Slay the Spire 2.
- [BaseLib for Slay the Spire 2](https://github.com/Alchyr/BaseLib-StS2).

## Install

1. Install BaseLib first.
2. Download the latest `IsekaiHero` release zip.
3. Extract the `IsekaiHero` folder into the Slay the Spire 2 `mods` folder.
4. Confirm the extracted folder contains:
   - `IsekaiHero.dll`
   - `IsekaiHero.pck`
   - `IsekaiHero.json`
5. Launch Slay the Spire 2 and load mods when prompted.

## Release Contents

The installable zip for this mod should use this layout:

```text
IsekaiHero/
  IsekaiHero.dll
  IsekaiHero.pck
  IsekaiHero.json
```

BaseLib is a dependency and is not bundled with this mod.

## Design Goal

The current character goal and design filters live in
[docs/isekai-hero-goal.md](docs/isekai-hero-goal.md).

## Build

Publishing requires the local Slay the Spire 2 and MegaDot/Godot paths expected
by the project files.

```powershell
dotnet publish -c Release
```
