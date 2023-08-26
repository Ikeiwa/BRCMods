#if !SDK
using HarmonyLib;
using Reptile;
using UnityEngine;
using System.Reflection;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Playables;

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

        [HarmonyPatch(typeof(NPC))]
        [HarmonyPatch(nameof(NPC.InitSceneObject))]
        class Patch_NPC_InitSceneObject
        {
            static void Prefix(NPC __instance,ref Transform ___head)
            {
                List<OutfitSwappableCharacter> swappableCharacters = __instance.GetComponentsInChildren<OutfitSwappableCharacter>().ToList();

                foreach (OutfitSwappableCharacter npcChar in swappableCharacters)
                {
                    if (npcChar != null && Utils.TryGetCustomCharacter(npcChar.Character, out CustomModel customModel))
                    {
                        DynamicBone[] dynamicBones = npcChar.transform.parent.GetComponentsInChildren<DynamicBone>();

                        foreach (DynamicBone dynamicBone in dynamicBones)
                            dynamicBone.enabled = false;

                        Animator anim = npcChar.GetComponentInChildren<Animator>();

                        GameObject customModelInstance = Object.Instantiate(customModel.fbx, npcChar.transform.parent);
                        Animator customAnimator = customModelInstance.GetComponent<Animator>();
                        customAnimator.runtimeAnimatorController = anim.runtimeAnimatorController;
                        customModelInstance.transform.localPosition = anim.transform.localPosition;
                        customModelInstance.transform.localRotation = anim.transform.localRotation;

                        OutfitSwappableCharacter outfitSwappableCharacter = customModelInstance.AddComponent<OutfitSwappableCharacter>();
                        outfitSwappableCharacter.SetPrivateField("character", npcChar.Character);
                        outfitSwappableCharacter.SetPrivateField("mainRenderer", customModelInstance.GetComponentInChildren<SkinnedMeshRenderer>());

                        LookAtIKComponent lookAtIK = customModelInstance.AddComponent<LookAtIKComponent>();

                        customModelInstance.SetActive(npcChar.gameObject.activeSelf);

                        anim.transform.SetParent(null);
                        anim.runtimeAnimatorController = null;
                        Object.Destroy(npcChar.gameObject);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(NPC))]
        [HarmonyPatch(nameof(NPC.GetLookAtPos))]
        class Patch_NPC_GetLookAtPos
        {
            static bool Prefix(NPC __instance, ref Transform ___head, ref bool ___canLookAtPlayer, ref Vector3 __result)
            {
                if (___canLookAtPlayer && ___head)
                {
                    __result = ___head.position + Vector3.up * 0.125f;
                    return false;
                }
                __result = __instance.transform.position + Vector3.up * __instance.playerLookAtHeightForNonHumanNPC;

                return false;
            }
        }

        [HarmonyPatch(typeof(SequenceHandler))]
        [HarmonyPatch("ReplaceMaterialsOnCharactersInCutscene")]
        class Patch_SequenceHandler_ReplaceMaterialsOnCharactersInCutscene
        {
            static void Prefix(SequenceHandler __instance, ref PlayableDirector ___sequence)
            {

            }
        }
    }
}
#endif