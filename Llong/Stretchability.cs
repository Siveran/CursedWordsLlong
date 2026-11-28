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

        private const int MAX_STRETCHABILITY_CACHE = 16;
        private static Dictionary<InventoryCache, int> StretchabilityCache = [];
        private static readonly List<InventoryCache> InventoryCaches = [];
        private static int NextIndexToSet = 0; // increments 0 to (MAX_STRETCHABILITY_CACHE-1)

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

        // Get the unstretched word (babbyy -> baby)
        static public List<Tile> ApplyStretching(List<Tile> tiles, int stretchability)
        {
            if (stretchability == 0)
            {
                return tiles;
            }

            List<Tile> result = [];
            Tile previousUppercaseTile = null;
            Tile previousLowercaseTile = null;
            int currentUppercaseStretch = 0;
            int currentLowercaseStretch = 0;
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].IsLowerCase)
                {
                    if (previousLowercaseTile != null && IsRepeat(previousLowercaseTile, tiles[i]))
                    {
                        // Repeat detected! Try to stretch.
                        if (currentLowercaseStretch < stretchability)
                        {
                            currentLowercaseStretch++;
                        }
                        else
                        {
                            currentLowercaseStretch = 0;
                            result.Add(tiles[i]);
                        }
                    }
                    else
                    {
                        // Non-repeat.
                        currentLowercaseStretch = 0;
                        result.Add(tiles[i]);
                    }
                    previousLowercaseTile = tiles[i];
                }
                else
                {
                    if (previousUppercaseTile != null && IsRepeat(previousUppercaseTile, tiles[i]))
                    {
                        // Repeat detected! Try to stretch.
                        if (currentUppercaseStretch < stretchability)
                        {
                            currentUppercaseStretch++;
                        }
                        else
                        {
                            currentUppercaseStretch = 0;
                            result.Add(tiles[i]);
                        }
                    }
                    else
                    {
                        // Non-repeat.
                        currentUppercaseStretch = 0;
                        result.Add(tiles[i]);
                    }
                    previousUppercaseTile = tiles[i];
                }
            }
            return result;
        }

        // Gets the full length of tiles involved in a submitted word. Used for Honeypot + pertinent words. ONLY works for fully uppercase/lowercase words.
        // Given tiles ABBCCC and stretchability 1, would return 1, 3, 5, and 6 for 1, 2, 3, and 4 respectively
        static public int GetStretchedLength(List<Tile> tiles, int stretchability, int unstretchedLength)
        {
            string previousTileString = null;
            int unstretchedLengthAccountedFor = 0;
            int currentStretch = 0;
            int stretchedLength = 0;
            for (int i = 0; i < tiles.Count; i++)
            {
                string currentTileString = GetCaseSensitiveStringRepresentation(tiles[i]);
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
            //Core.DebugLog($"Tiles selected: {tiles.Count()} | Stretchability: {stretchability} | Unstretched length: {unstretchedLength} | Stretched length: {stretchedLength}");
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

        static public bool IsRepeat(Tile a, Tile b)
        {
            return a.GetStringRepresentation(false) == b.GetStringRepresentation(false) && a.IsLowerCase == b.IsLowerCase;
        }

        static public string GetCaseSensitiveStringRepresentation(Tile tile)
        {
            return tile.IsLowerCase ? tile.GetStringRepresentation(false).ToLower() : tile.GetStringRepresentation(false).ToUpper();
        }
    }

    // Normally, InventoryCache is used to check the items currently applying to the word submission.
    // That's how things like Red Envelope and Microscope work. Unfortunately, we can't just plop a new field into InventoryCache,
    // so we need to roll our own.

    // Hook into InventoryCache's constructor so we count stretchability at the right time
    [HarmonyPatch(typeof(InventoryCache))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch([typeof(IEnumerable<Tile>), typeof(GridData)])]
    class InventoryCache_CTOR_Patch
    {
        static void Postfix(IEnumerable<Tile> currentWordTiles, InventoryCache __instance)
        {
            // Get all items in inventory + submitted word
            List<Item> allItems = GameStatics.GetPlayer().GetAllItems(false);
            foreach (Tile tile in currentWordTiles)
            {
                if (tile.GetGlyphType() == GlyphType.ScatteredItem)
                {
                    allItems.Add(tile.ScatteredItem);
                }
            }
            // Get items in Snapshot, Frankenstein, Random Access Memory, and Ice Cube
            allItems = Util.UnpackItems(allItems);

            var stretchability = Stretchability.GetStretchabilityFromItems(allItems);

            Stretchability.CacheStretchability(__instance, stretchability);
        }
    }

    [HarmonyPatch(typeof(Vocabulary), "GetValidUpperAndLowerCaseWordsFromTiles")]
    class Vocabulary_GetValidUpperAndLowerCaseWordsFromTiles_Patch
    {
        class GetValidUpperAndLowerCaseWordsFromTiles_State
        {
            public List<Tile> AllTiles;
            public int Stretchability;
        }

        static void Prefix(ref IEnumerable<Tile> tiles, GridData gridData, ref List<BossModifier> bossModifiers, out GetValidUpperAndLowerCaseWordsFromTiles_State __state)
        {
            InventoryCache inventoryCache = new InventoryCache(tiles, gridData);
            __state = new GetValidUpperAndLowerCaseWordsFromTiles_State
            {
                AllTiles = [.. tiles],
                Stretchability = Stretchability.GetStretchability(inventoryCache)
            };

            // Check to make sure the lowercase and uppercase words pass min/max word length boss modifiers (snake/wolf)
            // If they don't, leave the word in its stretched state, so they properly run into length problems on validation.
            // If they do, remove snake/wolf from the modifiers, as applying stretching will mess up later word length validation.
            int lowerCount = tiles.Where((Tile tile) => tile.IsLowerCase).Count();
            int upperCount = tiles.Count() - lowerCount;
            bool wordLengthsAreValid = true;
            if (lowerCount > 0 && !Vocabulary.IsWordLengthValid(lowerCount, bossModifiers))
            {
                wordLengthsAreValid = false;
            }
            else if (upperCount > 0 && !Vocabulary.IsWordLengthValid(upperCount, bossModifiers))
            {
                wordLengthsAreValid = false;
            }
            if (wordLengthsAreValid)
            {
                tiles = Stretchability.ApplyStretching(__state.AllTiles, __state.Stretchability);
                bossModifiers = [.. bossModifiers.Where(bossModifier => bossModifier is not MinWordLength && bossModifier is not MaxWordLength)];
            }
        }

        static void Postfix(ref SubmittedWords __result, GetValidUpperAndLowerCaseWordsFromTiles_State __state)
        {
            if (__state.Stretchability == 0 || __result == null)
            {
                return;
            }
            int uppercaseHead = 0;
            int lowercaseHead = 0;
            int currentUppercaseStretch = 0;
            int currentLowercaseStretch = 0;
            Tile previousUppercaseTile = null;
            Tile previousLowercaseTile = null;
            for (int head = 0; head < __state.AllTiles.Count(); head++)
            {
                if (__state.AllTiles[head].IsLowerCase)
                {
                    if (previousLowercaseTile != null && Stretchability.IsRepeat(previousLowercaseTile, __state.AllTiles[head]) && currentLowercaseStretch < __state.Stretchability)
                    {
                        currentLowercaseStretch++;
                        // Add this tile to the lowercase word's tiles
                        //MelonLogger.Msg("[Llong] Adding lowercase tile: " + __state.AllTiles[head].GetStringRepresentation(false));
                        __result.LowercaseWordTiles[0].Insert(lowercaseHead, __state.AllTiles[head]);
                    }
                    else
                    {
                        currentLowercaseStretch = 0;
                    }
                    lowercaseHead++;
                    previousLowercaseTile = __state.AllTiles[head];
                }
                else
                {
                    if (previousUppercaseTile != null && Stretchability.IsRepeat(previousUppercaseTile, __state.AllTiles[head]) && currentUppercaseStretch < __state.Stretchability)
                    {
                        currentUppercaseStretch++;
                        // Add this tile to the uppercase word's tiles
                        //MelonLogger.Msg("[Llong] Adding uppercase tile: " + __state.AllTiles[head].GetStringRepresentation(false));
                        __result.WordTiles[0].Insert(uppercaseHead, __state.AllTiles[head]);
                    }
                    else
                    {
                        currentUppercaseStretch = 0;
                    }
                    uppercaseHead++;
                    previousUppercaseTile = __state.AllTiles[head];
                }
            }
        }
    }

    // Honeypot is scary
    // It should never cut submissions on letters mid-stretch. As an example, if you have Yo-Yo and Honeypot,
    // submitting "MAGICIANSSHAT" should result in "MAGICIANS HAT" OR "MAGICIAN SHAT" rather than "MAGICIANS SHAT"
    // If you have Yo-Yo, Honeypot, AND Microphone, "MAGICIANSSHAT" should ONLY result in "MAGICIANS HAT"!
    [HarmonyPatch(typeof(Vocabulary), "GetConcatenatedValidWordsFromTiles")]
    class Vocabulary_GetConcatenatedValidWordsFromTiles_Patch
    {
        public static bool Prefix(List<Tile> tiles, GridData gridData, List<BossModifier> bossModifiers, out SubmittedWords __result)
        {
            //MelonLogger.Msg("[Llong] Vocabulary_GetConcatenatedValidWordsFromTiles_Patch");
            InventoryCache inventoryCache = new InventoryCache(tiles, gridData);
            if (Stretchability.GetStretchability(inventoryCache) == 0)
            {
                //MelonLogger.Msg("[Llong] 0 stretchability");
                // There's no stretchability in play. Just do the normal logic.
                __result = null;
                return true;
            }
            inventoryCache.PertinentAccumulatorWords.Clear();
            List<string> firstWords = inventoryCache.PertinentAccumulatorPairs.Select(pair => pair[0]).ToList();
            List<string> secondWords = inventoryCache.PertinentAccumulatorPairs.Select(pair => pair[1]).ToList();
            int stretchability = Stretchability.GetStretchability(inventoryCache);

            // 1. Prioritize getting a pair of words that have an effect. This ONLY works on fully uppercase/lowercase words!
            MethodInfo tryGetAccumulatorWordFromTiles = AccessTools.Method(typeof(Vocabulary), "TryGetAccumulatorWordFromTiles", [typeof(InventoryCache), typeof(IEnumerable<Tile>), typeof(GridData)]);
            foreach (string firstWord in firstWords)
            {
                inventoryCache.PertinentAccumulatorWords.Clear();
                inventoryCache.PertinentAccumulatorWords.Add(firstWord);
                int stretchedLength = Stretchability.GetStretchedLength(tiles, stretchability, firstWord.Length);
                List<Tile> tilePortion = [.. tiles.Take(stretchedLength)];
                if (tryGetAccumulatorWordFromTiles.Invoke(null, [inventoryCache, Stretchability.ApplyStretching(tilePortion, stretchability), gridData]) is string firstFoundWord)
                {
                    int index = firstWords.IndexOf(firstWord);
                    inventoryCache.PertinentAccumulatorWords.Clear();
                    inventoryCache.PertinentAccumulatorWords.Add(secondWords[index]);
                    tilePortion = [.. tiles.Skip(stretchedLength)];
                    if (tryGetAccumulatorWordFromTiles.Invoke(null, [inventoryCache, Stretchability.ApplyStretching(tilePortion, stretchability), gridData]) is string secondFoundWord)
                    {
                        //MelonLogger.Msg("[Llong] Accumulator pair!");
                        __result = new SubmittedWords
                                {
                                        Words = new List<string> { firstFoundWord, secondFoundWord },
                                        WordTiles = new List<List<Tile>>
                                    {
                                        tiles.Take(stretchedLength).ToList<Tile>(),
                                        tiles.Skip(stretchedLength).ToList<Tile>()
                                    }
                                };
                        return false;
                    }
                }
            }
            //MelonLogger.Msg("[Llong] No accumulator pair");

            // 2. Next, prioritize getting one (or upper+lowercase) word(s) (note that this follows vanilla logic. As of July 1, 2026, I intentionally don't add single accumulator words back to the pool here)
            SubmittedWords validUpperAndLowerCaseWordsFromTiles = Vocabulary.GetValidUpperAndLowerCaseWordsFromTiles(tiles, bossModifiers, gridData, inventoryCache);
            if (validUpperAndLowerCaseWordsFromTiles != null)
            {
                //MelonLogger.Msg("[Llong] Single word!");
                __result = validUpperAndLowerCaseWordsFromTiles;
                return false;
            }
            inventoryCache.PertinentAccumulatorWords.Clear();
            //MelonLogger.Msg("[Llong] No single word");

            // 3. Next, get ANY two words
            // Because you can get situations where a word is threaded between lowercase and uppercase stretched tiles, we either:
            // A: Need to ONLY cut the word where BOTH stretches are done (a little unfortunate)
            // B: Allow cutting the word ANYWHERE (chaos in the streets)
            // I do A.
            int currentUppercaseStretch = 0;
            int currentLowercaseStretch = 0;
            Tile previousUppercaseTile = null;
            Tile previousLowercaseTile = null;
            for (int i = 0; i < tiles.Count(); i++)
            {
                bool isMidUppercaseStretch = false;
                bool isMidLowercaseStretch = false;
                if (tiles[i].IsLowerCase)
                {
                    if (previousLowercaseTile != null && Stretchability.IsRepeat(previousLowercaseTile, tiles[i]))
                    {
                        // Repeat detected! Try to stretch.
                        if (currentLowercaseStretch < stretchability)
                        {
                            isMidLowercaseStretch = true;
                            currentLowercaseStretch++;
                        }
                        else
                        {
                            currentLowercaseStretch = 0;
                        }
                    }
                    else
                    {
                        // Non-repeat.
                        currentLowercaseStretch = 0;
                    }

                    // Look ahead to the next uppercase tile. If it's a repeat of the previous uppercase tile, prevent cuts.
                    if (!isMidLowercaseStretch && currentUppercaseStretch < stretchability && previousUppercaseTile != null)
                    {
                        for (int j = i + 1; j < tiles.Count(); j++)
                        {
                            if (!tiles[j].IsLowerCase)
                            {
                                isMidUppercaseStretch = Stretchability.IsRepeat(previousUppercaseTile, tiles[j]);
                                break;
                            }
                        }
                    }
                    previousLowercaseTile = tiles[i];
                }
                else
                {
                    if (previousUppercaseTile != null && Stretchability.IsRepeat(previousUppercaseTile, tiles[i]))
                    {
                        // Repeat detected! Try to stretch.
                        if (currentUppercaseStretch < stretchability)
                        {
                            isMidUppercaseStretch = true;
                            currentUppercaseStretch++;
                        }
                        else
                        {
                            currentUppercaseStretch = 0;
                        }
                    }
                    else
                    {
                        // Non-repeat.
                        currentUppercaseStretch = 0;
                    }

                    // Look ahead to the next uppercase tile. If it's a repeat of the previous uppercase tile, prevent cuts.
                    if (!isMidUppercaseStretch && currentLowercaseStretch < stretchability && previousLowercaseTile != null)
                    {
                        for (int j = i + 1; j < tiles.Count(); j++)
                        {
                            if (tiles[j].IsLowerCase)
                            {
                                isMidLowercaseStretch = Stretchability.IsRepeat(previousLowercaseTile, tiles[j]);
                                break;
                            }
                        }
                    }

                    previousUppercaseTile = tiles[i];
                }

                if (i > 0 && !isMidUppercaseStretch && !isMidLowercaseStretch)
                {
                    // There's no stretch happening, so we can cut here!
                    int cutIndex = i;
                    List<Tile> tilePortion = [.. tiles.Take(cutIndex)];
                    SubmittedWords firstValidWords = Vocabulary.GetValidUpperAndLowerCaseWordsFromTiles(tilePortion, bossModifiers, gridData, inventoryCache);
                    if (firstValidWords != null)
                    {
                        tilePortion = [.. tiles.Skip(cutIndex)];
                        SubmittedWords secondValidWords = Vocabulary.GetValidUpperAndLowerCaseWordsFromTiles(tilePortion, bossModifiers, gridData, inventoryCache);
                        if (secondValidWords != null)
                        {
                            //MelonLogger.Msg("[Llong] Honeypot!");
                            __result = new SubmittedWords
                            {
                                Words = [.. firstValidWords.Words.Concat(secondValidWords.Words)],
                                WordTiles = [.. firstValidWords.WordTiles.Concat(secondValidWords.WordTiles)],
                                LowercaseWords = [.. firstValidWords.LowercaseWords.Concat(secondValidWords.LowercaseWords)],
                                LowercaseWordTiles = [.. firstValidWords.LowercaseWordTiles.Concat(secondValidWords.LowercaseWordTiles)]
                            };

                            //MelonLogger.Msg("[Llong] Words: " + __result.Words.ToString() + " | Lowercase Words: " + __result.Words.ToString());
                            return false;
                        }
                    }
                }
            }
            __result = null;
            //MelonLogger.Msg("[Llong] No honeypot");
            return false;
        }
    }

    /*
    [HarmonyPatch(typeof(Vocabulary), "GetValidWordFromTiles")]
    class Vocabulary_GetValidWordFromTiles_Patch
    {
        static void Prefix(Tile tile, TileSelection tileSelection)
        {

        }
    }
    */

    [HarmonyPatch(typeof(WordLetterController), "Populate")]
    class WordLetterController_Populate_Patch
    {
        static void Prefix(Tile tile, TileSelection tileSelection)
        {
            // TODO: Check for changes here
            tileSelection.IsWobbly = tile.IsDisplayingAsVariableLetter();
        }
    }

    // Stretched selected tiles should show as wobbly, and all current stretch-eligible tiles should show as wobbly
    [HarmonyPatch(typeof(Tile), "IsDisplayingAsVariableLetter")]
    class Tile_IsDisplayingAsVariableLetter_Patch
    {
        static bool Prefix(ref bool __result, Tile __instance, TileSelectionManager tileSelectionManager = null, List<Tile> wordTiles = null)
        {
            //MelonLogger.Msg("[Llong] IsDisplayingAsVariableLetter");
            if (__instance.MyGlyphType == GlyphType.ScatteredItem)
            {
                // Items don't like wobbling :( Just use normal logic.
                return true;
            }

            // Get items from inventory + tiles
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
            allItems = Util.UnpackItems(allItems);

            // TODO: Try tacking on isCrossedOut and see what happens?
            var stretchability = Stretchability.GetStretchabilityFromItems(allItems);

            if (stretchability == 0 || tileSelectionManager == null)
            {
                // Run the normal logic
                return true;
            }

            List<Tile> selectedTiles = (wordTiles != null && wordTiles.Count() > 0) ? wordTiles : tileSelectionManager.GetTilesFromSelectedTiles();
            if (selectedTiles.Contains(__instance))
            {
                // This tile is selected. Determine its wobbliness based on its position in the word.
                Tile previousUppercaseTile = null;
                Tile previousLowercaseTile = null;
                int currentUppercaseStretch = 0;
                int currentLowercaseStretch = 0;
                for (int i = 0; i < selectedTiles.Count; i++)
                {
                    if (selectedTiles[i].IsLowerCase)
                    {
                        if (previousLowercaseTile != null && Stretchability.IsRepeat(previousLowercaseTile, selectedTiles[i]))
                        {
                            // Repeat detected! Try to stretch.
                            if (currentLowercaseStretch < stretchability)
                            {
                                currentLowercaseStretch++;
                            }
                            else
                            {
                                currentLowercaseStretch = 0;
                            }
                        }
                        else
                        {
                            // Non-repeat.
                            currentLowercaseStretch = 0;
                        }
                        if (selectedTiles[i] == __instance)
                        {
                            // That's us!!
                            if (currentLowercaseStretch > 0)
                            {
                                // Force the result to true
                                __result = true;
                                return false; // Skip normal logic
                            }
                            else
                            {
                                return true; // Do normal wobbly logic for this tile
                            }
                        }
                        previousLowercaseTile = selectedTiles[i];
                    }
                    else
                    {
                        if (previousUppercaseTile != null && Stretchability.IsRepeat(previousUppercaseTile, selectedTiles[i]))
                        {
                            // Repeat detected! Try to stretch.
                            if (currentUppercaseStretch < stretchability)
                            {
                                currentUppercaseStretch++;
                            }
                            else
                            {
                                currentUppercaseStretch = 0;
                            }
                        }
                        else
                        {
                            // Non-repeat.
                            currentUppercaseStretch = 0;
                        }
                        if (selectedTiles[i] == __instance)
                        {
                            // That's us!!
                            if (currentUppercaseStretch > 0)
                            {
                                // Force the result to true
                                __result = true;
                                return false; // Skip normal logic
                            }
                            else
                            {
                                return true; // Do normal wobbly logic for this tile
                            }
                        }
                        previousUppercaseTile = selectedTiles[i];
                    }
                }
            }
            else if (selectedTiles.Count == 0)
            {
                // Nothing is selected, so no stretching can occur
                return true;
            }
            else
            {
                string lastMatchingCaseTileString = "";
                bool isLowerCase = __instance.IsLowerCase;

                for (int tileIndex = 0; tileIndex < selectedTiles.Count(); tileIndex++)
                {
                    if (selectedTiles[tileIndex].IsLowerCase == isLowerCase)
                    {
                        lastMatchingCaseTileString = Stretchability.GetCaseSensitiveStringRepresentation(selectedTiles[tileIndex]);
                    }
                }

                if (lastMatchingCaseTileString != Stretchability.GetCaseSensitiveStringRepresentation(__instance))
                {
                    // The last matching-case tile of the selected word doesn't match this unselected tile. Do normal logic.
                    return true;
                }

                // Count backwards from the end of the selected word to see if this tile is stretch-eligible
                int repeatCount = -1; // EEE
                for (int i = selectedTiles.Count - 1; i >= 0; i--)
                {
                    // Skip over opposing case tiles
                    if (selectedTiles[i].IsLowerCase != isLowerCase)
                    {
                        continue;
                    }

                    if (Stretchability.GetCaseSensitiveStringRepresentation(selectedTiles[i]) == lastMatchingCaseTileString)
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
