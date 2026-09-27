# Level power artwork and EXP display

The level badge is a gold double chevron over a teal crystal. Transparent PNGs
are provided at 64×64 (`IsekaiHero/images/powers/level_power.png`) and 256×256
(`IsekaiHero/images/powers/big/level_power.png`). The existing power image resolver
loads these by name. Generated with the built-in imagegen tool, then resized
with System.Drawing high-quality bicubic interpolation, preserving alpha.

The number remains the current level. Four outlined slots beneath the badge
show progress toward the next level: 0–3 cyan fills, resetting on the fourth EXP.
At level 10 all four slots turn gold. Hovering shows exact EXP, or banked EXP
at the cap. Existing level-up costs, Vigor rewards, and surplus banking remain
unchanged. The display works on any creature using this power.

`LevelPowerProgress` patches `NPower.RefreshAmount`, the same path used for
initialization and level changes. EXP gains explicitly trigger that refresh
after processing level-ups. The bar ignores mouse input and uses no polling
or additional event subscriptions. The patch targets a private game method;
recheck compatibility after game updates.

## In-game acceptance checks

- Gain 1, 2, and 3 EXP without leveling: check the matching filled slots and tooltip.
- Gain the fourth EXP: level increases, slots reset, and 2 Vigor is awarded.
- Gain multiple levels at once: check final remainder and Vigor per level.
- Reach level 10 with surplus EXP: full gold bar, level stays 10, tooltip shows banked EXP.
- Gain more EXP at level 10: bank increases without further Vigor.
- Check another power beside Level, hover/controller tooltips, and an enemy
  with Level for overlap and mouse input behavior.

## Generation prompt

Use case: stylized-concept. Asset type: fantasy roguelike buff icon. Create a single beautifully hand-painted game UI icon of a bold gold double upward chevron, like a level-up rank insignia, over a compact faceted deep teal crystal medallion. Thick dark silhouette, bright ivory gold edges, restrained painterly shading, highly readable at 64x64 pixels. Centered square composition, icon occupies 85 percent of canvas. Transparent background with real alpha, no background square, no text, no digits, no border frame, no particles or detached ornaments, no progress bar. Deliver a clean 256x256 master if supported; will also be downsampled to 64x64.
