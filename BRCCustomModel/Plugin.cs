#if !SDK
using BepInEx;
using System.IO;
using System;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Reptile;
using UnityEngine.TextCore.Text;

namespace BRCCustomModel
{
    public struct CustomModel
    {
        public BRCAvatarDescriptor avatarDescriptor;
        public GameObject fbx;
        public Material[] skins;
        public int blinkBlendshapeIndex;
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static string playerModelsPath => Path.Combine(Environment.CurrentDirectory, "PlayerModels");

        public static Dictionary<Characters, AssetBundle> customModelBundles;
        public static Dictionary<Characters, CustomModel> customModelAssets;

        private void Awake()
        {
            Logging.logger = Logger;

            if (!Directory.Exists(playerModelsPath))
                Directory.CreateDirectory(playerModelsPath);

            RefreshBundlePaths();

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID + ".patch");
            harmony.PatchAll();

            // Plugin startup logic
            Logging.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void RefreshBundlePaths()
        {
            customModelBundles = new Dictionary<Characters, AssetBundle>();
            customModelAssets = new Dictionary<Characters, CustomModel>();

            string[] files = Directory.GetFiles(playerModelsPath, "*.brc", SearchOption.AllDirectories);

            foreach ( string file in files )
            {
                if(Enum.TryParse(Path.GetFileNameWithoutExtension(file),out Characters bundleCharacter) && !customModelBundles.ContainsKey(bundleCharacter))
                {
                    Logging.LogInfo("Found avatar for "+ bundleCharacter.ToString() + " : " + file);

                    AssetBundle bundle = AssetBundle.LoadFromFile(file);

                    customModelBundles.Add(bundleCharacter, bundle);

                    Material[] skins = new Material[]
                    {
                        bundle.LoadAsset<Material>(bundleCharacter.ToString() + "Mat0"),
                        bundle.LoadAsset<Material>(bundleCharacter.ToString() + "Mat1"),
                        bundle.LoadAsset<Material>(bundleCharacter.ToString() + "Mat2"),
                        bundle.LoadAsset<Material>(bundleCharacter.ToString() + "Mat3"),
                    };

                    CustomModel customModel = new CustomModel
                    {
                        fbx = bundle.LoadAsset<GameObject>(bundleCharacter.ToString()),
                        skins = skins,
                    };

                    customModel.avatarDescriptor = customModel.fbx.GetComponent<BRCAvatarDescriptor>();


                    customModel.blinkBlendshapeIndex = -1;

                    if (!string.IsNullOrEmpty(customModel.avatarDescriptor.blinkBlendshape) && customModel.avatarDescriptor.blinkRenderer)
                    {
                        Mesh blinkMesh = customModel.avatarDescriptor.blinkRenderer.sharedMesh;

                        for (int i = 0; i < blinkMesh.blendShapeCount; i++)
                        {
                            if(blinkMesh.GetBlendShapeName(i) == customModel.avatarDescriptor.blinkBlendshape)
                            {
                                customModel.blinkBlendshapeIndex = i;
                                break;
                            }
                        }
                    }

                    customModelAssets.Add(bundleCharacter, customModel);
                }
            }
        }

        
    }
}
#endif