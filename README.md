# 🎰 Slot Machine Game

A polished 3-reel slot machine game built in **Unity** for the *Unity Slot Game Assignment*. It features smooth reel animations, RNG-based outcomes, a payout system, a coin-rain win celebration, an animated balance counter, and a full audio layer.

| | |
|---|---|
| **Play in browser** | [niharikaa-agarwal.itch.io/slot-machine](https://niharikaa-agarwal.itch.io/slot-machine) |
| **Source code** | [github.com/niharikaa0124/SlotMachineGame](https://github.com/niharikaa0124/SlotMachineGame) |
| **Engine** | Unity 6.3 LTS (6000.3.21f1) |
| **Language** | C# |
| **Author** | Niharikaa Agarwal |

<!--
Add screenshots here once you have them, for example:
![Gameplay](Screenshots/gameplay.png)
![Win screen](Screenshots/win.png)
-->

---

## Table of Contents
1. [Assignment Requirements Checklist](#assignment-requirements-checklist)
2. [How to Play](#how-to-play)
3. [Features](#features)
4. [Payouts](#payouts)
5. [Randomization (RNG)](#randomization-rng)
6. [Animations](#animations)
7. [Audio](#audio)
8. [Code Structure](#code-structure)
9. [Running the Project](#running-the-project)
10. [Building for WebGL](#building-for-webgl)
11. [Credits](#credits)
12. [Future Improvements](#future-improvements)

---

## Assignment Requirements Checklist

| Requirement | Status | How it is implemented |
|---|---|---|
| **Winning logic** (win when all slots match) | ✅ | `GameManager.CheckWin()` compares the result index of all three reels. |
| **Smooth reel animations** | ✅ | `ReelController.Spin()` accelerates, spins at full speed, then decelerates and snaps to the result. Reels start one after another. |
| **Clean symbol display** | ✅ | Each reel is clipped with a `RectMask2D`, so exactly one symbol is visible per reel, aligned to the window. |
| **Randomized outcomes (RNG)** | ✅ | Every spin picks its result with `UnityEngine.Random`, independently for each reel. |
| **Winning combinations & payouts** | ✅ | Each symbol has a payout multiplier applied to the bet. |
| **Bonus features (optional)** | ✅ | Coin-rain celebration, animated balance counter, dynamic music, and a game over / retry flow. |

---

## How to Play

1. Press **SPIN**. Each spin costs **50 coins**.
2. The three reels spin and stop one after another.
3. If **all three symbols match**, you win. The reward is added to your balance.
4. Press **CONTINUE** on the win panel to keep playing.
5. If your balance can no longer cover a bet, the **game over** panel appears. Press **RETRY** to restart with 1000 coins.

You start with **1000 coins**.

---

## Features

- **3-reel slot machine** with a classic jackpot-style look
- **Win detection** when all three reels land on the same symbol
- **Payout multipliers** per symbol
- **Smooth reel motion** with acceleration, full-speed spin, and a slow stop
- **Staggered reel start** for a more realistic feel
- **Coin rain** across the whole screen on a win
- **Animated balance counter** that rolls up to the new total instead of jumping
- **Win panel and game over panel** with continue and retry options
- **Full sound design** (see [Audio](#audio))
- **Dynamic music volume** that fades down while the win and game over panels are open
- **Playable in the browser** through WebGL

---

## Payouts

The reward is the **bet multiplied by the symbol's multiplier**, added to the balance.

| Symbol | Multiplier | Reward (bet of 50) |
|---|---|---|
| Symbol 1 | ×2 | 100 coins |
| Symbol 2 | ×3 | 150 coins |
| Symbol 3 | ×5 | 250 coins |
| Symbol 4 | ×10 | 500 coins |

Symbols are numbered in the order they appear in the `Symbols` array on each reel. Higher-value symbols pay more.

| Setting | Value |
|---|---|
| Starting balance | 1000 coins |
| Bet per spin | 50 coins |

---

## Randomization (RNG)

- Each reel picks its result with `UnityEngine.Random.Range`, so every spin is independent and unpredictable.
- The three reels are chosen separately, and one spin never influences the next.
- Each symbol has an equal chance of landing on every reel. With 4 symbols, the chance of a match on any given spin is **1 in 16 (6.25%)**.
- The strip of symbols that scrolls past during the spin is filled with random symbols too, so the animation looks different every time.

---

## Animations

- **Reel spin:** the movement is split into three phases using time progress: acceleration (first 20%), fast spin (20% to 70%), and deceleration with a smooth step (last 30%). The reel then snaps precisely to its final symbol.
- **Staggered start:** the reels start 0.2 seconds apart.
- **Coin rain:** coins spawn above the top edge at random positions across the whole width, with random size, fall speed, sideways drift, and spin. Gravity makes them speed up as they fall.
- **Balance counter:** after a win, the balance text eases from the old value to the new one, fast at first and slowing near the end.
- All win-screen animations use **unscaled time**, so they keep playing while the game is paused (`Time.timeScale = 0`).

---

## Audio

Sound effects and music were sourced from **Freesound** and **Pixabay**.

| Sound | When it plays |
|---|---|
| Spin button click | When SPIN is pressed |
| Spin loop | While the reels are spinning |
| Win sound | When the player wins |
| Coin drop | When the coin rain starts |
| Counting loop | While the balance number rises |
| Continue click | When CONTINUE is pressed |
| Lose sound | When the player runs out of points |
| Background music | Loops all the time. Fades down while the win and game over panels are open, then returns to normal volume. |

---

## Code Structure

All scripts are in `Assets/Scripts`.

| Script | Responsibility |
|---|---|
| `GameManager.cs` | Controls the game flow: spinning the reels, checking the win, calculating payouts, updating and animating the balance, showing the win and game over panels, and managing all audio. |
| `ReelController.cs` | Builds the strip of symbols at runtime, spins it with the three-phase animation, and picks the random result. Exposes `ResultIndex` and `IsSpinning` to the game manager. |
| `CoinShower.cs` | Creates the coin rain effect on a win. |

**How the scripts work together:** `GameManager` starts the `Spin()` coroutine on each reel, waits until every reel reports it has stopped, then compares the `ResultIndex` of all three. On a win it plays the sounds, starts `CoinShower`, and animates the balance.

**Code practices used:**
- Header comments describing each script
- `#region` blocks to group related code
- Named constants instead of magic numbers
- Small, single-purpose methods
- Inspector fields grouped with `[Header]` attributes

---

## Running the Project

**Requirements:** Unity Hub and Unity **6.3 LTS (6000.3.21f1)**.

1. Clone the repository:
   ```bash
   git clone https://github.com/niharikaa0124/SlotMachineGame.git
   ```
2. Open Unity Hub, click **Add > Add project from disk**, and select the cloned folder.
3. Open the project with the version above. Unity will import the assets, which can take a few minutes the first time.
4. Open the scene at `Assets/Scenes/SampleScene`.
5. Press **Play**.

---

## Building for WebGL

1. Install **Web Build Support** for your Unity version through Unity Hub.
2. Go to **File > Build Profiles**, select **Web**, and click **Switch Platform**.
3. In **Player Settings > Publishing Settings**, set **Compression Format** to **Disabled**.
4. Click **Build** and choose an empty folder.
5. Zip the contents of the build folder, so that `index.html` is at the top level.
6. Upload the zip to itch.io as an **HTML** project and tick **This file will be played in the browser**.

> Browsers block audio until the player interacts with the page, so the background music may start only after the first click.

---

## Credits

- **Slot machine art and design:** provided with the assignment.
- **Sound effects and music:** [Freesound](https://freesound.org) and [Pixabay](https://pixabay.com/sound-effects/). Individual sounds are used under their respective licenses.

---

## Future Improvements

- Bonus symbols such as wilds, scatters, and free spins
- Adjustable bet amount
- Multiple paylines
- Mute and volume controls
- Saving the balance between sessions
- Mobile touch layout

---

**Made by Niharikaa Agarwal**
