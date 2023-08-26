#if !SDK
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

                if (Utils.IsCustomCharacter(character))
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

                if (Utils.IsCustomCharacter(character))
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

        [HarmonyPatch(typeof(CharacterVisual))]
        [HarmonyPatch(nameof(CharacterVisual.Init))]
        class Patch_CharacterVisual_Init
        {
            static void Postfix(Characters character, RuntimeAnimatorController animatorController, bool IK, float setGroundAngleLimit, CharacterVisual __instance)
            {
                if (Utils.TryGetCustomCharacter(character,out CustomModel customModel))
                {
                    __instance.canBlink = !string.IsNullOrEmpty(customModel.avatarDescriptor.blinkBlendshape);
                    __instance.anim.transform.localScale = customModel.fbx.transform.localScale;
                }
            }
        }

        [HarmonyPatch(typeof(Player))]
        [HarmonyPatch("UpdateBlinkAnimation")]
        class Patch_Player_UpdateBlinkAnimation
        {
            static bool Prefix(SkinnedMeshRenderer mainRenderer, Mesh characterMesh, ref float blinkTimer, ref bool blink, ref float blinkDuration, float deltaTime, Player __instance, Characters ___character)
            {
                if(Utils.TryGetCustomCharacter(___character, out CustomModel customModel))
                {
                    if (customModel.blinkBlendshapeIndex == -1)
                        return true;

                    blinkTimer -= deltaTime;
                    if (blinkTimer <= 0f)
                    {
                        blink = true;
                        customModel.avatarDescriptor.blinkRenderer.SetBlendShapeWeight(0, 100f);
                        blinkTimer = (float)Random.Range(2, 4);
                    }
                    if (blink)
                    {
                        blinkDuration -= deltaTime;
                    }
                    if (blinkDuration <= 0f)
                    {
                        blink = false;
                        customModel.avatarDescriptor.blinkRenderer.SetBlendShapeWeight(0, 0f);
                        blinkDuration = 0.1f;
                    }
                }

                return true;
            }
        }
    }
}
#endif