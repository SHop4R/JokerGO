<div align="center">

```
     ██╗ ██████╗ ██╗  ██╗███████╗██████╗     ██████╗  ██████╗
     ██║██╔═══██╗██║ ██╔╝██╔════╝██╔══██╗   ██╔════╝ ██╔═══██╗
     ██║██║   ██║█████╔╝ █████╗  ██████╔╝   ██║  ███╗██║   ██║
██   ██║██║   ██║██╔═██╗ ██╔══╝  ██╔══██╗   ██║   ██║██║   ██║
╚█████╔╝╚██████╔╝██║  ██╗███████╗██║  ██║   ╚██████╔╝╚██████╔╝
 ╚════╝  ╚═════╝ ╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝    ╚═════╝  ╚═════╝
```

### `> Monopoly GO-style 3D Board Game`

<img src="https://readme-typing-svg.demolab.com?font=Fira+Code&size=18&duration=3000&pause=1000&color=00D4FF&center=true&vCenter=true&width=520&lines=Type+your+dice.+Watch+them+tumble.;Hop+the+orchard+path.+Collect+fruit.;JSON-driven+board+%E2%80%94+edit+and+replay.;Pure+C%23+core%2C+zero+3rd-party+plugins." alt="Typing SVG" />

<br/>

![Unity](https://img.shields.io/badge/Unity-6000.0.76f1-00d4ff?style=for-the-badge&logo=unity&logoColor=white&labelColor=0a0a12)
![C#](https://img.shields.io/badge/C%23-.NET-7b2fff?style=for-the-badge&logo=csharp&logoColor=white&labelColor=0a0a12)
![URP](https://img.shields.io/badge/URP-17-3DDC84?style=for-the-badge&logo=unity&logoColor=white&labelColor=0a0a12)
![No Plugins](https://img.shields.io/badge/3rd--party_plugins-none-ff2d95?style=for-the-badge&labelColor=0a0a12)

</div>

---

### `// About`

A **3D single-player board game** inspired by *Monopoly GO*, *Dice Dreams* and *Board Kings*, built for the **joker.games** Unity Developer case study. Type your dice values, watch scripted dice tumble and settle on exactly those faces, hop along a JSON-driven orchard path, and collect fruit into a persistent inventory.

Built with **pure C#** domain logic and **Unity 6** presentation, adhering to SOLID principles with **zero third-party plugins** — every visual is a procedural primitive or a code-driven effect.

### `// Gameplay`

https://github.com/user-attachments/assets/c664b00c-0bf0-4ce2-936a-c7f7905948dd


---

### `// Features`

| Case requirement | Implementation |
|:-----------------|:---------------|
| **Inventory** (apples, pears, strawberries) | Live HUD panel with collect-flight animation and counter punch |
| **Persistence** | Inventory **and current tile** saved as JSON after every turn, restored on launch |
| **3D map from JSON** | `StreamingAssets/map.json` → validated domain model → board built at runtime |
| **Linear map** | 24-tile path (any length ≥ 2) with numbered tiles and visible rewards |
| **Dice input via textboxes** | Integer-only fields, validated 1–6 with shake + flash feedback; empty box rolls random |
| **3D dice matching input** | Scripted tumble — arc, spin, double bounce, settle **exactly** on typed faces (6 is underlined) |
| **Movement + wrap-around** | Token hops tile-by-tile (sum of dice), wraps past the last tile; rewards never consumed |
| **Landing log** | Bottom strip + console log of tile number and reward |
| **Unlimited throws** | Input locks only during a turn, then re-opens |
| **➕ Dice count dropdown** | 1–20 dice; value boxes rebuild dynamically (typed values preserved) in a scrollable list |

---

### `// Getting Started`

#### Prerequisites

- **Unity 6000.0.76f1** (or compatible 6000.0.x), with **Universal Render Pipeline**

#### Setup

```bash
# Clone the repository
git clone https://github.com/SHop4R/JokerGO.git

# Open in Unity Hub → Add project from disk
# Wait for asset import & compilation
# Open Assets/Project/Scenes/SampleScene.unity and press Play
```

The scene ships fully authored. `JokerGO > Author Scene` regenerates all prefabs and rewires the scene from scratch if anything is ever missing.

#### Editing the board

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

`item` is `apple`, `pear`, `strawberry` or `empty`; `amount` must be positive for reward tiles. Invalid data fails fast with a readable error instead of a broken board. The save file lives at `<persistentDataPath>/save.json` — delete it for a fresh start.

---

### `// Architecture`

Four assemblies enforce one-directional dependencies:

```
┌──────────────────┐
│  JokerGO.Core    │  pure C# rules engine — NO UnityEngine reference
└──────────────────┘
         ▲
         │              ┌────────────────────┐
         ├──────────────│  JokerGO.Pooling   │  generic Pool<T> over
         │              └────────────────────┘  UnityEngine.Pool (engine-aware)
         │                       ▲
┌──────────────────┐             │
│   JokerGO.UI     │─────────────┤  uGUI HUD: input intents in, events out
└──────────────────┘             │
         ▲                       │
┌──────────────────┐             │
│  JokerGO.Game    │─────────────┘  MonoBehaviour presentation of what Core decides
└──────────────────┘
```

#### GameSession state machine

```
        TryRoll(values)
   Idle ───────────────► Rolling ──NotifyDiceShown──► Moving ──NotifyMoveCompleted──► Idle
                            │            │                │
                       RollStarted   TileLanded     MoveStarted
                                    ItemsCollected    TurnEnded
```

`GameSession` (Core) is the single source of truth. UI calls `TryRoll(values)`; views animate what the raised events describe and report back via `NotifyDiceShown` / `NotifyMoveCompleted`. **Dice visuals never decide outcomes** — the validated input does.

#### Key systems

- **Data boundaries** — interfaces (`IMapSource`, `ISaveRepository`) with file-based implementations injected by the `GameBootstrap` composition root. No singletons, no service locators.
- **Immutable domain** — `Inventory.Add` returns a new instance; `BoardMap` and `MapTile` never change after validation.
- **Object pooling** — the shared **`JokerGO.Pooling`** assembly provides a generic `Pool<T>` over `UnityEngine.Pool`, used by both Game (dice, dust/burst particles via `PoolManager`) and UI (collect-flight chips). Pooled instances stay inactive and have their values assigned **before** activation, then reset through `IPoolable` on spawn/return — no Instantiate/Destroy churn.
- **In-house tweening** — `Easing` + composable coroutine `Tween` helpers (DOTween is not allowed). Dice tumble, token hops, squash-and-stretch, UI shake and collect flights all run on it.
- **Scene-authored composition** — every single-instance object (camera rig, token, HUD canvas, managers, ground) lives pre-configured in the scene, generated by one editor command (`JokerGO > Author Scene`). Only data-driven content spawns at runtime.
- **Cinemachine camera** — damped follow with a screen-space dead zone, impulse-based shake, and a second camera that glides home before the token's wrap-around sky drop.

---

### `// Project Structure`

```
Assets/
├── Project/
│   ├── Scenes/SampleScene.unity          # the only scene in Build Settings
│   ├── Scripts/
│   │   ├── Core/                          # JokerGO.Core   — pure C# rules engine
│   │   ├── Pooling/                       # JokerGO.Pooling — shared Pool<T> / IPoolable
│   │   ├── UI/                            # JokerGO.UI     — code-built uGUI HUD
│   │   ├── Game/                          # JokerGO.Game   — MonoBehaviour presentation
│   │   │   ├── Board/                     #   board build + serpentine layout
│   │   │   ├── Dice/                       #   scripted dice & roll director
│   │   │   ├── Fx/                         #   PoolManager (dust, burst, dice pools)
│   │   │   ├── Tweening/                   #   Easing + coroutine Tween helpers
│   │   │   ├── Data/                       #   JSON map source, file save repo
│   │   │   └── Utils/                      #   MonoSingleton, WaitHelper
│   │   └── Editor/                        # JokerGO.Editor — "Author Scene" menu command
│   ├── Prefabs/  Resources/  Sounds/  Graphics/
│
├── StreamingAssets/map.json               # the data-driven board
├── Settings/                              # URP pipeline + volume profiles
├── Input System/                          # InputSystem_Actions.inputactions
└── TextMesh Pro/                          # TMP essentials
```

---

### `// Tech`

<div align="center">

![Unity](https://img.shields.io/badge/Unity_6-000000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-9B4F96?style=for-the-badge&logo=csharp&logoColor=white)
![URP](https://img.shields.io/badge/URP_17-00d4ff?style=for-the-badge&labelColor=0a0a12)
![Input System](https://img.shields.io/badge/Input_System-7b2fff?style=for-the-badge&labelColor=0a0a12)
![TextMeshPro](https://img.shields.io/badge/TextMeshPro-ff2d95?style=for-the-badge&labelColor=0a0a12)

</div>

Unity 6 (URP), C#, the new Input System (UI events), TextMesh Pro, Unity Test Framework, Git. No third-party plugins or tween libraries — all visuals are procedural primitives and code-driven effects.

---

### `// Author`

<div align="center">

<a href="https://github.com/SHop4R">
  <img src="https://github.com/SHop4R.png" width="80" style="border-radius:50%"><br>
  <strong>Ege Akarsu</strong>
</a>

<br/><br/>

[![Website](https://img.shields.io/badge/Website-00d4ff?style=for-the-badge&logo=googlechrome&logoColor=white&labelColor=0a0a12)](https://egeakarsu.dev)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-0A66C2?style=for-the-badge&logo=linkedin&logoColor=white&labelColor=0a0a12)](https://linkedin.com/in/egeakarsu)
[![Email](https://img.shields.io/badge/Email-ff2d95?style=for-the-badge&logo=maildotru&logoColor=white&labelColor=0a0a12)](mailto:akarsu.ege@gmail.com)

</div>

---

<div align="center">
<img src="https://capsule-render.vercel.app/api?type=waving&color=0:7b2fff,100:00d4ff&height=80&section=footer" width="100%" />
</div>
