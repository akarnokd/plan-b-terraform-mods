# plan-b-terraform-mods
Unity/BepInEx mods for the game **Plan B Terraform** [@ Steam](https://store.steampowered.com/app/1894430/Plan_B_Terraform/)

## Supported version: Demo (0.6.0-610), Full (0.6.0-615)

:information_source: Might work with the now closed Beta/Full version, but those are under constant development so mods might stop working after an update.

#### Notable file paths

**Game install directory (usually):**

`c:\Program Files (x86)\Steam\steamapps\common\Plan B Terraform\`

**Save file location:**

`%USERPROFILE%\AppData\LocalLow\Gaddy Games\Plan B Terraform\{yoursteamid}\Saves`

:information_source: where `{yoursteamid}` is a bunch of numbers representing your Steam Account ID.

**Game log location:**

`%USERPROFILE%\AppData\LocalLow\Gaddy Games\Plan B Terraform\Player.log`


## Installation

1. *[One time only]* Download the 64-bit **BepInEx 5.4.21+** from [BepInEx releases](https://github.com/BepInEx/BepInEx/releases)
    - Make sure you **don't download** the latest, which is the 6.x.y line.
    - Make sure you download the correct version for your operating system.
2. *[One time only]* Unpack the BepInEx zip into the game's folder
    - Example: `c:\Program Files (x86)\Steam\steamapps\common\Plan B Terraform\`
3. *[One time only]* Run the game. Quit the game
    - You should now see the `BepInEx\plugins` directory in the game's directory
4. Unpack the mod zip into the `BepInEx\plugins` directory.
    - I highly recommend keeping the directory structure of the zip intact, so, for example, it will look like `BepInEx\plugins\akarnokd - (Feat) Add City Names`
    - It makes updating or removing mods much easier.
5. If the mods don't appear to work, check the `BepInEx\OutputLog.log` for errors.
    - Also check the game's own log in `%USERPROFILE%\AppData\LocalLow\Gaddy Games\Plan B Terraform\Player.log`
6. Many mods have configuration files you can edit under `BepInEx\config`.
    - *[Once per mod]* For the config file to show up, run the game and quit in the main menu.
    - The config file will be under the `BepInEx\config` (for example, `BepInEx\config\akarnokd.planbterraformmods.feataddcitynames.cfg`). You can edit with any text editor.
    - If something stops working, delete the `cfg` file and the mod will create a default one the next time the game is run.

## Uninstallation

1. Locate the `BepInEx\plugins` directory (or files if you haven't kept the directory structure from the zip).
   - Example: `c:\Program Files (x86)\Steam\steamapps\common\Plan B Terraform\BepInEx\plugins`
2. Delete the plugin's directory, including all files inside it
   - Example: `BepInEx\plugins\akarnokd - (Feat) Add City Names`
3. *[Optional]* Delete the mod's configuration from the `BepInEx\config` directory
   - Example: `BepInEx\config\akarnokd.planbterraformmods.feataddcitynames.cfg`

# Current Mods

### Features

- [Add City Names](#add-city-names) - customize the selection of city names the game will use to generate the map.
- [More Cities](#more-cities) - Generate more cities for a new game.
- [More Ore Fields](#more-ore-fields) - Generate more and bigger ore fields.
- [Hungarian Translation](#hungarian-translation) - Hungarian Translation (Magyar fordítás).
- [Navigate to Points of Interest](#navigate-to-points-of-interest) - Shows a panel on the right side of the screen with cities and landmarks. Clickable/Scrollable.
- [Production Limiter](#production-limiter) - limit the production items that go into the global storage.

### Cheats

- [Build Ice Extractors Anywhere](#build-ice-extractors-anywhere) - allows building Ice Extractors anywhere on the map, not just on ice.
- [Demo Unlocker](#demo-unlocker) - removes the demo restriction of the game.
- [Edit Ore Cells](#edit-ore-cells) - Place and remove ores from the surface.
- [Endless Resources](#endless-resources) - all resource nodes being extracted will have a minimum amount and never run out.
- [Progress Speed](#progress-speed) - speed up extractors, factories, drones and vehicles.


## Mod details

### Demo Unlocker

The Steam Demo features an `isDemo` flag. Forcing it to false enables gameplay beyond the first few tech levels and tasks.

Apparently, the demo is the full game with all data and assets?!

#### Configuration

None.

### Progress Speed

Speed up extractors, factories, drones and vehicles.

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

## Adds to the vehicle's low speed.
# Setting type: Single
# Default value: 0
VehicleSpeedLowAdd = 0

## Adds to the vehicle's medium speed.
# Setting type: Single
# Default value: 0
VehicleSpeedMediumAdd = 0

## Adds to the vehicle's medium speed.
# Setting type: Single
# Default value: 0
VehicleSpeedMaxAdd = 0
```

### Production Limiter

Limit the production items that go into the global storage.

The default is 500. Currently, the following items are supported:

- roadway
- roadstop
- truck
- railway
- railstop
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
- cityDam
- forest_pine
- forest_leavesHigh
- forest_leavesMultiple
- forest_cactus
- forest_savannah
- forest_coconut
- cityIn
- cityOut

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

## Limit the production of railstop
# Setting type: Int32
# Default value: 500
railstop = 500

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

## Limit the production of cityDam
# Setting type: Int32
# Default value: 500
cityDam = 500

## Limit the production of forest_pine
# Setting type: Int32
# Default value: 500
forest_pine = 500

## Limit the production of forest_leavesHigh
# Setting type: Int32
# Default value: 500
forest_leavesHigh = 500

## Limit the production of forest_leavesMultiple
# Setting type: Int32
# Default value: 500
forest_leavesMultiple = 500

## Limit the production of forest_cactus
# Setting type: Int32
# Default value: 500
forest_cactus = 500

## Limit the production of forest_savannah
# Setting type: Int32
# Default value: 500
forest_savannah = 500

## Limit the production of forest_coconut
# Setting type: Int32
# Default value: 500
forest_coconut = 500

## Limit the production of cityIn
# Setting type: Int32
# Default value: 500
cityIn = 500

## Limit the production of cityOut
# Setting type: Int32
# Default value: 500
cityOut = 500
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

### Add City Names

Customize the selection of city names the game will use to generate the map. The names are randomly picked by the game.

In the configuration, list the city names (comma separated). The mod then can add these names to the default game list or overwrite it.

#### Configuration

`akarnokd.planbterraformmods.feataddcitynames.cfg`

```
[General]

## Is the mod enabled?
# Setting type: Boolean
# Default value: true
Enabled = true

## If true, the city names will be added to the pool. If false, only the city names will be in the pool.
# Setting type: Boolean
# Default value: true
Additive = true

## The comma separated list of city names. Whitespaces around commas are ignored
# Setting type: String
# Default value: Budapest,Vienna,Bucharest,Bratislava,Ljubljana,Prague,Zagreb,Belgrade,Warsaw,Lisbon,Rome,Brussels,Athens,Berlin
Names = Budapest,Vienna,Bucharest,Bratislava,Ljubljana,Prague,Zagreb,Belgrade,Warsaw,Lisbon,Rome,Brussels,Athens,Berlin
```

### Hungarian Translation

Adds the **Hungarian (Magyar)** language option and translated labels to the game.

Magyar fordítás. Az **Options (Beállítások)** menüben lehet kiválasztani a játék nyelvét.

#### Configuration

Not relevant for end users; contains an option to dump languages to see the diffs between versions.

### More Cities

Generate more cities for a new game.

The game currently defaults to 3 cities per planet. This mod increases the number of cities generated additively via configuration option. I.e., `CityCountAdd = 2` will generate 5 cities total.

#### Configuration

`akarnokd.planbterraformmods.featmorecities.cfg`

```
## How many more cities to generate for a new game
# Setting type: Int32
# Default value: 0
CityCountAdd = 0
```

### More Ore Fields

Adjust the ore field generation logic by changing the field frequency and size range of the ore fields via configuration.

The numbers can be adjusted globally and/or per ore type:

- `GenerationPeriodAdd` - the game generates fields proportional to the number of all hexes, thus to increase the frequency of fields, set this number to negative.
- `MinHexesAdd` - increase the minimum number of hexes per ore field.
- `MaxHexesAdd` - increase the maximum number of hexes per ore field.
- `MineralMaxAdd` - the maximum number of minerals inside each hex.

Supported ore types (see them in separate config sections):

- `sulfur`
- `iron`
- `aluminumOre`
- `fluorite`

#### Configuration

`akarnokd.planbterraformmods.featmoreorefields.cfg`

```
[General]

## Is the mod enabled?
# Setting type: Boolean
# Default value: true
Enabled = true

## Positive value decreases field frequency, negative value increases field frequency.
# Setting type: Int32
# Default value: 0
GenerationPeriodAdd = 0

## Add to the minimum size of generated fields.
# Setting type: Int32
# Default value: 0
MinHexesAdd = 0

## Add to the maximum size of generated fields.
# Setting type: Int32
# Default value: 0
MaxHexesAdd = 0

## Add to the maximum number of minerals in a cell.
# Setting type: Int32
# Default value: 0
MineralMaxAdd = 0

[Ore-aluminumOre]

## Positive value decreases field frequency, negative value increases field frequency.
# Setting type: Int32
# Default value: 0
GenerationPeriodAdd = 0

## Add to the minimum size of generated fields.
# Setting type: Int32
# Default value: 0
MinHexesAdd = 0

## Add to the maximum size of generated fields.
# Setting type: Int32
# Default value: 0
MaxHexesAdd = 0

## Add to the maximum number of minerals in a cell.
# Setting type: Int32
# Default value: 0
MineralMaxAdd = 0

[Ore-fluorite]

## Positive value decreases field frequency, negative value increases field frequency.
# Setting type: Int32
# Default value: 0
GenerationPeriodAdd = 0

## Add to the minimum size of generated fields.
# Setting type: Int32
# Default value: 0
MinHexesAdd = 0

## Add to the maximum size of generated fields.
# Setting type: Int32
# Default value: 0
MaxHexesAdd = 0

## Add to the maximum number of minerals in a cell.
# Setting type: Int32
# Default value: 0
MineralMaxAdd = 0

[Ore-iron]

## Positive value decreases field frequency, negative value increases field frequency.
# Setting type: Int32
# Default value: 0
GenerationPeriodAdd = 0

## Add to the minimum size of generated fields.
# Setting type: Int32
# Default value: 0
MinHexesAdd = 0

## Add to the maximum size of generated fields.
# Setting type: Int32
# Default value: 0
MaxHexesAdd = 0

## Add to the maximum number of minerals in a cell.
# Setting type: Int32
# Default value: 0
MineralMaxAdd = 0

[Ore-sulfur]

## Positive value decreases field frequency, negative value increases field frequency.
# Setting type: Int32
# Default value: 0
GenerationPeriodAdd = 0

## Add to the minimum size of generated fields.
# Setting type: Int32
# Default value: 0
MinHexesAdd = 0

## Add to the maximum size of generated fields.
# Setting type: Int32
# Default value: 0
MaxHexesAdd = 0

## Add to the maximum number of minerals in a cell.
# Setting type: Int32
# Default value: 0
MineralMaxAdd = 0
```

### Edit Ore Cells

Add or remove various ores by clicking on the surface.

1. Enable the edit mode via <kbd>Numpad *</kbd>. An information panel will show up at the bottom of the screen.
2. Select the ore via <kbd>Numpad +</kbd> or <kbd>Numpad -</kbd>
3. <kbd>Left click</kbd> on a cell to change it to the specified ore and add some amount to it. The amount can be configured.
4. <kbd>Right click</kbd> to remove some amount from the cell or completely remove the ore if it reaches zero.
5. Disable the edit mode via <kbd>Numpad *</kbd>.

:information_source: Note: configuring the keys will be possible in future updates to this mod.

#### Configuration

`akarnokd.planbterraformmods.cheateditorecells.cfg`

```
[General]

## Is the mod enabled
# Setting type: Boolean
# Default value: true
Enabled = true

## How much ore to add or remove from the hex.
# Setting type: Int32
# Default value: 100
AmountChange = 100
```

### Build Ice Extractors Anywhere

Allows building Ice Extractors anywhere on the map, not just on ice.

#### Configuration

`akarnokd.planbterraformmods.cheatbuildiceextractorsanywhere.cfg`

```
[General]

## Is the mod enabled
# Setting type: Boolean
# Default value: true
Enabled = true
```

### Navigate to Points of Interest

Adds a panel on the right side of the screen with cities and landmarks.

You can click on a line and the map will center on that point of interest.

You can use the <kbd>Mouse Scroll</kbd> while hovering over the panel to scroll up or down if there are more than the preset limit of lines.

Use <kbd>L</kbd> (configurable) to hide the panel.

#### Configuration

`akarnokd.planbterraformmods.featnavigatetopoi`

```
```