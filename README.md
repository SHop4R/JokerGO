# JokerGO — Unity Game Developer Case Study

A 3D single-player board game inspired by *Monopoly GO*, *Dice Dreams* and *Board Kings*:
type your dice, watch them tumble in 3D, hop along a JSON-driven orchard path and
collect fruit into a persistent inventory.

> 🎬 **Gameplay video:** _link here_

## Features

| Case requirement | Implementation |
|---|---|
| Inventory (apples, pears, strawberries) | Live HUD panel (top-right) with collect-flight animation and counter punch |
| Persistence | Inventory **and current tile** saved as JSON after every turn and restored on launch |
| 3D map from JSON | `StreamingAssets/map.json` → validated domain model → board built at runtime; edit the file, get a different board |
| Linear map | Straight 24-tile path (any length ≥ 2 works) with numbered tiles and visible rewards |
| Dice input via textboxes | Labeled fields (top-left), integer-only, validated 1-6 with shake + flash + message feedback; an empty box rolls a random value |
| 3D dice animation matching input | Scripted tumble: arc, spin, double bounce, settle **exactly** on the typed faces (6 is underlined so it can't read as 9) |
| Movement + wrap-around | Token hops tile-by-tile (sum of dice), wraps past the last tile; rewards are never consumed |
| Landing log | Bottom strip + console log of tile number and reward |
| Unlimited throws | Input locks only during a turn, then re-opens |
| **PLUS: dice count dropdown** | 1-20 dice; value boxes rebuild dynamically (typed values preserved) in a scrollable list |

## How to run

1. Open the project with **Unity 6000.0.x** (developed on 6000.0.76f1).
2. Open `Assets/Project/Scenes/SampleScene.unity` and press Play
   (the scene self-assembles from a single `GameBootstrap` object).
3. Pick a dice count, type values (1-6 each) and press **ROLL**.

To try a different board, edit `Assets/StreamingAssets/map.json`:

```json
{
  "tiles": [
    { "item": "empty", "amount": 0 },
    { "item": "apple", "amount": 5 },
    { "item": "strawberry", "amount": 15 }
  ]
}
```

`item` is `apple`, `pear`, `strawberry` or `empty`; `amount` must be positive for
reward tiles. Invalid data fails fast with a readable error instead of a broken board.

The save file lives at `<persistentDataPath>/save.json`; delete it for a fresh start.

## Architecture

Three assemblies enforce one-directional dependencies:

```
JokerGO.Core   pure C# rules engine — no UnityEngine reference at all
JokerGO.Game   MonoBehaviour presentation of what Core decides
JokerGO.UI     uGUI HUD: input intents in, session events out
```

- **`GameSession`** (Core) is a small state machine (`Idle → Rolling → Moving → Idle`).
  UI calls `TryRoll(values)`; views animate what the raised events describe
  (`RollStarted`, `MoveStarted`, `TileLanded`, `ItemsCollected`, `TurnEnded`) and
  report back via `NotifyDiceShown` / `NotifyMoveCompleted`. Dice visuals never
  decide outcomes — the validated input is the single source of truth.
- **Data boundaries** are interfaces (`IMapSource`, `ISaveRepository`) with
  file-based implementations injected by the `GameBootstrap` composition root —
  no singletons, no service locators.
- **Immutable domain**: `Inventory.Add` returns a new instance; `BoardMap` and
  `MapTile` never change after validation.
- **In-house tweening** (`Easing` + composable coroutine `Tween` helpers) since
  third-party libraries like DOTween are not allowed; dice tumble, token hops,
  squash-and-stretch, UI shake and collect flights all run on it.
- **Object pooling**: dice and one-shot particles come from a generic
  `Pool<T>`/`PoolManager` (over `UnityEngine.Pool`) instead of
  Instantiate/Destroy churn; pooled objects reset through `IPoolable`.
- **Prefab + material assets** are generated once by an editor command
  (`JokerGO > Generate Art Assets`) and loaded at runtime, so environment
  props are single prefab instances rather than per-prop primitive assembly.
- **Cinemachine camera**: damped follow with a screen-space dead zone (no
  micro-twitch while dice land), impulse-based shake, and a second camera
  that glides home before the token's wrap-around sky drop.
- **Everything else is built from code at runtime** — board, HUD, post
  processing — so the scene stays a one-object composition and the whole game
  is reviewable as C#.

## Juice

Scripted dice with dust and camera shake on impact, tile press-bounce under every
hop, squash-and-stretch landings, fruit burst + chips flying into the inventory
with a counter punch, idle token bob, ambient falling leaves, fog-graded orchard
backdrop, URP bloom/vignette/color grading.

## Tech

Unity 6 (URP), C#, the new Input System (UI events), TextMesh Pro, Git.
No third-party plugins or tween libraries; all visuals are procedural primitives
and code-driven effects.
