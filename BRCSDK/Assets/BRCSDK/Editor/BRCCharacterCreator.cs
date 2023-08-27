using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BRCCustomModel;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;

public enum Characters
{
    NONE = -1,
    VINYL,
    FRANK,
    COIL,
    RED,
    TRYCE,
    BEL,
    RAVE,
    DOT_EXE,
    SOLACE,
    DJ_CYBER,
    ECLIPSE,
    DEVIL_THEORY,
    FAUX,
    FLESH_PRINCE,
    RIETVELD,
    FELIX,
    OLDHEAD,
    BASE,
    JAY,
    MESH,
    FUTURISM,
    RISE,
    SHINE,
    FAUX_NO_JETPACK,
    DOT_EXE_BOSS,
    RED_FELIX,
}

public class BRCCharacterCreator : EditorWindow
{
    private GameObject model;
    
    [MenuItem("BRC/Custom Model Creator")]
    static void Init()
    {
        BRCCharacterCreator window = (BRCCharacterCreator)EditorWindow.GetWindow(typeof(BRCCharacterCreator));
        window.Show();
    }

    void OnGUI()
    {
        model = (GameObject)EditorGUILayout.ObjectField("Model", model, typeof(GameObject), true);

        if (model)
        {
            SkinnedMeshRenderer[] skinnedMeshRenderers = model.GetComponentsInChildren<SkinnedMeshRenderer>();

            bool valid = true;
            
            if (skinnedMeshRenderers.Length == 0)
            {
                EditorGUILayout.HelpBox("Model has no skinnedmesh.", MessageType.Error);
                valid = false;
            }
            
            if (skinnedMeshRenderers.Length > 1)
            {
                EditorGUILayout.HelpBox("Model has more than 1 skinnedmesh. It will cause problems in game", MessageType.Warning);
            }

            if (skinnedMeshRenderers.Length > 1 && skinnedMeshRenderers[0].sharedMaterials.Length > 1)
            {
                EditorGUILayout.HelpBox("Model has more than 1 material. It will cause problems in game", MessageType.Warning);
            }

            Animator animator = model.GetComponentInChildren<Animator>();
            
            if (!animator)
            {
                EditorGUILayout.HelpBox("Model has no animator. Your rig is probably not set to humanoid", MessageType.Error);
                valid = false;
            }
            else
            {
                if (!animator.avatar || !animator.avatar.isHuman)
                {
                    EditorGUILayout.HelpBox("Model is not humanoid.", MessageType.Error);
                    valid = false;
                }
                else
                {
                    if (!animator.GetBoneTransform(HumanBodyBones.Head))
                    {
                        EditorGUILayout.HelpBox("Model has no Head.", MessageType.Error);
                        valid = false;
                    }
                    
                    if (!animator.GetBoneTransform(HumanBodyBones.LeftHand))
                    {
                        EditorGUILayout.HelpBox("Model has no Left Hand.", MessageType.Error);
                        valid = false;
                    }
                    
                    if (!animator.GetBoneTransform(HumanBodyBones.RightHand))
                    {
                        EditorGUILayout.HelpBox("Model has no Right Hand.", MessageType.Error);
                        valid = false;
                    }
                    
                    if (!animator.GetBoneTransform(HumanBodyBones.LeftFoot))
                    {
                        EditorGUILayout.HelpBox("Model has no Left Foot.", MessageType.Error);
                        valid = false;
                    }
                    
                    if (!animator.GetBoneTransform(HumanBodyBones.RightFoot))
                    {
                        EditorGUILayout.HelpBox("Model has no Right Foot.", MessageType.Error);
                        valid = false;
                    }
                    
                    if (!animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg))
                    {
                        EditorGUILayout.HelpBox("Model has no Left Lower Leg.", MessageType.Error);
                        valid = false;
                    }
                    
                    if (!animator.GetBoneTransform(HumanBodyBones.RightLowerLeg))
                    {
                        EditorGUILayout.HelpBox("Model has no Right Lower Leg.", MessageType.Error);
                        valid = false;
                    }
                    
                    if (animator.GetBoneTransform(HumanBodyBones.Head).position.y > 1.48f)
                    {
                        float modelHeight = animator.GetBoneTransform(HumanBodyBones.Head).position.y;
                        float heightdiff = 1.475f / modelHeight;
                        
                        EditorGUILayout.HelpBox("Model is too tall.\nYou should scale it by "+heightdiff, MessageType.Warning);
                    }
                    
                    if (animator.GetBoneTransform(HumanBodyBones.Head).position.y < 1.46f)
                    {
                        float modelHeight = animator.GetBoneTransform(HumanBodyBones.Head).position.y;
                        float heightdiff = modelHeight / 1.475f;
                        
                        EditorGUILayout.HelpBox("Model is too short.\nYou should scale it by "+heightdiff, MessageType.Warning);
                    }
                }
            }

            if (!valid)
                return;

            if (GUILayout.Button("Setup Model"))
            {
                PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                BRCAvatarDescriptor avatarDescriptor = model.AddComponent<BRCAvatarDescriptor>();
                avatarDescriptor.blinkRenderer = skinnedMeshRenderers[0];

                for (int i = 0; i < skinnedMeshRenderers[0].sharedMesh.blendShapeCount; i++)
                {
                    if (skinnedMeshRenderers[0].sharedMesh.GetBlendShapeName(i).ToLower() == "blink")
                    {
                        avatarDescriptor.blinkBlendshape = skinnedMeshRenderers[0].sharedMesh.GetBlendShapeName(i);
                        break;
                    }
                }

#region Generate Avatar

                HumanDescription oldAvatar = CloneHumanDescription(animator.avatar.humanDescription);

                Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                
                RenameBone(oldAvatar, animator, HumanBodyBones.Hips, "root");
                
                RenameBone(oldAvatar, animator, HumanBodyBones.LeftLowerLeg, "leg2l");
                RenameBone(oldAvatar, animator, HumanBodyBones.RightLowerLeg, "leg2r");
                RenameBone(oldAvatar, animator, HumanBodyBones.LeftHand, "handl");
                RenameBone(oldAvatar, animator, HumanBodyBones.RightHand, "handr");
                RenameBone(oldAvatar, animator, HumanBodyBones.Head, "head");
                
                // unparent hips (remove armature node)
                if(hips.parent != model.transform)
                    hips.SetParent(model.transform);
                
                AddBone(ref oldAvatar, model.transform);
                
                Avatar newAvatar = AvatarBuilder.BuildHumanAvatar(model, oldAvatar);
                animator.avatar = newAvatar;

#endregion


#region Create Fake Bones

                GameObject footl = CreateGameObject("footl", animator.GetBoneTransform(HumanBodyBones.LeftFoot));
                GameObject footr = CreateGameObject("footr", animator.GetBoneTransform(HumanBodyBones.RightFoot));
                
                CreateGameObject("handlIK", model.transform, new Vector3(0, 0, -90));
                CreateGameObject("handrIK", model.transform, new Vector3(0, 0, -90));
                
                GameObject propr = CreateGameObject("propr", animator.GetBoneTransform(HumanBodyBones.RightHand));
                GameObject propl = CreateGameObject("propl", animator.GetBoneTransform(HumanBodyBones.LeftHand));
                
#endregion


#region Create Props Empties

                GameObject phoneDirectionRoot = CreateGameObject("phoneDirectionRoot", model.transform, new Vector3(0, 0, -90));
                CreateGameObject("phoneDirection", phoneDirectionRoot.transform);
                                
                CreateGameObject("skateboard", model.transform, new Vector3(0, 0, -90));
                                
                GameObject bmxFrame = CreateGameObject("bmxFrame", model.transform, new Vector3(0, 0, -90));
                GameObject bmxGear = CreateGameObject("bmxGear", bmxFrame.transform);
                CreateGameObject("bmxPedalL", bmxGear.transform);
                CreateGameObject("bmxPedalR", bmxGear.transform);
                GameObject bmxHandlebars = CreateGameObject("bmxHandlebars", bmxFrame.transform);
                CreateGameObject("bmxWheelF", bmxHandlebars.transform);
                CreateGameObject("bmxWheelR", bmxFrame.transform);

                Transform spine2 = animator.GetBoneTransform(HumanBodyBones.Chest);

                GameObject jetpackPos = CreateGameObject("jetpackPos", spine2, new Vector3(0, -63.155f, 0));
                GameObject jetpack = CreateGameObject("jetpack", jetpackPos.transform);
                GameObject boostPos = CreateGameObject("boostPos", jetpack.transform);
                GameObject boost = CreateGameObject("boost", boostPos.transform);

#endregion

#region Create Previews

                SpawnPreview(propl.transform,"preview_phoneMesh");
                SpawnPreview(propr.transform,"preview_spraycanMesh");
                SpawnPreview(footl.transform,"preview_skateLeft");
                SpawnPreview(footr.transform,"preview_skateRight");

#endregion

#region Save Assets

                AssetDatabase.CreateAsset(newAvatar,"Assets/"+model.name+"Avatar.asset");

                Material skin1 = new Material(Shader.Find("Reptile/Ambient Character Fake"));
                AssetDatabase.CreateAsset(skin1,"Assets/"+model.name+"Mat0.mat");
                                
                Material skin2 = new Material(Shader.Find("Reptile/Ambient Character Fake"));
                AssetDatabase.CreateAsset(skin2,"Assets/"+model.name+"Mat1.mat");
                                
                Material skin3 = new Material(Shader.Find("Reptile/Ambient Character Fake"));
                AssetDatabase.CreateAsset(skin3,"Assets/"+model.name+"Mat2.mat");
                                
                Material skin4 = new Material(Shader.Find("Reptile/Ambient Character Fake"));
                AssetDatabase.CreateAsset(skin4,"Assets/"+model.name+"Mat3.mat");

                avatarDescriptor.skins = new[]
                {
                    skin1,
                    skin2,
                    skin3,
                    skin4
                };
                                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

#endregion
                
                EditorGUIUtility.PingObject(skin1);
            }
        }
    }

    private GameObject CreateGameObject(string name, Transform parent, Vector3 baseRotation = default(Vector3), Vector3 basePosition = default(Vector3))
    {
        GameObject obj = new GameObject(name);
        obj.transform.parent = parent;
        obj.transform.localEulerAngles = baseRotation;
        obj.transform.localPosition = Vector3.zero;
        return obj;
    }

    private void CreateBoneAlias(Animator animator, HumanBodyBones originalBone, string boneAliasName)
    {
        Transform bone = animator.GetBoneTransform(originalBone);
        Transform boneParent = bone.parent;

        GameObject boneAlias = CreateGameObject(boneAliasName, boneParent, bone.localEulerAngles, bone.localPosition);

        ParentConstraint constraint = bone.gameObject.AddComponent<ParentConstraint>();
        constraint.locked = true;
        constraint.AddSource(new ConstraintSource
        {
            sourceTransform = boneAlias.transform,
            weight = 1,
        });
        constraint.constraintActive = true;
    }

    private void RenameBone(HumanDescription avatar, Animator animator, HumanBodyBones originalBone, string newName)
    {
        Transform bone = animator.GetBoneTransform(originalBone);

        for (int i = 0; i < avatar.human.Length; i++)
        {
            if (avatar.human[i].boneName == bone.gameObject.name)
                avatar.human[i].boneName = newName;
        }

        for (int i = 0; i < avatar.skeleton.Length; i++)
        {
            if (avatar.skeleton[i].name == bone.gameObject.name)
                avatar.skeleton[i].name = newName;
        }

        bone.gameObject.name = newName;
    }

    private HumanDescription CloneHumanDescription(HumanDescription original)
    {
        HumanDescription newDescription = new HumanDescription();
        newDescription.skeleton = new SkeletonBone[original.skeleton.Length];
        newDescription.human = new HumanBone[original.human.Length];
        
        for (int i = 0; i < newDescription.human.Length; i++)
        {
            newDescription.human[i] = new HumanBone
            {
                boneName = original.human[i].boneName,
                humanName = original.human[i].humanName,
                limit = original.human[i].limit,
            };
        }

        for (int i = 0; i < newDescription.skeleton.Length; i++)
        {
            newDescription.skeleton[i] = new SkeletonBone
            {
                name = original.skeleton[i].name,
                position = original.skeleton[i].position,
                rotation = original.skeleton[i].rotation,
                scale = original.skeleton[i].scale,
            };
        }

        return newDescription;
    }

    private void AddBone(ref HumanDescription humanDescription, Transform bone)
    {
        List<SkeletonBone> bones = humanDescription.skeleton.ToList();
        bones.Add(new SkeletonBone
        {
            name = bone.gameObject.name,
            position = bone.transform.position,
            rotation = bone.transform.rotation,
            scale = bone.transform.localScale
        });

        humanDescription.skeleton = bones.ToArray();
    }

    private void SpawnPreview(Transform parent, string previewName)
    {
        GameObject preview = Resources.Load<GameObject>(previewName);
        GameObject previewInstance = Instantiate(preview, parent, false);

        previewInstance.transform.localPosition = Vector3.zero;
        previewInstance.transform.localRotation = Quaternion.identity;
        previewInstance.transform.localScale = Vector3.one;

        parent.AddComponent<ForceSelection>();
        
        SetTagRecursive(previewInstance,"EditorOnly");
    }
    
    private void SetTagRecursive(GameObject go, string tag)
    {
        go.tag = tag;
        foreach (Transform child in go.transform)
        {
            child.gameObject.tag = tag;
 
            if (child.childCount > 0)
                SetTagRecursive(child.gameObject, tag);
        }
    }
}
