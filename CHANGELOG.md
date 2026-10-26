# Changelog

Player-facing changes for each Isekai Hero release are recorded here.

## [Unreleased]

## [v0.13.0-alpha] - 2026-10-26

### Added

- Added Ascendant Form, a rare 3-cost Power that grants two free conditional-effect bypasses each turn (three upgraded), with original card art and a remaining-use icon.
- Conditional cards now show the base game's animated gold outline when their bonus is available, including available Exploit, Beginner's Luck, and Ascendant Form bypasses.
- Added original resting and shop character artwork with subtle breathing animations.
- Added a dedicated I Am Atomic charge, declaration, and violet detonation animation, including fast/replay and reduced-effects variants.

### Changed

- Renamed Tutorial Sword to Tutorial Strike and Combo Rush to Breakthrough Strike. Both now support Strike Dummy. Breakthrough Strike costs 0 and deals 3 damage twice (4 upgraded), with a third hit after leveling up that turn.
- Alchemist now grants 3 Block and 3 Vigor (5 each upgraded) on the first enemy debuff each turn.
- Negotiation now costs 0, applies 1 Weak (2 upgraded), and grants 1 EXP.
- Exploit can now activate the combat-only Fatal rewards on Overkill, Mob Hunt, Last-Hit Bonus, and Truck-kun without a kill. Forced rewards do not grant kill credit or permanent rewards; real kills preserve available bypasses.

### Fixed

- Fixed Tutorial Strike failing to recognize the Alchemist Job.
- Fixed conditional previews recording Exploit triggers and repeated Exploit settlement. Checking the gold outline does not spend bypass uses.
- Fixed I Am Atomic's animation failing in game.

### Compatibility

- Tutorial Strike and Breakthrough Strike use new internal card IDs. Existing runs containing Tutorial Sword or Combo Rush may be incompatible; starting a new run is recommended.

## [v0.12.0-alpha] - 2026-10-19

### Changed

- Reworked Level Up rewards: reaching even Levels grants 1 Strength, and reaching odd Levels grants 1 Dexterity, replacing the previous Vigor reward. Levels 5 and 10 each grant 1 immediate Energy and 1 additional Energy per turn for the rest of combat.
- Reworked Hero's Judgment: it now Retains and gains 4 damage (6 upgraded) this combat for each Level Up while held in hand.
- Reworked Return by Death into a Power that prevents death once per combat, healing 15 HP and granting 8 EXP (20 HP and 10 EXP upgraded). All copies share the same rescue.
- EXPLOSION! now deals 36 damage (44 upgraded) to all enemies and ends your turn, replacing its next-turn Attack restriction.
- I Am Atomic's Exploit discounts now last until the card is played rather than the entire combat.
- Reduced Farm the Field and System Menu to 1 Energy. Increased Daily Training's base Block to 6 and Underdog Spirit's base and bonus damage to 6 each, with revised upgrade scaling.
- Shield Bash now always grants Block, with an additional Block grant if its condition is met. Tutorial Sword's bonus can also trigger after playing a Power this turn.
- Steal and Degenerate Tactics now Exhaust and award more Gold: 6/9 and 12/16 respectively for base/upgraded cards.
- Map Hack's upgrade now adds Retain instead of drawing an additional card.
- Shortened card descriptions, clarified conditional bonuses and Fatal text, added Exploit hover tips to conditional cards, and updated Level tooltips with stat and Energy explanations.
## [v0.11.1-alpha] - 2026-10-17

### Fixed

- Migrated the BaseLib dependency to the current manifest format, resolving the mod-manifest migration warning when starting multiplayer.
- Declared BaseLib 3.4.7 and Slay the Spire 2 0.107.1 as the minimum supported versions.

## [v0.11.0-alpha] - 2026-10-13

### Added

- Added Beginner's Luck Charm, OP Smartphone, and Forbidden Walkthrough, bringing the character relic pool to four relics including The System.
- Added original combat character art and animations for idle, relaxed, attack, cast, hit, defeat, and revive actions.
- Added an animated character-selection cover, a crystal energy counter, and hero headshot icons.
- Added dedicated Exploit and Level icons with visible EXP progress.

### Changed

- Replaced Return by Death and Grinding Montage reference screenshots with custom card art.
- Updated the character design roadmap and dependency tooling.


## [v0.10.0-alpha] - 2026-09-27

### Added

- Added Combo Rush, Cross-Class Combo, Level Grinding, Power-Up Montage, Skill
  Chain, Game Knowledge, Map Hack, and Steal Technique, expanding the early-alpha
  card pool from 43 to 51 cards.
- Added card art for all eight new cards.

### Changed

- Refined several existing cards and refreshed card art across the set.
- Updated the BaseLib dependency to 3.4.7.


## [v0.9.0-alpha] - 2026-08-25

### Added

- Added Death Flag, Underdog Spirit, Raid Opener, Counter Read, Barrier Magic,
  Starburst Stream, Slow Life, and Anti-Boss Art, expanding the early-alpha
  card pool from 35 to 43 cards.
- Added card art for all eight new cards.

### Changed

- Refined damage-variable handling for Beginner Magic, Duel, Megiddo, and
  Tutorial Sword so their displayed values and effects stay in sync.
## [v0.8.0-alpha] - 2026-07-23

### Added

- Added Daily Training, Study the System, Beginner Magic, Steal, Twin Blades,
  Farm the Field, Emergency Dodge, Negotiation, Overkill, and Hero's Judgment,
  expanding the early-alpha card pool from 25 to 35 cards.
- Added card art for all ten new cards.

### Changed

- Expanded the EXP/Level and Exploit support package with more early commons,
  direct EXP gain, Fatal EXP rewards, Gold gain, Weak application, AoE farming,
  and level-gated payoff attacks.

## [v0.7.0-alpha] - 2026-07-16

### Added

- Added Mob Hunt, Growth Slash, and EXPLOSION!, expanding the early-alpha card
  pool from 22 to 25 cards.
- Added card art for Mob Hunt, Growth Slash, and EXPLOSION!.

### Changed

- Updated the BaseLib dependency to 3.3.5.
- Synced Nexus release metadata automatically during the GitHub Actions release
  workflow.

## [v0.6.0-alpha] - 2026-07-16

### Added

- Added Shield Bash, Duel, and Degenerate Tactics, expanding the early-alpha
  card pool from 19 to 22 cards.

### Changed

- Updated the character energy UI and map marker art.
- Moved release builds, package validation, and Nexus publishing to a
  repeatable GitHub Actions workflow.

## [v0.5.0-alpha] - 2026-07-02

### Added

- Added the EXP and Level combat system, including Vigor rewards when leveling
  up.
- Added The System starter relic and rebuilt the starter deck around Grind and
  Danger Sense.
- Added dedicated card and relic art for Grind and The System.
