#if !SDK
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BepInEx;
using Reptile;

namespace BRCCustomModel
{
    internal static class Utils
    {
        public static bool IsCustomCharacter(Characters character)
        {
            return Plugin.customModelAssets.ContainsKey(character);
        }

        public static bool TryGetCustomCharacter(Characters character, out CustomModel customModel)
        {
            customModel = new CustomModel();

            if(IsCustomCharacter(character))
            {
                customModel = Plugin.customModelAssets[character];
                return true;
            }

            return false;
        }

        public static void SetPrivateField(this object obj, string fieldName, object value)
        {
            var prop = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prop != null)
                prop.SetValue(obj, value);
            else
                Logging.LogError("Field " + fieldName + " Not found");
        }
    }
}
#endif