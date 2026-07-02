using FullSerializer;
using MelonLoader;
using UnityEngine;

namespace Llong
{
    class FallbackSaveUtility
    {
        string SavePath;

        public FallbackSaveUtility(int slotIndex)
        {
            string llongSaveFileName = string.Format("{0}/slot_{1}_llong.sav", GameStatics.SaveDirectory, (slotIndex == 0) ? GameStatics.SaveSlot : slotIndex);
            SavePath = Path.Combine(Application.persistentDataPath, llongSaveFileName);
        }

        public fsData Load()
        {
            Core.DebugLog($"[Llong] Trying to load recovery data from {SavePath}");
            if (File.Exists(SavePath))
            {
                try
                {
                    fsData fsData = fsJsonParser.Parse(File.ReadAllText(SavePath));
                    if (!fsData.IsDictionary)
                    {
                        return null;
                    }
                    return fsData;
                }
                catch(Exception ex)
                {
                    MelonLogger.Warning($"Error when loading {SavePath}: {ex.Message}");
                }
            }
            return null;
        }

        public void Save()
        {
            Dictionary<Type, int> highestCompletedAscensions = SaveManager.GetAllCharacterProgressInfo();
            if (highestCompletedAscensions != null && highestCompletedAscensions.ContainsKey(typeof(LlongCharacter)) && highestCompletedAscensions[typeof(LlongCharacter)] != -1)
            {
                Core.DebugLog($"[Llong] Saving recovery data to {SavePath}");
                FallbackSaveData data = new FallbackSaveData();
                data.LlongHighestCompletedAscension = highestCompletedAscensions[typeof(LlongCharacter)];
                string output = StringSerializer.Serialize(typeof(FallbackSaveData), data);
                File.WriteAllText(SavePath, output);
            }
        }
    }
}
