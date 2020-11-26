using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Xml;
using xTile;
using xTile.Dimensions;
using xTile.Display;
using xTile.Format;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace TMXTile
{
    public class TMXFormat : IMapFormat
    {
        const UInt32 FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        const UInt32 FLIPPED_VERTICALLY_FLAG = 0x40000000;
        const UInt32 FLIPPED_DIAGONALLY_FLAG = 0x20000000;

        public string Name => "Tiled XML Format";

        public string FileExtensionDescriptor => "Tiled XML Map Files (*.tmx) **";

        public string FileExtension => "tmx";

        public Size FixedTileSize { get; set; }
        public Size FixedTileSizeMultiplied { get; set; }

        public Size TileSizeMultiplier { get; set; } = new Size(1, 1);

        public Action<Layer, Rectangle> DrawImageLayer { get; set; }

        public TMXFormat(int tileWidth, int tileHeight)
            : this(new Size(tileWidth, tileHeight))
        {

        }

        public TMXFormat(int tileWidth, int tileHeight, Action<Layer, Rectangle> drawImageLayer)
            : this(tileWidth,tileHeight)
        {
            DrawImageLayer = drawImageLayer;
        }


        public TMXFormat(int tileWidth, int tileHeight, int tileSizeMultiplierX, int tileSizeMultiplierY)
            : this(new Size(tileWidth, tileHeight), new Size(tileSizeMultiplierX, tileSizeMultiplierY))
        {

        }

        public TMXFormat(int tileWidth, int tileHeight, int tileSizeMultiplierX, int tileSizeMultiplierY, Action<Layer, Rectangle> drawImageLayer)
            : this(tileWidth,tileHeight, tileSizeMultiplierX, tileSizeMultiplierY)
        {
            DrawImageLayer = drawImageLayer;
        }

        public TMXFormat(Size fixedTilesize)
        {
            FixedTileSize = fixedTilesize;
            TileSizeMultiplier = new Size(1, 1);
        }

        public TMXFormat(Size fixedTilesize, Size tileSizeMultiplier)
        {
            FixedTileSize = fixedTilesize;
            FixedTileSizeMultiplied = new Size(fixedTilesize.Width * tileSizeMultiplier.Width, fixedTilesize.Height * tileSizeMultiplier.Height);
            TileSizeMultiplier = tileSizeMultiplier;
        }

        public CompatibilityReport DetermineCompatibility(Map map)
        {
            List<CompatibilityNote> compatibilityNoteList = new List<CompatibilityNote>();
            foreach (TileSheet tileSheet in map.TileSheets)
            {
                Size size = tileSheet.Margin;
                if (!size.Square)
                    compatibilityNoteList.Add(new CompatibilityNote(CompatibilityLevel.None, string.Format("Tilesheet {0}: Margin values ({1}) are not equal.", tileSheet.Id, tileSheet.Margin)));
                size = tileSheet.Spacing;
                if (!size.Square)
                    compatibilityNoteList.Add(new CompatibilityNote(CompatibilityLevel.None, string.Format("Tilesheet {0}: Spacing values ({1}) are not equal.", tileSheet.Id, tileSheet.Spacing)));
            }
            if (map.Layers.Count > 0)
            {
                Layer layer1 = map.Layers[0];
                bool flag1 = false;
                bool flag2 = false;
                bool flag3 = false;
                bool flag4 = false;
                foreach (Layer layer2 in map.Layers)
                {
                    if (layer2 != layer1)
                    {
                        if (layer2.LayerWidth != layer1.LayerWidth)
                            flag1 = true;
                        if (layer2.LayerHeight != layer1.LayerHeight)
                            flag2 = true;
                        if (layer2.TileWidth != layer1.TileWidth)
                            flag3 = true;
                        if (layer2.TileHeight != layer1.TileHeight)
                            flag4 = true;
                    }
                }
                if (flag1)
                    compatibilityNoteList.Add(new CompatibilityNote(CompatibilityLevel.None, "Layer widths do not match across all layers."));
                if (flag2)
                    compatibilityNoteList.Add(new CompatibilityNote(CompatibilityLevel.None, "Layer heights do not match across all layers."));
                if (flag3)
                    compatibilityNoteList.Add(new CompatibilityNote(CompatibilityLevel.None, "Tile widths do not match across all layers."));
                if (flag4)
                    compatibilityNoteList.Add(new CompatibilityNote(CompatibilityLevel.None, "Tile heights do not match across all layers."));
            }
            return new CompatibilityReport(compatibilityNoteList);
        }
        public Map Load(Stream stream)
        {
            TMXParser parser = new TMXParser();
            TMXMap tmxMap = parser.Parse(stream);
            return Load(tmxMap);
        }

        public Map Load(string path)
        {
            TMXParser parser = new TMXParser();
            TMXMap tmxMap = parser.Parse(path);
            return Load(tmxMap);
        }

        public Map Load(XmlReader reader)
        {
            TMXParser parser = new TMXParser();
            TMXMap tmxMap = parser.Parse(reader);
            return Load(tmxMap);
        }
        
        public Map Load(TMXMap tmxMap)
        {
            Map map = new Map();
            if (tmxMap.Orientation != "orthogonal")
                throw new Exception("Only orthogonal Tiled maps are supported.");
            TMXProperty[] properties = GetOrdered(tmxMap.Properties);
            if (properties != null)
                foreach (var prop in properties)
                    if (prop.Name == "@Description")
                        map.Description = prop.StringValue;
                    else
                        map.Properties[prop.Name] = GetPropertyValue(prop);

            if (tmxMap.Backgroundcolor is TMXColor bg)
                map.Properties["@BackgroundColor"] = bg.ToString();

            LoadTileSets(tmxMap, ref map);
            LoadLayers(tmxMap, ref map);
            LoadImageLayers(tmxMap, ref map);
            LoadObjects(tmxMap, ref map);

            return map;
        }

        private TMXProperty[] GetOrdered(IEnumerable<TMXProperty> props)
        {
            return props?.OrderBy(p => p?.Name[0] is char f && f.ToString().Equals(f.ToString().ToLower()) ? "B_" + p?.Name : "A_" + p?.Name).ToArray();
        }

        public void LoadTileSets(TMXMap tmxMap, ref Map map)
        {
            foreach (var tileSet in tmxMap.Tilesets)
            {
                Size sheetSize = new Size(tileSet.Image.Width / tileSet.Tilewidth, tileSet.Image.Height / tileSet.Tileheight);
                TileSheet tileSheet = new TileSheet(tileSet.Name, map, tileSet.Image.Source, sheetSize, FixedTileSizeMultiplied)
                {
                    Spacing = new Size(0),
                    Margin = new Size(0)
                };


                tileSheet.Properties["@FirstGid"] = (int)tileSet.Firstgid;
                tileSheet.Properties["@LastGid"] = (int)tileSet.Firstgid + tileSet.Tilecount - 1;
                if (tileSet.Tiles != null)
                    foreach (var tile in tileSet.Tiles.Where(t => t.Properties != null))
                        foreach (var prop in tile.Properties)
                            tileSheet.Properties[string.Format("@TileIndex@{0}@{1}", tile.Id, prop.Name)] = GetPropertyValue(prop);

                if (tileSet.Properties != null)
                    foreach (TMXProperty prop in tileSet.Properties)
                        tileSheet.Properties[prop.Name] = GetPropertyValue(prop);

                map.AddTileSheet(tileSheet);
            }
        }

        public void LoadLayers(TMXMap tmxMap, ref Map map)
        {
            if (tmxMap.Layers == null)
                return;
            foreach (TMXLayer layer in tmxMap.Layers)
            {
                Layer layer1 = new Layer(layer.Name, map, new Size((int)layer.Width, (int)layer.Height), FixedTileSizeMultiplied);
                Layer mapLayer = layer1;

                if (layer.Properties != null)
                    foreach (TMXProperty prop in layer.Properties)
                        if (prop.Name == "@Description")
                            mapLayer.Description = prop.StringValue;
                        else
                            mapLayer.Properties[prop.Name] = GetPropertyValue(prop);

                if (layer.Data != null && layer.Data.Chunks != null && layer.Data.Chunks.Count > 0)
                    foreach (TMXChunk c in layer.Data.Chunks)
                    {
                        Location origin = new Location(c.X, c.Y);
                        foreach (TMXTile t in c.Tiles)
                        {
                            mapLayer.Tiles[origin] = LoadTile(mapLayer, tmxMap, t.Gid);
                            ++origin.X;
                            if (origin.X >= mapLayer.LayerWidth)
                            {
                                origin.X = 0;
                                ++origin.Y;
                            }
                        }
                    }
                else if (layer.Data != null && layer.Data.Tiles != null && layer.Data.Tiles.Count > 0)
                {
                    Location origin = Location.Origin;
                    foreach (TMXTile t in layer.Data.Tiles)
                    {
                        mapLayer.Tiles[origin] = LoadTile(mapLayer, tmxMap, t.Gid);
                        ++origin.X;
                        if (origin.X >= mapLayer.LayerWidth)
                        {
                            origin.X = 0;
                            ++origin.Y;
                        }
                    }
                }

                mapLayer.SetOffset(new Location((int)Math.Floor(layer.Offsetx * TileSizeMultiplier.Width), (int)Math.Floor(layer.Offsety * TileSizeMultiplier.Height)));
                mapLayer.SetOpacity(layer.Opacity);
                map.AddLayer(mapLayer);
            }
        }

        public void LoadImageLayers(TMXMap tmxMap, ref Map map)
        {
            if (tmxMap.ImageLayers == null || tmxMap.ImageLayers.Count == 0)
                return;

            foreach (TMXImageLayer layer in tmxMap.ImageLayers)
            {
                Size imageSize = new Size(layer.Image.Width, layer.Image.Height);
                TileSheet imagesheet = new TileSheet("zImageSheet_" + layer.Name, map, layer.Image.Source, new Size(1,1), imageSize);
                map.AddTileSheet(imagesheet);
                Layer imageLayer = new Layer(layer.Name, map, map.Layers[0].LayerSize, FixedTileSizeMultiplied);

                if (layer.Image.TransparentColor is TMXColor tcolor)
                    imagesheet.Properties["@TColor"] = tcolor.ToString();

                if (layer.Properties != null)
                    foreach (TMXProperty prop in layer.Properties)
                        if (prop.Name == "@Description")
                            imageLayer.Description = prop.StringValue;
                        else
                            imageLayer.Properties[prop.Name] = GetPropertyValue(prop);

                imageLayer.SetOffset(new Location((int)Math.Floor(layer.Offsetx * TileSizeMultiplier.Width), (int)Math.Floor(layer.Offsety * TileSizeMultiplier.Height)));
                imageLayer.SetOpacity(layer.Opacity);
                imageLayer.MakeImageLayer();
                imageLayer.SetTileSheetForImageLayer(imagesheet);
                imageLayer.AfterDraw += ImageLayer_AfterDraw;

                map.AddLayer(imageLayer);
            }
        }

        public static Location GlobalToLocal(xTile.Dimensions.Rectangle viewport, Location globalPosition)
        {
            return new Location(globalPosition.X - viewport.X, globalPosition.Y - viewport.Y);
        }

        public void ImageLayer_AfterDraw(object sender, LayerEventArgs layerEventArgs)
        {
            DrawImageLayer?.Invoke(layerEventArgs.Layer, layerEventArgs.Viewport);
        }

        internal void LoadObjects(TMXMap tmxMap, ref Map map)
        {
            if (tmxMap.Objectgroups == null || tmxMap.Objectgroups.Count() == 0)
                return;

            foreach (TMXObjectgroup objectGroup in tmxMap.Objectgroups)
                if (map.GetLayer(objectGroup.Name) is Layer layer)
                    foreach (TMXObject tiledObject in objectGroup.Objects)
                        if (tiledObject.Name == "TileData")
                        {
                            int tileX = (int)tiledObject.X / FixedTileSize.Width;
                            int tileWidth = (int)tiledObject.Width / FixedTileSize.Height;
                            int tileY = (int)tiledObject.Y / FixedTileSize.Width;
                            int tileHeight = (int)tiledObject.Height / FixedTileSize.Height;

                            for (int x = tileX; x < tileX + tileWidth; x++)
                                for (int y = tileY; y < tileY + tileHeight; y++)
                                    if (layer.Tiles[x, y] is Tile tile)
                                        foreach (TMXProperty prop in tiledObject.Properties)
                                            if (!tile.Properties.ContainsKey(prop.Name))
                                                tile.Properties.Add(prop.Name, GetPropertyValue(prop));
                                            else
                                                tile.Properties[prop.Name] = GetPropertyValue(prop);
                        }
        }



        internal Tile LoadTile(Layer layer, TMXMap tmxMap, UInt32 gid)
        {
            if (gid == 0)
                return null;
            TileSheet selectedTileSheet = null;

            int tileIndex = -1;
            uint g = gid;
            bool flipped_horizontally = false;
            bool flipped_vertically = false;
            bool flipped_diagonally = false;

            if (g >= FLIPPED_HORIZONTALLY_FLAG)
            {
                flipped_horizontally = true;
                g -= FLIPPED_HORIZONTALLY_FLAG;
            }

            if(g >= FLIPPED_VERTICALLY_FLAG)
            {
                flipped_vertically = true;
                g -= FLIPPED_VERTICALLY_FLAG;
            }

            if(g >= FLIPPED_DIAGONALLY_FLAG)
                flipped_diagonally = true;
           
            gid &= ~(FLIPPED_HORIZONTALLY_FLAG |
                        FLIPPED_VERTICALLY_FLAG |
                        FLIPPED_DIAGONALLY_FLAG);

            foreach (TileSheet tileSheet in layer.Map.TileSheets)
            {
                UInt32 property1 = (UInt32)tileSheet.Properties["@FirstGid"];
                UInt32 property2 = (UInt32)tileSheet.Properties["@LastGid"];
                if (gid >= property1 && gid <= property2)
                {
                    selectedTileSheet = tileSheet;
                    tileIndex = (int)(gid - property1);
                    break;
                }
            }
            if (selectedTileSheet == null)
                throw new Exception(string.Format("Invalid tile gid: {0}", gid));

            Tile result = null;

            if (tmxMap.Tilesets.Where(ts => ts.Name == selectedTileSheet.Id).FirstOrDefault() is TMXTileset tileset && tileset.Tiles.Where(t => t.Id == tileIndex).FirstOrDefault() is TMXTileSetTile tile && tile.Animations != null && tile.Animations.Count() > 0)
            {
                StaticTile[] array = tile.Animations.Select(frame => new StaticTile(layer, selectedTileSheet, BlendMode.Alpha, (int)frame.TileId)).ToArray();
                result = new AnimatedTile(layer, array, tile.Animations[0].Duration);
            }
            else
                result = new StaticTile(layer, selectedTileSheet, BlendMode.Alpha, tileIndex);

            if (result.GetRotationValue() == 0)
                result.SetRotationValue(GetRotationForFlippedTile(flipped_horizontally, flipped_vertically, flipped_diagonally));

            if (result.GetFlip() == 0)
                result.SetFlip(GetEffectForFlippedTile(flipped_horizontally, flipped_vertically, flipped_diagonally));

                return result;
        }

        private static int GetEffectForFlippedTile(bool horizontal, bool vertical, bool diagonal)
        {
            if (diagonal)
            {
                if (!vertical && !horizontal)
                    return 2;
                else if (horizontal && vertical)
                    return 2;
                if (horizontal)
                    return 0;//!
                else if (vertical)
                    return 0;//!
            }
                

            if (horizontal && vertical)
                return 0;
            else if (horizontal)
                return 1;
            else if (vertical)
                return 2;

            return 0;
        }

        private static int GetRotationForFlippedTile(bool horizontal, bool vertical, bool diagonal)
        {
            if (diagonal)
            {
                if (!vertical && !horizontal)
                    return 90;
                else if (horizontal && vertical)
                    return -90;
                else if (horizontal)
                    return 90;
                else if (vertical)
                    return -90;
            }            

            if (horizontal && vertical)
                return 180;

            return 0;
        }

        public void Store(Map map, Stream stream)
        {
            var parser = new TMXParser();
            var tmxMap = Store(map);
            parser.Export(tmxMap, stream);
        }

        public string StoreAsString(Map map, DataEncodingType encoding = DataEncodingType.XML)
        {
            var parser = new TMXParser();
            var tmxMap = Store(map);
           
            string result = "";
            using (StringWriter stringWriter = new StringWriter())
            using (XmlWriter writer = XmlWriter.Create(stringWriter, new XmlWriterSettings
            {
                Encoding = System.Text.Encoding.GetEncoding("UTF-8"),
                Indent = true
            }))
            {
                parser.Export(tmxMap, writer, encoding);
                writer.Close();
                result = stringWriter.ToString();
            }
            return AdjustToTiledFormat(result);
        }

        private string AdjustToTiledFormat(string data)
        {
            data = data
                .Replace("utf-16", "UTF-8")
                .Replace("\" />", "\"/>")
                .Replace("type=\"string\" value", "value")
                .Replace(" visible=\"false\" locked=\"false\"", "")
                .Replace(" opacity=\"1\"", "")
                .Replace(" offsetx=\"0\"", "")
                .Replace(" offsety=\"0\"", "")
                .Replace(" rotation=\"0\"", "")
                .Replace("<properties />", "")
                .Replace("  ", " ")
                .Replace(@">
  
  <data ", @">
  <data ");

            return data;
        }

        public void Store(Map map, string path, DataEncodingType encoding = DataEncodingType.XML)
        {
            var parser = new TMXParser();
            var tmxMap = Store(map);
            parser.Export(tmxMap, path, encoding);
        }

        public void Store(Map map, XmlWriter writer, DataEncodingType encoding = DataEncodingType.XML)
        {
            var parser = new TMXParser();
            var tmxMap = Store(map);
            parser.Export(tmxMap, writer, encoding);
        }

        public void Store(Map map, Stream stream, DataEncodingType encoding)
        {
            var parser = new TMXParser();
            var tmxMap = Store(map);
            parser.Export(tmxMap, stream, encoding);
        }

        public TMXMap Store(Map map)
        {
            TMXMap tiledMap1 = new TMXMap();
            tiledMap1.Version = "1.4";
            tiledMap1.Tiledversion = "1.4.2";
            tiledMap1.Compressionlevel = 0;
            tiledMap1.Orientation = "orthogonal";
            tiledMap1.Renderorder = "right-down";

            tiledMap1.Width = map.DisplayWidth / FixedTileSizeMultiplied.Width;
            tiledMap1.Height = map.DisplayHeight / FixedTileSizeMultiplied.Height;
            tiledMap1.Tilewidth = FixedTileSize.Width;
            tiledMap1.Tileheight = FixedTileSize.Height;

            List<TMXProperty> properties = new List<TMXProperty>();
            if (map.Description.Length > 0)
                map.Properties["@Description"] = map.Description;

            tiledMap1.Backgroundcolor = map.GetBackgroundColor();

            foreach (var prop in map.Properties)
                properties.Add(new TMXProperty() { Name = prop.Key, StringValue = prop.Value.ToString(), Type = GetPropertyType(prop.Value) });

            tiledMap1.Properties = GetOrdered(properties);
            tiledMap1.Tilesets = new List<TMXTileset>();



            tiledMap1.Layers = new List<TMXLayer>();
            tiledMap1.ImageLayers = new List<TMXImageLayer>();
            tiledMap1.Objectgroups = new List<TMXObjectgroup>();

            StoreTileSets(map, ref tiledMap1);
            StoreLayers(map, ref tiledMap1);
            StoreObjects(map, ref tiledMap1);

            tiledMap1.Tilesets.ForEach(ts => ts.Tiles = ts.Tiles.OrderBy(t => t.Id).ToList());

            return tiledMap1;
        }

        public void StoreTileSets(Map map, ref TMXMap tilemap)
        {
            int num = 1;
            foreach (TileSheet tileSheet in map.TileSheets)
            {
                if (tileSheet.Id.StartsWith("zImageSheet_"))
                    continue;

                TMXTileset tiledTileSet1 = new TMXTileset();
                tiledTileSet1.Firstgid = (uint) num;
                tiledTileSet1.Name = tileSheet.Id;
                tiledTileSet1.Tilewidth = FixedTileSize.Width;
                tiledTileSet1.Tileheight = FixedTileSize.Height;
                tiledTileSet1.Tilecount = tileSheet.TileCount;
                tiledTileSet1.Columns = tileSheet.SheetWidth;
                tiledTileSet1.Image = new TMXImage()
                {
                    Source = tileSheet.ImageSource,
                    Width = tileSheet.SheetWidth * FixedTileSize.Width,
                    Height = tileSheet.SheetHeight * FixedTileSize.Height,
                };
                tiledTileSet1.Tiles = new List<TMXTileSetTile>();

                for(int i = 0; i < tileSheet.TileCount; i++)
                {
                    foreach (KeyValuePair<string, PropertyValue> property in tileSheet.TileIndexProperties[i])
                    {
                        var tmxProp = new TMXProperty() { Name = property.Key, StringValue = property.Value.ToString(), Type = GetPropertyType(property.Value) };
                        var tile = tiledTileSet1.Tiles.FirstOrDefault(tiledTile => tiledTile.Id == i);
                        if (tile == null)
                        {
                            tile = new TMXTileSetTile() { Id = i, Properties = new TMXProperty[0] };
                            tiledTileSet1.Tiles.Add(tile);
                        }
                        if (tile.Properties == null)
                            tile.Properties = new TMXProperty[] { tmxProp };
                        else
                        {
                            var propList = new List<TMXProperty>();
                            propList.AddRange(tile.Properties);
                            propList.Add(tmxProp);
                            tile.Properties = GetOrdered(propList);
                        }
                    }
                }
                tilemap.Tilesets.Add(tiledTileSet1);
                num += tileSheet.TileCount;
            }
        }

        public void StoreLayers(Map map, ref TMXMap tiledMap)
        {
            int l = 0;
            foreach (Layer layer in map.Layers)
            {
                l++;

                if (layer.IsImageLayer())
                {
                    TMXImageLayer imageLayer = new TMXImageLayer();
                    imageLayer.Name = layer.Id;
                    imageLayer.Id = l;
                    var imageProps = new List<TMXProperty>();
                    foreach (var prop in layer.Properties)
                    {
                        if (prop.Key == "@OffsetX")
                            imageLayer.Offsetx = prop.Value;
                        else if (prop.Key == "@OffsetY")
                            imageLayer.Offsety = prop.Value;
                        else if (prop.Key == "@Opacity")
                            imageLayer.Opacity = prop.Value;
                        else
                            imageProps.Add(new TMXProperty() { Name = prop.Key, StringValue = prop.Value.ToString(), Type = GetPropertyType(prop.Value) });
                    }
                    imageLayer.Image = new TMXImage();
                    var imageTs = layer.GetTileSheetForImageLayer();
                    if (imageTs == null)
                        continue;

                    imageLayer.Image.Height = imageTs.TileHeight * FixedTileSize.Height;
                    imageLayer.Image.Width = imageTs.TileWidth * FixedTileSize.Width;
                    imageLayer.Image.Source = imageTs.ImageSource;

                    imageLayer.Properties = GetOrdered(imageProps);
                    tiledMap.ImageLayers.Add(imageLayer);
                    continue;
                }

                TMXLayer tiledLayer1 = new TMXLayer();
                tiledLayer1.Name = layer.Id;
                tiledLayer1.Id = l;
                tiledLayer1.Width = layer.LayerWidth;
                tiledLayer1.Height = layer.LayerHeight;
                tiledLayer1.Data = new TMXData();
            var props = new List<TMXProperty>();
                foreach (var prop in layer.Properties) {
                    if (prop.Key == "@OffsetX")
                        tiledLayer1.Offsetx = prop.Value;
                    else if (prop.Key == "@OffsetY")
                        tiledLayer1.Offsety = prop.Value;
                    else if (prop.Key == "@Opacity")
                        tiledLayer1.Opacity = prop.Value;
                    else
                        props.Add(new TMXProperty() { Name = prop.Key, StringValue = prop.Value.ToString(), Type = GetPropertyType(prop.Value) });
                }
                if (layer.Description.Length > 0)
                    props.Add(new TMXProperty() { Name = "@Description", StringValue = layer.Description, Type = "string" });

                List<int> intList = new List<int>();
                for (int index1 = 0; index1 < layer.LayerHeight; ++index1)
                    for (int index2 = 0; index2 < layer.LayerWidth; ++index2)
                    {
                        Tile tile = layer.Tiles[index2, index1];
                        if (tile is AnimatedTile animatedTile)
                            foreach (TMXTileset tileSet in tiledMap.Tilesets.Where(t => t.Name == animatedTile.TileSheet.Id))
                            {
                                TMXTileSetTile tiledTile1 = tileSet.Tiles.FirstOrDefault(tiledTile => tiledTile.Id == tile.TileIndex);
                                if (tiledTile1 == null)
                                    tileSet.Tiles.Add(new TMXTileSetTile()
                                    {
                                        Id = tile.TileIndex,
                                        Animations = ((IEnumerable<StaticTile>)animatedTile.TileFrames).Select(frame => new TMXFrame()
                                        {
                                            TileId = (uint)frame.TileIndex,
                                            Duration = (uint)animatedTile.FrameInterval
                                        }).ToArray()
                                    });
                                else if (tiledTile1.Animations == null)
                                    tiledTile1.Animations = ((IEnumerable<StaticTile>)animatedTile.TileFrames).Select(frame => new TMXFrame()
                                    {
                                        TileId = (uint)frame.TileIndex,
                                        Duration = (uint)animatedTile.FrameInterval
                                    }).ToArray();
                        }
                        int num2 = 0;
                        if (tile != null)
                        {
                            int tileIndex = tile.TileIndex;
                            TMXTileset tiledTileSet = tiledMap.Tilesets.FirstOrDefault(tileSet => tileSet.Name == tile.TileSheet.Id);
                            int num3 = tiledTileSet != null ? (int) tiledTileSet.Firstgid : 1;
                            num2 = tileIndex + num3;
                        }
                        intList.Add(num2);
                    }

                foreach (int gid in intList)
                    tiledLayer1.Data.Tiles.Add(new TMXTile() { Gid = (uint)gid });

                tiledLayer1.Properties = GetOrdered(props);
                tiledMap.Layers.Add(tiledLayer1);
            }
        }

        internal void StoreObjects(Map map, ref TMXMap tiledMap)
        {
            int l = map.Layers.Count();
            ++tiledMap.Nextobjectid;
            foreach (Layer layer in map.Layers)
            {
                l++;
                TMXObjectgroup tiledObjectGroup = new TMXObjectgroup()
                {
                    Name = layer.Id,
                    Id = l,
                    Objects = new List<TMXObject>()
                };
                for (int index1 = 0; index1 < layer.LayerHeight; ++index1)
                    for (int index2 = 0; index2 < layer.LayerWidth; ++index2)
                    {
                        Tile tile = layer.Tiles[index2, index1];
                        if ((tile != null ? tile.Properties : null) != null && tile.Properties.Any<KeyValuePair<string, PropertyValue>>())
                        {
                            TMXObject tiledObject = new TMXObject()
                            {
                                Id = tiledMap.Nextobjectid,
                                Name = "TileData",
                                X = index2 * FixedTileSize.Width,
                                Y = index1 * FixedTileSize.Height,
                                Width = FixedTileSize.Width,
                                Height = FixedTileSize.Height
                            };
                            List<TMXProperty> props = new List<TMXProperty>();
                            foreach (KeyValuePair<string, PropertyValue> property in tile.Properties)
                                props.Add(new TMXProperty() { Name = property.Key, StringValue = property.Value.ToString(), Type = GetPropertyType(property.Value) });
                            tiledObject.Properties = props.ToArray();
                            tiledObjectGroup.Objects.Add(tiledObject);
                            ++tiledMap.Nextobjectid;
                        }
                    }
                tiledMap.Objectgroups.Add(tiledObjectGroup);
            }
            tiledMap.Nextlayerid = (map.Layers.Count() * 2) + 1;
        }

        internal PropertyValue GetPropertyValue(TMXProperty prop)
        {
            if (prop.Type == null)
                return prop.StringValue;

            switch (prop.Type.ToLower())
            {
                case "int": return prop.IntValue;
                case "string": return prop.StringValue;
                case "bool": return prop.BoolValue;
                case "color": return prop.ColorValue.ToString();
                case "float": return prop.FloatValue;
                default: return prop.StringValue;
            }
        }

        internal string GetPropertyType(PropertyValue prop)
        {
            if (prop.Type == typeof(float))
                return "float";

            if (prop.Type == typeof(int))
                return "int";

            if (prop.Type == typeof(bool))
                return "bool";

            string s = prop.ToString();
            if (s.StartsWith("#") && s.Length == 7 && TMXColor.FromString(s).ToString() == s)
                return "color";

            return "string";
        }
    }

}
