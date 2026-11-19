using UnityEngine;

namespace MiningTown
{
    public sealed partial class MiningTownPrototype
    {
        // The atlas lists each idle/walk pair in this order.
        private enum ExplorerFacing { South, North, East, West }

        private const int ExplorerFrameSize = 32;
        private const int ExplorerWalkFrames = 6;
        private const int ExplorerIdleFrames = 2;
        private static readonly string[] PlaceholderParts = { "Miner body", "Hard hat", "Hat lamp" };
        private readonly Sprite[,] explorerFrames = new Sprite[8, ExplorerWalkFrames];
        private SpriteRenderer explorerRenderer;
        private Vector3 previousPlayerPosition;
        private ExplorerFacing explorerDirection;
        private bool explorerWalking;
        private float explorerAnimationTime;

        private void ApplyExplorerArt()
        {
            Texture2D atlas = Resources.Load<Texture2D>("MiningTownPlayer/Explorer");
            if (atlas == null || atlas.width < 192 || atlas.height < 256) return;

            for (int row = 0; row < explorerFrames.GetLength(0); row++)
            {
                int frameCount = row % 2 == 0 ? ExplorerIdleFrames : ExplorerWalkFrames;
                for (int frame = 0; frame < frameCount; frame++)
                {
                    var rect = new Rect(frame * ExplorerFrameSize, atlas.height - (row + 1) * ExplorerFrameSize,
                        ExplorerFrameSize, ExplorerFrameSize);
                    explorerFrames[row, frame] = Sprite.Create(atlas, rect, new Vector2(0.5f, 0.03f),
                        19f, 0, SpriteMeshType.FullRect);
                }
            }

            GameObject visual = Group("Explorer sprite", player);
            visual.transform.localPosition = new Vector3(0, -0.96f, 0);
            visual.transform.rotation = sceneCamera.transform.rotation;
            explorerRenderer = visual.AddComponent<SpriteRenderer>();
            explorerRenderer.sprite = explorerFrames[0, 0];
            previousPlayerPosition = player.position;

            foreach (string part in PlaceholderParts)
            {
                Transform placeholder = player.Find(part);
                if (placeholder != null) placeholder.gameObject.SetActive(false);
            }
        }

        private void UpdateExplorerArt()
        {
            if (explorerRenderer == null) return;

            Vector3 displacement = player.position - previousPlayerPosition;
            previousPlayerPosition = player.position;
            float distanceSquared = displacement.sqrMagnitude;
            bool walking = distanceSquared > 0.000001f && distanceSquared < 2.25f;
            if (walking != explorerWalking)
            {
                explorerAnimationTime = 0f;
                explorerWalking = walking;
            }
            explorerAnimationTime += Time.deltaTime;

            if (walking)
            {
                // Treat nearly equal axes as horizontal so diagonal steps keep one facing.
                bool horizontal = Mathf.Abs(displacement.x) >= Mathf.Abs(displacement.z) * 0.9f;
                explorerDirection = horizontal
                    ? (displacement.x < 0f ? ExplorerFacing.West : ExplorerFacing.East)
                    : (displacement.z < 0f ? ExplorerFacing.South : ExplorerFacing.North);
            }

            int row = (int)explorerDirection * 2 + (walking ? 1 : 0);
            int frameCount = walking ? ExplorerWalkFrames : ExplorerIdleFrames;
            int frame = Mathf.FloorToInt(explorerAnimationTime * (walking ? 10f : 2f)) % frameCount;
            explorerRenderer.sprite = explorerFrames[row, frame];
        }

        private void DestroyExplorerFrames()
        {
            foreach (Sprite frame in explorerFrames)
                if (frame != null) Destroy(frame);
        }
    }
}
