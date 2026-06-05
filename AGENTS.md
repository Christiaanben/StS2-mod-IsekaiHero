# IsekaiHero Agent Notes

## Build

- Use `dotnet build` for normal verification.

## Base Game References

- Base-game logic lives in `sts2.dll` under the Slay the Spire 2 `data_*`
  directory.
- ILSpy is installed as a repo-local .NET tool in `dotnet-tools.json`.
- Restore the tool with `dotnet tool restore`.
- Decompile the current local game DLL with:
  `dotnet tool run ilspycmd -p -o Scratch\Decompiled\sts2 "D:\SteamLibrary\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll"`
- Read decompiled cards under
  `Scratch\Decompiled\sts2\MegaCrit.Sts2.Core.Models.Cards\`.
- Spire Codex documents this workflow and its data pipeline:
  https://github.com/ptrlrd/spire-codex

## Card Text

- For upgradeable numbers, add a `DynamicVar` in `CanonicalVars`, reference its
  exact name in `CardLoc` with `!Name!`, and upgrade the same var in
  `OnUpgrade()`.
- Start dynamic-var card descriptions with `#`, for example:
  `"# Gain !Block! Block."`
- For card-count values, use `new CardsVar(3)`, `!Cards!`, and
  `DynamicVars.Cards.UpgradeValueBy(...)`.

## Card Selection

- For arbitrary card lists, use `CardSelectCmd.FromSimpleGrid(...)`.
- Add `("selectionScreenPrompt", "...")` to `CardLoc` before using a card's
  `SelectionScreenPrompt` in `CardSelectorPrefs`.
- To move a selected draw-pile card into hand, use
  `CardPileCmd.Add(card, PileType.Hand)`.
