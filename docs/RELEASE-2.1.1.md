# 2.1.1 — Small pet. Terrible decisions.

The first public GitHub release of BobbyBoi: a standalone Windows desktop companion with an Alex-inspired voxel character, multi-monitor throwing, original sound effects, persistent companions and an unreasonable number of hobbies.

### Inside the box

- Transparent desktop pet with gifts, inventory, personality and Minecraft-inspired speech bubbles.
- Fishing, houses, wolf companion, treasure, clones, window climbing and a tiny dragon fight.
- Ten connected expansion events: prison, casino, Nether portal, Herobrine, earthquakes, fake updates, pet creeper, BobbyTube, UFOs and cave mining.
- Actual TXT/PNG meme deliveries in new Notepad/Paint windows.
- 55 synthesized SFX with overlapping playback, plus volume/mute controls.
- Interrupted activities resume their existing stage; explicit Cancel still stops them.
- Bounded sprite caches, small redraw regions and an optional performance mode.

### Download

Extract `BobbyBoi-Windows-x64.zip`, then run `BobbyBoi.exe`.

Windows 10/11 x64. Self-contained .NET runtime; no installer or administrator access required. This is an unsigned local build.

The ZIP includes the executable, quick-start guide and SHA-256 checksum. Source and editable Blender projects are in the repository.

### Validation

Render/physics, sound, mobs, menus, expeditions, world activities, expansion flows and interruption regression checks passed locally. The ten-event native overlay run and Windows audio-device playback check also passed. Mixed-DPI, monitor hotplug and exclusive fullscreen behavior still need broader testing.

Unofficial fan project; not affiliated with Mojang or Microsoft.
