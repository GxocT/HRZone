# HR Zone <a href="https://github.com/GxocT/HRZone/releases"><img src="https://img.shields.io/github/v/release/GxocT/HRZone?include_prereleases&style=for-the-badge&label=RELEASE" align="right"></a> <img src="https://img.shields.io/badge/Beat_Saber-1.40.8-ff0048?style=for-the-badge" align="right">

**English** | [Русский](README.ru.md)

A Beat Saber custom counter that displays your heart rate in-game, based on [HRCounter](https://github.com/qe201020335/HRCounter) by [qe201020335](https://github.com/qe201020335).

## Features

- Reads your heart rate from a configured HTTP feed (see [Data Sources](#data-sources))
- Five heart-rate zones derived from your `MaxHR`, each with its own color
- `Colorize` tints the heart rate number and `Colorize Icon` tints the heart icon with the current zone color, independently
- The counter shows `--` when no heart rate value is available
- Customizable counter icon (drop your own image into `UserData/HRZone/Icons/`)
- World-space standalone counter that follows the HUD rotation in 360°/90° modes
- Counter stays hidden while a **BeatLeader** or **ScoreSaber** replay is playing

## Requirements

> [!NOTE]
> HR Zone is not on BeatMods yet, so mod installers like BSManager cannot install it — follow the manual steps below and check back after the mod is approved.

* Beat Saber **1.40.8** — the game version the mod is built and tested against
* [BSIPA](https://github.com/bsmg/BeatSaber-IPA-Reloaded) v4.3.6+
* [BSML](https://github.com/monkeymanboy/BeatSaberMarkupLanguage) v1.12.0+
* [SiraUtil](https://github.com/Auros/SiraUtil) v3.0.0+

## How to Use

> [!IMPORTANT]
> Make sure all required mods are working before installing HR Zone.

1. Download the [latest release](https://github.com/GxocT/HRZone/releases/latest).
2. Extract it into your `Beat Saber` game directory (the one next to `Beat Saber.exe`): you should end up with `Beat Saber/Plugins/HRZone.dll`. If the archive extracted into its own subfolder, move the `Plugins` folder up.
3. Launch the game.
4. Open the **HR Zone** settings menu from the main menu's mods section.
5. On the **Server** tab, pick the data source and enter your feed URL. See [Data Sources](#data-sources) for details.
6. Adjust zones, colors and the icon on the **HR Zone**, **Colorize** and **Icon** tabs.

To uninstall, delete `Beat Saber/Plugins/HRZone.dll`; optionally also remove the mod's data: `UserData/HR Zone.json` and `UserData/HRZone/`.

> [!NOTE]
> Please open an [issue](https://github.com/GxocT/HRZone/issues) if you run into a problem or find a bug.

## Data Sources

### WebRequest

Polls an http/https endpoint that returns HR data in either of these formats:

* A JSON object containing `bpm` (int)
* A plain integer body (regex `^\d+$`)

Enter your URL in the **Request URL** field of the **Server** tab and confirm it on the keyboard; it saves immediately, no Apply button needed. Below the field the tab previews the feed: your heart rate when the request works, `No connection` when it fails, and `URL not set` when the field is empty.

The same value is stored as `FeedLink`, so very long URLs can also be pasted into the config file. See [Manual Config Editing](#manual-config-editing).

### More?

Open an [issue](https://github.com/GxocT/HRZone/issues) if there's a device or service you'd like supported.

## Manual Config Editing

> [!IMPORTANT]
> Due to a BSIPA bug, edits made to the config file while the game is running won't be detected. Exit the game before editing.

Most options can be changed in-game but if you want to edit the config directly, the config file is at `Beat Saber/UserData/HR Zone.json` (BSIPA names it after the mod's display name). See the full field reference below.

| Field                    |   Type    |       Default        | Description                                                                                       |
|--------------------------|:---------:|:--------------------:|---------------------------------------------------------------------------------------------------|
| `ModEnable`              |   bool    |        `true`        | Enables the mod (DUH).                                                                            |
| `DataSource`             |  string   |     `"WebRequest"`   | Data source used to get heart rate.                                                               |
| `FeedLink`               |  string   | `http://127.0.0.1:62261/bpm` | URL for the `WebRequest` data source. A fresh install points at the local feed endpoint; clear it to disable the feed.  |
| `NoBloom`                |   bool    |       `false`        | Do you want no bloom on the counter number and heart icon?                                        |
| `Colorize`               |   bool    |        `true`        | Colorize the heart rate number.                                                                   |
| `ColorizeIcon`           |   bool    |        `true`        | Tint the heart icon with the current zone color. Turn off to keep the icon image's own colors.    |
| `MaxHR`                  |    int    |        `180`         | Player maximum heart rate (120-220); the five HR zones are derived from it.                       |
| `Zone1Color`             |  string   |  `"#909090"` (Gray)  | Color below 60 % of `MaxHR`.                                                                      |
| `Zone2Color`             |  string   |  `"#2FA0FF"` (Blue)  | Color for 60-70 % of `MaxHR`.                                                                     |
| `Zone3Color`             |  string   | `"#00FF00"` (Green)  | Color for 70-80 % of `MaxHR`.                                                                     |
| `Zone4Color`             |  string   | `"#FFFF00"` (Yellow) | Color for 80-90 % of `MaxHR`.                                                                     |
| `Zone5Color`             |  string   |  `"#FF0000"` (Red)   | Color for 90 % of `MaxHR` and above.                                                              |
| `CustomIcon`             |  string   |         `""`         | Filename of a custom icon under `UserData/HRZone/Icons/`. Empty string uses the default heart icon. |
| `StaticCounterPosition`  | 3D Vector |    `(0, 1.2, 7)`     | World-space position of the standalone counter in non-360/90 modes.                               |
