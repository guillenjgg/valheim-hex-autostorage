# HexAutoStorage

Automatically stores bars, flour, and eitr produced by supported production stations in nearby containers.

> ## ⚠️ Important
> **HexAutoStorage only supports items produced by vanilla production stations. Mods that add custom production stations, such as custom windmills or smelters, are not guaranteed to be supported.**
>
> Containers must be within the configured storage radius and must have been placed by the same player who placed the production station.

## Features

* Automatically stores produced items in nearby supported containers
* Supports the following production stations:
  * Smelter
  * Charcoal Kiln
  * Blast Furnace
  * Windmill
  * Eitr Refinery
* Supports the following storage containers:
  * Wood Chest
  * Barrel
  * Reinforced Chest
  * Black Metal Chest
  * Personal Chest
  * Cart
* Configurable storage radius
* Optional storage radius visualization when hovering over a production station
* Container tags allow specific resources to be routed to specific containers
* Container hover text displays assigned Auto Storage tags
* Tagged containers take priority over normal storage
* Storage falls back to other valid containers when tagged containers are full

## Screenshots

![HexAutoStorage](https://raw.githubusercontent.com/guillenjgg/valheim-hex-mod-images/main/hexautostorage/hexautostorage_1.png)
![HexAutoStorage](https://raw.githubusercontent.com/guillenjgg/valheim-hex-mod-images/main/hexautostorage/hexautostorage_2.png)
![HexAutoStorage](https://raw.githubusercontent.com/guillenjgg/valheim-hex-mod-images/main/hexautostorage/hexautostorage_3.png)

## Auto Storage Priority

When a production station creates an item, HexAutoStorage searches nearby supported containers in the following order:

1. Matching tagged containers, ordered by distance
2. Untagged containers that already contain the produced item, ordered by distance
3. If no valid container has room, the production station uses normal Valheim behavior and drops the item

Containers with tags that do not match the produced item are not used as fallback storage.

## Container Tags

Look at a supported container and use the configured **Edit Tags Shortcut** to open the Auto Storage tag editor.

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
* Eitr

The internal prefab names do not need to be entered by the player.

### Example

A chest tagged:

`Iron,Copper`

will be preferred whenever a nearby supported production station produces Iron or Copper.

If that chest is full, HexAutoStorage will try another matching tagged container before falling back to an appropriate untagged container.

## Storage Radius

Production stations only search for containers within the configured **StorageRadius**.

The storage radius can optionally be displayed by enabling:

`ShowRadiusVisual = true`

When enabled, hover over a supported production station to display its storage radius.

## Configuration

Configuration options are available in:

`BepInEx/config/com.hex.autostorage.cfg`

Current configuration options include:

* **Enabled** — Enable or disable HexAutoStorage
* **StorageRadius** — Radius in meters used to search for nearby storage
* **ShowRadiusVisual** — Display the Auto Storage radius while hovering over a supported production station
* **EditTagsShortcut** — Keyboard shortcut used to edit container Auto Storage tags

The **EditTagsShortcut** configuration description also contains the list of supported tag names for reference.

## Multiplayer

HexAutoStorage is a client-side mod.

HexAutoStorage uses the creator of the production station when searching for nearby storage.

Only supported containers placed by the same player who placed the production station are considered valid Auto Storage destinations.

## Installation

### r2modman / Thunderstore Mod Manager

Install **HexAutoStorage** through your preferred Thunderstore mod manager.

### Manual Installation

1. Install BepInEx for Valheim.
2. Download HexAutoStorage.
3. Copy `HexAutoStorage.dll` into:

   `BepInEx/plugins/HexAutoStorage/`

4. Start Valheim.

## Source Code

Source code is available on GitHub:

https://github.com/guillenjgg/valheim-hex-autostorage

## Issues / Suggestions

For bug reports, suggestions, or feedback, join the Discord:

https://discord.gg/wU2FXD94v4