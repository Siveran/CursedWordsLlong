using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Llong.Unlocks
{
    class Bookkeeper : Item
    {
        public List<Tile> TilesFound = [];

		public Bookkeeper()
		{
            Name = "Bookkeeper";
            SpriteData.Add(new LlongItemData(ItemSpriteUsage.Default, "books"));
            Rarity = ItemRarity.Legendary;
            Cost = 30;
            Tags = [];
            DependencyTags = [];
            ItemFunctionTags = [ItemFunctionTag.Scatterer];
            RelevantColours = [];
        }

        public override string GetDescription()
        {
            string description;
            if (TilesFound.Count > 0)
            {
                description = "Scatters: " + string.Join(", ", TilesFound.Select(tile => tile.GetStringRepresentation(false).ToUpper()));
            }
            else
            {
                description = "Does nothing";
            }
            return string.Join("<br><br>", [description, "(Submit a word with a double letter to improve this)"]);
        }

        public override GridData ApplyStartOfGridEffect(GridData gridData, int gridNumber, int numberOfGrids, List<HistoricWord> previousWords, List<BoardGenVizInfo> vizSteps, bool isReroll)
        {
            List<Tile> list = new List<Tile>();
            for (int i = 0; i < this.TilesFound.Count; i++)
            {
                Tile tile = this.TilesFound[i];
                Tile tileForItemScatter = GridUtility.Singleton.GetTileForItemScatter(gridData, tile.GetTileType(), tile.GetGlyphType(), null, true);
                if (tileForItemScatter != null)
                {
                    tileForItemScatter.SetAsCopy(TilesFound[i], false);
                    list.Add(tileForItemScatter);
                }
            }
            if (list.Count > 0)
            {
                vizSteps.Add(new BoardGenVizInfo(gridData, this, list, false, null, false, false, false, vizSteps[vizSteps.Count - 1].PlayerConsumableTiles));
            }
            return gridData;
        }

        public override void ApplyWordBonus(ScoreCalcVizInfo step, int gridNumber, List<Tile> tiles, List<string> words, List<TileSelection> selections, List<HistoricWord> previousWords, GridData gridData)
        {
            if (tiles.Count < 2)
            {
                return;
            }

            string previousTileString = null;
            string currentTileString;
            int foundIndex = 0;
            for (int i = 0; i < tiles.Count; i++)
            {
                currentTileString = tiles[i].GetStringRepresentation(false);
                if (previousTileString == currentTileString)
                {
                    TilesFound.Add(tiles[i - 1]);
                    foundIndex = i;
                    break;
                }
                previousTileString = currentTileString;
            }
            if (foundIndex > 0)
            {
                step.RelevantItem = this;
                step.LettersInWordToPulse = [tiles[foundIndex - 1], tiles[foundIndex]];
            }
        }
    }

    internal class BookkeeperUnlock : Achievement
    {
        public BookkeeperUnlock()
        {
            Name = "Knowledge Hoard";
            SteamAchievementName = "ACH_INVITATION_ONLY";
            TypeOfItemUnlocked = typeof(Bookkeeper);
            IsCrownReward = true;
            CrownRewardLevel = 7;
            CrownRewardCharacter = new LlongCharacter();
        }
    }
}
