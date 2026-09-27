# Isekai Hero: fun, balance, distinctiveness, and replayability review

Research date: 27 September 2026. Scope: the **planned Design v2.0 pool of 88 cards, 9 relics, 3 potions, and 8 Quest tokens**, with implementation spot-checks. This is a design review, not a measured win-rate study or a claim that the full proposed pool has been playtested.

## Verdict

**Isekai Hero has the ingredients of a worthwhile sixth character, but the current plan does not yet demonstrate base-game-level replayability.** It has some excellent tactical ideas and some genuinely exciting payoffs. Its biggest problem is not universally low power: it mixes weak setup cards, unusually efficient filtering, dangerous repeatable rewards, and capstones that can switch off its own interesting decisions.

If implemented unchanged, I expect a character that is enjoyable to discover, occasionally spectacular, but increasingly repetitive once the player identifies its best engines. Some runs could feel like an underpowered scaling character; others could exploit economy or recursion holes. Neither outcome reliably delivers “clever first, strong second.” This is a prediction from the rules, not player telemetry.

The best version of this character is **an adventurer who engineers the next breakthrough**: prepare the right hand, cross a Level threshold at the right moment, manipulate an objective, and decide which rule is worth cheating. The weaker version is “play ordinary damage and Block cards until their second lines turn on.” The design currently contains both.

| Dimension | Judgment | Confidence |
|---|---|---|
| Fantasy and first-run appeal | Strong. EXP popups, objectives, inventory tricks, and theatrical finishers belong together. | High as a design judgment |
| Individual card impact | Uneven. Atomic and Vigor combinations can impress; several expensive setup cards and rares disappoint on paper. | High for the numerical comparisons |
| Tactical depth | Real potential in Vigor allocation, kill order, retention, and limited cheats. Permanent overrides can erase it. | High for the structural issue |
| Long-term replayability | At risk. Four advertised builds do not yet mean four substantially different drafting and combat experiences. | Medium until full-pool playtests |
| Originality | Stronger as a connected system than as individual effects. Several cards are generic or near-duplicates. | High |
| Balance readiness | Not ready for final number tuning. Resolve recursion, repeatable rewards, Quest timing, and role overlap first. | High |

**Recommendation:** keep the core, improve its decisions, and replace redundant cards before expanding the pool. Do not add a fourth resource or turn Jobs into another Level-scaling system.

## 1. Research basis and corrections to the design's assumptions

I read the [design document](IsekaiHero_Design.md), the README, and relevant current mod classes. I consulted the supplied [character overview](https://slaythespire-2.com/characters) and its five character guides. That site is useful for orientation, but its tier labels and some mechanical descriptions are not reliable enough to set balance targets.

For numerical comparisons, I **freshly decompiled the installed `sts2.dll`**, rather than relying on the older existing decompilation. The reviewed DLL is 9,364,480 bytes, modified 19 June 2026, SHA-256 `A1F9E653F1E28E4076558FEE1E60D218619CB7E057B887C6417F62C62C6D7A52`. These findings describe that local build; they are not a claim about the latest September beta. The fresh reference files are in `Scratch/ResearchBalance/sts2/`, which is ignored by Git. Source links below point there; the numerical findings remain in this report even if Scratch is later removed.

The [Spire Codex project](https://github.com/ptrlrd/spire-codex) documents the decompilation/data-extraction approach. Mega Crit's [v0.100.0 patch announcement](https://store.steampowered.com/news/app/2868840/view/503978984819655259) also illustrates why a patch-specific baseline matters; no balance conclusion here depends on reconstructing its patch notes.

Several assumptions should be corrected before the next design revision:

- **There is no universal “four basics, 4/4+2 starter deck” rule.** The installed Ironclad starts with five Strikes, four Defends, and Bash. Matching another character's card counts is an organizational choice, not evidence of design quality. [Source: Ironclad](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Characters/Ironclad.cs).
- **Strike and Defend are intentionally poor reward-card baselines.** The relevant comparison is the card a player might actually draft. Shrug It Off gives 8 Block and a draw for 1 Energy; Backflip gives 5 Block and two draws; Twin Strike deals 5 twice. [Sources: Shrug It Off](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/ShrugItOff.cs), [Backflip](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/Backflip.cs), [Twin Strike](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/TwinStrike.cs).
- **The native Quest type is not already this combat-objective mechanic.** The installed Quest pool contains Byrdonis Egg, Lantern Key, and Spoils Map, with rest-site, event, and map interactions. It even reports `IsColorless => false`. Reusing the type is sensible; calling the proposed system merely an expansion of three colorless combat Quests is misleading. [Sources: Quest pool](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.CardPools/QuestCardPool.cs), [Spoils Map](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/SpoilsMap.cs), [Byrdonis Egg](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/ByrdonisEgg.cs), [Lantern Key](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/LanternKey.cs).
- **“Never dead, never broken” is too strong.** EXP can lose most of its value at the cap, kill rewards often arrive after the fight, and a base-rate conditional card can still be an undesirable draw. Cross-class combinations require their own tests.
- **The claimed 60% trigger-rate formula is wrong even on its own terms.** `0.4 × 0.85 + 0.6 × 1.15 = 1.03`, not 1.00. More fundamentally, conditions have different availability and strategic costs; one global percentage cannot price them.
- **The immediate-value rule is internally inconsistent.** Level Grinding costs two of the normal three Energy. Read the Code and System Menu also provide no direct damage, Block, or draw. Setup can be good without an immediate rider, but exceptions should be justified by their actual payoff.

The supplied site's [overview](https://slaythespire-2.com/characters) describes Sovereign Blade as Innate and Doom as automatically doubling attack damage. The local Blade has Retain, and Doom checks whether its own stacks reach remaining HP; ordinary attacks do not universally apply matching Doom. These are reasons to prefer the local rules over the site's rankings. [Sources: Sovereign Blade](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/SovereignBlade.cs), [Doom](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Powers/DoomPower.cs).

## 2. What the base game's depth actually asks you to match

The important comparison is not the number of named archetypes. It is whether different rewards change what the player values, what risks they take, and how they sequence a turn.

| Character | Concrete source of depth in the installed game | Lesson for Isekai Hero |
|---|---|---|
| Ironclad | Body Slam converts Block into offense; Feel No Pain and Dark Embrace turn Exhaust into defense and draw. The same card can become fuel, protection, or damage. | Let an awkward resource become valuable in more than one way. A Quest should sometimes be worth holding or abandoning, not always completing immediately. |
| Silent | Discard changes hand quality and can interact with other payoffs; Grand Finale requires an empty draw pile and rewards precise deck manipulation with 60 AoE for zero Energy. | A large payoff becomes distinctive when the player engineers its availability. The condition matters as much as the number. |
| Defect | Orb channeling provides ongoing value and affects future evokes; Focus and Echo Form change the value of existing cards and play order. | Setup should create future decisions. Permanent stats alone are less interesting than stats that change which action is best next. |
| Regent | Stars and Forge are different development paths; The Smith converts four Stars and one Energy into 30 Forge. Sovereign Blade is a retained evolving tool. | A meaningful build fork changes resource priorities and hand management, rather than merely using a different generator. |
| Necrobinder | Grave Warden protects now and inserts a Soul for later; Devour Life makes playing Souls summon Osty; Time's Up turns Doom into immediate attack damage. | Bridges should create new uses for a resource. “Another route to more EXP” is useful cohesion, but insufficient variety on its own. |

Sources: [Body Slam](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/BodySlam.cs), [Feel No Pain](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/FeelNoPain.cs), [Dark Embrace](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/DarkEmbrace.cs), [Grand Finale](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/GrandFinale.cs), [Calculated Gamble](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/CalculatedGamble.cs), [Coolheaded](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/Coolheaded.cs), [Defragment](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/Defragment.cs), [Echo Form](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/EchoForm.cs), [The Smith](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/TheSmith.cs), [Grave Warden](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/GraveWarden.cs), [Devour Life](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Powers/DevourLifePower.cs), [Time's Up](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/TimesUp.cs).

Simple cards are not a defect. Reliable attacks and defensive commons make the interesting combinations playable. The problem is spending too many slots on the same decision, while putting the distinctive decisions behind rare-card combinations.

## 3. EXP and Level: the best discovery, and the pacing problem

### Vigor is much more interesting than the document acknowledges

The installed Vigor implementation adds its amount during an attack and removes the stored amount afterward. With a multi-hit attack implemented as one attack command, it benefits **every hit**. Twin Blades and Combo Rush use exactly that structure. Level-Up Vigor is therefore not simply “two more damage.” [Sources: Vigor](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Powers/VigorPower.cs), [Twin Blades implementation](../IsekaiHeroCode/Cards/TwinBlades.cs), [Combo Rush implementation](../IsekaiHeroCode/Cards/ComboRush.cs).

Illustrative unupgraded outputs, without Strength, Weak, Vulnerable, or other modifiers:

| Attack | No Vigor | With 2 Vigor | Extra damage from one Level Up |
|---|---:|---:|---:|
| Strike | 6 | 8 | 2 |
| Twin Blades, below Level 4 | 6 | 10 | 4 |
| Twin Blades, Level 4+ | 9 | 15 | 6 |
| Skill Chain | 15 | 21 | 6 |
| Starburst Stream, X = 3 | 20 | 30 | 10 |

That creates an excellent decision: **which attack gets the breakthrough?** It also gives Level generation immediate tactical value even before a high-Level payoff arrives. Preserve this interaction and make it legible in previews.

For example, start at Level 3 with 2/4 EXP and play Study the System. It reaches Level 4 and grants 2 Vigor; Twin Blades now deals `(3 + 2) × 3 = 15`, rather than its previous 6. Spending that Vigor on a Strike first materially changes the turn. This is a constructed rules example, not a recorded playtest.

The corresponding danger is that multi-hit attacks become the overwhelmingly correct Vigor consumers, making single-hit finishers feel wrong. Compare complete turns, not just each attack's printed damage.

### The starter does not automatically deliver the advertised curve

With The System's 2 starting EXP, no kills, no upgrades, and only the starter Grind generating EXP:

| Grind plays completed | Total EXP including the starting 2 | Level |
|---|---:|---:|
| 0 | 2 | 1 |
| 1 | 4 | 2 |
| 2 | 6 | 2 |
| 3 | 8 | 3 |
| 5 | 12 | 4 |

The opening five cards contain Grind only 50% of the time in an unmodified ten-card deck. A one-enemy fight gives no kill EXP until that enemy dies. The reward can appear on screen but cannot help win an already-finished fight.

This does not prove Act 1 is too hard; reward cards and encounter patterns matter. It does show that “hallways end at Level 3–4” is not a sufficient pacing specification. Record **Level before the decisive attack**, turns to Level 2/3/4, and how often the player benefits from a Level Up before lethal.

Also, Level is not inherently “monstrous on turn 7.” It only scales cards that read Level and associated triggers. Without those, nine Level Ups supply a finite 18 Vigor, whose value depends heavily on which attacks consume it.

### Preserve milestones, add reasons to care about the next one

Most static Level thresholds eventually stop being decisions. Once Level 4 is reached, that condition is solved for the rest of combat. Put some existing payoff slots into **“you Leveled Up this turn”** and **“the next time you Level Up”** effects. Both fit the existing mechanic vocabulary.

Do not make every card demand perfect timing. Keep dependable cards alongside these spikes. The aim is a visible contrast between a safe turn and a prepared breakthrough turn.

At Level 10, give EXP cards an intentional role or allow them to become expendable. A capstone that keeps generating EXP with no relevant payoff should not be presented as permanently useful. Define separately whether overflow EXP counts for Grand Finale and whether opening the cap processes banked EXP.

## 4. Exploit: an excellent limited resource, a dangerous permanent switch

Limited Exploit stacks create useful ordering decisions: satisfy easy conditions naturally and save the override for something expensive to arrange. That is distinctive enough to build around.

But automatic consumption is not automatically free of frustration. A player may need Shield Bash now and spend a stack on three Block when they wanted it for Hero's Judgment. “Only consumed when unmet” prevents one kind of waste, not unwanted allocation. Clearly preview consumption. Do not add a confirmation dialog to every card; first test whether ordering plus previews provides enough control.

The value of a stack varies dramatically:

| Unmet clause forced by one stack | Printed marginal benefit |
|---|---|
| Shield Bash | 3 Block |
| Underdog Spirit | 5 damage |
| Steal Technique | 2 EXP |
| Megiddo | 9 damage to every enemy |
| Objective Cleared | Draw 2 |
| Hero's Judgment | 16 extra damage |

Paying one Energy for Game Knowledge and then one for an early Underdog Spirit produces ten damage across two cards, with Game Knowledge replacing itself. The late payoff, the hand situation, and other triggers must justify that expense. There is no defensible universal “half an Energy per Exploit” price.

**OP Protagonist currently removes too much of the game.** It makes Level thresholds, Job requirements, Quest checks, and other conditions irrelevant. It also stops natural demand for Exploit stacks; Mana Sense, which triggers on *consumption*, then stops benefiting from those cards. This is a real internal conflict, not the harmonious capstone triangle described in the plan.

A powerful rare may legitimately obsolete an earlier tool, but this one can obsolete a whole playstyle. Prototype a bounded version: **the first two unmet Exploit clauses each turn are treated as met**. It remains a dramatic cheat while leaving later conditions and stacks relevant. Retune cost after testing.

**Protagonist Privilege needs a precise rules contract.** Repeating “deal 32 instead” could mean a second 32-damage attack or a second application of a replacement, which are radically different. Prefer an explicit bonus model: a 16-damage base plus a 16-damage bonus, with Privilege repeating only the defined bonus. Specify whether added hits share Vigor and whether repeated effects emit condition-met events. They should not recursively trigger Fast Learner or Privilege themselves.

System Menu's idea is good, but its two-Energy investment in one card is difficult to recoup when many conditions become easy naturally. Test it at one Energy before adding more words. It needs repeatable targets that remain meaningfully difficult to enable.

## 5. Quests: strongest originality, largest unresolved design burden

Combat objectives could be this mod's most distinctive feature. The current rewards, however, mostly turn objectives into different denominations of EXP. Players may repeatedly choose whichever objective completes incidentally.

### A hand slot is not automatically a substantial cost

A generated Quest does not necessarily replace a normal draw. It occupies space, and the native hand cap is ten, but five drawn cards plus one generated Quest still leave room. If the Quest completes immediately, it may have imposed almost no ongoing hand cost. Conversely, several retained Quests can eventually suppress draws and cause frustrating lockups. [Source: hand cap](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Entities.Cards/CardPile.cs).

Measure how many turns a Quest remains unresolved and how many useful draws it actually prevents. Price immediate completions accordingly. The generator's Energy/card cost and the objective's opportunity cost are often more important than the nominal slot.

### Token-by-token review

| Quest | Fun/balance finding | Recommended change |
|---|---|---|
| Slay | Good target-order puzzle in groups; its draw and EXP often arrive too late against a lone enemy. | Offer it selectively or lower its single-target objective to a damage milestone. Clarify player kill versus any enemy death in co-op. |
| Guard Duty | A healthy objective when 12 Block requires a real sequencing choice; later it becomes routine. | Keep. Track actual completion difficulty by act and do not raise every objective just to preserve difficulty. |
| Combo Chain | Four plays is often routine for zero-cost filtering decks. Its Energy reward helps fund further completions. | Keep the concept; test feedback with Quest Log and repeatable generators. |
| Flawless | “End your turn having taken no unblocked damage” usually ignores the enemy attack that follows. Repeatable healing encourages stalling. | Check survival of an **enemy turn**, with progress beginning when acquired. Replace repeatable healing with a combat-only reward unless generation is bounded. |
| Critical Blow | Excellent match for Vigor planning early; trivial for a mature Growth Slash or big attack. | Keep as an accessible objective, but don't let the same already-resolved hit complete newly generated copies retroactively. |
| Spellcaster | Three Skills can demand sacrificing offense; useful contrast with Critical Blow. | Keep; verify whether the generator counts and define the progress window. |
| Hoarder | Potentially interesting “don't spend everything” challenge. End-turn draw and normal discard order make its reward ambiguous. | Define the checkpoint before discard; give draw at the next turn start if that is the intended benefit. Specify whether Quests count toward six cards. |
| Boss Slayer | Three Level Ups normally arrive when the fight ends. “Main Quest effects” have no identified generator in the proposed pool. | Replace kill with an in-combat milestone, such as personally dealing a specified amount of damage to an Elite/Boss after acquisition. Name its generator. |

Do not allow Quests created after an objective event to claim that event retroactively. Decide whether multiple *already-held* Quests can share one future action. Shared completion can be a satisfying prepared combo; unrestricted retroactive completion can become a reward machine.

Use a small, curated offer pool initially. “Choose one of three from an ever-growing pool” does not guarantee variety: adding unsuitable objectives dilutes access to good ones, while adding easy objectives can make selection predictable. Guild Master+ also risks becoming a mandatory menu every turn. Track time spent choosing, not just combat power.

### Give a Quest two legitimate uses

Perfect Preparation already hints at a promising choice: hold an objective for defense or complete it for tempo. Training Arc can create a third use—abandon it for progress—but its compulsory exhaust must be clear and must not accidentally count as completion.

A strong Quest deck should sometimes prefer a different objective **because it wants a different kind of turn**, not merely because that objective awards more EXP. Keep some EXP rewards, but distribute draw, defense, and immediate burst deliberately. Prototype these differences through existing slots rather than adding another subsystem.

## 6. Important card comparisons and impact gaps

Cross-class comparisons identify questions, not automatic equality requirements. A weaker standalone card can be correct inside a stronger engine. These examples nevertheless expose substantial gaps.

| Proposed card | Verified comparator or internal comparison | Assessment |
|---|---|---|
| Farm the Field: 2 Energy, 4 AoE, 2 EXP | Sweeping Beam: 1 Energy, 6 AoE, draw 1. Dagger Spray: 1 Energy, 4 AoE twice. | Very likely disappointing early. Two EXP is not reliably worth the lost tempo. First prototype: keep its current effect at 1 Energy. |
| Twin Blades: 3×2, or 3×3 at Level 4 | Twin Strike: 5×2, upgraded 7×2. | Weak raw damage, but third-hit Vigor scaling gives a genuine niche. Do not buff without testing that interaction. |
| Shield Bash: 6 damage; existing Block enables 3 Block | Iron Wave: 5 damage and 5 Block unconditionally. | An extra condition for a worse defensive contribution. Needs a clearer advantage or a more interesting counterattack role. |
| Daily Training: 5 Block, 1 EXP | Shrug It Off: 8 Block, draw 1. | EXP is priced aggressively. Test 6 Block/2 EXP as a candidate, with threshold timing measured. |
| Power-Up Montage: 2 Energy, 4 EXP, 4 Block | It uses most of a normal turn to supply one Level Up and less than a Defend's Block. | The immediate rider is too small to establish good tempo. Test 1 Energy with the existing numbers before expanding the effect. |
| Map Hack+: 0 Energy, draw 3/discard 1 | Prepared+: 0 Energy, draw 2/discard 2. | Very efficient. Net hand gain after playing Map Hack+ is +1. Watch it becoming an automatic upgrade and draft across all builds. |
| Save Scum: 1 Energy, replace remaining hand | Calculated Gamble: 0 Energy, Exhaust; its local upgrade adds Retain, not removal of Exhaust. | Recurrence differentiates it, but it needs a purpose beyond “slower Gamble.” Quest abandonment/replanning could supply one. |
| System Menu: 2 Energy, one card gets Override | Game Knowledge/Genre Savvy supply flexible cheats; natural Level conditions eventually switch on anyway. | Weak expected return unless its target is central and repeatedly used. Cost reduction is the simplest test. |
| Hero's Judgment: 2 Energy, 16/32 at Level 7 | Anti-Boss Art: 2 Energy, 20/30 immediately against an Elite/Boss. Growth Slash at Level 7: 21 for 1 Energy. | The earned payoff lacks separation. Hero's Judgment should be a milestone burst, not another conditional large hit. |
| Grand Finale: 3 Energy, 10 + total EXP, Exhaust | At 12 EXP: 22; at 24: 34; at 36: 46. Local Bludgeon: 3 Energy, 32, Uncommon. | Often disappointing until late, although it scales beyond 36 if total EXP keeps counting. Rename it and give it a clearer payoff window. |
| EXPLOSION!: 3 Energy, 28 AoE, no Attacks next turn | I Am Atomic: 3 Energy, 36 AoE, discounts itself, Exhaust. | Repeatability distinguishes Explosion, but its theatrical downside may exceed its advantage. Test against real nonlethal turns. |
| Starburst Stream: 4×(X+2) | Skewer: 8×X; upgraded 11×X. | At X=3, raw 20 versus 24. At X=0, Stream still hits twice. Vigor can reverse the comparison; this is a promising distinct identity. |

Sources for additional comparators: [Sweeping Beam](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/SweepingBeam.cs), [Dagger Spray](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/DaggerSpray.cs), [Iron Wave](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/IronWave.cs), [Prepared](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/Prepared.cs), [Bludgeon](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/Bludgeon.cs), [Skewer](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/Skewer.cs).

Atomic is already a credible signature rare. Three met conditions make it free; upgraded, its Vulnerable can make its unmodified 36 become 54 against a susceptible target. That is exciting enough. The question is whether the player meaningfully prepared it or merely played three routine cards. Test how frequently it is free before its first draw and whether it overshadows the rest of the rare attacks.

There is also a **setup-versus-acceleration imbalance** worth testing. Many proposed Powers and setup Skills cost two Energy, while reliable immediate Energy generation is relatively narrow or conditional. Base-game Adrenaline, Offering, and Big Bang create dramatic turns by making other cards playable: respectively, zero-cost draw plus Energy, HP-for-draw-and-Energy, and a compact draw/Energy/Stars/Forge burst. The mod does not need copies of those cards, but it needs equally convincing ways to turn preparation into a playable hand *now*. Last-Hit Bonus and objective rewards are promising answers; verify they function before enemies are already defeated. [Sources: Adrenaline](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/Adrenaline.cs), [Offering](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/Offering.cs), [Big Bang](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Cards/BigBang.cs).

## 7. Balance failures to resolve before fine tuning

### P0: Dodge the Bad End+ has a two-copy loop on the proposed text

Assume A is in hand and B is in discard. Pay one Energy for A, retrieve B, and set B to zero this turn. After A resolves into discard, play B for zero to retrieve A and make it zero. Repeat. No draw engine is necessary.

The loop alone produces unlimited card plays rather than automatic damage, but ordinary play-trigger payoffs can convert it into combat output. Even without one, it is an unintended unbounded action loop. **Keep Exhaust on this retrieval effect at both upgrade levels, or prohibit retrieving another copy of this card.** Exhaust is easier to understand. This is a consequence of the proposed text; Dodge is not implemented in the inspected card directory.

### P0: repetition can reward making the game boring

- **Steal / Degenerate Tactics:** gold on every play rewards extending a solved fight. Attacks eventually kill many targets, so this is not universally infinite, but blocking/regenerating targets and favorable encounters make farming a real design incentive. Limit gold to once per card per combat, or a clearly tracked first reward from each eligible enemy.
- **Reborn Vending Machine:** nine ordinary Level Ups mean up to 45 gold per combat. Removing the cap permits unbounded rewards if the deck can sustain EXP generation. Put a finite per-combat limit on gold regardless of Level-cap effects.
- **Flawless:** repeatable generation plus healing can turn safe turns into a refill station. Fix the timing and remove or bound repeatable healing.
- **Cheat Inventory+:** renewable copying can turn Exhaust healing or Checkpoint into repeatable resources. Exhaust on the copied card does not bound the number of copies. Retain a cost or restriction on the generator; test copying copies and persistent-stat cards explicitly.

The goal is not to ban strong combinations. It is to avoid optimal play requiring five extra minutes of repetitive clicks after danger has passed. Combat-only excess is much easier to permit than repeatable permanent rewards.

### P1: cap removal exposes other systems

Break the Level Cap simultaneously increases Level-scaled attacks, enables more Vigor, grants Strength, and—upgraded—Dexterity. It also amplifies Smartphone, Insignia, Admin Mode, and Vending Machine. Some of that should feel spectacular. A numerical Level cap, however, is **not an anti-infinite mechanism**: cost reduction, card copying, and play-trigger loops operate independently of it.

Define event ordering for multiple Level Ups, death during a trigger chain, draw into further EXP, and acquired Powers during a chain. Also decide whether direct “Level Up” effects count as EXP for Grand Finale. Treat “EXP gained,” “Level increased,” “condition satisfied,” and “condition forced” as separate events.

### P1: sustain can erase the intended weakness

Healing Circle, Goddess's Blessing, Slow Life, Checkpoint, Return by Death, and Flawless together offer substantial recovery or death protection. Their costs differ, and some will not appear together, but the class cannot be assumed fragile simply because it has 70 HP and slow setup. Measure healing per run and deliberate Checkpoint activations. A death save that is best used as planned healing plus EXP can undermine the stated no-self-harm identity.

## 8. Relics: every proposed item reviewed

Relics should sometimes change card evaluations or route decisions. Most of these currently reward doing the same Level plan more efficiently.

| Relic | Verdict | Improvement / test |
|---|---|---|
| The System | Good identity anchor, inconsistent immediate help. Its final-enemy EXP often has no combat value. | Keep provisionally. Measure first useful Level Up in solo-enemy fights before changing start EXP. |
| Admin Mode | A real upgrade: six starting EXP immediately reach Level 2; Level-Up draw compounds engines. | Define whether that initial Level Up draws and whether minions remain excluded. Test draw/EXP chains and hand overflow. |
| Beginner's Luck Charm | Clean, modest, understandable. Good common relic. | Preview which unmet clause will consume it; don't promise it cannot feel wasted. |
| OP Smartphone | Punchy feedback; up to 45 triggered damage before the normal cap. Random targeting can spoil Fatal setups. | Keep the tension if intentional. Test kill ownership, death-trigger chains, and whether it steals the very kills a Fatal deck needs. |
| Quest Board | Supports the most distinctive package but a random objective can be a burden. | Curate combat-appropriate objectives or offer a quick choice. Do not make it replace an opening draw. |
| Forbidden Walkthrough | Strong turn-one consistency, but obsolete in some permanent-Override decks. | Keep as a reliable relic; measure wasted stacks. It need not be equally good in every build. |
| Hero's Insignia | Nine Strength before cap removal is substantial, especially with multi-hit attacks. | Compare time-to-Strength with local Shuriken's three-Attack triggers, not merely both being rare. Test alongside Break the Level Cap. |
| Return by Death | Good thematic adaptation. At 70 max HP, 30% is 21 versus local Lizard Tail's 35, exchanged for EXP and Exploit. | A legitimate tradeoff. Verify it survives follow-up hits and doesn't misleadingly promise a safe recovery. |
| Reborn Vending Machine | Fun theme, unhealthy incentive as written. | Bound gold per combat. Prototype rewarding the first few breakthroughs rather than every possible Level Up. |

Sources: [Shuriken](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Relics/Shuriken.cs), [Lizard Tail](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Relics/LizardTail.cs). As an economy contrast, local [Maw Bank](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Relics/MawBank.cs) rewards entering rooms and disables after spending at a merchant: it creates a route/spending consideration rather than rewarding extra combat turns.

One future relic replacement could reward **Quest abandonment or a naturally fulfilled difficult clause**, opening a different draft preference. Replace an overlapping slot before increasing the relic count.

## 9. Potions: every proposed item reviewed

Potions should offer a decision on a dangerous turn. Their value must be evident before drinking them.

| Potion | Assessment | Recommendation |
|---|---|---|
| Jar of Slime: 6 EXP | One or two Level Ups depending on current progress, unless near cap. Can unlock a threshold or power a multi-hit burst immediately. | Keep. Preview resulting Level, Vigor, and cap waste. Evaluate against local Energy Potion's flexible two Energy, not a fixed EXP exchange rate. |
| Bottled Cheat Code: 3 Exploit | Potentially excellent emergency access, especially to expensive unmet clauses. Poor when clauses are already natural or permanently overridden. | Keep as a specialist potion. Clearly indicate eligible cards and stack persistence. |
| Truck Summoning Ritual: 25 random damage, Fatal grants two Levels | Random targeting undermines deliberate Fatal play; against the last enemy the reward is usually irrelevant. Local common Fire Potion offers 20 **targeted** damage. | Change to a chosen target; keep the joke in animation. Consider one Level Up before impact, plus another on a valid Fatal, so a nonlethal use has identity. Treat numbers as a prototype. |

Sources: [Fire Potion](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Potions/FirePotion.cs), [Energy Potion](../Scratch/ResearchBalance/sts2/MegaCrit.Sts2.Core.Models.Potions/EnergyPotion.cs).

Rarity need not mean strict superiority. But the rare potion should deliver a memorable decision more often than it delivers an uncontrollable miss followed by no reward.

## 10. Make the advertised builds play differently

| Proposed run | Current structural weakness | Distinct experience to target |
|---|---|---|
| Level Rush | Many generators feed a universal scalar; payoff often becomes simply the highest-rate attack. | Engineer a Level Up on the right turn and bank the right attack for its Vigor. |
| Cheat Engine | The most complete version stops caring about conditions or stacks. | Carefully allocate a limited supply of bypasses; gain separate value from naturally fulfilling conditions. |
| Quest Completionist | Most objectives converge on EXP; automatic completion may dominate deliberate objective play. | Choose between holding, completing, and abandoning objectives to solve different combat problems. |
| Fatal Sweeper | Kill-dependent setup is least useful against isolated elites/bosses; final-kill rewards vanish. | A support package that funds the next attack, with a bridge into sustained single-target combat. Do not advertise it as a complete independent boss solution yet. |

Jobs can help distinguish these runs without becoming a fourth pillar. Alchemist encourages debuff sequencing; Spellblade encourages Skill-to-Attack turns. Appraiser currently risks doing nothing: reordering the next three cards immediately before a five-card draw usually changes no cards drawn. If it triggers after the draw, it instead supports later draw effects. Specify timing. Its upgraded bottoming option is much more consequential. A once-per-turn opt-in appraisal or a small filtering change is preferable to a mandatory low-value menu.

The strongest base-game-like replayability test is: **can two successful decks sincerely disagree about taking the same common?** If every successful deck wants the same filtering, EXP density, multi-hit attacks, and cap remover, named archetypes have not solved the problem.

### What should feel fun during an actual turn?

Use these as experience targets, not as claims about existing playtests:

- **Anticipation:** the player sees that two more EXP will unlock an extra hit, then keeps the right attack with Item Box. The next hand matters before it arrives.
- **A clever reversal:** a Quest is inconvenient, but abandoning it prevents damage, or forcing its completion supplies the Energy needed to escape a bad turn. The player solved a problem instead of merely receiving a stat increase.
- **A risky opportunity:** Last-Hit Bonus can fund another play, but taking that kill consumes Vigor intended for a larger attack. Different enemy HP totals should change the answer.
- **An earned spectacle:** Atomic finishes a turn whose preceding condition checks required deliberate choices. Its animation celebrates a plan the player understands.
- **A different next run:** an early relic or Job makes a previously marginal card valuable, rather than just accelerating the same Level plan.

The corresponding anti-fun experiences are waiting for thresholds with poor cards, receiving progress after lethal, selecting the same obvious Quest repeatedly, accidentally spending a cheat on a trivial clause, and farming gold after combat is effectively solved. These deserve their own playtest notes even in a run that wins comfortably.

## 11. Full card audit

This covers all 88 planned slots. “Keep” means a useful role worth testing, not certified balance. Proposed values elsewhere are experiments, not final replacements.

### Basics — 4

| Card | Action and reason |
|---|---|
| Strike | Keep as a removal baseline; no special mechanic needed. |
| Defend | Keep. Its simplicity gives the signature basics room to teach. |
| Grind | Keep. EXP after damage usefully banks Vigor for the next attack; teach that timing. |
| Danger Sense | Keep. Its first threshold gives Level 2 immediate relevance; inspect bad opening hands. |

### Commons — 20

| Card | Action and reason |
|---|---|
| Mob Hunt | Keep. Straightforward Fatal lesson; distinguish it from Killing Blow. |
| Underdog Spirit | Revisit payoff. A delayed ten-damage attack is not inherently exciting compared with reliable attack rewards. |
| Beginner Magic | Keep as accessible sequencing glue. Natural fulfillment should often beat spending a cheat. |
| Shield Bash | Improve its role/rate; conditional defense compares poorly with Iron Wave. |
| Twin Blades | Keep for Vigor interaction; remove its near-duplicate elsewhere. |
| Steal | Bound gold before tuning damage. |
| Farm the Field | Improve early tempo; current cost undermines the multi-enemy identity. |
| Last-Hit Bonus | Keep. Immediate draw/Energy after a kill creates an actual chain turn. |
| Boss Telegraph | Keep. Damage plus responsive defense is readable; upgraded draw is attractive. |
| Tutorial Sword | Reconsider dependency on only three uncommon Job Powers. Give the condition an alternative or ensure its base remains acceptable. |
| Daily Training | Test a stronger rider or defense; one EXP often feels invisible. |
| Study the System | Keep the zero-cost timing tool; assess card-slot cost and weak cap behavior. |
| Job Board | Keep as a signature enabler after objective timing is specified. |
| Game Knowledge | Keep for flexible allocation; price by actual useful activations rather than a fixed stack exchange rate. |
| Seen It Coming | Keep. Immediate protection plus Weak helps earn future turns. |
| Emergency Dodge | Keep. Zero-cost defense lets setup turns remain playable; monitor upgraded free Block in cycling decks. |
| Side Quest | Differentiate from Side Story or combine their roles. Random objective plus one EXP may be poor compensation for hand clutter. |
| Status Appraisal | Strong thematic keeper. Zero-cost selection among three/five can homogenize every deck; measure pick/upgrade dominance. |
| Item Box | Strong keeper. Clarify whether Retain lasts this turn or all combat; current code applies the keyword directly. |
| Route Guide | Keep. Useful forward planning, but ensure the choice demonstrably changes the next hand and isn't menu work for little return. |

### Uncommons — 36

| Card | Action and reason |
|---|---|
| Growth Slash | Keep as a clear Level payoff. At Level 10 it is 30/40 damage for one Energy; compare against expensive rares. |
| Overkill | Keep cautiously. Eight EXP plus The System's three makes one kill a major acceleration; reward must occur while enemies remain. |
| Objective Cleared | Strong keeper. Completion turns into immediate tempo; protect against automatic Quest loops. |
| Combo Rush | Redesign. It is essentially Twin Blades with larger numbers. Use this slot for a Level-Up timing condition. |
| Raid Opener | Keep. A one-time target opportunity creates sequencing and conflict with chip damage. |
| Duel | Keep as dependable single-target support. It partly patches the Fatal package's boss weakness. |
| Cross-Class Combo | Keep but clarify “card type”: Attacks/Skills/Powers, not character classes. Unplayable Quests do not add a played type. |
| Cleave the Horde | Keep provisionally. Verify eligible kills and EXP timing; don't make Farm the Field simply its bad version. |
| Counter Read | Useful but overlaps Seen It Coming/Boss Telegraph. Keep only if the offensive Weak role earns its slot. |
| Killing Blow | Differentiate from Mob Hunt: a direct Level Up differs mainly in remainder handling/cap interactions. |
| Skill Chain | Keep as a reliable Vigor outlet; plain multi-hit damage is useful supporting material. |
| Monster Grinding | Keep cautiously as the run-long progression card. Define minion eligibility, copy inheritance, and per-original-card persistence. |
| Steal Technique | Mechanically sound glue; the name promises stolen abilities but delivers EXP. Improve flavor accuracy or its distinct action. |
| Read the Code | Rework or merge with Game Knowledge. Pure stacks can consume a turn without a worthwhile target. |
| Guild Reception | Strong keeper: survival now plus an objective to plan around. |
| Cheat Inventory | Preserve copying fantasy; restrict renewable copies of sustain, itself, and persistent rewards. |
| Map Hack | Watch especially closely. Cheap positive hand economy can crowd out more distinctive options. |
| Save Scum | Give it a Quest/replanning role; otherwise its familiarity exceeds its novelty. |
| Barrier Magic | Keep as dependable larger defense. No need for every defensive card to be a new system. |
| Healing Circle | Reassess alongside Slow Life and Goddess's Blessing; three variants of straightforward healing use valuable space. |
| Level Grinding | Rework tempo/cost. Two Energy for six EXP is hard to justify without an immediate threshold or multi-hit payoff. |
| Negotiation | Keep. Weak plus progress fits the class's survival rhythm. |
| Applied Physics | Keep as a flavorful exception. Block bypass is situational; upgraded draw may make it broadly efficient. |
| Side Story | Stronger bridge than Side Quest; test whether one of the two should occupy a different role. |
| Power-Up Montage | Improve immediate usability; four Block is scant compensation for spending two Energy. |
| Death Flag | Keep. A scarce Vulnerable source changes attack valuation; document that Atomic+ also supplies Vulnerable. |
| Dodge the Bad End | Fix upgraded recursion before implementation. Add Exhaust at both upgrade levels. |
| Genre Savvy | Keep as a gradual cheat source; assess overlap with OP Protagonist. |
| Quest Log | Keep as an engine, with careful same-turn generation/completion limits and event rules. |
| Training Arc | Keep the abandonment/thinning idea. Clarify mandatory exhaust, turn timing, empty hand behavior, and no completion credit for mere Exhaust. |
| Fast Learner | Keep as a bridge. Repeated clauses must not create additional condition-met events. |
| Job: Alchemist | Keep. Debuffs change which attack should follow; clarify target-specific damage bonus duration. |
| Job: Spellblade | Keep. Strong Skill-to-Attack economy; define how duplicate Powers stack and what “once each turn” governs. |
| Job: Appraiser | Rework timing/value. Sorting cards all about to be drawn is largely cosmetic. |
| Skill Tree | Keep provisionally. Additional Vigor is much more powerful on multi-hit cards than the wording's modest number suggests. |
| Mana Sense | Interesting specialist; permanent overrides and naturally met thresholds reduce its activations. Make this tension explicit. |

### Rares — 26

| Card | Action and reason |
|---|---|
| EXPLOSION! | Keep the overcommitment fantasy; give it a credible role beside Atomic and a useful non-Attack recovery turn. |
| I Am Atomic | Signature keeper. Monitor how routinely it becomes zero-cost and how much its upgrade swings damage. |
| Starburst Stream | Signature keeper for Vigor. Test X=0, large X, and externally increased X values. |
| Degenerate Tactics | Bound gold. Damage/Weak/gold is useful but not especially rare-worthy as a decision; reconsider rarity or role. |
| Hero's Judgment | Redesign around a breakthrough window. Current delayed output competes poorly with Growth Slash and Anti-Boss Art. |
| Anti-Boss Art | Useful reliability, low agency: encounter category is not an achievement. Keep only if it earns a rare slot beyond raw rate. |
| Ultimate Skill: Sage | Sound damage-plus-cheats bridge. Test if two Energy leaves too little room to use its stacks that turn. |
| Megiddo | Keep. Power-before-attack sequencing gives a real setup opportunity; clarify bonus hit/Vigor behavior. |
| Grand Finale | Rename: already an iconic Silent card. Redesign or retune its expensive EXP-total payoff. |
| Sequence Break | Signature keeper if hard objectives remain worth cheating. Clarify it forces completion, not merely Exhaust. |
| Checkpoint | Thematic but risky sustain. Decide whether intentional death is acceptable and whether repeated copies can stack saves. |
| System Menu | Signature concept, questionable current rate. Test lower cost and important repeated targets. |
| Goddess's Blessing | Useful recovery but overlaps other heals; monitor run sustain and copying. |
| Perfect Preparation | Keep. Makes retaining a Quest potentially desirable rather than universally bad. |
| Reincarnate | Immediate breakthrough burst can work; ensure two Levels are valuable enough for two Energy and an Exhausting rare. |
| Party Formation | Strong flexible card. Upgrade removes its interesting choice; consider strengthening chosen modes instead. |
| Full Clear | Spectacular only with multiple held objectives. At three Energy it can be awkward or unnecessary if objectives already finish incidentally. Prototype after Quest behavior is proven. |
| Slow Life | Useful bridge from protection to sustain, but modest spectacle for a rare. Reassess rarity against healing density. |
| OP Protagonist | Bound automatic overrides so some decisions survive. |
| Break the Level Cap | Keep the fantasy; audit all downstream triggers and economy rewards before tuning numbers. |
| Guild Master | Keep cautiously. Quest reward multiplication plus repeated generation is a potent engine; upgraded selection can be tedious. |
| Grinding Montage | Keep thematic evolution, but random targeting can feel arbitrary. Monitor upgrade misses, cost-reduction value, and duplicate stacks. |
| Mana Overflow | Solid economic payoff with a delay. Measure first activation and turns needed to repay its two-Energy setup. |
| Plot Armor | Good signature defense. Test single large hits versus multi-attacks; upgraded two activations are a major qualitative change. |
| Legend in the Making | Rework. Two Energy invested in future kills is poor against isolated bosses and often pays after the fight is decided. |
| Protagonist Privilege | Keep only with precise bonus/repeat semantics. It should reward sequencing, not accidentally recurse. |

### Ancient cards — 2

| Card | Action and reason |
|---|---|
| Truck-kun | Great name and kill-chain spectacle; 28 AoE competes with Atomic's larger, discountable attack. Make surviving follow-up targets or the reward sequence meaningful. |
| NEW GAME+ | Nice engine closure, but mostly another generator and weaker at the ordinary cap. Specify overflow behavior and verify it feels worthy of its acquisition source. |

## 12. Recommended redesign experiments

These are alternatives to test, not instructions to implement every proposal simultaneously.

1. **Combo Rush becomes the timing attack.** Retain a modest two-hit base, but make the extra hit depend on leveling this turn instead of merely being Level 4. It immediately differs from Twin Blades and rewards EXP planning.
2. **OP Protagonist forces only the first two unmet clauses per turn.** Keeps the power fantasy and leaves a reason to care about later natural conditions and remaining stacks.
3. **Hero's Judgment becomes a retained breakthrough finisher.** Prototype two Energy, 16 damage plus a substantial bonus if you leveled this turn, with Retain. This separates it from immediate Anti-Boss Art and repeated Growth Slash. Choose the bonus only after Vigor-turn testing.
4. **System Menu costs one Energy.** Start with the simplest change; do not simultaneously add draw, a free play, and copying. See whether permanent modification alone becomes desirable.
5. **Boss Slayer rewards progress during the fight.** A cumulative damage objective after acquisition can pay while the boss is alive. Tie its availability to a named generator, and tune threshold by observed completion time.
6. **Legend in the Making helps before the first kill.** Prototype an immediate modest defensive/EXP rider or replace part of its kill-only effect with a bounded boss-relevant objective. Avoid adding free scaling every turn merely to fill the hole.
7. **One healing slot becomes an objective-management card.** For example, abandon a held Quest for immediate protection and a fresh objective. This creates a contingency plan and reduces sustain duplication.
8. **Reborn Vending Machine pays only for the first few Level Ups per combat.** Display the remaining payouts. The exact cap is a balance variable; finiteness is the design requirement.

Keep the agreed three-Job scope and no Job/Level integration. None of these changes needs a new resource, an expanded keyword library, or additional reward-card slots.

## 13. Playtest plan: measure fun as well as wins

### First: deterministic interaction checks

Before comparative runs, manually test these exact situations:

- Two Dodge the Bad End+ copies cannot loop after the fix.
- A newly generated Quest cannot claim an earlier completed action; two already-held Quests resolve in the documented order.
- Flawless checks the intended enemy-turn window, and Hoarder rewards at a useful, specified time.
- Quest exhaustion and Quest completion are distinct; copies and Full Clear cannot accidentally award rewards twice.
- One Level Up followed by Twin Blades/Skill Chain/Starburst Stream produces the expected Vigor bonus.
- OP Protagonist, Mana Sense, Fast Learner, Privilege, and Atomic agree on natural, forced, consumed, and repeated events.
- Cap overflow, cap removal, direct Level Ups, and total EXP tracking have explicit expected outcomes.
- Gold/healing generation remains bounded under repeatable copying and safe stalls.
- Smartphone does not give contradictory Fatal credit; last-enemy rewards and co-op kills behave as documented.
- Appraiser and Training Arc trigger at their intended point relative to the normal draw.

### Second: short exploratory runs

Start with six Act 1 runs across solo enemies, groups, and elites. Include runs without early rare payoffs. These diagnose pacing and obvious dead offers; they do not establish balance.

Then run at least twelve full runs per candidate revision, split between low Ascension and an experienced tester's usual difficulty. Keep a base-game comparison log under similar difficulty and player familiarity. Reusing seeds can help reproducibility, but different character pools mean these are not perfectly controlled identical runs. Do not interpret a dozen wins/losses as a precise balance estimate.

Avoid forcing a preselected archetype in every run. Record what rewards actually supported; a design that requires a rare to make its commons worthwhile is an important failure signal.

### Log these fields

| Measure | What it reveals |
|---|---|
| Turn of first useful Level Up; Level on turns 1–3 and before lethal | Whether the fantasy arrives while it matters |
| EXP gained after final lethal; Level at combat end | How misleading the current end-Level target is |
| Vigor damage realized and attack chosen | Whether breakthrough timing creates decisions |
| Exploit stacks gained, consumed, wasted; natural versus forced clauses | Whether the cheat economy is worth drafting |
| Quest selected; turns held; reward realized; useful draws blocked | Whether objective difficulty and slot costs are real |
| Time in selection menus; repeated selections of the same objective | Whether apparent depth is actually busywork |
| Healing/gold earned; turns deliberately added to obtain them | Whether optimal play rewards stalling |
| Picks, skips, upgrades, and removals with the deck's current needs | Which cards are automatic or never justified |
| Fatal rewards that helped a surviving target versus arrived after combat | Whether the kill package carries its advertised role |
| One “I felt clever” turn and one frustrating turn, with the alternative considered | Whether choices, rather than only large numbers, produce fun |

Ask testers after each run: “What changed how you evaluated a card?”, “When did you change your plan?”, “Which reward made the run feel different?”, and “Did you ever keep playing after you knew you had won?” These produce more useful evidence than a single fun score.

### Acceptance criteria for the next design pass

- At least three successful deck shapes demonstrate different draft priorities and turn patterns; they need not align exactly with the four advertised labels.
- Early rewards can solve immediate survival problems without waiting for a rare engine.
- Some strong turns require a visible choice about Level timing, Quest handling, or stack allocation.
- No repeatable permanent reward makes deliberate stalling the expected optimal routine.
- No two-copy retrieval loop; any intentional infinite requires a deliberate design decision and documented constraints.
- At least two memorable payoffs work without Break the Level Cap or Hero's Insignia.
- Players can explain what their cheat spent and why their Quest completed without opening external documentation.

## 14. Implementation and documentation cautions

The current alpha is not the full proposal. README describes 51 cards; the inspected code still contains the old common Attack version of Training Arc, while the plan makes it an uncommon Power. Therefore observations from the current alpha cannot validate the planned Quest package or full pool. [Sources: README](../README.md), [Training Arc implementation](../IsekaiHeroCode/Cards/TrainingArc.cs).

Specific documentation repairs to make alongside balance work:

- Replace “all classes obey this template” with a chosen pool structure.
- Replace a single EXP/Energy or Exploit/Energy exchange rate with early-turn, threshold, and engine-context evaluations.
- Correct “Death Flag is our one Vulnerable card”: Atomic+ also applies it.
- Give Boss Slayer an actual generator and specify all Quest timing, failure, copying, and abandonment rules.
- Make Item Box's Retain duration explicit. Applying a keyword directly is broader than merely retaining a card once.
- Clarify Appraiser's timing and Grinding Montage's upgrade-versus-cost-reduction behavior.
- Stop describing capstone compatibility and cross-pool safety as established facts before those interactions are tested.

**Order of work:** fix unbounded loops and reward incentives; finalize Quest/Exploit semantics; repair early tempo and overlapping slots; test three genuinely different successful deck shapes; then tune numbers across the full pool.

The core is worth developing. Its strongest asset is not the anime references or the eventual damage ceiling: it is the possibility of making the player arrange a breakthrough that would not have happened without their decisions. Design the commons, objectives, and capstones to preserve that feeling all the way through the run.
