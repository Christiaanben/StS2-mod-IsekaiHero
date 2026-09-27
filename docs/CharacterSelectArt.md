# Character selection cover

The active cover is `IsekaiHero/images/character/hero_select_cover_v3.png`. It was generated with the built-in imagegen tool and copied without raster processing. No game UI is baked into the artwork. Earlier drafts are retained for reference; only version 3 is used. See [generation prompt](CharacterSelectArt-v3-prompt.md).

## Rendering and effects

`IsekaiHeroSelectArt.cs` replaces this character's entire placeholder background after normal and random selection. It preserves the parent, sibling position and character-specific node name. A native TextureRect displays the cover with aspect-preserving fill. A canvas shader animates cyan motes and a pulsing crystal glow; a small pass-through hover target tweens the glow intensity.

The renderer uses native nodes and shader rendering to avoid the managed Godot virtual callbacks that failed in the game runtime.

The shipped AnimatedBg container has offsets (-388, -80, +252, +40), scale (1.1, 1.1), and pivot (1280, 600). The cover maps the screen rectangle through the inverse container transform and updates on screen/container changes, avoiding inherited zoom. Resize subscriptions are removed when the cover exits the tree.

## Verification

- `dotnet build`: passed with zero warnings/errors; DLL and PCK installed into the game.
- Native Godot smoke check: version 3 texture loaded from the built PCK; TextureRect and shader instantiated; hover parameter set.
- Native Godot layout assertions: both cover corners matched the screen at 1920x1080, 2560x1440, 2560x1369 and 1024x768 despite the oversized parent.
- User confirmed the final result works correctly in-game.

Random selection and every resize/switching sequence have not been independently exercised in-game.
