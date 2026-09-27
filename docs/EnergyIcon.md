# Energy counter artwork

The combat energy counter uses `IsekaiHero/images/charui/energy_diamond_v1.png`, generated with the built-in imagegen tool and copied without raster processing. The cyan crystal diamond follows the menu-cursor silhouette in the character design; its violet and cyan swirl echoes the character selection portal.

`IsekaiHero.cs` uses BaseLib's simple `CustomEnergyCounter` API to display it at 128 × 128 with the game's dynamic numeric label and font. The game retains its energy updates, empty-energy darkening, hover tip, and entrance/exit motion. The painted vortex rotates inside a softly masked circular well; the crystal frame stays stationary. `IsekaiHeroEnergyArt.cs` generates and caches mipmaps and enables linear mipmap filtering to smooth the source at HUD size. A character-specific hook restores the animation material after energy-label refreshes, including depleted desaturation and the game's dark modulation. A transparent one-pixel PNG fills the four unused layers. Both texture paths are preloaded. The inherited placeholder scene path is cleared so BaseLib selects the custom counter.

The simple API keeps the normal quick PCK build compatible: the packer supports PNG assets but skips packing when it finds a `.tscn` scene.

This asset is for the combat counter. Existing card-cost and inline energy icons remain separate assets.

## Generation prompt

Use case: stylized-concept. Create one production game UI energy-counter icon for Isekai Hero in Slay the Spire 2. Transparent background. A bold squat diamond-shaped menu-cursor crystal medallion, with four pointed corners, chunky faceted bevel border, dark ink outline. Cyan/teal crystalline rim with restrained indigo-violet shadow facets; inner disk filled by a broad hand-painted cyan and pale turquoise swirling vortex over deep blue-violet. Center must remain visually simple and broad enough for the game to overlay 3/3. Match the attached game's energy icons: imperfect angular silhouette, flat chunky painterly planes, subtle dry-brush texture, strong readability at 128 pixels, stylized 2D illustrated fantasy UI. Inspired by the character's floating cyan crystal and violet portal. Front-facing, centered, square canvas with small even transparent margin, occupies 90% width and height. No text or numbers baked in. No extra star, no secondary energy pool, no particles outside silhouette, no scene, no tiny decorations, no realistic glossy gemstone rendering, no drop shadow on background. Deliver only the single icon.
