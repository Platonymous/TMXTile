using System;
using System.Linq;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace TMXTile
{
    public enum TileFlip
    {
        FLIPPED_HORIZONTALLY,
        FLIPPED_VERTICALLY,
        FLIPPED_DIAGONALLY
    }

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

        public static TileSheet GetTileSheetForImageLayer(this Layer layer)
        {
            if (!layer.IsImageLayer())
                return null;

            return layer.Map.TileSheets.FirstOrDefault(ts => ts.Id == "zImageSheet_" + layer.Id);
        }

        public static bool IsFlipped(this Tile tile, TileFlip direction)
        {
            switch (direction)
            {
                case (TileFlip.FLIPPED_HORIZONTALLY):
                    return tile.Properties.ContainsKey("FLIPPED_HORIZONTALLY") && tile.Properties["FLIPPED_HORIZONTALLY"] == true;
                case (TileFlip.FLIPPED_VERTICALLY):
                    return tile.Properties.ContainsKey("FLIPPED_VERTICALLY") && tile.Properties["FLIPPED_VERTICALLY"] == true;
                case (TileFlip.FLIPPED_DIAGONALLY):
                    return tile.Properties.ContainsKey("FLIPPED_DIAGONALLY") && tile.Properties["FLIPPED_DIAGONALLY"] == true;
                default:
                    return false;
            }
        }

        public static Location GetOffset(this Layer layer)
        {
            return new Location(
                layer.Properties.ContainsKey("@OffsetX") ? (int) layer.Properties["@OffsetX"] : 0,
                layer.Properties.ContainsKey("@OffsetY") ? (int) layer.Properties["@OffsetY"] : 0
                );
        }

        public static void SetOffset(this Layer layer, Location offset)
        {
            layer.Properties["@OffsetX"] = offset.X;
            layer.Properties["@OffsetY"] = offset.Y;
        }

        public static float GetOpacity(this Layer layer)
        {
            if(layer.Properties.ContainsKey("@Opacity"))
                return layer.Properties["@Opacity"];

            return 1f;
        }

        public static void SetOpacity(this Layer layer, float opacity)
        {
            layer.Properties["@Opacity"] = opacity;
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
            bool horizontal = tile.IsFlipped(TileFlip.FLIPPED_HORIZONTALLY),
                    vertical = tile.IsFlipped(TileFlip.FLIPPED_VERTICALLY),
                    diagonal = tile.IsFlipped(TileFlip.FLIPPED_DIAGONALLY);

            TMXColor color = tile.GetColor();
            if (color == null)
                color = tile.Layer.GetColor();
            if (color == null)
                color = tile.Layer.Map.GetColor();


            return new DrawInstructions()
            {
                Rotation = GetRotationForFlippedTile(horizontal, vertical, diagonal),
                Effect = GetEffectForFlippedTile(horizontal, vertical, diagonal),
                Offset = GetOffsetForFlippedTile(horizontal, vertical, diagonal, tile),
                Color = color,
                Opacity = tile.Layer.GetOpacity()
            };
        }

        private static int GetEffectForFlippedTile(bool horizontal, bool vertical, bool diagonal)
        {
            if (!horizontal && !vertical && !diagonal)
                return 0;

            int effects = 0;

            if ((diagonal && vertical == horizontal) || (!diagonal && vertical && !horizontal))
                effects = 2;
            else if (horizontal)
                effects = 1;

            return effects;
        }
        
        private static float GetRotationForFlippedTile(bool horizontal, bool vertical, bool diagonal)
        {
            if (!horizontal && !vertical && !diagonal)
                return 0f;

            float rotation = 0f;

            if (diagonal && !vertical)
                rotation += (float)Math.PI / 2;
            else if(diagonal)
                rotation -= (float)Math.PI / 2;
            else if (vertical && horizontal)
                    rotation += (float)Math.PI;

            return rotation;
        }

        private static Location GetOffsetForFlippedTile(bool horizontal, bool vertical, bool diagonal, Tile tile = null)
        {

            if (!horizontal && !vertical && !diagonal)
                return Location.Origin;

            Location offset = Location.Origin;

            if (diagonal && !vertical)
                offset.X = 1;
            else if (diagonal)
                offset.Y = 1;
            else if (vertical && horizontal)
            {
                offset.X = 1;
                offset.Y = 1;
            }

            if(tile != null)
            {
                xTile.Dimensions.Size tileSize = tile.Layer.TileSize;
                offset.X *= tileSize.Width;
                offset.Y *= tileSize.Height;
            }

            return offset;
        }
    }
}
