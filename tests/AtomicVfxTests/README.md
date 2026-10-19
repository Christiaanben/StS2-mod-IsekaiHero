# Atomic engine smoke tests

Build with `dotnet build tests/AtomicVfxTests`, then run a Godot 4.5.1 .NET executable with `--headless --path tests/AtomicVfxTests`. Success prints `6/6 engine checks passed` and exits with code 0.

Links the production Atomic controller and uses real Godot nodes, scene-tree processing, and tweens. Checks normal/fast/instant/replay/reduced playback, visible charge-to-impact transition, cleanup, and interruption. Game lookup/settings are boundary doubles. This is not a full game/co-op playtest.

The game log showed `ArgumentException` in the old custom node's generated `InvokeGodotClassMethod`. The test rejects scripted effect nodes so that failing callback path cannot return. Unlike the earlier preview, it does not manually invoke processing or advance tweens.
