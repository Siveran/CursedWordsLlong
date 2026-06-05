using HarmonyLib;
using Llong.Unlocks;
using MelonLoader;
using nickeltin.SDF.Runtime;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(Llong.Core), "Llong", "1.0.0", "Orange", null)]
[assembly: MelonGame("Buried Things", "Cursed Words")]

namespace Llong
{
    public class Core : MelonMod
    {
        public static AssetBundle LlongAssetBundle = null;
        public static RuntimeAnimatorController LlongAnimator = null;
        public static Sprite LlongSprite = null;
        public static Sprite LlongDialogueSprite = null;
        public static Sprite LlongDialogueFlipSprite = null;
        public static SDFSpriteMetadataAsset LlongDialogueSdf = null;
        public static SDFSpriteMetadataAsset LlongDialogueFlipSdf = null;
        public static Dictionary<string, Sprite> ItemSprites = [];
        public static Dictionary<string, SDFSpriteMetadataAsset> ItemSdfs = [];
        public static Emotions EmotionLlongFlip = (Emotions)11079;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized Llong::Core.");
        }

        public override void OnApplicationStart()
        {
            // Manual install instructions tell people to put the files directly in Mods/, but r2modman/gale/thunderstore put files in Mods/CREATOR_NAME/
            // Support both locations!
            string path = Path.Combine([Application.dataPath, "../Mods/noodleassets"]);
            string modManagerInstallPath = Path.Combine([Application.dataPath, "../Mods/Siveran/noodleassets"]);
            AssetBundle assetBundle = AssetBundle.LoadFromFile(path) ?? AssetBundle.LoadFromFile(modManagerInstallPath);
            if (assetBundle == null)
            {
                MelonLogger.Warning($"[Llong] We couldn't find the bundle from {path}");
            }
            else
            {
                // Search through the bundle for the two root assets that we need
                LlongAssetBundle = assetBundle;
                // MelonLogger.Msg("[Llong] " + string.Join(" ||| ", assetBundle.GetAllAssetNames()));
                UnityEngine.Object[] assets = assetBundle.LoadAllAssets();
                foreach (var item in assets)
                {
                    MelonLogger.Msg(item.name);
                    if (item.GetType() == typeof(RuntimeAnimatorController))
                    {
                        MelonLogger.Msg("[Llong] We found an animator controller!");
                        LlongAnimator = item as RuntimeAnimatorController;
                    }
                    if (item.GetType() == typeof(Sprite))
                    {
                        if (item.name == "Noodle")
                        {
                            MelonLogger.Msg("[Llong] We found a main sprite!");
                            LlongSprite = item as Sprite;
                        }
                    }
                }
            }

            // Items are stored separately because I quarantined SDF-having assets to their own project.
            // That wasn't at all necessary, I just was trying to isolate them while figuring them out!
            // Signed Distance Fields are nifty things that help with drawing strokes around sprites
            // Cursed Words generates and uses them via this plugin: https://assetstore.unity.com/packages/tools/gui/sdf-image-quality-ui-outlines-and-shadow-244942
            // For those who come after: 102 border offset, 0.5 gradient size, make sure generate mipmaps is checked on the base image
            string itemsPath = Path.Combine([Application.dataPath, "../Mods/noodleitems"]);
            string itemsModManagerInstallPath = Path.Combine([Application.dataPath, "../Mods/Siveran/noodleitems"]);
            AssetBundle itemsAssetBundle = AssetBundle.LoadFromFile(itemsPath) ?? AssetBundle.LoadFromFile(itemsModManagerInstallPath);
            if (itemsAssetBundle == null)
            {
                MelonLogger.Warning($"[Llong] We couldn't find the item bundle from {itemsPath}");
            }
            UnityEngine.Object[] itemAssets = itemsAssetBundle.LoadAllAssets();
            foreach (var item in itemAssets)
            {
                // The dialogue sprites are with the items because they need SDF metadata
                if (item.GetType() == typeof(Sprite))
                {
                    if (item.name == "NoodleDialogue2")
                    {
                        MelonLogger.Msg("[Llong] We found a dialogue sprite!");
                        LlongDialogueSprite = item as Sprite;
                    }
                    else if (item.name == "NoodleDialogue1")
                    {
                        MelonLogger.Msg("[Llong] We found a flipped dialogue sprite!");
                        LlongDialogueFlipSprite = item as Sprite;
                    }
                    else if (!item.name.Contains("(SDFSprite)"))
                    {
                        MelonLogger.Msg($"[Llong] The items file had a Sprite named {item.name}!");
                        ItemSprites[item.name] = item as Sprite;
                    }
                }
                else if (item.GetType() == typeof(SDFSpriteMetadataAsset))
                {
                    string trimmedName = item.name.PartBefore('(');
                    if (trimmedName == "NoodleDialogue2")
                    {
                        MelonLogger.Msg("[Llong] We found a dialogue SDF!");
                        LlongDialogueSdf = item as SDFSpriteMetadataAsset;
                    }
                    else if (trimmedName == "NoodleDialogue1")
                    {
                        MelonLogger.Msg("[Llong] We found a flipped dialogue SDF!");
                        LlongDialogueFlipSdf = item as SDFSpriteMetadataAsset;
                    }
                    else
                    {
                        MelonLogger.Msg($"[Llong] The items file had an SDF named {item.name}!");
                        ItemSdfs[item.name.PartBefore('(')] = item as SDFSpriteMetadataAsset;
                    }
                }
            }
        }

        // See Type_GetType_Patch
        static public Type GetType(string typeName)
        {
            return Assembly.GetExecutingAssembly().GetType(typeName);
        }
    }

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
                // MelonLogger.Msg($"[Llong] Did we get a type for {typeName}? {(__result == null ? "no" : "yes")}");
                return false;
            }

            // Do normal type lookup
            __result = null;
            return true;
        }
    }

    [HarmonyPatch(typeof(CharacterSelectController), "GetCharacters")]
    static class CharacterSelectController_GetCharacters_Patch
    {
        static void Postfix(Dictionary<Type, bool> ____characters)
        {
            MelonLogger.Msg("[Llong] Allowing Llong to be selected");
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
            MelonLogger.Msg("[Llong] Adding Llong tooltips");
            if (!____characterSpecificToolTips.ContainsKey(typeof(LlongCharacter)))
            {
                ____characterSpecificToolTips.Add(typeof(LlongCharacter), [
                    new ValueTuple<string, Emotions>("Stretched letters are ALWAYS ignored when checking the dictionary", Core.EmotionLlongFlip),
                    new ValueTuple<string, Emotions>("Numbers don't count stretched letters", Core.EmotionLlongFlip)
                ]);
            }
        }
    }


    [HarmonyPatch(typeof(IDCard))]
    [HarmonyPatch(MethodType.Constructor)]
    static class IDCard_CTOR_Patch
    {
        static void Postfix(IDCard __instance)
        {
            MelonLogger.Msg("[Llong] Adding Llong's ID card");
            // TODO: Make an ID for Llong.
            // __instance.SpriteData.Add(new ItemSpriteData(ItemSpriteUsage.AvocadoMushy, "LlongIDCard"));
            // __instance.CharacterToSpriteUsage.Add(typeof(LlongCharacter), ItemSpriteUsage.AvocadoMushy);
            __instance.CharacterToSpriteUsage.Add(typeof(LlongCharacter), ItemSpriteUsage.Default);
        }
    }
}