using System;
using System.Collections.Generic;
using System.Text;

namespace Llong.Unlocks
{
    class YoYo : Item
    {
        public YoYo()
        {
            Name = "Otter";
            SpriteData.Add(new LlongItemData(ItemSpriteUsage.Default, "otter"));
            Rarity = ItemRarity.Common;
            Cost = 8;
            Tags = [];
            DependencyTags = [];
            UpgradeableComponents = [new UpgradeableComponent(1, 1, 1)];
            ItemFunctionTags = [ItemFunctionTag.Tech];
            RelevantColours = [];
            IsAnimal = true;
        }
        public override string GetDescription()
        {
            return string.Format("Letters can be stretched {0}+{1}{2} time{3}",
                GameStatics.ZeroWidthCharacter,
                this.UpgradeableComponents[0].VariableValue,
                GameStatics.ZeroWidthCharacter,
                CheckPlural("s", this.UpgradeableComponents[0].VariableValue));
        }
    }

    class YoYoUnlock : Achievement
    {
        public YoYoUnlock()
        {
            Name = "Long Animal";
            SteamAchievementName = "ACH_INVITATION_ONLY";
            TypeOfItemUnlocked = typeof(YoYo);
            IsCrownReward = true;
            CrownRewardLevel = 1;
            CrownRewardCharacter = new LlongCharacter();
        }
    }
}