# HR Zone <a href="https://github.com/GxocT/HRZone/releases"><img src="https://img.shields.io/github/v/release/GxocT/HRZone?include_prereleases&style=for-the-badge&label=RELEASE" align="right"></a> <img src="https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2FGxocT%2FHRZone%2Fmain%2FHRZone%2Fmanifest.json&query=%24.gameVersion&label=Beat%20Saber&color=ff0048&style=for-the-badge" align="right">

**[English](README.md)** | Русский

Кастомный счётчик для Beat Saber, показывающий ваш пульс прямо в игре. Основан на [HRCounter](https://github.com/qe201020335/HRCounter) от [qe201020335](https://github.com/qe201020335).

## Возможности

- 📡 Читает пульс из настроенного HTTP-фида (см. [Источники данных](#источники-данных))
- ❤️ Пять пульсовых зон, которые вычисляются из вашего `MaxHR`, у каждой свой цвет
- 🎨 `Colorize` окрашивает число пульса, а `Colorize Icon` — иконку сердца в цвет текущей зоны, независимо друг от друга
- 🖼️ Настраиваемая иконка счётчика — выберите одну из шести готовых сердец или положите своё изображение в `UserData/HRZone/Icons/`

## Требования

> [!NOTE]
> HR Zone ещё не на BeatMods.

* Beat Saber **1.40.8** — версия игры, на которой мод собран и протестирован
* [BSIPA](https://github.com/bsmg/BeatSaber-IPA-Reloaded) v4.3.6+
* [BSML](https://github.com/monkeymanboy/BeatSaberMarkupLanguage) v1.12.0+
* [SiraUtil](https://github.com/Auros/SiraUtil) v3.0.0+

## Как использовать

> [!IMPORTANT]
> Перед установкой HR Zone убедитесь, что все обязательные моды работают.

1. Скачайте [последний релиз](https://github.com/GxocT/HRZone/releases/latest).
2. Распакуйте в папку игры `Beat Saber` (ту, где лежит `Beat Saber.exe`): должен получиться файл `Beat Saber/Plugins/HRZone.dll`. Если архив распаковался в свою подпапку, поднимите папку `Plugins` на уровень выше.
3. Запустите игру.
4. Откройте меню настроек **HR Zone** в разделе модов главного меню.
5. На вкладке **Server** выберите источник данных и укажите URL фида. Подробности — в разделе [Источники данных](#источники-данных).
6. Настройте зоны, цвета и иконку на вкладках **HR Zone**, **Colorize** и **Icon**.

Для удаления удалите `Beat Saber/Plugins/HRZone.dll`; при желании заодно удалите данные мода: `UserData/HR Zone.json` и `UserData/HRZone/`.

> [!NOTE]
> Если столкнулись с проблемой или нашли баг — откройте [issue](https://github.com/GxocT/HRZone/issues).

## Источники данных

### WebRequest 🌐

Опрашивает http/https-эндпоинт и берёт пульс из тела ответа. Работает оба формата — просто число:

```
72
```

или JSON-объект с полем `bpm`:

```json
{"bpm": 72}
```

URL указывается в поле **Request URL** на вкладке **Server** и подтверждается на клавиатуре; сохраняется сразу, кнопка Apply не нужна. Под полем вкладка показывает превью фида: ваш пульс, если запрос проходит, `No connection` при ошибке и `URL not set`, если поле пустое.

То же значение хранится в `FeedLink`, поэтому очень длинный URL можно вставить прямо в файл конфига. См. [Ручное редактирование конфига](#ручное-редактирование-конфига).

### Больше? 💡

Откройте [issue](https://github.com/GxocT/HRZone/issues), если хотите поддержку какого-то устройства или сервиса.

## Ручное редактирование конфига

> [!IMPORTANT]
> Перед редактированием файла конфига выйдите из игры — изменения применяются при запуске.

Большинство настроек меняется прямо в игре, но если хотите править конфиг напрямую: файл лежит в `Beat Saber/UserData/HR Zone.json` (BSIPA называет его по отображаемому имени мода). Полная справка по полям ниже.

| Поле                     |   Тип     |   По умолчанию      | Описание                                                                                          |
|--------------------------|:---------:|:-------------------:|---------------------------------------------------------------------------------------------------|
| `ModEnable`              |   bool    |        `true`       | Включает мод (и так очевидно).                                                                    |
| `DataSource`             |  string   |    `"WebRequest"`   | Источник данных пульса.                                                                           |
| `FeedLink`               |  string   | `http://127.0.0.1:62261/bpm` | URL для источника `WebRequest`. При свежей установке указан локальный фид; очистите, чтобы отключить фид.  |
| `NoBloom`                |   bool    |       `false`       | Отключить свечение (bloom) у числа счётчика и иконки сердца?                                      |
| `Colorize`               |   bool    |        `true`       | Окрашивать число пульса.                                                                          |
| `ColorizeIcon`           |   bool    |        `true`       | Окрашивать иконку сердца в цвет текущей зоны. Выключите, чтобы сохранить цвета самой иконки.      |
| `MaxHR`                  |    int    |        `180`        | Максимальный пульс игрока (120-220); от него вычисляются пять зон.                               |
| `Zone1Color`             |  string   |  `"#9C8F8C"` (сиреневый) | Цвет ниже 60 % от `MaxHR`.                                                                   |
| `Zone2Color`             |  string   |  `"#7FA6C4"` (голубой)   | Цвет для 60-70 % от `MaxHR`.                                                                   |
| `Zone3Color`             |  string   | `"#93C08A"` (зелёный)    | Цвет для 70-80 % от `MaxHR`.                                                                   |
| `Zone4Color`             |  string   | `"#F0A35E"` (оранжевый)  | Цвет для 80-90 % от `MaxHR`.                                                                   |
| `Zone5Color`             |  string   |  `"#E76A5E"` (красный)   | Цвет для 90 % `MaxHR` и выше.                                                                  |
| `CustomIcon`             |  string   |         `""`        | Имя файла иконки в `UserData/HRZone/Icons/`. Пустая строка — стандартная иконка сердца.          |
| `StaticCounterPosition`  | 3D-вектор |    `(0, 1.2, 7)`    | Позиция счётчика в мире для режимов без 360/90.                                                   |
