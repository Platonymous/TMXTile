using System;
using System.Linq;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace TMXTile
{
    public struct DrawInstructions
    {
        public float Rotation;
        public int Effect;
        public Location Offset;
        public TMXColor Color;
        public float Opacity;
    }

    public static class TMXExtensions
    {

        public static bool IsImageLayer(this Layer layer)
        {
            return layer.Properties.ContainsKey("@ImageLayer") && layer.Properties["@ImageLayer"] == true;
        }

        public static void MakeImageLayer(this Layer layer)
        {
            layer.Properties["@ImageLayer"] = true;
        }

        public static TileSheet GetTileSheetForImageLayer(this Layer layer)
        {
            if (!layer.IsImageLayer())
                return null;

            if (layer.Properties.ContainsKey("@ImageLayerTileSheet") && layer.Map.TileSheets.FirstOrDefault(t => t.Id == (string) layer.Properties["@ImageLayerTileSheet"]) is TileSheet ts)
                return ts;

            return null;
        }

        public static void SetTileSheetForImageLayer(this Layer layer, TileSheet tileSheet)
        {
            layer.Properties["@ImageLayerTileSheet"] = tileSheet.Id;
        }

        public static Location GetOffset(this Layer layer)
        {
            return new Location(
                layer.Properties.ContainsKey("@OffsetX") ? (int) layer.Properties["@OffsetX"] : 0,
                layer.Properties.ContainsKey("@OffsetY") ? (int) layer.Properties["@OffsetY"] : 0
                );
        }

        public static Location GetOffset(this Tile tile)
        {
            return new Location(
                tile.Properties.ContainsKey("@OffsetX") ? (int)tile.Properties["@OffsetX"] : 0,
                tile.Properties.ContainsKey("@OffsetY") ? (int)tile.Properties["@OffsetY"] : 0
                );
        }

        public static void SetOffset(this Layer layer, Location offset)
        {
            layer.Properties["@OffsetX"] = offset.X;
            layer.Properties["@OffsetY"] = offset.Y;
        }

        public static void SetOffset(this Tile tile, Location offset)
        {
            tile.Properties["@OffsetX"] = offset.X;
            tile.Properties["@OffsetY"] = offset.Y;
        }

        public static float GetOpacity(this Layer layer)
        {
            if(layer.Properties.ContainsKey("@Opacity"))
                return layer.Properties["@Opacity"];

            return 1f;
        }

        public static float GetOpacity(this Tile tile)
        {
            if (tile.Properties.ContainsKey("@Opacity"))
                return tile.Properties["@Opacity"];

            return 1f;
        }

        public static void SetOpacity(this Layer layer, float opacity)
        {
            layer.Properties["@Opacity"] = opacity;
        }

        public static void SetOpacity(this Tile tile, float opacity)
        {
            tile.Properties["@Opacity"] = opacity;
        }

        public static TMXColor GetColor(this Layer layer)
        {
            if (layer.Properties.ContainsKey("@Color"))
                return TMXColor.FromString(layer.Properties["@Color"]);

            return null;
        }       

        public static TMXColor GetColor(this Tile tile)
        {
            if (tile.Properties.ContainsKey("@Color"))
                return TMXColor.FromString(tile.Properties["@Color"]);

            return null;
        }

        public static TMXColor GetColor(this Map map)
        {
            if (map.Properties.ContainsKey("@Color"))
                return TMXColor.FromString(map.Properties["@Color"]);

            return null;
        }

        public static void SetColor(this Layer layer, TMXColor color)
        {
            layer.Properties["@Color"] = color.ToString();
        }

        public static void SetColor(this Tile tile, TMXColor color)
        {
            tile.Properties["@Color"] = color.ToString();
        }

        public static void SetColor(this Map map, TMXColor color)
        {
            map.Properties["@Color"] = color.ToString();
        }

        public static float GetRotation(this Tile tile)
        {
            if (tile.Properties.ContainsKey("@Rotation"))
                return tile.Properties["@Rotation"];

            return 0f;
        }

        public static void SetRotation(this Tile tile, float rotation)
        {
            tile.Properties["@Rotation"] = rotation;
        }

        public static int GetFlip(this Tile tile)
        {
            if (tile.Properties.ContainsKey("@Flip"))
                return tile.Properties["@Flip"];

            return 0;
        }

        public static void SetFlip(this Tile tile, int flip)
        {
            tile.Properties["@Flip"] = flip;
        }


        public static TMXColor GetBackgroundColor(this Map map)
        {
            if (map.Properties.ContainsKey("@BackgroundColor"))
                return TMXColor.FromString(map.Properties["@BackgroundColor"]);

            return null;
        }

        public static TMXColor SetBackgroundColor(this Map map, TMXColor color)
        {
            map.Properties["@BackgroundColor"] = color.ToString();

            return null;
        }

        public static DrawInstructions GetDrawInstructions(this Tile tile)
        {
            return new DrawInstructions()
            {
                Rotation = tile.GetRotation(),
                Effect = tile.GetFlip(),
                Offset = tile.Layer.GetOffset() + tile.GetOffset(),
                Color = tile.GetColor() ?? tile.Layer.GetColor() ?? tile.Layer.Map.GetColor(),
                Opacity = tile.Layer.GetOpacity() * tile.GetOpacity()
            };
        }

        public static void SetupImageLayer(this Layer layer)
        {
            if (!layer.IsImageLayer())
                return;

            if (xTile.Format.FormatManager.Instance?.GetMapFormatByExtension("tmx") is TMXFormat tmxFormat)
            {
                layer.AfterDraw -= tmxFormat.ImageLayer_AfterDraw;
                layer.AfterDraw += tmxFormat.ImageLayer_AfterDraw;
            }
        }
    }
}
