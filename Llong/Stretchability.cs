using HarmonyLib;
using Llong.Unlocks;
using MelonLoader;
using System.Reflection;

namespace Llong
{
    // Does what's needed to do to support ssttrreettcchhaabbiilliittyy in words
    internal class Stretchability
    {
        // We use this as a fallback if the cache messes up somehow.
        // Note: this MAY be buggy in the future or with other mods, but works currently
        // as every inventoryCache is used before the next one is generated. I think.
        static public int CurrentStretchability;

        private const int MAX_STRETCHABILITY_CACHE = 3;
        private static Dictionary<InventoryCache, int> StretchabilityCache = [];
        private static readonly List<InventoryCache> InventoryCaches = [];
        private static int NextIndexToSet = 0; // increments 0 - 2

        static public void CacheStretchability(InventoryCache inventoryCache, int stretchability)
        {
            if (Stretchability.InventoryCaches.Count > NextIndexToSet)
            {
                Stretchability.StretchabilityCache.Remove(Stretchability.InventoryCaches[NextIndexToSet]);
                Stretchability.InventoryCaches.RemoveAt(NextIndexToSet);
            }
            Stretchability.InventoryCaches.Insert(NextIndexToSet, inventoryCache);
            StretchabilityCache.Add(inventoryCache, stretchability);
            NextIndexToSet = (NextIndexToSet + 1) % MAX_STRETCHABILITY_CACHE;
            CurrentStretchability = stretchability;
        }

        static public int GetStretchability(InventoryCache inventoryCache)
        {
            if (!StretchabilityCache.ContainsKey(inventoryCache))
            {
                MelonLogger.Warning("Couldn't find a cached inventory when looking up stretchability! This is likely due to a mod conflict with Llong and something else. Defaulting to current stretchability.");
                return CurrentStretchability;
            }

            return StretchabilityCache[inventoryCache];
        }


        static public List<Tile> ApplyStretching(List<Tile> tiles, InventoryCache inventoryCache)
        {
            return ApplyStretching(tiles, GetStretchability(inventoryCache));
        }

        static public List<Tile> ApplyStretching(List<Tile> tiles, int stretchability)
        {
            if (stretchability == 0)
            {
                return tiles;
            }

            List<Tile> result = [];
            Tile previousTile = null;
            int currentStretch = 0;
            for (int i = 0; i < tiles.Count; i++)
            {
                if (previousTile != null && previousTile.GetStringRepresentation(false) == tiles[i].GetStringRepresentation(false))
                {
                    // Repeat detected! Try to stretch.
                    if (currentStretch < stretchability)
                    {
                        currentStretch++;
                    }
                    else
                    {
                        currentStretch = 0;
                        result.Add(tiles[i]);
                    }
                }
                else
                {
                    // Non-repeat.
                    currentStretch = 0;
                    result.Add(tiles[i]);
                }
                previousTile = tiles[i];
            }
            return result;
        }

        // Gets the full length of tiles involved in a submitted word. Used for Honeypot + pertinent words.
        // Given tiles ABBCCC and stretchability 1, would return 1, 3, 5, and 6 for 1, 2, 3, and 4 respectively
        static public int GetStretchedLength(List<Tile> tiles, int stretchability, int unstretchedLength)
        {
            string previousTileString = null;
            int unstretchedLengthAccountedFor = 0;
            int currentStretch = 0;
            int stretchedLength = 0;
            for (int i = 0; i < tiles.Count; i++)
            {
                string currentTileString = tiles[i].GetStringRepresentation(false);
                if (previousTileString == currentTileString)
                {
                    if (currentStretch < stretchability)
                    {
                        currentStretch++;
                    }
                    else
                    {
                        currentStretch = 0;
                        unstretchedLengthAccountedFor++;
                    }
                }
                else
                {
                    currentStretch = 0;
                    unstretchedLengthAccountedFor++;
                }
                previousTileString = currentTileString;
                if (unstretchedLengthAccountedFor > unstretchedLength)
                {
                    break;
                }
                else
                {
                    stretchedLength++;
                }
            }
            //MelonLogger.Msg($"Tiles selected: {tiles.Count()} | Stretchability: {stretchability} | Unstretched length: {unstretchedLength} | Stretched length: {stretchedLength}");
            return stretchedLength;
        }

        static public int GetStretchedLength(List<Tile> tiles, InventoryCache inventoryCache, int unstretchedLength)
        {
            return GetStretchedLength(tiles, GetStretchability(inventoryCache), unstretchedLength);
        }

        static public int GetStretchabilityFromItems(List<Item> items)
        {
            var stretchability = 0;

            Item spaghetti = items.Find((Item item) => item is Spaghetti);
            if (spaghetti != null)
            {
                stretchability += spaghetti.UpgradeableComponents[0].VariableValue;
            }

            Item yoYo = items.Find((Item item) => item is YoYo);
            if (yoYo != null)
            {
                stretchability += yoYo.UpgradeableComponents[0].VariableValue;
            }

            return stretchability;
        }
    }

    // Normally, InventoryCache is used to check the items currently applying to the word submission.
    // That's how things like Red Envelope and Microscope work. Unfortunately, we can't just plop a new field into InventoryCache,
    // so we need to roll our own.

    // Hook into InventoryCache's constructor so we count stretchability at the right time
    [HarmonyPatch(typeof(InventoryCache))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(IEnumerable<Tile>) })]
    class InventoryCache_CTOR_Patch
    {
        static void Postfix(IEnumerable<Tile> currentWordTiles, InventoryCache __instance)
        {
            ///// Duplicated logic from InventoryCache /////
            List<Item> allItems = GameStatics.GetPlayer().GetAllItems(false);
            foreach (Tile tile in currentWordTiles)
            {
                if (tile.GetGlyphType() == GlyphType.ScatteredItem)
                {
                    allItems.Add(tile.ScatteredItem);
                }
            }
            Item item8 = allItems.Find((Item item) => item is RandomAccessMemory);
            Item item2 = allItems.Find((Item item) => item is Snapshot);
            List<Item> list = allItems.Where((Item item) => item is Frankenstein).ToList<Item>();
            if (item8 != null)
            {
                RandomAccessMemory randomAccessMemory = item8 as RandomAccessMemory;
                allItems.AddRange(randomAccessMemory.ItemsInMemory);
            }
            if (item2 != null)
            {
                Snapshot snapshot = item2 as Snapshot;
                if (snapshot.SnapshottedItem != null)
                {
                    allItems.Add(snapshot.SnapshottedItem);
                }
            }
            foreach (Item item3 in list)
            {
                Frankenstein frankenstein = item3 as Frankenstein;
                allItems.AddRange(frankenstein.StitchedItems);
            }
            ///// End duplicated logic from InventoryCache /////

            var stretchability = Stretchability.GetStretchabilityFromItems(allItems);

            Stretchability.CacheStretchability(__instance, stretchability);
        }
    }

    // Hook into the places InventoryCache is used so we can apply stretching at the right time
    [HarmonyPatch(typeof(Vocabulary), "IsWordContainedInList")]
    class Vocabulary_IsWordContainedInList_Patch
    {
        static void Prefix(ref List<Tile> tiles, InventoryCache inventoryCache)
        {
            tiles = Stretchability.ApplyStretching(tiles, Stretchability.GetStretchability(inventoryCache));
        }
    }

    [HarmonyPatch(typeof(Vocabulary), "TryGetAccumulatorWordFromTiles")]
    class Vocabulary_TryGetAccumulatorWordFromTiles_Patch
    {
        static void Prefix(ref List<Tile> tiles, InventoryCache inventoryCache)
        {
            tiles = Stretchability.ApplyStretching(tiles, Stretchability.GetStretchability(inventoryCache));
        }
    }

    // Honeypot is scary
    [HarmonyPatch(typeof(Vocabulary), "GetConcatenatedValidWordsFromTiles")]
    class Vocabulary_GetConcatenatedValidWordsFromTiles_Patch
    {
        public static bool Prefix(ref List<Tile> tiles, List<BossModifier> bossModifiers, out List<string> __result)
        {
            InventoryCache inventoryCache = new InventoryCache(tiles);
            if (Stretchability.GetStretchability(inventoryCache) == 0)
            {
                // There's no stretchability in play. Just do the normal logic.
                __result = [];
                return true;
            }
            inventoryCache.PertinentAccumulatorWords.Clear();
            List<string> list3 = inventoryCache.PertinentAccumulatorPairs.Select((List<string> list) => list[0]).ToList<string>();
            List<string> list2 = inventoryCache.PertinentAccumulatorPairs.Select((List<string> list) => list[1]).ToList<string>();

            MethodInfo tryGetAccumulatorWordFromTiles = AccessTools.Method(typeof(Vocabulary), "TryGetAccumulatorWordFromTiles", [typeof(InventoryCache), typeof(List<Tile>)]);
            foreach (string text in list3)
            {
                inventoryCache.PertinentAccumulatorWords.Clear();
                inventoryCache.PertinentAccumulatorWords.Add(text);
                int stretchedLength = Stretchability.GetStretchedLength(tiles, inventoryCache, text.Length);
                if (tryGetAccumulatorWordFromTiles.Invoke(null, [inventoryCache, tiles.Take(stretchedLength)]) is string text2)
                {
                    int num = list3.IndexOf(text);
                    inventoryCache.PertinentAccumulatorWords.Clear();
                    inventoryCache.PertinentAccumulatorWords.Add(list2[num]);
                    if (tryGetAccumulatorWordFromTiles.Invoke(null, [inventoryCache, tiles.Skip(stretchedLength)]) is string text3)
                    {
                        __result = new List<string> { text2, text3 };
                        return false;
                    }
                }
            }
            
            InventoryCache inventoryCache2 = new InventoryCache(tiles);
            string validWordFromTiles = Vocabulary.GetValidWordFromTiles(tiles, bossModifiers, inventoryCache2);
            if (validWordFromTiles != null)
            {
                __result = new List<string> { validWordFromTiles };
                return false;
            }
            inventoryCache2.PertinentAccumulatorWords.Clear();
            
            // Unstretch the word to get the unstretched length
            List<Tile> unstretched = Stretchability.ApplyStretching(tiles, inventoryCache2);
            for (int i = 1; i < unstretched.Count<Tile>(); i++)
            {
                int cutIndex = Stretchability.GetStretchedLength(tiles, inventoryCache2, i);
                List<Tile> tilePortion = [.. tiles.Take(cutIndex)];
                string validWordFromTiles2 = Vocabulary.GetValidWordFromTiles(tilePortion, bossModifiers, inventoryCache2);
                if (validWordFromTiles2 != null)
                {
                    tilePortion = [.. tiles.Skip(cutIndex)];
                    string validWordFromTiles3 = Vocabulary.GetValidWordFromTiles(tilePortion, bossModifiers, inventoryCache2);
                    if (validWordFromTiles3 != null)
                    {
                        __result = new List<string> { validWordFromTiles2, validWordFromTiles3 };
                        return false;
                    }
                }
            }
            __result = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(WordLetterController), "Populate")]
    class WordLetterController_Populate_Patch
    {
        static void Prefix(Tile tile, TileSelection tileSelection)
        {
            tileSelection.IsWobbly = tile.IsDisplayingAsVariableLetter();
            //MelonLogger.Msg($"[Llong:Stretchability] Setting tile {tile.GetStringRepresentation(false)} wobbliness to {(tileSelection.IsWobbly ? "true" : "false")}");
        }
    }

    // Stretched selected tiles should show as wobbly, and all current stretch-eligible tiles should show as wobbly
    [HarmonyPatch(typeof(Tile), "IsDisplayingAsVariableLetter")]
    class Tile_IsDisplayingAsVariableLetter_Patch
    {
        static bool Prefix(ref bool __result, Tile __instance, TileSelectionManager tileSelectionManager = null)
        {
            if (__instance.MyGlyphType == GlyphType.ScatteredItem)
            {
                // Items don't like wobbling :( Just use normal logic.
                return true;
            }

            ///// Copied logic from original IsDisplayingAsVariableLetter /////
            List<Item> allItems = GameStatics.GetPlayer().GetAllItems(false);
            if (tileSelectionManager == null)
            {
                tileSelectionManager = UnityEngine.Object.FindFirstObjectByType<TileSelectionManager>();
            }
            if (tileSelectionManager != null)
            {
                foreach (Tile tile in tileSelectionManager.GetTilesFromSelectedTiles())
                {
                    if (tile.GetGlyphType() == GlyphType.ScatteredItem)
                    {
                        allItems.Add(tile.ScatteredItem);
                    }
                }
            }
            Item item5 = allItems.Find((Item item) => item is RandomAccessMemory);
            Item item2 = allItems.Find((Item item) => item is Snapshot);
            List<Item> list = allItems.Where((Item item) => item is Frankenstein).ToList<Item>();
            if (item5 != null)
            {
                RandomAccessMemory randomAccessMemory = item5 as RandomAccessMemory;
                allItems.AddRange(randomAccessMemory.ItemsInMemory);
            }
            if (item2 != null)
            {
                Snapshot snapshot = item2 as Snapshot;
                if (snapshot.SnapshottedItem != null)
                {
                    allItems.Add(snapshot.SnapshottedItem);
                }
            }
            ///// End copied logic from original IsDisplayingAsVariableLetter /////
            
            // TODO: Try tacking on isCrossedOut and see what happens?
            var stretchability = Stretchability.GetStretchabilityFromItems(allItems);

            //MelonLogger.Msg($"[Llong:Stretchability] Current stretchability is {stretchability}");
            if (stretchability == 0 || tileSelectionManager == null)
            {
                //MelonLogger.Msg($"[Llong:Stretchability] Skipping stretch wobbly check (0 stretchability or no selection manager)");
                // Run the normal logic
                return true;
            }

            List<Tile> selectedTiles = tileSelectionManager.GetTilesFromSelectedTiles();
            if (selectedTiles.Contains(__instance))
            {
                // This tile is selected. Determine its wobbliness based on its position in the word.
                Tile previousTile = null;
                int currentStretch = 0;
                for (int i = 0; i < selectedTiles.Count; i++)
                {
                    if (previousTile != null && previousTile.GetStringRepresentation(false) == selectedTiles[i].GetStringRepresentation(false))
                    {
                        // Repeat detected! Try to stretch.
                        if (currentStretch < stretchability)
                        {
                            currentStretch++;
                        }
                        else
                        {
                            currentStretch = 0;
                        }
                    }
                    else
                    {
                        // Non-repeat.
                        currentStretch = 0;
                    }
                    if (selectedTiles[i] == __instance)
                    {
                        // That's us!!
                        if (currentStretch > 0)
                        {
                            //MelonLogger.Msg($"[Llong:Stretchability] Setting mid-selected-word wobbliness for {__instance.GetStringRepresentation(false)}");
                            // Force the result to true
                            __result = true;
                            return false; // Skip normal logic
                        }
                        else
                        {
                            return true; // Do normal wobbly logic for this tile
                        }
                    }
                    previousTile = selectedTiles[i];
                }
            }
            else if (selectedTiles.Count == 0)
            {
                // Nothing is selected, so no stretching can occur
                return true;
            }
            else
            {
                string lastTileString = selectedTiles[^1].GetStringRepresentation(false);

                if (lastTileString != __instance.GetStringRepresentation(false))
                {
                    // The last tile of the selected word doesn't match this unselected tile. Do normal logic.
                    return true;
                }

                // Count backwards from the end of the selected word to see if this tile is stretch-eligible
                int repeatCount = 0;
                for (int i = selectedTiles.Count - 2; i >= 0; i--)
                {
                    if (selectedTiles[i].GetStringRepresentation(false) == lastTileString)
                    {
                        if (repeatCount == stretchability)
                        {
                            // We reached a repeat non-stretched letter.
                            // If we have a stretchability of 1, "seee"'s next E will be a stretched letter, but "seeee"'s next E won't be.
                            repeatCount = 0;
                        }
                        else
                        {
                            repeatCount++;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (repeatCount < stretchability)
                {
                    //MelonLogger.Msg($"[Llong:Stretchability] Setting unselected tile wobbliness for {__instance.GetStringRepresentation(false)}");
                    __result = true;
                    return false; // Skip normal logic
                }
                else
                {
                    return true; // Do normal logic
                }
            }

            return true; // Do normal logic
        }
    }
}
