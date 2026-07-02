# IsekaiHero — Character Design Document

> **Mod:** IsekaiHero · **Game:** Slay the Spire 2 (Early Access) · **Stack:** C# / [BaseLib-StS2](https://github.com/Alchyr/BaseLib-StS2) · Steam Workshop since game v0.107.1
> **Character:** Isekai Hero (`ISEKAIHERO-ISEKAI_HERO`)
> **Status:** Design v2.0 — merges the `v0.4.0-alpha` card set and the old [goal doc](isekai-hero-goal.md) into one plan. This file is the **single source of truth**; update its checkboxes as features land.

---

## 1. Elevator pitch

An ordinary office worker gets hit by a truck and wakes up at the foot of the Spire with one gift: **they can see the world's game interface.** Status screens, EXP bars, quest prompts — and the knowledge of how to abuse all of it.

**You start every combat at Level 1 and end it as the overpowered protagonist.** That is the isekai power fantasy, compressed into a single fight: grind, level up, snowball, then delete the boss with a technique that has a chant longer than the enemy's remaining lifespan.

The alpha's North Star still governs everything: *the hero becomes powerful by understanding systems faster than the Spire expects, turning that advantage into dramatic spikes of strength.* The player should feel **clever first, strong second**.

---

## 2. What the research says (and how we obey it)

Verified structure of every STS2 character (from slaythespire.wiki.gg data modules):

| Slot | Template | IsekaiHero |
|---|---|---|
| Cards | 4 Basic + 20 Common + 36 Uncommon + 26 Rare + 2 Ancient = 88 | ✅ same |
| Starter deck | 4 Strike + 4 Defend + 2 signature basics (1 generator + 1 spender) | Grind + Stat Check |
| Relics | 1 Starter + 1 upgraded Starter + 1 C + 2 U + 3 R + 1 Shop = 9 | ✅ same |
| Potions | 3 (one per rarity) | ✅ same |
| Mechanics | 1 signature **resource** + 1 signature **keyword** + 1 supporting **package** | EXP/Level + Exploit + Quests |
| Starting relic | Automates the signature resource turn 1 | "The System" grants EXP passively |
| Baselines | 1⚡ ≈ 6 dmg / 5 Block; riders push to ~9–10; upgrades add ~+3; 3⚡/turn, draw 5 | ✅ same |

**Fun lessons stolen from MegaCrit (each one is a hard design rule here):**

1. **The Regent lesson.** Pure delayed-payoff felt terrible at launch; the fix was immediate-value riders on setup cards. → *Every EXP/setup card in this set does something NOW (damage, block, or draw) in addition to feeding the engine.*
2. **The Silent lesson.** Sly is S-tier because one action (discard) feeds three payoffs simultaneously. → *One action here — gaining EXP — feeds Level scaling, Level-Up triggers, and Exploit-condition thresholds simultaneously.*
3. **The Necrobinder lesson.** Her three mechanics all trade in one currency (time). → *Our three mechanics all trade in one currency: **progress**. Quests produce EXP, EXP produces Levels, Levels unlock Exploit conditions, and Exploit stacks skip the queue.*
4. **The new-keyword pricing rule.** Doom waits a turn, Sly is overcosted to hard-cast, Stars don't refill — every new mechanic pays for above-rate numbers with a time-shaped drawback. → *Exploit-clause cards are ~15% below rate unmet and ~15% above rate met; Quests are dead hand-slots until completed.*
5. **The Tyranny rule.** One deliberately off-philosophy card per class is healthy when it patches the class's core weakness rather than importing another win condition. → *Ours is **Training Arc** (a turn-start exhaust power, pure Ironclad grammar) because our weakness is hand clog from Quests.*

**Passive playstyle lean** (the "Ironclad quietly gets more Vulnerable" trick): Isekai Hero leans on **Weak** and **on-kill (Fatal) triggers**. The rhythm is *stall → grind → snowball*: Weak buys the early turns you need to level, Fatal effects reward sequencing kills in multi-enemy fights. We get almost no Vulnerable — big numbers come from Levels, not debuff multiplication.

**Distinctness check** — why this isn't a mish-mash (this table restates the goal doc's "should not define itself through existing class identities" rule):

| Character | Their engine | Why ours is different |
|---|---|---|
| Ironclad | Pays HP/cards for power | We pay **time and hand-slots** for power; we never self-harm |
| Silent | Velocity — many cheap cards per turn | Our snowball is a **state** (Level), not a per-turn combo count |
| Defect | Passive slot machine (orbs + Focus) | Level is earned through *play patterns*, not channel actions; no slots |
| Regent | Banks resources for one big turn | Levels **never get spent** — we snowball monotonically, no bank-and-dump |
| Necrobinder | Board state (Osty) + execute clock | No companion, no execute; our clock runs *up*, not down |

### 2.1 Merging the alpha — old pillars → new pillars

The goal doc's three pillars survive; they just stop being three separate systems and become **roles inside one triangle**:

| Old pillar (goal doc) | Where it lives now |
|---|---|
| **Jobs** — "What class did I get?" | A **cycle of 3 uncommon `Job:` Powers** (Alchemist ✅, Spellblade, Appraiser) plus the Exploit condition *"you have a Job"* (Tutorial Sword ✅). Jobs read as roles because cards *check* for them — build meaning without a fourth mechanic. If the cycle proves popular, it can grow (see §13). |
| **Exploits** — "How do I break the systems?" | Split in two: the **formal Exploit clause/buff** (§3.2 — the "explicit implementation hook" the goal doc asked for, and the alpha's `HasConditionalEffects` code already provides), plus the **knowledge/manipulation card family** (Status Appraisal ✅, Route Guide ✅, Map Hack, Save Scum, Applied Physics, Dodge the Bad End). |
| **Cheat Skills** — "When does the protagonist moment happen?" | The **rare payoff suite**: I Am Atomic ✅, Megiddo ✅, System Menu ✅, Grinding Montage ✅, EXPLOSION!, Grand Finale, OP Protagonist, Protagonist Privilege. Earned via Level thresholds, Exploit setup, or Quest completion — never free. |

**Design filters** (kept from the goal doc — ask these of every new card):

1. Does this feel like an isekai trope instead of a normal fantasy hero card?
2. Does it support EXP/Level, Exploit, Quests, a Job, or a strong flavor exception?
3. Does the player feel smart or powerful for using it?
4. Would the same mechanic make more sense on an existing STS2 class?
5. Is the power fantasy earned through choices, risk, setup, or deckbuilding?

**Acceptance checks** (adapted): future card sets should support runs that feel like — **Level Run** (grind and snowball), **Cheat Run** (Exploit consistency, every card at max text), **Quest Run** (objectives into engines), **Fatal Run** (kill-sequencing), and hybrids of any two.

---

## 3. The three mechanics

### 3.1 EXP & Level — the signature resource

- You start every combat at **Level 1** with 0 EXP. **Every 4 EXP = 1 Level Up** (EXP rolls over). **Level cap: 10.**
- **When you Level Up: gain 2 Vigor** (your next Attack deals +2). Small, aggressive, always feels good — the "ding!" moment.
- Cards reference Level two ways: scaling ("deal 3× Level damage") and thresholds ("Exploit (Level 4+): …").
- **Enemies grant EXP when they die** (via the starting relic) — killing mobs to level up is *literally the gameplay*.
- Expected curve on a normal hallway fight: Level 3–4. Elite: 5–6. Boss with a dedicated deck: 8–10. Tune generators to this curve.

**Time-shaped drawback:** Level is worthless on turn 1 and monstrous on turn 7. The character is intrinsically weak early — every fight re-runs the zero-to-hero arc.

**Persistence:** Level resets to 1 every combat — like Stars, orbs, and Block, only relics persist. Re-running the zero-to-hero arc *is* the fantasy, and the per-combat reset is what lets mid-combat numbers stay big: a run-persistent Level would force balancing every fight around Level 10. The run-long "I'm getting stronger" feeling comes from the usual place — upgrades, relics, and Admin Mode's bigger starting EXP.

**Other classes:** EXP, Level, and the Level-Up Vigor ding are implemented as **class-agnostic player buffs** (the way Doom and Poison aren't hard-wired to Necrobinder/Silent). Any character who obtains an EXP card — co-op, events, shared-pool modes — gains EXP, levels up, and gets the Vigor. Without our payoff cards each level is just a pleasant 2-Vigor drip, so cross-class EXP cards are never dead and never broken. Kill-EXP lives on The System relic, so other classes never level passively.

### 3.2 Exploit — the signature keyword

One keyword, two faces — the clause on cards names exactly what the buff manipulates. (Earlier drafts used a separate "Bonus" keyword for the clause; merged, because one name keeps the mental model tight.)

- **Exploit clause (card keyword):** a conditional rider. `Deal 5 damage. Exploit (Level 3+): deal 5 more.` Base effect ~15% under rate; met effect ~15% over rate. This formalizes both the Nexus alpha's design *and* the goal doc's "conditional effect" shorthand — the goal doc explicitly asked for "an explicit implementation hook instead of relying on rules text parsing," and this is it.
- **Exploit (player buff, stacks):** *"When you play a card with an unmet Exploit condition, consume 1 stack: the condition counts as met."* Key detail: **a stack is only consumed when the condition is actually unmet** — you never waste one, so stacking is never a feel-bad.
- **Override (card state keyword — already in the alpha ✅):** *"This card's Exploit conditions always count as met."* The permanent, per-card big brother of the Exploit buff. Granted by System Menu ✅; **OP Protagonist** is functionally Override-on-everything. Keyword id `ISEKAIHERO-OVERRIDE` and localization already exist.
- **Implementation status:** the hook is real code today — `IsekaiHeroCard.HasConditionalEffects`, `IsConditionalEffectActive(bool)`, and `EnableConditionalEffectsForCombat()` — used by Tutorial Sword, Boss Telegraph, System Menu, I Am Atomic, et al. Remaining work is the *stacking buff* (checking/consuming Exploit stacks inside `IsConditionalEffectActive`) and printing conditions in the `Exploit (…)` format.
- **Base-game support:** the buff is not limited to our cards. A compatibility layer tags a **curated** list of base-game "Do X. If you did Y, do Z." cards with the Exploit clause. Per-card, because conditions are code — there is no generic hook that can flip an arbitrary `if` on someone else's card. Curated, because some conditionals must stay un-exploitable: forcing Fatal-style "if this kills" payoffs (heal, max-HP, permanent stats) without a kill would be degenerate. Candidate audit happens in Phase 2.
- Flavor: you're not getting stronger, you're *abusing the game's code*. The clause is the rule; the buff is the cheat.

**Standard Exploit condition library** (reuse these; don't invent one-off conditions):
`Level X+` · `an enemy intends to Attack` · `you have a Quest in your hand` · `you completed a Quest this turn` · `you Leveled Up this turn` · `this is the first Attack this turn` · `you played N+ cards this turn` · `only one enemy remains` · `target is at full HP` · `target has a debuff` · `you have Block` · `you played a Skill this turn` · `you played a Power this turn` · `you have a Job`

### 3.3 Quests — the supporting package

STS2 already has a **Quest card type** (unplayable objective cards, 3 in the colorless pool). We build the archetype the base game only teased — great modding pitch.

- **Rules text:** Quests are **Unplayable. Retain.** While in your hand they track an objective. When the objective completes, the Quest **exhausts itself and grants its Reward** (this counts as "completing a Quest" for other triggers).
- **Drawback:** a Quest is a dead hand-slot until you finish it. That's the cost; rewards are above-rate to compensate. Uncompleted Quests simply vanish at end of combat — no penalty, no feel-bad.
- Quests mostly reward **EXP** (feeding pillar 1) plus a small kicker.
- The token pool below is deliberately **open-ended**: Job Board / Guild Reception / Guild Master offer *choose 1 of 3 from the full pool*, so new Quests added later widen variety without costing any of the 88 card slots.

**Quest token pool** (added by cards/relics, not part of the 88):

- [ ] **Slay** — *Kill an enemy.* → Reward: 6 EXP, draw 1.
- [ ] **Guard Duty** — *Gain 12+ Block in a single turn.* → 5 EXP, gain 5 Block.
- [ ] **Combo Chain** — *Play 4+ cards in a single turn.* → 5 EXP, gain 1 Energy.
- [ ] **Flawless** — *End your turn having taken no unblocked damage.* → 4 EXP, heal 2.
- [ ] **Critical Blow** — *Deal 15+ damage with a single hit.* → 5 EXP, gain 2 Vigor.
- [ ] **Spellcaster** — *Play 3+ Skills in a single turn.* → 4 EXP, gain 3 Block.
- [ ] **Hoarder** — *End your turn with 6+ cards in hand.* → 4 EXP, draw 1.
- [ ] **Boss Slayer** *(only from Main Quest effects)* — *Kill an Elite or Boss.* → Level Up ×3, heal 8.

### The synergy triangle

```
        QUESTS ──complete──▶ EXP/LEVEL
           ▲                    │
   dead hand-slots        thresholds unlock
   need skipping          Exploit conditions
           │                    ▼
   EXPLOIT stacks ◀──skip── unmet conditions
```

Every pair also gets explicit bridge cards (marked **[bridge]** below), per the Time's-Up/Devour-Life pattern.

---

## 4. Character sheet

- **Name:** Isekai Hero (implemented id `ISEKAIHERO-ISEKAI_HERO`) · **HP:** 70 ✅ (as implemented — ties Silent) · **Energy:** 3 · **Color:** purple `#6C3082` ✅ (see §13 — possible clash with Necrobinder's palette) · **Energy icon:** a floating menu-cursor diamond
- **Story blurb:** *"Died on a crosswalk. Woke up with a status screen. The Spire's rules are just code — and nobody patched it."* (Tie-in: summoned by a very bored Neow, who is canonically an Ancient in STS2.)
- **Starting relic:** The System (§7) — **replaces the alpha's placeholder Veil of the Unseen** (heal 3 at combat start); its combat-start hook code is reusable.
- **Starter deck (10):** 4× Strike ✅ · 4× Defend ✅ · 1× Grind ✅ · 1× Stat Check ✅

---

## 5. Card list — 88 cards

Format: `Name — cost · type · effect (upgrade) · art`. Art = stylized homage scene (see §11 on IP).
**Legend:** ✅ = already implemented in the alpha (`IsekaiHeroCode/Cards/*.cs`) — needs only the Exploit-clause wording/localization port and any noted tweak. Checkboxes track the *new-framework* implementation.
Baselines respected: 1⚡ ≈ 6 dmg / 5 Block; Exploit-clause cards run under-rate base / over-rate met.

### 5.1 Basics (4)

- [x] **Strike** ✅ — 1⚡ · Attack · Deal 6 damage. *(U: 9)* · Art: nervous first swing at a slime (Grimgar vibes)
- [x] **Defend** ✅ — 1⚡ · Skill · Gain 5 Block. *(U: 8)* · Art: arms crossed behind a battered wooden shield
- [x] **Grind** ✅ — 1⚡ · Attack · Deal 6 damage. Gain 2 EXP. *(U: 8 dmg, 3 EXP)* · Art needed: field of low-level slimes at sunrise
- [x] **Stat Check** ✅ — 1⚡ · Skill · Gain 4 Block. Exploit (Level 2+): gain 4 more. *(U: 5/+5)* · Art needed: a guild-issued status plate snapping open on the wrist (*Log Horizon* vibes — the Spider-style system window is taken by System Menu's existing art)

### 5.2 Commons (20 — 10 Attacks / 10 Skills)

*Job: teach the three pillars with simple, honest cards. Heavy on EXP riders and easy Exploit conditions.*

**Attacks**

- [ ] **Mob Hunt** — 1⚡ · Deal 8. Fatal: gain 4 EXP. *(U: 11, 6 EXP)* · Art: giant toad hunt (*KonoSuba*)
- [ ] **Underdog Spirit** — 1⚡ · Deal 5. Exploit (Level 3+): deal 5 more. *(U: 6/+7)* · Art: child prodigy's wooden-sword drills (*Mushoku Tensei*)
- [ ] **Beginner Magic** — 1⚡ · Deal 5. Exploit (you played a Skill this turn): deal 4 more. *(U: 6/+6)* · Art: first wobbly firebolt
- [ ] **Shield Bash** — 1⚡ · Deal 6. Exploit (you have Block): gain 3 Block. *(U: 8/+4)* · Art: shield-first counterattack (*Shield Hero*)
- [ ] **Twin Blades** — 1⚡ · Deal 3 twice. Exploit (Level 4+): deal 3 a third time. *(U: 4×)* · Art: dual-wield silhouette (*SAO*)
- [ ] **Steal** — 1⚡ · Deal 6. Gain 4 Gold. *(U: 9, 6 Gold)* · Art: a certain scummy adventurer's signature move (*KonoSuba*)
- [ ] **Farm the Field** — 2⚡ · Deal 4 to ALL. Gain 2 EXP. *(U: 6, 3 EXP)* · Art: AoE spell over a monster field, EXP numbers popping
- [ ] **Last-Hit Bonus** ✅ — 1⚡ · Deal 8. Fatal: draw 1 and gain 1 Energy. *(U: 11, draw 2)* · Art: the kill-credit popup every MMO player fights over
- [ ] **Boss Telegraph** ✅ — 1⚡ · Deal 6. Exploit (an enemy intends to Attack): gain 5 Block. *(U: 8; when triggered, also draw 1)* · Art: glowing red AoE marker on the floor
- [ ] **Tutorial Sword** ✅ — 1⚡ · Deal 7. Exploit (you have a Job): deal 4 more. *(U: 9/+6)* · Art: the starter blade every summoned hero outgrows (*SAO* tutorial plaza)

**Skills**

- [ ] **Daily Training** — 1⚡ · Gain 5 Block. Gain 1 EXP. *(U: 7, 2 EXP)* · Art: pre-dawn practice montage (*Mushoku Tensei*)
- [ ] **Study the System** — 0⚡ · Gain 2 EXP. *(U: 3 EXP)* · Art: scrolling through a skill menu mid-dungeon (*So I'm a Spider*)
- [ ] **Job Board** — 1⚡ · Choose 1 of 3 Quests and add it to your hand. Draw 1. *(U: also gain 1 EXP)* · Art: corkboard of bounty posters at the guild (*Log Horizon*)
- [ ] **Game Knowledge** — 1⚡ · Gain 1 Exploit. Draw 1. *(U: 2 Exploit)* · Art: smug gamer grin in a fantasy tavern (*No Game No Life*)
- [ ] **Seen It Coming** ✅ — 1⚡ · Gain 6 Block. Exploit (an enemy intends to Attack): apply 1 Weak. *(U: 8, 2 Weak)* · Art: sidestep begun before the swing starts (*Cautious Hero*)
- [ ] **Emergency Dodge** — 0⚡ · Gain 3 Block. Exploit (Level 3+): gain 3 more. *(U: 4/+4)* · Art: undignified but effective flailing dive (*Re:Zero*)
- [ ] **Side Quest** — 0⚡ · Add a random Quest to your hand. Gain 1 EXP. *(U: 2 EXP)* · Art: villager with an exclamation mark over their head
- [ ] **Status Appraisal** ✅ — 0⚡ · Look at the top 3 cards of your draw pile. Put one into your hand and discard the others. *(U: top 5)* · Art: appraisal window over a suspicious potion (*Tensura*)
- [ ] **Item Box** ✅ — 1⚡ · Gain 7 Block. Choose a card in your hand and Retain it. *(U: 10 Block, up to 2 cards)* · Art: pulling tomorrow's answer out of hammerspace
- [ ] **Route Guide** ✅ — 1⚡ · Gain 5 Block. Look at the top 4 cards of your draw pile. Put one on top and the rest on the bottom. *(U: 7 Block; up to 2 on top in any order)* · Art: a walkthrough for a world that shouldn't have one

### 5.3 Uncommons (36 — 13 Attacks / 14 Skills / 9 Powers)

*Job: the build-arounds and the bridges. This is where archetypes fork: Level-scaling, Exploit engine, Quest engine.*

**Attacks**

- [ ] **Growth Slash** — 1⚡ · Deal damage equal to 3× your Level. *(U: 4×)* · Art: sword swing leaving a level-up light trail (*SAO*)
- [ ] **Overkill** — 2⚡ · Deal 14. Fatal: gain 8 EXP. *(U: 18, 10 EXP)* · Art: artillery spell vaporizing one goblin (*Tanya the Evil*)
- [ ] **Objective Cleared** — 1⚡ · Deal 9. Exploit (you completed a Quest this turn): draw 2. *(U: 12)* · **[bridge: Quest→tempo]** · Art: "QUEST COMPLETE" banner mid-swing
- [ ] **Combo Rush** — 1⚡ · Deal 4 twice. Exploit (Level 4+): deal 4 a third time. *(U: 5×)* · Art: flurry with afterimages
- [ ] **Raid Opener** — 2⚡ · Deal 15. Exploit (target at full HP): deal 8 more. *(U: 18/+10)* · Art: 24-player raid's first strike (*Log Horizon*)
- [ ] **Duel** — 1⚡ · Deal 8. Exploit (only one enemy remains): deal 6 more. *(U: 10/+8)* · Art: arena duel before a roaring crowd (*Overlord*)
- [ ] **Cross-Class Combo** — 1⚡ · Deal 6 damage once for each different card type you played this turn before this card. *(U: count this card too)* · Art: sword, spell, and scroll in one motion — multiclassing is technically illegal
- [ ] **Cleave the Horde** — 2⚡ · Deal 8 to ALL. Whenever this kills an enemy, gain 3 EXP. *(U: 11)* · Art: one swing, a dozen EXP popups
- [ ] **Counter Read** — 1⚡ · Deal 7. Exploit (an enemy intends to Attack): apply 2 Weak. *(U: 9, 3 Weak)* · Art: stepping inside a telegraphed swing
- [ ] **Killing Blow** — 1⚡ · Deal 6. Fatal: Level Up. *(U: 9)* · **[bridge: kill→Level]** · Art: finishing strike dissolving a boss into light
- [ ] **Skill Chain** — 2⚡ · Deal 5 three times. *(U: 6×)* · Art: system-assisted sword-skill combo, edges glowing
- [ ] **Monster Grinding** — 1⚡ · Deal 10. Fatal: permanently increase this card's damage by 3. *(U: 13, +4)* · Art: evolution menu after the hundredth kill (*So I'm a Spider*) — run-persistent scaling, see §13
- [ ] **Steal Technique** — 1⚡ · Deal 7. Exploit (target has a debuff): gain 2 EXP. *(U: 9, 3 EXP)* · Art: copying an enemy skill into the menu (*Shield Hero*)

**Skills**

- [ ] **Read the Code** — 1⚡ · Gain 2 Exploit. *(U: 3)* · Art: the world dissolving into green glyphs
- [ ] **Guild Reception** — 1⚡ · Gain 6 Block. Choose 1 of 3 Quests and add it to your hand. *(U: 8 Block)* · Art: beaming guild receptionist stamping paperwork
- [ ] **Cheat Inventory** — 1⚡ · Choose a card in your hand. Add a copy of it to your hand. Exhaust. *(U: no Exhaust)* · Art: pulling a duplicate sword out of thin air (*Tensura* Great Sage vibes)
- [ ] **Map Hack** — 0⚡ · Draw 2, then discard 1. *(U: draw 3)* · Art: minimap revealing every hidden room
- [ ] **Save Scum** — 1⚡ · Discard your hand. Draw that many cards. *(U: draw 1 more)* · Art: the same hallway, the seventh attempt (*Re:Zero*)
- [ ] **Barrier Magic** — 2⚡ · Gain 13 Block. Exploit (Level 4+): gain 5 more. *(U: 15/+6)* · Art: layered hexagonal ward
- [ ] **Healing Circle** — 1⚡ · Heal 3. Exploit (Level 6+): heal 6 instead. Exhaust. *(U: 4/8)* · Art: warm green glyph underfoot (*KonoSuba*)
- [ ] **Level Grinding** — 2⚡ · Gain 6 EXP. *(U: 8)* · Art: a montage of 400 identical slime kills
- [ ] **Negotiation** — 1⚡ · Apply 2 Weak. Gain 1 EXP. *(U: 3 Weak, 2 EXP)* · Art: noble villainess talking her way out of a doom flag (*My Next Life as a Villainess*)
- [ ] **Applied Physics** — 1⚡ · Gain 8 Block. The next Attack you play this turn ignores Block. *(U: 11 Block; draw 1)* · Art: explaining leverage to a knight, moments before demonstrating it (*Bookworm* energy)
- [ ] **Side Story** — 0⚡ · Add a random Quest to your hand. Gain 1 Exploit. *(U: choose 1 of 3 Quests)* · **[bridge: Quest×Exploit]** · Art: a stranger's subplot becoming your problem
- [ ] **Power-Up Montage** — 2⚡ · Gain 4 EXP and 4 Block. *(U: 5/6)* · Art: waterfall meditation, split-screen training cuts
- [ ] **Death Flag** — 1⚡ · Apply 2 Vulnerable. Gain 1 EXP. *(U: 3 Vulnerable)* · Art: bandit boasting he'll "end this in one hit" (he won't) — our one Vulnerable card
- [ ] **Dodge the Bad End** — 1⚡ · Choose a card in your discard pile. Put it on top of your draw pile. It costs 0 next turn. *(U: put it into your hand instead; it costs 0 this turn)* · Art: frantically steering away from the doomed route (*Villainess*)

**Powers**

- [ ] **Genre Savvy** — 1⚡ · At the start of your turn, gain 1 Exploit. *(U: Innate)* · Art: hero side-eyeing an obvious mimic chest
- [ ] **Quest Log** — 1⚡ · Whenever you complete a Quest, draw 2. *(U: also gain 2 Block)* · Art: an immaculately organized journal (*Log Horizon*)
- [ ] **Training Arc** — 1⚡ · At the start of your turn, Exhaust a card from your hand and gain 2 EXP. *(U: 3 EXP)* · **[the off-philosophy card — Ironclad grammar, patches our hand-clog weakness, Tyranny-style]** · Art: burning yesterday's techniques to forge better ones · *Note: the alpha's "Training Arc" was an unrelated common Attack, now dropped (§14) — the name belongs to this power.*
- [ ] **Fast Learner** — 1⚡ · Whenever one of your Exploit conditions is met, gain 1 EXP. *(U: 0⚡)* · **[bridge: Exploit→Level]** · Art: skill notifications stacking faster than they can be read
- [ ] **Job: Alchemist** ✅ — 1⚡ · The first time each turn you apply a debuff, gain 4 Block and your next Attack against that enemy deals 4 more damage. *(U: 6/6)* · Art: transmutation circle mid-brawl (*Arifureta* crafting-into-combat)
- [ ] **Job: Spellblade** — 1⚡ · Once each turn, after you play a Skill, your next Attack this turn costs 1 less and deals 4 more damage. *(U: +7)* · Art: spell wrapped around a blade edge
- [ ] **Job: Appraiser** — 1⚡ · At the start of your turn, look at the top 3 cards of your draw pile and put them back in any order. *(U: you may put one on the bottom)* · Art: the world's stats, always visible
- [ ] **Skill Tree** — 1⚡ · Level Ups grant 2 additional Vigor. *(U: 3)* · Art: constellation of unlocked nodes
- [ ] **Mana Sense** — 1⚡ · Whenever you consume an Exploit, draw 1. *(U: also gain 2 Block)* · Art: seeing the seams in reality

### 5.4 Rares (26 — 9 Attacks / 9 Skills / 8 Powers)

*Job: capstones and iconic anime moments — the goal doc's "Cheat Skills" pillar. Every rare should make someone screenshot their hand.*

**Attacks**

- [ ] **EXPLOSION!** — 3⚡ · Deal 28 to ALL. You cannot play Attacks next turn. *(U: 36)* · Art: one glorious detonation, caster face-down in the dirt (*KonoSuba* — the obvious one)
- [ ] **I Am Atomic** ✅ — 3⚡ · Deal 36 to ALL. Costs 1 less this combat whenever an Exploit condition is met. Exhaust. *(U: apply 1 Vulnerable to ALL enemies first)* · **[bridge: Exploit→finisher]** · Art: a black-caped figure whispering something chuunibyou (*Eminence in Shadow*)
- [ ] **Starburst Stream** — X⚡ · Deal 4 damage X+2 times. *(U: 5)* · Art: sixteen-hit sword skill, screen full of light trails (*SAO*)
- [ ] **Degenerate Tactics** — 1⚡ · Deal 8. Apply 2 Weak. Gain 8 Gold. *(U: 10, 10 Gold)* · Art: winning as dishonorably as physically possible (*KonoSuba*)
- [ ] **Hero's Judgment** — 2⚡ · Deal 16. Exploit (Level 7+): deal 32 instead. *(U: 20/40)* · Art: holy sword at full charge, cape physics at maximum
- [ ] **Anti-Boss Art** — 2⚡ · Deal 20. Exploit (target is an Elite or Boss): deal 10 more. *(U: 24/+12)* · Art: raid-wide buff timers aligning on the final phase
- [ ] **Ultimate Skill: Sage** — 2⚡ · Deal 12. Gain 2 Exploit. *(U: 15, 3 Exploit)* · **[bridge: damage×Exploit]** · Art: calm blue analysis text over a chaotic battlefield (*Tensura*)
- [ ] **Megiddo** ✅ — 2⚡ · Deal 18. Exploit (you played a Power this turn): deal 9 to ALL enemies. *(U: 24/12)* · Art: pillar of light called down with theatrical excess (*Overlord*)
- [ ] **Grand Finale** — 3⚡ · Deal damage equal to 10 plus all EXP you gained this combat. Exhaust. *(U: 15 plus)* · Art: every technique learned this arc, used at once

**Skills**

- [ ] **Sequence Break** — 1⚡ · Complete a Quest in your hand. Draw 1. *(U: 0⚡)* · **[bridge: Exploit-philosophy×Quest]** · Art: walking through a wall the developers forgot to finish
- [ ] **Checkpoint** — 1⚡ · Exhaust. The next time you would die this combat, instead heal 15 HP and gain 8 EXP. *(U: 20 HP, 10 EXP)* · Art: waking up at the save point, memories intact (*Re:Zero*) · *Salvage: the alpha's `ReturnByDeathPower.cs` state-tracking is a starting point (§14).*
- [ ] **System Menu** ✅ — 2⚡ · Choose a card in your hand. Add **Override** to it for the rest of combat. Exhaust. *(U: may choose from your discard pile instead)* · **[the per-card permanent cheat]** · Art: dragging enemy stats into the trash
- [ ] **Goddess's Blessing** — 2⚡ · Heal 8. Exploit (Level 6+): heal 14 instead. Exhaust. *(U: 10/17)* · Art: divine light, smug goddess demanding gratitude
- [ ] **Perfect Preparation** — 2⚡ · Gain 15 Block. Exploit (you have a Quest in your hand): gain 10 more. *(U: 18/+12)* · Art: 47 contingency plans, laminated (*Cautious Hero*)
- [ ] **Reincarnate** — 2⚡ · Level Up twice. Exhaust. *(U: three times)* · Art: the glowing circle, the new sky, the second chance
- [ ] **Party Formation** — 1⚡ · Choose 2: deal 8 damage / gain 8 Block / gain 4 EXP. *(U: all three)* · Art: dysfunctional four-person party, somehow functional
- [ ] **Full Clear** — 3⚡ · Complete ALL Quests in your hand. *(U: 2⚡)* · Art: 100% completion screen, every sidequest ticked
- [ ] **Slow Life** — 1⚡ · Heal 3 and gain 6 Block. Exhaust. *(U: 4/8)* · Art: farming isekai — turnips, sunshine, zero urgency (*Farming Life in Another World*)

**Powers**

- [ ] **OP Protagonist** — 3⚡ · Your Exploit conditions always count as met. *(U: 2⚡)* · **[the capstone — Override on everything]** · Art: enemies checking the hero's stats and quietly leaving
- [ ] **Break the Level Cap** — 2⚡ · Remove your Level cap. When you Level Up, gain 1 Strength. *(U: also 1 Dexterity every 2nd Level Up)* · Art: the number 10 shattering like glass
- [ ] **Guild Master** — 2⚡ · At the start of your turn, add a random Quest to your hand. Quests grant double EXP. *(U: choose 1 of 3)* · Art: the desk where every adventurer's story starts
- [ ] **Grinding Montage** ✅ — 2⚡ · At the start of your turn, upgrade 1 random Attack or Skill in your hand for this combat. If it's already upgraded, reduce its cost by 1 this turn. *(U: 2 cards)* · Art: the training episode, permanently
- [ ] **Mana Overflow** — 2⚡ · While you are Level 5 or higher, gain 1 additional Energy at the start of your turn. *(U: Level 4+)* · Art: mana circuits glowing through skin
- [ ] **Plot Armor** — 2⚡ · The first time you would take unblocked damage each turn, reduce it by your Level. *(U: first two times)* · **[bridge: Level→defense]** · Art: the blade that stops exactly one millimeter short
- [ ] **Legend in the Making** — 2⚡ · Whenever a non-minion enemy dies, Level Up. *(U: also draw 1)* · Art: bards already writing the song mid-battle
- [ ] **Protagonist Privilege** — 2⚡ · Once each turn, when one of your Exploit conditions is met, repeat that clause's effect. *(U: twice)* · **[the other capstone — Exploit doubling, from the goal doc]** · Art: the rules apply to everyone else

### 5.5 Ancient cards (2 — full-art specials, obtainable only from Ancients)

- [ ] **Truck-kun** ✅→rework — 3⚡ · Attack · Deal 28 damage to ALL enemies. Whenever this kills an enemy, gain 1 Energy and Level Up. Exhaust. *(U: 36)* · Art: **existing alpha art — reuse** (adapt to the Ancient full-art frame) · *Merge note: promoted from the alpha's 2⚡ uncommon (15 ALL, kill→energy) to Ancient — the genre's inciting incident deserves the full-art slot. The NAME is fixed (art exists); the effect is fair game to retune freely in playtests.*
- [ ] **NEW GAME+** — 2⚡ · Power · At the start of your turn, gain 2 EXP. When you Level Up, gain 1 Exploit. *(U: 3 EXP)* · **[the triangle in one card: Level engine that feeds Exploit]** · Art: title screen with a save file that remembers everything

**Set totals:** 35 Attacks / 35 Skills / 18 Powers — matches the verified STS2 envelope (Necrobinder is exactly 35/35/18).

---

## 6. Archetypes this pool supports

1. **Level Rush** (Grind, Level Grinding, Killing Blow, Reincarnate → Growth Slash, Hero's Judgment, Mana Overflow, Break the Level Cap) — pure snowball; the "become the demon lord" run.
2. **Cheat Engine** (Game Knowledge, Read the Code, Genre Savvy, Ultimate Skill: Sage → heavy Exploit-clause cards + Mana Sense, System Menu, I Am Atomic, OP Protagonist, Protagonist Privilege) — consistency deck; every card always at max text.
3. **Quest Completionist** (Job Board, Guild Reception, Side Story → Quest Log, Sequence Break, Full Clear, Guild Master) — engine deck; converts hand-slots into card advantage and EXP bursts.
4. **Fatal Sweeper** (Mob Hunt, Last-Hit Bonus, Cleave the Horde, Overkill, Monster Grinding, Legend in the Making, Truck-kun) — multi-enemy specialist; sequencing kills for chain level-ups.

Every archetype touches at least two pillars; no card is dead in a neighboring archetype — that's the internal-synergy bar the base characters set. (These four are the goal doc's "acceptance check" run types, reified.)

---

## 7. Relics (9 — verified tier template: Starter + upgraded Starter + 1C + 2U + 3R + 1 Shop)

- [x] **The System** ✅ *(Starter)* — Enemies grant 3 EXP when they die (minions excluded, matching Fatal rules). Start each combat with 2 EXP. · Art needed: the blue window only you can see · *Replaced alpha placeholder Veil of the Unseen.*
- [ ] **The System: Admin Mode** *(Ancient-upgraded Starter)* — Enemies grant 4 EXP when they die. Start each combat with 6 EXP. When you Level Up, draw 1. · Art: the same window, now with a password field left blank
- [ ] **Beginner's Luck Charm** *(Common)* — Your first unmet Exploit condition each combat counts as met. · Art: a four-leaf clover in a smartphone case
- [ ] **OP Smartphone** *(Uncommon)* — Whenever you Level Up, deal 5 damage to a random enemy. · Art: it has no signal and it doesn't matter (*In Another World With My Smartphone*)
- [ ] **Quest Board** *(Uncommon)* — At the start of each combat, add a random Quest to your hand. · Art: portable corkboard, suspiciously well-stocked
- [ ] **Forbidden Walkthrough** *(Rare)* — At the start of each combat, gain 3 Exploit. · Art: a strategy guide for a world that shouldn't have one
- [ ] **Hero's Insignia** *(Rare)* — Whenever you Level Up, gain 1 Strength. · Art: the royal crest they hand out with the summoning
- [ ] **Return by Death** *(Rare)* — When you would die, instead heal to 30% of your max HP and gain 10 EXP and 3 Exploit. Once per run. · Art: the smell of the loop (*Re:Zero* — the Lizard Tail slot, but you come back *stronger and knowing more*) · *The alpha's Return by Death rare **skill** is dropped; the name and fantasy live here (§14).*
- [ ] **Reborn Vending Machine** *(Shop)* — Whenever you Level Up, gain 5 Gold. · Art: it fell into another world and it's thriving (*Reborn as a Vending Machine*)

---

## 8. Potions (3 — one per rarity, verified template)

- [ ] **Jar of Slime** *(Common)* — Gain 6 EXP. · Art: it's friendly and it's delicious EXP (*Tensura*)
- [ ] **Bottled Cheat Code** *(Uncommon)* — Gain 3 Exploit. · Art: fizzing liquid full of tiny glyphs
- [ ] **Truck Summoning Ritual** *(Rare)* — Deal 25 damage to a random enemy. Fatal: Level Up twice. · Art: chalk circle, tire tracks — the random target is the joke

---

## 9. Balance guardrails

- [ ] Exploit-clause pricing: base ≈ 15% under rate, met ≈ 15% over rate; the *average* assuming ~60% natural trigger rate should sit exactly at rate.
- [ ] Exploit is budgeted at ~0.5⚡ per stack (Game Knowledge: 1⚡ = 1 Exploit + draw 1 is the anchor).
- [ ] EXP is budgeted at ~3 EXP per 1⚡ when it's the whole card (Level Grinding), ~1–2 EXP as a rider.
- [ ] Level curve targets: hallway fight ends ~Level 3–4, elite ~5–6, boss ~8–10. If playtests exceed this, raise the 4-EXP threshold to 5, not the card numbers.
- [ ] Anti-infinite checks: Level cap 10 gates Growth Slash/Hero's Judgment; Strength-per-Level lives only on rares (Hero's Insignia, Break the Level Cap); Mana Overflow is level-gated energy, strictly worse than Ancient-tier energy relics early.
- [ ] Anti-frustration: Exploit never consumed on met conditions; uncompleted Quests vanish silently at combat end; Level Up Vigor means EXP riders never feel like dead text.
- [ ] The Regent test: audit every setup card — each must produce damage, Block, or a draw the turn it's played. (Level Grinding and Study the System are the two allowed pure-setup exceptions, both cheap.)
- [ ] Capstone stacking: OP Protagonist + Protagonist Privilege + Fast Learner is the intended "engine ascension"; make sure repeated clauses (Privilege) don't double EXP triggers into a runaway (Fast Learner reads *met*, Privilege *repeats effects* — repeats should not re-fire "condition met" events).

## 10. Cross-class synergy (the "could I draft this?" test)

- **They'd want ours:** Training Arc (Ironclad would kill for turn-start exhaust), Status Appraisal/Map Hack (Silent-grade card quality), Seen It Coming/Negotiation (any defensive deck), Steal/Degenerate Tactics (gold-gen is universal).
- **We'd want theirs:** Silent draw velocity (more Exploit triggers per turn); Ironclad's Vulnerable package multiplies Hero's Judgment; Regent's colorless generation feeds Cross-Class Combo and Quest-turn card counts; Necrobinder's Souls cantrips accelerate our engine assembly.
- **Safe cross-pool rule:** an Exploit-clause card in a non-IsekaiHero deck simply plays at base rate — never dead, never broken.
- **Compat patch dividend:** once curated base-game conditionals carry Exploit tags (§3.2), Bottled Cheat Code and Forbidden Walkthrough become draftable value for *any* class in shared-pool modes.

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

## 12. Implementation roadmap

The stack is **C# on BaseLib-StS2** (not the raw GDScript loader): cards subclass `IsekaiHeroCard`, localization lives in `IsekaiHero/localization/eng/*.json`, build with `dotnet build` (see `AGENTS.md` for ILSpy decompile workflow and card-text conventions).

- [ ] **Phase 1 — Resource core:** EXP/Level player buffs + Level-Up Vigor + **The System** starter relic (replace Veil of the Unseen) + Grind & Stat Check basics + trim starter deck to 4/4+2. *Code complete (LevelPower/TheSystem/Grind/StatCheck) — written on macOS without the game DLLs, so it needs a Windows `dotnet build` + in-game check. Exit criterion: a full Act 1 run where leveling visibly happens.*
- [ ] **Phase 2 — Exploit formalization:** extend the existing `HasConditionalEffects`/`IsConditionalEffectActive` hook with the stacking **Exploit** buff (consume-on-unmet), keep **Override** as the permanent state, print all conditions in `Exploit (…)` wording, build the shared condition library (one checker, not per-card logic). Port the 14 alpha cards to the new wording. Audit base-game cards for the compat tag list.
- [ ] **Phase 3 — Quests:** Quest token type (Unplayable/Retain/objective tracking/exhaust-on-complete), the 8-token pool, Job Board & choose-1-of-3 UI (reuse `CardSelectCmd.FromSimpleGrid`).
- [ ] **Phase 4 — Full pool:** all 88 cards, 9 relics, 3 potions, 2 Ancient cards wired to Ancient encounters; Job cycle (Spellblade, Appraiser — Alchemist exists).
- [ ] **Phase 5 — Balance & release:** §9 targets, Ascension scaling, art pass, Workshop + Nexus release; retire alpha card texts.

## 13. Open questions (for future sessions)

- [ ] Does the STS2 mod API expose Ancient-encounter reward pools (needed for the 2 Ancient cards + Admin Mode upgrade)?
- [ ] Should EXP-on-kill live on the character (safe from relic loss) or on The System relic (matches Bound Phylactery precedent)? Currently: relic.
- [ ] The clause and the buff share one name (Exploit). If playtests show players think a clause *requires* the buff, rename the **buff** to **Cheat** (genre-perfect: "Gain 2 Cheat") and keep Exploit on cards. Override stays either way.
- [ ] Which base-game conditionals make the initial Exploit compat list? Audit during Phase 2.
- [ ] Level-Up intrinsic bonus: 2 Vigor (current) vs 2 Block (defensive) vs nothing (pure card-driven) — playtest.
- [ ] **Color:** implemented `#6C3082` purple may read as Necrobinder-adjacent; consider shifting toward teal/cyan ("another world" portal palette) during the art pass.
- [ ] **Monster Grinding** run-persistent damage: the goal doc flagged save-data complexity; keep only if per-card persistent state serializes cleanly, otherwise change to "this combat."
- [x] **Jobs scope — decided (2026-07-02):** Jobs stay a 3-power cycle + the "you have a Job" condition, with **no Level integration** — tying Job effects to Level would stack two scaling systems on one power and blow the complexity budget; the Exploit condition is already the bridge. Job: Alchemist's existing art stays. If players love Jobs, grow to ~5 powers and 2–3 more payoffs — never into a 4th core mechanic.
- [ ] Give the hero an in-world name/portrait identity, or keep the anonymous "Isekai Hero" genre-blank? Currently: anonymous.

## 14. Alpha merge ledger (v0.4.0-alpha → this design)

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
