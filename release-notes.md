← [README](README.md)

# Release notes
## 1.3.7
Released 31 May 2020.

* Fixed CSV tile layers not loaded correctly on Android (thanks to Pathoschild!).

## 1.3.3
Released 07 March 2020.

* Added `@TColor` property.
* `@Rotation` now uses degrees instead of π rad.
* Fixed rotation/flip conversions.

## 1.2.4
Released 06 March 2020.

* Added `@Rotation` property.
* Flips are now converted into an equivalent `@Rotation` value when applicable.
* Merged `@FLIPPED_*` properties into one `@Flip` property containing a `SpriteEffects` value.
* Improved extension methods.

## 1.2.2
Released 05 March 2020.

* Fixed `@BackgroundColor` not always set correctly.

## 1.1.9
Released 04 March 2020.

* Added support for zlib-compressed tile layers.
* Added alpha support to color properties.

## 1.1.6
Released 04 March 2020.

* Added `.tsx` tilesheet support.
* Added `@FLIPPED_HORIZONTALLY`, `@FLIPPED_VERTICALLY`, and `@FLIPPED_DIAGONALLY` properties.
* All extended property names now start with `@`.
* Added extension methods to get/set extended properties.

## 1.0.2
Released 01 March 2020.

* Changed target framework from `net452` to `net45`.
* Fixed TMX tile data not being loaded in some cases.

## 1.0
Released 01 November 2019.

* Initial release.
