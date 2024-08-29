# Unlimited Town Building

Build as many houses and specialized buildings as you want both in and out of new sirocco!

You must progress to New Sirocco to unlock the ability to build buildings, or use the debug menu to enable the building flag along with the flags for faction specific building upgrades, more info below.

## Manual Installation

1. Install BepInExPack for Outward
2. Extract the archive into the BepInEx/plugins folder
3. Run the game

## Configuration Options

By default, this mod is meant to be vanilla-like and will only change the ability to build outside New Sirocco as well as build more buildings than you would normally be able to.

However, you can also make building much easier and better by editing the config file located at `BepInEx/config/Glowstick.unlimitedtownbuilding.cfg` or clicking F5 in game if you have [Outward_Config_Manager](https://thunderstore.io/c/outward/p/Mefino/Outward_Config_Manager/) which is highly recommended or using the config editor of a mod manager like [r2modman](https://thunderstore.io/c/outward/p/ebkr/r2modman/).

These are the following options you can change:

- **Ignore Colliders**: If true, you can build anywhere, even if there are objects in the way. Default is true.
- **Infinite Resources**: If true, your building resources will be all set to 99999, when disabled your resources will turn to 0. Default is false.
- **Destroy Key**: The key to press to destroy buildings, you will not receive any resources back. Default is Delete Key.
- **Bypass Destroy Confirmation**: If true, you will not be asked to confirm the destruction of a building. Default is false.
- **Instant Construction**: If true, construction on buildings and upgrades will be instant. Default is false.

## Building flags

DO NOT ENABLE THESE FLAGS UNLESS YOU KNOW WHAT YOU ARE DOING AND ALREADY FINISHED THE GAME! THEY CAN BREAK YOUR GAME!

You can enable the building flag by using the debug menu's F4 key. Follow [these instructions](https://outward.fandom.com/wiki/Debug_Mode#How_to_Enable_Debug_Mode) to enable the debug menu or install the [DebugMode mod](https://thunderstore.io/c/outward/p/exp111/DebugMode/).

Once you have the debug menu enabled, press F4 to open the quest flags menu, click Add Quest Event in the top right, then search for the following quest flags and double click to enable.

The flags you're looking for are, again DO NOT ENABLE UNLESS YOU FINISHED THE GAME AND KNOW WHAT YOU ARE DOING:

- **CA_Basebuilding_Enabled**: This flag will allow you to start building.
- **Faction_BlueChamber**: This flag will allow you to upgrade City Hall to Krypteia Hideout.
- **Faction_HeroicKingdom**: This flag will allow you to upgrade Alchemist Shop to Levantin Laboratory.
- **Faction_Sorobore**: This flag will allow you to upgrade Enchanting Guild to Soroborean Laboratory.
- **Faction_HolyMission**: This flag will allow you to upgrade Chapel to Lotus of Light.