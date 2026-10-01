# APEX / Afterglow — Human hero, strength and city driving update

A standalone Windows game with two selectable maps: an open city where you can play as a human superhero or drive a car, and a dedicated racing circuit. The city hero has Spider-Man-inspired web powers and an original red and blue costume.

## Play

Double-click **Play.cmd**. Choose your circuit car and paint, then select **Choose Map & Play** (or Enter). Choose **Solstice City** for the human hero or **Grand Tour** for car racing. Return to the garage from the pause menu to change maps. **O** in the garage opens the city directly.

The updated executable is **Build-v8/APEX Afterglow.exe**. Previous builds remain available separately.

Click the game window before using the keyboard. The city supports both WASD and arrows for movement. Hold Shift to run; move beside a car and press E or G. Opening J, Tab or Esc pauses gameplay; close the journal/map or choose Resume to move again.

## Human hero / Solstice City

Walk and run through a 2.7 × 2.7 km region with 110 exterior buildings, glass towers, brick apartments, shops, houses, warehouses, sidewalks and crossings. Walking residents populate the city. The hero now has a tapered human torso, proportioned head, articulated shoulders, elbows, hips and knees, pointed mask lenses, suit web patterns, and different walking/running poses. Four parked cars near the starting street can be driven, carried or thrown. White map dots mark their current positions.

| Control | Action |
| --- | --- |
| WASD or arrow keys | Walk; camera-relative movement |
| Shift + movement keys | Run |
| Space | Super jump |
| Hold left mouse button | Attach a web to a nearby rooftop and swing; release to retain momentum |
| Q | Web zip toward a rooftop; costs 30 energy |
| Hold F + WASD near a wall | Wall walk up, down or sideways; release movement to cling in place. Space leaps off; climbing over the top steps onto the roof. |
| Hold right mouse button and move mouse | Turn the camera |
| Comma / Period | Turn camera with keyboard |
| E or G | Enter a nearby car / exit after stopping |
| H | Super strength: lift a nearby car / put it down in clear space |
| T | Throw the carried car in the camera direction |
| E | Interact with an objective on foot; near a car, enter it. J opens the task journal. |
| J | Open / close task journal |
| Tab | Open / close city map; click to set waypoint |
| R | Recover the hero or driven car to the nearest street |
| C | Change camera distance |
| Esc | Pause / resume |

Webs attach to buildings within 145 metres and above the hero, rather than empty air. Move toward taller buildings if no roof is in reach. Swinging has a rope length and gravity; WASD pumps the swing. Web energy recharges automatically. Climbing and jumping are available without energy.

Walk within five metres of a parked car and press **E** or **G** (or click **Enter Car**). Drive with **WASD** or arrows: W accelerates, S brakes and reverses, A/D steer, and Space is the handbrake. Stop before pressing E or G to exit; the hero appears on a clear side of the car. **R** recovers the car to a road. You can drive all four models around the city, including the luxury Sovereign V12. Exit the car to interact with mission objectives.

For super strength, stand beside a car and press **H**. Walk while holding it overhead, press H to put it down, or aim with the camera and press **T** to throw it. A thrown car follows a gravity arc, collides with buildings and lands; it remains usable afterward. The hero keeps both hands on a carried car, so web powers and jumping resume after putting it down or throwing it. City cars reset to their parking places when you re-enter the city.

Choose a task in the **J** journal or **Tab** map. Gold markers show the current objective. Rooftop objectives require reaching the roof; standing on the street below does not count.

| Task | Objective | First completion reward |
| --- | --- | --- |
| Rooftop Rescue | Reach two stranded residents; press E beside each | 250 hero points |
| Skyline Patrol | Reach three rooftop beacons in order | 200 hero points |
| Neighbourhood Delivery | Reach three street stops and press E within 150 seconds | 300 hero points |

The map, journal and pause menu stop movement and task timers. Tasks can be cancelled or restarted from the journal; completed tasks can be replayed. Completion and hero points are saved locally. Replays do not award points again. An unfinished task restarts when you choose it again or re-enter the city.

Characters and buildings use stylized procedural geometry. Wall walking, rooftop landing and car collisions use simplified exterior surfaces. There are no building interiors, combat, enemies or cinematic story missions in this update. Pedestrians follow simple sidewalk routes.

## Car racing / Grand Tour

Choose among **Sovereign V12** (a Rolls-Royce-inspired luxury saloon), **Vantage GT-R**, **Outlaw V8**, and **Spectre X**, with six paints and distinct performance. These are original car designs. Use 1–4 or Left / Right in the garage, P to change paint, and the difficulty button to select Rookie, Club or Pro opponents.

Race four rivals over three laps on the dedicated Grand Tour circuit, with a paddock, canyon, forest, bridge and lake. WASD or arrows drive, S brakes, Space drifts, Shift uses rechargeable nitro, C changes chase/hood camera, R recovers, and Tab opens the circuit map. Best lap and race times are saved separately for each car and difficulty.

**F11** toggles fullscreen. **M** toggles engine audio. Graphics presets are available in the garage and pause menu.

## Build and validation

Built with Unity **6000.6.0f1** for Windows x64 / Direct3D 11. Run **Build-Game.ps1** to rebuild; it writes **Logs/build.log**. See **VALIDATION.md** for the latest checks.

The built player accepts **--hero-test** (also **--world-test**) for hero validation and screenshots, **--keyboard-test** for real Windows keyboard input through the normal game loop, or **--smoke-test** for the full racing checks. These test modes do not change saved preferences or records. The source is in **Assets/Scripts** and the build entry point is **Assets/Editor/RaceBuilder.cs**.