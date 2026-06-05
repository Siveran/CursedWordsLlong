using UnityEngine;

namespace Llong.Unlocks
{
    class CommonAccordion : Item
    {
        public CommonAccordion()
        {
            Name = "Common Accordion";
            //SpriteData.Add(new ItemSpriteData(ItemSpriteUsage.Default, "BlackHole"));
            SpriteData.Add(new LlongItemData(ItemSpriteUsage.Default, "accordion"));
            Rarity = ItemRarity.Common;
            Cost = 13;
            Tags =
                [
                    ItemTag.RainbowBuild
                ];
            DependencyTags = [];
            //UpgradeableComponents = [new UpgradeableComponent(1, 4, 4)];
            ItemFunctionTags = [ItemFunctionTag.Scatterer];
            RelevantColours = [];
        }
        public override string GetDescription()
        {
            return string.Format("START OF GRID: Randomly recolor tiles that have the most common letter on the grid");
            /*
            return string.Format("START OF GRID: Tiles with the most common letter on the grid get {0}+{1}{2} BASE SCORE",
                GameStatics.ZeroWidthCharacter,
                this.UpgradeableComponents[0].VariableValue,
                GameStatics.ZeroWidthCharacter);
            */
        }

        public override GridData ApplyStartOfGridEffect(GridData gridData, int gridNumber, int numberOfGrids, List<HistoricWord> previousWords, List<BoardGenVizInfo> vizSteps, bool isReroll)
        {
            // Find the highest tile count
            Dictionary<string, int> tileCounts = [];
            int largestCount = 0;
            foreach (var tile in gridData.GetAvailableTiles())
            {
                string tileString = tile.GetStringRepresentation(false);
                if (tileCounts.ContainsKey(tileString))
                {
                    tileCounts[tileString]++;
                    if (tileCounts[tileString] > largestCount)
                    {
                        largestCount = tileCounts[tileString];
                    }
                }
                else
                {
                    tileCounts[tileString] = 1;
                }
            }

            List<Tile> commonTiles = (from tile in gridData.GetAvailableTiles()
                                      where tileCounts[tile.GetStringRepresentation(false)] == largestCount
                                      select tile).ToList();
            List<Tile> changedTiles = [];
            while (commonTiles.Count > 0)
            {
                Tile tile = commonTiles[UnityEngine.Random.Range(0, commonTiles.Count)];
                tile.SetTileType(ItemPools.GetRandomColouredTileTypeWeighted());
                changedTiles.Add(tile);
                commonTiles.Remove(tile);
            }
            if (changedTiles.Count > 0)
            {
                vizSteps.Add(new BoardGenVizInfo(gridData, this, changedTiles, false, null, false, false, false, vizSteps[vizSteps.Count - 1].PlayerConsumableTiles));
            }
            return gridData;
        }
    }

    class ClassicAccordionUnlock : Achievement
    {
        public ClassicAccordionUnlock()
        {
            Name = "Expand and Contract";
            SteamAchievementName = "ACH_INVITATION_ONLY";
            TypeOfItemUnlocked = typeof(CommonAccordion);
            IsCrownReward = true;
            CrownRewardLevel = 4;
            CrownRewardCharacter = new LlongCharacter();
        }
    }
}
