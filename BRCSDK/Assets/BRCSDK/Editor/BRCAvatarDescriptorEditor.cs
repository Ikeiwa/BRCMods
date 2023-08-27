using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BRCCustomModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BRCAvatarDescriptor))]
public class BRCAvatarDescriptorEditor : Editor
{
    private Dictionary<Characters, string> nameSelection = new Dictionary<Characters, string>
    {
        {Characters.NONE,"none"},
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
    
    SerializedProperty blinkRenderer;
    SerializedProperty blinkBlendshape;
    SerializedProperty character;
    SerializedProperty skins;
    
    private void OnEnable()
    {
        blinkRenderer = serializedObject.FindProperty("blinkRenderer");
        blinkBlendshape = serializedObject.FindProperty("blinkBlendshape");
        character = serializedObject.FindProperty("character");
        skins = serializedObject.FindProperty("skins");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        BRCAvatarDescriptor avatarDescriptor = (BRCAvatarDescriptor)target;


        List<string> blendshapes = new List<string>();
        int currentBlendshape = -1;
        
        if (!avatarDescriptor.blinkRenderer)
        {
            blinkRenderer.objectReferenceValue = target.GetComponentInChildren<SkinnedMeshRenderer>();
        }
        else
        {
            for (int i = 0; i < avatarDescriptor.blinkRenderer.sharedMesh.blendShapeCount; i++)
            {
                string blendshapeName = avatarDescriptor.blinkRenderer.sharedMesh.GetBlendShapeName(i);
                blendshapes.Add(blendshapeName);

                if (avatarDescriptor.blinkBlendshape == blendshapeName)
                {
                    currentBlendshape = i;
                }
                
                if (string.IsNullOrEmpty(avatarDescriptor.blinkBlendshape) && blendshapeName.ToLower() == "blink")
                {
                    avatarDescriptor.blinkBlendshape = blendshapeName;
                    break;
                }
            }
        }
        
        Characters currentCharacter = (Characters)character.intValue;
        character.intValue = (int)(Characters)EditorGUILayout.EnumPopup("Character", currentCharacter);

        EditorGUILayout.PropertyField(blinkRenderer);

        blinkBlendshape.stringValue = blendshapes[EditorGUILayout.Popup("Blink Blendshape", currentBlendshape, blendshapes.ToArray())];

        EditorGUILayout.PropertyField(skins);
        
        serializedObject.ApplyModifiedProperties();
        
        GUILayout.Space(20);

        bool valid = character.intValue > -1 && character.intValue < 28;
        
        valid = skins.arraySize == 4;
        foreach (var skin in avatarDescriptor.skins)
        {
            if (!skin)
            {
                valid = false;
                break;
            }
        }

        GUI.enabled = valid;
        if (GUILayout.Button("ExportModel"))
        {
            ExportModel();
        }

    }

    private void ExportModel()
    {
        BRCAvatarDescriptor avatarDescriptor = (BRCAvatarDescriptor)target;
        
        string characterName = nameSelection[(Characters)character.intValue];
        string path = EditorUtility.SaveFilePanel("Save brc avatar", "", characterName + ".brc", "brc");

        if (path != "")
        {
            string fileName = Path.GetFileName(path);
            string folderPath = Path.GetDirectoryName(path);
            
            GameObject avatarClone = Instantiate(avatarDescriptor.GameObject());
            foreach (Transform child in avatarClone.GetComponentsInChildren<Transform>())
            {
                if(child != null && child.CompareTag("EditorOnly")) DestroyImmediate(child.gameObject);
            }

            foreach (ForceSelection child in avatarClone.GetComponentsInChildren<ForceSelection>())
            {
                if (child != null) DestroyImmediate(child);
            }
            
            PrefabUtility.SaveAsPrefabAsset(avatarClone, "Assets/"+characterName+".prefab");
            DestroyImmediate(avatarClone);
            
            AssetBundleBuild assetBundleBuild = default(AssetBundleBuild);
            assetBundleBuild.assetNames = new string[] {
                $"Assets/{characterName}.prefab",
                $"Assets/{characterName}Mat0.mat",
                $"Assets/{characterName}Mat1.mat",
                $"Assets/{characterName}Mat2.mat",
                $"Assets/{characterName}Mat3.mat",
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
}
