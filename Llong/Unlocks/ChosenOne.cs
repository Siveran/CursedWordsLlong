using MelonLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SocialPlatforms.Impl;

namespace Llong.Unlocks
{
    class ChosenOne : Item
    {
        public string ChosenLetter = "";
        public int ChosenTotal = 0;

        public ChosenOne()
        {
            Name = "Chosen One";
            //SpriteData.Add(new ItemSpriteData(ItemSpriteUsage.Default, "BlackHole"));
            SpriteData.Add(new LlongItemData(ItemSpriteUsage.Default, "person"));
            Rarity = ItemRarity.Rare;
            Cost = 16;
            Tags = [];
            DependencyTags = [];
            UpgradeableComponents = [new UpgradeableComponent(1, 1, 1)];
            ItemFunctionTags = [ItemFunctionTag.Tech];
            RelevantColours = [];
        }

        public override string GetDescription()
        {
            string description = string.Format("Get {0}+{1}{2} WORD SCORE. Improved by {3}+{4}{5} for each time you've used the chosen letter<br><br>",
                GameStatics.ZeroWidthCharacter,
                this.ChosenTotal,
                GameStatics.ZeroWidthCharacter,
                GameStatics.ZeroWidthCharacter,
                this.UpgradeableComponents[0].VariableValue,
                GameStatics.ZeroWidthCharacter,
                CheckPlural("y", "ies", this.UpgradeableComponents[0].VariableValue));
            if (this.ChosenLetter == "")
            {
                return string.Join("", description, "(The first letter you use becomes the chosen letter)");
            }
            else
            {
                return string.Join("", description, $"(The chosen letter is {ChosenLetter.ToUpper()})");
            }
        }

        public override void ApplyWordBonus(ScoreCalcVizInfo step, int gridNumber, List<Tile> tiles, List<string> words, List<TileSelection> selections, List<HistoricWord> previousWords, GridData gridData)
        {
            if (this.ChosenLetter == "")
            {
                this.ChosenLetter = tiles[0].GetStringRepresentation(false);
            }

            foreach (var tile in tiles)
            {
                if (tile.GetStringRepresentation(false) == this.ChosenLetter)
                {
                    this.ChosenTotal += this.UpgradeableComponents[0].VariableValue;
                    step.LettersInWordToPulse.Add(tile);
                }
            }

            if (this.ChosenTotal != 0)
            {
                step.WordBonus = new WordBonusToken((long)(this.ChosenTotal), false, false);
            }
        }
    }

    class ChosenOneUnlock : Achievement
    {
        public ChosenOneUnlock()
        {
            Name = "I Need a Hero";
            SteamAchievementName = "ACH_INVITATION_ONLY";
            TypeOfItemUnlocked = typeof(ChosenOne);
            IsCrownReward = true;
            CrownRewardLevel = 2;
            CrownRewardCharacter = new LlongCharacter();
        }
    }
}
