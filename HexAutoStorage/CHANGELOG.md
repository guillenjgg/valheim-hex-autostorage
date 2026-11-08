# Changelog

## v1.1.3

### Added
- Optional debug logging, disabled by default.

### Fixed
- Allow shared public storage regardless of who placed the station or container; skip private and group containers.
- Improved storage checks while loading a base.
- Fixed multiplayer tag saving and added access checks and confirmation.
- Remotely managed containers remain skipped to prevent unsaved deposits, even when public.
- Preserved vanilla item metadata on stored output.

## v1.1.2

### Added
- Support for the Frost Kiln
- Auto storage for deep north resources:
- Bloodgold
- LiquidFrost

### Changed
- Auto storage now only uses matching tagged containers or untagged containers already holding the produced item. Otherwise, items drop normally.
- Compiled with Valheim `v1.0.21`
- Updated BepInEx dependency `denikson-BepInExPack_Valheim-5.4.2360`

## v1.1.1

### Added
- Valheim 1.0 update

## v1.1.0

### Added

- Support for the Spinning Wheel and Linen Thread

### Changed

- Improved the storage radius visualizer to make it more noticeable and easier to see across uneven terrain

## v1.0.0

- Initial release
