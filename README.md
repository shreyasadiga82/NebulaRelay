# ✦ DRIFT CORE: The Last Signal ✦

<div align="center">

![Unity 6](https://img.shields.io/badge/Unity%206%20LTS-6000.0.84f1-blue.svg?style=for-the-badge&logo=unity&logoColor=white)
![Render Pipeline](https://img.shields.io/badge/Render%20Pipeline-Universal%202D%20(URP)-blueviolet.svg?style=for-the-badge&logo=render&logoColor=white)
![Language](https://img.shields.io/badge/Language-C%23%209.0%2B-239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows%20x64-0078D6.svg?style=for-the-badge&logo=windows&logoColor=white)
![Input System](https://img.shields.io/badge/Input-Unity%20Input%20System%20(New)-orange.svg?style=for-the-badge&logo=unity&logoColor=white)
![Automated Tests](https://img.shields.io/badge/Tests-48%2F48%20Passing-brightgreen.svg?style=for-the-badge&logo=pytest&logoColor=white)
![Missions](https://img.shields.io/badge/Campaign-10%20Long%20Missions-cyan.svg?style=for-the-badge&logo=compass&logoColor=white)
![Difficulty](https://img.shields.io/badge/Difficulty-4%20Modes-amber.svg?style=for-the-badge&logo=target&logoColor=white)

<br/>

**A precision zero-gravity 2D sci-fi courier simulator built for Unity 6 LTS.**  
*Pilot through 10 expansive cosmic corridors, thread sequential checkpoint gates, weave through mid-route hazard fields, sling high-energy plasma capsules into receiver arrays, and execute precision soft-docking maneuvers at orbital relay spires across 4 flight difficulties.*

---

[🚀 Quick Start](#-quick-start--installation) •
[🎮 Flight Controls](#-flight-controls--pilot-manual) •
[🌌 Core Mechanics](#-core-gameplay-mechanics) •
[🗺️ Campaign Sectors](#-campaign-sectors) •
[📐 Mathematical Models](#-physics--mathematical-foundations) •
[🧪 Verification Suite](#-automated-verification-suite) •
[📁 Architecture](#-project-architecture)

---

</div>

<br/>

## 📖 Mission Briefing

The outer galactic relay network has collapsed, severing interstellar communication across remote sectors. As the pilot of the high-mobility **Delta Courier Drone**, your directive is to traverse forgotten corridors of deep space, activate sequential holographic checkpoint gates, sling high-energy plasma capsules into receiver arrays, and execute precision soft-docking at each sector's **Relay Station Spire** before battery reserves fail.

In the vacuum of space, **Newtonian inertia rules supreme**: every burst of thruster fire continues indefinitely until counteracted by retro-burns. You must calculate drift, respect momentum, avoid kinetic impacts with rogue asteroid belts, and guide the Last Signal back to life.

---

## 🎮 Flight Controls & Pilot Manual

The courier drone is controlled via keyboard thrust and mouse trajectory targeting using the Unity New Input System:

| Control | Key / Input | Action & Maneuver Notes |
| :--- | :---: | :--- |
| **Main Thruster** | <kbd>W</kbd> / <kbd>↑</kbd> | Fires main engine forward; generates continuous linear momentum |
| **Retro / Reverse** | <kbd>S</kbd> / <kbd>↓</kbd> | Fires retro-thrusters to counter velocity or reverse heading |
| **Port / Starboard Drift** | <kbd>A</kbd> / <kbd>D</kbd> or <kbd>←</kbd> / <kbd>→</kbd> | Applies lateral maneuvering impulses |
| **Aiming System** | <kbd>Mouse Cursor</kbd> | Projects real-time calculated ballistic trajectory line |
| **Launch Capsule** | <kbd>Space</kbd> / <kbd>Left Click</kbd> | Fires a zero-g plasma energy core along the predicted flight path |
| **Emergency Reboot** | <kbd>R</kbd> | Instantly restarts current sector after collision or battery drain |
| **Sector Advance** | <kbd>Space</kbd> / <kbd>Enter</kbd> | Transitions to subsequent mission sector upon relay activation |
| **Exit Mission** | <kbd>Escape</kbd> | Returns to Sector Selection / Main Menu |

> [!TIP]
> **Retro-Braking Doctrine**: Because there is virtually zero atmospheric drag in space ($c_d = 0.2$), your ship will coast on momentum. To slow down for relay docking, tap your thruster in the exact opposite direction of your flight vector, or observe the real-time speedometer in the HUD.

---

## 🌌 Core Gameplay Mechanics

### 1. Calibrated World Scaling & Zero-Override Architecture
All objects are calibrated to exact on-screen world dimensions in their prefabs (no scene overrides):
* **Player Drone**: `~1.20u` length (hull diameter ~1.06u, local scale 0.132)
* **Ring Gate**: `~2.40u` outer diameter (comfortable passage clearance for the drone, local scale 0.26)
* **Relay Station Spire**: `~3.20u` height (activation zone radius 2.2u, local scale 0.335)
* **Gameplay Asteroids**: `1.0–1.8u` diameter (base prefab 1.39u at scale 0.17, varied via runtime multiplier 0.8–1.3)
* **Energy Capsule**: `~0.40u` diameter (local scale 0.057)
* **Energy Receiver**: `~1.50u` diameter (local scale 1.43)
* **Orthographic Camera**: Size `7.5` across all level scenes (`8.0` in Main Menu), guaranteeing full visibility of corridors without void edges.

### 2. Zero-G Inertial Flight Dynamics & Flight Corridor Soft-Clamping
Governed by [DroneController](Assets/_NebulaRelay/Scripts/Player/DroneController.cs) using a pure 2D Newtonian physics simulation:
* **Thrust Rating**: `6.0 u/s²` with smooth directional interpolation towards movement vector.
* **Top Speed Cap**: Strictly capped at `8.0 u/s` to maintain navigational precision.
* **Inertial Coasting**: Subtle drag coefficient ($0.2$) gives the authentic feel of deep-space drift while retaining crisp micro-adjustments.
* **Flight Corridor Soft-Clamping**: The drone is soft-clamped inside the level corridor ($X \in [\text{MinX}, \text{MaxX}]$, $Y \in [-6.5, +6.5]$). When contacting boundary limits, outward velocity is cleanly zeroed and a warning alert is flashed on the HUD.
* **Dynamic Visuals**: Animated thruster trail, rotation smoothing, and kinetic hull feedback handled by [DroneVisuals](Assets/_NebulaRelay/Scripts/Player/DroneVisuals.cs).

### 3. Ballistic Trajectory & Energy Capsule Launcher
Implemented across [CapsuleLauncher](Assets/_NebulaRelay/Scripts/Delivery/CapsuleLauncher.cs), [EnergyCapsule](Assets/_NebulaRelay/Scripts/Delivery/EnergyCapsule.cs), and [TrajectoryPreview](Assets/_NebulaRelay/Scripts/Delivery/TrajectoryPreview.cs):
* **Predictive LineRenderer**: Casts 30 analytical Newtonian sample points showing the exact path the capsule will travel before firing.
* **Single-Slot Protocol**: Only one simultaneous capsule may exist in flight at a time (`0.5s` cooldown), requiring deliberate targeting.
* **Zero-G Projectile Mechanics**: Projectiles travel at `12.0 u/s` with true zero-gravity ballistics and automatic boundary despawning outside flight limits.

### 4. Sequential Holographic Checkpoint Gates
Managed by [RingManager](Assets/_NebulaRelay/Scripts/World/RingManager.cs) and [RingGate](Assets/_NebulaRelay/Scripts/World/RingGate.cs):
* **Strict Sequential Logic**: Checkpoints must be threaded in order (`Gate 1 → Gate 2 → Gate 3...`).
* **Passage Clearance**: Calibrated gate openings allow the drone to glide through comfortably.
* **Audio-Visual Feedback**:
  * *Active Gate*: Pulsing cyan hologram waiting for passage.
  * *Cleared Gate*: Transitions to neon emerald with harmonic validation tone.
  * *Wrong-Order Penalty*: Flashes warning crimson with rejection tone; progression is blocked until previous gates are cleared.

### 5. Dual-Phase Relay Station Activation
Orchestrated by [RelayStation](Assets/_NebulaRelay/Scripts/World/RelayStation.cs), [RelayStationVisuals](Assets/_NebulaRelay/Scripts/World/RelayStationVisuals.cs), and [EnergyReceiver](Assets/_NebulaRelay/Scripts/World/EnergyReceiver.cs):
* **Phase 1: Energy Delivery**: Fire the energy capsule into the stationary receiver dish. The receiver latches once, transitions to energized green, and primes the relay tower.
* **Phase 2: Safe-Speed Docking**: Navigate the drone into the relay docking zone. 
* **Safe Velocity Interlock**: Docking speed must not exceed **$1.5\text{ u/s}$**.
  * If velocity exceeds $1.5\text{ u/s}$, the docking system repels the drone with an audible alert and an on-screen **"SLOW DOWN!"** indicator.
  * Approaching at safe speed activates the relay tower, ignites the master beacon beam, and completes the sector.

### 6. Power & Shield Management
Handled by [BatteryManager](Assets/_NebulaRelay/Scripts/Core/BatteryManager.cs):
* **Max Capacity**: 100% capacity displayed via dynamic HUD gauge.
* **Thruster Consumption**: Depletes at `0.8% / sec` under continuous propulsion.
* **Launch Cost**: Expends `4.0%` energy per capsule fire.
* **Kinetic Damage**: Colliding with [AsteroidHazard](Assets/_NebulaRelay/Scripts/World/AsteroidHazard.cs) obstacles deals `-15%` immediate shield loss with physical recoil.
* **Critical Warning & Depletion**: Flashes red alert below `20%`; total depletion (`0%`) disables ship systems and triggers mission failure.

### 7. Procedural Audio Synthesizer
Powered by [AudioManager](Assets/_NebulaRelay/Scripts/Effects/AudioManager.cs):
* **Zero External Dependencies**: Generates pure procedural audio waveforms directly into memory using math routines.
* **Dynamic Waveforms**:
  * *Thruster Propulsion*: Modulated low-frequency noise rumble.
  * *Capsule Fire*: High-pitch frequency-swept laser chirp.
  * *Gate Checkpoint*: Harmonic resonant bell chime.
  * *Docking Success*: Victorious multi-tone arpeggiated fanfare.
  * *Speed & Boundary Warnings*: Rapid square-wave warning buzzers.
  * *Ambient Space*: Continuous generative cosmic hum.

---

## 🗺️ Campaign Sectors

The campaign spans 10 progressively demanding sectors, selectable from the [MainMenuController](Assets/_NebulaRelay/Scripts/UI/MainMenuController.cs) Mission Briefing terminal across 4 distinct difficulty modes:

```
[MAIN MENU] ───> [DIFFICULTY SELECTION] ───> [10-MISSION SELECT (2x5 GRID)]
    │
    ├── Sector 01: First Signal           ───>  16 Gates  │  Calm Orbital Corridor       │  Ocean Planet Vista
    ├── Sector 02: The Debris Field       ───>  18 Gates  │  Asteroid Gauntlet           │  Shield Preservation
    ├── Sector 03: The Ion Storm          ───>  20 Gates  │  Dynamic Hazard Slaloms      │  Cross-Drift Navigation
    ├── Sector 04: The Gravity Well       ───>  22 Gates  │  Gravitational Corridors     │  Precision Retro-Burns
    ├── Sector 05: The Omega Transmission ───>  24 Gates  │  Galactic Nexus Hub          │  Full Network Recovery
    ├── Sector 06: The Silent Array       ───>  26 Gates  │  Dormant Sensor Grid         │  Debris Field Weave
    ├── Sector 07: The Pulsar Reach       ───>  28 Gates  │  Pulsar Radiation Route      │  High Frequency Gauntlet
    ├── Sector 08: The Shattered Moon     ───>  30 Gates  │  Lunar Fragment Gauntlet     │  Dense Moving Clusters
    ├── Sector 09: The Event Horizon      ───>  32 Gates  │  Gravitational Ergosphere    │  Extreme Drift Navigation
    └── Sector 10: The Last Signal        ───>  34 Gates  │  Galactic Relay Core (Finale)│  All 10 Relays Restored!
```

| Sector | Title | Checkpoints | Environmental Hazards | Primary Objective |
| :---: | :--- | :---: | :--- | :--- |
| **01** | **First Signal** | 16 Ring Gates | Calm orbital drift corridor | Master zero-g propulsion, launch first capsule, dock at $\le 1.5\text{ u/s}$ |
| **02** | **The Debris Field** | 18 Ring Gates | Dense asteroid belts with collision damage | Thread tight orbital lanes, protect battery shields from kinetic hits |
| **03** | **The Ion Storm** | 20 Ring Gates | Oscillating asteroid patrol hazards | Navigate moving slalom obstacles under dynamic cross-drift turbulence |
| **04** | **The Gravity Well** | 22 Ring Gates | Concentrated perimeter debris & narrow gates | Precision speed management; high-risk retro-braking corridors |
| **05** | **The Omega Transmission** | 24 Ring Gates | Grand multi-tier asteroid gauntlet | Thread 24 sequential gates and activate the nexus relay |
| **06** | **The Silent Array** | 26 Ring Gates | Decommissioned sensor array debris | Weave 26 sequential gates through dormant orbital corridors |
| **07** | **The Pulsar Reach** | 28 Ring Gates | High-frequency radiation hazard zones | Thread 28 synchronized gates under tightening obstacle clearance |
| **08** | **The Shattered Moon** | 30 Ring Gates | Lunar fragment gauntlet & moving clusters | Dodge drifting lunar rock fields across 30 sequential checkpoints |
| **09** | **The Event Horizon** | 32 Ring Gates | Ergosphere gravitational anomalies | High-velocity momentum control across 32 gates |
| **10** | **The Last Signal** | 34 Ring Gates | Central galactic core gauntlet | Synchronize all 34 gates, deliver final capsule, restore galactic communications |

---

## ⚙️ Difficulty Modes

Drift Core features 4 calibrated difficulty modes extending gameplay without altering core physics:

| Difficulty Mode | Rank Badge | Timer (Base) | Docking Speed Limit | Hazard Speed | Polyline Clearance | Capsules | Score Multiplier |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Explorer** | `EXPLORER` (Green) | **360s (6:00)** | $\le 1.8\text{ u/s}$ | $0.7\times$ | $\ge 3.2\text{u}$ | 4 Ready | $1.0\times$ |
| **Pilot (Normal)**| `PILOT` (Cyan) | **300s (5:00)** | $\le 1.5\text{ u/s}$ | $1.0\times$ | $\ge 2.6\text{u}$ | 2 Ready | $1.0\times$ |
| **Veteran** | `VETERAN` (Amber) | **270s (4:30)** | $\le 1.2\text{ u/s}$ | $1.3\times$ | $\ge 2.2\text{u}$ | 2 Ready | $1.5\times$ |
| **Voidwalker** | `VOIDWALKER` (Red) | **240s (4:00)** | $\le 1.0\text{ u/s}$ | $1.5\times$ | $\ge 1.9\text{u}$ | 2 Ready | $2.0\times$ |

---

## 📐 Physics & Mathematical Foundations

### 1. Newtonian Projectile Trajectory
The trajectory preview and capsule ballistics share identical Newtonian mechanics modeled in [ProjectileMath](Assets/_NebulaRelay/Scripts/Delivery/TrajectoryPreview.cs):

$$p(t) = p_0 + v_0 t + \frac{1}{2} a t^2$$

In zero-gravity space where gravitational acceleration $a = 0$:

$$p(t) = p_0 + v_0 t$$

The trajectory preview samples 30 discrete intervals across $t \in [0, 1.5\text{ s}]$, producing an exact overlay with the projectile's physical trajectory.

### 2. Inertial Damping & Speed Clamping
Linear drag and velocity clamping ensure responsive control while preserving authentic space drift:

$$v_{t + \Delta t} = \min\left(\|v_t\| \cdot (1 - c_d \Delta t), \; v_{\max}\right) \cdot \hat{v}_t$$

Where:
* $c_d = 0.2\text{ s}^{-1}$ (Linear drag coefficient)
* $v_{\max} = 8.0\text{ u/s}$ (Maximum courier velocity)
* Launch velocity $v_{\text{capsule}} = 12.0\text{ u/s}$

### 3. Safe Docking Interlock
The relay docking clamp strictly evaluates drone kinetic energy at boundary contact:

$$\|v_{\text{drone}}\| \le v_{\text{docking\_limit}} \quad (v_{\text{docking\_limit}} = 1.5\text{ u/s on Normal})$$

If $\|v_{\text{drone}}\| > v_{\text{docking\_limit}}$, docking fails and an elastic impulse repels the ship away from the spire.

---

## 🧪 Automated Verification Suite & Mission Validator

Drift Core includes two complementary automated validation suites:
1. **Verification Test Suite** ([NebulaRelayVerificationTests](Assets/_NebulaRelay/Editor/NebulaRelayVerificationTests.cs)): 48 automated tests covering core math, drone physics, ring sequencing, battery discharge, camera orthographic bounds, zero scale overrides, 11-scene ordering, and UI text fitting across 5 desktop resolutions.
2. **Mission Validator** ([MissionValidator](Assets/_NebulaRelay/Editor/MissionValidator.cs)): 40-combination validation checking all 10 missions across all 4 difficulty modes. Every combination verifies gate ordering, corridor bounds, 14–22u gate spacing, estimated par time $\ge 120\text{s}$, timer $\ge 1.25\times\text{par}$, battery drain within par targets, obstacle clearance, and nested deterministic subsets.

To execute the test suite in Unity:
> Top Menu Bar $\rightarrow$ **Drift Core** $\rightarrow$ **Run All Verification Tests**  
> Top Menu Bar $\rightarrow$ **Drift Core** $\rightarrow$ **Validate All Missions**

---

## 📁 Project Architecture

```
NebulaRelay/
├── Assets/_NebulaRelay/
│   ├── Art/                              # 1024px High-Resolution Space Sprites
│   │   ├── Drone_Courier.png             # Player delta spacecraft
│   │   ├── RelayStation_Tower.png        # Orbital communications spire
│   │   ├── RingGate_Neon.png             # Holographic checkpoint gate
│   │   ├── EnergyCapsule_Core.png        # Magenta plasma projectile
│   │   ├── Asteroid_Detailed.png         # Cratered deep-space hazard
│   │   ├── Planet_BlueOcean.png          # Majestic terrestrial backdrop
│   │   ├── MainMenu_CosmicBG.png         # Title screen panoramic vista
│   │   ├── Nebula_FirstSignal.png        # Sector 1 cyan nebula skybox
│   │   ├── Nebula_DebrisField.png        # Sector 2 magenta nebula skybox
│   │   ├── Nebula_LastTransmission.png   # Sector 3 electric azure skybox
│   │   ├── Nebula_GravityWell.png        # Sector 4 golden vortex skybox
│   │   └── Nebula_OmegaCore.png          # Sector 5 deep-violet nexus skybox
│   │
│   ├── Prefabs/                          # Calibrated Game Objects
│   │   ├── PlayerDrone.prefab            # Courier drone (~1.2u) with physics & launchers
│   │   ├── EnergyCapsule.prefab          # Zero-g plasma projectile (~0.4u) with triggers
│   │   ├── RingGate.prefab               # Holographic checkpoint ring (~2.4u)
│   │   ├── EnergyReceiver.prefab         # Stationary energy collection dish (~1.5u)
│   │   ├── RelayStation.prefab           # Dual-phase orbital transmission tower (~3.2u)
│   │   └── AsteroidHazard.prefab         # Kinematic/patrolling kinetic obstacle (1.0-1.8u)
│   │
│   ├── Scenes/                           # 6 Registered Game Scenes
│   │   ├── MainMenu.unity                # Scene 0: Sector selector & options
│   │   ├── Level_01.unity                # Scene 1: Sector 1 (First Signal - 4 Gates)
│   │   ├── Level_02.unity                # Scene 2: Sector 2 (The Debris Field - 5 Gates)
│   │   ├── Level_03.unity                # Scene 3: Sector 3 (The Ion Storm - 5 Gates)
│   │   ├── Level_04.unity                # Scene 4: Sector 4 (The Gravity Well - 5 Gates)
│   │   └── Level_05.unity                # Scene 5: Sector 5 (The Omega Transmission - 6 Gates)
│   │
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs            # Persistent singleton & relay tracker
│   │   │   ├── LevelManager.cs           # Sector lifecycle, objectives & corridor bounds
│   │   │   ├── BatteryManager.cs         # Shield capacity & power consumption
│   │   │   └── ScreenSetup.cs            # Native fullscreen init & F11 / Alt+Enter window toggle
│   │   ├── Player/
│   │   │   ├── DroneController.cs        # 2D Newtonian flight, corridor soft-clamping & speed caps
│   │   │   └── DroneVisuals.cs           # Thruster glow, banking & hit flashes
│   │   ├── Delivery/
│   │   │   ├── CapsuleLauncher.cs        # Rate-limited plasma core launcher (New Input System)
│   │   │   ├── EnergyCapsule.cs          # Physics projectile & impact triggers
│   │   │   └── TrajectoryPreview.cs      # Exact 30-sample quadratic line renderer
│   │   ├── World/
│   │   │   ├── RingGate.cs               # Sequential checkpoint state machine
│   │   │   ├── RingManager.cs            # Sector ring coordinator & UI banners
│   │   │   ├── EnergyReceiver.cs         # Single-latch capsule detection dish
│   │   │   ├── RelayStation.cs           # Safe-speed velocity gate & docking
│   │   │   ├── RelayStationVisuals.cs    # Spire energy charging & beacon beam
│   │   │   ├── AsteroidHazard.cs         # Collision damage, rotation & runtime variation
│   │   │   ├── CameraFollow.cs           # Smooth corridor damping & aspect ratio adaptation
│   │   │   ├── ParallaxBackground.cs     # Continuous cosmic parallax drift
│   │   │   ├── CosmicEnvironment.cs      # Autonomous rotating celestial objects
│   │   │   ├── CosmicDustParticles.cs    # Procedural floating star mote field
│   │   │   ├── MissionData.cs            # 10 comprehensive mission definitions & par times
│   │   │   └── MissionObstacleSpawner.cs # Seeded deterministic obstacle spawner (nested difficulties)
│   │   ├── UI/
│   │   │   ├── UIScale.cs                # Shared 1280x720 reference scaler for laptop & desktop UI
│   │   │   ├── UIDesignSystem.cs         # Zero-allocation high-contrast sci-fi design system & procedural textures
│   │   │   ├── HUDController.cs          # Real-time speedometer, 260x26 segmented battery, countdown timer & telemetry dock
│   │   │   └── MainMenuController.cs     # 10-mission selector grid, difficulty select & mission overview panel
│   │   └── Effects/
│   │       └── AudioManager.cs           # Zero-dependency procedural sound generator
│   │
│   └── Editor/
│       ├── NebulaSceneBuilder.cs         # Deterministic scene generator & build pipeline (10 missions)
│       ├── MissionValidator.cs           # 40-combination validation matrix (10 missions x 4 difficulties)
│       └── NebulaRelayVerificationTests.cs # Comprehensive 48-check automated verification suite
```

---

## 🚀 Quick Start & Installation

### Option 1: Play in Unity Editor
1. **System Requirements**:
   * **Unity 6 LTS** (`6000.0.84f1` or compatible)
   * Universal Render Pipeline (URP 2D)
   * Unity Input System Package (active input handler: new)
   * Windows 10/11 x64
2. **Open Project**:
   * Open **Unity Hub** $\rightarrow$ Click **Add** $\rightarrow$ Select the `NebulaRelay` repository directory.
   * Open the project with Unity 6.
3. **Play Instantly**:
   * Open [Assets/_NebulaRelay/Scenes/MainMenu.unity](Assets/_NebulaRelay/Scenes/MainMenu.unity) or [Assets/_NebulaRelay/Scenes/Level_01.unity](Assets/_NebulaRelay/Scenes/Level_01.unity).
   * Press **Play** in the Unity Editor toolbar.

### Option 2: Build Standalone Executable (.exe)
To compile a fresh standalone Windows executable:
1. In the Unity Editor, select from the top menu:
   ```
   Drift Core  ──>  Build Windows Standalone (.exe)
   ```
2. The executable will be compiled to `C:\Games\DriftCore\DriftCore.exe`.

---

## 🖥️ User Interface & Telemetry HUD

The in-flight heads-up display ([HUDController](Assets/_NebulaRelay/Scripts/UI/HUDController.cs)) renders with an adaptive 1280x720 virtual canvas matrix so graphics scale crisply across all display resolutions without per-frame memory allocations or fonts below 14px:

```
┌────────────────────────────────────────────────────────────────────────┐
│ [ MISSION 01: PROVING GROUNDS ]                   [ 05:00 ] [ ❚❚ ]     │
│                 [ TARGET: RING GATE 02 / 08 ]                          │
│                                                                        │
│                                                                        │
│ ┌───────────────────────┬───────────────────────┬────────────────────┐ │
│ │ BATTERY  78% [■■■■■░] │ VELOCITY 2.4 u/s SAFE │ CAPSULES [●●] 2 RDY│ │
│ └───────────────────────┴───────────────────────┴────────────────────┘ │
└────────────────────────────────────────────────────────────────────────┘
```

* **Speedometer & Safe Docking Indicator**:
  * Displays current velocity in `u/s` with threshold ticks.
  * Renders in **Cyan** (`<= maxSafeSpeed` - Safe for docking).
  * Flashes in **Crimson** (`> maxSafeSpeed` - Warning: Too fast to dock).
* **Segmented Shield Battery Bar**:
  * High-visibility 260x26 segmented power bar: Green ($>50\%$), Amber ($20\% - 50\%$), and Flashing Red ($<20\%$).
* **Sequential Objective Banner**:
  * Updates dynamically to display the current target (`Ring Gate X / Y`, `Fire Capsule at Receiver`, or `Dock at Relay Spire`).
* **5-Minute Mission Timer**:
  * Persistent countdown timer in `mm:ss` format with amber warning at $<60\text{s}$ and red pulsing urgency at $<30\text{s}$.
* **Capsule Reserves Tracker**:
  * Displays remaining plasma capsules with glowing pips and readiness indicator.

---

## ⚡ Technical Specifications

* **Target Framerate**: 60–144 FPS V-Sync
* **Physics Refresh**: `FixedUpdate` running at 50Hz ($0.02\text{s}$)
* **Physics Gravity**: `(0, 0)` zero gravity
* **Coordinate System**: Pure 2D Cartesian ($X$ horizontal corridor, $Y$ vertical flight bounds)
* **Audio Engine**: Mathematical procedural synthesizers; zero uncompressed audio disk bloat
* **Camera System**: Orthographic size 7.5 across all 5 levels (8.0 in MainMenu) with velocity-based lookahead damping
* **Asset Optimization**: High-resolution 1024px sprite textures with Full Rect mesh type, mipmaps disabled, and standard compression

---

## 📜 Credits & Collaborators

* **Lead Developer**: Shreyas Adiga ([@shreyasadiga82](https://github.com/shreyasadiga82))
* **Engine**: Built with [Unity 6](https://unity.com/)
* **License**: MIT License — open for educational, personal, and commercial modification.

<div align="center">
  <sub>✦ Crafted with precision for high-speed cosmic couriers across the galaxy ✦</sub>
</div>
