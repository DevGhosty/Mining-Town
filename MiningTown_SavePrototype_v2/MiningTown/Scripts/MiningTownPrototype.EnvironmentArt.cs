using UnityEngine;

namespace MiningTown
{
    public sealed partial class MiningTownPrototype
    {
        private const string CainosArtPath = "MiningTownCainos/";

        private void ApplyCainosArt()
        {
            Shader textureShader = Shader.Find("Unlit/Texture");
            if (textureShader != null && UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline == null)
            {
                Texture2D grassTile = Resources.Load<Texture2D>(CainosArtPath + "Grass");
                Texture2D stoneTile = Resources.Load<Texture2D>(CainosArtPath + "Stone");
                ApplyTerrainTile(ground.GetComponent<MeshRenderer>(), textureShader, grassTile, 6f);
                foreach (MeshRenderer road in roads)
                    if (IsOriginalRoad(road.name)) ApplyTerrainTile(road, textureShader, stoneTile, 4f);
            }

            HouseView workshop = houses.Find(house => house.Id == "workshop");
            if (workshop == null) return;
            PlaceCainosProp("Barrel", workshop.Root, new Vector3(1.7f, 0.03f, -2.02f));
            PlaceCainosProp("Crates", workshop.Root, new Vector3(-1.78f, 0.03f, -2.02f));
        }

        private void ApplyTerrainTile(MeshRenderer renderer, Shader shader, Texture2D texture, float unitsPerTile)
        {
            if (texture == null) return;
            var material = new Material(shader);
            material.mainTexture = texture;
            Vector3 scale = renderer.transform.lossyScale;
            material.mainTextureScale = new Vector2(scale.x, scale.z) / unitsPerTile;
            materials.Add(material);
            renderer.sharedMaterial = material;
        }

        private void PlaceCainosProp(string asset, Transform parent, Vector3 position)
        {
            Sprite sprite = Resources.Load<Sprite>(CainosArtPath + asset);
            if (sprite == null) return;

            GameObject item = Group(asset, parent);
            item.transform.localPosition = position;
            item.transform.rotation = sceneCamera.transform.rotation;
            item.transform.position += item.transform.up * (sprite.bounds.size.y * 0.5f);
            item.AddComponent<SpriteRenderer>().sprite = sprite;
        }

        private void BuildWorkshopFront(Transform root, Transform damaged, Transform restored)
        {
            Material weathered = Colored(new Color(0.36f, 0.25f, 0.18f));
            Material trim = Colored(new Color(0.68f, 0.43f, 0.25f));
            for (int i = 0; i < 6; i++)
                Shape("Boardwalk plank", PrimitiveType.Cube, new Vector3(-1.18f + i * 0.47f, 0.17f, -1.87f),
                    new Vector3(0.44f, 0.09f, 0.86f), i % 2 == 0 ? weathered : timber, root);

            for (int side = -1; side <= 1; side += 2)
                Shape("Storefront corner post", PrimitiveType.Cube, new Vector3(side * 1.48f, 1.2f, -1.48f),
                    new Vector3(0.13f, 2.3f, 0.19f), weathered, root);

            Shape("Weathered storefront sign", PrimitiveType.Cube, new Vector3(0, 2.16f, -1.53f),
                new Vector3(1.92f, 0.37f, 0.14f), weathered, damaged);
            Shape("Restored storefront sign", PrimitiveType.Cube, new Vector3(0, 2.16f, -1.53f),
                new Vector3(1.92f, 0.37f, 0.14f), trim, restored);
            Shape("Restored sign inset", PrimitiveType.Cube, new Vector3(0, 2.16f, -1.62f),
                new Vector3(1.68f, 0.19f, 0.07f), cream, restored);
        }
    }
}
