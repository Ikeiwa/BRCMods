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

        public static Dictionary<string, Characters> cutsceneNames = new Dictionary<string, Characters>
            {
                {"FauxNoJetpackStory" ,Characters.headManNoJetpack },
                {"FauxStory" ,Characters.headMan },
                {"SolaceStory" ,Characters.dummy },
                {"IreneStory" ,Characters.jetpackBossPlayer },
                {"DJMaskedStory" ,Characters.dj },
                {"DJNoMaskStory" ,Characters.dj },
                {"FuturismStory" ,Characters.futureGirl },
                {"FuturismBStory" ,Characters.futureGirl },
                {"FuturismCStory" ,Characters.futureGirl },
                {"FuturismDStory" ,Characters.futureGirl },
                {"EclipseAStory" ,Characters.medusa },
                {"EclipseBStory" ,Characters.medusa },
                {"EclipseCStory" ,Characters.medusa },
                {"EclipseDStory" ,Characters.medusa },
                {"DotExeEStory" ,Characters.eightBallBoss },
                {"DotExeAStory" ,Characters.eightBall },
                {"DotExeBStory" ,Characters.eightBall },
                {"DotExeCStory" ,Characters.eightBall },
                {"DotExeDStory" ,Characters.eightBall },
                {"RedShatteredStory" ,Characters.metalHead },
                {"DemonTheoryAStory" ,Characters.boarder },
                {"DemonTheoryBStory" ,Characters.boarder },
                {"DemonTheoryCStory" ,Characters.boarder },
                {"FelixNoJetpackStory" ,Characters.legendFace },
                {"FrankAStory" ,Characters.frank },
                {"FrankBStory" ,Characters.frank },
                {"FrankCStory" ,Characters.frank },
                {"FrankDStory" ,Characters.frank },
            };
    }
}
#endif