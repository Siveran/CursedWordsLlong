using System;
using System.Collections.Generic;
using System.Text;

namespace Llong.Unlocks
{
    class Atria : Item
    {
        public Atria()
        {
            Name = "Atria";
            SpriteData.Add(new LlongItemData(ItemSpriteUsage.Default, "revolvinghearts"));
            Rarity = ItemRarity.Common;
            Cost = 9;
            Tags =
                [
                    ItemTag.CardsBuild,
                    ItemTag.CardsGenerator
                ];
            DependencyTags = [];
            UpgradeableComponents.Add(new UpgradeableComponent(1, 2, 2));
            ItemFunctionTags = [ItemFunctionTag.Scatterer];
            RelevantColours = [];
        }

        public override string GetDescription()
        {
            return string.Format("Each set of repeated letters scatters {0}{1}{2} {3}s onto the next grid",
                GameStatics.ZeroWidthCharacter,
                this.UpgradeableComponents[0].VariableValue,
                GameStatics.ZeroWidthCharacter,
                PlayingCardUtility.GetFontTaggedSuitIcon(Suit.Hearts));
        }

        private int CountRepeatSets(List<Tile> tiles)
        {
            string previousTileString = null;
            string currentTileString = null;
            bool isCurrentSetCounted = false;
            int setCount = 0;
            for (int i = 0; i < tiles.Count; i++)
            {
                currentTileString = tiles[i].GetStringRepresentation(false);
                if (previousTileString == currentTileString)
                {
                    if (!isCurrentSetCounted)
                    {
                        setCount++;
                        isCurrentSetCounted = true;
                    }
                }
                else
                {
                    isCurrentSetCounted = false;
                }
                previousTileString = currentTileString;
            }
            return setCount;
        }

        public override GridData ApplyStartOfGridEffect(GridData gridData, int gridNumber, int numberOfGrids, List<HistoricWord> previousWords, List<BoardGenVizInfo> vizSteps, bool isReroll)
        {
            if (previousWords.Count == 0)
            {
                return gridData;
            }
            int num = CountRepeatSets(previousWords[previousWords.Count - 1].Tiles);
            
            if (num == 0)
            {
                return gridData;
            }
            List<Tile> list = new List<Tile>();
            for (int i = 0; i < num * this.UpgradeableComponents[0].VariableValue; i++)
            {
                Tile tileForItemScatter = GridUtility.Singleton.GetTileForItemScatter(gridData, TileType.Normal, GlyphType.None, null, true);
                if (tileForItemScatter != null)
                {
                    tileForItemScatter.SetSuit(Suit.Hearts);
                    list.Add(tileForItemScatter);
                }
            }
            if (list.Count > 0)
            {
                vizSteps.Add(new BoardGenVizInfo(gridData, this, list, false, null, false, false, true, vizSteps[vizSteps.Count - 1].PlayerConsumableTiles));
            }
            return gridData;
        }
    }

    class AtriaUnlock : Achievement
    {
        public AtriaUnlock()
        {
            Name = "Contract and Expand";
            SteamAchievementName = "ACH_INVITATION_ONLY";
            TypeOfItemUnlocked = typeof(Atria);
            IsCrownReward = true;
            CrownRewardLevel = 5;
            CrownRewardCharacter = new LlongCharacter();
        }
    }
}
