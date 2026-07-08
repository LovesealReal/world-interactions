# Changelog

All notable changes to the World Interactions package are documented in this file.

## [1.0.0] - 2026-07-08

### Added

- Initial release.
- Build-time generator: drops a config-only prefab in the scene and generates all
  world-origin contact receivers, relays, and effect handlers when the world builds
  (mirroring how the Staff Scanner V2 avatar package builds its contacts).
- Built-in **Slow** effect (`ClubMaul/Slow`): scales player movement speed while active.
- Built-in **Rumble** effect (`ClubMaul/Rumble`): pulses controller haptics while active.
- **Staff Scanner detection** (`ClubMaul/Scanner/Show` + legacy `ClubMaulShow`): built-in
  effects skip scanner wearers, and `StaffScannerDetector.IsLocalPlayerStaff` is available
  to creator code.
- **Custom interactions**: map any collision tag to start/end custom events on your own
  UdonSharpBehaviour, per-interaction instance-wide syncing (with late-joiner support)
  and staff exemption.
- `ExampleInteractionHandler` sample script.
- Menu items: Tools → Club Maul → Initialize World Interactions, and GameObject → Club Maul → World Interactions.
