## [2.2.1] - 2026-04-22

### Added

- Debug data, has no effect when playing, but might give some more helpful info when crashes occur.

## [2.2.0] - 2026-04-20 -- The DLC Update

### Added

- Working traps, now in 10 flavours:
	- Eclipse: Briefly changes the time of day
	- Bird attack: 2 familliar birds appear...
	- Gravity: Temporarily increases gravity
	- 3 Traps that temporarily change the hold types:
		- Crimps
		- Slopers
		- Pitches
	- And 4 traps that will make you lose an item:
		- ropeLoss
		- coffeeLoss
		- chalkLoss
		- birdSeedLoss
- Artefact cleaning minigame
- Mermaid (like) locations
- Support for the Alps DLC:
	- 1 Alps Ticket
	- 23 new peaks
	- 3 books
	- 10 Idol items
	- 20 Idol locations (10x2)
	- 14 Flower locations
	- 5 Eagles
	- 5 goats

### Removed

- Artefacts are now no longer in the item pool, they will still count as locations.
	- This allows the item pool to contains more useful items

### Changed

- Expert Book has now been renamed to Northern Range Ticket

## [2.1.1] - 2026-04-04

### Added

- On-screen warnings when APWorld or mod are outdated

## [2.1.0] - 2026-04-04

### Added

- Tutorial for opening the progress screen.
- New completion target!
  - Peak Goal will allow you to set a specific peak as goal.
- Starting peak!
  - Only affects the game in Peak unlock mode, allows you to choose what peak to start with.

### Changed

- Changed the compatible APworld version (older versions will not be rejected)
- Extra ropes now give 2 extra ropes instead of 1.
  - Correspondingly, extra rope item count halved in the APworld 
- Layout of progress screen to prepare for futur addition of Alpine Books
- When playing with Peak Unlock mode, the progress screen will no longer show books with no unlocked peaks

### Fixed

- "Go to next peak" should no longer appear if the next peak is not unlocked.
- Bug where crampons and other items could be given to the player for completing a peak in YFYD mode.
- Issue where simply existing in the expert cabin would fill up the log with NullReferenceExceptions

## [2.0.3] - 2026-03-13

### Added

- added You Fall You Die mode back
- added very simple APWorld version checker that will warn the player if the apworld or mod are outdated and potentially incompatible

### Fixed

- fixed a bug where the game intro would play, while it shouldn't
- fixed a bug where Ice Axe info would be overlayed every time the player recieved an item

## [2.0.2] - 2026-03-05

### Added

- Save file blocking:
  - An apworld run will now remember what save slot it was on, and will not allow you to open any other slot, empty slots may be opened
- Entering the Northern Range Cabin without owning the Ice Axes will notify the player.
  
### Fixed

- Fixed a bug where the ticket to the northern range would only show up after Ice Axes were also unlocked 

## [2.0.1] - 2026-03-04

### Fixed

- Folder structure, allowing the mod to correctly load assets when installed through thunderstore or r2modman

## [2.0.0] - 2026-03-03

- initial release of 2.0 build

- Any version before 2.0 are undocumented, as 2.0 was essentially built from scratch
