using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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

                        BuildPipeline.BuildAssetBundles(folderPath, new AssetBundleBuild[] { assetBundleBuild }, 0, EditorUserBuildSettings.activeBuildTarget);
                        //File.Move(Application.temporaryCachePath + "/" + fileName, path);
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
                EditorGUILayout.HelpBox("Model as more than no skinnedmesh.", MessageType.Error);
                return;
            }
            
            if (skinnedMeshRenderers.Length > 1)
            {
                EditorGUILayout.HelpBox("Model as more than 1 skinnedmesh.", MessageType.Error);
                return;
            }

            if (skinnedMeshRenderers[0].sharedMaterials.Length > 1)
            {
                EditorGUILayout.HelpBox("Model as more than 1 material.", MessageType.Error);
                return;
            }

            Animator animator = model.GetComponentInChildren<Animator>();
            
            if (!animator)
            {
                EditorGUILayout.HelpBox("Model as more than no animator.", MessageType.Error);
                return;
            }
            
            if (!animator.avatar || !animator.avatar.isHuman)
            {
                EditorGUILayout.HelpBox("Model is not humanoid.", MessageType.Error);
                return;
            }
            
            if (animator.GetBoneTransform(HumanBodyBones.Hips).parent != skinnedMeshRenderers[0].transform.parent)
            {
                EditorGUILayout.HelpBox("Hip bone doesn't have the same parent as the main skinnedmesh.", MessageType.Error);
                return;
            }
            
            if (animator.GetBoneTransform(HumanBodyBones.Hips).gameObject.name != "root")
            {
                EditorGUILayout.HelpBox("Hips bone is not named \"root\".", MessageType.Error);
                return;
            }
            
            if (!animator.GetBoneTransform(HumanBodyBones.Head))
            {
                EditorGUILayout.HelpBox("Model has no Head.", MessageType.Error);
                return;
            }
            
            if (animator.GetBoneTransform(HumanBodyBones.Head).gameObject.name != "head")
            {
                EditorGUILayout.HelpBox("Head bone is not named \"head\".", MessageType.Error);
                return;
            }
            
            if (!animator.GetBoneTransform(HumanBodyBones.LeftHand))
            {
                EditorGUILayout.HelpBox("Model has no Left Hand.", MessageType.Error);
                return;
            }
            
            if (animator.GetBoneTransform(HumanBodyBones.LeftHand).gameObject.name != "handl")
            {
                EditorGUILayout.HelpBox("LeftHand bone is not named \"handl\".", MessageType.Error);
                return;
            }
            
            if (!animator.GetBoneTransform(HumanBodyBones.RightHand))
            {
                EditorGUILayout.HelpBox("Model has no Right Hand.", MessageType.Error);
                return;
            }
            
            if (animator.GetBoneTransform(HumanBodyBones.RightHand).gameObject.name != "handr")
            {
                EditorGUILayout.HelpBox("RightHand bone is not named \"handr\".", MessageType.Error);
                return;
            }
            
            if (!animator.GetBoneTransform(HumanBodyBones.LeftFoot))
            {
                EditorGUILayout.HelpBox("Model has no Left Foot.", MessageType.Error);
                return;
            }
            
            if (animator.GetBoneTransform(HumanBodyBones.LeftFoot).gameObject.name != "footl")
            {
                EditorGUILayout.HelpBox("LeftFoot bone is not named \"footl\".", MessageType.Error);
                return;
            }
            
            if (!animator.GetBoneTransform(HumanBodyBones.RightFoot))
            {
                EditorGUILayout.HelpBox("Model has no Right Foot.", MessageType.Error);
                return;
            }
            
            if (animator.GetBoneTransform(HumanBodyBones.RightFoot).gameObject.name != "footr")
            {
                EditorGUILayout.HelpBox("RightFoot bone is not named \"footr\".", MessageType.Error);
                return;
            }
            
            if (!animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg))
            {
                EditorGUILayout.HelpBox("Model has no Left Lower Leg.", MessageType.Error);
                return;
            }
            
            if (animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).gameObject.name != "leg2l")
            {
                EditorGUILayout.HelpBox("LeftLowerLeg bone is not named \"leg2l\".", MessageType.Error);
                return;
            }
            
            if (!animator.GetBoneTransform(HumanBodyBones.RightLowerLeg))
            {
                EditorGUILayout.HelpBox("Model has no Right Lower Leg.", MessageType.Error);
                return;
            }
            
            if (animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).gameObject.name != "leg2r")
            {
                EditorGUILayout.HelpBox("RightLowerLeg bone is not named \"leg2r\".", MessageType.Error);
                return;
            }
            
            if (animator.GetBoneTransform(HumanBodyBones.Head).position.y > 1.48f)
            {
                float modelHeight = animator.GetBoneTransform(HumanBodyBones.Head).position.y;
                float heightdiff = 1.475f / modelHeight;
                
                EditorGUILayout.HelpBox("Model is too tall.\nYou should scale it by "+heightdiff, MessageType.Error);
                return;
            }
            
            if (animator.GetBoneTransform(HumanBodyBones.Head).position.y < 1.46f)
            {
                float modelHeight = animator.GetBoneTransform(HumanBodyBones.Head).position.y;
                float heightdiff = modelHeight / 1.475f;
                
                EditorGUILayout.HelpBox("Model is too short.\nYou should scale it by "+heightdiff, MessageType.Error);
                return;
            }

            selectedCharacter = (Characters)EditorGUILayout.EnumPopup("Character to replace", selectedCharacter);

            if (GUILayout.Button("Setup Model"))
            {
                model.name = nameSelection[selectedCharacter];
                
                GameObject phoneDirectionRoot = CreateGameObject("phoneDirectionRoot", model.transform, new Vector3(0, 0, -90));
                CreateGameObject("phoneDirection", phoneDirectionRoot.transform, new Vector3(0, 0, 0));
                
                CreateGameObject("skateboard", model.transform, new Vector3(0, 0, -90));
                
                CreateGameObject("handlIK", model.transform, new Vector3(0, 0, -90));
                CreateGameObject("handrIK", model.transform, new Vector3(0, 0, -90));
                
                GameObject bmxFrame = CreateGameObject("bmxFrame", model.transform, new Vector3(0, 0, -90));
                GameObject bmxGear = CreateGameObject("bmxGear", bmxFrame.transform, new Vector3(0, 0, 0));
                CreateGameObject("bmxPedalL", bmxGear.transform, new Vector3(0, 0, 0));
                CreateGameObject("bmxPedalR", bmxGear.transform, new Vector3(0, 0, 0));
                GameObject bmxHandlebars = CreateGameObject("bmxHandlebars", bmxFrame.transform, new Vector3(0, 0, 0));
                CreateGameObject("bmxWheelF", bmxHandlebars.transform, new Vector3(0, 0, 0));
                CreateGameObject("bmxWheelR", bmxFrame.transform, new Vector3(0, 0, 0));

                Transform spine2 = animator.GetBoneTransform(HumanBodyBones.Chest);

                GameObject jetpackPos = CreateGameObject("jetpackPos", spine2, new Vector3(0, -63.155f, 0));
                GameObject jetpack = CreateGameObject("jetpack", jetpackPos.transform, new Vector3(0, 0, 0));
                GameObject boostPos = CreateGameObject("boostPos", jetpack.transform, new Vector3(0, 0, 0));
                CreateGameObject("boost", boostPos.transform, new Vector3(0, 0, 0));
                
                GameObject propr = CreateGameObject("propr", animator.GetBoneTransform(HumanBodyBones.RightHand), new Vector3(0, 0, 0));
                GameObject propl = CreateGameObject("propl", animator.GetBoneTransform(HumanBodyBones.LeftHand), new Vector3(0, 0, 0));

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

    private GameObject CreateGameObject(string name, Transform parent, Vector3 baseRotation)
    {
        GameObject obj = new GameObject(name);
        obj.transform.parent = parent;
        obj.transform.localEulerAngles = baseRotation;
        obj.transform.localPosition = Vector3.zero;
        return obj;
    }
}
