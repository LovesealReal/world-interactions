# World Interactions

The world-side counterpart to the **[Staff Scanner V2](https://github.com/Club-Maul/staff-scanner-v2)** avatar package. Drop one prefab into your VRChat world and it reacts to the scanner's contact broadcasts — with built-in **Slow** and **Rumble** effects, automatic **staff exemption**, and easy hooks for your own UdonSharp code.

## Installation

1. Drag the **Club Maul World Interactions** prefab into your scene (or use **Tools → Club Maul → Initialize World Interactions**). Its position doesn't matter.
2. That's it for the defaults — Slow and Rumble are enabled out of the box.

Like the avatar package, **all contacts are created at build time**: the prefab only holds a config component, and when you build (or enter Play Mode) the generator creates the contact receivers, relays, and effect handlers pinned to the world origin, then strips the config from the built scene.

## How it works

Staff Scanner avatars pin small contact senders to the **world origin**, one per feature, each broadcasting a collision tag (e.g. `ClubMaul/Slow`). Those senders are local-only, so they exist only on the client of the player wearing the scanner.

This package generates a matching `VRCContactReceiver` at the world origin for each tag. In worlds, contact receivers send `OnContactEnter` / `OnContactExit` Udon events, so when the local player's scanner toggles a feature on, the world knows — and can either react locally or sync the state to the whole instance.

### Built-in features

| Feature | Tag | Default behaviour |
|---|---|---|
| **Slow** | `ClubMaul/Slow` | Multiplies walk/run/strafe speed by 0.5× for everyone in the instance while active. |
| **Rumble** | `ClubMaul/Rumble` | Pulses both controllers' haptics for everyone in the instance while active. |

Both are **instance-wide** by default: the activating client syncs the state so every player is affected, including late joiners. Both **exempt staff**: any player whose own Staff Scanner presence beacon is active is skipped.

### Staff detection

A generated **Staff Scanner Detector** listens for the scanner's presence tags (`ClubMaul/Scanner/Show`, plus the V1 scanner's legacy `ClubMaulShow`). Because the scanner's senders are local-only, this receiver can only ever hear the *local* player's scanner — making it a reliable "is this player staff?" check. Effects with **Exempt Staff** enabled simply never fire on staff clients.

Your own code can check it too: the detector exposes a public `IsLocalPlayerStaff` bool.

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
   - **Collision Tag** — the tag the avatar broadcasts (e.g. `MyWorld/Confetti`). On the avatar side, creators add the same tag via a Staff Scanner **plugin** or their own world-origin contact sender.
   - **Instance Wide** — off: events fire only on the client whose avatar sent the contact; on: state is synced and events fire for everyone (late joiners included).
   - **Exempt Staff** — skip the events on staff clients.
   - **Target / Start Event / End Event** — your behaviour and method names.

A matching receiver + relay is generated at the next build. The relay also exposes a public `IsActive` bool you can poll.

An `ExampleInteractionHandler` (toggles a GameObject on/off) ships with the package as a starting point.

## Notes & limitations

- **Instance-wide state is last-writer-wins**: if two scanner wearers toggle the same feature, whoever changed it last controls the synced state. A wearer whose contact is still live re-asserts it automatically, and stale state is cleared if the activator leaves.
- The Slow effect captures the player's speeds when it starts and restores them after; if another system changes movement speeds mid-effect, those changes are overwritten on restore.
- Contact receivers can't be tested with ClientSim alone (there's no avatar to send contacts); test with a Staff Scanner avatar in a real instance or with the SDK's Build & Test.

## Requirements

- VRChat Worlds SDK (`com.vrchat.worlds`) 3.7.0 or newer (includes UdonSharp)
