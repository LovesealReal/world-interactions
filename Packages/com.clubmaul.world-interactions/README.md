# Club Maul World Interactions

The Club Maul preset for **[World Interactions](https://github.com/LovesealReal/world-interactions)** — the world-side counterpart to the **[Staff Scanner V2](https://github.com/Club-Maul/staff-scanner-v2)** avatar package.

Installing this package pulls in the World Interactions core and adds a prefab pre-configured for the Club Maul ecosystem:

- **Slow** on `ClubMaul/Slow` and **Rumble** on `ClubMaul/Rumble` — the Staff Scanner's world features, instance-wide by default.
- **Staff detection** on `ClubMaul/Scanner/Show` (plus the V1 scanner's legacy `ClubMaulShow`, and the standard `WI/Presence`) — staff scanner wearers are automatically exempt from the effects, and your code can check `PresenceDetector.IsLocalPlayerBroadcasting`.

## Installation

1. Drag the **Club Maul World Interactions** prefab into your scene (or use **Tools → Club Maul → Initialize World Interactions**). Its position doesn't matter.
2. Done — everything else (contact receivers, relays, effects) is generated at build time by the core package.

## Custom interactions

Add your own tags and code on the same component — see the [World Interactions README](https://github.com/LovesealReal/world-interactions/blob/main/Packages/com.loveseal.world-interactions/README.md) and [PROTOCOL.md](https://github.com/LovesealReal/world-interactions/blob/main/Packages/com.loveseal.world-interactions/PROTOCOL.md). On the avatar side, tags can be added with a Staff Scanner **plugin** asset.

## Requirements

- World Interactions (`com.loveseal.world-interactions`) 1.0.0+ (installed automatically via VCC)
- VRChat Worlds SDK (`com.vrchat.worlds`) 3.7.0+
