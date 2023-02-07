# plan-b-terraform-mods
Unity/BepInEx mods for the game **Plan B Terraform** [@ Steam](https://store.steampowered.com/app/1894430/Plan_B_Terraform/)

## Version <a href='https://github.com/akarnokd/plan-b-terraform-mods/releases'><img src='https://img.shields.io/github/v/release/akarnokd/plan-b-terraform-mods' alt='Latest GitHub Release Version'/></a>

[![Github All Releases](https://img.shields.io/github/downloads/akarnokd/plan-b-terraform-mods/total.svg)](https://github.com/akarnokd/plan-b-terraform-mods/releases)

## Supported version: Demo (0.6.2-630), Beta (0.6.2-632)

:information_source: Might work with the now closed Beta/Full version, but those are under constant development so mods might stop working after an update.

### Notable file paths

#### Game install directory

**(for example)**

`c:\Program Files (x86)\Steam\steamapps\common\Plan B Terraform\`

#### Save file location

`%USERPROFILE%\AppData\LocalLow\Gaddy Games\Plan B Terraform\{yoursteamid}\Saves`

:information_source: where `{yoursteamid}` is a bunch of numbers representing your Steam Account ID.

#### Game log location

`%USERPROFILE%\AppData\LocalLow\Gaddy Games\Plan B Terraform\Player.log`

#### BepInEx log location

**(depending on your game's install directory)**

`c:\Program Files (x86)\Steam\steamapps\common\Plan B Terraform\BepInEx\LogOutput.log`

#### Plugin directory

**(depending on your game's install directory)**

`c:\Program Files (x86)\Steam\steamapps\common\Plan B Terraform\BepInEx\plugins`

#### Plugin config directory

**(depending on your game's install directory)**

`c:\Program Files (x86)\Steam\steamapps\common\Plan B Terraform\BepInEx\config`

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
- [City Population Label](#city-population-label) - display the population number underneath the city label in the main view and/or minimap.
- [Disable Building](#disable-building) - Enable and disable production buildings via a on-screen button or keyboard shortcut.
- [Go to Exhausted Extractors](#go-to-exhausted-extractors) - shows a blinking panel (bottom left) if there are any extractors that have run out of minable ore.
- [Hotbar](#hotbar) - Adds a bar at the bottom of the screen with 3 subpanels and 9 slots each for quickly selecting a building to be built.
- [More Cities](#more-cities) - Generate more cities for a new game.
- [More Ore Fields](#more-ore-fields) - Generate more and bigger ore fields.
- [Hungarian Translation](#hungarian-translation) - Hungarian Translation (Magyar fordítás).
- [Live GUI Scaler](#live-gui-scaler) - Scale the GUI by holding <kbd>CTRL</kbd> and pressing <kbd>Numpad Plus</kbd>, <kbd>Numpad Minus</kbd> or via <kbd>Mouse wheel</kbd>
- [Navigate to Points of Interest](#navigate-to-points-of-interest) - Shows a panel on the right side of the screen with cities and landmarks. Clickable/Scrollable.
- [Production Limiter](#production-limiter) - limit the production items that go into the global storage.
- [Production Statistics](#production-statistics) - show the production and consumption speed of items

### Cheats

- [Build Ice Extractors Anywhere](#build-ice-extractors-anywhere) - allows building Ice Extractors anywhere on the map, not just on ice.
- [Edit Ore Cells](#edit-ore-cells) - Place and remove ores from the surface.
- [Endless Resources](#endless-resources) - all resource nodes being extracted will have a minimum amount and never run out.
- [Progress Speed](#progress-speed) - speed up extractors, factories, drones and vehicles.


# Mod details

## Progress Speed

Speed up extractors, factories, drones and vehicles.

Extractors and factories can produce 2x, 3x, etc. (integer) rate.

Drones have increased speed and (if negative) decreased takeoff time.

#### Configuration

<details><summary><code>akarnokd.planbterraformmods.cheatprogressspeed.cfg</code></summary>

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
</details>

## Production Limiter

Limit the production items that go into the global storage.

There is an ingame button to the top-left of the screen ("Limit Prod") that opens up a dialog where each limit can be adjusted ingame (Shortcut <kbd>F4</kbd> - configurable). 

You can hold <kbd>SHIFT</kbd> to change by **10x** the button's amount and  <kbd>CTRL+SHIFT</kbd> to change by **100x**. (I.e., clicking on the <kbd>+100</kbd> while holding <kbd>CTRL+SHIFT</kbd> will add **10000** to the limit.)

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

<details><summary><code>akarnokd.planbterraformmods.featproductionlimiter.cfg</code></summary>

```
[General]

## Is the mod enabled?
# Setting type: Boolean
# Default value: true
Enabled = true

## Always show all products?
# Setting type: Boolean
# Default value: false
ShowAll = false

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

## Limit the production of cityIn
# Setting type: Int32
# Default value: 500
cityIn = 500

## Limit the production of cityOut
# Setting type: Int32
# Default value: 500
cityOut = 500

## Key to toggle the limiter panel
# Setting type: KeyCode
# Default value: F4
# Acceptable values: None, Backspace, Tab, Clear, Return, Pause, Escape, Space, Exclaim, DoubleQuote, Hash, Dollar, Percent, Ampersand, Quote, LeftParen, RightParen, Asterisk, Plus, Comma, Minus, Period, Slash, Alpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9, Colon, Semicolon, Less, Equals, Greater, Question, At, LeftBracket, Backslash, RightBracket, Caret, Underscore, BackQuote, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, LeftCurlyBracket, Pipe, RightCurlyBracket, Tilde, Delete, Keypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9, KeypadPeriod, KeypadDivide, KeypadMultiply, KeypadMinus, KeypadPlus, KeypadEnter, KeypadEquals, UpArrow, DownArrow, RightArrow, LeftArrow, Insert, Home, End, PageUp, PageDown, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, Numlock, CapsLock, ScrollLock, RightShift, LeftShift, RightControl, LeftControl, RightAlt, LeftAlt, RightMeta, RightCommand, RightApple, LeftMeta, LeftCommand, LeftApple, LeftWindows, RightWindows, AltGr, Help, Print, SysReq, Break, Menu, Mouse0, Mouse1, Mouse2, Mouse3, Mouse4, Mouse5, Mouse6, JoystickButton0, JoystickButton1, JoystickButton2, JoystickButton3, JoystickButton4, JoystickButton5, JoystickButton6, JoystickButton7, JoystickButton8, JoystickButton9, JoystickButton10, JoystickButton11, JoystickButton12, JoystickButton13, JoystickButton14, JoystickButton15, JoystickButton16, JoystickButton17, JoystickButton18, JoystickButton19, Joystick1Button0, Joystick1Button1, Joystick1Button2, Joystick1Button3, Joystick1Button4, Joystick1Button5, Joystick1Button6, Joystick1Button7, Joystick1Button8, Joystick1Button9, Joystick1Button10, Joystick1Button11, Joystick1Button12, Joystick1Button13, Joystick1Button14, Joystick1Button15, Joystick1Button16, Joystick1Button17, Joystick1Button18, Joystick1Button19, Joystick2Button0, Joystick2Button1, Joystick2Button2, Joystick2Button3, Joystick2Button4, Joystick2Button5, Joystick2Button6, Joystick2Button7, Joystick2Button8, Joystick2Button9, Joystick2Button10, Joystick2Button11, Joystick2Button12, Joystick2Button13, Joystick2Button14, Joystick2Button15, Joystick2Button16, Joystick2Button17, Joystick2Button18, Joystick2Button19, Joystick3Button0, Joystick3Button1, Joystick3Button2, Joystick3Button3, Joystick3Button4, Joystick3Button5, Joystick3Button6, Joystick3Button7, Joystick3Button8, Joystick3Button9, Joystick3Button10, Joystick3Button11, Joystick3Button12, Joystick3Button13, Joystick3Button14, Joystick3Button15, Joystick3Button16, Joystick3Button17, Joystick3Button18, Joystick3Button19, Joystick4Button0, Joystick4Button1, Joystick4Button2, Joystick4Button3, Joystick4Button4, Joystick4Button5, Joystick4Button6, Joystick4Button7, Joystick4Button8, Joystick4Button9, Joystick4Button10, Joystick4Button11, Joystick4Button12, Joystick4Button13, Joystick4Button14, Joystick4Button15, Joystick4Button16, Joystick4Button17, Joystick4Button18, Joystick4Button19, Joystick5Button0, Joystick5Button1, Joystick5Button2, Joystick5Button3, Joystick5Button4, Joystick5Button5, Joystick5Button6, Joystick5Button7, Joystick5Button8, Joystick5Button9, Joystick5Button10, Joystick5Button11, Joystick5Button12, Joystick5Button13, Joystick5Button14, Joystick5Button15, Joystick5Button16, Joystick5Button17, Joystick5Button18, Joystick5Button19, Joystick6Button0, Joystick6Button1, Joystick6Button2, Joystick6Button3, Joystick6Button4, Joystick6Button5, Joystick6Button6, Joystick6Button7, Joystick6Button8, Joystick6Button9, Joystick6Button10, Joystick6Button11, Joystick6Button12, Joystick6Button13, Joystick6Button14, Joystick6Button15, Joystick6Button16, Joystick6Button17, Joystick6Button18, Joystick6Button19, Joystick7Button0, Joystick7Button1, Joystick7Button2, Joystick7Button3, Joystick7Button4, Joystick7Button5, Joystick7Button6, Joystick7Button7, Joystick7Button8, Joystick7Button9, Joystick7Button10, Joystick7Button11, Joystick7Button12, Joystick7Button13, Joystick7Button14, Joystick7Button15, Joystick7Button16, Joystick7Button17, Joystick7Button18, Joystick7Button19, Joystick8Button0, Joystick8Button1, Joystick8Button2, Joystick8Button3, Joystick8Button4, Joystick8Button5, Joystick8Button6, Joystick8Button7, Joystick8Button8, Joystick8Button9, Joystick8Button10, Joystick8Button11, Joystick8Button12, Joystick8Button13, Joystick8Button14, Joystick8Button15, Joystick8Button16, Joystick8Button17, Joystick8Button18, Joystick8Button19
ToggleKey = F4

## The font size in the panel
# Setting type: Int32
# Default value: 15
FontSize = 15

## The size of the item's icon in the list
# Setting type: Int32
# Default value: 32
ItemSize = 32

## The button's position relative to the left of the screen
# Setting type: Int32
# Default value: 175
ButtonLeft = 175

## The button's width and height
# Setting type: Int32
# Default value: 50
ButtonSize = 50

## How many lines of items to show
# Setting type: Int32
# Default value: 16
MaxLines = 16
```
</details>

## Endless Resources

All resource nodes being extracted will have a minimum amount and never run out. Default is minimum 500 units.

#### Configuration

<details><summary><code>akarnokd.planbterraformmods.cheatendlessresources.cfg</code></summary>

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
</details>

## Add City Names

Customize the selection of city names the game will use to generate the map. The names are randomly picked by the game.

In the configuration, list the city names (comma separated). The mod then can add these names to the default game list or overwrite it.

#### Configuration

<details><summary><code>akarnokd.planbterraformmods.feataddcitynames.cfg</code></summary>

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
</details>

## Hungarian Translation

Adds the **Hungarian (Magyar)** language option and translated labels to the game.

Magyar fordítás. Az **Options (Beállítások)** menüben lehet kiválasztani a játék nyelvét.

#### Configuration

Not relevant for end users; contains an option to dump languages to see the diffs between versions.

## More Cities

Generate more cities for a new game.

The game currently defaults to 3 cities per planet. This mod adds a slider to the **New Planet** screen where you can set the number of additional cities to be generated. For example, if set to *2*, the game will have 5 cities total.

#### Configuration

<details><summary><code>akarnokd.planbterraformmods.featmorecities.cfg</code></summary>

```
## How many more cities to generate for a new game
# Setting type: Int32
# Default value: 0
CityCountAdd = 0
```
</details>

## More Ore Fields

Adjust the ore field generation logic by changing the field frequency and size range of the ore fields in the **New Planet** dialog (or via configuration file).

The numbers can be adjusted globally and/or per ore type (this latter only via config file for now):

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

<details><summary><code>akarnokd.planbterraformmods.featmoreorefields.cfg</code></summary>

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
</details>

## Edit Ore Cells

Add or remove various ores by clicking on the surface.

1. Enable the edit mode via <kbd>Numpad *</kbd>. An information panel will show up at the bottom of the screen.
2. Select the ore via <kbd>Numpad +</kbd> or <kbd>Numpad -</kbd>
3. <kbd>Left click</kbd> on a cell to change it to the specified ore and add some amount to it. The amount can be configured.
4. <kbd>Right click</kbd> to remove some amount from the cell or completely remove the ore if it reaches zero.
5. Disable the edit mode via <kbd>Numpad *</kbd>.

:information_source: Note: configuring the keys will be possible in future updates to this mod.

#### Configuration

<details><summary><code>akarnokd.planbterraformmods.cheateditorecells.cfg</code></summary>

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
</details>

## Build Ice Extractors Anywhere

Allows building Ice Extractors anywhere on the map, not just on ice.

#### Configuration

<details><summary><code>akarnokd.planbterraformmods.cheatbuildiceextractorsanywhere.cfg</code></summary>

```
[General]

## Is the mod enabled
# Setting type: Boolean
# Default value: true
Enabled = true
```
</details>

## Navigate to Points of Interest

Adds a panel on the right side of the screen with cities and landmarks.

You can click on a line and the map will center on that point of interest.

You can use the <kbd>Mouse Scroll</kbd> while hovering over the panel to scroll up or down if there are more than the preset limit of lines.

Use <kbd>L</kbd> (configurable) to hide the panel.

:information_source: the panel does not adapt to screen sizes, use the configuration to position the panel and adjust the font size.

#### Configuration

<details><summary><code>akarnokd.planbterraformmods.featnavigatetopoi</code></summary>

```
[General]

## Is the mod enabled?
# Setting type: Boolean
# Default value: true
Enabled = true

## The font size of the panel text
# Setting type: Int32
# Default value: 20
FontSize = 20

## The maximum number of points of interest to show at once (scrollable)
# Setting type: Int32
# Default value: 10
MaxLines = 10

## The top position of the panel relative to the top of the screen
# Setting type: Int32
# Default value: 300
PanelTop = 300

## The key to show/hide the panel
# Setting type: KeyCode
# Default value: L
# Acceptable values: None, Backspace, Tab, Clear, Return, Pause, Escape, Space, Exclaim, DoubleQuote, Hash, Dollar, Percent, Ampersand, Quote, LeftParen, RightParen, Asterisk, Plus, Comma, Minus, Period, Slash, Alpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9, Colon, Semicolon, Less, Equals, Greater, Question, At, LeftBracket, Backslash, RightBracket, Caret, Underscore, BackQuote, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, LeftCurlyBracket, Pipe, RightCurlyBracket, Tilde, Delete, Keypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9, KeypadPeriod, KeypadDivide, KeypadMultiply, KeypadMinus, KeypadPlus, KeypadEnter, KeypadEquals, UpArrow, DownArrow, RightArrow, LeftArrow, Insert, Home, End, PageUp, PageDown, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, Numlock, CapsLock, ScrollLock, RightShift, LeftShift, RightControl, LeftControl, RightAlt, LeftAlt, RightMeta, RightCommand, RightApple, LeftMeta, LeftCommand, LeftApple, LeftWindows, RightWindows, AltGr, Help, Print, SysReq, Break, Menu, Mouse0, Mouse1, Mouse2, Mouse3, Mouse4, Mouse5, Mouse6, JoystickButton0, JoystickButton1, JoystickButton2, JoystickButton3, JoystickButton4, JoystickButton5, JoystickButton6, JoystickButton7, JoystickButton8, JoystickButton9, JoystickButton10, JoystickButton11, JoystickButton12, JoystickButton13, JoystickButton14, JoystickButton15, JoystickButton16, JoystickButton17, JoystickButton18, JoystickButton19, Joystick1Button0, Joystick1Button1, Joystick1Button2, Joystick1Button3, Joystick1Button4, Joystick1Button5, Joystick1Button6, Joystick1Button7, Joystick1Button8, Joystick1Button9, Joystick1Button10, Joystick1Button11, Joystick1Button12, Joystick1Button13, Joystick1Button14, Joystick1Button15, Joystick1Button16, Joystick1Button17, Joystick1Button18, Joystick1Button19, Joystick2Button0, Joystick2Button1, Joystick2Button2, Joystick2Button3, Joystick2Button4, Joystick2Button5, Joystick2Button6, Joystick2Button7, Joystick2Button8, Joystick2Button9, Joystick2Button10, Joystick2Button11, Joystick2Button12, Joystick2Button13, Joystick2Button14, Joystick2Button15, Joystick2Button16, Joystick2Button17, Joystick2Button18, Joystick2Button19, Joystick3Button0, Joystick3Button1, Joystick3Button2, Joystick3Button3, Joystick3Button4, Joystick3Button5, Joystick3Button6, Joystick3Button7, Joystick3Button8, Joystick3Button9, Joystick3Button10, Joystick3Button11, Joystick3Button12, Joystick3Button13, Joystick3Button14, Joystick3Button15, Joystick3Button16, Joystick3Button17, Joystick3Button18, Joystick3Button19, Joystick4Button0, Joystick4Button1, Joystick4Button2, Joystick4Button3, Joystick4Button4, Joystick4Button5, Joystick4Button6, Joystick4Button7, Joystick4Button8, Joystick4Button9, Joystick4Button10, Joystick4Button11, Joystick4Button12, Joystick4Button13, Joystick4Button14, Joystick4Button15, Joystick4Button16, Joystick4Button17, Joystick4Button18, Joystick4Button19, Joystick5Button0, Joystick5Button1, Joystick5Button2, Joystick5Button3, Joystick5Button4, Joystick5Button5, Joystick5Button6, Joystick5Button7, Joystick5Button8, Joystick5Button9, Joystick5Button10, Joystick5Button11, Joystick5Button12, Joystick5Button13, Joystick5Button14, Joystick5Button15, Joystick5Button16, Joystick5Button17, Joystick5Button18, Joystick5Button19, Joystick6Button0, Joystick6Button1, Joystick6Button2, Joystick6Button3, Joystick6Button4, Joystick6Button5, Joystick6Button6, Joystick6Button7, Joystick6Button8, Joystick6Button9, Joystick6Button10, Joystick6Button11, Joystick6Button12, Joystick6Button13, Joystick6Button14, Joystick6Button15, Joystick6Button16, Joystick6Button17, Joystick6Button18, Joystick6Button19, Joystick7Button0, Joystick7Button1, Joystick7Button2, Joystick7Button3, Joystick7Button4, Joystick7Button5, Joystick7Button6, Joystick7Button7, Joystick7Button8, Joystick7Button9, Joystick7Button10, Joystick7Button11, Joystick7Button12, Joystick7Button13, Joystick7Button14, Joystick7Button15, Joystick7Button16, Joystick7Button17, Joystick7Button18, Joystick7Button19, Joystick8Button0, Joystick8Button1, Joystick8Button2, Joystick8Button3, Joystick8Button4, Joystick8Button5, Joystick8Button6, Joystick8Button7, Joystick8Button8, Joystick8Button9, Joystick8Button10, Joystick8Button11, Joystick8Button12, Joystick8Button13, Joystick8Button14, Joystick8Button15, Joystick8Button16, Joystick8Button17, Joystick8Button18, Joystick8Button19
TogglePanelKey = L
```
</details>

## Go to exhausted Extractors

Shows a blinking panel (bottom left) if there are any extractors that have run out of minable ore.

Click on the panel, or press <kbd>.</kbd> (configurable) to go one of those Extractors.

:information_source: the panel does not adapt to screen sizes, use the configuration to position the panel and adjust the font size.

#### Configuration

<details><summary><code>akarnokd.planbterraformmods.featgotoexhaustedextractors.cfg</code></summary>

```
[General]

## Is the mod enabled?
# Setting type: Boolean
# Default value: true
Enabled = true

## The panel size
# Setting type: Int32
# Default value: 75
PanelSize = 75

## The panel position from the bottom of the screen
# Setting type: Int32
# Default value: 35
PanelBottom = 35

## The panel position from the left of the screen
# Setting type: Int32
# Default value: 50
PanelLeft = 50

## The font size
# Setting type: Int32
# Default value: 15
FontSize = 15

## The shortcut key for locating the idle extractor
# Setting type: KeyCode
# Default value: Period
# Acceptable values: None, Backspace, Tab, Clear, Return, Pause, Escape, Space, Exclaim, DoubleQuote, Hash, Dollar, Percent, Ampersand, Quote, LeftParen, RightParen, Asterisk, Plus, Comma, Minus, Period, Slash, Alpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9, Colon, Semicolon, Less, Equals, Greater, Question, At, LeftBracket, Backslash, RightBracket, Caret, Underscore, BackQuote, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, LeftCurlyBracket, Pipe, RightCurlyBracket, Tilde, Delete, Keypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9, KeypadPeriod, KeypadDivide, KeypadMultiply, KeypadMinus, KeypadPlus, KeypadEnter, KeypadEquals, UpArrow, DownArrow, RightArrow, LeftArrow, Insert, Home, End, PageUp, PageDown, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, Numlock, CapsLock, ScrollLock, RightShift, LeftShift, RightControl, LeftControl, RightAlt, LeftAlt, RightMeta, RightCommand, RightApple, LeftMeta, LeftCommand, LeftApple, LeftWindows, RightWindows, AltGr, Help, Print, SysReq, Break, Menu, Mouse0, Mouse1, Mouse2, Mouse3, Mouse4, Mouse5, Mouse6, JoystickButton0, JoystickButton1, JoystickButton2, JoystickButton3, JoystickButton4, JoystickButton5, JoystickButton6, JoystickButton7, JoystickButton8, JoystickButton9, JoystickButton10, JoystickButton11, JoystickButton12, JoystickButton13, JoystickButton14, JoystickButton15, JoystickButton16, JoystickButton17, JoystickButton18, JoystickButton19, Joystick1Button0, Joystick1Button1, Joystick1Button2, Joystick1Button3, Joystick1Button4, Joystick1Button5, Joystick1Button6, Joystick1Button7, Joystick1Button8, Joystick1Button9, Joystick1Button10, Joystick1Button11, Joystick1Button12, Joystick1Button13, Joystick1Button14, Joystick1Button15, Joystick1Button16, Joystick1Button17, Joystick1Button18, Joystick1Button19, Joystick2Button0, Joystick2Button1, Joystick2Button2, Joystick2Button3, Joystick2Button4, Joystick2Button5, Joystick2Button6, Joystick2Button7, Joystick2Button8, Joystick2Button9, Joystick2Button10, Joystick2Button11, Joystick2Button12, Joystick2Button13, Joystick2Button14, Joystick2Button15, Joystick2Button16, Joystick2Button17, Joystick2Button18, Joystick2Button19, Joystick3Button0, Joystick3Button1, Joystick3Button2, Joystick3Button3, Joystick3Button4, Joystick3Button5, Joystick3Button6, Joystick3Button7, Joystick3Button8, Joystick3Button9, Joystick3Button10, Joystick3Button11, Joystick3Button12, Joystick3Button13, Joystick3Button14, Joystick3Button15, Joystick3Button16, Joystick3Button17, Joystick3Button18, Joystick3Button19, Joystick4Button0, Joystick4Button1, Joystick4Button2, Joystick4Button3, Joystick4Button4, Joystick4Button5, Joystick4Button6, Joystick4Button7, Joystick4Button8, Joystick4Button9, Joystick4Button10, Joystick4Button11, Joystick4Button12, Joystick4Button13, Joystick4Button14, Joystick4Button15, Joystick4Button16, Joystick4Button17, Joystick4Button18, Joystick4Button19, Joystick5Button0, Joystick5Button1, Joystick5Button2, Joystick5Button3, Joystick5Button4, Joystick5Button5, Joystick5Button6, Joystick5Button7, Joystick5Button8, Joystick5Button9, Joystick5Button10, Joystick5Button11, Joystick5Button12, Joystick5Button13, Joystick5Button14, Joystick5Button15, Joystick5Button16, Joystick5Button17, Joystick5Button18, Joystick5Button19, Joystick6Button0, Joystick6Button1, Joystick6Button2, Joystick6Button3, Joystick6Button4, Joystick6Button5, Joystick6Button6, Joystick6Button7, Joystick6Button8, Joystick6Button9, Joystick6Button10, Joystick6Button11, Joystick6Button12, Joystick6Button13, Joystick6Button14, Joystick6Button15, Joystick6Button16, Joystick6Button17, Joystick6Button18, Joystick6Button19, Joystick7Button0, Joystick7Button1, Joystick7Button2, Joystick7Button3, Joystick7Button4, Joystick7Button5, Joystick7Button6, Joystick7Button7, Joystick7Button8, Joystick7Button9, Joystick7Button10, Joystick7Button11, Joystick7Button12, Joystick7Button13, Joystick7Button14, Joystick7Button15, Joystick7Button16, Joystick7Button17, Joystick7Button18, Joystick7Button19, Joystick8Button0, Joystick8Button1, Joystick8Button2, Joystick8Button3, Joystick8Button4, Joystick8Button5, Joystick8Button6, Joystick8Button7, Joystick8Button8, Joystick8Button9, Joystick8Button10, Joystick8Button11, Joystick8Button12, Joystick8Button13, Joystick8Button14, Joystick8Button15, Joystick8Button16, Joystick8Button17, Joystick8Button18, Joystick8Button19
Key = Period
```
</details>

## Disable building

Enable and disable production buildings via a on-screen button or keyboard shortcut.

Toggle enable/disable with <kbd>K</kbd> (configurable) when a building is selected or use the **factory icon button** (default bottom-left of the screen). Disabled buildings will show a red crossed out circle (:no_entry_sign:) over the buildings on the main screen.

The building state is persisted in your save.

:information_source: The save format is not changed by the mod and remains compatible with the vanilla save, meaning that removing the mod won't break it.

:information_source: Note that the location and size of the **factory icon button** is not adapting to screen resolution by itself so please use the configuration settings to adjust its position and size.

Supported buildings:

- Extractors (plain *Extractor*, *Ice Extractor*)
- Factories (plain *Factory*, *Assembly Plant*, *Greenhouse*)
- Other (*Pumping Station*)

#### Configuration

<details><summary><code>akarnokd.planbterraformmods.featdisablebuilding.cfg</code></summary>

```
[General]

## Is the mod enabled?
# Setting type: Boolean
# Default value: true
Enabled = true

## Key to press while the building is selected to toggle its enabled/disabled state
# Setting type: KeyCode
# Default value: K
# Acceptable values: None, Backspace, Tab, Clear, Return, Pause, Escape, Space, Exclaim, DoubleQuote, Hash, Dollar, Percent, Ampersand, Quote, LeftParen, RightParen, Asterisk, Plus, Comma, Minus, Period, Slash, Alpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9, Colon, Semicolon, Less, Equals, Greater, Question, At, LeftBracket, Backslash, RightBracket, Caret, Underscore, BackQuote, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, LeftCurlyBracket, Pipe, RightCurlyBracket, Tilde, Delete, Keypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9, KeypadPeriod, KeypadDivide, KeypadMultiply, KeypadMinus, KeypadPlus, KeypadEnter, KeypadEquals, UpArrow, DownArrow, RightArrow, LeftArrow, Insert, Home, End, PageUp, PageDown, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, Numlock, CapsLock, ScrollLock, RightShift, LeftShift, RightControl, LeftControl, RightAlt, LeftAlt, RightMeta, RightCommand, RightApple, LeftMeta, LeftCommand, LeftApple, LeftWindows, RightWindows, AltGr, Help, Print, SysReq, Break, Menu, Mouse0, Mouse1, Mouse2, Mouse3, Mouse4, Mouse5, Mouse6, JoystickButton0, JoystickButton1, JoystickButton2, JoystickButton3, JoystickButton4, JoystickButton5, JoystickButton6, JoystickButton7, JoystickButton8, JoystickButton9, JoystickButton10, JoystickButton11, JoystickButton12, JoystickButton13, JoystickButton14, JoystickButton15, JoystickButton16, JoystickButton17, JoystickButton18, JoystickButton19, Joystick1Button0, Joystick1Button1, Joystick1Button2, Joystick1Button3, Joystick1Button4, Joystick1Button5, Joystick1Button6, Joystick1Button7, Joystick1Button8, Joystick1Button9, Joystick1Button10, Joystick1Button11, Joystick1Button12, Joystick1Button13, Joystick1Button14, Joystick1Button15, Joystick1Button16, Joystick1Button17, Joystick1Button18, Joystick1Button19, Joystick2Button0, Joystick2Button1, Joystick2Button2, Joystick2Button3, Joystick2Button4, Joystick2Button5, Joystick2Button6, Joystick2Button7, Joystick2Button8, Joystick2Button9, Joystick2Button10, Joystick2Button11, Joystick2Button12, Joystick2Button13, Joystick2Button14, Joystick2Button15, Joystick2Button16, Joystick2Button17, Joystick2Button18, Joystick2Button19, Joystick3Button0, Joystick3Button1, Joystick3Button2, Joystick3Button3, Joystick3Button4, Joystick3Button5, Joystick3Button6, Joystick3Button7, Joystick3Button8, Joystick3Button9, Joystick3Button10, Joystick3Button11, Joystick3Button12, Joystick3Button13, Joystick3Button14, Joystick3Button15, Joystick3Button16, Joystick3Button17, Joystick3Button18, Joystick3Button19, Joystick4Button0, Joystick4Button1, Joystick4Button2, Joystick4Button3, Joystick4Button4, Joystick4Button5, Joystick4Button6, Joystick4Button7, Joystick4Button8, Joystick4Button9, Joystick4Button10, Joystick4Button11, Joystick4Button12, Joystick4Button13, Joystick4Button14, Joystick4Button15, Joystick4Button16, Joystick4Button17, Joystick4Button18, Joystick4Button19, Joystick5Button0, Joystick5Button1, Joystick5Button2, Joystick5Button3, Joystick5Button4, Joystick5Button5, Joystick5Button6, Joystick5Button7, Joystick5Button8, Joystick5Button9, Joystick5Button10, Joystick5Button11, Joystick5Button12, Joystick5Button13, Joystick5Button14, Joystick5Button15, Joystick5Button16, Joystick5Button17, Joystick5Button18, Joystick5Button19, Joystick6Button0, Joystick6Button1, Joystick6Button2, Joystick6Button3, Joystick6Button4, Joystick6Button5, Joystick6Button6, Joystick6Button7, Joystick6Button8, Joystick6Button9, Joystick6Button10, Joystick6Button11, Joystick6Button12, Joystick6Button13, Joystick6Button14, Joystick6Button15, Joystick6Button16, Joystick6Button17, Joystick6Button18, Joystick6Button19, Joystick7Button0, Joystick7Button1, Joystick7Button2, Joystick7Button3, Joystick7Button4, Joystick7Button5, Joystick7Button6, Joystick7Button7, Joystick7Button8, Joystick7Button9, Joystick7Button10, Joystick7Button11, Joystick7Button12, Joystick7Button13, Joystick7Button14, Joystick7Button15, Joystick7Button16, Joystick7Button17, Joystick7Button18, Joystick7Button19, Joystick8Button0, Joystick8Button1, Joystick8Button2, Joystick8Button3, Joystick8Button4, Joystick8Button5, Joystick8Button6, Joystick8Button7, Joystick8Button8, Joystick8Button9, Joystick8Button10, Joystick8Button11, Joystick8Button12, Joystick8Button13, Joystick8Button14, Joystick8Button15, Joystick8Button16, Joystick8Button17, Joystick8Button18, Joystick8Button19
ToggleKey = K

## The panel size
# Setting type: Int32
# Default value: 75
PanelSize = 75

## The panel position from the bottom of the screen
# Setting type: Int32
# Default value: 35
PanelBottom = 35

## The panel position from the left of the screen
# Setting type: Int32
# Default value: 150
PanelLeft = 150
```
</details>

## Production Statistics

Add a button to the top left of the screen that when clicked, shows a panel of current production and consumption statistics.

The panel can be toggled via <kbd>F3</kbd> (configurable too).

The statistics is persisted in your save.

:information_source: The save format is not changed by the mod and remains compatible with the vanilla save, meaning that removing the mod won't break it.

:information_source: Note that the graphics does not adapt to the current screen resolution. Please adjust the font, button and panel sizes via configuration.

#### Configuration

<details><summary><code>akarnokd.planbterraformmods.featproductionstatistics</code></summary>

```
[General]

## Is the mod enabled?
# Setting type: Boolean
# Default value: true
Enabled = true

## Key to press while the building is selected to toggle its enabled/disabled state
# Setting type: KeyCode
# Default value: F3
# Acceptable values: None, Backspace, Tab, Clear, Return, Pause, Escape, Space, Exclaim, DoubleQuote, Hash, Dollar, Percent, Ampersand, Quote, LeftParen, RightParen, Asterisk, Plus, Comma, Minus, Period, Slash, Alpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9, Colon, Semicolon, Less, Equals, Greater, Question, At, LeftBracket, Backslash, RightBracket, Caret, Underscore, BackQuote, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, LeftCurlyBracket, Pipe, RightCurlyBracket, Tilde, Delete, Keypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9, KeypadPeriod, KeypadDivide, KeypadMultiply, KeypadMinus, KeypadPlus, KeypadEnter, KeypadEquals, UpArrow, DownArrow, RightArrow, LeftArrow, Insert, Home, End, PageUp, PageDown, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, Numlock, CapsLock, ScrollLock, RightShift, LeftShift, RightControl, LeftControl, RightAlt, LeftAlt, RightMeta, RightCommand, RightApple, LeftMeta, LeftCommand, LeftApple, LeftWindows, RightWindows, AltGr, Help, Print, SysReq, Break, Menu, Mouse0, Mouse1, Mouse2, Mouse3, Mouse4, Mouse5, Mouse6, JoystickButton0, JoystickButton1, JoystickButton2, JoystickButton3, JoystickButton4, JoystickButton5, JoystickButton6, JoystickButton7, JoystickButton8, JoystickButton9, JoystickButton10, JoystickButton11, JoystickButton12, JoystickButton13, JoystickButton14, JoystickButton15, JoystickButton16, JoystickButton17, JoystickButton18, JoystickButton19, Joystick1Button0, Joystick1Button1, Joystick1Button2, Joystick1Button3, Joystick1Button4, Joystick1Button5, Joystick1Button6, Joystick1Button7, Joystick1Button8, Joystick1Button9, Joystick1Button10, Joystick1Button11, Joystick1Button12, Joystick1Button13, Joystick1Button14, Joystick1Button15, Joystick1Button16, Joystick1Button17, Joystick1Button18, Joystick1Button19, Joystick2Button0, Joystick2Button1, Joystick2Button2, Joystick2Button3, Joystick2Button4, Joystick2Button5, Joystick2Button6, Joystick2Button7, Joystick2Button8, Joystick2Button9, Joystick2Button10, Joystick2Button11, Joystick2Button12, Joystick2Button13, Joystick2Button14, Joystick2Button15, Joystick2Button16, Joystick2Button17, Joystick2Button18, Joystick2Button19, Joystick3Button0, Joystick3Button1, Joystick3Button2, Joystick3Button3, Joystick3Button4, Joystick3Button5, Joystick3Button6, Joystick3Button7, Joystick3Button8, Joystick3Button9, Joystick3Button10, Joystick3Button11, Joystick3Button12, Joystick3Button13, Joystick3Button14, Joystick3Button15, Joystick3Button16, Joystick3Button17, Joystick3Button18, Joystick3Button19, Joystick4Button0, Joystick4Button1, Joystick4Button2, Joystick4Button3, Joystick4Button4, Joystick4Button5, Joystick4Button6, Joystick4Button7, Joystick4Button8, Joystick4Button9, Joystick4Button10, Joystick4Button11, Joystick4Button12, Joystick4Button13, Joystick4Button14, Joystick4Button15, Joystick4Button16, Joystick4Button17, Joystick4Button18, Joystick4Button19, Joystick5Button0, Joystick5Button1, Joystick5Button2, Joystick5Button3, Joystick5Button4, Joystick5Button5, Joystick5Button6, Joystick5Button7, Joystick5Button8, Joystick5Button9, Joystick5Button10, Joystick5Button11, Joystick5Button12, Joystick5Button13, Joystick5Button14, Joystick5Button15, Joystick5Button16, Joystick5Button17, Joystick5Button18, Joystick5Button19, Joystick6Button0, Joystick6Button1, Joystick6Button2, Joystick6Button3, Joystick6Button4, Joystick6Button5, Joystick6Button6, Joystick6Button7, Joystick6Button8, Joystick6Button9, Joystick6Button10, Joystick6Button11, Joystick6Button12, Joystick6Button13, Joystick6Button14, Joystick6Button15, Joystick6Button16, Joystick6Button17, Joystick6Button18, Joystick6Button19, Joystick7Button0, Joystick7Button1, Joystick7Button2, Joystick7Button3, Joystick7Button4, Joystick7Button5, Joystick7Button6, Joystick7Button7, Joystick7Button8, Joystick7Button9, Joystick7Button10, Joystick7Button11, Joystick7Button12, Joystick7Button13, Joystick7Button14, Joystick7Button15, Joystick7Button16, Joystick7Button17, Joystick7Button18, Joystick7Button19, Joystick8Button0, Joystick8Button1, Joystick8Button2, Joystick8Button3, Joystick8Button4, Joystick8Button5, Joystick8Button6, Joystick8Button7, Joystick8Button8, Joystick8Button9, Joystick8Button10, Joystick8Button11, Joystick8Button12, Joystick8Button13, Joystick8Button14, Joystick8Button15, Joystick8Button16, Joystick8Button17, Joystick8Button18, Joystick8Button19
ToggleKey = F3

## The font size in the panel
# Setting type: Int32
# Default value: 15
FontSize = 15

## The size of the item's icon in the list
# Setting type: Int32
# Default value: 32
ItemSize = 32

## The button's position relative to the left of the screen
# Setting type: Int32
# Default value: 100
ButtonLeft = 100

## The button's width and height
# Setting type: Int32
# Default value: 50
ButtonSize = 50

## How many lines of items to show
# Setting type: Int32
# Default value: 16
MaxLines = 16

## How many days to keep as past production data?
# Setting type: Int32
# Default value: 300
HistoryLength = 300
```
</details>

## Live GUI Scaler

:information_source: Requires **game build 623+**, does nothing on older versions due to lack of official scaling support.

Scale the GUI by holding <kbd>CTRL</kbd> and pressing <kbd>Numpad Plus</kbd>, <kbd>Numpad Minus</kbd> or via <kbd>Mouse wheel</kbd>.

You can configure the minimum (50%), maximum (300%) and scaling step (5%). 

:information_source: Note that the game's scaling is between 70% to 270% and may jump back into this range when opening the options screen.

#### Configuration

<details><summary><code>akarnokd.planbterraformmods.uiliveguiscaler</code></summary>

```
[General]

## Is the mod enabled?
# Setting type: Boolean
# Default value: true
Enabled = true

## The minimum percent for scaling.
# Setting type: Int32
# Default value: 50
MinScale = 50

## The maximum percent for scaling.
# Setting type: Int32
# Default value: 300
MaxScale = 300

## Step percent of scaling when changing it
# Setting type: Int32
# Default value: 5
Step = 5
```
</details>

## City Population Label

Display the city population number under the city's name in the main view and/or minimap (configurable).

#### Configuration

<details><summary><code>akarnokd.planbterraformmods.uicitypopulationlabel</code></summary>

```
[General]

## Is the mod enabled?
# Setting type: Boolean
# Default value: true

Enabled = true
## Show the label on the main view?
# Setting type: Boolean
# Default value: true
ShowOnMain = true

## Show the label on the minimap view?
# Setting type: Boolean
# Default value: true
ShowOnMinimap = true

```
</details>

## Hotbar

Adds a bar at the bottom of the screen with 3 subpanels and 9 slots each for quickly selecting a building to be built.

The bar can be hidden via <kbd>H</kbd> key (configurable). 

To select a building for a slot, <kbd>Right click</kbd> on it. Press <kbd>Escape</kbd> or right click on the panel or any slot to hide the selection panel. The panel is scrollable via <kbd>Mouse scroll</kbd>.

To clear a slot, <kbd>Middle click</kbd> on the slot.

To start building a non-empty slot, <kbd>Left click</kbd> on it.

:warning: The hotbar is currently saved as global settings (i.e., save independent) and allows selecting any building, not just the unlocked ones, so spoilers!

#### Configuration

<details><summary><code>akarnokd.planbterraformmods.feathotbar</code></summary>

```
[General]

## Is the mod enabled?
# Setting type: Boolean
# Default value: true
Enabled = true

## The height of the panel
# Setting type: Int32
# Default value: 75
PanelHeight = 75

## The distance from the bottom of the screen
# Setting type: Int32
# Default value: 45
PanelBottom = 45

## Scale the position and size of the button with the UI scale of the game?
# Setting type: Boolean
# Default value: true
AutoScale = true

## The size of the item's icon in the building selection list
# Setting type: Int32
# Default value: 32
ItemSize = 32

## How many lines of items to show in the building selection list
# Setting type: Int32
# Default value: 16
MaxLines = 16

## The font size in the building selection panel
# Setting type: Int32
# Default value: 15
FontSize = 15

## The font size of the total current count on buildings
# Setting type: Int32
# Default value: 12
FontSizeSmall = 12

## The key to show/hide the hotbar
# Setting type: KeyCode
# Default value: H
# Acceptable values: None, Backspace, Tab, Clear, Return, Pause, Escape, Space, Exclaim, DoubleQuote, Hash, Dollar, Percent, Ampersand, Quote, LeftParen, RightParen, Asterisk, Plus, Comma, Minus, Period, Slash, Alpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9, Colon, Semicolon, Less, Equals, Greater, Question, At, LeftBracket, Backslash, RightBracket, Caret, Underscore, BackQuote, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, LeftCurlyBracket, Pipe, RightCurlyBracket, Tilde, Delete, Keypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9, KeypadPeriod, KeypadDivide, KeypadMultiply, KeypadMinus, KeypadPlus, KeypadEnter, KeypadEquals, UpArrow, DownArrow, RightArrow, LeftArrow, Insert, Home, End, PageUp, PageDown, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, Numlock, CapsLock, ScrollLock, RightShift, LeftShift, RightControl, LeftControl, RightAlt, LeftAlt, RightMeta, RightCommand, RightApple, LeftMeta, LeftCommand, LeftApple, LeftWindows, RightWindows, AltGr, Help, Print, SysReq, Break, Menu, Mouse0, Mouse1, Mouse2, Mouse3, Mouse4, Mouse5, Mouse6, JoystickButton0, JoystickButton1, JoystickButton2, JoystickButton3, JoystickButton4, JoystickButton5, JoystickButton6, JoystickButton7, JoystickButton8, JoystickButton9, JoystickButton10, JoystickButton11, JoystickButton12, JoystickButton13, JoystickButton14, JoystickButton15, JoystickButton16, JoystickButton17, JoystickButton18, JoystickButton19, Joystick1Button0, Joystick1Button1, Joystick1Button2, Joystick1Button3, Joystick1Button4, Joystick1Button5, Joystick1Button6, Joystick1Button7, Joystick1Button8, Joystick1Button9, Joystick1Button10, Joystick1Button11, Joystick1Button12, Joystick1Button13, Joystick1Button14, Joystick1Button15, Joystick1Button16, Joystick1Button17, Joystick1Button18, Joystick1Button19, Joystick2Button0, Joystick2Button1, Joystick2Button2, Joystick2Button3, Joystick2Button4, Joystick2Button5, Joystick2Button6, Joystick2Button7, Joystick2Button8, Joystick2Button9, Joystick2Button10, Joystick2Button11, Joystick2Button12, Joystick2Button13, Joystick2Button14, Joystick2Button15, Joystick2Button16, Joystick2Button17, Joystick2Button18, Joystick2Button19, Joystick3Button0, Joystick3Button1, Joystick3Button2, Joystick3Button3, Joystick3Button4, Joystick3Button5, Joystick3Button6, Joystick3Button7, Joystick3Button8, Joystick3Button9, Joystick3Button10, Joystick3Button11, Joystick3Button12, Joystick3Button13, Joystick3Button14, Joystick3Button15, Joystick3Button16, Joystick3Button17, Joystick3Button18, Joystick3Button19, Joystick4Button0, Joystick4Button1, Joystick4Button2, Joystick4Button3, Joystick4Button4, Joystick4Button5, Joystick4Button6, Joystick4Button7, Joystick4Button8, Joystick4Button9, Joystick4Button10, Joystick4Button11, Joystick4Button12, Joystick4Button13, Joystick4Button14, Joystick4Button15, Joystick4Button16, Joystick4Button17, Joystick4Button18, Joystick4Button19, Joystick5Button0, Joystick5Button1, Joystick5Button2, Joystick5Button3, Joystick5Button4, Joystick5Button5, Joystick5Button6, Joystick5Button7, Joystick5Button8, Joystick5Button9, Joystick5Button10, Joystick5Button11, Joystick5Button12, Joystick5Button13, Joystick5Button14, Joystick5Button15, Joystick5Button16, Joystick5Button17, Joystick5Button18, Joystick5Button19, Joystick6Button0, Joystick6Button1, Joystick6Button2, Joystick6Button3, Joystick6Button4, Joystick6Button5, Joystick6Button6, Joystick6Button7, Joystick6Button8, Joystick6Button9, Joystick6Button10, Joystick6Button11, Joystick6Button12, Joystick6Button13, Joystick6Button14, Joystick6Button15, Joystick6Button16, Joystick6Button17, Joystick6Button18, Joystick6Button19, Joystick7Button0, Joystick7Button1, Joystick7Button2, Joystick7Button3, Joystick7Button4, Joystick7Button5, Joystick7Button6, Joystick7Button7, Joystick7Button8, Joystick7Button9, Joystick7Button10, Joystick7Button11, Joystick7Button12, Joystick7Button13, Joystick7Button14, Joystick7Button15, Joystick7Button16, Joystick7Button17, Joystick7Button18, Joystick7Button19, Joystick8Button0, Joystick8Button1, Joystick8Button2, Joystick8Button3, Joystick8Button4, Joystick8Button5, Joystick8Button6, Joystick8Button7, Joystick8Button8, Joystick8Button9, Joystick8Button10, Joystick8Button11, Joystick8Button12, Joystick8Button13, Joystick8Button14, Joystick8Button15, Joystick8Button16, Joystick8Button17, Joystick8Button18, Joystick8Button19
ToggleKey = H

## The list of buildings for subpanel 1
# Setting type: String
# Default value: 
Loadout1 = depot,factory,cityOut,,,,,,

## The list of buildings for subpanel 2
# Setting type: String
# Default value: 
Loadout2 = railway,railstop,,,,,,,

## The list of buildings for subpanel 3
# Setting type: String
# Default value: 
Loadout3 = ,,,,,,,,

```
</details>
