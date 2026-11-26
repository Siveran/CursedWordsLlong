namespace Llong.Unlocks
{
    class Bookkeeper : Item
    {
        public List<Tile> TilesRemembered = [];

		public Bookkeeper()
		{
            Name = "Bookkeeper";
            SpriteData.Add(new LlongItemData(ItemSpriteUsage.Default, "shush"));
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
            if (TilesRemembered.Count > 0)
            {
                description = "Scatters: " + string.Join(", ", TilesRemembered.Select(tile => tile.GetStringRepresentation(false).ToUpper()));
            }
            else
            {
                description = "Does nothing";
            }
            return string.Join("<br><br>", [description, "(Submit a word with a double letter to improve this)"]);
        }

        private void CopyGlyphFromTile(Tile source, Tile destination)
        {
            destination.Letter = source.Letter;
            destination.Number = source.Number;
            destination.FractionNumbers = [.. source.FractionNumbers];
            destination.PieceType = source.PieceType;
            destination.IsWhitePiece = source.IsWhitePiece;
            destination.IsEmptyTile = source.IsEmpty();
            if (source.ScatteredItem != null)
            {
                Item item = Activator.CreateInstance(source.ScatteredItem.GetType()) as Item;
                if (source.ScatteredItem.UpgradeableComponents.Count > 0 && source.ScatteredItem.UpgradeableComponents[0].Level > 1)
                {
                    for (int i = 0; i < source.ScatteredItem.UpgradeableComponents[0].Level - 1; i++)
                    {
                        item.Upgrade(0, false);
                    }
                }
                destination.ScatteredItem = item;
            }
            destination.MyGlyphType = source.GetGlyphType();
        }

        public override GridData ApplyStartOfGridEffect(GridData gridData, int gridNumber, int numberOfGrids, List<HistoricWord> previousWords, List<BoardGenVizInfo> vizSteps, bool isReroll)
        {
            List<Tile> changedTiles = [];
            List<Tile> allowedTargets = gridData.GetAvailableTiles();
            List<Tile> availableRememberedTiles = [.. TilesRemembered];
            for (int i = 0; i < this.TilesRemembered.Count; i++)
            {
                Tile rememberedTile = availableRememberedTiles[UnityEngine.Random.Range(0, availableRememberedTiles.Count)];
                availableRememberedTiles.Remove(rememberedTile);
                Tile tileForItemScatter = GridUtility.Singleton.GetTileForItemScatter(gridData, TileType.Normal, rememberedTile.GetGlyphType(), allowedTargets, true);
                if (tileForItemScatter != null)
                {
                    allowedTargets.Remove(tileForItemScatter);
                    CopyGlyphFromTile(rememberedTile, tileForItemScatter);
                    changedTiles.Add(tileForItemScatter);
                }
            }
            if (changedTiles.Count > 0)
            {
                vizSteps.Add(new BoardGenVizInfo(gridData, this, changedTiles, false, null, false, false, false, vizSteps[vizSteps.Count - 1].PlayerConsumableTiles));
            }
            return gridData;
        }

        public override void ApplyWordBonus(ScoreCalcVizInfo step, int gridNumber, List<Tile> tiles, List<string> words, List<TileSelection> selections, List<HistoricWord> previousWords, GridData gridData, List<Tile> allTiles = null)
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
                    TilesRemembered.Add(tiles[i - 1]);
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
