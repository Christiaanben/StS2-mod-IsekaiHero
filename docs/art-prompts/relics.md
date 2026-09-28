# Relic artwork

Generated with the built-in image_gen tool, using the supplied base-game relic sheet as the visual style reference. Each asset was generated separately with a transparent background. Sources are kept at 1280px in `docs/art-prompts/relic-sources/`; runtime icons are 94px, large icons are 256px, and white outline masks are 94px. `scripts/prepare_relic_art.ps1` reproduces the runtime derivatives using the source alpha.

Common prompt (substitute the subject below):

> Use case: stylized-concept. Asset type: Slay the Spire relic inventory icon. Subject: SUBJECT. Match the attached relic sheet's art style: chunky dark ink outlines, hand-painted cartoon fantasy object, simple faceted shading, saturated restrained colors, slightly irregular silhouette, highly readable at 96 pixels. One isolated centered object, generous transparent padding, square canvas. No words, no letters, no watermark, no frame, no scenery. Genuine transparent background.

- `beginners_luck_charm`: a four-leaf green clover preserved inside a small battered smartphone case, showing the back of the case, lucky charm
- `op_smartphone`: a slightly tilted black smartphone with chunky worn brass corners, luminous cyan screen, a tiny crossed-out signal icon and one bold golden lightning bolt on the screen
- `forbidden_walkthrough`: a chunky worn purple strategy guide book for a forbidden fantasy world, brass corner guards, a mysterious cyan maze-arrow emblem on the cover, a red ribbon bookmark
