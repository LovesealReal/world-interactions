# World Interactions — Contact Protocol

This is the small convention that makes avatars and worlds interoperable. Anything that
follows it — whether built with a World Interactions package, the Club Maul Staff Scanner,
VRCFury, or by hand — is compatible with anything else that follows it.

## Avatar side: broadcasting a feature

A feature broadcast is a **VRCContactSender** with:

| Property | Value |
|---|---|
| Shape | Sphere, **radius 0.5** |
| Position | Pinned to the **world origin** — parent the sender under a GameObject holding a `VRCParentConstraint` whose source is a world-space transform at (0,0,0) |
| Local Only | **On** — the sender exists only on the wearer's client |
| Collision Tags | One tag per feature (see naming below) |
| Enabled | Off by default, toggled by the wearer (e.g. an expressions-menu toggle) |

Pinning everything to the origin makes this a pure *flag broadcast* system: proximity never
matters, only whether the toggle is on. Local-only senders mean the world always knows **which
player** is broadcasting — it's the local player of whichever client detects the contact.
Worlds that want everyone to react sync the state themselves via Udon networking.

## World side: receiving

A **VRCContactReceiver** (sphere, any positive radius, also at the world origin) with a
matching collision tag. In worlds, receivers emit `OnContactEnter` / `OnContactExit` Udon
events to UdonBehaviours on the same GameObject — that's what the World Interactions package
generates for you at build time.

## Tag naming

Tags are namespaced `Vendor/Feature`:

| Tag | Meaning |
|---|---|
| `WI/Slow` | Standard Slow request: reduce player movement speed while active. |
| `WI/Rumble` | Standard Rumble request: pulse controller haptics while active. |
| `WI/Presence` | Standard presence beacon: "I'm wearing the badge" — broadcast while the wearer's system is on, used by worlds to exempt wearers from effects. |
| `MyWorld/...` | Anything else: define your own tags under your own vendor prefix. |

### Known vendor ecosystems

| Tag | Ecosystem |
|---|---|
| `ClubMaul/Slow`, `ClubMaul/Rumble` | Club Maul Staff Scanner V2 world features. |
| `ClubMaul/Scanner/Show` | Staff Scanner V2 presence beacon. |
| `ClubMaulShow` | Staff Scanner V1 legacy presence beacon (no vendor prefix, predates this convention). |

Deployed Staff Scanner avatars currently broadcast only the `ClubMaul/*` tags; a future
scanner update is planned to additionally broadcast the standard `WI/*` tags (senders carry a
tag *list*, so this is additive and breaks nothing). Until the avatar fleet has re-uploaded,
worlds that want Club Maul compatibility should list both tag sets.

Worlds are encouraged to accept both the `WI/*` standard tags and any vendor tags relevant to
their community — every tag field in World Interactions is a plain string list precisely so
ecosystems can be mixed.

## Semantics

- **Enter = feature on, exit = feature off.** Receivers should tolerate multiple simultaneous
  matching senders (count enters/exits rather than assuming one).
- **Presence beacons** should be broadcast whenever the wearer's system is active, not gated
  behind a sub-toggle, so exemption is dependable.
- A sender GameObject being disabled, the avatar changing, or the player leaving all end the
  contact — worlds must treat `OnContactExit` as authoritative for "off".
