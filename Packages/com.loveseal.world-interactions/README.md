# World Interactions

Let avatars talk to your VRChat world through contact broadcasts. Drop one prefab into your scene and it reacts to compatible avatar contacts — with built-in **Slow** and **Rumble** effects, automatic **presence-beacon exemption** (e.g. staff badges), and easy hooks for your own UdonSharp code.

World Interactions is a general-purpose system, unaffiliated with any community. Any avatar system that follows the small contact convention in [PROTOCOL.md](PROTOCOL.md) works with it.

## Installation

1. Drag the **World Interactions** prefab into your scene (or use **Tools → World Interactions → Initialize World Interactions**). Its position doesn't matter.
2. That's it for the defaults — Slow and Rumble are enabled out of the box on the standard `WI/*` tags.

**All contacts are created at build time**: the prefab only holds a config component, and when you build (or enter Play Mode) the generator creates the contact receivers, relays, and effect handlers pinned to the world origin, then strips the config from the built scene.

## How it works

Compatible avatars pin small, local-only contact senders to the **world origin**, one per feature, each broadcasting a collision tag (see [PROTOCOL.md](PROTOCOL.md)). Because the senders are local-only, they exist only on the client of the player wearing them.

This package generates a matching `VRCContactReceiver` at the world origin for each tag. In worlds, contact receivers send `OnContactEnter` / `OnContactExit` Udon events, so when the local player's avatar toggles a feature on, the world knows — and can either react locally or sync the state so other players react too.

### Built-in features

| Feature | Default tags | Default behaviour |
|---|---|---|
| **Slow** | `WI/Slow` | Multiplies walk/run/strafe speed by 0.5× for players within range while active. |
| **Rumble** | `WI/Rumble` | Pulses both controllers' haptics for players within range while active. |

Each feature listens on a **list of tags**, so you can accept several ecosystems at once (e.g. a community's vendor tags next to the standard ones). Both are **synced and ranged** by default: the broadcasting client syncs the state (late joiners included), and each player is affected while within the **effect range** — default 8 m around the broadcaster, configurable per feature, `0` = the entire instance. Selecting the prefab shows a **wire-sphere gizmo per range** in the scene view for scale reference. Both features **exempt presence wearers**: any player whose own presence beacon is active is skipped.

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
   - **Synced** — off: events fire only on the broadcaster's own client; on: state is synced and events fire for players within range (late joiners included).
   - **Effect Range** — radius in meters around the broadcaster (synced only), previewed as a gizmo; `0` = entire instance.
   - **Exempt Presence** — skip the events on presence-wearer clients.
   - **Target / Start Event / End Event** — your behaviour and method names.

A matching receiver + relay is generated at the next build. The relay also exposes a public `IsActive` bool you can poll.

An `ExampleInteractionHandler` (toggles a GameObject on/off) ships with the package as a starting point.

## Supporting other ecosystems

Every tag field is a plain string list, so a world can serve several avatar ecosystems at once: add a vendor's tags (e.g. `MyClub/Slow`) alongside the standard `WI/*` entries on the same feature, and list every badge system you honor under Presence Tags. Vendor communities are encouraged to publish their own short compatibility guide listing the tags their avatars broadcast.

## Notes & limitations

- **Synced state is last-writer-wins**: if two broadcasters toggle the same feature, whoever changed it last controls the synced state — and the effect range follows that player only. A broadcaster whose contact is still live re-asserts the state automatically, and stale state is cleared if the activator leaves.
- Range is evaluated per client every 0.25 s against the broadcaster's position, so entering/leaving the radius has a small reaction delay.
- The Slow effect captures the player's speeds when it starts and restores them after; if another system changes movement speeds mid-effect, those changes are overwritten on restore.
- Contact receivers can't be tested with ClientSim alone (there's no avatar to send contacts); test with a compatible avatar in a real instance or with the SDK's Build & Test.

## Requirements

- VRChat Worlds SDK (`com.vrchat.worlds`) 3.7.0 or newer (includes UdonSharp)
