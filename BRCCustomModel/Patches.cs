#if !SDK
using HarmonyLib;
using Reptile;
using UnityEngine;
using System.Reflection;
using System.Collections;
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

                        
                    Object.Destroy(returnValue);
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
                        newmaterialAsset = Object.Instantiate(Plugin.customModelAssets[character].avatarDescriptor.skins[outfit]);

                    if(newmaterialAsset == null)
                        return returnValue;

                    newmaterialAsset.shader = returnValue.shader;

                    return newmaterialAsset;
                }
                else
                    return returnValue;
            }
        }

        [HarmonyPatch(typeof(OutfitSwitchMenu))]
        [HarmonyPatch(nameof(OutfitSwitchMenu.SkinButtonSelected))]
        class Patch_OutfitSwitchMenu_SkinButtonSelected
        {
            static void Postfix(MenuTimelineButton clickedButton, int skinIndex, ref CharacterVisual ___previewCharacterVisual)
            {
                Characters character = (Characters)___previewCharacterVisual.GetComponentInChildren<BRCAvatarDescriptor>().character;

                if (Utils.IsCustomCharacter(character))
                {
                    Shader oldShader = ___previewCharacterVisual.mainRenderer.material.shader;
                    ___previewCharacterVisual.mainRenderer.material = Plugin.customModelAssets[character].avatarDescriptor.skins[skinIndex];
                    ___previewCharacterVisual.mainRenderer.material.shader = oldShader;
                }
                
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
                List<OutfitSwappableCharacter> swappableCharacters = __instance.GetComponentsInChildren<OutfitSwappableCharacter>(true).ToList();

                foreach (OutfitSwappableCharacter npcChar in swappableCharacters)
                {
                    if (npcChar != null && Utils.TryGetCustomCharacter(npcChar.Character, out CustomModel customModel))
                    {
                        DynamicBone[] dynamicBones = npcChar.GetComponents<DynamicBone>();

                        foreach (DynamicBone dynamicBone in dynamicBones)
                            dynamicBone.enabled = false;

                        Animator anim = npcChar.GetComponentInChildren<Animator>(true);

                        GameObject customModelInstance = Object.Instantiate(customModel.fbx, npcChar.transform);
                        Animator customAnimator = customModelInstance.GetComponent<Animator>();
                        customAnimator.runtimeAnimatorController = anim.runtimeAnimatorController;
                        customModelInstance.transform.localPosition = anim.transform.localPosition;
                        customModelInstance.transform.localRotation = anim.transform.localRotation;

                        npcChar.SetPrivateField("character", npcChar.Character);
                        npcChar.SetPrivateField("mainRenderer", customModelInstance.GetComponentInChildren<SkinnedMeshRenderer>(true));

                        customModelInstance.AddComponent<DummyAnimationEventRelay>();
                        customModelInstance.AddComponent<LookAtIKComponent>();

                        customModelInstance.SetActive(anim.gameObject.activeSelf);

                        anim.transform.SetParent(null);
                        anim.runtimeAnimatorController = null;
                        Object.Destroy(anim.gameObject);
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
            private static Dictionary<string, Characters> cutsceneNames = new Dictionary<string, Characters>
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

            private static List<string> reparentedProps = new List<string>
            {
                "footl",
                "footr",
                "jetpackPos",
                "propr",
                "propl",
            };

            private static void ReparentProp(Transform originalRoot, Transform customRoot, string propName)
            {
                Transform prop = originalRoot.FindRecursive(propName);
                Transform propCustom = customRoot.FindRecursive(propName);
                if (prop && propCustom)
                {
                    for(int i=0;i < prop.childCount; i++)
                    {
                        Transform propChild = prop.GetChild(i);

                        Vector3 basePos = propChild.position;
                        Quaternion baseRot = propChild.rotation;
                        Vector3 baseScale = propChild.localScale;

                        propChild.SetParent(propCustom);
                        propChild.localPosition = basePos;
                        propChild.localRotation = baseRot;
                        propChild.localScale = baseScale;
                    }
                }
            }

            private static void SwapCutsceneCharacter(Transform charRoot, Characters character, OutfitSwappableCharacter outfitSwap = null)
            {
                if (charRoot != null && Utils.TryGetCustomCharacter(character, out CustomModel customModel) && !charRoot.gameObject.name.EndsWith("SWAPPED"))
                {
                    DynamicBone[] dynamicBones = charRoot.GetComponents<DynamicBone>();

                    foreach (DynamicBone dynamicBone in dynamicBones)
                        dynamicBone.enabled = false;

                    Animator anim = null;
                    foreach (Transform child in charRoot)
                    {
                        anim = child.GetComponent<Animator>();
                        if (anim) break;
                    }

                    anim.transform.Find("root").gameObject.name = "OLD Root";

                    SkinnedMeshRenderer[] oldRenderers = anim.GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach(var oldRenderer in oldRenderers)
                    {
                        oldRenderer.gameObject.name += " OLD";
                        oldRenderer.enabled = false;
                    }

                    GameObject customModelInstance = Object.Instantiate(customModel.fbx, charRoot);
                    Animator customAnimator = customModelInstance.GetComponent<Animator>();

                    anim.avatar = customAnimator.avatar;

                    customModelInstance.transform.localPosition = anim.transform.localPosition;
                    customModelInstance.transform.localRotation = anim.transform.localRotation;

                    SkinnedMeshRenderer skinnedMeshRenderer = customModelInstance.GetComponentInChildren<SkinnedMeshRenderer>(true);

                    if (!outfitSwap)
                        outfitSwap = charRoot.gameObject.AddComponent<OutfitSwappableCharacter>();

                    outfitSwap.SetPrivateField("character", character);
                    outfitSwap.SetPrivateField("mainRenderer", skinnedMeshRenderer);

                    if (customModel.avatarDescriptor.blinkRenderer && !string.IsNullOrEmpty(customModel.avatarDescriptor.blinkBlendshape))
                    {
                        StoryBlinkAnimation storyBlinkAnimation = anim.GetComponent<StoryBlinkAnimation>();
                        if (storyBlinkAnimation)
                        {
                            storyBlinkAnimation.mainRenderer = customModel.avatarDescriptor.blinkRenderer;
                            storyBlinkAnimation.characterMesh = storyBlinkAnimation.mainRenderer.sharedMesh;
                        }
                    }
                    else
                    {
                        StoryBlinkAnimation storyBlinkAnimation = anim.GetComponent<StoryBlinkAnimation>();
                        if (storyBlinkAnimation)
                            storyBlinkAnimation.enabled = false;
                    }

                    string[] nonTransferObjects = new string[]
                    {
                            "handlIK",
                            "handrIK",
                            "phoneDirectionRoot",
                            "skateboard",
                            "bmxFrame",
                    };

                    foreach (Transform child in customModelInstance.transform)
                    {
                        string childName = child.gameObject.name;

                        if (!nonTransferObjects.Contains(childName))
                        {
                            child.SetParent(anim.transform);
                            child.localPosition = Vector3.zero;
                            child.localRotation = Quaternion.identity;
                        }
                    }

                    Transform customRoot = customModelInstance.transform.Find("root");

                    customRoot.SetParent(anim.transform);
                    customRoot.localPosition = Vector3.zero;
                    customRoot.localRotation = Quaternion.identity;

                    foreach(string prop in reparentedProps)
                    {
                        ReparentProp(charRoot, customRoot, prop);
                    }

                    charRoot.gameObject.name += " SWAPPED";

                    Object.Destroy(customModelInstance);
                }
            }

            static void Prefix(SequenceHandler __instance, ref PlayableDirector ___sequence)
            {
                Transform[] allCharacters = ___sequence.transform.GetComponentsInChildren<Transform>(true);

                foreach (Transform tr in allCharacters)
                {
                    foreach(string key in cutsceneNames.Keys)
                    {
                        if(tr.gameObject.name.StartsWith(key))
                            SwapCutsceneCharacter(tr.transform, cutsceneNames[key], null);
                    }
                }

                OutfitSwappableCharacter[] swappableCharacters = ___sequence.transform.GetComponentsInChildren<OutfitSwappableCharacter>(true);

                foreach (OutfitSwappableCharacter npcChar in swappableCharacters)
                {
                    SwapCutsceneCharacter(npcChar.transform, npcChar.Character, npcChar);
                }
            }
        }
    }
}
#endif