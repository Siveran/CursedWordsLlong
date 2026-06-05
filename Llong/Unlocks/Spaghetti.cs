using UnityEngine;

namespace Llong.Unlocks
{
    // The item itself
    internal class Spaghetti : Item
    {
        public Spaghetti()
        {
            this.Name = "Spaghetti";
            this.SpriteData.Add(new LlongItemData(ItemSpriteUsage.Default, "spaghetti"));
            this.Rarity = ItemRarity.Unique;
            this.UpgradeableComponents = [
                    new UpgradeableComponent(1, 1, 1), // Max letter repeats (1/2/3/...)
                    new UpgradeableComponent(1, 1, 1) // Repeated letter multiplier (1/2/3/...)
                ];

            //this.Tags = [ ItemTag.ShinyBuild, ItemTag.BlankBuild ];

            this.PinColors = [
                    new Color32(110, byte.MaxValue, 60, byte.MaxValue),
                    new Color32(230, 191, 85, byte.MaxValue)
                ];

            this.ItemFunctionTags = new List<ItemFunctionTag>();
        }

        public override string GetDescription()
        {
            var description = string.Format("You can stretch each letter up to {0}{1}{2} time{3}",
                [
                    GameStatics.ZeroWidthCharacter,
                    this.UpgradeableComponents[0].VariableValue,
                    GameStatics.ZeroWidthCharacter,
                    Item.CheckPlural("s", this.UpgradeableComponents[0].VariableValue),
                ]);

            if (this.UpgradeableComponents[1].VariableValue > 1)
            {
                var multiplierDescription = string.Format("Repeated letters get {0}x{1}{2} TILE SCORE",
                    [
                        GameStatics.ZeroWidthCharacter,
                        this.UpgradeableComponents[1].VariableValue,
                        GameStatics.ZeroWidthCharacter,
                    ]);
                description = string.Join(". ", [description, multiplierDescription]);
            }

            return description;
        }

        public override void Upgrade(int componentIndex, bool upgradingBoth = false)
        {
            if (upgradingBoth)
            {
                this.Upgrade(0, false);
                this.Upgrade(1, false);
                return;
            }
            base.Upgrade(componentIndex, false);
            if (this.UpgradeableComponents[1].Level >= 2 && !this.ItemFunctionTags.Contains(ItemFunctionTag.SpecificMultiplier))
            {
                this.ItemFunctionTags.Add(ItemFunctionTag.SpecificMultiplier);
            }
        }

        public override void ApplyTileBonus(ScoreCalcVizInfo step, int index, List<Tile> tiles, List<TileSelection> selections, List<HistoricWord> previousWords, GridData gridData)
        {
            if (tiles.Count <= 1)
            {
                return;
            }

            // If the next or previous tile is the same letter...
            if ((index > 0 && tiles[index].GetStringRepresentation(false) == tiles[index - 1].GetStringRepresentation(false))
                || (index + 1 < tiles.Count && tiles[index].GetStringRepresentation(false) == tiles[index + 1].GetStringRepresentation(false)))
            {
                List<ScorePacket> tileScores = step.TileScores;
                tileScores[index] *= (long)this.UpgradeableComponents[1].VariableValue;
                step.TileScoreMultipliers[index] = new int?(this.UpgradeableComponents[1].VariableValue);
            }
        }
    }
}
