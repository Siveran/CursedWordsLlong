using FullSerializer;
using HarmonyLib;
using Llong.Unlocks;
using MelonLoader;
using nickeltin.SDF.Runtime;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Llong
{
    #region Sprites & Animation
    [HarmonyPatch(typeof(RunsHistoryController), "PopulateOverallStats")]
    static class RunsHistoryController_PopulateOverallStats_Patch
    {
        static void Postfix(Image ____mostPlayedCharacterPortrait)
        {
            if (SaveManager.GetMostPlayedCharacterType() == typeof(LlongCharacter))
            {
                ____mostPlayedCharacterPortrait.sprite = Core.LlongDialogueFlipSprite;
            }
        }
    }

    [HarmonyPatch(typeof(RunParentController), "GetPortraitSprite")]
    static class RunParentController_GetPortraitSprite_Patch
    {
        static bool Prefix(out SDFSpriteMetadataAsset __result, Character c)
        {
            if (c.GetName() == "Llong" && Core.LlongDialogueFlipSdf)
            {
                __result = Core.LlongDialogueFlipSdf;
                return false;
            }
            __result = null;
            return true;
        }
    }

    [HarmonyPatch(typeof(DialogueUtility), "GetEmotionSprite")]
    static class DialogueUtility_GetEmotionSprite_Patch
    {
        static bool Prefix(out Sprite __result, Emotions emotion)
        {
            if (emotion == Core.EmotionLlongFlip)
            {
                __result = Core.LlongDialogueFlipSprite;
                return false;
            }
            __result = null;
            return true;
        }
    }

    [HarmonyPatch(typeof(ContinueRunDetails), "OnEnable")]
    static class ContinueRunDetails_OnEnable_Patch
    {
        static void Postfix(Image ____characterImage)
        {
            // If the save has 0 as the emotion, it's PROBABLY because it's supposed to be Llong but he's not in the enum
            if (SaveManager.GetCurrentRun().MyCharacter.MyEmotion == (Emotions)0)
            {
                ____characterImage.sprite = Core.LlongDialogueFlipSprite;
            }
        }
    }

    [HarmonyPatch(typeof(DialogueUtility), "GetEmotionSDFSprite")]
    static class DialogueUtility_GetEmotionSDFSprite_Patch
    {
        static bool Prefix(out SDFSpriteMetadataAsset __result, Emotions emotion)
        {
            if (emotion == Core.EmotionLlongFlip)
            {
                __result = Core.LlongDialogueFlipSdf;
                return false;
            }
            __result = null;
            return true;
        }
    }

    [HarmonyPatch(typeof(ItemSpriteData), "GetSDFSprite")]
    static class ItemSpriteData_GetSDFSprite_Patch
    {
        static void Prefix(ItemSpriteData __instance)
        {
            // I use BLACKHOLE all caps as a special identifier for custom items
            // Even if the assets can't be found via this mod, it'll fall back to the Black Hole items' assets
            if (__instance.AssetPath == "BLACKHOLE")
            {
                LlongItemData item = __instance as LlongItemData;
                if (item.TrueSDF)
                {
                    item.ItemSDFSprite = item.TrueSDF;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ItemSpriteData), "GetSprite")]
    static class ItemSpriteData_GetSprite_Patch
    {
        static void Prefix(ItemSpriteData __instance)
        {
            // I use BLACKHOLE all caps as a special identifier for custom items
            // Even if the assets can't be found via this mod, it'll fall back to the Black Hole items' assets
            if (__instance.AssetPath == "BLACKHOLE")
            {
                LlongItemData item = __instance as LlongItemData;
                if (item.TrueSprite)
                {
                    item.ItemSprite = item.TrueSprite;
                }
            }
        }
    }
    #endregion

    #region Global Type List
    [HarmonyPatch(typeof(Assembly), "GetTypes", MethodType.Normal)]
    [HarmonyPatch([])]
    static class Assembly_GetTypes_Patch
    {
        static void Postfix(Assembly __instance, ref Type[] __result)
        {
            if (__instance.GetName().Name == "Assembly-CSharp")
            {
                __result = [.. __result, .. Assembly.GetAssembly(typeof(LlongCharacter)).GetTypes()];
            }
        }
    }

    [HarmonyPatch(typeof(Type), "GetType", MethodType.Normal)]
    [HarmonyPatch([typeof(string)])]
    static class Type_GetType_Patch
    {
        static bool Prefix(string typeName, ref Type __result)
        {
            if (typeName.StartsWith("Llong."))
            {
                __result = Core.GetType(typeName);
                return false;
            }

            // Do normal type lookup
            __result = null;
            return true;
        }
    }
    #endregion

    #region Inserting Llong as an Unlocked Character
    [HarmonyPatch(typeof(CharacterSelectController), "GetCharacters")]
    static class CharacterSelectController_GetCharacters_Patch
    {
        static void Postfix(Dictionary<Type, bool> ____characters)
        {
            Core.DebugLog("[Llong] Allowing Llong to be selected");
            ____characters[typeof(LlongCharacter)] = true;
        }
    }

    [HarmonyPatch(typeof(SaveManager), "LoadSaveFromFile")]
    static class SaveManager_LoadSaveFromFile_Patch
    {
        static void Postfix(SaveFile ____saveFile)
        {
            if (!____saveFile.CharacterHighestCompletedAscensions.ContainsKey(typeof(LlongCharacter)))
            {
                MelonLogger.Msg("[Llong] Added Llong to the save");
                SaveManager.AddCharacterToUnlockedCharacters(typeof(LlongCharacter));
            }
            else
            {
                MelonLogger.Msg("[Llong] Saw Llong in the save");
            }
        }
    }

    // The game crashes on stage 3+ if you're playing a character with no tooltips!
    [HarmonyPatch(typeof(TooltipController), "GetAppropriateToolTips")]
    static class TooltipController_GetAppropriateToolTips_Patch
    {
        static void Prefix(ref Dictionary<Type, List<ValueTuple<string, Emotions>>> ____characterSpecificToolTips)
        {
            Core.DebugLog("[Llong] Adding Llong tooltips");
            if (!____characterSpecificToolTips.ContainsKey(typeof(LlongCharacter)))
            {
                ____characterSpecificToolTips.Add(typeof(LlongCharacter), [
                    new ValueTuple<string, Emotions>("Stretched letters are ALWAYS ignored when checking the dictionary", Core.EmotionLlongFlip),
                    new ValueTuple<string, Emotions>("Numbers don't count stretched letters", Core.EmotionLlongFlip)
                ]);

                if (SaveManager.IsBulkUnlockUnlocked(typeof(LowercaseLettersUnlock)))
                {
                    ____characterSpecificToolTips[typeof(LlongCharacter)].Add(new ValueTuple<string, Emotions>("Lowercase letters won't interrupt a stretch of uppercase letters", Core.EmotionLlongFlip));
                    ____characterSpecificToolTips[typeof(LlongCharacter)].Add(new ValueTuple<string, Emotions>("Uppercase letters won't interrupt a stretch of lowercase letters", Core.EmotionLlongFlip));
                }
                if (SaveManager.IsItemUnlocked(typeof(Honeypot)))
                {
                    ____characterSpecificToolTips[typeof(LlongCharacter)].Add(new ValueTuple<string, Emotions>("Honeypot can't break a stretched letter into two", Core.EmotionLlongFlip));
                }
            }
        }
    }

    [HarmonyPatch(typeof(IDCard))]
    [HarmonyPatch(MethodType.Constructor)]
    static class IDCard_CTOR_Patch
    {
        static void Postfix(IDCard __instance)
        {
            Core.DebugLog("[Llong] Adding Llong's ID card");
            __instance.SpriteData.Add(new LlongItemData(ItemSpriteUsage.AvocadoMushy, "LlongIDCard"));
            __instance.CharacterToSpriteUsage.Add(typeof(LlongCharacter), ItemSpriteUsage.AvocadoMushy);
        }
    }
    #endregion

    #region Fallback Saves (if Llong is uninstalled and then reinstalled, recover old Llong data)
    // If there's no Llong data in the save when loading, try to load from the Llong-specific backup file
    [HarmonyPatch(typeof(SaveFileUtility), "GetSaveFileFromFile")]
    static class SaveFileUtility_GetSaveFileFromFile_Patch
    {
        static void Postfix(int slotIndex, ref SaveFile __result)
        {
            int highestLlongCompletedAscension = -1;
            if (__result != null && __result.CharacterHighestCompletedAscensions.ContainsKey(typeof(LlongCharacter)))
            {
                highestLlongCompletedAscension = __result.CharacterHighestCompletedAscensions[typeof(LlongCharacter)];
            }
            if (highestLlongCompletedAscension == -1)
            {
                // Llong is either missing or just got reset to -1. Check to see if there's a slot_X_llong.sav to recover from
                FallbackSaveUtility fallback = new FallbackSaveUtility(slotIndex);
                fsData fallbackData = fallback.Load();
                if (fallbackData != null && fallbackData.AsDictionary.ContainsKey("LlongHighestCompletedAscension") && fallbackData.AsDictionary["LlongHighestCompletedAscension"].IsInt64)
                {
                    __result.CharacterHighestCompletedAscensions[typeof(LlongCharacter)] = (int)fallbackData.AsDictionary["LlongHighestCompletedAscension"].AsInt64;
                    MelonLogger.Msg($"[Llong] Recovered crown level: {__result.CharacterHighestCompletedAscensions[typeof(LlongCharacter)]}");
                }
                else
                {
                    Core.DebugLog($"[Llong] No crown data to recover");
                }
            }
        }
    }

    // When the game saves, also save Llong data to a separate file in case it gets wiped.
    [HarmonyPatch(typeof(SaveManager), "SaveFileToFile")]
    static class SaveManager_SaveFileToFile_Patch
    {
        static void Postfix(int slotIndex)
        {
            FallbackSaveUtility fallback = new FallbackSaveUtility(slotIndex);
            fallback.Save();
        }
    }
    #endregion
}
