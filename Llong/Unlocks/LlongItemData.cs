using nickeltin.SDF.Runtime;
using UnityEngine;

namespace Llong.Unlocks
{
    // Llong's items' sprites/SDFs are stored in a different place than normal items.
    // We set things up so the vanilla logic will use Blackhole's sprite if the mod fails to load the custom sprite.
    // See ItemSpriteData_GetSprite_Patch or ItemSpriteData_GetSDFSprite_Patch in Core.cs for how these are referenced!
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
                this.TrueSprite = Core.ItemSprites[assetPath];
            }
            if (Core.ItemSdfs.ContainsKey(assetPath))
            {
                this.TrueSDF = Core.ItemSdfs[assetPath];
            }
        }
    }
}
