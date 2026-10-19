# Relic and card behavior tests

Run from the repository root:

```powershell
dotnet run --project tests/RelicTests
```

This dependency-free console test suite returns a nonzero exit code on failure. It links the production relic classes, revised cards, card condition resolver, Exploit/Level powers, and Return by Death rescue directly. The game/Godot boundary is replaced with minimal command-recording doubles so the suite can run without launching the game or redistributing its assemblies. It tests decisions and command ordering, not Godot rendering, actual damage mitigation, hook dispatch, or multiplayer networking. `dotnet build -p:InstallModOnBuild=false` separately checks the real game API and packages resources.

Coverage includes natural/Override/luck/stack precedence, first-unmet usage, replays, repeated checks within a play, non-consuming checks outside play, combat reset, owner isolation, the three-stack opening grant, random target selection, exact damage and Vigor ordering, level-cap overflow, kill-EXP re-entry, combat termination, and recovery after a reaction exception.

In-game smoke checks still required:

- Acquire each relic from the Isekai Hero pool; check small, large, and outline art plus tooltips.
- With the charm and walkthrough, play an unmet conditional card twice: the first uses luck; the second uses one of the three Exploit stacks. Confirm natural conditions and Override preserve luck.
- With The System and OP Smartphone, gain enough EXP for multiple levels; confirm one 5-damage hit per level, kill EXP, and no extra hits after combat ends.
- Enter another combat and verify luck and the opening Exploit grant reset. Check a co-op ally neither spends your luck nor triggers your smartphone.

## Card revision coverage

The suite also checks revised values/upgrades, Map Hack Retain and unchanged draw count, System Menu discard selection, the unchanged Level Grinding/Power-Up Montage roles, Shield Bash ordering, selected-target intent, Tutorial Sword's Job/Power alternatives, and Combo Rush's retained Level threshold. Judgment tests cover actual Level Ups, multi-level gains, before-relic ordering, changing hand membership, cap overflow, allies, upgrade growth, and combat cleanup. Rescue tests cover base/upgraded HP and EXP, reservation before reactions, duplicate/reapplied copies, owner isolation, HP caps, and new combats. Atomic tests cover until-played offsets, independent copies, fresh discounts, and unrelated combat modifiers.

The command doubles record EXPLOSION's damage-before-EndTurn order and skip the turn request after lethal combat completion. Actual end-turn discard, Retain presentation, enemy turns, multiplayer synchronization, native cost cleanup dispatch, and death-prevention precedence still require the game. The doubles model these APIs; they do not prove engine dispatch or rendering.

Additional in-game smoke checks:

- Inspect both upgrade states of changed cards, conditional Exploit tooltips, and gold-highlighted Fatal. Verify System Menu's selection prompt and Map Hack's Retain.
- Hold Judgment through several Level Ups, discard/redraw it, then enter a new combat; check damage previews and reset. Check temporary upgrade/downgrade preserves earned growth.
- Play EXPLOSION with enemies surviving: confirm ordinary end-turn effects/discard occur once and Attacks work normally next turn. Repeat with lethal damage and in co-op.
- Trigger Return by Death on a multi-hit, then take another lethal hit. Test upgraded/duplicate copies, a second combat, and Lizard Tail or another late rescue: only one save should fire per lethal event.
- Discount Atomic, retain or discard/redraw it, play and recover it from Exhaust, and verify fresh discounts and reset without clearing other cost effects. Atomic+ must apply Vulnerable before damage.
