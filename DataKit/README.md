# DataKit

My mod for Guns of Icarus Online. Gets and uploads game data.

## Download
Visit the [release page](https://github.com/DrPitLazarus/goi-mods/releases/tag/DataKit@0.3.0). Current version is 0.3.0.

## Features
This mod currently does not have any UI. It runs in the background and uploads data at regular intervals.

- **TerritoryStatesTask**: Uploads faction/alliance ownership of territories every 2 minutes.
- **TerritoryConflictsTask**: Uploads battle progress on territories every 2 minutes.
- **WorldMapDataTask**: Uploads 5 recent battle logs for each faction every 5 minutes.

Pages that display collected data:
- https://goi-world-map.drpitlazar.us/
- https://goi-library.drpitlazar.us/world-map-battles
- https://goi-library.drpitlazar.us/faction-leaders

Commands:
- `/datakit github` - Opens the GitHub page for this mod in your web browser.
- `/datakit update` - Opens the latest release page for this mod in your web browser.

Logs mod version and update available messages to the chat on startup.
