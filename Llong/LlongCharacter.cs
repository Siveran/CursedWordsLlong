using MelonLoader;
using nickeltin.SDF.Runtime;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Llong.Unlocks;

namespace Llong
{
    public class LlongCharacter : Character
    {
        public LlongCharacter()
            : base("Llong",
                "",
                "After his most recent decades-long slumber, Llong seeks to get updated on all the recent slang to become keeeeeen and groooooooooovy again.<br><br>...Llong you're covering up three people please move.",
                new Spaghetti(),
                "llong",
                new Color32(86, 63, 115, byte.MaxValue), // Left pin upgrade color
                new Color32(171, 150, 136, byte.MaxValue), // Right pin upgrade color
                new Vector3(-30f, 150f, 0f), // Victory Offset
                new Vector3(230f, 400f, 0f), // Defeat Offset
                [ // Intro starting items
                    typeof(YellowGlasses),
                    typeof(FishCake),
                    typeof(AprilShower),
                    typeof(GraduationCap)
                ],
                Core.EmotionLlongFlip,
                false, // Not secret
                "") // No secret unlock condition
        {
        }

        public override RuntimeAnimatorController GetRuntimeAnimatorController()
        {
            if (Core.LlongAnimator)
            {
                CharacterRuntimeAnimators[typeof(LlongCharacter)] = Core.LlongAnimator;
                return Core.LlongAnimator;
            }
            else
            {
                MelonLogger.Warning("[Llong] No animation found! Are the assets in the Mods/ folder?");
                return Activator.CreateInstance<BonesTheDog>().GetRuntimeAnimatorController();
            }
        }

        public override SDFSpriteMetadataAsset GetCharacterSDFSprite()
        {
            return Activator.CreateInstance<BonesTheDog>().GetCharacterSDFSprite();
        }

        public override Sprite GetCharacterSprite()
        {
            if (Core.LlongSprite)
            {
                CharacterRuntimeAnimators[typeof(LlongCharacter)] = Core.LlongAnimator;
                return Core.LlongSprite;
            }
            else
            {
                MelonLogger.Warning("[Llong] No sprite found! Are the assets in the Mods/ folder?");
                return Activator.CreateInstance<BonesTheDog>().GetCharacterSprite();
            }
        }
    }
}