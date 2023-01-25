# plan-b-terraform-mods
Unity/BepInEx mods for the game **Plan B Terraform** [@ Steam](https://store.steampowered.com/app/1894430/Plan_B_Terraform/)

### Supported version: Demo, Full(?)

:information_source: Might work with the now closed Beta/Full version, but those are under constant development so mods might stop working after an update.

Save file location: `%USERPROFILE%\AppData\LocalLow\Gaddy Games\Plan B Terraform\{yoursteamid}\Saves`

where `{yoursteamid}` is a bunch of numbers representing your Steam Account ID.

### Installation

1. Download the 64-bit BepInEx 5.4.21+ from [BepInEx releases](https://github.com/BepInEx/BepInEx/releases)
    - Make sure you **don't download** the latest, which is the 6.x.y line.
    - Make sure you download the correct version for your operating system.
2. Unpack the BepInEx zip into the game's folder
    - Example: `c:\Program Files (x86)\Steam\steamapps\common\Plan B Terraform\`
3. Run the game. Quit the game
    - You should now see the `BepInEx\plugins` directory in the game's directory
4. Unpack the mod zip into the `BepInEx\plugins` directory.
    - I highly recommend keeping the directory structure of the zip intact, so, for example, it will look like `BepInEx\plugins\akarnokd - (Cheat) Demo Unlocker\DemoUnlocker.dll`
    - It makes updating or removing mods much easier.
5. If the mods don't appear to work, check the `BepInEx\OutputLog.log` for errors.
    - Also check the game's own log in `%USERPROFILE%\AppData\LocalLow\Gaddy Games\Plan B Terraform\Player.log`

### Current Mods

- [Demo Unlocker](#demounlocker) - removes the demo restriction of the game.
- [Progress Speed](#progressspeed) - speed up extractors, factories and drones.
- [Production Limiter](#productionlimiter) - limit the production items that go into the global storage.
- [Endless Resources](#endlessresources) - all resource nodes being extracted will have a minimum amount and never run out.


## Mod details

### Demo Unlocker

The Steam Demo features an `isDemo` flag. Forcing it to false enables gameplay beyond the first few tech levels and tasks.

Apparently, the demo is the full game with all data and assets?!

#### Configuration

None.

### Progress Speed

Speed up extractors, factories and drones.

Extractors and factories can produce 2x, 3x, etc. (integer) rate.

Drones have increased speed and (if negative) decreased takeoff time.

#### Configuration

`akarnokd.planbterraformmods.cheatprogressspeed.cfg`

```
[General]

## Is the mod enabled?
# Setting type: Boolean
# Default value: true
Enabled = true

## The speed multiplier of Extractors.
# Setting type: Int32
# Default value: 1
ExtractorSpeed = 1

## The speed multiplier of Deep Extractors.
# Setting type: Int32
# Default value: 1
DeepExtractorSpeed = 1

## The speed multiplier of Factories (includes Assemblers, Greenhouses, Ice Extractors).
# Setting type: Int32
# Default value: 1
FactorySpeed = 1

## The speed multiplier of Cities.
# Setting type: Int32
# Default value: 1
CitySpeed = 1

## Adds to the global drone speed.
# Setting type: Single
# Default value: 0
DroneSpeedAdd = 0

## Adds to the global drone takeoff duration. Use negative to speed it up.
# Setting type: Single
# Default value: 0
DroneTakeoffDurationAdd = 0

```

### Production Limiter

Limit the production items that go into the global storage.

The default is 500. Currently, the following items are supported:

- roadway
- roadstop
- truck
- railway
- railwaystop
- train
- extractor
- iceExtractor
- pumpingStation
- depot
- depotMK2
- depotMK3
- factory
- factoryAssemblyPlant
- factoryAtmExtractor
- factoryGreenhouse
- factoryRecycle
- factoryFood
- landmark

#### Configuration

`akarnokd.planbterraformmods.featproductionlimiter`

```
[General]

## Is the mod enabled?
# Setting type: Boolean
# Default value: true
Enabled = true

## Limit the production of roadway
# Setting type: Int32
# Default value: 500
roadway = 500

## Limit the production of roadstop
# Setting type: Int32
# Default value: 500
roadstop = 500

## Limit the production of truck
# Setting type: Int32
# Default value: 500
truck = 500

## Limit the production of railway
# Setting type: Int32
# Default value: 500
railway = 500

## Limit the production of railwaystop
# Setting type: Int32
# Default value: 500
railwaystop = 500

## Limit the production of train
# Setting type: Int32
# Default value: 500
train = 500

## Limit the production of extractor
# Setting type: Int32
# Default value: 500
extractor = 500

## Limit the production of iceExtractor
# Setting type: Int32
# Default value: 500
iceExtractor = 500

## Limit the production of pumpingStation
# Setting type: Int32
# Default value: 500
pumpingStation = 500

## Limit the production of depot
# Setting type: Int32
# Default value: 500
depot = 500

## Limit the production of depotMK2
# Setting type: Int32
# Default value: 500
depotMK2 = 500

## Limit the production of depotMK3
# Setting type: Int32
# Default value: 500
depotMK3 = 500

## Limit the production of factory
# Setting type: Int32
# Default value: 500
factory = 500

## Limit the production of factoryAssemblyPlant
# Setting type: Int32
# Default value: 500
factoryAssemblyPlant = 500

## Limit the production of factoryAtmExtractor
# Setting type: Int32
# Default value: 500
factoryAtmExtractor = 500

## Limit the production of factoryGreenhouse
# Setting type: Int32
# Default value: 500
factoryGreenhouse = 500

## Limit the production of factoryRecycle
# Setting type: Int32
# Default value: 500
factoryRecycle = 500

## Limit the production of factoryFood
# Setting type: Int32
# Default value: 500
factoryFood = 500

## Limit the production of landmark
# Setting type: Int32
# Default value: 500
landmark = 500
```

### Endless Resources

All resource nodes being extracted will have a minimum amount and never run out. Default is minimum 500 units.

#### Configuration

`akarnokd.planbterraformmods.cheatendlessresources.cfg`

```
## Is the mod enabled?
# Setting type: Boolean
# Default value: true
Enabled = true

## Minimum resource amount.
# Setting type: Int32
# Default value: 500
MinResources = 500
```