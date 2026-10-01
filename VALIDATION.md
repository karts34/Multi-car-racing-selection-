# Movement controls fix verification

Verified on 1 October 2026 in the built Windows player.

The city now accepts **WASD and arrow keys** to walk, and **Shift + movement** to run. Comma/period turn the keyboard camera. **E or G** enters a nearby car and exits after stopping; a nearby-car button is also available. E away from a car or mission objective gives a hint without opening the journal and pausing movement. J remains the explicit task-journal key. An unfocused window displays a click-to-play hint.

The previous on-foot controls ignored up/down arrows and assigned left/right to the camera, unlike the car controls. The previous E interaction opened the paused journal when no mission was active. These paths were corrected. The earlier direct-controller tests did not cover Windows keyboard input, so a separate keyboard integration suite was added.

**Logs/keyboard-controls.log** and **Logs/keyboard-test.txt** record **KEYBOARD_GAMEPLAY_PASS** for the actual Unity Input / normal Update path. The opt-in --keyboard-test player sends Windows scan-code events only while its own window is in the foreground. It does not call StepHero, StepCityCar, ToggleCityCar or BeginOpenWorld directly. Gameplay uses the ordinary Unity Input API.

The keyboard suite verified O entering the city, E without a nearby target leaving gameplay active, W walking (6.94 m in two seconds), Shift+S running (17.12 m in two seconds), arrow walking (5.16 m), approaching a parked car using keys, G entry, W driving, preventing moving exit, Space stopping the car, G exit, E re-entry/exit, walking after leaving the car, journal pause/resume, map close/resume and ordinary pause/resume. Distances are from this rendered run and depend on frame timing. **Logs/keyboard-walking.png** and **Logs/keyboard-driving.png** capture the tested world and car view.

The direct hero regression suite passed again in **Logs/controls-hero-test.log**, including **HERO_ALL_TESTS_PASS** and **HERO_DRIVING_STRENGTH_WALL_PASS**. Circuit regression results are in **Logs/controls-circuit-test.log**. Details of the retained hero, strength and vehicle features follow.

Pre-fix source and launcher files are preserved in **Logs/before-controls-fix**. **Play.cmd** launches Build-v8; earlier releases remain available.

---
# Human hero, wall walking and city driving verification

Verified on 1 October 2026 with Unity 6000.6.0f1 and an NVIDIA GeForce RTX 3050 A Laptop GPU.

## Release

The current Windows x64 / Direct3D 11 release is **Build-v8/APEX Afterglow.exe**, selected by **Play.cmd**. Build-v6 and older releases remain available. Pre-update source and documentation are preserved in **Logs/before-human-driving-upgrade**. Build results are in **Logs/build.log**.

## Hero model and movement

The hero now has a custom tapered torso mesh, human head-to-body proportions, shoulder/elbow/hip/knee joints, separate thigh/calf and upper/lower arm sections, shaped mask lenses, a web pattern and front/back spider emblems. Walking and running use different strides and bent knees/elbows. Citizens also use the articulated body. Climbing, swinging and carrying use dedicated poses. Screenshots were inspected for proportions, mask detail, wall contact and overhead car placement.

The built-player hero suite passed with **HERO_ALL_TESTS_PASS**, and the new feature suite passed with **HERO_DRIVING_STRENGTH_WALL_PASS** in **Logs/controls-hero-test.log**. Summaries are **Logs/hero-test.txt** and **Logs/hero-upgrade-tests.txt**.

- Walking, faster running, super jump, gravity, landing and building collision.
- Web anchors on real rooftops, tether distance, swinging, release momentum, web zip and energy limits.
- Stationary wall adhesion, sideways walking, ascending, descending, jumping away and stepping onto the roof.
- All four city cars: nearby entry, acceleration, steering, reverse, rejecting exit while moving, safe stopped exit and re-entry.
- City car building collision, recovery to the road, pause/map behavior, and returning to the garage.
- Super-strength lift, walking while carrying, preventing web/jump use with a car held overhead, putting a car down in clear space, ballistic throwing and landing, and driving the thrown car afterward.
- 40 moving pedestrians; rooftop rescue proximity and height checks; reaching a resident through wall climbing; ordered patrol beacons; all three mission completions; rewards, replay and delivery timeout.
- The delivery route was completed by running through the actual controller in 27.08 simulated seconds, within its 150-second limit. Other mission-state checks combine independently verified traversal with positioning at objectives; they are not full manual route playthroughs.

These checks exercise the production movement and interaction methods from an automated coroutine in the rendered Windows build. Test modes skip PlayerPrefs writes, preserving normal saved preferences and records.

## Racing regression

**Logs/controls-circuit-test.log** contains **GARAGE_ALL_12_RACES_PASS** and **AFTERGLOW_SMOKE_PASS**. Each of the four cars completed three full circuit laps at Rookie, Club and Pro difficulty. Garage selection, paint, race selection lock, acceleration, opponents, nitro, pause/map, barriers, recovery, finish/restart, graphics presets and minimap bounds also passed. Details are in **Logs/garage-race-tests.txt** and **Logs/smoke-test.txt**.

## Screenshots

- **Logs/hero-costume.png** - new human silhouette, mask and costume.
- **Logs/hero-street.png**, **hero-rooftop.png**, **hero-web-swing.png** - traversal and city.
- **Logs/hero-wall-walking.png** - wall pose.
- **Logs/hero-city-driving.png** - city car camera and controls.
- **Logs/hero-super-strength.png**, **hero-car-throw.png** - carrying and throwing a car.
- **Logs/hero-map-selection.png**, **hero-city-map.png**, **hero-task-journal.png**, **hero-rescue.png**, **hero-task-complete.png** - navigation and missions.

## Scope

Characters remain stylized procedural models. The city contains 110 exterior buildings, 24.54 km of roads, 40 walking citizens and four usable parked cars. Collision and throwing use arcade physics and simplified exterior surfaces; cars remain upright when landing. Roof equipment, balconies and pitched roofs are scenery rather than individually simulated platforms. City car positions reset on city re-entry. There are no building interiors, combat enemies, vehicle damage or ambient AI traffic in this update. The separate racing circuit retains its four opponents.