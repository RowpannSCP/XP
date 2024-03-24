# XPSystem, fork of BruteForceMaestro/XPSystem
 A not so basic, customisable leveling system for SCP: SL.
Features include:
- Badges based on level
- Level in Nickname
- Lots of ways to earn xp
- Customizable Hints
- Some other stuff I cannot be bothered to write

You can always request more features by messaging me on discord: `moddedmcplayer`

# Installation
Drag and drop plugin into the plugins folder. <br>
Make sure you have harmony (0Harmony.dll) in the dependencies folder.
### NWAPI version also requires
- https://github.com/CedModV2/NWAPIPermissionSystem/releases/latest

## READ: If you have previously (before version 2.0) used the plugin and your data is gone,
run `xps migrate` in the server console to migrate the old database.

## Commands
- Client console
    - `xps get` - Get your xp
    - `xps lb` - Get the leaderboard
- Remote admin console
    - `xps get` - Get the xp of a player
    - `xps set` - Sets the xp of a player
    - `xps give` - Adds xp to a player
    - `xps lb` - Gets the leaderboard
    - `xps pause` - Pause xp gain
    - `xps refresh` - Refreshes xp displays
    - `xps clearcache` - Clears the storage providers cache
    - `xps showmessage` - Messaging and translation debug tool
- Server console
    - `xps deleteeverything` - Deletes everything
    - `xps migrate` - Migrates old database