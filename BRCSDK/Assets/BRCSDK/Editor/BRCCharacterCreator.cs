using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;

public class BRCCharacterCreator : EditorWindow
{
    private GameObject model;
    private Characters selectedCharacter = Characters.RED;

    public enum Characters
    {
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
    
    private Dictionary<Characters, string> nameSelection = new Dictionary<Characters, string>
    {
        {Characters.VINYL,"girl1"},
        {Characters.FRANK,"frank"},
        {Characters.COIL,"ringdude"},
        {Characters.RED,"metalHead"},
        {Characters.TRYCE,"blockGuy"},
        {Characters.BEL,"spaceGirl"},
        {Characters.RAVE,"angel"},
        {Characters.DOT_EXE,"eightBall"},
        {Characters.SOLACE,"dummy"},
        {Characters.DJ_CYBER,"dj"},
        {Characters.ECLIPSE,"medusa"},
        {Characters.DEVIL_THEORY,"boarder"},
        {Characters.FAUX,"headMan"},
        {Characters.FLESH_PRINCE,"prince"},
        {Characters.RIETVELD,"jetpackBossPlayer"},
        {Characters.FELIX,"legendFace"},
        {Characters.OLDHEAD,"oldheadPlayer"},
        {Characters.BASE,"robot"},
        {Characters.JAY,"skate"},
        {Characters.MESH,"wideKid"},
        {Characters.FUTURISM,"futureGirl"},
        {Characters.RISE,"pufferGirl"},
        {Characters.SHINE,"bunGirl"},
        {Characters.FAUX_NO_JETPACK,"headManNoJetpack"},
        {Characters.DOT_EXE_BOSS,"eightBallBoss"},
        {Characters.RED_FELIX,"legendMetalHead"},
    };
    
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
            if (nameSelection.Values.Contains(model.name))
            {
                if(GUILayout.Button("ExportModel"))
                {
                    string path = EditorUtility.SaveFilePanel("Save brc avatar", "", model.name + ".brc", "brc");

                    if (path != "")
                    {
                        string fileName = Path.GetFileName(path);
                        string folderPath = Path.GetDirectoryName(path);
                        
                        AssetBundleBuild assetBundleBuild = default(AssetBundleBuild);
                        assetBundleBuild.assetNames = new string[] {
                            $"Assets/{model.name}.prefab",
                            $"Assets/{model.name}Mat0.mat",
                            $"Assets/{model.name}Mat1.mat",
                            $"Assets/{model.name}Mat2.mat",
                            $"Assets/{model.name}Mat3.mat",
                        };

                        assetBundleBuild.assetBundleName = fileName;
                        
                        DirectoryInfo d = new DirectoryInfo(Application.temporaryCachePath);
                        foreach (var file in d.GetFiles("*.manifest"))
                            file.Delete();

                        BuildPipeline.BuildAssetBundles(Application.temporaryCachePath, new AssetBundleBuild[] { assetBundleBuild }, 0, EditorUserBuildSettings.activeBuildTarget);
                        if(File.Exists(path))
                            File.Delete(path);
                        File.Move(Application.temporaryCachePath + "/" + fileName, path);
                        EditorUtility.DisplayDialog("Exportation Successful!", "Exportation Successful!", "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Exportation Failed!", "Path is invalid.", "OK");
                    }
                }

                return;
            }
            
            SkinnedMeshRenderer[] skinnedMeshRenderers = model.GetComponentsInChildren<SkinnedMeshRenderer>();
            
            if (skinnedMeshRenderers.Length == 0)
            {
                EditorGUILayout.HelpBox("Model has no skinnedmesh.", MessageType.Error);
            }
            
            if (skinnedMeshRenderers.Length > 1)
            {
                EditorGUILayout.HelpBox("Model has more than 1 skinnedmesh.", MessageType.Error);
            }

            if (skinnedMeshRenderers[0].sharedMaterials.Length > 1)
            {
                EditorGUILayout.HelpBox("Model has more than 1 material.", MessageType.Error);
                return;
            }

            Animator animator = model.GetComponentInChildren<Animator>();
            
            if (!animator)
            {
                EditorGUILayout.HelpBox("Model has no animator.", MessageType.Error);
                return;
            }
            
            if (!animator.avatar || !animator.avatar.isHuman)
            {
                EditorGUILayout.HelpBox("Model is not humanoid.", MessageType.Error);
                return;
            }
            
            if (!animator.GetBoneTransform(HumanBodyBones.Head))
            {
                EditorGUILayout.HelpBox("Model has no Head.", MessageType.Error);
                return;
            }
            
            if (!animator.GetBoneTransform(HumanBodyBones.LeftHand))
            {
                EditorGUILayout.HelpBox("Model has no Left Hand.", MessageType.Error);
                return;
            }
            
            if (!animator.GetBoneTransform(HumanBodyBones.RightHand))
            {
                EditorGUILayout.HelpBox("Model has no Right Hand.", MessageType.Error);
                return;
            }
            
            if (!animator.GetBoneTransform(HumanBodyBones.LeftFoot))
            {
                EditorGUILayout.HelpBox("Model has no Left Foot.", MessageType.Error);
                return;
            }
            
            if (!animator.GetBoneTransform(HumanBodyBones.RightFoot))
            {
                EditorGUILayout.HelpBox("Model has no Right Foot.", MessageType.Error);
                return;
            }
            
            if (!animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg))
            {
                EditorGUILayout.HelpBox("Model has no Left Lower Leg.", MessageType.Error);
                return;
            }
            
            if (!animator.GetBoneTransform(HumanBodyBones.RightLowerLeg))
            {
                EditorGUILayout.HelpBox("Model has no Right Lower Leg.", MessageType.Error);
                return;
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

            selectedCharacter = (Characters)EditorGUILayout.EnumPopup("Character to replace", selectedCharacter);

            if (GUILayout.Button("Setup Model"))
            {
                PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                HumanDescription oldAvatar = CloneHumanDescription(animator.avatar.humanDescription);

                Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                
                RenameBone(oldAvatar, animator, HumanBodyBones.Hips, "root");
                
                RenameBone(oldAvatar, animator, HumanBodyBones.LeftLowerLeg, "leg2l");
                RenameBone(oldAvatar, animator, HumanBodyBones.RightLowerLeg, "leg2r");
                RenameBone(oldAvatar, animator, HumanBodyBones.LeftHand, "handl");
                RenameBone(oldAvatar, animator, HumanBodyBones.RightHand, "handr");
                RenameBone(oldAvatar, animator, HumanBodyBones.Head, "head");
                
                if(hips.parent != model.transform)
                    hips.SetParent(model.transform);
                
                model.name = nameSelection[selectedCharacter];
                
                AddBone(ref oldAvatar, model.transform);
                
                Avatar newAvatar = AvatarBuilder.BuildHumanAvatar(model, oldAvatar);
                animator.avatar = newAvatar;
                
                CreateGameObject("footl", animator.GetBoneTransform(HumanBodyBones.LeftFoot));
                CreateGameObject("footr", animator.GetBoneTransform(HumanBodyBones.RightFoot));
                CreateGameObject("head", animator.GetBoneTransform(HumanBodyBones.Head));
                
                GameObject phoneDirectionRoot = CreateGameObject("phoneDirectionRoot", model.transform, new Vector3(0, 0, -90));
                CreateGameObject("phoneDirection", phoneDirectionRoot.transform);
                
                CreateGameObject("skateboard", model.transform, new Vector3(0, 0, -90));
                
                CreateGameObject("handlIK", model.transform, new Vector3(0, 0, -90));
                CreateGameObject("handrIK", model.transform, new Vector3(0, 0, -90));
                
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
                CreateGameObject("boost", boostPos.transform);
                
                GameObject propr = CreateGameObject("propr", animator.GetBoneTransform(HumanBodyBones.RightHand));
                GameObject propl = CreateGameObject("propl", animator.GetBoneTransform(HumanBodyBones.LeftHand));

                AssetDatabase.CreateAsset(newAvatar,"Assets/"+nameSelection[selectedCharacter]+"Avatar.asset");
                
                PrefabUtility.SaveAsPrefabAssetAndConnect(model, "Assets/"+nameSelection[selectedCharacter]+".prefab",
                    InteractionMode.AutomatedAction);

                Material skin1 = new Material(Shader.Find("Reptile/Ambient Character Fake"));
                AssetDatabase.CreateAsset(skin1,"Assets/"+nameSelection[selectedCharacter]+"Mat0.mat");
                
                Material skin2 = new Material(Shader.Find("Reptile/Ambient Character Fake"));
                AssetDatabase.CreateAsset(skin2,"Assets/"+nameSelection[selectedCharacter]+"Mat1.mat");
                
                Material skin3 = new Material(Shader.Find("Reptile/Ambient Character Fake"));
                AssetDatabase.CreateAsset(skin3,"Assets/"+nameSelection[selectedCharacter]+"Mat2.mat");
                
                Material skin4 = new Material(Shader.Find("Reptile/Ambient Character Fake"));
                AssetDatabase.CreateAsset(skin4,"Assets/"+nameSelection[selectedCharacter]+"Mat3.mat");
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
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

            /*string parentName = GetPrivateFieldValue<string>(avatar.skeleton[i], "parentName");
            if(parentName == bone.gameObject.name)
                SetPrivateFieldValue(avatar.skeleton[i], "parentName", newName);*/
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
    
    public static T GetPrivateFieldValue<T>(object obj, string propName)
    {
        if (obj == null) throw new ArgumentNullException("obj");
        Type t = obj.GetType();
        FieldInfo fi = null;
        while (fi == null && t != null)
        {
            fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            t = t.BaseType;
        }
        if (fi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
        return (T)fi.GetValue(obj);
    }
    
    public static void SetPrivateFieldValue<T>(object obj, string propName, T val)
    {
        if (obj == null) throw new ArgumentNullException("obj");
        Type t = obj.GetType();
        FieldInfo fi = null;
        while (fi == null && t != null)
        {
            fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            t = t.BaseType;
        }
        if (fi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
        fi.SetValue(obj, val);
    }
}
