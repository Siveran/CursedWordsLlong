using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Llong.Unlocks
{
    class SnoozedAlarmClock : Item
    {
        public SnoozedAlarmClock()
        {
            Name = "Snoozed Alarm Clock";
            SpriteData.Add(new LlongItemData(ItemSpriteUsage.Default, "alarmclock"));
            Rarity = ItemRarity.Common;
            Cost = 13;
            DependencyTags = [];
            ItemFunctionTags = [ItemFunctionTag.Scatterer];
            RelevantColours = [];
        }
        public override string GetDescription()
        {
            return "START OF GRID: The bottom row of tiles become Zs";
        }

        private bool IsBottomTile(Tile tile)
        {
            return tile.GetCoordinates().y == 0;
        }

        public override GridData ApplyStartOfGridEffect(GridData gridData, int gridNumber, int numberOfGrids, List<HistoricWord> previousWords, List<BoardGenVizInfo> vizSteps, bool isReroll)
        {
            List<Tile> list = new List<Tile>();
            foreach (Tile tile in gridData.GetAvailableTiles())
            {
                if (IsBottomTile(tile))
                {
                    tile.SetLetter("z");
                    list.Add(tile);
                }
            }
            if (list.Count > 0)
            {
                vizSteps.Add(new BoardGenVizInfo(gridData, this, list, false, null, false, false, false, vizSteps[vizSteps.Count - 1].PlayerConsumableTiles));
            }
            return gridData;
        }
    }

    class SnoozedAlarmClockUnlock : Achievement
    {
        public SnoozedAlarmClockUnlock()
        {
            Name = "Just Five More Minutes";
            SteamAchievementName = "ACH_INVITATION_ONLY";
            TypeOfItemUnlocked = typeof(SnoozedAlarmClock);
            IsCrownReward = true;
            CrownRewardLevel = 0;
            CrownRewardCharacter = new LlongCharacter();
        }
    }
}
