# Character animation art

The combat hero follows the office-worker/system-reader premise in `IsekaiHero_Design.md`: a rolled-sleeve shirt and loose tie under an improvised purple cape, with cyan status panels. User-supplied Slay the Spire 2 character screenshots informed the angular silhouettes, broad flat shadows, oversized hands and boots, and reduced surface detail.

## Files and preview

- `IsekaiHero/images/character/hero_combat_atlas.png`: original 1536 × 1024 RGBA atlas, six poses. Alpha is preserved unmodified from generation.
- `IsekaiHero/animations/hero_combat.tres`: seven native Godot animations.
- [Interactive preview](animations/preview.html): open locally in a browser; includes light/dark/checkerboard backdrops, pause and quarter-speed playback.
- [Godot-rendered contact sheet](animations/contact-sheet.png): six pose samples rendered from the native animation tracks.
- `scripts/build_character_animations.py`: source of pose regions, anchors and timing; regenerates both the Godot library and the preview with Python 3, no third-party packages.
- `IsekaiHeroCode/Character/IsekaiHeroVisuals.cs`: constructs the combat visual nodes and queues idle after one-shot actions. BaseLib routes the game's animation triggers to the AnimationPlayer.

## Animation contract

| Trigger | Duration | Completion |
| --- | --- | --- |
| Idle | 2.0 s | Loop breathing/sway |
| Relaxed | 2.8 s | Loop subdued breathing |
| Attack | 0.6 s | Wind-up → slash → Idle |
| Cast | 0.8 s | System diamond gesture → Idle |
| Hit | 0.4 s | Recoil → Idle |
| Dead | 1.2 s | Collapse, hold final pose |
| Revive | 0.7 s | Rise → Idle |

This is a pose-based set with interpolated movement, scale and rotation, not a Spine skeletal rig or individually drawn in-between frames. The sprite is rendered at 65% atlas resolution, approximately 270 px tall in idle. Pose-specific anchors keep feet on the ground; regions include the cape and spell overhangs. Every animation resets all animated transforms so interrupted actions do not leave stale offsets. Bounds, intent, center, orb and speech anchors satisfy NCreatureVisuals' node contract. Existing character-select, merchant and rest-site placeholders are outside this combat animation pass.

No new Harmony patches are needed. The visual factory avoids `.tscn` files, which the current quick PCK packer does not support; `.tres` and PNG are supported. The character adds its atlas to preloaded asset paths.

## Generation provenance

Generated using the built-in imagegen tool, then copied into the repository without raster post-processing. Initial painterly drafts were discarded after the user supplied in-game style references. The references are not redistributed in the repository.

Final generation prompt:

> Use case: stylized-concept. Create a NEW original Slay the Spire 2 mod hero combat sprite atlas. References 1-3 are STYLE ONLY: copy their simplified angular chunky silhouette language, large flat color blocks, hard geometric shadows and minimal detail. NO anime rendering, NO painterly texture, NO gradients, NO atmospheric haze. Character: anonymous androgynous office worker reincarnated as adventurer, messy dark hair with only 5 large spikes, cream rolled-sleeve shirt, loose purple tie, short purple cape, charcoal trousers, brown boots, short broad steel sword, floating cyan system panel with simple diamond and 3 bars. Clear face with minimal features. Exactly SIX isolated full-body poses on TRANSPARENT ALPHA background, arranged strict equal 3 columns by 2 rows. 1536x1024 atlas; each cell 512x512. Keep ALL art within each cell with 35px clear margins. Same scale and ground baseline y=465 within cells. All face right. Top-left idle holding sword down, left hand at floating small cyan menu. Top-middle anticipation: sword raised back. Top-right sword attack lunge with small sharp cyan slash. Bottom-left casting: hand extended cyan diamond. Bottom-middle hurt: recoiling back. Bottom-right defeat: fallen to one knee, head bowed, sword on floor. Oversized boots and hands for clarity like supplied in-game sprites. Tight 12-color palette, crisp edges, graphic cutout look. No background color anywhere, genuinely transparent empty pixels. No grid lines, text, UI health bar or labels.

## Verification

- `dotnet build -p:InstallModOnBuild=false`: passed, zero warnings/errors, PCK produced. The flag leaves the installed mod untouched.
- `scripts/verify_character_animations.gd` in Godot 4.5.1: passed both against source resources and the built PCK (including its PNG import remap). Checks all seven animations, alpha, track paths, region bounds, action-to-idle transitions, death hold and interruption resets.
- Native Godot contact sheet rendered and inspected for transparency, pose framing and style.
- Full in-game combat has not been exercised. The HTML preview is supplied for local review; automated browser access to local files was blocked.

The smoke script can run in a minimal Godot project without the game's C# assemblies. Pass `-- --pck=<absolute path to IsekaiHero.pck>` after `--script <absolute path to verify_character_animations.gd>` to test the packaged resources. Running it from the mod project itself may emit unrelated missing-game-assembly errors from Godot's C# loader; the isolated project avoids those.

For the in-game check, enter combat as Isekai Hero, play an Attack and Skill, take damage, end combat, and exercise death/revival (including Return by Death). Check co-op mirroring, intent/health-bar placement and interruption of actions at fast speed. Browser preview validates appearance and authored timing, not game integration.
