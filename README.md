**TMXTile** provides a generic way to load Tiled's `.tmx` map files into a game which uses the
xTile map engine. For example, this is used by [SMAPI](https://github.com/Pathoschild/SMAPI/) to
let any Stardew Valley mod use `.tmx` files.

## Usage
1. Install the [`Platonymous.TMXTile` NuGet package](https://www.nuget.org/packages/Platonymous.TMXTile).
2. Run this code sometime before you need to load `.tmx` files (filling in the correct arguments
   for the game's tile size):
   ```c#
   var tileSize = new Size(64, 64);
   FormatManager.Instance.RegisterMapFormat(new TMXTile.TMXFormat(tileSize));
   ```
3. Now you can load `.tmx` files directly through xTile:
   ```c#
   Map map = FormatManager.Instance.LoadMap("path/to/map.tmx");
   ```

## Advanced
### Extended properties
`.tmx` maps support some features that aren't recognized by xTile, like tile rotation and layer
opacity. These are ignored by xTile, but TMXTile adds properties and extension methods to let you
optionally manage them.

For example, you can implement XNA Framework's `IDisplayDevice` to add missing features in-code.
See [SMAPI's `SDisplayDevice`](https://github.com/Pathoschild/SMAPI/blob/develop/src/SMAPI/Framework/Rendering/SDisplayDevice.cs)
for example code which implements flip and rotation.

Layer properties:

property      | type       | description
------------- | ---------- | -----------
`@Opacity`    | `float`    | The layer opacity as a value between 0 (fully transparent) and 1 (fully opaque). You can get the opacity using `layer.GetOpacity()` and set it like `layer.SetOpacity(0.5f)`.

Tile properties:

property      | type       | description
------------- | ---------- | -----------
`@Opacity`    | `float`    | The tile opacity as a value between 0 (fully transparent) and 1 (fully opaque). You can get the opacity using `tile.GetOpacity()` and set it like `tile.SetOpacity(0.5f)`.
`@Rotation`   | `int`      | The tile rotation in degrees. You can get a normalized value using `tile.GetRotation()`, and set it like `tile.SetRotation(180)`.
`@Flip`       | `SpriteEffects` | The horizontal and vertical flips applied to the tile. You can get the value using `(SpriteEffects)tile.GetSpriteEffects()`, and set it like `tile.SetSpriteEffects((int)SpriteEffects.FlipHorizontally)`.

_TODO: document `@BackgroundColor`/`@Color`/`@TColor`, `@ImageLayer`/`@ImageLayerTileSheet`, and
`@OffsetX`/`@OffsetY`._

### Compile from source
1. Copy `xTile.dll` from the game folder to `src/TMXTile/lib/xTile.dll`.
2. Open the solution in Visual Studio.
3. Compile.