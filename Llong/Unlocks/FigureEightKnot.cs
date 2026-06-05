using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Llong.Unlocks
{
    class FigureEightKnot : Item
    {
        public FigureEightKnot()
        {
            Name = "Figure Eight Knot";
            SpriteData.Add(new LlongItemData(ItemSpriteUsage.Default, "knot"));
            Rarity = ItemRarity.Common;
            Cost = 16;
            Tags =
                [
                    ItemTag.NumbersBuild,
                    ItemTag.BigNumbersBuild
                ];
            DependencyTags = [];
            ItemFunctionTags = [ItemFunctionTag.Tech];
            RelevantColours = [];
        }

        public override string GetDescription()
        {
            return string.Format("START OF GRID: Double the number on all number tiles");
        }

        public override GridData ApplyStartOfGridEffect(GridData gridData, int gridNumber, int numberOfGrids, List<HistoricWord> previousWords, List<BoardGenVizInfo> vizSteps, bool isReroll)
        {
            List<Tile> tiles = (from tile in gridData.GetAvailableTiles()
                               where tile.IsNumber()
                               select tile).ToList<Tile>();
            foreach (var tile in tiles)
            {
                if (tile.GetGlyphType() == GlyphType.Number)
                {
                    int newNumber = tile.GetNumber() * 2;
                    if (newNumber > 999999)
                    {
                        newNumber = 999999;
                    }
                    tile.SetNumber(newNumber);
                }
                else if (tile.GetGlyphType() == GlyphType.Fraction)
                {
                    List<int> fractionNumbers = tile.GetFractionNumbers();
                    fractionNumbers[0] *= 2;
                    if (fractionNumbers[0] > 999999)
                    {
                        fractionNumbers[0] = 999999;
                    }
                    tile.SetFractionNumbers(fractionNumbers);
                }
            }

            if (tiles.Count > 0)
            {
                vizSteps.Add(new BoardGenVizInfo(gridData, this, tiles, false, null, false, false, false, vizSteps[vizSteps.Count - 1].PlayerConsumableTiles));
            }
            return gridData;
        }
    }

    class FigureEightKnotUnlock : Achievement
    {
        public FigureEightKnotUnlock()
        {
            Name = "Tied Up";
            SteamAchievementName = "ACH_INVITATION_ONLY";
            TypeOfItemUnlocked = typeof(FigureEightKnot);
            IsCrownReward = true;
            CrownRewardLevel = 6;
            CrownRewardCharacter = new LlongCharacter();
        }
    }
}
