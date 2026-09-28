# Relic behavior tests

Run from the repository root:

```powershell
dotnet run --project tests/RelicTests
```

This dependency-free console test suite returns a nonzero exit code on failure. It links the production relic classes, card condition resolver, Exploit power, and Level power directly. The game/Godot boundary is replaced with minimal command-recording doubles so the suite can run without launching the game or redistributing its assemblies. It tests decisions and command ordering, not Godot rendering, actual damage mitigation, hook dispatch, or multiplayer networking. `dotnet build -p:InstallModOnBuild=false` separately checks the real game API and packages resources.

Coverage includes natural/Override/luck/stack precedence, first-unmet usage, replays, repeated checks within a play, non-consuming checks outside play, combat reset, owner isolation, the three-stack opening grant, random target selection, exact damage and Vigor ordering, level-cap overflow, kill-EXP re-entry, combat termination, and recovery after a reaction exception.

In-game smoke checks still required:

- Acquire each relic from the Isekai Hero pool; check small, large, and outline art plus tooltips.
- With the charm and walkthrough, play an unmet conditional card twice: the first uses luck; the second uses one of the three Exploit stacks. Confirm natural conditions and Override preserve luck.
- With The System and OP Smartphone, gain enough EXP for multiple levels; confirm one 5-damage hit per level, kill EXP, and no extra hits after combat ends.
- Enter another combat and verify luck and the opening Exploit grant reset. Check a co-op ally neither spends your luck nor triggers your smartphone.
