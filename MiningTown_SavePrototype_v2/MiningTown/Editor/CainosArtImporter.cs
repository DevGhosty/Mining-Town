using System;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;

namespace MiningTown.Editor
{
    public static class CainosArtImporter
    {
        private const string OutputFolder = "Assets/MiningTown/Resources/MiningTownCainos";

        [MenuItem("Tools/Mining Town/Import Cainos Art")]
        private static void PickArchive()
        {
            string archive = EditorUtility.OpenFilePanel("Select the Cainos Top Down Basic ZIP", "", "zip");
            if (!string.IsNullOrEmpty(archive)) Import(archive);
        }

        public static void ImportFromCommandLine()
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-cainosArchive");
            if (index < 0 || index + 1 >= args.Length)
                throw new ArgumentException("Pass -cainosArchive followed by the Cainos ZIP path.");
            Import(args[index + 1]);
        }

        private static void Import(string archive)
        {
            Directory.CreateDirectory(OutputFolder);
            using (ZipArchive zip = ZipFile.OpenRead(archive))
            {
                WriteTile(zip, "Texture/TX Tileset Grass.png", new RectInt(0, 0, 96, 96), "Grass", false);
                WriteTile(zip, "Texture/TX Tileset Stone Ground.png", new RectInt(7, 7, 80, 80), "Stone", false);
                WriteTile(zip, "Texture/Extra/TX Props with Shadow.png", new RectInt(160, 150, 32, 42), "Barrel", true);
                WriteTile(zip, "Texture/Extra/TX Props with Shadow.png", new RectInt(159, 16, 34, 48), "Crates", true);
            }

            Debug.Log("Imported Cainos terrain and props. Enter Play Mode to see the art.");
        }

        private static void WriteTile(ZipArchive zip, string entry, RectInt crop, string name, bool sprite)
        {
            ZipArchiveEntry file = zip.GetEntry(entry);
            if (file == null) throw new InvalidDataException("Missing " + entry + " in the Cainos ZIP.");
            using (var data = new MemoryStream())
            using (Stream stream = file.Open())
            {
                stream.CopyTo(data);
                var source = new Texture2D(2, 2);
                var tile = new Texture2D(crop.width, crop.height, TextureFormat.RGBA32, false);
                try
                {
                    if (!source.LoadImage(data.ToArray()) || source.width < crop.xMax || source.height < crop.yMax)
                        throw new InvalidDataException("Unexpected texture layout in " + entry);
                    tile.SetPixels(source.GetPixels(crop.x, source.height - crop.yMax, crop.width, crop.height));
                    tile.Apply();
                    string path = OutputFolder + "/" + name + ".png";
                    File.WriteAllBytes(path, tile.EncodeToPNG());
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                    var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                    importer.textureType = sprite ? TextureImporterType.Sprite : TextureImporterType.Default;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 32;
                    importer.alphaIsTransparency = sprite;
                    importer.filterMode = FilterMode.Point;
                    importer.wrapMode = sprite ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
                    importer.mipmapEnabled = false;
                    importer.npotScale = TextureImporterNPOTScale.None;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(tile);
                    UnityEngine.Object.DestroyImmediate(source);
                }
            }
        }
    }
}
