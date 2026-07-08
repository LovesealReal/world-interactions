# World Interactions

Let avatars talk to your VRChat world through contact broadcasts. Drop one prefab into your scene and it reacts to compatible avatar contacts — with built-in **Slow** and **Rumble** effects, automatic **presence-beacon exemption** (e.g. staff badges), and easy hooks for your own UdonSharp code.

World Interactions is a general-purpose system, unaffiliated with any community. It's cross-compatible with the [Club Maul Staff Scanner V2](https://github.com/Club-Maul/staff-scanner-v2) via the **Club Maul preset package** (`com.clubmaul.world-interactions`), and any avatar system that follows the small contact convention in [PROTOCOL.md](PROTOCOL.md).

## Installation

1. Drag the **World Interactions** prefab into your scene (or use **Tools → World Interactions → Initialize World Interactions**). Its position doesn't matter.
2. That's it for the defaults — Slow and Rumble are enabled out of the box on the standard `WI/*` tags.

**All contacts are created at build time**: the prefab only holds a config component, and when you build (or enter Play Mode) the generator creates the contact receivers, relays, and effect handlers pinned to the world origin, then strips the config from the built scene.

## How it works

Compatible avatars pin small, local-only contact senders to the **world origin**, one per feature, each broadcasting a collision tag (see [PROTOCOL.md](PROTOCOL.md)). Because the senders are local-only, they exist only on the client of the player wearing them.

This package generates a matching `VRCContactReceiver` at the world origin for each tag. In worlds, contact receivers send `OnContactEnter` / `OnContactExit` Udon events, so when the local player's avatar toggles a feature on, the world knows — and can either react locally or sync the state to the whole instance.

### Built-in features

| Feature | Default tag | Default behaviour |
|---|---|---|
| **Slow** | `WI/Slow` | Multiplies walk/run/strafe speed by 0.5× for everyone in the instance while active. |
| **Rumble** | `WI/Rumble` | Pulses both controllers' haptics for everyone in the instance while active. |

Both are **instance-wide** by default: the activating client syncs the state so every player is affected, including late joiners. Both **exempt presence wearers**: any player whose own presence beacon is active is skipped.

### Presence detection

A generated **Presence Detector** listens for presence-beacon tags (default `WI/Presence`). Because compatible beacons are local-only, this receiver can only ever hear the *local* player's beacon — making it a reliable "is this player wearing the badge?" check. Effects with **Exempt Presence** enabled simply never fire on those clients.

Your own code can check it too: the detector exposes a public `IsLocalPlayerBroadcasting` bool.

## Adding your own interactions

1. Write an UdonSharpBehaviour with a public method per event:

   ```csharp
   public class ConfettiHandler : UdonSharpBehaviour
   {
       public ParticleSystem Confetti;
       public void OnConfettiStart() => Confetti.Play();
       public void OnConfettiEnd()   => Confetti.Stop();
   }
   ```

2. Add it to any object in your scene.
3. On the **World Interactions** component, add an entry under **Custom Interactions**:
   - **Name** — label for the generated object (e.g. `Confetti`).
   - **Collision Tag** — the tag the avatar broadcasts (e.g. `MyWorld/Confetti`).
   - **Instance Wide** — off: events fire only on the client whose avatar sent the contact; on: state is synced and events fire for everyone (late joiners included).
   - **Exempt Presence** — skip the events on presence-wearer clients.
   - **Target / Start Event / End Event** — your behaviour and method names.

A matching receiver + relay is generated at the next build. The relay also exposes a public `IsActive` bool you can poll.

An `ExampleInteractionHandler` (toggles a GameObject on/off) ships with the package as a starting point.

## Cross-compatibility with Club Maul

The [Club Maul preset package](https://github.com/LovesealReal/world-interactions/tree/main/Packages/com.clubmaul.world-interactions) ships a prefab pre-configured for the Staff Scanner V2 ecosystem (`ClubMaul/Slow`, `ClubMaul/Rumble`, staff detection via `ClubMaul/Scanner/Show` + legacy `ClubMaulShow`).

You can also mix ecosystems by hand — the tag fields are just strings. For example, a world can honor both its own community badge *and* Club Maul staff by listing multiple Presence Tags.

## Notes & limitations

- **Instance-wide state is last-writer-wins**: if two broadcasters toggle the same feature, whoever changed it last controls the synced state. A broadcaster whose contact is still live re-asserts it automatically, and stale state is cleared if the activator leaves.
- The Slow effect captures the player's speeds when it starts and restores them after; if another system changes movement speeds mid-effect, those changes are overwritten on restore.
- Contact receivers can't be tested with ClientSim alone (there's no avatar to send contacts); test with a compatible avatar in a real instance or with the SDK's Build & Test.

## Requirements

- VRChat Worlds SDK (`com.vrchat.worlds`) 3.7.0 or newer (includes UdonSharp)
