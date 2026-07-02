using MelonLoader;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Llong.Unlocks
{
    class CleverDisguise : Item
    {
        public CleverDisguise()
        {
            Name = "Clever Disguise";
            SpriteData.Add(new LlongItemData(ItemSpriteUsage.Default, "disguise"));
            Rarity = ItemRarity.Rare;
            Cost = 16;
            Tags = [];
            DependencyTags = [];
            UpgradeableComponents = [new UpgradeableComponent(1, 1, 2)];
            ItemFunctionTags = [ItemFunctionTag.Scatterer];
            RelevantColours = [];
        }
        public override string GetDescription()
        {
            return string.Format("START OF GRID: Spread {0}{1}{2} cop{3} of a tile with the highest base value",
                GameStatics.ZeroWidthCharacter,
                this.UpgradeableComponents[0].VariableValue,
                GameStatics.ZeroWidthCharacter,
                CheckPlural("y", "ies", this.UpgradeableComponents[0].VariableValue));
        }

        public override GridData ApplyStartOfGridEffect(GridData gridData, int gridNumber, int numberOfGrids, List<HistoricWord> previousWords, List<BoardGenVizInfo> vizSteps, bool isReroll)
        {
            // Find the highest base value tiles
            List<Tile> tiles = gridData.GetAvailableTiles();
            List<Tile> valuableTiles = [];
            long highestValue = -999999;
            foreach (var tile in tiles)
            {
                if (tile.GetValue().Score > highestValue)
                {
                    highestValue = tile.GetValue().Score;
                    valuableTiles = [tile];
                }
                else if (tile.GetValue().Score == highestValue)
                {
                    valuableTiles.Add(tile);
                }
            }

            if (tiles.Count == 0)
            {
                MelonLogger.Warning("[Llong] Somehow Clever Disguise didn't find any highest value submitted tiles");
                return gridData;
            }

            // Pick a random one
            Tile highestTile = valuableTiles[UnityEngine.Random.Range(0, valuableTiles.Count)];
            List<Tile> scatteredTiles = [];
            for (int i = 0; i < this.UpgradeableComponents[0].VariableValue; i++)
            {
                Tile tileToScatter = GridUtility.Singleton.GetTileForItemScatter(gridData,
                    highestTile.GetTileType(),
                    highestTile.GetGlyphType(),
                    tiles.Except([.. scatteredTiles, .. valuableTiles]).ToList(),
                    highestTile.CardSuit != Suit.None && highestTile.CardSuit != Suit.Joker);

                if (tileToScatter == null)
                {
                    MelonLogger.Warning("[Llong] Clever Disguise couldn't get a tile to scatter to!");
                    continue;
                }

                Core.DebugLog($"[Llong:Unlocks.CleverDisguise] Scattering a {highestTile.GetStringRepresentation(false)} to {tileToScatter.GetCoordinates().x},{tileToScatter.GetCoordinates().y}");
                tileToScatter.SetAsCopy(highestTile, false);
                scatteredTiles.Add(tileToScatter);
            }

            vizSteps.Add(new BoardGenVizInfo(gridData, this, scatteredTiles, false, null, false, false, false, vizSteps[vizSteps.Count - 1].PlayerConsumableTiles));
            return gridData;
        }
    }

    class CleverDisguiseUnlock : Achievement
    {
        public CleverDisguiseUnlock()
        {
            Name = "Nice Moustache";
            SteamAchievementName = "ACH_INVITATION_ONLY";
            TypeOfItemUnlocked = typeof(CleverDisguise);
            IsCrownReward = true;
            CrownRewardLevel = 3;
            CrownRewardCharacter = new LlongCharacter();
        }
    }
}
