using MelonLoader;
using nickeltin.SDF.Runtime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Llong.Unlocks
{
    class LlongItemData : ItemSpriteData
    {
        public string TrueName;
        public Sprite TrueSprite;
        public SDFSpriteMetadataAsset TrueSDF;

        public LlongItemData(ItemSpriteUsage usage, string assetPath) : base(usage, assetPath)
        {
            this.TrueName = assetPath;
            this.Usage = usage;
            this.AssetPath = "BLACKHOLE";

            if (Core.ItemSprites.ContainsKey(assetPath))
            {
                //MelonLogger.Msg($"[Llong] Grabbing sprite for {assetPath}");
                this.TrueSprite = Core.ItemSprites[assetPath];
                //MelonLogger.Msg(data.ItemSprite);
            }
            if (Core.ItemSdfs.ContainsKey(assetPath))
            {
                //MelonLogger.Msg($"[Llong] Grabbing SDF for {assetPath}");
                this.TrueSDF = Core.ItemSdfs[assetPath];
                //MelonLogger.Msg(data.ItemSDFSprite);
            }
        }
    }
}
