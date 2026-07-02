using System;
using System.Collections.Generic;
using System.Text;

namespace Llong
{
    internal class CodeGraveyard
    {
        // These are things that I tried that didn't work. Behold and avoid!

        /*
        // This is a huge collection of things that were replaced by one patch to Assembly.GetTypes that obsoleted all of them.
        // Cursed Words will look for all defined Characters, Items, Achievements, etc. in its own assembly,
        // but won't notice them defined in the mod's assembly. These fixed them individually rather than all at once.
        [HarmonyPatch(typeof(SaveManager), "GetUnlockedItems")]
        static class SaveManager_GetUnlockedItems_Patch
        {
            static void Prefix()
            {
                MelonLogger.Msg("[Llong] Backfilling crown achievements");
                // Get the highest run of Llong
                Dictionary<Type, int> ascensions = SaveManager.GetHighestCompletedAscensions();
                if (!ascensions.ContainsKey(typeof(LlongCharacter)) || ascensions[typeof(LlongCharacter)] < 0)
                {
                    // Llong hasn't been loaded.
                    return;
                }

                // Create instances of all the Llong crown achievements
                List<Type> llongAchievementTypes = Core.GetLlongAchievementTypes();
                List<Achievement> llongAchievementInstances = [];
                foreach (var achievementType in llongAchievementTypes)
                {
                    llongAchievementInstances.Add(Activator.CreateInstance(achievementType) as Achievement);
                }

                // Add the instances to the save if they should already be unlocked
                for (int i = 0; i < ascensions[typeof(LlongCharacter)]; i++)
                {
                    foreach (var achievement in llongAchievementInstances)
                    {
                        if (achievement.CrownRewardLevel == i)
                        {
                            SaveManager.AddAchievement(achievement);
                        }
                    }
                }
            }
        }

        
        ...
        static public List<Type> GetLlongAchievementTypes()
        {
            return AddLlongAchievementTypes([]);
        }

        static public List<Type> GetLlongItemTypes()
        {
            return new List<Type>([
                typeof(Unlocks.Atria),
                typeof(Unlocks.Bookkeeper),
                typeof(Unlocks.ChosenOne),
                typeof(Unlocks.CommonAccordion),
                typeof(Unlocks.CleverDisguise),
                typeof(Unlocks.FigureEightKnot),
                typeof(Unlocks.SnoozedAlarmClock),
                typeof(Unlocks.YoYo)
            ]);
        }

        static public List<Type> AddLlongAchievementTypes(List<Type> list)
        {
            list.Add(typeof(Unlocks.AtriaUnlock));
            list.Add(typeof(Unlocks.BookkeeperUnlock));
            list.Add(typeof(Unlocks.ChosenOneUnlock));
            list.Add(typeof(Unlocks.ClassicAccordionUnlock));
            list.Add(typeof(Unlocks.CleverDisguiseUnlock));
            list.Add(typeof(Unlocks.FigureEightKnotUnlock));
            list.Add(typeof(Unlocks.SnoozedAlarmClockUnlock));
            list.Add(typeof(Unlocks.YoYoUnlock));
            return list;
        }
        ...

        [HarmonyPatch(typeof(Achievements), "SetAllUnearnedAchievements")]
        static class Achievements_SetAllUnearnedAchievements_Patch
        {
            static void Postfix(List<Achievement> ____allUnearnedAchievements)
            {
                MelonLogger.Msg("[Llong] Creating unearned achievements");
                List<Type> earnedAchievementTypes =
                    [.. (from achievement in SaveManager.GetAchievements()
                         select achievement.AchievementType)];

                List<Type> llongAchievementTypes = Core.GetLlongAchievementTypes();
                foreach (var achievementType in llongAchievementTypes)
                {
                    if (!earnedAchievementTypes.Contains(achievementType))
                    {
                        ____allUnearnedAchievements.Add(Activator.CreateInstance(achievementType) as Achievement);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ItemPools), "PopulatePools")]
        static class ItemPools_PopulatePools_Patch
        {
            static void Postfix()
            {
                MelonLogger.Msg("[Llong] Adding items to the pool");
                ItemPools.AddItemsToPools(Core.GetLlongItemTypes());
            }
        }

        [HarmonyPatch(typeof(CollectionCanvasController), "GetAllItems")]
        static class CollectionCanvasController_GetAllItems
        {
            static void Postfix(List<Item> __result)
            {
                MelonLogger.Msg("[Llong] Adding items to the collection screen");
                Core.GetLlongItemTypes().ForEach(itemType =>
                {
                    __result.Add(Activator.CreateInstance(itemType) as Item);
                });
            }
        }

        // All of Llong's stamps/stickers must be unlocked
        [HarmonyPatch(typeof(GameStatics), "GetItemsRequiringUnlock")]
        static class GameStatics_GetItemsRequiringUnlock_Patch
        {
            static void Postfix(List<Type> __result)
            {
                __result.AddRange(Core.GetLlongItemTypes());
            }
        }
        */

        // This is the same category as the above (obsoleted by Assembly_GetTypes_Patch)
        // Transpilation was extra annoying:
        // - It crashes the game on close even if the transpiler does nothing and returns the OG instructions
        // - There's some dependency weirdness with ILGenerator, mscorlib, and System.Reflection.Emit
        // - It's difficult to read and understand vs a standard patch
        //[HarmonyPatch(typeof(SaveManager), "GetUnlockedItems")]
        //static class SaveManager_GetUnlockedItems_Patch
        //{
        //    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions /*, ILGenerator generator*/)
        //    {
        //        // Print the original version of everything
        //        foreach (var item in instructions)
        //        {
        //            MelonLogger.Msg(item.ToString());
        //        }

        //        CodeMatcher matcher = new();
        //        matcher.Insert(instructions);
        //        return matcher
        //            //.MatchStartForward(CodeMatch.Calls(() => default(SaveManager).GetHighestCompletedAscensions(default)))
        //            .MatchStartForward(new CodeMatch(CodeInstruction.Call(typeof(SaveManager), "GetHighestCompletedAscensions")))
        //            .ThrowIfInvalid("Could not find call to SaveManager.GetHighestCompletedAscensions")
        //            .InsertAndAdvance(
        //                new CodeInstruction(OpCodes.Ldloc_1),
        //                CodeInstruction.Call(typeof(Core), "AddLlongAchievements", [typeof(List<Achievement>)])
        //            ).InstructionEnumeration();
        //    }
        //}



        // Was used for debugging custom animations
        /*
        [HarmonyPatch(typeof(CharacterSelectVisualController), "PopulateElements")]
        static class CharacterSelectVisualController_PopulateElements_Patch
        {
            static void Postfix(Animator ____characterAnimator, Character character)
            {
                ____characterAnimator.logWarnings = true;
                RuntimeAnimatorController rac = ____characterAnimator.runtimeAnimatorController;
                if (rac != null)
                {
                    if (rac.IsUnityNull())
                    {
                        MelonLogger.Msg($"Populated controller IS NULL");
                    }
                    else
                    {
                        MelonLogger.Msg($"Populated controller: {rac.name}");
                        MelonLogger.Msg($"For character: {character.GetName()}");
                        foreach (var ac in rac.animationClips)
                        {
                            MelonLogger.Msg(ac.name);
                        }
                        foreach (var sprite in ____characterAnimator.GetComponents<Sprite>())
                        {
                            MelonLogger.Msg(sprite.name);
                        }
                    }
                }
                else
                {
                    MelonLogger.Msg("RAC IS NULL");
                    MelonLogger.Msg(Core.LlongAnimator);
                }
            }
        }
        */


        // I was on a patching spree and then remembered that I could just override these Character functions in LlongCharacter lmao
        /*
        [HarmonyPatch(typeof(Character), "GetRuntimeAnimatorController")]
        static class Character_GetRuntimeAnimatorController_Patch
        {
            //static bool HasLoaded = false;
            //static void Prefix(Character __instance)
            //{
            //    if (__instance.GetName() == "Llong" && !HasLoaded)
            //    {
            //        HasLoaded = true;
            //        AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine([Application.dataPath, "../Mods/llongAssets"]));
            //        if (assetBundle == null)
            //        {
            //            MelonLogger.Msg($"[Llong] We couldn't find the bundle from {Path.Combine([Application.dataPath, "../Mods/llongAssets"]).ToString()}");
            //        }
            //        else
            //        {
            //            MelonLogger.Msg(string.Join(" ||| ", assetBundle.GetAllAssetNames()));
            //        }
            //    }
            //}

            static void Postfix(Character __instance, ref RuntimeAnimatorController __result)
            {
                MelonLogger.Msg($"Character_GetRuntimeAnimatorController_Patch instance: {__instance.GetName()}");

                if (__result)
                {
                    MelonLogger.Msg($"Character_GetRuntimeAnimatorController_Patch result in: {__result.ToString()}");
                }
                else
                {
                    MelonLogger.Msg("No result, throwing Llong at it");

                    //__result = Core.LlongAssetBundle.LoadAsset<RuntimeAnimatorController>("assets/resources/Piero/Players/" + __instance.GetArtFileName() + "/" + __instance.GetArtFileName());
                    __result = Core.LlongAnimator;
                    Character.CharacterRuntimeAnimators[typeof(LlongCharacter)] = Core.LlongAnimator;
                    foreach (var item in __result.animationClips)
                    {
                        MelonLogger.Msg(item.ToString());
                    }
                    MelonLogger.Msg(__result.name);
                }
                MelonLogger.Msg($"Character_GetRuntimeAnimatorController_Patch result out: {__result.ToString()}");
            }
        }

        [HarmonyPatch(typeof(Character), "GetCharacterSprite")]
        static class Character_GetCharacterSprite_Patch
        {
            static bool Prefix(Character __instance, out Sprite __result)
            {
                if (__instance.GetArtFileName() == "llong")
                {
                    //__result = Core.LlongAssetBundle.LoadAsset<Sprite>("assets/resources/Piero/Players/" + __instance.GetArtFileName() + "/" + __instance.GetArtFileName());
                    __result = Core.LlongSprite;
                    Character.CharacterSprites[typeof(LlongCharacter)] = Core.LlongSprite;
                    return false;
                }
                __result = null;
                return true;
            }
        }


        // Part of Neptune6866's character starter. Not sure when to use it, though
        public static Sprite LoadEmbeddedSprite(string resourceName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                MelonLogger.Warning($"Embedded sprite {resourceName} not found!");
                return null;
            }

            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            tex.LoadImage(bytes);
            tex.Apply();

            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }
        */

        // Useful for debugging specific items in runs
        /*
        [HarmonyPatch(typeof(ItemPools), "GetRandomBuildBiasedStamp", [typeof(List<Type>)])]
        static class ItemPools_GetRandomBuildBiasedStamp_Patch
        {
            static bool Prefix(out Item __result, List<Type> unavailableItemTypes)
            {
                List<Type> stampsToTest = [ typeof(SnoozedAlarmClock), typeof(Bookkeeper), typeof(FigureEightKnot), typeof(CommonAccordion)];

                int randomChoice = UnityEngine.Random.Range(0, stampsToTest.Count());
                if (!unavailableItemTypes.Contains(stampsToTest[randomChoice]))
                {
                    __result = Activator.CreateInstance(stampsToTest[randomChoice]) as Item;
                    MelonLogger.Msg($"[Llong] Stocking item: {__result.Name}");
                    return false;
                }
                else
                {
                    MelonLogger.Msg($"[Llong] Couldn't stock: {stampsToTest[randomChoice].Name}");
                }

                __result = null;
                return true;
            }
        }


        [HarmonyPatch(typeof(ItemPools), "GetRandomBuildBiasedSticker", [ typeof(List<Type>) ])]
        static class ItemPools_GetRandomBuildBiasedSticker_Patch
        {
            static bool Prefix(out Item __result, List<Type> unavailableItemTypes)
            {
                List<Type> stickersToTest = [typeof(CleverDisguise), typeof(Atria), typeof(ChosenOne), typeof(YoYo)];

                int randomChoice = UnityEngine.Random.Range(0, stickersToTest.Count());
                if (!unavailableItemTypes.Contains(stickersToTest[randomChoice]))
                {
                    __result = Activator.CreateInstance(stickersToTest[randomChoice]) as Item;
                    MelonLogger.Msg($"[Llong] Stocking item: {__result.Name}");
                    return false;
                }
                else
                {
                    MelonLogger.Msg($"[Llong] Couldn't stock: {stickersToTest[randomChoice].Name}");
                }

                __result = null;
                return true;
            }
        }
        */

        // This was the old way of adding Llong to multiplayer, but multiplayer stores/accesses custom characters differently now.
        /*
        [HarmonyPatch(typeof(RunProgress), "GetCurrentBossDraft")]
        public static class RunProgress_GetCurrentBossDraft_Patch
        {
            public static bool Prefix(RunProgress __instance, ref BossDraft __result)
            {
                Type ReceivedInfo = Type.GetType("CWMultiplayer.ReceivedInfo");
                if (ReceivedInfo == null)
                {
                    // No multiplayer mod. Continue as normal!
                    return true;
                }

                bool hasOpponent = (bool)ReceivedInfo.GetField("hasOpponent").GetValue(null);

                if (!hasOpponent)
                {
                    // The multiplayer mod is loaded, but this isn't a multiplayer match.
                    return true;
                }

                Character foeCharacter = (Character)ReceivedInfo.GetField("foeCharacter").GetValue(null);

                if (foeCharacter == null || foeCharacter.GetType() != typeof(LlongCharacter))
                {
                    // The multiplayer mod is active, but Llong isn't the opponent
                    return true;
                }

                // If we're here, we're facing Llong
                BossModifier bossModifier = new LlongBoss();
                __result = __instance.BossDrafts[__instance.CurrentStage - 1];
                __result.AvailableBosses.Clear();
                __result.AvailableBosses.Add(bossModifier);
                __result.AvailableBosses.Add(bossModifier);
                return false;
            }
        }
        */
        /*
        BossModifier bossModifier = new LlongBoss();
        __result = __instance.BossDrafts[__instance.CurrentStage - 1];
        __result.AvailableBosses.Clear();
        __result.AvailableBosses.Add(bossModifier);
        __result.AvailableBosses.Add(bossModifier);
        return false;
        */
    }
}
