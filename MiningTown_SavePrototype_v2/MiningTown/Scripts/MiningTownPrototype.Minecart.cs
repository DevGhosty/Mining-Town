using UnityEngine;

namespace MiningTown
{
    public sealed partial class MiningTownPrototype
    {
        private enum MinecartPhase { PickCart, TakeOrDouble, PickSack, Result }

        private static readonly string[] CartNames = { "Left cart", "Middle cart", "Right cart" };
        private static readonly string[] MinecartCues = { "CartReveal", "SackOpen", "PrizeWin", "PrizeEmpty" };
        private static readonly Color MineGold = new Color(0.97f, 0.74f, 0.37f);
        private static readonly Color MineTeal = new Color(0.43f, 0.84f, 0.72f);
        private readonly int[] cartPrizes = new int[3];
        private readonly AudioClip[] minecartClips = new AudioClip[MinecartCues.Length];

        private AudioSource mineMusic;
        private bool minecartClipsLoaded;
        private GUIStyle mineScoreStyle, mineChoiceStyle;
        private SaveData minecartSession;
        private MinecartPhase minecartPhase;
        private bool minecartOpen;
        private int revealedPrize, winningSack, roundScore, roundsPlayed, bestScore, doubleStreak, bestStreak;
        private string roundResult;
        private float revealedAt;

        private bool MinecartDemoComplete => minecartSession == saves.State && roundsPlayed > 0 &&
            saves.State.bridgeRepaired && saves.State.repairedBuildingIds.Contains("miner");

        private void OpenMinecart()
        {
            if (minecartSession != saves.State)
            {
                minecartSession = saves.State;
                roundsPlayed = bestScore = doubleStreak = bestStreak = 0;
                StartMinecartRound();
            }

            if (!minecartClipsLoaded)
            {
                for (int i = 0; i < minecartClips.Length; i++)
                    minecartClips[i] = Resources.Load<AudioClip>("MiningTownAudio/" + MinecartCues[i]);
                minecartClipsLoaded = true;
            }

            minecartOpen = true;
        }

        private void StartMinecartRound()
        {
            for (int i = 0; i < cartPrizes.Length; i++)
                cartPrizes[i] = i + 1;
            for (int i = cartPrizes.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (cartPrizes[i], cartPrizes[j]) = (cartPrizes[j], cartPrizes[i]);
            }
            minecartPhase = MinecartPhase.PickCart;
        }

        private void RevealCart(int index)
        {
            revealedPrize = cartPrizes[index];
            revealedAt = Time.unscaledTime;
            minecartPhase = MinecartPhase.TakeOrDouble;
            PlayMinecartCue(0);
        }

        private void FinishMinecartRound(int score, bool doubled)
        {
            roundScore = score;
            roundsPlayed++;
            bestScore = Mathf.Max(bestScore, score);
            doubleStreak = doubled ? doubleStreak + 1 : 0;
            bestStreak = Mathf.Max(bestStreak, doubleStreak);
            roundResult = doubled ? "DOUBLE PRIZE!" : score == 0 ? "The sack was empty" : "Prize kept";
            revealedAt = Time.unscaledTime;
            minecartPhase = MinecartPhase.Result;
            PlayMinecartCue(score == 0 ? 3 : 2);
        }

        private void OfferSacks()
        {
            winningSack = Random.Range(0, 2);
            minecartPhase = MinecartPhase.PickSack;
        }

        private void OpenSack(int index)
        {
            PlayMinecartCue(1);
            bool doubled = index == winningSack;
            FinishMinecartRound(doubled ? revealedPrize * 2 : 0, doubled);
        }

        private void PlayMinecartCue(int index)
        {
            PlayGameplayEffect(minecartClips[index]);
        }

        private void DrawMinecart()
        {
            if (mineScoreStyle == null)
            {
                mineScoreStyle = LabelStyle(58, true);
                mineScoreStyle.alignment = TextAnchor.MiddleCenter;
                mineScoreStyle.normal.textColor = MineGold;
                mineChoiceStyle = new GUIStyle(buttonStyle) { fontSize = 18 };
            }

            Panel(new Rect(0, 0, 1280, 720), new Color(0.025f, 0.04f, 0.045f, 0.97f));
            Panel(new Rect(132, 48, 1016, 624), new Color(0.10f, 0.15f, 0.16f));
            Panel(new Rect(132, 48, 1016, 4), MineGold);
            GUI.Label(new Rect(168, 72, 780, 28), "OLD MINE  /  RETIRED MINER", smallStyle);
            GUI.Label(new Rect(168, 106, 600, 42), "LUCKY MINECART", titleStyle);
            GUI.Label(new Rect(168, 150, 940, 30),
                $"Rounds {roundsPlayed}     Best round {bestScore}     Double streak {doubleStreak}     Best streak {bestStreak}", textStyle);

            switch (minecartPhase)
            {
                case MinecartPhase.PickCart: DrawCartChoices(); break;
                case MinecartPhase.TakeOrDouble: DrawPrizeChoice(); break;
                case MinecartPhase.PickSack: DrawSackChoices(); break;
                case MinecartPhase.Result: DrawMinecartResult(); break;
            }

            if (Button(new Rect(934, 69, 178, 36), "Back to town [Esc]"))
                minecartOpen = false;
        }

        private void DrawCartChoices()
        {
            GUI.Label(new Rect(168, 201, 940, 58),
                "“Pick a cart,” says the miner. “Each hides a different prize.”", headingStyle);
            for (int i = 0; i < CartNames.Length; i++)
            {
                float x = 168 + i * 319;
                Panel(new Rect(x, 280, 306, 292), new Color(0.07f, 0.11f, 0.12f));
                DrawCoveredCart(x + 153, 385);
                if (Button(new Rect(x + 24, 490, 258, 55), CartNames[i], mineChoiceStyle))
                    RevealCart(i);
            }
            GUI.Label(new Rect(168, 611, 940, 32), "Minecart points are for this minigame.", smallStyle);
        }

        private void DrawCoveredCart(float x, float y)
        {
            Panel(new Rect(x - 107, y + 58, 214, 5), new Color(0.37f, 0.36f, 0.32f));
            Panel(new Rect(x - 107, y + 68, 214, 5), new Color(0.21f, 0.22f, 0.22f));
            Panel(new Rect(x - 75, y + 7, 150, 40), new Color(0.28f, 0.34f, 0.35f));
            Panel(new Rect(x - 63, y - 32, 126, 38), new Color(0.43f, 0.29f, 0.19f));
            Panel(new Rect(x - 75, y - 12, 150, 18), new Color(0.58f, 0.41f, 0.26f));
            Panel(new Rect(x - 52, y + 37, 23, 22), new Color(0.03f, 0.05f, 0.05f));
            Panel(new Rect(x + 29, y + 37, 23, 22), new Color(0.03f, 0.05f, 0.05f));
            Panel(new Rect(x - 18, y - 18, 36, 48), MineGold);
            GUI.Label(new Rect(x - 6, y - 4, 18, 24), "?", headingStyle);
        }

        private void DrawPrizeChoice()
        {
            GUI.Label(new Rect(168, 201, 940, 40), "Your cart held a prize", headingStyle);
            DrawMinecartScore(revealedPrize);
            GUI.Label(new Rect(168, 431, 940, 38),
                "Keep your points or pick a sealed sack. One doubles the prize. The other is empty.", textStyle);
            if (Button(new Rect(168, 506, 447, 55), $"Keep {revealedPrize} {(revealedPrize == 1 ? "point" : "points")}", mineChoiceStyle))
                FinishMinecartRound(revealedPrize, false);
            if (Button(new Rect(635, 506, 477, 55), "Double or nothing", mineChoiceStyle))
                OfferSacks();
        }

        private void DrawSackChoices()
        {
            GUI.Label(new Rect(168, 201, 940, 40),
                $"Pick a sealed sack. This round is worth {revealedPrize * 2} points or zero.", headingStyle);
            for (int i = 0; i < 2; i++)
            {
                float x = 168 + 477 * i;
                Panel(new Rect(x, 280, 467, 292), new Color(0.07f, 0.11f, 0.12f));
                Panel(new Rect(x + 179, 360, 109, 80), new Color(0.56f, 0.40f, 0.25f));
                Panel(new Rect(x + 197, 340, 73, 21), new Color(0.68f, 0.49f, 0.32f));
                Panel(new Rect(x + 194, 357, 79, 7), MineGold);
                if (Button(new Rect(x + 36, 490, 395, 55), i == 0 ? "Open left sack" : "Open right sack", mineChoiceStyle))
                    OpenSack(i);
            }
            GUI.Label(new Rect(168, 610, 940, 32), "Only this round's prize is at stake.", smallStyle);
        }

        private void DrawMinecartResult()
        {
            GUI.Label(new Rect(168, 201, 940, 40), roundResult, headingStyle);
            DrawMinecartScore(roundScore);
            GUI.Label(new Rect(168, 434, 940, 40), roundScore == 0
                ? "The miner sets out three fresh carts for the next round."
                : "The miner grins and covers the carts again.", textStyle);
            if (Button(new Rect(168, 506, 488, 55), "Play again", mineChoiceStyle))
                StartMinecartRound();
            if (Button(new Rect(680, 506, 432, 55), "Return to town", mineChoiceStyle))
                minecartOpen = false;
            if (MinecartDemoComplete)
                GUI.Label(new Rect(168, 610, 940, 32), "Demo complete. You can keep playing Lucky Minecart.", smallStyle);
        }

        private void DrawMinecartScore(int score)
        {
            float reveal = Mathf.SmoothStep(0f, 1f, (Time.unscaledTime - revealedAt) / 0.28f);
            Panel(new Rect(168, 265, 944, 132), new Color(0.06f, 0.10f, 0.11f));
            Panel(new Rect(168, 393, 944 * reveal, 4), score > 0 ? MineTeal : new Color(0.55f, 0.46f, 0.38f));
            GUI.Label(new Rect(168, 269 + 17 * (1f - reveal), 944, 117),
                $"+{score} {(score == 1 ? "point" : "points")}", mineScoreStyle);
        }
    }
}
