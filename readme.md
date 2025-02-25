## 📌
> [!NOTE]
> This repository is a fork of the [project](https://github.com/jakubbartosik/dRandomSkills) created by [Jakub Bartosik (D3X)](https://github.com/jakubbartosik).

## 💡 About
#### Every round, you receive a random skill. A plugin made for CS2.
<div align="center">
  <a href="https://GitHub.com/Juzlus/jRandomSkills/releases/"><img alt="GitHub release" src="https://img.shields.io/github/release/Juzlus/jRandomSkills.svg?style=social"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
  <a href="https://GitHub.com/Juzlus/jRandomSkills/commit/"><img alt="GitHub latest commit" src="https://img.shields.io/github/last-commit/Juzlus/jRandomSkills.svg?style=social&logo=github"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
  <a href="https://GitHub.com/Juzlus/jRandomSkills/releases/"><img alt="Github all releases" src="https://img.shields.io/github/downloads/Juzlus/jRandomSkills/total.svg?style=social"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
  <a href="https://GitHub.com/Juzlus/jRandomSkills/stargazers/"><img alt="GitHub stars" src="https://img.shields.io/github/stars/Juzlus/jRandomSkills.svg?style=social"></a>
</div>

#### Choose language:
<a href="./readme-pl.md">
<img width="50px" src="https://upload.wikimedia.org/wikipedia/en/thumb/1/12/Flag_of_Poland.svg/1920px-Flag_of_Poland.svg.png">
</a>
<a href="./readme.md">
<img width="50px" src="https://upload.wikimedia.org/wikipedia/commons/thumb/a/a5/Flag_of_the_United_Kingdom_%281-2%29.svg/1920px-Flag_of_the_United_Kingdom_%281-2%29.svg.png">
</a>

## ✨ Current Skills (43)
> [!NOTE]
> If the server is hosted on **Windows** there may be problems with the skills: **Aimbot**, **Tank**, **Soldier**, **One-Shot**.

- **Freezing Decoy:** Your decoy freezes all nearby players.
- **Soldier:** You have a random damage multiplier (1.15 - 1.35)x.
- **Tank:** You have a random received damage multiplier (0.65 - 0.85)x.
- **Aimbot:** Every bullet you hit counts as a headshot.
- **Retreat:** Teleport back to spawn. Press [css_useSkill], cooldown 15s.
- **Enemy Respawn:** Teleport to the enemy spawn. Press [css_useSkill], cooldown 15s.
- **Zeus:** Zeus x27 instantly recharges.
- **Radar Hack:** You see enemies on the radar.
- **Quick-Shot:** No cooldown when shooting.
- **Planter:** You can plant the bomb anywhere, detonation time is 60s.
- **Silent** Your footsteps and jumps are inaudible to OTHER players.
- **Deadly Flash:** Anyone completely blinded by your flashbang dies (including you).
- **Time Slow:** Slows down time for everyone for 6 seconds. Press [css_useSkill], cooldown 30s.
- **God Mode:** You are invincible for 2 seconds. Press [css_useSkill], cooldown 30s.
- **Random Weapon:** You get a random weapon. Press [css_useSkill], cooldown 15s.
- **Weapon Swap:** Swap weapons with a random enemy. Press [css_useSkill], cooldown 30s.
- **Wallhack:** You can see enemies through walls.
- **Swapper:** Swap places with a random enemy. Press [css_useSkill], cooldown 30s.
- **Dwarf:** Random character size at the start of the round (0.6 - 0.95).
- **Flash:** Random movement speed at the start of the round (1.2 - 3.0).
- **Paweł Jumper:** You get an additional jump.
- **Bunnyhop:** You have automatic BunnyHop.
- **Spy:** You start the round with an enemy model.
- **One-Shot:** Instantly kills an enemy upon hit.
- **Muhammad:** Explodes upon death, killing nearby players.
- **Rich Boy:** You receive a random amount of money at the start of the round (+5000 - +14999).
- **Rambo:** You get a random amount of health at the start of the round (+50 - +500).
- **Medic:** You receive a random number of medkits at the start of the round (1 - 4).
- **Ghost:** You are completely invisible.
- **Chicken:** You receive a chicken model + 10% speed boost but -50HP.
- **Astronaut:** You receive a random gravity value at the start of the round (0.1 - 0.7)x.
- **Disarmament:** You have a random chance to make the enemy drop their weapon upon hit (20% - 40%).
- **Anti-Flash:** You are immune to flashes, and your flashes last for 7 seconds.
- **Enemy Rotation:** You have a random chance to turn an enemy 180 degrees upon hit (20% - 40%).
- **Infinite Ammo:** You have unlimited ammo for all weapons.
- **Catapult:** You have a random chance to launch an enemy into the air (20% - 40%).
- **Dracula:** You regain health based on a percentage of the damage you deal (0.3x).
- **Teleporter:** Swap places with a hit enemy.
- **Saper:** You can plant and defuse bombs faster.
- **Phoenix:** You have a random chance to respawn after death.
- **Pilot:** Fly for a limited time. Hold [USE - E] to fly.
- **Shadow:** Teleport behind a hit enemy.
- **Anty Head:** You take no headshot damage.

## </> Server Commands
> [!TIP]
> **Bind to use skills:** `bind x css_useSkill`

- **List of skills:** `!skills`
- **[*] Set skill:** `!setskill <nickname> <skill>`, `!setskill Juzlus Random Weapon`
$\quad$
- **[*] Change map:** `!map <map_name>`, `!map de_nuke`
- **[*] Change workshop map:** `!map <map_id>`, `!map 3332005394`
- **[*] Quick game start:** `!map start`

_[*] - Requires "@css/root" permission, which can be set in game/csgo/addons/counterstrikesharp/configs/admins.json_

## ⚙️ Configuration
Easily configure each skill in the "**Config.cfg**" file:
```json
{
  "SkillsInfo": [
    {
      "Name": "None",           // Name of the skill
      "Active": true,           // Whether the skill should be enabled at startup
      "Cooldown": 30.0,         // How long to wait before using the skill next time
      "ChanceFrom": 1.0,        // Minimum multiplier for the given skill
      "ChanceTo": 1.0,          // Maximum multiplier for the given skill
      "Team": 1,                // Which team can use this skill
                                // 1 - All
                                // 2 - Terrorist
                                // 3 - CounterTerrorist
      "Only1v1": false          // The skill only works when you are alone in a team
    },
    ...
  ],
  "Settings": {
    "LangCode": "en",             // Select language: en, en
    "Set_Skill": "setskill, setskill",
    "SkillsList_Menu": "superpowers, skills, skillslist, superpowers, randompowers",
    "KillerSkillInfo": true,      // Display your killer's skill information in chat
    "TeamMateSkillInfo": true,    // Display in chat information about your teammaties skills
    "SummaryAfterTheRoud": true,  // Display a summary of the last round in the chat 
```

## 💻 Installation
1. install / buy a **CS2 server**.
    - Good tutorial on how to create your own CS2 server [[Video]](https://www.youtube.com/watch?v=1ZrEn0CiMi4&ab_channel=TroubleChute), [[Website]](https://hub.tcno.co/games/cs2/dedicated_server/).
2. Install **Metamod**.
    - Download [Metamod:Source 2.x](https://www.sourcemm.net/downloads.php/?branch=master)
    - Extract it to the `C2Server/game/csgo/` folder.
    - Edit the `gameinfo.gi` file by adding a new line
        ```json
            Game_LowViolence csgo_lv // Perfect World content override
            Game csgo/addons/metamod // <-- Line to add

            Game csgo
        ```
3. Install **CounterStrikeSharp**.
    - Download [CounterStrikeSharp-With-Runtime](https://github.com/roflmuffin/CounterStrikeSharp/releases).
    - Extract it to the `C2Server/game/csgo/` folder.
4. install **jRandomSkills**.
    - Download [jRandomSkills](https://github.com/Juzlus/jRandomSkills/releases)
    - Extract it to the `C2Server/game/csgo/addons/counterstrikesharp/plugins/` folder.

## 🖼️ Images
![Preview](./preview.gif)

## 📋 Changelog

### v1.0.3 (by Juzlus)
- #### General:
    - ###### English language added.
    - ###### Added welcome message.
    - ###### Added summary of last round.
    - ###### Added preview of spectated player's skills.
    - ###### Blocked the possibility of having the same skill twice in a row.
    - ###### Added simple configuration for each skill.
    - ###### Reworked all superpowers so they can be manipulated at any time (Fixed setskill command)
    - ###### Changed activation of superpowers to bind css_useSkill
- #### New skills:
    - ##### Freezing Decoy
        - ###### Your decoy freezes all nearby players.
    - ##### Soldier:
        - ###### You have a random damage multiplier (1.15 - 1.35)x.
    - ##### Tank:
        - ###### You have a random received damage multiplier (0.65 - 0.85)x.
    - ##### Aimbot:
        - ###### Each bullet you hit is counted as a head.
    - ##### Retreat:
        - ###### Return to spawn. Click [css_useSkill], cooldown 15s.
    - ##### Enemy Respawn:
        - ###### Teleport to enemy spawn. Click [css_useSkill], cooldown 15s.
    - ##### Zeus:
        - ###### Zeus x27 instant reload.
    - ##### Radar Hack:
        - ###### You see enemies on radar.
    - ##### Quick-Shot:
        - ###### No cooldown when shooting.
    - ##### Planter:
        - ###### You can plant a bomb anywhere, bomb detonation time is 60s.
    - ##### Silent:
        - ###### Your steps and jumps are unheard by OTHER players.
    - ##### Deadly Flash:
        - ###### Anyone completely blinded by your grenade dies (including you).
    - ##### Time Slow:
        - ###### Time slowdown for everyone for 6 seconds. Click [css_useSkill], cooldown 30s.
    - ##### God Mode:
        - ###### You are immortal for 2 seconds. Click [css_useSkill], cooldown 30s.
    - ##### Random Weapon:
        - ###### You are given a random weapon. Click [css_useSkill], cooldown 15s.
    - ##### Weapon Swap:
        - ###### You swap weapons with a random enemy. Click [css_useSkill], cooldown 30s.
- #### Skill improvements:
    - ##### Sapper:
        - ###### Fixed bug with instantly planting a bomb as anyone on the server had the skill ‘Sapper’.
    - ##### Ghost:
        - ###### Fixed being invisible after death.
    - ###### Minor fixes for the rest of the powers.

### v1.0.2 (by Juzlus)
- #### New skills:
    - ##### Dwarf:
        - ###### Random character size range (60% - 95%).
    - ##### Swapper:
        - ###### Swap places with a random enemy when clicking the [css_useSkill] button. Cooldown is 30s.
- #### Skill improvements:
    - ##### Anti-Flash:
        - ###### Added: Your flash takes longer (7s).
    - ##### Astronaut:
        - ###### Changed: Gravity multiplier from (0.2 - 0.7) to (0.1 - 0.7).
        - ###### Added: Player can now see their gravity multiplier in chat.
    - ##### Dracula:
        - ###### Changed: Dracula can now have excess health from now on.
    - ##### Ghost:
        - ###### Added: Blocking the use of weapons other than a knife.
        - ###### Added: Any weapon picked up by a ghost becomes invisible.
    - ##### Flash:
        - ###### Fixed not getting speed.
        - ###### Changed: Speed multiplier from (1.2 - 2.5) to (1.2 - 3.0).
        - ###### Added: Player can now see their speed multiplier in chat.
    - ##### Catapult:
        - ###### Added: Random toss chances (20% - 40%).
        - ###### Added: The player now sees their chances in chat.
    - ##### Chicken:
        - ###### Fixed not getting the chicken model.
        - ###### Added: Blocking the use of weapons other than a knife or gun.
        - ###### Added: Chicken can only have 50 hp.
    - ##### Medic:
        - ###### Fixed not getting first aid kits.
        - ###### Changed: From now on everyone loses first aid kits at the end of the round.
    - ##### Infinite Ammo:
        - ###### Added: From now on, grenades are also infinite.
    - ##### Enemy Rotation:
        - ###### Added: Random chance of enemy rotation (20% - 40%).
        - ###### Added: Player can now see their chances in chat.
    - ##### Phoenix:
        - ###### Fixed not being reborn after death.
        - ###### Added: Random revival chances (20% - 40%).
        - ###### Added: Player can now see their chances in chat.
    - ##### Pilot:
        - ###### Changed: The description of this skill has been improved.
    - ##### Rambo:
        - ###### Fixed not getting extra health.
    - ##### Disarmament:
        - ###### Added: Random chances to throw away enemy weapons after hit (20% - 40%).
        - ###### Added: Player can now see their chances in chat.
        - ###### Changed: Now only active enemy weapon is thrown (Knife throwing bug fixed).
    - ##### Eliminator -> Sapper:
        - ###### Changed: Renamed the power from ‘Eliminator’ to ‘Sapper’.
    - ##### Teleporter:
        - ###### Fixed a bug where only the player with the skill was being teleported.

## 📝 Contact

If you have a question, please write to juzlus.biznes@gmail.com or [Discord](https://discordapp.com/users/284780352042434570).
