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
    public struct ModelAssets
    {
        public GameObject fbx;
        public Material[] skins;
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static string playerModelsPath => Path.Combine(Environment.CurrentDirectory, "PlayerModels");

        public static Dictionary<Characters, AssetBundle> customModelBundles;
        public static Dictionary<Characters, ModelAssets> customModelAssets;

        private void Awake()
        {
            if (!Directory.Exists(playerModelsPath))
                Directory.CreateDirectory(playerModelsPath);

            RefreshBundlePaths();

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID + ".patch");
            harmony.PatchAll();

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void RefreshBundlePaths()
        {
            customModelBundles = new Dictionary<Characters, AssetBundle>();
            customModelAssets = new Dictionary<Characters, ModelAssets>();

            string[] files = Directory.GetFiles(playerModelsPath, "*.brc", SearchOption.AllDirectories);

            foreach ( string file in files )
            {
                if(Enum.TryParse(Path.GetFileNameWithoutExtension(file),out Characters bundleCharacter) && !customModelBundles.ContainsKey(bundleCharacter))
                {
                    Logger.LogInfo("Found avatar for "+ bundleCharacter.ToString() + " : " + file);

                    AssetBundle bundle = AssetBundle.LoadFromFile(file);

                    customModelBundles.Add(bundleCharacter, bundle);

                    Material[] skins = new Material[]
                    {
                        bundle.LoadAsset<Material>(bundleCharacter.ToString() + "Mat0"),
                        bundle.LoadAsset<Material>(bundleCharacter.ToString() + "Mat1"),
                        bundle.LoadAsset<Material>(bundleCharacter.ToString() + "Mat2"),
                        bundle.LoadAsset<Material>(bundleCharacter.ToString() + "Mat3"),
                    };

                    ModelAssets modelAssets = new ModelAssets
                    {
                        fbx = bundle.LoadAsset<GameObject>(bundleCharacter.ToString()),
                        skins = skins,
                    };

                    customModelAssets.Add(bundleCharacter, modelAssets);
                }
            }
        }
    }
}
