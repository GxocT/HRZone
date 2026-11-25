# HR Zone <a href="https://github.com/GxocT/HRZone/releases"><img src="https://img.shields.io/github/v/release/GxocT/HRZone?include_prereleases&style=for-the-badge&label=RELEASE" align="right"></a> <img src="https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2FGxocT%2FHRZone%2Fmain%2FHRZone%2Fmanifest.json&query=%24.gameVersion&label=Beat%20Saber&color=ff0048&style=for-the-badge" align="right">

**English** | [Русский](README.ru.md)

A Beat Saber custom counter that displays your heart rate in-game, based on [HRCounter](https://github.com/qe201020335/HRCounter) by [qe201020335](https://github.com/qe201020335).

## Features

- 📡 Reads your heart rate from a configured HTTP feed (see [Data Sources](#data-sources))
- ❤️ Five heart-rate zones derived from your `MaxHR`, each with its own color
- 🎨 `Colorize` tints the heart rate number and `Colorize Icon` tints the heart icon with the current zone color, independently
- 🖼️ Customizable counter icon — pick one of the six preset hearts or drop your own image into `UserData/HRZone/Icons/`
- 🕐 Menu display — a clock line and a heart rate counter above the song selection screen; clock and counter toggle independently, the clock format is selectable and the block position adjustable (both on by default)
- 🌈 Zone color presets — `Sunset`, `Pastel` and `Bright`, or `User Theme` for your own five colors
- 👻 `Autohide Heart Rate` keeps the in-level counter hidden until the first heart rate arrives
- 🔌 Optional integration with [Counters+](https://github.com/NuggoDEV/CountersPlus) — HR Zone appears as the `HRZone` counter in the Counters+ list, with an `Offset` slider on its Counters+ page to shift the widget up or down

## Requirements

> [!NOTE]
> HR Zone is not on BeatMods yet.

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
6. Adjust the counter, zones and colors on the **HR Zone** and **Theme** tabs, the icon on the **Icon** tab, and the menu clock & heart rate block on the **Menu** tab.

> [!NOTE]
> The menu block shows a clock. If another mod (for example Enhancements) shows a clock as well, turn one of the two off to avoid double clocks.

To uninstall, delete `Beat Saber/Plugins/HRZone.dll`; optionally also remove the mod's data: `UserData/HR Zone.json` and `UserData/HRZone/`.

> [!NOTE]
> Please open an [issue](https://github.com/GxocT/HRZone/issues) if you run into a problem or find a bug.

## Data Sources

### WebRequest 🌐

Polls an http/https endpoint and reads the heart rate from the response body. Either format works — a plain number:

```
72
```

or a JSON object with a `bpm` field:

```json
{"bpm": 72}
```

Enter your URL in the **Request URL** field of the **Server** tab and confirm it on the keyboard; it saves immediately, no Apply button needed. Below the field the tab previews the feed: your heart rate when the request works, `No connection` when it fails, `URL not set` when the field is empty and `Mod is disabled` when the mod is turned off. The preview is fed by the same once-per-second poller that drives the menu display, so it is current the moment the screen opens.

During a level the feed is polled once per second as well; when it is unreachable, the mod makes four connect attempts 3 seconds apart and then stops requesting it until the next level.

The same value is stored as `FeedLink`, so very long URLs can also be pasted into the config file. See [Manual Config Editing](#manual-config-editing).

### More? 💡

Open an [issue](https://github.com/GxocT/HRZone/issues) if there's a device or service you'd like supported.

## Counters+ Integration

With [Counters+](https://github.com/NuggoDEV/CountersPlus) installed, HR Zone shows up as the `HRZone` counter in the Counters+ list. The integration is **off by default**: turn on `Counters+ Integration` in the General tab of HR Zone settings, and the widget renders on a Counters+ canvas, placed like any other Counters+ counter. Its position comes from the Counters+ settings screen, where the `HRZone` page has an `Offset` slider nudging the widget up or down; the look (number color, heart icon, bloom) stays in HR Zone settings.

The two displays are independent: `ShowInGameCounter` toggles HR Zone's own counter and the integration has its own toggle, so they can both be on at once — turn one off if you don't want double counters. Both toggles take effect from the next level start. The hosted widget follows the same rules as the standalone counter: hidden during BeatLeader and ScoreSaber replays and while the mod is disabled.

## Manual Config Editing

> [!IMPORTANT]
> Exit the game before editing the config file; edits are applied when the game starts.

Most options can be changed in-game but if you want to edit the config directly, the config file is at `Beat Saber/UserData/HR Zone.json` (BSIPA names it after the mod's display name). See the full field reference below.

| Field                    |   Type    |       Default        | Description                                                                                       |
|--------------------------|:---------:|:--------------------:|---------------------------------------------------------------------------------------------------|
| `ModEnable`              |   bool    |        `true`        | Enables the mod (DUH).                                                                            |
| `ShowInGameCounter`      |   bool    |        `true`        | Show HR Zone's own counter in game.                                                               |
| `DataSource`             |  string   |     `"WebRequest"`   | Data source used to get heart rate.                                                               |
| `FeedLink`               |  string   | `http://127.0.0.1:62261/bpm` | URL for the `WebRequest` data source. A fresh install points at the local feed endpoint; clear it to disable the feed.  |
| `NoBloom`                |   bool    |       `false`        | Do you want no bloom on the counter number and heart icon?                                        |
| `Colorize`               |   bool    |        `true`        | Colorize the heart rate number.                                                                   |
| `ColorizeIcon`           |   bool    |        `true`        | Tint the heart icon with the current zone color. Turn off to keep the icon image's own colors.    |
| `AutoHideCounter`        |   bool    |       `false`        | Keep the in-level counter hidden until the first heart rate arrives.                              |
| `CountersPlusIntegration` |   bool   |       `false`        | Show the heart rate widget on a Counters+ canvas (opt-in, see [Counters+ Integration](#counters-integration)). |
| `CountersPlusOffsetY`     |  float   |        `0`           | Vertical offset of the hosted Counters+ widget in meters (-1.5..1.5), set by the `Offset` slider on the `HRZone` page in Counters+ settings. |
| `MaxHR`                  |    int    |        `180`         | Player maximum heart rate (120-220); the five HR zones are derived from it.                       |
| `Zone1Color`             |  string   |  `"#9C8F8C"` (Mauve) | Color below 60 % of `MaxHR`.                                                                      |
| `Zone2Color`             |  string   |  `"#7FA6C4"` (Blue)  | Color for 60-70 % of `MaxHR`.                                                                     |
| `Zone3Color`             |  string   | `"#93C08A"` (Green)  | Color for 70-80 % of `MaxHR`.                                                                     |
| `Zone4Color`             |  string   | `"#F0A35E"` (Orange) | Color for 80-90 % of `MaxHR`.                                                                     |
| `Zone5Color`             |  string   |  `"#E76A5E"` (Red)   | Color for 90 % of `MaxHR` and above.                                                              |
| `ColorTheme`             |  string   |      `"Sunset"`      | Zone color preset: `Sunset`, `Pastel`, `Bright` or `User Theme`. Picking a preset rewrites the five `Zone*Color` fields; editing a color on the Theme tab keeps your colors as `User Theme`. |
| `UserThemeZone1Color`    |  string   |  `"#9C8F8C"` (Mauve) | Your color below 60 % of `MaxHR`, used while `ColorTheme` is `User Theme`.                        |
| `UserThemeZone2Color`    |  string   |  `"#7FA6C4"` (Blue)  | Your color for 60-70 % of `MaxHR`.                                                                |
| `UserThemeZone3Color`    |  string   | `"#93C08A"` (Green)  | Your color for 70-80 % of `MaxHR`.                                                                |
| `UserThemeZone4Color`    |  string   | `"#F0A35E"` (Orange) | Your color for 80-90 % of `MaxHR`.                                                                |
| `UserThemeZone5Color`    |  string   |  `"#E76A5E"` (Red)   | Your color for 90 % of `MaxHR` and above.                                                         |
| `CustomIcon`             |  string   |         `""`         | Filename of a custom icon under `UserData/HRZone/Icons/`. Empty string uses the default heart icon. |
| `StaticCounterPosition`  | 3D Vector |    `(0, 1.2, 7)`     | World-space position of the standalone counter in non-360/90 modes.                               |
| `ShowMenuClock`          |   bool    |        `true`        | Show the clock line in the menus, above the menu heart rate counter.                              |
| `ShowMenuHeartRate`      |   bool    |        `true`        | Show the heart rate counter in the menus.                                                         |
| `MenuClockFormat`        |  string   |      `"HH:mm"`       | Clock format: `HH:mm`, `HH:mm:ss` or `h:mm tt`. An unknown value falls back to `HH:mm`.           |
| `MenuCounterHeight`      |  float    |         `0`          | Menu block up/down offset in meters (-3..3); `0` is the default position.                         |
| `MenuCounterDistance`    |  float    |         `0`          | Menu block closer/farther offset in meters (-3..3); `0` is the default position.                  |
| `MenuCounterTilt`        |  float    |         `0`          | Menu block tilt in degrees (-45..45) on top of its base tilt.                                     |
