# Newbie I18n

Simple U# Internationalization (i18n) library for VRChat.

## Features

- Translate `Text` / `TextMeshPro` texts by key, using `I18NText` and `I18NTMPText`.
- Initial language respects VRChat Client language settings.
- Switch languages at runtime with `I18NLanguageSelectionDropdown`.
- Write translations in Google Sheets (or CSV), then export them using `Tools/DerpyNewbie/I18N/Sheet Exporter`.
- Falls back to the first language when a translation is missing.
- Custom translation sources by extending `I18NProvider`.

## How to Import

### Import using VCC

1. Open this [link](https://derpynewbie.github.io/vpm-repos/) and add repository to your VCC.
2. Add Newbie I18n package from DerpyNewbie repos
3. Done!

### Import using unitypackage

1. Open [releases](https://github.com/DerpyNewbie/NewbieI18n/releases)
2. Download unitypackage in assets.
3. Import downloaded unitypackage to your project.
4. Done!

> [!NOTE]
> This package requires [Newbie Commons](https://github.com/DerpyNewbie/NewbieCommons).

## FAQ

### Q. Where is the proper documentation?

I'm lazy, so there's no documentation yet. Check out the sample scene in `Samples` for now.

### Q. How do I format the translation sheet?

First row is a header with language codes. `@name` row is for language names shown in dropdown (optional).
Rows with key starting with `#`, and columns with header starting with `#` are ignored.

| key       | ja         | en          | #memo         |
|-----------|------------|-------------|---------------|
| @name     | 日本語     | English     |               |
| # comment |            |             |               |
| menu.title| メニュー   | Menu        | for title     |

See `Samples/SampleTranslations.csv` for an example.

### Q. How do I write custom views?

Extend `I18NView`, Override `OnI18NRefresh()`, Get the text using `provider.GetString(key)`, Done!
