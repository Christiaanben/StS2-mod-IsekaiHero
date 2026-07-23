# Isekai Hero

Isekai Hero is an alpha playable character mod for Slay the Spire 2.

**Current release:** `v0.8.0-alpha`

The character is a genre-parody power fantasy built around outsider logic:
gain a **Job**, find ways to **Exploit** the Spire's systems, and earn absurd
**Cheat Skills** without turning every combat into an automatic win.

## Alpha Status

The current release is an early public test build. It includes:

- The Isekai Hero playable character.
- A starter deck with 4 Strikes, 4 Defends, Grind, Danger Sense, and the
  starter relic The System.
- A 35-card custom set with cards such as Status Appraisal, Training Arc, Route
  Guide, Truck-kun, Mob Hunt, Daily Training, Study the System, Twin Blades,
  Farm the Field, Overkill, Hero's Judgment, EXPLOSION!, and I Am Atomic.

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

## Design

The full character design — mechanics (EXP/Level, Exploit, Quests), the 88-card
list, relics, potions, balance guardrails, and the implementation roadmap —
lives in [docs/IsekaiHero_Design.md](docs/IsekaiHero_Design.md).

## Build

The PCK packer runs as part of the normal build. Local builds automatically use
the installed game assemblies and copy the result into the mods folder. When
the game is unavailable, such as in GitHub Actions, the project uses stripped
reference assemblies from `Book.StS2.RefLib` instead.

```powershell
dotnet build
```

## Releasing

Public releases use a deliberate manual GitHub Actions workflow. The mod is
built and packaged on a GitHub-hosted runner; version, changelog, README, and
package checks run before publishing to GitHub and Nexus Mods. See
[docs/RELEASING.md](docs/RELEASING.md) for setup and the short release checklist.
