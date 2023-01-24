# plan-b-terraform-mods
Unity/BepInEx mods for the game **Plan B Terraform** [@ Steam](https://store.steampowered.com/app/1894430/Plan_B_Terraform/)

### Supported version: Demo

Save file location: `%USERPROFILE%\AppData\LocalLow\Gaddy Games\Plan B Terraform\{yoursteamid}\Saves`

where `{yoursteamid}` is a bunch of numbers representing your Steam Account ID.

### Installation

1. Download the 64-bit BepInEx 5.4.21+ from https://github.com/BepInEx/BepInEx/releases
    - Make sure you **don't download** the latest, which is the 6.x.y line.
    - Make sure you download the correct version for your operating system.
2. Unpack the BepInEx zip into the game's folder
    - Default `c:\Program Files (x86)\Steam\steamapps\common\Plan B Terraform\`
3. Run the game. Quit the game
    - You should now see the `BepInEx\plugins` directory in the game's directory
4. Unpack the mod zip into the `BepInEx\plugins` directory.
    - I highly recommend keeping the directory structure of the zip intact, so, for example, it will look like `BepInEx\plugins\(Cheat) Unlock Demo\UnlockDemo.dll`
    - It makes updating or removing mods much easier.
5. If the mods don't appear to work, check the `BepInEx\OutputLog.log` for errors.
    - Also check the game's own log in `%USERPROFILE%\AppData\LocalLow\Gaddy Games\Plan B Terraform\Player.log`

### Current Mods

- [Demo Unlocker](#demounlocker) - removes the demo restriction of the game.


## Mod details

### Demo Unlocker

The Steam Demo features an `isDemo` flag. Forcing it to false enables gameplay beyound the first few tech levels and tasks.

Apparently, the demo is the full game with all data and assets?!

#### Configuration

None.