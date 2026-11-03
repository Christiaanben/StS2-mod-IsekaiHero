# Ancient reward art provenance

Generated with the built-in image generation tool on 2026-11-02. One generation per portrait; no revision prompts. These are original compositions based on thematic scene inspiration, not verified recreations of specific anime frames. No episode or timestamp is claimed.

## Power Leveling

- Anime: *So I'm a Spider, So What?*
- Scene inspiration: Kumoko grinding through monsters and evolving in the Great Elroe Labyrinth.
- Original composition: the mod's human office-worker hero slashes through insect/reptile silhouettes amid cyan progression rings and a violet evolution halo.
- Assets: `IsekaiHero/images/card_portraits/power_leveling.png` (250×190), `big/power_leveling.png` (1000×760), and `beta/power_leveling.png` (250×190).
- Full-resolution generated master: `docs/art/ancient-rewards/power_leveling.png`.

### Exact generation prompt

```text
Use case: illustration-story. Create a finished landscape 1000x760 card-art illustration for a fantasy deckbuilding game. Original hand-painted stylized illustration with chunky angular silhouettes, thick confident shapes, broad flat shadows, restrained detail readable at thumbnail size, cyan magical interface motifs and deep purple palette. Hero is an anonymous young adult office-worker adventurer: messy dark hair, cream rolled-sleeve shirt, purple tie and short purple cape, charcoal trousers, brown gloves and boots. Full bleed artwork only, no card frame, no lettering, numbers, logos or watermark. Keep main action inside central 80 percent for cropping. Scene: a dark subterranean labyrinth. The hero lunges diagonally with a sword through successive shadowy insect and reptile monster silhouettes, luminous cyan progression rings converging around his torso and blade, a violet evolution halo behind him. Dynamic forward momentum and delighted hard-earned breakthrough, strong readable hero silhouette. Thematic inspiration: Kumoko's desperate grinding and evolution in the Great Elroe Labyrinth in So I'm a Spider, So What?; reinterpret the theme with our original human hero, do not recreate a specific anime frame.
```

## NEW GAME+

- Anime: *Re:Zero*.
- Scene inspiration: Subaru awakening at a Return by Death checkpoint remembering a failed timeline.
- Original composition: the mod's hero reaches through a cyan restart window while a spectral sword slash, spell orb, and shield return to his hand.
- Assets: `IsekaiHero/images/card_portraits/new_game_plus.png` (250×190), `big/new_game_plus.png` (1000×760), and `beta/new_game_plus.png` (250×190).
- Full-resolution generated master: `docs/art/ancient-rewards/new_game_plus.png`.

### Exact generation prompt

```text
Use case: illustration-story. Create a finished landscape 1000x760 card-art illustration for a fantasy deckbuilding game. Original hand-painted stylized illustration with chunky angular silhouettes, thick confident shapes, broad flat shadows, restrained detail readable at thumbnail size, cyan magical interface motifs and deep purple palette. Hero is an anonymous young adult office-worker adventurer: messy dark hair, cream rolled-sleeve shirt, purple tie and short purple cape, charcoal trousers, brown gloves and boots. Full bleed artwork only, no card frame, no lettering, numbers, logos or watermark. Keep main action inside central 80 percent for cropping. Scene: a fractured dark fantasy checkpoint with ghostly echoes of a failed battle. The hero reaches through a luminous cyan rectangular restart window; three large spectral techniques curve back toward his open gloved hand: a sword slash, a violet spell orb, and a cyan shield. His expression is knowing determination, as though he remembers the previous attempt. The portal and returning techniques dominate the composition, the hero occupies center-right. Thematic inspiration: Subaru awakening at a Return by Death checkpoint remembering the failed timeline in Re:Zero; reinterpret the theme with our original hero, do not recreate a specific anime frame.
```

## Derivation and review

Preserve each generated master. Derive portraits with a centered 25:19 crop and Lanczos downsampling; beta uses the same final art. No separate upgraded illustration is needed. Portrait inspection checks composition, lack of lettering, thumbnail readability, and crop safety. Native card-text layout and in-game acceptance remain pending.


## Implementation validation

- `dotnet build` against the installed game, installation disabled: zero warnings/errors; PCK packaging succeeded.
- All six game portrait paths verified in the PCK; dimensions checked at 1000×760 and 250×190.
- **74/74 headless tests passed.** Tests cover the linked production card logic with command-recording game doubles. They verify ordering, filtering, retrieval and cost contracts; native reward selection, native Strength damage calculation, enchantment transfer and rendering still require game acceptance.

### In-game acceptance checklist (pending)

- [ ] Enter Darv as Isekai Hero: no Dusty Tome setup exception; reward preview and obtained card show NEW GAME+ upgraded (4 selections).
- [ ] With Grind/Grind+ and an enchanted Grind, verify Tooth previews and transforms into the corresponding Power Leveling state, preserving the enchantment. Without Grind, no valid Tooth choice is offered.
- [ ] Verify Power Leveling grants EXP before both hits, including newly earned Strength and Level 5/10 energy; confirm capped EXP banks and combat-ending reactions suppress the attack.
- [ ] Play a Power, Atomic, and ordinary cards; use NEW GAME+ to retrieve the original instances. Existing Power effects remain, normal stacking applies on replay, and Atomic leaves Exhaust.
- [ ] Verify optional 0–3/4 selection, the currently resolving copy excluded, prior-copy retrieval, X/additional resource costs, full-hand overflow, co-op owner filtering and end-of-turn discount cleanup.
- [ ] Inspect both cards unupgraded and upgraded in the native card viewer: readable text, correct numbers, unclipped portraits and selection prompt.
- [ ] Verify an intentional two-copy NEW GAME+ loop works and ordinary one-copy play cannot retrieve itself while resolving.
