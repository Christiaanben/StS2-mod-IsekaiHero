# Rest-site and shop character art

The two original sprites follow the established anonymous office-worker hero: cream rolled sleeves, a purple tie/cape, dark trousers, brown gloves/boots and cyan system motifs. The user's cropped Ironclad screenshots supplied pose and style references only. The screenshots are not redistributed.

## Assets and prompts

- `IsekaiHero/images/character/hero_rest_site_v1.png`: 1254 x 1254 RGBA; seated hero on a log, facing right, sword safely lowered.
- `IsekaiHero/images/character/hero_shop_v1.png`: 1024 x 1536 RGBA; thoughtful standing hero facing left toward the merchant, sword lowered and a small cyan system panel.
- Both were generated/refined with the built-in imagegen tool and copied without raster post-processing. Generated alpha is preserved. Discarded atmospheric shop variants are not shipped.
- The exact generation/refinement prompts for the shipped images are in [room-character-art.json](art-prompts/room-character-art.json).
- [Native Godot preview](animations/room-character-art.png) shows the packaged textures on plain backgrounds. It is an asset/placement preview, not an in-game capture.

## Integration

`IsekaiHeroCode/Character/IsekaiHeroRoomArt.cs` uses Harmony postfixes on the native rest character's `_Ready` and the merchant room's `AfterRoomIsLoaded`. It checks the actual player's character before installing art and guards against duplicate installation. The shop uses the room's player order after it moves the local player to the first slot.

The inherited room scenes still load; their top-level Spine artwork is hidden only for Isekai Hero. Native rest-site controls, selection reticle, thought anchors, room layout and native animation calls remain in place. The seated sprite lives inside `ControlRoot`, so the game's `FlipX` also mirrors it. Its hitbox/reticle bounds cover the new silhouette. Other characters retain their native artwork.

Sprites use native nodes and bound breathing tweens, with no new C# Godot callbacks. Shop `die` requests also slump/dim the replacement art. The character preloads both textures. This avoids adding unsupported `.tscn` files to the quick PCK packer.

The raw image scales are 0.56 for the camp and 0.30 for the shop. Camp art uses the inherited scene's mid-body origin and 0.760006 root scale; shop art uses a feet origin. The original camp uses a 421 x 683 local hitbox; the new 530 x 660 bounds include the seated cape and log.

## Verification

- `dotnet build`: passed with zero warnings/errors; DLL/PCK installed by the normal local build.
- `scripts/verify_room_art.gd` against the built PCK in Godot 4.5.1: both texture remaps loaded; RGBA alpha checks passed; native render saved and visually inspected for clean transparency, full-body framing and style.
- Source review checked the patch targets, private shop player-list binding, local-player ordering, duplicate guard and rest-site mirroring against the local decompiled game.
- Full in-game rest/shop acceptance remains pending. The render check does not exercise Harmony hooks, real room entry or co-op behavior.

For packaged asset verification, use a minimal Godot project and run:

```text
Godot --headless --path <minimal-project> --script <repo>/scripts/verify_room_art.gd -- --pck=<built-IsekaiHero.pck>
```

To render the preview, omit `--headless`, use `--rendering-method gl_compatibility` and append `--preview=<output.png>` after `--`. The render requires a graphics backend.

Single-player acceptance: restart the game, enter a rest site and a shop as Isekai Hero, hover/select rest options, leave/re-enter rooms, and confirm placement and absence of visible Ironclad art. Check another character still shows its original art. Multiplayer-only hands and portrait outline remain separate pending assets; multiplayer testing is deferred until available.
