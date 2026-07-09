# Changelog

All notable changes to the World Interactions package are documented in this file.

## [1.0.0] - 2026-07-08

### Added

- Initial release.
- Build-time generator: drops a config-only prefab in the scene and generates all
  world-origin contact receivers, relays, and effect handlers when the world builds.
- Built-in **Slow** effect (`WI/Slow`): scales player movement speed while active.
- Built-in **Rumble** effect (`WI/Rumble`): pulses controller haptics while active.
- Synced effects are **ranged**: they apply within a configurable radius around the
  broadcasting player (default 8 m, `0` = entire instance), with a scene-view gizmo
  previewing each range while the prefab is selected.
- **Falloff** per interaction (None/Linear/Smooth): rumble strength and slow amount fade
  toward the range edge; handlers read intensity via `LocalIntensity`/`GetIntensity`.
- **World Objects range mode**: interactions can range over scene objects instead of
  players, with a bundled `ProximityObjectToggler` handler (e.g. lights that flicker
  only within the broadcaster's radius).
- Relays inject themselves into a handler's `SourceInteraction` variable before events,
  exposing `LocalIntensity`, `GetIntensity(position)`, and `BroadcasterPosition`.
- Built-in features and presence detection accept **tag lists**, so one world can serve
  multiple ecosystems (the standard tags plus any vendor tags) with a single receiver each.
- **Presence detection** (`WI/Presence`): built-in effects skip presence-beacon wearers
  (e.g. staff badges), and `PresenceDetector.IsLocalPlayerBroadcasting` is available to
  creator code.
- **Custom interactions**: map any collision tag to start/end custom events on your own
  UdonSharpBehaviour, with per-interaction syncing (late joiners included), effect range,
  and presence exemption.
- `PROTOCOL.md` documenting the avatar/world contact convention.
- `ExampleInteractionHandler` sample script.
- Menu items: Tools → World Interactions → Initialize World Interactions, and GameObject → World Interactions.
