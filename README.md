# ✦ Nebula Relay: The Last Signal ✦

<div align="center">

![Unity 6](https://img.shields.io/badge/Unity%206%20LTS-6000.0.84f1-blue.svg?style=for-the-badge&logo=unity&logoColor=white)
![Render Pipeline](https://img.shields.io/badge/Render%20Pipeline-Universal%202D%20(URP)-blueviolet.svg?style=for-the-badge&logo=render&logoColor=white)
![Language](https://img.shields.io/badge/Language-C%23%209.0%2B-239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows%20x64-0078D6.svg?style=for-the-badge&logo=windows&logoColor=white)
![Build Status](https://img.shields.io/badge/Build%20Status-Complete%20%26%20Verified-00E5FF.svg?style=for-the-badge&logo=checkmarx&logoColor=white)
![Automated Tests](https://img.shields.io/badge/Tests-15%2F15%20Passing-brightgreen.svg?style=for-the-badge&logo=pytest&logoColor=white)

<br/>

**A precision zero-gravity 2D sci-fi courier simulator built for Unity 6 LTS.**  
*Pilot through treacherous cosmic sectors, thread sequential checkpoint gates, launch plasma energy cores into receiver dishes, and execute soft-docking maneuvers at orbital relay spires to restore the deep-space communications network.*

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

The courier drone is controlled via keyboard thrust and mouse trajectory targeting:

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

### 1. Zero-G Inertial Flight Dynamics
Governed by [DroneController](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Scripts/Player/DroneController.cs) using a pure 2D Newtonian physics simulation:
* **Thrust Rating**: `6.0 u/s²` with smooth directional interpolation towards movement vector.
* **Top Speed Cap**: Strictly capped at `8.0 u/s` to maintain navigational precision.
* **Inertial Coasting**: Subtle drag coefficient ($0.2$) gives the authentic feel of deep-space drift while retaining crisp micro-adjustments.
* **Dynamic Visuals**: Animated thruster trail, rotation smoothing, and kinetic hull feedback handled by [DroneVisuals](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Scripts/Player/DroneVisuals.cs).

### 2. Ballistic Trajectory & Energy Capsule Launcher
Implemented across [CapsuleLauncher](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Scripts/Delivery/CapsuleLauncher.cs), [EnergyCapsule](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Scripts/Delivery/EnergyCapsule.cs), and [TrajectoryPreview](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Scripts/Delivery/TrajectoryPreview.cs):
* **Predictive LineRenderer**: Casts 30 analytical Newtonian sample points showing the exact path the capsule will travel before firing.
* **Single-Slot Protocol**: Only one simultaneous capsule may exist in flight at a time (`0.5s` cooldown), requiring deliberate targeting.
* **Zero-G Projectile Mechanics**: Projectiles travel at `12.0 u/s` with true zero-gravity ballistics and automatic boundary despawning.

### 3. Sequential Holographic Checkpoint Gates
Managed by [RingManager](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Scripts/World/RingManager.cs) and [RingGate](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Scripts/World/RingGate.cs):
* **Strict Sequential Logic**: Checkpoints must be threaded in order (`Gate 1 → Gate 2 → Gate 3...`).
* **Audio-Visual Feedback**:
  * *Active Gate*: Pulsing cyan hologram waiting for passage.
  * *Cleared Gate*: Transitions to neon emerald with harmonic validation tone.
  * *Wrong-Order Penalty*: Flashes warning crimson with rejection tone; progression is blocked until previous gates are cleared.

### 4. Dual-Phase Relay Station Activation
Orchestrated by [RelayStation](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Scripts/World/RelayStation.cs), [RelayStationVisuals](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Scripts/World/RelayStationVisuals.cs), and [EnergyReceiver](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Scripts/World/EnergyReceiver.cs):
* **Phase 1: Energy Delivery**: Fire the energy capsule into the stationary receiver dish. The receiver latches once, transitions to energized green, and primes the relay tower.
* **Phase 2: Safe-Speed Docking**: Navigate the drone into the relay docking zone. 
* **Safe Velocity Interlock**: Docking speed must not exceed **$1.5\text{ u/s}$**.
  * If velocity exceeds $1.5\text{ u/s}$, the docking system repels the drone with an audible alert and an on-screen **"SLOW DOWN!"** indicator.
  * Approaching at safe speed activates the relay tower, ignites the master beacon beam, and completes the sector.

### 5. Power & Shield Management
Handled by [BatteryManager](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Scripts/Core/BatteryManager.cs):
* **Max Capacity**: 100% capacity displayed via dynamic HUD gauge.
* **Thruster Consumption**: Depletes at `0.8% / sec` under continuous propulsion.
* **Launch Cost**: Expends `4.0%` energy per capsule fire.
* **Kinetic Damage**: Colliding with [AsteroidHazard](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Scripts/World/AsteroidHazard.cs) obstacles deals `-15%` immediate shield loss with physical recoil.
* **Critical Warning & Depletion**: Flashes red alert below `20%`; total depletion (`0%`) disables ship systems and triggers mission failure.

### 6. Procedural Audio Synthesizer
Powered by [AudioManager](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Scripts/Effects/AudioManager.cs):
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

The campaign spans 5 progressively demanding sectors, selectable from the [MainMenuController](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Scripts/UI/MainMenuController.cs) Mission Briefing terminal:

```
[MAIN MENU]
    │
    ├── Sector 01: First Signal           ───>  4 Gates  │  Calm Orbital Corridor  │  Ocean Planet Vista
    ├── Sector 02: The Debris Field       ───>  5 Gates  │  Asteroid Gauntlet      │  Shield Preservation
    ├── Sector 03: The Ion Storm          ───>  5 Gates  │  Dynamic Hazard Slaloms │  Cross-Drift Navigation
    ├── Sector 04: The Gravity Well       ───>  5 Gates  │  Tight Corridor Choke   │  Precision Retro-Burns
    └── Sector 05: The Omega Transmission ───>  6 Gates  │  Galactic Nexus Hub     │  Full Network Recovery
```

| Sector | Title | Checkpoints | Environmental Hazards | Primary Objective |
| :---: | :--- | :---: | :--- | :--- |
| **01** | **First Signal** | 4 Ring Gates | Calm orbital drift corridor | Master zero-g propulsion, launch first capsule, dock at $\le 1.5\text{ u/s}$ |
| **02** | **The Debris Field** | 5 Ring Gates | 3 Dense asteroid belts with collision damage | Thread tight orbital lanes, protect battery shields from kinetic hits |
| **03** | **The Ion Storm** | 5 Ring Gates | Oscillating asteroid patrol hazards | Navigate moving slalom obstacles under dynamic cross-drift turbulence |
| **04** | **The Gravity Well** | 5 Ring Gates | Concentrated perimeter debris & narrow gates | Precision speed management; high-risk retro-braking corridors |
| **05** | **The Omega Transmission** | 6 Ring Gates | Grand multi-tier asteroid gauntlet | Thread 6 sequential gates and activate the master relay to restore the galaxy |

---

## 📐 Physics & Mathematical Foundations

### 1. Newtonian Projectile Trajectory
The trajectory preview and capsule ballistics share identical Newtonian mechanics modeled in [ProjectileMath](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Scripts/Delivery/TrajectoryPreview.cs):

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

$$\|v_{\text{drone}}\| \le v_{\text{docking\_limit}} \quad (v_{\text{docking\_limit}} = 1.5\text{ u/s})$$

If $\|v_{\text{drone}}\| > 1.5\text{ u/s}$, docking fails and an elastic impulse repels the ship away from the spire.

---

## 🧪 Automated Verification Suite

Nebula Relay includes a 15-test automated verification suite built into [NebulaRelayVerificationTests](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Editor/NebulaRelayVerificationTests.cs). All 15 tests pass on every compilation and scene build:

```
============================================================
[NEBULA RELAY] AUTOMATED TEST SUITE EXECUTION SUMMARY
============================================================
[PASS]  01. ProjectileMath Zero-Gravity Linear Motion (p(1s) = (12, 0))
[PASS]  02. ProjectileMath Acceleration Integration (p(1s).y = 5.10 matches 0.5*a*t^2)
[PASS]  03. Trajectory Preview and Real Capsule Trajectory Alignment
[PASS]  04. Drone Maximum Speed Clamping (Clamped magnitude = 8.0 u/s)
[PASS]  05. Drag Deceleration Model (Decelerates from 8.0 to 7.2 u/s over 0.5s)
[PASS]  06. Out-of-Order Ring Rejection (Gate 2 rejected while Gate 1 active)
[PASS]  07. Correct Ring Sequence Advance (Gate 1 triggers Gate 2 advance)
[PASS]  08. Single-Delivery Latch (Subsequent capsule deliveries ignored)
[PASS]  09. Relay Safe-Speed Gate (Entry > 1.5 rejected, entry <= 1.5 accepted)
[PASS]  10. Relay Prerequisite Gate (Relay blocked until energy capsule delivered)
[PASS]  11. Battery Floor Clamp (Battery stops strictly at 0% without negative overflow)
[PASS]  12. Low-Battery Warning (Triggers below 20% capacity)
[PASS]  13. Victory Condition Verification (Restoring all sector relays triggers galactic victory)
[PASS]  14. Build Settings Registration (All 6 scenes registered in EditorBuildSettings)
[PASS]  15. Asset Integrity Check (All 6 core prefabs and 6 scene files verified on disk)
------------------------------------------------------------
RESULT: 15 / 15 TESTS PASSED (100% COVERAGE)
============================================================
```

To execute the test suite in Unity:
> Top Menu Bar $\rightarrow$ **Nebula Relay** $\rightarrow$ **Run All 15 Verification Tests**

---

## 📁 Project Architecture

```
NebulaRelay/
├── Builds/
│   └── Windows/
│       ├── NebulaRelay.exe               # Compiled Standalone Windows x64 binary
│       ├── UnityPlayer.dll               # Unity 6 runtime engine library
│       └── NebulaRelay_Data/             # Packaged sector data & asset bundles
│
└── Assets/_NebulaRelay/
    ├── Art/                              # 4K Handcrafted Space Textures
    │   ├── Drone_Courier.png             # Player delta spacecraft
    │   ├── RelayStation_Tower.png        # Orbital communications spire
    │   ├── RingGate_Neon.png             # Holographic checkpoint gate
    │   ├── EnergyCapsule_Core.png        # Magenta plasma projectile
    │   ├── Asteroid_Detailed.png         # Cratered deep-space hazard
    │   ├── Planet_BlueOcean.png          # Majestic terrestrial backdrop
    │   ├── MainMenu_CosmicBG.png         # Title screen panoramic vista
    │   ├── Nebula_FirstSignal.png        # Sector 1 cyan nebula skybox
    │   ├── Nebula_DebrisField.png        # Sector 2 magenta nebula skybox
    │   ├── Nebula_LastTransmission.png   # Sector 3 electric azure skybox
    │   ├── Nebula_GravityWell.png        # Sector 4 golden vortex skybox
    │   └── Nebula_OmegaCore.png          # Sector 5 deep-violet nexus skybox
    │
    ├── Prefabs/                          # Calibrated Game Objects
    │   ├── PlayerDrone.prefab            # Courier drone with physics & launchers
    │   ├── EnergyCapsule.prefab          # Zero-g plasma projectile with triggers
    │   ├── RingGate.prefab               # Holographic checkpoint ring
    │   ├── EnergyReceiver.prefab         # Stationary energy collection dish
    │   ├── RelayStation.prefab           # Dual-phase orbital transmission tower
    │   └── AsteroidHazard.prefab         # Kinematic/patrolling kinetic obstacle
    │
    ├── Scenes/                           # 6 Registered Game Scenes
    │   ├── MainMenu.unity                # Scene 0: Sector selector & options
    │   ├── Level_01.unity                # Scene 1: Sector 1 (First Signal)
    │   ├── Level_02.unity                # Scene 2: Sector 2 (The Debris Field)
    │   ├── Level_03.unity                # Scene 3: Sector 3 (The Ion Storm)
    │   ├── Level_04.unity                # Scene 4: Sector 4 (The Gravity Well)
    │   └── Level_05.unity                # Scene 5: Sector 5 (The Omega Transmission)
    │
    ├── Scripts/
    │   ├── Core/
    │   │   ├── GameManager.cs            # Persistent singleton & relay tracker
    │   │   ├── LevelManager.cs           # Sector lifecycle, objectives & state
    │   │   └── BatteryManager.cs         # Shield capacity & power consumption
    │   ├── Player/
    │   │   ├── DroneController.cs        # 2D Newtonian flight & speed clamps
    │   │   └── DroneVisuals.cs           # Thruster glow, banking & hit flashes
    │   ├── Delivery/
    │   │   ├── CapsuleLauncher.cs        # Rate-limited plasma core launcher
    │   │   ├── EnergyCapsule.cs          # Physics projectile & impact triggers
    │   │   └── TrajectoryPreview.cs      # Exact 30-sample quadratic line renderer
    │   ├── World/
    │   │   ├── RingGate.cs               # Sequential checkpoint state machine
    │   │   ├── RingManager.cs            # Sector ring coordinator & UI banners
    │   │   ├── EnergyReceiver.cs         # Single-latch capsule detection dish
    │   │   ├── RelayStation.cs           # Safe-speed velocity gate & docking
    │   │   ├── RelayStationVisuals.cs    # Spire energy charging & beacon beam
    │   │   ├── AsteroidHazard.cs         # Collision damage, rotation & patrols
    │   │   ├── CameraFollow.cs           # Smooth corridor damping & tracking
    │   │   ├── ParallaxBackground.cs     # Continuous cosmic parallax drift
    │   │   ├── CosmicEnvironment.cs      # Autonomous rotating celestial objects
    │   │   └── CosmicDustParticles.cs    # Procedural floating star mote field
    │   ├── UI/
    │   │   ├── HUDController.cs          # Real-time speedometer, battery & radar
    │   │   └── MainMenuController.cs     # Sci-fi mission selector & audio settings
    │   └── Effects/
    │       └── AudioManager.cs           # Zero-dependency procedural sound generator
    │
    └── Editor/
        ├── NebulaSceneBuilder.cs         # 1-Click scene generator & build pipeline
        └── NebulaRelayVerificationTests.cs # 15-check automated verification suite
```

---

## 🚀 Quick Start & Installation

### Option 1: Play Standalone (.exe) — Windows x64
No installation or Unity setup required. The standalone production build is pre-compiled and ready to play:

1. Navigate to:
   ```
   Builds/Windows/
   ```
2. Double-click [NebulaRelay.exe](file:///c:/Users/User/Desktop/NebulaRelay/Builds/Windows/NebulaRelay.exe).
3. The game launches into the Main Menu. Select **Sector 01: First Signal** and initiate flight!

### Option 2: Open in Unity Editor
For developers wishing to inspect, modify, or extend the project:

1. **System Requirements**:
   * **Unity 6 LTS** (Recommended: `6000.0.84f1` or later)
   * Universal Render Pipeline (URP 2D) package installed
   * Windows 10/11 x64
2. **Launch Project**:
   * Open **Unity Hub** $\rightarrow$ Click **Add** $\rightarrow$ Select the `NebulaRelay` repository directory.
   * Open the project with Unity 6.
3. **One-Click Scene Rebuilder**:
   * Navigate to the top Unity menu bar:
     ```
     Nebula Relay  ──>  Build All 5 Levels & Main Menu
     ```
   * This automatically calibrates all prefabs, generates all 6 scenes, assigns the continuous cosmic parallax layers, and registers build order.
4. **Compile a Fresh Standalone Executable**:
   * Navigate to:
     ```
     Nebula Relay  ──>  Build Windows Standalone (.exe)
     ```
   * A fresh executable will be compiled directly to `Builds/Windows/NebulaRelay.exe`.

---

## 🖥️ User Interface & Telemetry HUD

The in-flight heads-up display ([HUDController](file:///c:/Users/User/Desktop/NebulaRelay/Assets/_NebulaRelay/Scripts/UI/HUDController.cs)) delivers continuous real-time flight telemetry:

```
┌────────────────────────────────────────────────────────────────────────┐
│  VELOCITY: 0.82 m/s [SAFE]                    BATTERY: [████████░░] 78% │
│                                                                        │
│                                                                        │
│                [ TARGET: RING GATE 2 OF 4 ]                            │
│                                                                        │
│                                                                        │
│  RELAYS RESTORED: [ 1 / 5 ]                     SYSTEM INTEGRITY: 100% │
└────────────────────────────────────────────────────────────────────────┘
```

* **Speedometer & Safe Docking Indicator**:
  * Displays current velocity in `m/s`.
  * Renders in **Cyan** (`<= 1.5 m/s` - Safe for docking).
  * Flashes in **Crimson** (`> 1.5 m/s` - Warning: Too fast to dock).
* **Segmented Shield Battery Bar**:
  * Real-time power percentage with color coding: Green ($>50\%$), Amber ($20\% - 50\%$), and Flashing Red ($<20\%$).
* **Sequential Objective Banner**:
  * Updates dynamically to display the current target (`Ring Gate X`, `Fire Capsule at Receiver`, or `Dock at Relay Spire`).
* **Persistent Relay Restoration Counter**:
  * Tracks total campaign completion across all 5 sectors.

---

## ⚡ Technical Specifications

* **Target Framerate**: 60–144 FPS V-Sync
* **Physics Refresh**: `FixedUpdate` running at 50Hz ($0.02\text{s}$)
* **Coordinate System**: Pure 2D Cartesian ($X$ horizontal corridor, $Y$ vertical flight bounds)
* **Audio Engine**: Mathematical procedural synthesizers; zero uncompressed audio disk bloat
* **Camera System**: Dynamic orthographic framing with velocity-based lookahead damping
* **Asset Optimization**: High-resolution 4K sprite textures utilizing sprite packing and power-of-two atlasing

---

## 📜 Credits & License

* **Developer**: Shreyas Adiga ([@shreyasadiga82](https://github.com/shreyasadiga82))
* **Engine**: Built with [Unity 6](https://unity.com/)
* **License**: MIT License — open for educational, personal, and commercial modification.

<div align="center">
  <sub>✦ Crafted with precision for high-speed cosmic couriers across the galaxy ✦</sub>
</div>
