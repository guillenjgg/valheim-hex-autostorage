# HexAutoStorage

Automatically stores bars, coal, flour, linen thread, eitr, Bloodgold, and LiquidFrost produced by supported vanilla production stations in nearby containers.

> ## ⚠️ Important
> **HexAutoStorage only supports items produced by supported vanilla production stations. Mods that add custom production stations, such as custom windmills or smelters, are not guaranteed to be supported.**
>
> Containers must be public and within the configured storage radius. The player who placed the container or production station does not matter. Private and group containers are skipped.

## How to Use

HexAutoStorage works automatically. **Containers must either have a matching Auto Storage tag or already contain the produced item.**

Place a supported storage container within range of a supported production station, then either tag it for the produced item or place at least one of that item inside it.

Tags let you use an empty container for a specific resource. Without tags, leave at least one of the desired item inside the container so HexAutoStorage continues using it.

When a supported production station creates an item, HexAutoStorage searches nearby containers in the following order:

1. **Matching tagged containers**
   * A container tagged for the produced item is always preferred.
   * Matching tagged containers are checked from nearest to farthest.

2. **Untagged containers that already contain the produced item**
   * If no matching tagged container has room, HexAutoStorage looks for untagged containers that already contain that item.
   * These containers are checked from nearest to farthest.

3. **Normal Valheim behavior**
   * If no matching tagged container or untagged container already holding the item has room, the production station drops the item normally.

Containers with tags that do not match the produced item are skipped. Untagged containers that do not already contain the produced item are also skipped, even if they have empty slots.

### Example

You have three nearby chests:

* Chest A is tagged `Iron`
* Chest B is untagged and already contains Iron
* Chest C is untagged and empty

When a Smelter produces Iron:

1. HexAutoStorage tries Chest A first
2. If Chest A is full, it tries Chest B
3. If Chest B is full, the Iron drops normally. Chest C is skipped because it is untagged and does not contain Iron.

If none of the chests are tagged, HexAutoStorage only uses chests that already contain Iron.

For example, if Chest B already contains Iron and has room, it will receive the Iron. If no untagged container already contains Iron and has room, the Iron drops normally.

## Features

* Automatically stores produced items in nearby supported containers
* Container tags are optional
* Supports the following production stations:
  * Smelter
  * Charcoal Kiln
  * Frost Kiln
  * Blast Furnace
  * Windmill
  * Spinning Wheel
  * Eitr Refinery
* Supports the following storage containers:
  * Wood Chest
  * Barrel
  * Reinforced Chest
  * Black Metal Chest
  * Cart
* Configurable storage radius
* Optional storage radius visualization when hovering over a production station
* Container tags allow specific resources to be routed to specific containers
* Container hover text displays assigned Auto Storage tags
* Tagged containers take priority over normal storage
* Storage falls back to untagged containers already holding the item when tagged containers are full

## Screenshots

![HexAutoStorage](https://raw.githubusercontent.com/guillenjgg/valheim-hex-mod-images/main/hexautostorage/hexautostorage_1.png)
![HexAutoStorage](https://raw.githubusercontent.com/guillenjgg/valheim-hex-mod-images/main/hexautostorage/hexautostorage_2.png)
![HexAutoStorage](https://raw.githubusercontent.com/guillenjgg/valheim-hex-mod-images/main/hexautostorage/hexautostorage_3.png)

## Container Tags

Container tags allow you to choose where specific resources are stored, including empty containers. Untagged containers only receive resources they already contain.

Look at a supported container and press **Left Shift + P** by default, or use your configured **Edit Tags Shortcut**, to open the Auto Storage tag editor.

Multiple tags can be assigned to the same container by separating them with commas.

Example:

`Iron,Copper,Coal`

Supported tags:

* Copper
* Tin
* Bronze
* Iron
* Silver
* Blackmetal
* Flametal
* Coal
* Flour
* Linen
* Eitr
* Bloodgold
* LiquidFrost

Tags are not case-sensitive. Use `Bloodgold` and `LiquidFrost` for the Deep North resources; `LiquidFrost` is one word. The internal prefab names do not need to be entered by the player.

To remove tags, clear the tag editor and save. The container will then only receive produced items it already contains.

You must be close enough to interact with the chest and have access under its privacy and ward settings. A confirmation appears when the chest's network owner saves your changes. If the save is not confirmed within 10 seconds, reopen the editor to check the saved tags. See **Multiplayer** below for installation requirements.

### Tag Example

A chest tagged:

`Iron,Copper`

will be preferred whenever a nearby supported production station produces Iron or Copper.

If that chest is full, HexAutoStorage will try another matching tagged container before falling back to an untagged container already holding the produced item. If none has room, the item drops normally.

A tagged container will only accept produced items that match one of its assigned tags. It will not be used as fallback storage for other resources.

## Storage Radius

Production stations only search for containers within the configured **StorageRadius**.

The default radius is **5 meters**, configurable from **5 to 100 meters**. Detection uses container collider overlap, so a large container's center can be slightly outside the radius while its edge is within range.

The storage radius can optionally be displayed by enabling:

`ShowRadiusVisual = true`

When enabled, hover over a supported production station to display its storage radius.

## Configuration

Configuration options are available in:

`BepInEx/config/com.hex.autostorage.cfg`

Current configuration options include:

* **Enabled** — Enable or disable HexAutoStorage
* **EnableDebugLogging** — Enable diagnostic logs for storage and tag saves in Debug or Release builds. Defaults to `false`. Plugin load/unload logs always remain enabled. This setting does not change production speed.
* **StorageRadius** — Radius in meters used to search for nearby storage
* **ShowRadiusVisual** — Display the Auto Storage radius while hovering over a supported production station
* **EditTagsShortcut** — Keyboard shortcut used to edit container Auto Storage tags

The **EditTagsShortcut** configuration description also contains the list of supported tag names for reference.

For troubleshooting, set the following under `[General]` and restart the game or server:

```ini
EnableDebugLogging = true
```

Diagnostics include storage decisions, tag-save requests, and owner confirmations. They are written to `BepInEx/LogOutput.log`. Enable logging on both the client and dedicated server when investigating a multiplayer issue. Set it back to `false` when finished.

## Multiplayer

**Install the same version of HexAutoStorage on every player's client. For dedicated-server multiplayer, install it on the server too so it can handle tag-save requests when it owns a chest.**

Valheim uses two separate kinds of ownership:

* **Creator:** The player who placed the station or container. Auto storage does not require matching creators.
* **Network owner:** The client or server currently responsible for saving an object. Valheim manages this automatically, and it can change during play.

Several production stations can share one public container, even when different players placed them. Each station still checks its own radius, matching tags or existing items, and available capacity. Private and group containers are excluded, even when the same player placed them.

**Limitation:** Remotely managed chests are still skipped to prevent unsaved deposits. Removing the creator restriction allows different players' stations and public chests to work together, but the computer processing production must also be the chest's current network owner.

For example, if your computer processes a kiln and another player's computer or the server manages the public chest, that kiln skips the chest—even if it has a matching tag and plenty of room. HexAutoStorage tries another eligible container or lets the item drop normally. It does not take ownership away from another player. Installing the mod on every client and the server does not remove this limitation.

**Container tags:** Your client sends the tag change to the chest's network owner. The owner checks your access, saves the tag, and sends confirmation. Valheim then synchronizes the saved tag with other clients. The owner must run this version of HexAutoStorage to handle the request; otherwise, the edit times out without confirmation.

## Installation

### r2modman / Thunderstore Mod Manager

Install **HexAutoStorage** through your preferred Thunderstore mod manager.

### Manual Installation

1. Install BepInEx for Valheim.
2. Download HexAutoStorage.
3. Copy `HexAutoStorage.dll` into:

   `BepInEx/plugins/HexAutoStorage/`

4. Start Valheim.

For a dedicated server, install BepInEx and copy the same `HexAutoStorage.dll` into the server's `BepInEx/plugins/HexAutoStorage/` folder, then restart the server. Each client and the server has its own configuration file.

## Source Code

Source code is available on GitHub:

https://github.com/guillenjgg/valheim-hex-autostorage

## Issues / Suggestions

For bug reports, suggestions, or feedback, join the Discord:

https://discord.gg/wU2FXD94v4
