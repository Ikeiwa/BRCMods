using HarmonyLib;
using Reptile;
using UnityEngine;
using System.Reflection;
using System.Collections;
using UnityEngine;

namespace BRCCustomModel
{
    internal class Patches
    {
        /*[HarmonyPatch(typeof(CharacterLoader), "AddCharacterFBX")]
        static class Patch_CharacterLoader_AddCharacterFBX
        {
            [HarmonyPrefix]
            static bool Prefix(Characters character, GameObject fbxAsset, ref GameObject[] ___loadedCharacterFbxAssets)
            {
                if (Plugin.customModelBundles.ContainsKey(character))
                {
                    GameObject newfbxAsset = Plugin.customModelAssets[character].fbx;

                    ___loadedCharacterFbxAssets[(int)character] = newfbxAsset;

                    return false;
                }

                return true;
            }
        }*/

        [HarmonyPatch(typeof(CharacterConstructor))]
        [HarmonyPatch(nameof(CharacterConstructor.CreateCharacterFbx))]
        class Patch_CharacterConstructor_CreateCharacterFbx
        {
            static GameObject Postfix(GameObject returnValue, Characters character, CharacterConstructor __instance)
            {
                if (returnValue == null)
                    return returnValue;

                if (Plugin.customModelAssets.ContainsKey(character))
                {
                    GameObject newFbxAsset = Object.Instantiate(Plugin.customModelAssets[character].fbx);

                    if (newFbxAsset == null)
                        return returnValue;

                    return newFbxAsset;
                }
                else
                    return returnValue;
            }
        }

        [HarmonyPatch(typeof(CharacterConstructor))]
        [HarmonyPatch(nameof(CharacterConstructor.CreateCharacterMaterial))]
        class Patch_CharacterConstructor_CreateCharacterMaterial
        {
            static Material Postfix(Material returnValue, Characters character, int outfit, CharacterConstructor __instance)
            {
                if(returnValue == null)
                    return returnValue;

                if (Plugin.customModelAssets.ContainsKey(character))
                {
                    Material newmaterialAsset = null;

                    if (outfit >= 0 && outfit < 4)
                        newmaterialAsset = Object.Instantiate(Plugin.customModelAssets[character].skins[outfit]);

                    if(newmaterialAsset == null)
                        return returnValue;

                    newmaterialAsset.shader = returnValue.shader;

                    return newmaterialAsset;
                }
                else
                    return returnValue;
            }
        }
    }
}
