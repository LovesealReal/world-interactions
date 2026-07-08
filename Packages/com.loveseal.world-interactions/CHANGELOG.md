# Changelog

All notable changes to the World Interactions package are documented in this file.

## [1.0.0] - 2026-07-08

### Added

- Initial release.
- Build-time generator: drops a config-only prefab in the scene and generates all
  world-origin contact receivers, relays, and effect handlers when the world builds.
- Built-in **Slow** effect (`WI/Slow`): scales player movement speed while active.
- Built-in **Rumble** effect (`WI/Rumble`): pulses controller haptics while active.
- Built-in features and presence detection accept **tag lists**, so one world can serve
  multiple ecosystems (e.g. `WI/Slow` + `ClubMaul/Slow`) with a single receiver each.
- **Presence detection** (`WI/Presence`): built-in effects skip presence-beacon wearers
  (e.g. staff badges), and `PresenceDetector.IsLocalPlayerBroadcasting` is available to
  creator code.
- **Custom interactions**: map any collision tag to start/end custom events on your own
  UdonSharpBehaviour, per-interaction instance-wide syncing (with late-joiner support)
  and presence exemption.
- `PROTOCOL.md` documenting the avatar/world contact convention, including the Club Maul
  Staff Scanner tags for cross-compatibility.
- `ExampleInteractionHandler` sample script.
- Menu items: Tools → World Interactions → Initialize World Interactions, and GameObject → World Interactions.
