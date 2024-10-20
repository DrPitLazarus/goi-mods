---
title: BuffKit
---
# BuffKit
That "secret" goi mod. A collection of game modifications that feature:
- Moderation/referee tools
- Quality of Life enhancements and bug fixes
- Misc. features

Created by Trgk and Ightrril. Dr. Pit Lazarus is the current developer.

#### Install/Upgrade Instructions
[BuffKit_SCS_521.1.0.zip](https://github.com/user-attachments/files/17452455/BuffKit_SCS_521.1.0.zip)
1. Open downloaded .zip file.
2. Go to your `Guns of Icarus Online` folder.
3. Drag and drop .zip contents to your game folder, overwrite if prompted.
4. *Additional steps for Linux: Use Proton and add to your launch options: `WINEDLLOVERRIDES="version=n,b" %command%`.
  
#### Uninstall Instructions
1. Remove the same files from the .zip from your game folder

gl & hf

## Changelog

### SCS 521.1 - Minor Fix (2024-10-20)
> ShipLoadoutViewer: Fix faction icons displaying even though the setting is disabled on startup. Setting is now checked on startup.

### SCS 521 - Crew Tooltips, Faction Icons, and Force Start Button (2024-10-16)
> 20 weeks later, better than never. 🗓️
<details><summary>Expand</summary>

[BuffKit_SCS_521.0.0.zip](https://github.com/user-attachments/files/17405629/BuffKit_SCS_521.0.0.zip)
- **SimpleFixes**:
  - New **NoScrambleByDefault**: Scramble checkbox on Create Match screen is now unchecked by default.
  - New **FixClearNotifications**: The clear all notifications button now cleans up stuck notifications. Noticed certain territory notifications were not dismissible during the Gauntlet event. This does not fix the individual clear buttons, however.
  - **Scroll Sensitivity Adjustments**:
    - Chat window: `10 -> 30`.
    - Library pages except Lore: `20 -> 60`.
 - **SkirmishAlerts**: Add Spectator Only Setting, enabled by default. `skirmish alerts > spectator only`
 - **MatchRefTools**: 
   - New **ForceStartModButton**: Adds a Force Start button to the match lobby footer. There is a confirm prompt. Enabled by default. 
   Setting: `ref tools > force start mod button`
![BuffKit ForceStartModButton](https://github.com/user-attachments/assets/6553291e-d8e3-4edf-8110-313e650c1c0c)
- **ShipLoadoutViewer**:
  - New **CrewFactionDisplay**: See everyone's faction without checking everyone's profile. Disabled by default.
  This calls `GetUserProfile` once for every player like you would normally. Factions are cached for the game session and do not get updated.
  Setting: `loadout viewer > crew loadout faction display`. `crew loadout viewer` must also be enabled.
  - New **CrewToolTooltips**: Like the gun tooltips, you can now hover/click crew tools to display tooltips for tools! That's a lot of tools!
  Setting: `loadout viewer > lobby crew tool tooltip display`. Options: `disabled`, `hover`, `click` (default).  
![BuffKit_CrewFactionDisplay_and_ToolTooltips](https://github.com/user-attachments/assets/8621d510-e741-4428-b989-55cc1ffd9cfa)
</details>

### SCS 501 - Poorly Applied Bandage (2024-06-01)
> Rebuild wasted. Hope this 30s failsafe kit holds us together for the next 3 minutes... 🩹
<details><summary>Expand</summary>
  
[BuffKit_SCS_501.0.0.zip](https://github.com/user-attachments/files/16571241/BuffKit_SCS_501.0.0.zip)
- **Speedometer**: 
  - Add Jester's Parade to allowed maps.
  - Fix possible NullReferenceException on mission start. (Broke repair UI. Thanks Zetnus!)
- **ToggleMatchUI**:
  - Fix name tags not re-appearing after toggling.
  - Fix ship health bar sometimes breaking if the UI is disabled.
- New **SimpleFixes**: 
  - Format the "Time Completed" stat from seconds to `m:ss` on the UIMatchEndCrewPanel.
  - Increase the character limit in the kill feed: `58 -> 83` (+25).  
![AdjustKillFeedCharacterLimit](https://github.com/user-attachments/assets/0cbceeef-4968-44d1-807b-128d2cd6f89e)
  - **AudioResetButton**: Resets audio to use the current output device and may fix audio issues. Button in audio settings.  
![AudioResetButton](https://github.com/user-attachments/assets/1f3eff68-3d13-48b8-b841-663bdd37e771)
- New **RepairCluster**: Repair indicators are in a fixed position above the ship health bar UI. Disabled by default.
  - Only available in PvE.
  - Disables original offscreen logic and indicators. Original indicators on components remain.
  - Indicators in the RepairCluster are a basic re-implementation of the originals and are not 1:1.
  - Icons do not blink at all, flash between fire status, or have background/shadow icon.
  - Health bar does not gradually change color based on health or have a background bar.
  - Health bar turns orange at <= 50% and return white above that. If destroyed, icon is red and health bar is hidden. Health bar is hidden if full.
  - A small fire or bigFire icon appears on top of the main icon in the top-right corner when the part is on... you know.  
![RepairCluster](https://github.com/user-attachments/assets/d5159f13-70a7-44a8-a2cd-24cf4f432660)  
</details>

### SCS 491.1 - A Pinch of QoL (2024-03-25)
> The speedometer is pretty neat. 🚢💨
<details><summary>Expand</summary>

- **LobbyTimer**: Teams who can request overtime is now inclusive of the PreLockAnnouncementTime (30 seconds).
- **BuffKit Settings**:
  - Settings are now sorted alphabetically instead of random load order.
  - Settings will no longer scroll back to the top when a setting is expanded/collapsed.
  - Scroll sensitivity increased from `20` to `40`. This affects other BuffKit UI with scroll views, like Title Selection.
- **Small Fix**: Blue Team will no longer have an extra space on the match end screen. `Blue Team  Wins` to `Blue Team Wins`.
- New **UpdateChecker**: Lets you know if there is a new BuffKit version available in chat when you login to the game. Also prints the current version.  
![buffkitUpdateChecker](https://github.com/user-attachments/assets/e2ec6d40-be57-4cf4-8895-2c9956b01658)
- New **SkirmishAlerts**: Displays objective progress alerts in modes that currently do not use them. Enabled by default.
  - For spectators only.
  - Setting to log these alerts in chat, enabled by default.
  - Setting to use alert sounds, enabled by default.
  - Currently supports Deathmatch.
    - Last kill is not (but sometimes?) reported by the server. Logs final score on match end.  
![SkirmishAlertsDeathmatch](https://github.com/user-attachments/assets/7a35ecb8-f253-4dd1-8044-9aa5ee2f215c)
- New **Speedometer**: Finally see how fast your ship is in numbers! Disabled by default.
  - Pilot only. Available in Practice, Pirate Deathmatch, and PvE modes. Not supported for spectators. Overlay displayed above hotbar.
  - Meters: 
    - Speed: Horizontal m/s, Vertical m/s, Rotation degrees/s
    - Position: X east/west, Y altitude, Z north/south (X0, Z0 is map center).
  - Settings available per meter. First column will be centered if only 3 or less meters are shown.  
![Speedometer](https://github.com/user-attachments/assets/4dac57d7-5af6-4f2e-855c-80778d1d5fc6)
</details>

### SCS 489 - Ship Notes (2024-03-08)
> Copy & paste a bit less, I suppose. 📝
<details><summary>Expand</summary>
  
- New **ShipLoadoutNotes**: Adds a text box on the ship customization screen where you can set a note per ship loadout. Enabled by default. 
  - Announce to Crew feature: Captains can send their note to crew chat.
  - You can use `<slot1><slot2><slot3>` tags to insert player names from those slots. If no player in the slot, the tag stays.
  - A blank line between notes will send in a separate chat message. 
    - A chat message is limited to about 490 characters. 
    - If you exceed that, your message will be cut off. To prevent this, add a blank line.
  - Notes are limited to 1,000 characters (almost a whole expanded chat window). 
  - Announcing has a 15 second cooldown and sends a maximum of 4 chat messages.
  - The Announce to Crew button will appear under the text box when these are true: 
    - Player is the captain, Note is not empty, and Not on cooldown.
  - The save button will appear if there are changes made.  
![ShipLoadoutNotes](https://github.com/user-attachments/assets/8af594e3-54d2-47c9-b344-19a33fd317c5)
- New **ModMatchTimerFix**: Fixes the placement of the timer so it does not overlap the captain's order UI. Enabled by default.
  - Moves the timer down if the captain's order UI is active. If inactive, the timer is in its original position.
  - Updates the appearance to match the PvE timer UI. Settings are available to revert these changes individually. 
    - Hide leading 0 from minutes.
    - Change font color from yellow to white.
    - Change font from `Roboto Regular` to `PenumbraHalfSerifStd Reg`.  
![ModMatchTimerFix](https://github.com/user-attachments/assets/efc5ee2b-0ac6-40ae-8041-d140b6f51035)  
- New **AchievementScreenState**: Remembers the last state the screen was on and restores it the next time the screen is entered. Enabled by default.
  - Except when "Go to Achievement" button is clicked. (Profile > Rewards)
  - Remembers state for each tab (PvP, Co-op, Neutral) and restores it when switching between tabs.
</details>

### SCS 486 - Kaboom! (2024-02-14)
> I hope you like fireworks! 🎆
<details><summary>Expand</summary>

- Special Abilities can now be shown in the Crew Loadout Viewer. Enabled by default with settings available.
- SkipIntro will now automatically press the play button on the launcher. (Renamed to SkipLauncherAndIntro).
- New ForceSeasonalDecor feature: Ability to force on seasonal decor. Game restart not required. Settings added under MISC:
  - Force Seasonal Fireworks
  - Force Seasonal Spooky AI
  - Force Seasonal Christmas Trees
</details>
