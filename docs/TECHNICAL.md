# BobbyBoi

## Version 2.1.1: sound and interaction polish

Adds 55 original synthesized cues for movement, throwing/landing, eating, mining, fishing casts/splashes/bites/reeling, damage, death/respawn, creeper fuses/explosions, companions, achievements, loot and the expansion scenes. An eight-voice buffered Windows mixer lets explosions, death cues and other important effects overlap instead of stopping each other. Samples are generated once per cue and reused. Existing volume, mute and expedition-sounds settings apply; pause/hide silence active voices. No downloaded recordings, sound packs or online services are used.

Temporary dragging, gifts, dance and sleep interactions now retain the interrupted activity in memory and resume its existing phase afterward, once BobbyBoi lands and the interaction is finished. Menu opening pauses activity timing. Meme trips retain their state and generated cargo. Explicit Cancel, disabled features and death still end an activity. This resume state is for the current app session, not a saved checkpoint across exit/restart.

The golem has separate left/right articulated limbs, alternates its arms and legs while walking, turns with the arrest/escort direction and faces the cell while guarding it. Herobrine now has pixel-varied skin/clothes, shirt sleeves, hair, a beard/nose and boots. The BobbyTube board is 40 pixels higher. Menus stay open when the pointer merely leaves them; outside clicks, Escape or choosing an action close the root and its submenus.

The replacement icon is a large 3D close-up of the actual Blender character head in seven sizes. `--build-icon` regenerates BobbyBoi.ico and assets/icon-preview.png. Updating through Restart notifies the Windows shell to refresh cached icons. `--refresh-icon` repeats that notification without deleting icon-cache files or restarting Explorer.

`--sfx-test` verifies every PCM/WAV cue, distinct sound families, overlapping voices, clipping protection, cleanup and mute behavior. `--audio-playback-test` briefly plays fishing and overlapping explosion/death cues through the actual Windows audio device at the saved volume (skips if muted). `--interruption-test` checks preserved event IDs/phases, ordinary behavior and meme-trip progress, reward-once behavior and explicit cancellation. Reports are under artifacts/sfx-tests.txt, sfx-playback.txt and interruption-tests.txt. Speaker/headphone audibility and preferred loudness remain user checks.

## Version 2.1: Crime, chaos and bad decisions

Right-click **Crime and bad decisions** for ten working core events: Get Arrested, Open Casino, Build Nether Portal, Herobrine Sighting, Mini Earthquake, Run Fake Update, Adopt Pet Creeper, Record Tiny Video, Summon UFO and Go Cave Mining. They also join the existing automatic activity scheduler. Only one major event owns movement at a time, and menu actions, dragging, damage, cancellation and fake death recover the character's visibility and scale. Existing meme trips keep their countdown and wait for major events to finish.

- Prison brings an approaching golem, a failed run, an escort, jail reactions and automatic release. An adopted creeper can supply a confetti escape gag.
- The casino animates its lever and reels, spends only BobbyBoi's fictional emerald balance, and awards silly loot or queues prison after a loss. Counts cannot go negative.
- The portal builds its obsidian frame, opens a purple animated interior, hides the traveler briefly and returns Nether loot. Some returns queue creeper adoption or a sighting.
- Herobrine appears briefly with white eyes, then disappears while BobbyBoi reacts. No jump-scare sound is played.
- Earthquakes shake only BobbyBoi and his overlay props, add dust/cracks, can remove a few house blocks and can reveal a mining cave. Other applications and the OS display never move.
- The tiny **BOBBYBOI FAKE UPDATE** panel holds at 99%, frustrates him and installs fictional debt. It is click-through and cannot block the desktop.
- The persistent creeper companion follows or sits, has a colored collar, and releases harmless confetti. It is separate from hostile mobs and never applies explosion damage. Menu controls adopt, sit/follow or dismiss it.
- The recording event has a camera, an offline thumbnail board, gestures and a fake subscriber count. UFO returns can queue a conspiracy video; the creeper can photobomb. No upload, browser access or YouTube account is involved.
- The UFO flies in, beams him up, takes him away briefly, returns and drops him safely. The cave supplies an entrance, pickaxe, mining dust, temporary disappearance and loot; certain trips queue Herobrine.

Settings includes expansion events, creepy sightings, prank events, a wolf/creeper companion limit and performance mode. Performance mode uses up to 20 character, 12 mob and 10 world visual frames per second without reducing the physics timer rate. Feature preferences, companion adoption, subscriber count, achievements and expansion cooldown timestamps persist alongside existing inventory and settings. Manual summons bypass ordinary event cooldowns but respect feature toggles and eligibility. Automatic cooldowns range from 150 to 600 seconds.

New original Blender source: assets/BobbyBoiChaos.blend, rebuildable with assets/build_chaos.py using Blender in background mode. Nine low-poly models are exported to assets/chaos.json and embedded in the EXE. Their sprites use a bounded 12 MB cache. Blender is not needed to run the pet. These are complete core sequences; the brief's many alternative endings, elaborate jail escapes and extra crossover skits remain simplified.

Speech now uses an original 5-by-7 bitmap alphabet, dark Minecraft-inspired tooltip panels, pixel borders, an event-colored accent, shadowed text, a stepped pointer and type-on animation. Long text wraps to up to four lines. No font installation or external asset is required. Closing a main menu also closes its nested menus, and inventory counts use plain `xN` text to avoid broken separators.

`--chaos-test` verifies all ten sequences, assets, cleanup, persisted state and the casino/prison, cave/sighting, portal/creeper, UFO/video, update/frustration and quake/cave flows. It also checks submenu cleanup and inventory labels. `--chaos-smoke-test` runs the ten real native overlay sequences with accelerated test timing and no preference saves. Reports and previews are under artifacts/chaos-tests.txt, chaos-live.txt, chaos-assets.png and speech-bubbles.png.

Manual checks: try each menu action at normal speed; drag BobbyBoi during the portal or UFO event; disable sightings/pranks during an event; restart with the creeper adopted; open a nested inventory menu then click another app or press Escape; check the bubbles and world props on each monitor/DPI scale. Native automated tests cannot establish behavior on every monitor arrangement or fullscreen game.

## Version 2.0.1: performance and silliness

Adds 124 shuffled contextual speech-bubble lines for idle chatter, greetings, being grabbed, sleep, mining, gifts, damage, respawning, fishing, treasure, building and dragons. Each category cycles through its lines before repeating. Speech bubbles wrap longer text. These are text quips, not recorded or spoken dialogue.

Character and mob rendering now reuses similar animation frames, with separate bounded caches (24 MB character, 16 MB mobs). Character poses and clone animation state still advance independently. Character drawing reuses color brushes and precomputes rotations; the main character reuses its frame bitmap and normally presents at up to 30 FPS, while dragging remains responsive at the physics timer rate. Physics, damage and activity timing are not slowed to match cached frames.

World overlays use up to twelve small regions per monitor instead of one bounding rectangle covering distant props. This avoids repainting a nearly fullscreen transparent bitmap when, for example, a notification appears above a house. Collision, menu and clickable world-object behavior remain enabled.

`--performance-test` compares raw and cached rendering of the same eight-mob animation sequence, checks sparse 4K redraw regions, and verifies quip decks do not repeat early. See artifacts/performance-test.txt. The results are local rendering microbenchmarks; they do not guarantee a particular desktop FPS. First-time sprite generation still costs work, and exclusive fullscreen and mixed-DPI limitations below still apply.

## Version 2.0: World activities

Right-click BobbyBoi and open **World actions** to try fishing, build or visit a house, summon a wolf, find treasure, spawn two temporary clones, climb a window, or summon the Ender Dragon. **Cancel current activity / boss** safely ends the current event. Automatic activities are enabled and coordinated so only one activity controls BobbyBoi at a time; meme trips retain their own countdown and wait while an activity finishes. Mobs can interrupt ordinary activities.

- Fishing casts a visible rod and line, waits, reels in one of 27 weighted catches and adds it to inventory. Rare heavy catches can pull him or cause a fictional death.
- Houses assemble block by block, with four variants, a door, window, bed and torch. Right-click the house for visits, roof sitting, wolf bedtime and gift storage. House position, progress and stored gifts persist.
- The wolf follows, wags, sits, sleeps and helps repel mobs. Click to pet it; right-click for commands, rename, bone or dismissal. Adoption, name and mode persist.
- Chests animate opening, award common/rare/cursed/nested loot once and can be clicked to inspect contents. Chests expire after 150 active seconds.
- Climbing uses the visible top edge of an ordinary application window without moving or editing that window. Moving platforms carry him; minimizing or closing the window makes him fall. Very high or obscured edges are excluded.
- The Ender Dragon flies and dives, has a health bar, trades damage and can award XP effects and a trophy. Automatic boss events have a ten-minute minimum cooldown; manual summons are available in the menu.
- Clones have separate poses and lifetimes of 25–45 seconds, with a maximum of two. They never duplicate inventory or deliver files.
- Fictional death lasts four active seconds and preserves inventory. Cosmetic drops cannot affect real files.

Settings contains **BOBBYBOI WORLD** with activity toggles, automatic interval (30–300 seconds), debug logging and a confirmed world reset. Inventory and achievements are under World actions. Resetting the world preserves gift inventory and generated meme files.

**Real cursor movement is disabled by default.** Only enable the explicit checkbox if you want it. The prank only runs while the Windows desktop itself is foreground, lasts at most 1.2 wall-clock seconds and has a five-minute cooldown. Escape or a fast mouse movement cancels it; Ctrl+Alt+Backspace disables it. It sends no clicks or keystrokes. Enabled pointer movement has not been exercised during automated tests.

The world uses original procedural voxel assets and quiet synthesized sounds alongside the existing Blender character and mobs. This is a basic implementation of the nine feature areas: advanced window-to-window parkour, every proposed comedy skit, elaborate boss AI and companion interactions are still simplified. Desktop world objects are overlays; house storage belongs to BobbyBoi's inventory.

## Latest fixes

Floor throws now leave the falling pose when they land. Always on top defaults to enabled, and fullscreen hiding defaults to disabled, so switching apps keeps BobbyBoi visible without taking your typing focus. Existing installs migrate to these defaults once; settings still let you change them. Explicit Hide and going inside a house remain intentional hiding actions. Exclusive fullscreen visibility depends on Windows and the game.

Outside clicks and Escape dismiss the menu; mob updates no longer dismiss it on focus changes. The icon is now an original close-up pixel portrait in seven sizes (16–256 pixels), rather than a tiny full-body screenshot. Its editable source is assets/build_icon.ps1. Self-tests no longer overwrite it.

Additional verification commands:

- `--world-test`: activity coordination, weighted loot, save roundtrip, independent poses, fishing, progressive houses, visits, chest rewards exactly once, wolf commands, clone cleanup, boss win/loss, cursor opt-out and landing.
- `--world-smoke-test`: six complete activities using actual layered windows; no real application launches or preference saves.
- `--climb-smoke-test`: climbs a dedicated test window, follows its movement and falls after it minimizes. The fixture only moves itself.

Reports and previews are written to artifacts/world-tests.txt, world-live.txt, climb-live.txt and world-assets.png. Multi-monitor hotplug, mixed DPI, exclusive fullscreen and enabled cursor movement still need manual validation. Existing test commands are documented below.

Launch **BobbyBoi.exe** in this folder (or dist/BobbyBoi.exe). Windows 10/11 x64. The executable includes .NET; no Python, installer, administrator access, or runtime network is needed. First launch extracts bundled runtime libraries to the Windows temporary directory. This is an unsigned local build.

BobbyBoi uses an original Alex-style voxel character designed in Blender, with fuller layered hair, a long braid, complete sleeves, blinking eyes, and an animated mouth. BobbyBoi keeps his name and personality with Alex's female appearance. The editable model is assets/BobbyBoi.blend; assets/build_character.py rebuilds it inside Blender and exports assets/character.json. That mesh is embedded in the executable. Orthographic software rendering preserves crisp pixels on a per-pixel-alpha Windows layered overlay. No downloaded character or audio assets are used. This project is unofficial and is not affiliated with Mojang or Microsoft.

## Controls

- Click: wave and look toward the pointer. Double-click: jump.
- Drag the character: pick up; flick and release to throw him. Release after holding still to drop him. Throws preserve mouse momentum, arc under gravity, bounce off screen edges, and slow down on the taskbar.
- Right-click the character or tray icon: feed, diamonds, dance, sleep/wake, follow, explore, chaos, inventory, settings, pause, hide, exit.
- Double-click the tray icon, or open BobbyBoi.exe again, to show and resume him above the taskbar on the monitor containing the cursor. Instances are isolated per Windows desktop so background test sessions cannot block your launch.
- Inventory entries give the selected item and play its interaction. Counts record gifts, including food; these are not consumable game resources.
- Settings change scale, movement speed, volume/mute, speech, chaos, topmost, fullscreen pause, and optional Windows startup. Startup is off unless you enable it. Reset buttons reset position or gift counts.

The pet alternates idle, exploration, sitting, sleeping, mining, dancing, waving, running, confusion, and occasional jumps. Hunger, energy, happiness, curiosity, and mischief evolve over time; low energy leads to sleep and low hunger requests food. Chaos adds dramatic collapses and temporary dirt-block visuals. Nothing changes desktop icons, files, or other windows.

Settings and gift counts are stored in `%LOCALAPPDATA%\BobbyBoi\settings.json`, using an atomic replacement. Startup, if enabled, adds only BobbyBoi to the current user's Run registry key. Exit saves and disposes the tray icon.

## Build and verification

Install the .NET 10 SDK, then run `build.bat`. Initial publish downloads official Microsoft runtime packages from NuGet. Subsequent cached builds can use `--no-restore`. Source has no third-party package dependencies. `requirements.txt` explains the deliberate C# architecture choice.

`BobbyBoi.exe --self-test` renders all 22 animation states, checks inventory initialization, and writes artifacts/character.png and artifacts/self-test.txt. `BobbyBoi.exe --smoke-test` runs the real overlay for 30 seconds with fullscreen suspension disabled and a diagnostic taskbar entry, writes artifacts/smoke-test.txt and artifacts/live-frame.png, then exits. Close another running instance first. Normal launch has no taskbar button.

## Current limits

This is a working first release with simplified behaviors. Ordinary window top edges can be climbed; desktop icons are not obstacles. Throws cross monitor boundaries. Walking crosses side-by-side screens using a short ramp at different taskbar heights; autonomous exploration visits other monitors, using a visual jump for stacked or separated layouts. Screen gaps are traversed visually, not treated as physical platforms. Mixed-DPI layouts still need manual validation. Standing on a climbed window follows its movement and falls when it closes or minimizes. The full set of suggested comedy skits is not implemented. Poses are procedural; several transition states share the idle pose and activity targets use a short pose blend. Sounds are quiet original synthesized tones rather than Minecraft samples. The menu uses a dark native Windows menu with monospace type. Fullscreen detection is heuristic. The optional fullscreen pause setting hides the pet; it is now off by default. World activities suspend during detected fullscreen apps. Ordinary maximized windows are excluded. Exclusive fullscreen overlays depend on Windows and the game. Performance and high-DPI behavior vary by machine; 60 FPS is a timer target, not a guarantee. No clean second-PC or mixed-DPI monitor test has been performed.


Launch diagnostics are written to `%LOCALAPPDATA%\BobbyBoi\launch.log`. `--diagnose` also writes native window/desktop details to artifacts/diagnostics.txt for troubleshooting.



Falling raises both arms, spreads the legs, and increases shaking and mouth opening with downward speed. Explore asks him to visit another monitor immediately; he also periodically chooses new monitors himself.


All eight gift types have original extruded pixel-art 3D models. Giving an item displays it in BobbyBoi's right hand for eight seconds, attached to the arm throughout his interaction. Food moves toward his face while eating; gems are held aloft; tools move with their associated animation. No downloaded assets or runtime network requests are used.


## Meme Expeditions

Enabled by default. The first countdown is 20 active seconds after launch. BobbyBoi looks both ways, accelerates off an exposed outer desktop edge, stays fully outside the virtual desktop for at least 5 seconds, returns pulling an actual content window, raises it, and sets it down. He then waits a new random 120�180 active seconds, measured from completion of the delivery animation. Sleeping, dragging, gifts and critical animations postpone departure. Pause, hide and fullscreen suspension freeze expeditions. Opening the context menu pauses a trip. Picking up the pet, giving a gift, or requesting sleep/dance temporarily pauses a trip; it resumes afterward. Monitor removal recovers the pet to a visible monitor.

Each successful trip saves one original TXT or PNG in **BobbyBoi Memes** under Windows' resolved Desktop directory (including normal OneDrive Known Folder redirection). Unique timestamp/GUID filenames are created with CreateNew and never replace an existing file. Generated bytes are prepared in memory before writing; saved files are never automatically deleted. No executables, scripts, network downloads, AI services, or external dependencies are used.

Content includes 40 TXT concepts, 30 illustrated pixel-art meme scenes and 12 intentionally terrible Paint themes with seeded variations. Each category uses a shuffled deck so concepts do not repeat until its deck has been exhausted. Special documents include the emerald bank statement and classified chicken report; drawings have their own dialogue. A generation/storage failure returns the pet safely with a failed-delivery reaction rather than crashing.

By default, deliveries open the actual generated PNG in Microsoft Paint and the actual TXT in Windows Notepad. BobbyBoi moves that newly created application window in from the edge and releases it at a visible size. You can edit the real file normally afterward; the pet no longer controls that window after dropping it and never closes it for you. Existing windows are recorded before launch and are never moved. If the application reuses an existing window/tab, the file is opened there but that pre-existing window is left untouched. Apps may briefly appear while starting. Windows Paint and Notepad must be installed; Windows 11 Paint execution aliases are supported. Turn off Use real Paint / Notepad windows in expedition settings to use the built-in image/document viewer instead. Copies are always retained in BobbyBoi Memes.

Right-click options: **Send on Meme Expedition** and **Open BobbyBoi Memes Folder**. Settings has a **Meme Expeditions** section: enabled, expedition sounds, contents while carrying, opt-in automatic PNG opening in Paint (off by default), minimum/maximum cooldown in seconds, optional session limit (0 = unlimited), manual departure and folder access. Manual departures use the same scheduler and cannot overlap. Minimum configurable frequency is 30 seconds; the default remains 120�180. Normal pet sound volume/mute also applies.

Verification commands (close any instance on the same Windows desktop first):

- `BobbyBoi.exe --expedition-test`: validates every concept, file format/dimensions, unique saves, seeded drawings, cancellation, correct file-launch targets and repeated generation, plus deterministic scheduler tests. Test artifacts go under artifacts/expedition-tests, not your Desktop.
- `BobbyBoi.exe --expedition-smoke-test`: runs one real-time 20-second first departure and complete native overlay delivery, logs state timestamps and writes a content-window preview. Uses artifacts/expedition-live-output, automatic opening disabled, and does not save test preferences.
- `BobbyBoi.exe --expedition-stress-test`: runs 12 full expeditions at an accelerated scheduler rate, including native rendering, background generation and content-window replacement/disposal. Uses artifacts/expedition-stress-output. Timing assertions come from the real-time run and deterministic clock test, not the accelerated run.

Logs: artifacts/expedition-tests.txt, expedition-live.txt and expedition-stress.txt. File-launch targets are tested without opening external applications during automated tests; actual viewer behavior depends on the user's Windows associations. Mixed-DPI monitor hotplug and automatic Paint opening have not been exercised on a second PC. The existing fullscreen heuristic and software rendering limits still apply. Expeditions travel to an outer edge of the full monitor layout so the entire character window and cargo really leave all screens. On offset layouts, the existing walking floor transition is reused.



## Zombies, creepers and explosions

Every **60 active seconds**, a mixed group of 3�5 mobs spawns near BobbyBoi on the current desktop. The group always includes a zombie and a creeper. Zombies pursue/shamble; creepers approach, flash through a two-second fuse and explode. A creeper also eventually detonates if BobbyBoi is away. Explosions have original synthesized audio, a flash, expanding shockwave, smoke/sparks, gravity-driven pixel debris, knockback for BobbyBoi and other mobs, and nearby creeper chain reactions. Everything is app-only: no real files, icons, user input or unrelated windows are damaged or moved by mobs/explosions.

Right-click BobbyBoi ? **Spawn mobs**: mixed group, single zombie, single creeper, detonate existing creepers, toggle natural spawning, or clear all mobs/effects. Natural spawning is on by default and persists. Settings also has the natural-spawn checkbox. Clear resets the next natural-spawn timer to 60 seconds. At most eight mobs can be alive; survivors despawn after 55 active seconds. Mobs and effects are fully click-through, pause/hide with BobbyBoi, respect fullscreen suspension, and are disposed on exit. Sounds use the existing volume/mute setting.

Editable Blender models: `assets/BobbyBoiMobs.blend`; rebuild/export script: `assets/build_mobs.py`. The zombie and creeper JSON meshes are embedded in the standalone EXE. Blender is not required at runtime.

`BobbyBoi.exe --mob-test` checks embedded meshes, the minute timer, group sizes, pursuit, timed/proximity fuses, explosions, knockback, cleanup and spawn caps; writes artifacts/mob-tests.txt and mobs-preview.png. `--mob-smoke-test` runs an 82-second native-overlay test, with expeditions disabled and audio muted only for that test, and records actual spawn/explosion timestamps in artifacts/mob-live.txt. `--native-delivery-test` explicitly opens one generated TXT in real Notepad and one PNG in real Paint and tests attaching/moving their newly-created windows; it leaves the dropped applications open for review. The latter test must run on the interactive Windows desktop, not a sandbox desktop.

## Restart and recent fixes

**Restart BobbyBoi** saves settings, shuts down the current instance cleanly, and launches a new one. If a newer, different `dist/BobbyBoi.exe` exists beside the installed executable, a helper waits for the old process to exit, applies that staged build, then relaunches. There are no network downloads and no SDK is needed for a normal restart. Source changes must first be compiled with build.bat; if BobbyBoi is running, that script leaves the new EXE in dist and tells you to use the restart menu. `--restart-test` verifies update discovery/copy logic. `--restart-running` requests the same restart action from an instance on the same Windows desktop.

**Clear Mobs** is now directly in the main menu. Mobs have separated spawn slots and a position-separation solver, so pursuing groups cannot occupy the same point. Creeper meshes use depth-tested rasterization to prevent skin surfaces overwriting the eyes, mouth and feet. Facial features stay dark during fuse flashes. Mob overlays do not repeatedly reset their topmost state.

BobbyBoi tries to flee nearby zombies and creepers, and may jump when cornered. Zombie contact deals two damage with attack cooldowns; nearby creeper explosions deal eight. Damage has brief invulnerability, knockback, a 0.32-second red tint and a temporary health display. At zero health BobbyBoi collapses and respawns after four active seconds; saved inventory is preserved. Combat postpones due expeditions without resetting their countdown.

Menus use explicit outside-click/Escape/action dismissal and wait until the opening mouse press is released before arming the outside-click check. This avoids mob overlay focus changes closing the menu. `--menu-test` checks actual spawn/clear handlers, damage, fleeing and menu configuration. `--menu-live-test` checks the native popup remains visible while five mobs update and render.

For separate real TXT delivery windows, Windows 11 Notepad's **Opening files** preference must be **Open in a new window**. Set this preference yourself in Notepad settings if you want separate TXT delivery windows. BobbyBoi never moves a pre-existing multi-tab Notepad window. Paint opens the actual PNG file in its own application window; no image viewer substitute is used when real-window deliveries are enabled.
