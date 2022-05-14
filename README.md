# XPSystem
 A basic, customisable XP and LVL system for SCP: SL.
## Config
```
x_p_system:
  is_enabled: true
  # Hint shown to the players if they have DNT enabled.
  d_n_t_hint: We can't track your stats while you have DNT enabled in your game options!
  # (You may add your own entries) Role1: Role2: XP player with Role1 gets for killing a person with Role2 
  kill_x_p:
    ClassD:
      Scientist: 200
      FacilityGuard: 150
      NtfPrivate: 200
      NtfSergeant: 250
      NtfCaptain: 300
      Scp049: 500
      Scp0492: 100
      Scp106: 500
      Scp173: 500
      Scp096: 500
      Scp93953: 500
      Scp93989: 500
    Scientist:
      ClassD: 50
      ChaosConscript: 200
      ChaosRifleman: 200
      ChaosRepressor: 250
      ChaosMarauder: 300
      Scp049: 500
      Scp0492: 100
      Scp106: 500
      Scp173: 500
      Scp096: 500
      Scp93953: 500
      Scp93989: 500
  # How many XP should a player get if their team wins.
  team_win_x_p: 250
  # How many XP is required to advance a level.
  x_p_per_level: 1000
  # Show a mini-hint if a player gets XP
  show_added_x_p: true
  # Show a hint to the player if he advances a level.
  show_added_l_v_l: true
  # What hint to show if player advances a level. (if ShowAddedLVL = false, this is irrelevant)
  added_l_v_l_hint: 'NEW LEVEL: <color=red>%level%</color>'
  # (You may add your own entries) How many XP a player gets for escaping
  escape_x_p:
    ClassD: 500
    Scientist: 300
  # (You may add your own entries) Level threshold and a badge. %color%. if you get a TAG FAIL in your console, either change your color, or remove special characters like brackets.
  levels_badge:
    0: Visitor %cyan%
    1: Junior %orange%
    5: Senior %yellow%
    10: Veteran %red%
    50: Nerd %purple%
  # The structure of the badge displayed in-game. Variables: %lvl% - the level. %badge% earned badge in specified in LevelsBadge. %oldbadge% - base-game badge, like ones specified in config-remoteadmin, or a global badge. can be null.
  badge_structure: (LVL %lvl% | %badge%) %oldbadge%
  # Path files get saved to. Requires change on linux.
  save_path: (Exiled folder on windows.)
```
## Commands
```
XPSystem (or xps) - parent command, always use before others. 
leaderboards (or lb) <optional, amount of players> - displays top amount of players by level. defaults to 10. Permission: xps.get
set <UserId or in-game id> <level> - sets the level for chosen player. Permission: xps.set
get <UserId or in-game id> - gets the level and XP of a certain player. Permission: xps.get
```
