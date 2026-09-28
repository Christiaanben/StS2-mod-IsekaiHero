# IsekaiHero — Character Design Document

> **Mod:** IsekaiHero · **Game:** Slay the Spire 2 (Early Access) · **Stack:** C# / [BaseLib-StS2](https://github.com/Alchyr/BaseLib-StS2) · Steam Workshop since game v0.107.1
> **Character:** Isekai Hero (`ISEKAIHERO-ISEKAI_HERO`)
> **Status:** Design v3.1 — revised 2026-09-28 for shorter card text and simpler effects, following the [fun and balance review](IsekaiHero_Fun_And_Balance_Review.md). This file is the **single source of truth** for implementation. New values are prototype targets, not measured balance results.
> **Tracking:** checked active entries already exist; unchecked active entries need implementation. Struck-through checked entries preserve superseded implemented versions and do **not** count toward the 88 active card slots. Implement the unchecked replacement immediately below each such entry. This revision changes the plan, not game code.

---

## 1. Elevator pitch

An ordinary office worker gets hit by a truck and wakes up at the foot of the Spire with one gift: **they can see the world's game interface.** Status screens, EXP bars, quest prompts — and the knowledge of how to abuse all of it.

**You start every combat at Level 1 and end it as the overpowered protagonist.** That is the isekai power fantasy, compressed into a single fight: grind, level up, snowball, then delete the boss with a technique that has a chant longer than the enemy's remaining lifespan.

The alpha's North Star still governs everything: *the hero becomes powerful by understanding systems faster than the Spire expects, turning that advantage into dramatic spikes of strength.* The player should feel **clever first, strong second**.

---

## 2. Design commitments and comparison baseline

The following is **our chosen scope**, not a universal base-game template. The review checked the installed game DLL; its numerical comparisons are version-specific. Strike/Defend are removal baselines, not the standard for drafted reward cards.

| Slot | Planned scope |
|---|---|
| Cards | 4 Basic + 20 Common + 36 Uncommon + 26 Rare + 2 Ancient = 88 active designs |
| Starter deck | 4 Strike + 4 Defend + Grind + Danger Sense |
| Relics | 1 Starter + 1 upgraded Starter + 1 Common + 2 Uncommon + 3 Rare + 1 Shop = 9 |
| Potions | 3, one per rarity |
| Mechanics | EXP/Level + Exploit + combat Quests; Jobs remain three supporting Powers |
| First-turn identity | The System grants 2 EXP; Grind can immediately reach Level 2 |

**Fun requirements:**

1. **Prepare a breakthrough.** Level-Up Vigor benefits every hit of a multi-hit attack. Retention, EXP timing, and the choice of Vigor consumer must create different good turns.
2. **Keep decisions after setup.** Limited Exploit bypasses help solve a turn; no global Power permanently makes every condition irrelevant.
3. **Give objectives different uses.** Complete, hold, or abandon a Quest for different benefits. Rewards vary between EXP, draw, defense, and Energy.
4. **Pay for setup honestly.** Judge an effect's whole turn, card-slot cost, threshold timing, and eventual return. A small rider does not automatically make expensive setup playable. Pure setup is allowed when its burst window justifies it.
5. **Avoid rewarded busywork.** Gold and healing cannot be farmed without limit. Quest menus must offer meaningful choices, not mandatory repetitive clicks.
6. **Allow different drafts.** At least three successful deck shapes must disagree about useful cards and upgrades. Fatal is a support package until single-target tests establish more.
7. **Preserve clear staples.** Not every card needs a novel effect; remove near-duplicates before adding complexity or more slots.

**Playstyle:** defend or apply Weak, engineer a Level Up, then exploit the opening. Death Flag and Atomic+ supply limited Vulnerable. The character pays tempo and sometimes hand space for growth; intentional death is not the intended setup engine.

**Distinct identity:** Levels persist within combat, while their Vigor is allocated to a particular attack; limited cheats bypass selected rules; Quests create objectives the player can manipulate. This combination, rather than a claim that other characters have only one engine, distinguishes the hero. Jobs do not gain Level scaling.

### 2.1 Merging the alpha — old pillars → new pillars

The goal doc's three pillars survive; they just stop being three separate systems and become **roles inside one triangle**:

| Old pillar (goal doc) | Where it lives now |
|---|---|
| **Jobs** — "What class did I get?" | A **cycle of 3 uncommon `Job:` Powers** (Alchemist, Spellblade, Appraiser) plus supporting conditions such as Tutorial Sword's. Jobs create draft preferences without a fourth mechanic; expansion is deferred. Implementation status is tracked in §5. |
| **Exploits** — "How do I break the systems?" | The **formal Exploit clause/buff** (§3.2) plus knowledge/manipulation cards: Status Appraisal, Route Guide, Map Hack, Save Scum, Applied Physics, and Dodge the Bad End. The existing condition hooks are the starting point for the v3 event contracts. |
| **Cheat Skills** — "When does the protagonist moment happen?" | The **rare payoff suite**: I Am Atomic, Megiddo, System Menu, Grinding Montage, EXPLOSION!, Season Finale, OP Protagonist, and Protagonist Privilege. Acquisition, setup, timing, or limited uses must justify their power. Implementation status is tracked in §5. |

**Design filters** (kept from the goal doc — ask these of every new card):

1. Does this feel like an isekai trope instead of a normal fantasy hero card?
2. Does it support EXP/Level, Exploit, Quests, a Job, or a strong flavor exception?
3. Does the player feel smart or powerful for using it?
4. Would the same mechanic make more sense on an existing STS2 class?
5. Is the power fantasy earned through choices, risk, setup, or deckbuilding?

**Acceptance checks** (adapted): future card sets should support runs that feel like — **Level Run** (grind and snowball), **Cheat Run** (allocate bypasses and fulfill other conditions naturally), **Quest Run** (objectives into engines), **Fatal support** (kill-sequencing with a single-target plan), and hybrids of any two.

---

## 3. The three mechanics

### 3.1 EXP & Level — persistent progress, timed breakthroughs

- Start each combat at **Level 1**, 0 EXP, before starting-relic effects. Every **4 EXP** grants one Level Up; unspent progress rolls over. Normal cap: **Level 10**.
- Every actual Level Up grants **2 Vigor**. It increases each hit of the next eligible attack command, then is consumed. Multiple hits are one command; a later separate attack does not reuse the consumed Vigor. Preview actual damage.
- Cards check current Level, scale with Level, or reward **leveling this turn**. These are deliberately different roles.
- At the cap, EXP remains banked but grants no further Level Ups or Vigor. Maintain a separate **total EXP gained this combat** counter, including starting EXP and overflow, for Season Finale. Direct Level Ups do not add to this total and preserve the EXP remainder.
- A direct Level Up cannot exceed the normal cap unless it has been removed. Break the Level Cap first installs its rewards, then removes the cap, then processes banked EXP in four-point increments. Each actual increase fires one Level-Up event; no retroactive rewards for earlier increases.
- The System grants kill EXP for actual non-minion enemy deaths, regardless of who killed them. Card Fatal rewards require that card's eligible kill; repeated death-prevention notifications do not count. Once combat ends, do not begin further draw/damage/Level-trigger chains.
- Process a Level Up, its Vigor, and queued reactions in deterministic order before the next player action. A death inside a reaction can queue further EXP; it cannot re-credit the same death.

**Pacing targets to test:** record first useful Level Up, Level on turns 1–3, and Level **before lethal**. Hallway Level 3–4, elite 5–6, and dedicated boss 8–10 are provisional developed-deck expectations, not promises for the starter. With no kills, starting EXP plus Grind reaches Level 2 after one Grind, Level 3 after three, and Level 4 after five. Do not tune using post-lethal progress.

**Persistence:** all combat progress resets. Permanent growth comes from deck upgrades, relics, and the explicitly bounded Monster Grinding card. Level alone is not automatic damage or defense; cards and Vigor realize its value.

**Cross-class:** EXP remains a class-agnostic buff. Its multi-hit interaction and zero-cost generators need shared-pool tests; availability does not guarantee safety or usefulness.

### 3.2 Exploit — conditions and limited bypasses

- **Exploit clause:** a base effect followed by a defined conditional bonus. Price the bonus by its actual context, not a fixed Energy-per-stack exchange rate.
- **Exploit buff:** when an unmet condition is forced by a stack, consume one stack for that card play. Never spend on naturally met conditions. The preview shows the reason a clause is active and whether a stack or limited bypass will be spent.
- **Override:** this particular card's clauses count as met for this combat. System Menu grants it. It does not make conditions naturally true.
- **OP Protagonist:** supplies only two free unmet-condition bypasses each turn. Duplicate copies do not increase this allowance. Remaining cards still need natural conditions, Override, or stacks.
- **Resolution precedence:** natural condition → card Override → Beginner's Luck Charm → OP Protagonist allowance → Exploit stack. Spend only the first applicable limited source. Recompute after each completed card play; never consume resources while previewing.
- Emit exactly one **condition-satisfied** event per eligible original card play, whether natural or forced. Track **naturally satisfied**, **forced**, and **stack consumed** separately. Fast Learner and Atomic use satisfied; Mana Sense uses stack consumed.
- **Privilege repeats only the explicit bonus**, never the base, a replacement, or the condition event. Added-hit bonuses increase the same attack command's hit count; flat damage bonuses modify that command. Repeated draw/Block/EXP applies once more without creating another satisfaction event. Distinct later attack commands, such as Megiddo's AoE after its first hit, do not inherit already-consumed Vigor. Limits are reserved before resolving rewards to prevent re-entry.
- Healing and other alternative values use base-plus-bonus wording, not “instead,” so repeating the bonus has one meaning. Actual card replays are separate plays and may trigger events again; effect-only repeats are not replays.
- **Fatal is never an Exploit condition.** No cheat, Override, or forced Quest completion fabricates a kill for Fatal, gold, or permanent-card-growth credit.
- Base-game compatibility is future work: audit and tag individual safe cards rather than trying to flip arbitrary conditions. Do not include permanent-reward or eligibility checks.

**Condition library:** `Level X+` · `an enemy intends to Attack` · `a Quest is in your hand` · `you completed a Quest this turn` · `you Leveled Up this turn` · `first Attack this turn` · `N+ cards played this turn` · `only one enemy remains` · `target at full HP` · `target has a debuff` · `you have Block` · `you played a Skill this turn` · `you played a Power this turn`. Use one condition per card; Tutorial Sword now uses the played-Power condition, not an alternative condition list.

### 3.3 Quests — complete, hold, or abandon

The installed game has a Quest card type with map/event/rest-site uses. These **combat objectives** are a new system using that type, not an existing colorless combat package.

**Rules and timing:**

- Quest tokens are **Unplayable. Retain.** They track only events while held, after acquisition. Counters never claim earlier actions. A generator's own play does not count for a Quest it creates.
- A player may hold at most one Quest of each title. Once a title completes, it cannot be generated again until the start of that player's next turn. All forced completions obey this limit. Different already-held objectives may share a future event.
- Complete once: mark completed and remove from hand, Exhaust it, then grant the reward and emit one completion event. A reward-created Quest cannot claim the event or reward that created it. Snapshot held Quests before Full Clear; resolve in hand order.
- **Abandon** means Exhaust without reward or completion event. Ordinary discard pauses progress; returning to hand resumes stored cumulative progress, but turn-based counters reset each player turn. Copies never inherit objective progress, have no permanent-deck existence, and obey title eligibility.
- Normal generators draw from the seven ordinary titles below. Offers contain distinct eligible titles; show fewer than three if needed, and skip only the generation effect if none qualify. Exclude Slay when fewer than two enemies remain. Boss Slayer is exclusive to Legend in the Making.
- A generated Quest occupies hand space but does not replace the normal draw. At the ten-card hand limit, skip generation with a clear preview; do not hide the Quest in the discard pile. Draw and other card effects still resolve normally.
- Turn-based objectives count only qualifying events after acquisition in that turn. **Hoarder** checks before end-turn discard and counts all held cards, including Quests. Its delayed draw occurs after the next normal draw. **Flawless** observes a full enemy turn beginning after acquisition; it completes at that enemy turn's end if the owner remained alive and lost no HP during that enemy turn. Its delayed Block occurs after next-turn Block removal. Delayed rewards vanish if combat ends.
- Slay counts an eligible non-minion enemy killed by the owner's card or owned effect while the Quest is held. Damage objectives count actual enemy HP lost from the owner's cards/effects, not blocked damage or allies' output. Boss Slayer accumulates against living Elite/Boss enemies only; it does not require lethal.
- All uncompleted Quests vanish at combat end. Completing the final enemy's kill may not produce useful combat rewards; do not promise otherwise.

**Quest tokens — 8, outside the 88-card pool:**

- [ ] **Slay** — Kill a non-minion enemy.<br>Reward: gain 4 EXP. Draw 1 card.
- [ ] **Guard Duty** — Gain 12 Block in one turn.<br>Reward: gain 2 EXP and 8 Block.
- [ ] **Combo Chain** — Play 4 cards in one turn.<br>Reward: gain 2 EXP and 1 Energy.
- [ ] **Flawless** — Survive an enemy turn without losing HP.<br>Reward: gain 2 EXP. Gain 8 Block next turn.
- [ ] **Critical Blow** — Deal 15 unblocked damage in one hit.<br>Reward: gain 4 EXP and 4 Vigor.
- [ ] **Spellcaster** — Play 3 Skills in one turn.<br>Reward: gain 2 EXP. Draw 2 cards.
- [ ] **Hoarder** — End your turn with 6+ cards in hand.<br>Reward: gain 4 EXP. Draw 1 extra card next turn.
- [ ] **Boss Slayer** — Deal 40 unblocked damage to Elites or Bosses.<br>Reward: Level Up twice. Draw 2 cards.

Quest objectives are displayed above a separate **Reward** line, with visible progress. The shared Quest tooltip says: “Unplayable. Retain. Progress counts only while held. Complete the objective to Exhaust this and gain its reward.” Numeric progress goals mean at least that amount. Acquisition windows and offer eligibility follow the shared rules above; they are not repeated on every token.

**Connections:** EXP unlocks thresholds and times Vigor; limited Exploit bypasses support difficult clauses; Quests supply distinct combat rewards; holding one enables Perfect Preparation, completing one enables Objective Cleared, and abandonment enables contingency cards. Plain Exploit stacks do not themselves complete Quests—Sequence Break and Full Clear explicitly do that.

---

## 4. Character sheet

- **Name:** Isekai Hero (implemented id `ISEKAIHERO-ISEKAI_HERO`) · **HP:** 70 ✅ (as implemented — ties Silent) · **Energy:** 3 · **Color:** purple `#6C3082` ✅ (see §13 — possible clash with Necrobinder's palette) · **Energy icon:** a floating menu-cursor diamond
- **Story blurb:** *"Died on a crosswalk. Woke up with a status screen. The Spire's rules are just code — and nobody patched it."* (Tie-in: summoned by a very bored Neow, who is canonically an Ancient in STS2.)
- **Starting relic:** The System (§7) — **replaces the alpha's placeholder Veil of the Unseen** (heal 3 at combat start); its combat-start hook code is reusable.
- **Starter deck (10):** 4× Strike ✅ · 4× Defend ✅ · 1× Grind ✅ · 1× Danger Sense ✅

---

## 5. Card list — 88 cards

Format: `Name — cost · card text (upgrade) · art`. Only the effect before the upgrade/art metadata belongs on the card. `<br>` marks an intended line break. Rules and implementation details belong in §5.6 or the shared mechanics, not in the description.
**Legend:** `[x]` = active effect already implemented; `[ ]` = pending effect. `~~Struck-through~~` checked entries are **historical implemented versions**, immediately followed by the pending replacement; exclude historical entries from all totals. Existing code is evidence of implementation, not full playtest approval. Art is retained when an effect changes. Shared engine/UI tasks remain tracked in §12 even for unchanged cards.

**Numbers:** all changed values are prototype targets. Compare whole turns and actual drafted base-game cards; validate Vigor, rarity, sustain, and draw together. Cards without changed effects stay checked and go on the §9 watchlist instead of being falsely marked for reimplementation.

**Writing standard — one clear idea, then its payoff:**

- Aim for **two short effect sentences** on ordinary cards. One condition and one payoff is enough. A rare may use three short sentences when each is necessary.
- Use direct verbs: **Deal, Gain, Draw, Apply, Choose, Exhaust.** Write “damage” and “cards” where needed; brevity must not become cryptic shorthand.
- Keep familiar keyword lines separate: **Retain. Exhaust.** Do not invent a new keyword to hide a paragraph that only one card needs.
- Keep meaningful costs and restrictions visible. “Does not stack,” “this turn,” and copy eligibility cannot disappear into developer notes if they change a player's decision.
- Shared tooltips explain EXP, Level, Exploit, Override, Quests, and Abandon once. Numerical previews show the actual result. They must not conceal card-specific drawbacks.
- Prefer simple limitations such as Exhaust over per-name counters and lists of exceptions. If shortening the wording still leaves too many decisions to remember, simplify the effect.
- Upgrades normally change a number or cost, not add another rule. Metadata below is the upgrade specification, not a second paragraph printed on the card.
- Checkboxes track **effect implementation**. Wording-only revisions do not erase completed gameplay work; the localization pass in §12 remains pending for the entire pool. Historical struck-through text stays unchanged.

### Implemented cards awaiting revised implementations

**These changes are already written into the card designs below, but have not been applied to the C# implementations.** Each old implemented entry is struck through and followed immediately by its unchecked replacement. The 34 other implemented cards retain their effects deliberately; they are not missing replacements. This table summarizes changes, not additional card slots.

| Card | Existing implementation | Revised design to implement |
|---|---|---|
| Underdog Spirit | 5 damage + 5 conditional | 6 damage + 6 conditional; upgrade becomes 8 + 7 |
| Shield Bash | 6 damage; existing Block enables 3 Block | Always gain 3 Block; having Block before play enables 3 more; upgrade becomes 8 damage and 4 + 4 Block |
| Steal | Repeatable 4/6 Gold | Gain 8/12 Gold, then Exhaust; no shared-name counter |
| Farm the Field | Costs 2 Energy | Costs 1 Energy |
| Tutorial Sword | Bonus requires a Job | Bonus requires **a Power played this turn**; one readable condition |
| Daily Training | 5 Block, 1 EXP | 6 Block, 2 EXP; upgrade becomes 8 Block, 3 EXP |
| Combo Rush | Third hit requires Level 4+ | Third hit requires **leveling this turn**, separating it from Twin Blades |
| Map Hack | Upgrade draws an extra card | Upgrade grants Retain instead; draw stays at 2 |
| Level Grinding | 2 Energy, repeatable | 1 Energy, Exhaust; retains 6/8 EXP |
| Power-Up Montage | Costs 2 Energy | Costs 1 Energy |
| Training Arc | Common multi-hit Attack with conditional Block | Uncommon Power: optionally Exhaust a card after each turn's draw to gain 2/3 EXP |
| EXPLOSION! | 28/36 AoE damage | 36/44 AoE damage; retains the next-turn Attack restriction |
| Degenerate Tactics | Repeatable 8/10 Gold | Gain 12/16 Gold, then Exhaust; no shared-name counter |
| Hero's Judgment | 16 damage, replaced by 32 at Level 7+ | Retain; 16 damage **plus 20 if you leveled this turn**; upgrade becomes 20 + 24 |
| System Menu | Costs 2 Energy | Costs 1 Energy |
| Truck-kun | 2-Energy uncommon; 15/19 AoE; one Fatal payout | 3-Energy Ancient; Level Up before 32/40 AoE; Energy and Level Up for each eligible kill |

The legacy **Return by Death** Skill is separately struck through in §12, with its migration to the pending **Checkpoint** design. Unimplemented cards were revised in place, including OP Protagonist, Dodge the Bad End, the Quest package, and the replacements **Emergency Commission** and **Season Finale**.

### 5.1 Basics (4)

- [x] **Strike** ✅ — 1⚡ · Attack · Deal 6 damage. *(U: 9)* · Art: nervous first swing at a slime (Grimgar vibes)
- [x] **Defend** ✅ — 1⚡ · Skill · Gain 5 Block. *(U: 8)* · Art: arms crossed behind a battered wooden shield
- [x] **Grind** ✅ — 1⚡ · Attack · Deal 6 damage. Gain 2 EXP. *(U: 8 dmg, 3 EXP)* · Art needed: field of low-level slimes at sunrise
- [x] **Danger Sense** ✅ — 1⚡ · Skill · Gain 4 Block. Exploit (Level 2+): gain 4 more. *(U: 5/+5)* · Art needed: hero sidestepping an attack with an ! above their heads

### 5.2 Commons (20 — 10 Attacks / 10 Skills)

*Job: teach the three pillars with simple, honest cards. Heavy on EXP riders and easy Exploit conditions.*

**Attacks**

- [x] **Mob Hunt** — 1⚡ · Deal 8 damage.<br>Fatal: gain 4 EXP. *(U: 11 damage, 6 EXP)* · Art: giant toad hunt (*KonoSuba*)
- [x] ~~**Underdog Spirit** — 1⚡ · Deal 5. Exploit (Level 3+): deal 5 more. *(U: 6/+7)* · Art: child prodigy's wooden-sword drills (*Mushoku Tensei*)~~ **Superseded implemented version.**
- [ ] **Underdog Spirit** — 1⚡ · Deal 6 damage. Exploit (Level 3+): deal 6 more. *(U: 8/+7)* · Art: child prodigy's wooden-sword drills (*Mushoku Tensei*)
- [x] **Beginner Magic** — 1⚡ · Deal 5 damage.<br>Exploit (you played a Skill this turn): deal 4 more. *(U: 6/+6)* · Art: a child mage's first oversized Water Ball (*Mushoku Tensei*)
- [x] ~~**Shield Bash** — 1⚡ · Deal 6. Exploit (you have Block): gain 3 Block. *(U: 8/+4)* · Art: shield-first counterattack (*Shield Hero*)~~ **Superseded implemented version.**
- [ ] **Shield Bash** — 1⚡ · Deal 6 damage.<br>Exploit (you have Block): gain 3 Block.<br>Gain 3 Block. *(U: 8 damage; both Block values become 4)* · Art: shield-first counterattack (*Shield Hero*)
- [x] **Twin Blades** — 1⚡ · Deal 3 twice. Exploit (Level 4+): deal 3 a third time. *(U: 4×)* · Art: crossed cyan/orange blades against The Gleam Eyes (*SAO*)
- [x] ~~**Steal** — 1⚡ · Deal 6. Gain 4 Gold. *(U: 9, 6 Gold)* · Art: a smug green-caped adventurer catching a blue-ribbon coin pouch, with his shocked goddess companion behind (*KonoSuba*)~~ **Superseded implemented version.**
- [ ] **Steal** — 1⚡ · Deal 6 damage. Gain 8 Gold.<br>Exhaust. *(U: 9 damage, 12 Gold)* · Art: a smug green-caped adventurer catching a blue-ribbon coin pouch, with his shocked goddess companion behind (*KonoSuba*)
- [x] ~~**Farm the Field** — 2⚡ · Deal 4 to ALL. Gain 2 EXP. *(U: 6, 3 EXP)* · Art: water arrow splitting into a wave across a monster field (*Tsukimichi*)~~ **Superseded implemented version.**
- [ ] **Farm the Field** — 1⚡ · Deal 4 damage to ALL enemies. Gain 2 EXP. *(U: 6 damage, 3 EXP)* · Art: water arrow splitting into a wave across a monster field (*Tsukimichi*)
- [x] **Last-Hit Bonus** ✅ — 1⚡ · Deal 8 damage.<br>Fatal: draw 1 card and gain 1 Energy. *(U: 11 damage; draw 2)* · Art: Kirito's finishing strike against the goblin boss (*SAO*)
- [x] **Boss Telegraph** ✅ — 1⚡ · Deal 6 damage.<br>Exploit (an enemy intends to Attack): gain 5 Block. *(U: 8 damage; the bonus also draws 1 card)* · Art: Tanya diving through artillery toward a glowing strike zone (*Saga of Tanya the Evil*)
- [x] ~~**Tutorial Sword** ✅ — 1⚡ · Deal 7. Exploit (you have a Job): deal 4 more. *(U: 9/+6)* · Art: Rio's disciplined wooden-sword academy duel (*Spirit Chronicles*)~~ **Superseded implemented version.**
- [ ] **Tutorial Sword** — 1⚡ · Deal 7 damage.<br>Exploit (you played a Power this turn): deal 4 more. *(U: 9/+6)* · Art: Rio's disciplined wooden-sword academy duel (*Spirit Chronicles*)

**Skills**

- [x] ~~**Daily Training** — 1⚡ · Gain 5 Block. Gain 1 EXP. *(U: 7, 2 EXP)* · Art: absurd weighted push-ups before dawn (*Cautious Hero*)~~ **Superseded implemented version.**
- [ ] **Daily Training** — 1⚡ · Gain 6 Block. Gain 2 EXP. *(U: 8 Block, 3 EXP)* · Art: absurd weighted push-ups before dawn (*Cautious Hero*)
- [x] **Study the System** — 0⚡ · Gain 2 EXP. *(U: 3 EXP)* · Art: scrolling through a skill menu mid-dungeon (*So I'm a Spider*)
- [ ] **Job Board** — 1⚡ · Choose 1 of 3 Quests to add to your hand.<br>Draw 1 card. *(U: costs 0)* · Art: corkboard of bounty posters at the guild (*Log Horizon*)
- [x] **Game Knowledge** — 1⚡ · Gain 1 Exploit. Draw 1 card. *(U: 2 Exploit)* · Art: Sora physically breaking the rules of the living-chess match (*No Game No Life*)
- [x] **Seen It Coming** ✅ — 1⚡ · Gain 6 Block. Exploit (an enemy intends to Attack): apply 1 Weak. *(U: 8, 2 Weak)* · Art: Seiya's sidestep begun before the demon's swing starts (*Cautious Hero*)
- [x] **Emergency Dodge** — 0⚡ · Gain 3 Block. Exploit (Level 3+): gain 3 more. *(U: 4/+4)* · Art: Subaru's panicked back-fall beneath Elsa's kukri (*Re:Zero*)
- [ ] **Side Quest** — 0⚡ · Add a random Quest to your hand. Draw 1 card.<br>Exhaust. *(U: choose 1 of 3 Quests instead of random)* · Art: villager with an exclamation mark over their head
- [x] **Status Appraisal** ✅ — 0⚡ · Look at the top 3 cards of your draw pile.<br>Take 1. Discard the rest. *(U: look at 5)* · Art: Great Sage's appraisal window over a suspicious potion (*Tensura*)
- [x] **Item Box** ✅ — 1⚡ · Gain 7 Block.<br>Give a card in your hand Retain this combat. *(U: 10 Block; give up to 2 cards Retain)* · Art: Lloyd drawing a sword from a forbidden-library storage portal (*7th Prince*)
- [x] **Route Guide** ✅ — 1⚡ · Gain 5 Block.<br>Look at the top 4 cards of your draw pile. Keep 1 on top; put the rest on the bottom. *(U: 7 Block; keep up to 2 on top in any order)* · Art: Catarina mapping the branching death and exile routes (*My Next Life as a Villainess*)

### 5.3 Uncommons (36 — 13 Attacks / 14 Skills / 9 Powers)

*Job: the build-arounds and the bridges. This is where archetypes fork: Level-scaling, Exploit engine, Quest engine.*

**Attacks**

- [x] **Growth Slash** — 1⚡ · Deal damage equal to 3× your Level. *(U: 4×)* · Art: sword swing leaving a level-up light trail (*SAO*)
- [x] **Overkill** — 2⚡ · Deal 14 damage.<br>Fatal: gain 8 EXP. *(U: 18 damage, 10 EXP)* · Art: airborne computation-jewel rifle blast obliterating one goblin (*Saga of Tanya the Evil*)
- [ ] **Objective Cleared** — 1⚡ · Deal 9 damage.<br>Exploit (you completed a Quest this turn): draw 2 cards. *(U: 12 damage)* · Art: "QUEST COMPLETE" banner mid-swing
- [x] ~~**Combo Rush** — 1⚡ · Deal 4 twice. Exploit (Level 4+): deal 4 a third time. *(U: 5×)* · Art: Lloyd's gleeful layered spell barrage against Guisarme (*I Was Reincarnated as the 7th Prince* — Guisarme duel)~~ **Superseded implemented version.**
- [ ] **Combo Rush** — 1⚡ · Deal 4 damage twice. Exploit (you Leveled Up this turn): deal 4 damage a third time. *(U: 5 per hit)* · Art: Lloyd's gleeful layered spell barrage against Guisarme (*I Was Reincarnated as the 7th Prince* — Guisarme duel)
- [x] **Raid Opener** — 2⚡ · Deal 15 damage.<br>Exploit (target at full HP): deal 8 more. *(U: 18/+10)* · Art: 24-player raid's first strike (*Log Horizon*)
- [x] **Duel** — 1⚡ · Deal 8 damage.<br>Exploit (only one enemy remains): deal 6 more. *(U: 10/+8)* · Art: arena duel before a roaring crowd (*Overlord*)
- [x] **Cross-Class Combo** — 1⚡ · Deal 6 damage once for each different card type you played earlier this turn. *(U: also count this card's type)* · Art: Iruma fires the Pandoroola bow as sword, spell, and scroll motifs converge (*Welcome to Demon School! Iruma-kun* — Harvest Festival bow scene)
- [ ] **Cleave the Horde** — 2⚡ · Deal 8 damage to ALL enemies.<br>For each Fatal kill, gain 3 EXP. *(U: 11 damage)* · Art: one swing, a dozen EXP popups
- [x] **Counter Read** — 1⚡ · Deal 7 damage.<br>Exploit (an enemy intends to Attack): apply 2 Weak. *(U: 9 damage, 3 Weak)* · Art: a masked mastermind catching a blade bare-handed atop a train (*The Eminence in Shadow*)
- [ ] **Killing Blow** — 1⚡ · Retain. Deal 6 damage. Fatal: Level Up. *(U: 9 damage)* · Art: finishing strike dissolving a boss into light
- [x] **Skill Chain** — 2⚡ · Deal 5 damage 3 times. *(U: 6 damage per hit)* · Art: Diablo chains three blue-white spell impacts into a demon opponent (*How Not to Summon a Demon Lord* — high-tier spell duel)
- [ ] **Monster Grinding** — 1⚡ · Deal 10 damage.<br>Fatal: permanently increase this card's damage by 3.<br>Exhaust. *(U: 13 damage, +4 growth)* · Art: evolution menu after the hundredth kill (*So I'm a Spider*)
- [x] **Steal Technique** — 1⚡ · Deal 7 damage.<br>Exploit (target has a debuff): gain 2 EXP. *(U: 9 damage, 3 EXP)* · Art: Maple acquires poison resistance and Devour from the Poison Dragon (*BOFURI*)

**Skills**

- [ ] **Read the Code** — 0⚡ · Gain 2 Exploit. Exhaust. *(U: 3 Exploit)* · Art: the world dissolving into green glyphs
- [ ] **Guild Reception** — 1⚡ · Gain 6 Block. Choose 1 of 3 Quests and add it to your hand. *(U: 8 Block)* · Art: beaming guild receptionist stamping paperwork
- [ ] **Cheat Inventory** — 1⚡ · Copy an Attack in your hand without Exhaust.<br>The copy gains Exhaust.<br>Exhaust. *(U: costs 0)* · Art: pulling a duplicate sword out of thin air (*Tensura* Great Sage vibes)
- [x] ~~**Map Hack** — 0⚡ · Draw 2, then discard 1. *(U: draw 3)* · Art: Shiroe reroutes a raid across a glowing dungeon map (*Log Horizon*)~~ **Superseded implemented version.**
- [ ] **Map Hack** — 0⚡ · Draw 2, then discard 1. *(U: Retain; no additional draw)* · Art: Shiroe reroutes a raid across a glowing dungeon map (*Log Horizon*)
- [ ] **Save Scum** — 1⚡ · Abandon your Quests. Discard your hand.<br>Draw 1 card for each card discarded or abandoned. *(U: draw 1 more)* · Art: the same hallway, the seventh attempt (*Re:Zero*)
- [x] **Barrier Magic** — 2⚡ · Gain 13 Block. Exploit (Level 4+): gain 5 more. *(U: 15/+6)* · Art: Air Strike Shield chaining into three layered wards (*The Rising of the Shield Hero*)
- [ ] **Emergency Commission** — 1⚡ · Gain 8 Block.<br>You may abandon a Quest to choose a new one from 3. *(U: 11 Block)* · Art: a guild receptionist tears up an impossible contract and stamps an emergency replacement (*KonoSuba* guild comedy)
- [x] ~~**Level Grinding** — 2⚡ · Gain 6 EXP. *(U: 8)* · Art: Kumoko amid a trail of defeated low-level labyrinth monsters and stacked level-up glows (*So I'm a Spider, So What?* — Great Elroe Labyrinth grind)~~ **Superseded implemented version.**
- [ ] **Level Grinding** — 1⚡ · Gain 6 EXP. Exhaust. *(U: 8 EXP)* · Art: Kumoko amid a trail of defeated low-level labyrinth monsters and stacked level-up glows (*So I'm a Spider, So What?* — Great Elroe Labyrinth grind)
- [x] **Negotiation** — 1⚡ · Apply 2 Weak. Gain 1 EXP. *(U: 3 Weak, 2 EXP)* · Art: brown-haired noble girl offering tea as three crimson doom arrows bend harmlessly behind her (*My Next Life as a Villainess*)
- [ ] **Applied Physics** — 1⚡ · Gain 8 Block. The next Attack you play this turn ignores Block. *(U: 11 Block; draw 1)* · Art: explaining leverage to a knight, moments before demonstrating it (*Bookworm* energy)
- [ ] **Side Story** — 0⚡ · Abandon a Quest to gain 2 Exploit and draw 1 card. *(U: 3 Exploit)* · Art: the hero discovers a loophole in a sidequest contract and walks away with its hidden knowledge
- [x] ~~**Power-Up Montage** — 2⚡ · Gain 4 EXP and 4 Block. *(U: 5/6)* · Art: Kazuya studies maps and reforms while Living Poltergeist drives a fan of floating quills (*How a Realist Hero Rebuilt the Kingdom* — royal paperwork montage)~~ **Superseded implemented version.**
- [ ] **Power-Up Montage** — 1⚡ · Gain 4 EXP and 4 Block. *(U: 5 EXP, 6 Block)* · Art: Kazuya studies maps and reforms while Living Poltergeist drives a fan of floating quills (*How a Realist Hero Rebuilt the Kingdom* — royal paperwork montage)
- [x] **Death Flag** — 1⚡ · Apply 2 Vulnerable. Gain 1 EXP. *(U: 3 Vulnerable)* · Art: a young villainess uprooting doom flags with a hoe (*My Next Life as a Villainess*) — one of our two planned Vulnerable sources, alongside Atomic+
- [ ] **Dodge the Bad End** — 1⚡ · Put a card from your discard pile on top of your draw pile.<br>It costs 0 next turn.<br>Exhaust. *(U: put it into your hand; it costs 0 this turn)* · Art: frantically steering away from the doomed route (*Villainess*)

**Powers**

- [ ] **Genre Savvy** — 1⚡ · At the start of your turn, gain 1 Exploit. *(U: Innate)* · Art: hero side-eyeing an obvious mimic chest
- [ ] **Quest Log** — 1⚡ · Whenever you complete a Quest, draw 2 cards. *(U: draw 3 instead)* · Art: an immaculately organized journal (*Log Horizon*)
- [x] ~~**Training Arc** — 1⚡ · Common Attack · Deal 5 damage twice. If you played a Skill this turn, gain 4 Block. *(U: 3 hits)*~~ **Superseded implemented version.**
- [ ] **Training Arc** — 1⚡ · At the start of your turn, you may Exhaust a card to gain 2 EXP. *(U: 3 EXP)* · Art: burning yesterday's techniques to forge better ones
- [ ] **Fast Learner** — 1⚡ · Whenever you trigger an Exploit bonus, gain 1 EXP. *(U: costs 0)* · Art: skill notifications stacking faster than they can be read
- [x] **Job: Alchemist** ✅ — 1⚡ · The first time each turn you apply a debuff, gain 4 Block and your next Attack against that enemy deals 4 more damage. *(U: 6/6)* · Art: transmutation circle mid-brawl (*Arifureta* crafting-into-combat)
- [ ] **Job: Spellblade** — 1⚡ · After your first Skill each turn, your next Attack this turn costs 1 less and deals 4 more damage. *(U: 7 more damage)* · Art: spell wrapped around a blade edge
- [ ] **Job: Appraiser** — 1⚡ · Before drawing each turn, look at the top 3 cards of your draw pile.<br>You may put 1 on the bottom. *(U: up to 2)* · Art: the world's stats, always visible
- [ ] **Skill Tree** — 1⚡ · Level Ups grant 2 additional Vigor. *(U: 3)* · Art: constellation of unlocked nodes
- [ ] **Mana Sense** — 1⚡ · Whenever you spend an Exploit, draw 1 card. *(U: also gain 2 Block)* · Art: seeing the seams in reality

### 5.4 Rares (26 — 9 Attacks / 9 Skills / 8 Powers)

*Job: capstones and iconic anime moments — the goal doc's "Cheat Skills" pillar. Every rare should make someone screenshot their hand.*

**Attacks**

- [x] ~~**EXPLOSION!** — 3⚡ · Deal 28 to ALL. You cannot play Attacks next turn. *(U: 36)* · Art: one glorious detonation, caster face-down in the dirt (*KonoSuba* — the obvious one)~~ **Superseded implemented version.**
- [ ] **EXPLOSION!** — 3⚡ · Deal 36 damage to ALL enemies. You cannot play Attacks next turn. *(U: 44 damage)* · Art: one glorious detonation, caster face-down in the dirt (*KonoSuba* — the obvious one)
- [x] **I Am Atomic** ✅ — 3⚡ · Deal 36 damage to ALL enemies.<br>Costs 1 less this combat whenever you trigger an Exploit bonus.<br>Exhaust. *(U: apply 1 Vulnerable to ALL enemies before damage)* · Art: Shadow beneath the violet halo in the underground sanctuary (*Eminence in Shadow*)
- [x] **Starburst Stream** — X⚡ · Deal 4 damage X+2 times. *(U: 5)* · Art: a dual-wielder spiraling through cyan and orange sixteen-hit trails toward The Gleam Eyes (*SAO*)
- [x] ~~**Degenerate Tactics** — 1⚡ · Deal 8. Apply 2 Weak. Gain 8 Gold. *(U: 10, 10 Gold)* · Art: winning as dishonorably as physically possible (*KonoSuba*)~~ **Superseded implemented version.**
- [ ] **Degenerate Tactics** — 1⚡ · Deal 8 damage. Apply 2 Weak. Gain 12 Gold.<br>Exhaust. *(U: 10 damage, 16 Gold)* · Art: winning as dishonorably as physically possible (*KonoSuba*)
- [x] ~~**Hero's Judgment** — 2⚡ · Deal 16. Exploit (Level 7+): deal 32 instead. *(U: 20/40)* · Art: white-and-gold skeletal knight raises a blue-white judgment sword as his purple-black cape fills a forest clearing (*Skeleton Knight in Another World*)~~ **Superseded implemented version.**
- [ ] **Hero's Judgment** — 2⚡ · Retain.<br>Deal 16 damage.<br>Exploit (you Leveled Up this turn): deal 20 more. *(U: 20/+24)* · Art: white-and-gold skeletal knight raises a blue-white judgment sword as his purple-black cape fills a forest clearing (*Skeleton Knight in Another World*)
- [x] **Anti-Boss Art** — 2⚡ · Deal 20 damage.<br>Exploit (target is an Elite or Boss): deal 10 more. *(U: 24/+12)* · Art: a black-red Machine God cannon array converges on a colossal boss core (*BOFURI*)
- [ ] **Ultimate Skill: Sage** — 2⚡ · Deal 12 damage. Gain 2 Exploit. *(U: 15 damage, 3 Exploit)* · Art: calm blue analysis text over a chaotic battlefield (*Tensura*)
- [x] **Megiddo** ✅ — 2⚡ · Deal 18 damage.<br>Exploit (you played a Power this turn): deal 9 damage to ALL enemies. *(U: 24/12)* · Art: Rimuru's water lenses focus sunlight onto the Falmuth army (*Tensura*)
- [ ] **Season Finale** — 2⚡ · Deal damage equal to 10 + EXP gained this combat.<br>Exhaust. *(U: 15 + EXP)* · Art: every technique learned this arc, used at once

**Skills**

- [ ] **Sequence Break** — 1⚡ · Complete a Quest in your hand. Draw 1 card. *(U: costs 0)* · Art: walking through a wall the developers forgot to finish
- [ ] **Checkpoint** — 1⚡ · The next time you would die, heal 15 HP and gain 8 EXP instead.<br>Checkpoint can save you only once per combat.<br>Exhaust. *(U: 20 HP, 10 EXP)* · Art: waking up at the save point, memories intact (*Re:Zero*)
- [x] ~~**System Menu** ✅ — 2⚡ · Choose a card in your hand. Add **Override** to it for the rest of combat. Exhaust. *(U: may choose from your discard pile instead)* · **[the per-card permanent cheat]** · Art: Kumoko rearranging her visible skill tree in the Great Elroe Labyrinth (*So I'm a Spider, So What?*)~~ **Superseded implemented version.**
- [ ] **System Menu** — 1⚡ · Give a card in your hand **Override** this combat.<br>Exhaust. *(U: may also choose from your discard pile)* · Art: Kumoko rearranging her visible skill tree in the Great Elroe Labyrinth (*So I'm a Spider, So What?*)
- [ ] **Goddess's Blessing** — 2⚡ · Heal 8. Exploit (Level 6+): heal 6 more. Exhaust. *(U: heal 10/+7)* · Art: divine light, smug goddess demanding gratitude
- [ ] **Perfect Preparation** — 2⚡ · Gain 15 Block. Exploit (you have a Quest in your hand): gain 10 more. *(U: 18/+12)* · Art: 47 contingency plans, laminated (*Cautious Hero*)
- [ ] **Reincarnate** — 2⚡ · Level Up twice. Draw 2 cards.<br>Exhaust. *(U: Level Up 3 times)* · Art: the glowing circle, the new sky, the second chance
- [ ] **Party Formation** — 1⚡ · Choose 2: deal 8 damage, gain 8 Block, or gain 4 EXP. *(U: 11 damage / 11 Block / 6 EXP)* · Art: dysfunctional four-person party, somehow functional
- [ ] **Full Clear** — 2⚡ · Complete all Quests in your hand.<br>Exhaust. *(U: costs 1)* · Art: 100% completion screen, every sidequest ticked
- [x] **Slow Life** — 1⚡ · Heal 3 and gain 6 Block. Exhaust. *(U: 4/8)* · Art: a first giant-turnip harvest beneath the great tree (*Farming Life in Another World*)

**Powers**

- [ ] **OP Protagonist** — 3⚡ · Your first 2 unmet Exploit conditions each turn count as met for free.<br>Does not stack. *(U: costs 2)* · Art: enemies checking the hero's stats and quietly leaving
- [ ] **Break the Level Cap** — 2⚡ · Remove your Level cap.<br>Whenever you Level Up, gain 1 Strength.<br>Does not stack. *(U: costs 1)* · Art: the number 10 shattering like glass
- [ ] **Guild Master** — 2⚡ · At the start of your turn, add a random Quest if you have none.<br>Quests grant double EXP.<br>Does not stack. *(U: choose 1 of 3 Quests instead of random)* · Art: the desk where every adventurer's story starts
- [x] **Grinding Montage** ✅ — 2⚡ · At the start of your turn, upgrade 1 random Attack or Skill in your hand for this combat. If it's already upgraded, reduce its cost by 1 this turn. *(U: 2 cards)* · Art: the training episode, permanently
- [ ] **Mana Overflow** — 2⚡ · At the start of your turn, gain 1 Energy if you are Level 5+. *(U: Level 4+)* · Art: mana circuits glowing through skin
- [ ] **Plot Armor** — 2⚡ · Each turn, reduce the first unblocked damage you take by your Level. *(U: first 2 times)* · Art: the blade that stops exactly one millimeter short
- [ ] **Legend in the Making** — 2⚡ · Gain 8 Block. Choose 1 of 3 Quests.<br>Whenever a non-minion enemy dies, Level Up. *(U: 11 Block)* · Art: bards already writing the song mid-battle
- [ ] **Protagonist Privilege** — 2⚡ · Repeat the first Exploit bonus you trigger each turn.<br>Does not stack. *(U: first 2 bonuses)* · Art: the rules apply to everyone else

### 5.5 Ancient cards (2 — full-art specials, obtainable only from Ancients)

- [x] ~~**Truck-kun** — 2⚡ · Uncommon Attack · Deal 15 damage to ALL enemies. Fatal: gain 1 Energy once if any eligible enemy dies. Exhaust. *(U: 19 damage; Fatal also draws 1)*~~ **Superseded implemented version.**
- [ ] **Truck-kun** — 3⚡ · Attack · Level Up. Deal 32 damage to ALL enemies.<br>For each Fatal kill, gain 1 Energy and Level Up.<br>Exhaust. *(U: 40 damage)* · Art: **existing alpha art — reuse** (adapt to the Ancient full-art frame)
- [ ] **NEW GAME+** — 2⚡ · Power · Gain 4 EXP.<br>At the start of your turn, gain 2 EXP and 1 Exploit. *(U: recurring EXP becomes 3)* · Art: title screen with a save file that remembers everything

**Active set totals:** 35 Attacks / 35 Skills / 18 Powers = 88. Historical struck-through entries are excluded. These are our scope targets, not proof of balance.

**Unchanged implementation clarifications:** Item Box grants Retain for the rest of combat, not only this turn. Alchemist’s target-specific bonus persists until the next attack against that enemy and benefits its hits; unused bonuses may accumulate. Grinding Montage triggers after the normal draw, chooses distinct Attack/Skill cards, upgrades an upgradable card, otherwise discounts a non-X-cost card for this turn. Cross-Class Combo counts played card types, not character classes; unplayable Quests do not count. Confirm all four in the shared acceptance pass.

### 5.6 Developer notes — not card text

These specify execution and verification. They must not be appended to card descriptions. Player-visible restrictions remain in the effects above.

| Cards | Implementation contract |
|---|---|
| Exploit cards, Atomic, Fast Learner, Privilege | “Trigger an Exploit bonus” is the shared condition-satisfied event in §3.2, whether natural or forced. Repeating the bonus does not trigger it again. Flat bonus damage modifies the existing hit; extra-hit bonuses increase that attack's hit count. |
| Shield Bash | Resolve the conditional Block before the unconditional Block. This removes the unusual “before playing this” sentence while keeping the card from satisfying its own condition. Preview in printed order. |
| Steal, Degenerate Tactics | Exhaust replaces the v3.0 shared-name payout counter. Each acquired copy can pay out once through ordinary play. Neither is eligible for Cheat Inventory. Exhaust recovery, external generation, and external duplication remain explicit balance tests, not claims of universal immunity to farming. |
| Monster Grinding | Exhaust replaces the custom per-combat growth counter. A Fatal upgrades only the played card's own persistent identity; a temporary copy has no deck identity to mutate and retains its gains only in combat. No minion Fatal. Display current damage; save original-card growth. Test external replay and Exhaust recovery. |
| Cheat Inventory | Copy only a non-Exhaust Attack from hand, into hand, for this combat. The copy inherits upgrade and current damage, then gains Exhaust. The general target rule naturally excludes this Skill, all healing Skills, and Monster Grinding; no named exception list. |
| Save Scum | Count only cards this play successfully abandons or discards. Abandon all held Quests, then discard all remaining cards. Draw their combined count, plus the upgrade's extra card. No retroactive progress or Quest completion. |
| Emergency Commission, Side Story | Abandonment must succeed before paying its conditional reward. Commission's Block is unconditional. Abandon means Exhaust with no Quest reward. |
| Job Board, Guild Reception, other Quest offers | “Choose 1 of 3 Quests” adds the choice to hand and follows shared eligible-offer rules. Fewer eligible titles means fewer options; do not invent illegal objectives to fill the screen. Allow skipping offers. |
| Training Arc, Guild Master | Trigger after the normal draw. Training Arc is optional and grants EXP only after a card is actually Exhausted. Guild Master checks for a held Quest at resolution. Its non-stacking restriction is printed. |
| Job: Spellblade | First Skill arms the next Attack for this turn. Damage applies to each hit; the discount floors at zero and does not reduce X costs. Multiple copies add their damage and discounts normally; no hidden non-stacking exception. |
| Job: Appraiser | Inspect before the normal draw. Move the chosen card(s) to the bottom; the remaining inspected cards keep their order. Multiple copies increase the maximum number that can be moved, capped by the three inspected cards, in one combined prompt. |
| Full Clear | Snapshot Quests in hand on play, complete in hand order, and never include Quests generated by those rewards. Each Quest completes once. |
| Season Finale, Reincarnate | The former uses actual EXP gained, including starting EXP and overflow; direct Level Ups do not count as EXP. The latter preserves the EXP remainder and only fires Level-Up rewards for actual increases. |
| Checkpoint | One rescue per player per combat across all copies. Mark the rescue spent before granting HP/EXP; another copy cannot re-arm it. Later hits still apply. §7 defines precedence with the relic. |
| Party Formation | Choose two different modes, then resolve damage, Block, and EXP in that order for the selected modes. Upgrade changes their numbers, not the number of choices. |
| OP Protagonist, Break the Level Cap, Guild Master, Privilege | “Does not stack” means another copy cannot increase allowances/multipliers or reset counters. The highest upgraded allowance applies where relevant. Existing §3 event rules still apply. Break the Level Cap's upgrade now reduces cost; it no longer grants Dexterity. |
| Legend in the Making | With a living Elite/Boss, offer Boss Slayer if eligible alongside up to two ordinary Quests; otherwise offer up to three ordinary Quests. Choice is optional. Block and the ongoing death trigger resolve even if no Quest can be added. This is a special offer pool, not another player rule. |
| Truck-kun, Cleave the Horde | “For each Fatal kill” counts eligible non-minion kills caused by this attack, once per victim. Truck's initial Level Up occurs before damage; kill rewards occur after the attack. |
| NEW GAME+ | Grant 4 EXP on play, then 2/3 EXP and 1 Exploit each subsequent turn. No Level-Up listener, cap-dependent fallback, or special duplicate rule. Copies stack normally. EXP can overflow; Exploit remains useful at the cap. |

**Readability acceptance:** inspect unupgraded and upgraded cards at normal game size. Prefer 25 words or fewer for a basic/common effect and 35 or fewer for other cards; these are review targets, not reasons to omit essential timing. Alchemist, Route Guide, and Grinding Montage are allowed modestly longer text because they express one connected operation. If their rendered cards still feel dense, reopen the effect rather than shrink the font.

---

## 6. Distinct draft and combat plans

1. **Breakthrough timing:** Grind/Study the System/Level Grinding → Item Box/Combo Rush/Hero's Judgment/Starburst Stream. Arrange the next Level Up and choose the right Vigor consumer. Growth Slash provides dependable sustained scaling; Season Finale converts cumulative progress into one burst.
2. **Cheat allocation:** Game Knowledge/Read the Code/Genre Savvy → System Menu/Mana Sense/Atomic/Privilege. Fulfill easy conditions naturally and reserve finite bypasses for expensive conditions. OP Protagonist helps two unmet clauses per turn without solving the entire deck.
3. **Quest management:** Job Board/Guild Reception/Legend → Quest Log/Objective Cleared/Sequence Break/Full Clear. Hold a Quest for Perfect Preparation, complete it for tempo, or abandon it with Side Story/Emergency Commission/Training Arc. The useful objective changes with the hand.
4. **Fatal support:** Mob Hunt/retained Killing Blow/Last-Hit Bonus/Monster Grinding/Truck-kun reward kill sequencing. Bring Duel, Growth Slash, Hero's Judgment, or a Quest plan for isolated bosses. This is a support package, not a promised standalone archetype.

Jobs remain three uncommon Powers: Alchemist favors debuffs, Spellblade favors Skill-to-Attack turns, and Appraiser improves future draws. None scales from Level. Successful runs should differ in picks, upgrades, retention targets, and combat order; a universal best deck of filtering plus EXP plus multi-hit attacks fails the replayability check.

---

## 7. Relics (9 — chosen distribution: Starter + upgraded Starter + 1C + 2U + 3R + 1 Shop)

- [x] **The System** ✅ *(Starter)* — Enemies grant 3 EXP when they die (minions excluded, matching Fatal rules). Start each combat with 2 EXP. · Art needed: the blue window only you can see · *Replaced alpha placeholder Veil of the Unseen.*
- [ ] **The System: Admin Mode** *(Ancient-upgraded Starter)* — Replace The System. Non-minion enemy deaths grant 4 EXP. Start each combat with 6 EXP. Whenever you Level Up, draw 1. The initial Level Up counts; resolve its draw after the normal opening draw. Exclude duplicate/death-prevented notifications. · Art: the same window, now with a password field left blank
- [x] **Beginner's Luck Charm** *(Common)* — Your first unmet Exploit condition each combat counts as met. · Art: a four-leaf clover in a smartphone case
- [x] **OP Smartphone** *(Uncommon)* — Whenever you Level Up, deal 5 damage to a random enemy. · Art: it has no signal and it doesn't matter (*In Another World With My Smartphone*)
- [ ] **Quest Board** *(Uncommon)* — After your opening draw, choose 1 of up to 3 eligible ordinary Quests to add to your hand, or skip. Does not replace a normal draw; use §3.3 offer and hand-cap rules. · Art: portable corkboard, suspiciously well-stocked
- [x] **Forbidden Walkthrough** *(Rare)* — At the start of each combat, gain 3 Exploit. · Art: a strategy guide for a world that shouldn't have one
- [ ] **Hero's Insignia** *(Rare)* — Whenever you Level Up, gain 1 Strength. · Art: the royal crest they hand out with the summoning
- [ ] **Return by Death** *(Rare)* — When you would die, instead heal to 30% of your max HP and gain 10 EXP and 3 Exploit. Once per run. · Art: the smell of the loop (*Re:Zero* — the Lizard Tail slot, but you come back *stronger and knowing more*) · *The alpha's Return by Death rare **skill** is dropped; the name and fantasy live here (§14).*
- [ ] **Reborn Vending Machine** *(Shop)* — The first 3 times you Level Up each combat, gain 5 Gold. Display remaining payouts; cap removal, copies, and direct Level Ups cannot exceed the 15-Gold combat limit. · Art: it fell into another world and it's thriving (*Reborn as a Vending Machine*)

---

## 8. Potions (3 — one per rarity)

- [ ] **Jar of Slime** *(Common)* — Gain 6 EXP. · Art: it's friendly and it's delicious EXP (*Tensura*)
- [ ] **Bottled Cheat Code** *(Uncommon)* — Gain 3 Exploit. · Art: fizzing liquid full of tiny glyphs
- [ ] **Truck Summoning Ritual** *(Rare)* — Choose an enemy. Level Up, then deal 25 unpowered damage to that enemy. Fatal: Level Up once more. Potion damage does not consume Vigor; preview the target and resulting Level. · Art: chalk circle and tire tracks; the joke is in the animation, not an uncontrollable target.

---

**Relic/potion event rules:** Smartphone damage is an owned relic effect and can complete Slay, but never credits a card's Fatal. Finish the current action before resolving queued relic damage so previews and kill attribution remain consistent. Return by Death is once per run, does not grant immunity to later hits, and checks after an unused Checkpoint rescue; only one rescue fires for one lethal event. Jar of Slime previews resulting Level/Vigor and cap waste; Bottled Cheat Code previews current eligible uses and follows bypass precedence. All combat-only resources reset at combat end.

## 9. Balance and fun acceptance checklist

- [ ] **Early tempo:** test revised Farm the Field, Daily Training, Power-Up Montage, Level Grinding, and Read the Code against real Act 1 hands without rare engines. Track HP lost and actions enabled, not only printed value.
- [ ] **Progress while useful:** record first useful Level Up, turn 1–3 Levels, Level before lethal, and post-lethal EXP separately. Change generators only after identifying the actual pacing failure; do not automatically raise EXP-per-Level globally.
- [ ] **Vigor allocation:** verify Twin Blades, Combo Rush, Skill Chain, Starburst Stream at X=0/3/high X, and AoE consumers. Single-hit finishers must retain a reason to exist.
- [ ] **Finite cheats:** preview natural/Override/relic/Power/stack resolution. Test unwanted automatic spending, OP Protagonist + Mana Sense, and bonus-only Privilege repeats. Copies of non-stacking Powers must say so in tooltips.
- [ ] **Quest rules:** test acquisition windows, duplicate titles, same-event completion of different held titles, full hands, Hoarder/Flawless delayed rewards, abandonment, discard/return, and Full Clear snapshots. No reward may retroactively complete a newly generated Quest.
- [ ] **No easy recursion:** two Dodge the Bad End+ copies both Exhaust. Test free retrieval, actual card replays, copying copies, and external duplication. A Level cap is not an anti-infinite mechanism.
- [ ] **Permanent reward limits:** Steal, Degenerate Tactics, and Monster Grinding Exhaust; Cheat Inventory cannot copy them. Vending Machine caps at 15 Gold. Temporary copies never mutate a deck original's growth. No Quest heals. Test multiple acquired copies, external duplication, actual replay, Exhaust recovery, and generation; Exhaust bounds ordinary replay but is not a universal anti-farming guarantee.
- [ ] **Sustain:** record healing per run and intentional Checkpoint activations. Test Checkpoint with Return by Death and multi-attacks; no stacked or rearmed Checkpoint saves.
- [ ] **Cap removal:** verify overflow conversion, event order, direct Level Ups versus EXP totals, Smartphone/Insignia/Admin chains, and finite gold. Break the Level Cap cannot be required for every successful deck.
- [ ] **Rate/rarity watchlist:** unchanged Status Appraisal, Growth Slash, Anti-Boss Art, Counter Read, Slow Life, Skill Tree, Grinding Montage, Mana Overflow, Plot Armor, and relic Insignia need comparative playtests before further changes. Revised Map Hack must not remain an automatic pick across all deck plans.
- [ ] **Distinct payoffs:** compare revised Explosion, Hero's Judgment, Season Finale, Reincarnate, and Truck-kun against Atomic, including turns where they do not kill. Record when a player actually chooses each over alternatives.
- [ ] **Menu value:** record Quest selection time and Appraiser decisions. If choices repeatedly have an obvious answer, simplify rather than expand the token pool.
- [ ] **Replayability:** at least three successful deck shapes disagree on useful rewards/upgrades and turn order; two memorable payoffs must work without Insignia or cap removal.

**Playtest sequence:** deterministic interaction checks first; six exploratory Act 1 runs; then at least twelve full runs per revision split between low Ascension and the tester's usual difficulty. Log comparable base-game runs, but do not interpret small samples or shared seeds as precise controlled win-rate evidence. Record one clever turn, one frustrating turn, useful rewards skipped, turns intentionally prolonged for rewards, and what changed the draft plan. Full procedure and original evidence: [review §§12–14](IsekaiHero_Fun_And_Balance_Review.md#12-recommended-redesign-experiments).

## 10. Cross-class and co-op acceptance

- EXP/Vigor, filtering, Weak, copying, and bounded economy can all be useful outside this class. They must be tested, not described as automatically safe.
- Naturally satisfied conditions still work outside this character; unmet clauses can use audited Exploit compatibility. A card without relevant support may be a poor draft despite having a base effect.
- Curate base-game conditional compatibility individually. Exclude Fatal, permanent rewards, and playability restrictions; do not make an illegal target or unplayable card legal via Exploit.
- The System sees actual eligible enemy deaths from any player; individual Fatal rewards belong to the killing card. Quest counters are owner-local. Test simultaneous deaths, damage-over-time kills, revival, shared enemies, and prevented deaths.
- Validate external draw, cost reduction, duplication, and multi-hit modifiers against §9 limits. Quest completion never fabricates Fatal credit, and copies never reset economy limits.

## 11. Art & tone direction

Tone (from the goal doc, still binding): lean into **genre parody** — stat screens, Truck-kun jokes, Jobs, knowledge exploits, dramatic protagonist nonsense. Specific reference cards can exist, but the character should be broader than any single series.

Card art = stylized homage scenes. For a free fan mod this is community-normal, but direct anime screenshots are copyrighted — prefer **redrawn/stylized homages** (recognizable composition, original rendering) to survive takedown requests on Workshop/Nexus.

**Inspiration references** (carried over from the goal doc):

| Series | Very short story note | Noteworthy inspiration |
| --- | --- | --- |
| KonoSuba | A reincarnated shut-in gets a disastrous adventuring party and a comedy-first fantasy life. | Genre parody, bad Jobs that still work, luck, party chaos, explosive overcommitment. |
| That Time I Got Reincarnated as a Slime | A man reincarnates as a Slime and grows through skills, allies, and monster evolution. | Appraisal-like analysis, skill acquisition, skill fusion, snowballing growth. |
| Re:Zero | A transported boy repeatedly returns from death while trying to save people in a hostile fantasy world. | Checkpoints, retry risk, knowledge gained from failure, `Return by Death`. |
| The Eminence in Shadow | A boy obsessed with being a secret mastermind lands in a world where his improvised shadow war is real. | Absurd protagonist theatrics, dramatic finishers, accidental genius, `I Am Atomic`. |
| Sword Art Online | Players are trapped in a lethal VRMMO and must fight through game systems to survive. | Menus, skill trees, party roles, boss reads, game-literacy cards. |
| Mushoku Tensei | A reincarnated shut-in grows up again in a magic world and trains into a gifted adventurer. | Learning arcs, training, technique mastery, long-term growth. |
| Overlord | An MMO guild leader remains in a game-like fantasy world as his overpowered undead avatar. | Prepared power, outsider meta knowledge, minion command, overwhelming presence. |
| The Rising of the Shield Hero | A summoned hero is stuck with the Shield role and must survive betrayal and restrictions. | Job limits that reshape drafting, defense converted into progress, underdog resourcefulness. |
| No Game No Life | Sibling gamers are taken to a world where conflicts are decided through games. | Rule clauses, prediction, sequencing puzzles, winning by exploiting assumptions. |
| Tsukimichi: Moonlit Fantasy | A summoned hero is rejected by a goddess and builds his own place among non-humans. | Rejected chosen-one comedy, monstrous allies, hidden scale of power, outsider faction-building. |
| Arifureta | A weak crafter is betrayed in a dungeon and survives by turning craft knowledge into brutal power. | Transmutation, improvised weapons, dungeon adaptation, weak Job becoming a cheat. |
| So I'm a Spider, So What? | A student reincarnates as a lowly dungeon spider and levels through constant survival fights. | Monster evolution, skill grinding, predatory survival, desperate snowballing. |
| Ascendance of a Bookworm | A book lover reincarnates into a poor sickly child in a world where books are scarce. | Modern knowledge, making technology from constraints, obsessive goals, low-power cleverness. |
| My Next Life as a Villainess | A girl realizes she is the doomed villainess of an otome game and tries to dodge every bad route. | Doom flags, route prediction, social loopholes, winning by misunderstanding the genre. |
| Welcome to Demon School! Iruma-kun | A kind human boy is adopted by a demon and enrolled in a school where revealing his humanity would be disastrous. | Rank progression, unusual Jobs, found-party comedy, clever survival, and the Pandoroola bow. |
| Saga of Tanya the Evil | A ruthless salaryman is reincarnated as a child mage fighting an alternate-world industrial war. | Tactical foresight, aerial combat reads, computation-jewel magic, and terrifying overkill. |
| I Was Reincarnated as the 7th Prince | A magic obsessive is reborn with the talent and freedom to master every spell he can find. | Spell experimentation, stacked magic circles, rapid skill chains, and gleefully excessive power. |
| Seirei Gensouki: Spirit Chronicles | A slum orphan awakens memories of a past life and grows into a gifted swordsman navigating divided identities. | Underdog training, spirit arts, academy duels, and disciplined sword technique. |
| BOFURI: I Don't Want to Get Hurt, so I'll Max Out My Defense. | A new VRMMO player puts every point into defense and accidentally creates a delightfully broken build. | Defense-as-offense, system loopholes, party protection, transformations, and unconventional optimization. |
| How a Realist Hero Rebuilt the Kingdom | A summoned student rebuilds a struggling kingdom through administration, economics, and practical knowledge. | Modern knowledge applied at scale, paperwork as power, team delegation, and solving fantasy problems systemically. |
| How Not to Summon a Demon Lord | A socially awkward MMO expert is summoned in the body of his overpowered demon-lord avatar. | Game knowledge, chained high-tier spells, role-playing bravado, and power hidden behind social panic. |

**Implemented card-art checklist (updated 2026-09-22):** each generated portrait is an original, simplified redraw that uses the named scene as composition inspiration rather than copying a frame. Restored custom art is called out explicitly.

| Done | Card | Anime inspiration | Scene used for the card art |
| --- | --- | --- | --- |
| [x] | Last-Hit Bonus | *Sword Art Online* | Restored existing custom art of Kirito's finishing strike against the goblin boss. |
| [x] | Boss Telegraph | *Saga of Tanya the Evil* | Tanya dives through an aerial artillery barrage while the strike zone burns below her. |
| [x] | Tutorial Sword | *Seirei Gensouki: Spirit Chronicles* | Rio demonstrates his disciplined wooden-sword stance during Royal Academy training. |
| [x] | Seen It Coming | *Cautious Hero* | Seiya has already sidestepped before an oversized demon attack finishes its swing. |
| [x] | Status Appraisal | *That Time I Got Reincarnated as a Slime* | Rimuru asks Great Sage to analyze a suspicious potion and its ingredients. |
| [x] | Item Box | *I Was Reincarnated as the 7th Prince* | Lloyd explores the forbidden library, calmly drawing a weapon from a violet portal amid floating grimoires. |
| [x] | Route Guide | *My Next Life as a Villainess* | Restored existing custom art of Catarina plotting the branching death and exile routes. |
| [x] | I Am Atomic | *The Eminence in Shadow* | Shadow's first “I Am Atomic” against Zenon, centered beneath the violet halo in the underground sanctuary. |
| [x] | Megiddo | *That Time I Got Reincarnated as a Slime* | Rimuru hovers above Falmuth with black wings while suspended water lenses focus sunlight into precise beams. |
| [x] | System Menu | *So I'm a Spider, So What?* | Kumoko manipulates her visible status screen and branching skill tree in the Great Elroe Labyrinth. |
| [x] | Steal Technique | *BOFURI* | Maple survives the Poison Dragon, then acquires poison resistance and Devour from the encounter. |
| [x] | Map Hack | *Log Horizon* | Shiroe routes the raid party around the obvious path using a glowing dungeon map. |
| [x] | Game Knowledge | *No Game No Life* | Sora realizes the living-chess match does not follow ordinary chess rules and exploits that discovery. |

## 12. Implementation roadmap and definition of done

- [x] **Combat character animations:** original office-worker adventurer art and existing animation work; see [CharacterAnimations](CharacterAnimations.md). Retain assets when revising card effects.
- [x] **Existing core:** LevelPower, The System, Grind, Danger Sense, Exploit stacks, and per-card Override exist. This check records code presence, not acceptance of all v3 semantics.
- [ ] **Phase 1 — Engine contracts:** total EXP counter, overflow/direct-Level semantics, deterministic triggers, source-aware bypass precedence, natural/forced/consumed events, and bonus-only repeat rules (§3). Add damage/resource previews. Validate Vigor with multi-hit and separate attack commands.
- [ ] **Phase 2 — Implement the struck-through replacements:** apply the engine-independent unchecked successors in §5, including bounded gold, revised early tempo, Map Hack, Combo Rush, Hero's Judgment, Explosion, and System Menu. Finish Quest-dependent Training Arc in Phase 3 and Ancient Truck-kun acquisition in Phase 4. Keep the history lines; check successors only after implementation and verification.
- [ ] **Phase 3 — Combat Quests:** eight tokens, eligible offers, bounded title completion, progress windows, abandonment, delayed rewards, full-hand behavior, and owner-local co-op tracking. Implement the common/uncommon generators before judging rare Quest payoffs.
- [ ] **Phase 4 — Complete active pool:** finish all remaining unchecked cards, nine relics, three potions, three Jobs, and Ancient acquisition hooks. Replace the legacy Training Arc Attack and Return by Death Skill in the active pool; do not leave historical versions obtainable alongside replacements.
- [ ] **Phase 5 — Interaction and fun acceptance:** complete §9 deterministic checks, Act 1 exploration, and comparative full runs; log decision quality and stall incentives as well as wins. Then tune numbers and rarity.
- [ ] **Phase 6 — Release:** apply the v3.1 wording to localization, including unchanged implemented effects; render both upgrade states at normal card size; keep §5.6 developer notes out of descriptions. Refresh README pool counts and changelog; verify active counts and acquisition rules before release.

**Per-entry completion:** implement the active text and upgrade; use named DynamicVars for upgradeable values and exact CardLoc references; verify targeting, selection prompts, event timing, copy/replay behavior, and localization; run `dotnet build` for implementation changes and relevant in-game checks. A documentation edit alone does not complete implementation. Do not check untested shared engine contracts just because a card class compiles.

**Legacy Return by Death migration:**

- [x] ~~**Return by Death** — 2⚡ Rare Skill. At the start of your next turn restore the HP, Block, and status values captured when played; lose all Energy next turn. Exhaust. Upgrade has no effect.~~ **Superseded implemented version; outside the 88 active slots.**
- [ ] **Checkpoint** — implement the bounded rescue Skill in §5.4 and remove legacy ReturnByDeath from card rewards. This task references that existing active slot; it is not a second card. The separate Return by Death relic remains pending in §7.

## 13. Open questions (for future sessions)

- [ ] Does the STS2 mod API expose Ancient-encounter reward pools (needed for the 2 Ancient cards + Admin Mode upgrade)?
- [ ] Should EXP-on-kill live on the character (safe from relic loss) or on The System relic (matches Bound Phylactery precedent)? Currently: relic.
- [ ] The clause and the buff share one name (Exploit). If playtests show players think a clause *requires* the buff, rename the **buff** to **Cheat** (genre-perfect: "Gain 2 Cheat") and keep Exploit on cards. Override stays either way.
- [ ] Which base-game conditionals make the initial Exploit compatibility list? Audit after Phase 1 event contracts are stable; shared-pool tests are part of Phase 5.
- [x] **Level-Up identity — decided 2026-09-28:** retain 2 Vigor and its multi-hit interaction. Tune supporting cards before replacing this source of tactical depth.
- [ ] **Color:** implemented `#6C3082` purple may read as Necrobinder-adjacent; consider shifting toward teal/cyan ("another world" portal palette) during the art pass.
- [ ] **Monster Grinding persistence:** serialize growth on the original deck card; Exhaust limits ordinary reuse. A temporary copy cannot mutate another card's permanent state. Test external replay/recovery before release. If persistence cannot be verified, reopen the design instead of silently changing its run-long role.
- [x] **Jobs scope — decided (2026-07-02):** Jobs stay a 3-power cycle + the "you have a Job" condition, with **no Level integration** — tying Job effects to Level would stack two scaling systems on one power and blow the complexity budget; the Exploit condition is already the bridge. Job: Alchemist's existing art stays. Expansion beyond three Jobs is deferred until the v3 replayability checks pass; it is not current implementation scope.
- [ ] Give the hero an in-world name/portrait identity, or keep the anonymous "Isekai Hero" genre-blank? Currently: anonymous.

## 14. Historical alpha merge ledger (v0.4.0-alpha → v2)

**Historical context only.** Active v3 effects, statuses, and successors in §§3–12 supersede this ledger. Names in this section are not additional active slots or implementation claims.

**Kept nearly verbatim (14)** — code exists; needs Exploit-clause wording + §9 number check:
Strike, Defend, Tutorial Sword, Boss Telegraph, Last-Hit Bonus, Seen It Coming, Status Appraisal, Item Box, Route Guide, Megiddo, I Am Atomic, System Menu, Job: Alchemist, Grinding Montage.

**Adopted from the goal doc's unimplemented designs (6):** Cross-Class Combo, Monster Grinding, Applied Physics, Dodge the Bad End, Job: Spellblade, Job: Appraiser, Protagonist Privilege *(7, counting Privilege)*.

**Reworked (1):** Truck-kun — uncommon AoE → **Ancient card** (§5.5), keeping the name (art exists) and the kill→energy hook, adding Level Ups; effect free to retune.

**Dropped from the alpha (3):**
- *Training Arc (common Attack)* — "played a Skill" condition space is covered by Beginner Magic; the montage flavor was needed for the exhaust Power, which keeps the name.
- *Return by Death (rare Skill)* — full state-rewind is confusing and code-heavy; the fantasy moved to the **Return by Death relic** (death save, once per run) and **Checkpoint** (in-combat death cheat). Salvage `ReturnByDeathPower.cs` snapshot logic for Checkpoint.
- *Veil of the Unseen (starter relic)* — placeholder; replaced by **The System**. Reuse its combat-start hook.

**Dropped from the goal doc's plans (2):** Job Change (tutoring 3 Jobs isn't worth a slot yet — revisit if the Job cycle grows), the Jobs-as-pillar framing (now a cycle, see §2.1).

**Dropped from Design v1 in favor of alpha cards (11):** Lucky Crit, Cheap Shot, Warm-Up Swing, Cautious Guard, Appraisal, Party Cheer, Feint, Rapid Cast, Enchanted Arsenal, Strategic Retreat, Otherworld Common Sense — each displaced by a strictly more interesting alpha/goal-doc card in the same slot; plus Atomic (superseded by I Am Atomic), Artillery Barrage (→ Megiddo), Menu Editing (→ System Menu), Status Open (→ Grinding Montage), Demon Lord Form & Protagonist Aura & Familiar & Guild Sponsorship (cut for Protagonist Privilege + the Job cycle).

## 15. v3 revision ledger — 2026-09-28

This records the first balance pass. The v3.1 wording/simplicity decisions in §16 supersede its shared-name gold counters, some upgrades, and NEW GAME+'s fallback. Active entries in §5 always take precedence.

The [research report](IsekaiHero_Fun_And_Balance_Review.md) remains an unchanged review of v2. This plan resolves its experiments into the following prototype choices:

| Review concern | Adopted revision / tracking location |
|---|---|
| Weak early tempo | Farm the Field and Power-Up Montage cost 1; Daily Training becomes 6 Block/2 EXP; Level Grinding becomes 1-Energy burst with Exhaust; Read the Code becomes a zero-cost Exhaust burst. |
| Duplicate attacks / delayed rares | Combo Rush checks a Level Up this turn; Hero's Judgment becomes retained timed burst; Killing Blow gains Retain; Season Finale replaces the unimplemented Grand Finale and costs 2. |
| Explosion overshadowed by Atomic | Test 36/44 AoE with the existing next-turn restriction; compare nonlethal turns before any further buffs. |
| Permanent cheat removes decisions | OP Protagonist only forces two unmet clauses per turn; System Menu costs 1; Privilege repeats explicit bonuses with separate event semantics. |
| Unclear Quest costs / repetitive rewards | Distinct token rewards, no healing, explicit progress windows, one title held/completed per turn window, and a named Boss Slayer generator. |
| Holding / abandoning objectives | Side Quest becomes one-shot generation plus draw; Side Story converts abandonment into cheats/draw; Emergency Commission replaces unimplemented Healing Circle; Save Scum replaces abandoned Quest slots with fresh cards. |
| Gold, copying, recursion, rescue | Shared per-name gold limits, capped Vending Machine, bounded Monster Grinding, Exhaust-preserving Cheat Inventory/Dodge, and one Checkpoint rescue per combat. |
| Filtering dominance | Map Hack upgrade grants Retain instead of additional draw; Status Appraisal stays unchanged pending measured dominance checks. |
| Low-value choices / upgrade removes choice | Appraiser can bottom cards before drawing; Party Formation+ strengthens the chosen two modes; Guild Master only generates when no Quest is held. |
| Capstone and Ancient impact | Reincarnate draws 2; Legend supplies immediate Block and a relevant objective; Truck-kun grants an initial Level Up before attacking; NEW GAME+ has immediate EXP and a cap fallback. |
| Relic / potion usefulness | Quest Board offers a choice, Admin opening draw order is explicit, Truck potion targets an enemy and grants one Level before impact. |
| Unknown balance / role overlap | §9 retains watchlists for unchanged cards and relics instead of speculative blanket changes. §12 requires implementation and playtests before checks become complete. |

Struck-through predecessors are preserved only for effects that already existed in code. Unimplemented proposals were replaced directly. Future revisions should follow the same rule, retain art assets, keep exactly 88 active slots unless scope is explicitly changed, and record new evidence before declaring balance solved.

## 16. v3.1 — short card text, simpler rules

**User direction:** effects should read like short punchlines, not paragraphs of exceptions. The revised card text lives directly in §5; §5.6 contains developer-only details. The research report remains a historical analysis of v2.

| Change | Reason / balance consequence |
|---|---|
| Steal: 8/12 Gold + Exhaust; Degenerate Tactics: 12/16 Gold + Exhaust | Removes shared-name counters. Larger single payouts compensate for losing ordinary repeatability. Multiple drafted copies are useful again; external duplication needs testing. |
| Monster Grinding gains Exhaust | Replaces a bespoke once-per-combat growth counter with a familiar cost. It is less reusable as an attack; retains run-long growth. |
| Cheat Inventory copies only non-Exhaust Attacks | Removes the named exclusion list and excludes healing Skills naturally. This is a narrower, simpler copying card, not merely shorter wording. |
| Tutorial Sword checks only a Power played this turn | Removes the Job-or-Power alternative. The payoff is less automatic after a Job is installed; its 7/11 damage remains the test target. |
| Job Board+ costs 0; Quest Log+ draws 3 | Upgrades improve the existing action instead of adding EXP or Block clauses. Both strengthen Quest tempo and require loop checks. |
| Save Scum abandons all held Quests before redrawing | A complete reset with one counting rule; loses selective abandonment. |
| Appraiser bottoms cards without reordering the remainder | Removes a second low-value sorting step. Copies use one combined prompt and add bottoming capacity. |
| Spellblade copies stack their discount and damage normally | Removes the hidden special discount cap. Watch multi-copy Energy efficiency. |
| Break the Level Cap+ costs 1; no Dexterity rider | Removes the every-second-Level counter and keeps this an offensive scaling Power. Defensive support must come from other cards. |
| Legend offers three Quests, including Boss Slayer when eligible | One readable choice replaces an encounter-dependent if/else sentence. |
| NEW GAME+ grants 4 EXP now, then 2/3 EXP + 1 Exploit per turn | Removes the Level-Up listener, cap fallback, and duplicate exception. It has steady cheat income at every Level, but no cheat burst from multiple Level Ups. |
| Other effects shortened without mechanical changes | Separate keyword lines, direct verbs, consistent Exploit wording, and developer notes outside the description. Historical implemented versions remain intact. |

No balance changes are implied by typography alone. **Cards with unchanged behavior stay checked, but their revised wording still needs the pending localization pass.** No C# gameplay or localization files were changed by this design edit.
