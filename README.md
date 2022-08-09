# XPSystem
 A basic, customisable XP and LVL system for SCP: SL.
## Config
```
## Emporium Config:

``` x_p_system:
# Enable plugin?
  is_enabled: true
  # Show debug messages?
  show_debug: false
  # Hint shown to the players if they have DNT enabled.
  d_n_t_hint: We can't track your stats while you have DNT enabled in your game options!
  # Badge for players with DNT enabled.
  d_n_t_badge:
    name: (DNT) anonymous man????
    color: nickel
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
      NtfSpecialist: 250
    ChaosConscript:
      Scientist: 50
      FacilityGuard: 150
      NtfPrivate: 200
      NtfSergeant: 250
      NtfCaptain: 300
      Scp049: 250
      Scp0492: 100
      Scp106: 250
      Scp173: 250
      Scp096: 250
      Scp93953: 250
      Scp93989: 250
      NtfSpecialist: 250
    ChaosRifleman:
      Scientist: 50
      FacilityGuard: 150
      NtfPrivate: 200
      NtfSergeant: 250
      NtfCaptain: 300
      Scp049: 250
      Scp0492: 100
      Scp106: 250
      Scp173: 250
      Scp096: 250
      Scp93953: 250
      Scp93989: 250
      NtfSpecialist: 250
    ChaosRepressor:
      Scientist: 50
      FacilityGuard: 150
      NtfPrivate: 200
      NtfSergeant: 250
      NtfCaptain: 300
      Scp049: 250
      Scp0492: 100
      Scp106: 250
      Scp173: 250
      Scp096: 250
      Scp93953: 250
      Scp93989: 250
      NtfSpecialist: 250
    ChaosMarauder:
      Scientist: 50
      FacilityGuard: 150
      NtfPrivate: 200
      NtfSergeant: 250
      NtfCaptain: 300
      Scp049: 250
      Scp0492: 100
      Scp106: 250
      Scp173: 250
      Scp096: 250
      Scp93953: 250
      Scp93989: 250
      NtfSpecialist: 250
    Scientist:
      ClassD: 100
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
    FacilityGuard:
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
    NtfPrivate:
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
    NtfSergeant:
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
    NtfCaptain:
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
      NtfSpecialist: 250
    NtfSpecialist:
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
    Scp049:
      ClassD: 50
      ChaosConscript: 200
      ChaosRifleman: 200
      ChaosRepressor: 250
      ChaosMarauder: 300
      Scientist: 200
      FacilityGuard: 150
      NtfPrivate: 200
      NtfSergeant: 250
      NtfCaptain: 300
      NtfSpecialist: 250
    Scp0492:
      ClassD: 300
      ChaosConscript: 300
      ChaosRifleman: 300
      ChaosRepressor: 300
      ChaosMarauder: 300
      Scientist: 300
      FacilityGuard: 300
      NtfPrivate: 300
      NtfSergeant: 300
      NtfCaptain: 300
      NtfSpecialist: 250
    Scp106:
      ClassD: 50
      ChaosConscript: 200
      ChaosRifleman: 200
      ChaosRepressor: 250
      ChaosMarauder: 300
      Scientist: 200
      FacilityGuard: 150
      NtfPrivate: 200
      NtfSergeant: 250
      NtfCaptain: 300
      NtfSpecialist: 250
    Scp173:
      ClassD: 50
      ChaosConscript: 200
      ChaosRifleman: 200
      ChaosRepressor: 250
      ChaosMarauder: 300
      Scientist: 200
      FacilityGuard: 150
      NtfPrivate: 200
      NtfSergeant: 250
      NtfCaptain: 300
      NtfSpecialist: 250
    Scp096:
      ClassD: 50
      ChaosConscript: 200
      ChaosRifleman: 200
      ChaosRepressor: 250
      ChaosMarauder: 300
      Scientist: 200
      FacilityGuard: 150
      NtfPrivate: 200
      NtfSergeant: 250
      NtfCaptain: 300
      NtfSpecialist: 250
    Scp93953:
      ClassD: 50
      ChaosConscript: 200
      ChaosRifleman: 200
      ChaosRepressor: 250
      ChaosMarauder: 300
      Scientist: 200
      FacilityGuard: 150
      NtfPrivate: 200
      NtfSergeant: 250
      NtfCaptain: 300
      NtfSpecialist: 250
    Scp93989:
      ClassD: 50
      ChaosConscript: 200
      ChaosRifleman: 200
      ChaosRepressor: 250
      ChaosMarauder: 300
      Scientist: 200
      FacilityGuard: 150
      NtfPrivate: 200
      NtfSergeant: 250
      NtfCaptain: 300
      NtfSpecialist: 250
  # How many XP should a player get if their team wins.
  team_win_x_p: 250
  # How many XP is required to advance a level.
  x_p_per_level: 5000
  # Show a mini-hint if a player gets XP
  show_added_x_p: true
  # Show a hint to the player if he advances a level.
  show_added_l_v_l: true
  # What hint to show if player advances a level. (if ShowAddedLVL = false, this is irrelevant)
  added_l_v_l_hint: Level Up! You are now level %level%
  # (You may add your own entries) How many XP a player gets for escaping
  escape_x_p:
    ClassD: 500
    Scientist: 300
  # (You may add your own entries) Level threshold and a badge. %color%. if you get a TAG FAIL in your console, either change your color, or remove special characters like brackets.
  levels_badge:
    0:
      name: Visitor
      color: cyan
    1:
      name: Junior
      color: orange
    5:
      name: Senior
      color: yellow
    10:
      name: Veteran
      color: red
    50:
      name: Nerd
      color: lime
  # The structure of the player nick. Variables: %lvl% - the level. %name% - the players nickname/name
  nick_structure: LVL %lvl% | %name%
  # The structure of the badge displayed in-game. Variables: %lvl% - the level. %badge% earned badge in specified in LevelsBadge. %oldbadge% - base-game badge, like ones specified in config-remoteadmin, or a global badge. can be null.
  badge_structure: '%oldbadge%'
  # Path files get saved to. Requires change on linux.
  save_path: /home/container/.config/EXILED/Configs/Players.db
  # Override colors for people who already have a rank
  override_color: false
  # Displayer location of hints.
  hint_location: Top
  # Size of hints.
  hint_size: 100
  # Spacing of the in (horizontal offset)
  hint_space: -25
  # Vertical offset of hints.
  v_offest: 7
  # Duration of hints.
  hint_duration: 3
  # Should hints override the AdvancedHints quene?
  override_quene: true ```

## Commands
```
XPSystem (or xps) - parent command, always use before others. 
leaderboards (or lb) <optional, amount of players> - displays top amount of players by level. defaults to 10. Permission: xps.get
set <UserId or in-game id> <level> - sets the level for chosen player. Permission: xps.set
get <UserId or in-game id> - gets the level and XP of a certain player. Permission: xps.get
```
