using HarmonyLib;
using MelonLoader;
using UnityEngine;

// This entire file only has an impact if the multiplayer mod is loaded (https://github.com/PurelyK2/Cursed-Words-Multiplayer)
namespace Llong
{
    class LlongBoss : BossModifier
    {
        // Allow Llong to be a boss encounter in multiplayer only.
        // He makes the grid looonnnger (6x4 -> 7x3 -> 8x2)
        public LlongBoss()
        {
            this.Name = "Llong";
            this.PrefabFileName = "Llong";
            this.AudioPrefix = "Axolotl";
            this.SpriteFileName = new LlongCharacter().GetArtFileName();
            this.UIColor = new LlongCharacter().GetUIColorA();
            this.DifficultyModifier = [ 4, 3, 3, 2, 2, 1 ];
            this.DifficultyIncrease = [ -1, -1, -1, -1, -1, 0 ];
            this.BannedFloorIndexes = [ 0, 1, 2, 3, 4, 5, 6 ];
            this.CharacterToUnlock = typeof(LlongCharacter);
            this.CanBeSummonedByMichael = false;
            this.IsSecretCharacter = false;
        }

        public int GetWidth()
        {
            return 10 - FloorAdjustedModification;
        }

        public int GetHeight()
        {
            return FloorAdjustedModification;
        }

        public int GetRepeats()
        {
            return 6 - FloorAdjustedModification;
        }

        public override string GetDescription()
        {
            RunProgress currentRunProgress = GameStatics.GetPlayer().CurrentRunProgress;
            SetFloorAdjustedModification(currentRunProgress != null ? currentRunProgress.GetStage() - 1 : 0, false);
            int repeats = GetRepeats();
            return "Your grid is l" + new string('o', repeats) + new string('n', repeats) + "g (" + GetWidth() + "x" + GetHeight() + ")";
        }

        public override Sprite GetBossSprite()
        {
            System.Type type = base.GetType();
            if (!Character.CharacterSprites.ContainsKey(type))
            {
                Character.CharacterSprites[type] = Core.LlongSprite;
            }
            return Character.CharacterSprites[type];
        }

        public override RuntimeAnimatorController GetRuntimeAnimatorController()
        {
            System.Type type = base.GetType();
            if (!Character.CharacterRuntimeAnimators.ContainsKey(type))
            {
                Character.CharacterRuntimeAnimators[type] = Core.LlongAnimator;
            }
            return Character.CharacterRuntimeAnimators[type];
        }
    }

    [HarmonyPatch(typeof(CharacterSelectController), "Start")]
    static class CharacterSelectController_Start_Patch
    {
        static void Postfix()
        {
            Type ReceivedInfo = Type.GetType("CWMultiplayer.ReceivedInfo");
            if (ReceivedInfo != null)
            {
                MelonLogger.Msg("[Llong] Adding Llong as a multiplayer boss");
                Type[] allBosses = (Type[])ReceivedInfo.GetField("AllBosses").GetValue(null);
                allBosses.AddItem(typeof(LlongBoss));
            }
        }
    }

    [HarmonyPatch(typeof(EncounterController), "GetGridDimensions")]
    public static class EncounterController_GetGridDimensions_Patch
    {
        public static bool Prefix(EncounterController __instance, ref Vector2Int __result)
        {
            if (__instance.IsBossModifierActive(typeof(LlongBoss)))
            {
                LlongBoss boss = __instance.GetActiveBossModifierOfType(typeof(LlongBoss)) as LlongBoss;
                __result = new Vector2Int(boss.GetWidth(), boss.GetHeight());
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
