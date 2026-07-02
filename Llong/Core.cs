
using MelonLoader;
using nickeltin.SDF.Runtime;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

[assembly: MelonInfo(typeof(Llong.Core), "Llong", "1.0.6", "Orange", null)]
[assembly: MelonGame("Buried Things", "Cursed Words")]

namespace Llong
{
    public class Core : MelonMod
    {
        public const bool DEBUG_ENABLED = true;
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
            // The two asset files (noodleassets and noodleitems) SHOULD be in the same directory as the assembly.
            // This covers both manual install and mod manager install options.
            string directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string path = directory + "/noodleassets";
            MelonLogger.Msg("[Llong] trying to find assets at " + path);
            AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
            if (assetBundle == null)
            {
                MelonLogger.Warning($"[Llong] We couldn't find the bundle from {path}");
            }
            else
            {
                // Search through the bundle for the two root assets that we need
                LlongAssetBundle = assetBundle;
                DebugLog("[Llong] " + string.Join(" ||| ", assetBundle.GetAllAssetNames()));
                UnityEngine.Object[] assets = assetBundle.LoadAllAssets();
                foreach (var item in assets)
                {
                    DebugLog($"[Llong] Looking at asset {item.name}");
                    if (item.GetType() == typeof(RuntimeAnimatorController))
                    {
                        DebugLog("[Llong] We found an animator controller!");
                        LlongAnimator = item as RuntimeAnimatorController;
                    }
                    if (item.GetType() == typeof(Sprite) && item.name == "Noodle")
                    {
                        DebugLog("[Llong] We found a main sprite!");
                        LlongSprite = item as Sprite;
                    }
                }
            }

            // Items are stored separately because I quarantined SDF-having assets to their own project.
            // That wasn't at all necessary, I just was trying to isolate them while figuring them out!
            // Signed Distance Fields are nifty things that help with drawing strokes around sprites
            // Cursed Words generates and uses them via this plugin: https://assetstore.unity.com/packages/tools/gui/sdf-image-quality-ui-outlines-and-shadow-244942
            // For those who come after: 102 border offset, 0.5 gradient size, make sure generate mipmaps is checked on the base image
            string itemsPath = Path.Combine([Application.dataPath, "../Mods/noodleitems"]);
            string itemsModManagerInstallPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/noodleitems";
            MelonLogger.Msg("[Llong] trying to find item assets at " + itemsModManagerInstallPath);
            AssetBundle itemsAssetBundle = AssetBundle.LoadFromFile(itemsPath) ?? AssetBundle.LoadFromFile(itemsModManagerInstallPath);
            if (itemsAssetBundle == null)
            {
                MelonLogger.Warning($"[Llong] We couldn't find the item bundle from {itemsPath}");
            }
            else
            {
                UnityEngine.Object[] itemAssets = itemsAssetBundle.LoadAllAssets();
                foreach (var item in itemAssets)
                {
                    // The dialogue sprites are with the items because they need SDF metadata
                    if (item.GetType() == typeof(Sprite))
                    {
                        if (item.name == "NoodleDialogue2")
                        {
                            DebugLog("[Llong] We found a dialogue sprite!");
                            LlongDialogueSprite = item as Sprite;
                        }
                        else if (item.name == "NoodleDialogue1")
                        {
                            DebugLog("[Llong] We found a flipped dialogue sprite!");
                            LlongDialogueFlipSprite = item as Sprite;
                        }
                        else if (!item.name.Contains("(SDFSprite)"))
                        {
                            DebugLog($"[Llong] The items file had a Sprite named {item.name}!");
                            ItemSprites[item.name] = item as Sprite;
                        }
                    }
                    else if (item.GetType() == typeof(SDFSpriteMetadataAsset))
                    {
                        string trimmedName = item.name.PartBefore('(');
                        if (trimmedName == "NoodleDialogue2")
                        {
                            DebugLog("[Llong] We found a dialogue SDF!");
                            LlongDialogueSdf = item as SDFSpriteMetadataAsset;
                        }
                        else if (trimmedName == "NoodleDialogue1")
                        {
                            DebugLog("[Llong] We found a flipped dialogue SDF!");
                            LlongDialogueFlipSdf = item as SDFSpriteMetadataAsset;
                        }
                        else
                        {
                            DebugLog($"[Llong] The items file had an SDF named {item.name}!");
                            ItemSdfs[item.name.PartBefore('(')] = item as SDFSpriteMetadataAsset;
                        }
                    }
                }
            }
        }

        static public void DebugLog(string message)
        {
            if (DEBUG_ENABLED)
            {
                MelonLogger.Msg(System.ConsoleColor.Gray, message);
            }
        }

        // See Type_GetType_Patch in Patches.cs
        static public Type GetType(string typeName)
        {
            return Assembly.GetExecutingAssembly().GetType(typeName);
        }
    }
}