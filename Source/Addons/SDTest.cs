﻿using CustomPreLaunchChecks;
using KerbalKonstructs.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Test implementation for Simga88
/// </summary>

namespace KerbalKonstructs.Addons
{
    class SDTest
    {

        private static Dictionary<string, string> pos2Group = new Dictionary<string, string>();
        private static Dictionary<string, Vector3> group2Center = new Dictionary<string, Vector3>();

        // needed for dynamic group center
        private static Dictionary<string, List<Vector3>> groupMembers = new Dictionary<string, List<Vector3>>();


        private static bool initialized = false;

        //public static void ScanParticles()
        //{
        //    //var pliste = Resources.FindObjectsOfTypeAll<ParticleSystem>();

        //    //foreach (var system in pliste)
        //    //{
        //    //    Log.Normal("ParticleSystem: " + system.name);
        //    //}

        //    //var prliste = Resources.FindObjectsOfTypeAll<ParticleSystemRenderer>();

        //    //foreach (var renderer in prliste)
        //    //{
        //    //    Log.Normal("ParticleRenderer: " + renderer.name);
        //    //    Log.Normal("ParticleRendererMat: " + renderer.material.name);
        //    //}

        //}

        internal static void SpawnWorker()
        {
            ProtoCrewMember protoCrew = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
            protoCrew.trait = "Worker";


            protoCrew.rosterStatus = ProtoCrewMember.RosterStatus.Available;
        }

        internal static GameObject pickerPrefab;

        internal static void LoadBundles()
        {

            string bundleFileName;
            GameObject myPrefab;

            switch (Application.platform)
            {
                case RuntimePlatform.OSXPlayer:
                    bundleFileName = "colorpickerbundle.bundle";
                    break;
                case RuntimePlatform.LinuxPlayer:
                    bundleFileName = "colorpickerbundle.bundle";
                    break;
                default:
                    bundleFileName = "colorpickerbundle.bundle";
                    break;
            }
            string bundlePath = KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/Prefabs/" + bundleFileName;

            AssetBundle prefabBundle = null;
            try
            {
                prefabBundle = AssetBundle.LoadFromFile(bundlePath);
                if (prefabBundle == null)
                {
                    Log.Normal("Failed to load shader asset file: " + bundlePath);
                    return;
                }


                    Log.Normal("Loading Prefab: " + "assets/kkcolorpicker.prefab");
                    myPrefab = prefabBundle.LoadAsset<GameObject>("assets/kkcolorpicker.prefab");
                    pickerPrefab = myPrefab;
                    pickerPrefab.SetActive(false);
                    GameObject.DontDestroyOnLoad(pickerPrefab);


                //foreach (string prefabName in prefabBundle.GetAllAssetNames())
                //{
                //    Log.Normal("Loading Prefab: " + prefabName);
                //    myPrefab = prefabBundle.LoadAsset<GameObject>(prefabName);
                //    pickerPrefab = myPrefab;
                //    pickerPrefab.SetActive(false);
                //    GameObject.DontDestroyOnLoad(pickerPrefab);
                    
                //}

            }
            catch (System.Exception exeption)
            {
                Log.Error("Error loading assetbundle " + exeption);
            }
            finally
            {
                if (prefabBundle != null)
                {
                    prefabBundle.Unload(false);
                }
            }

        }


        private static void Add2Group(string groupName, Vector3 pos)
        {
            if (!groupMembers.ContainsKey(groupName))
            {
                groupMembers.Add(groupName, new List<Vector3>());
            }
            groupMembers[groupName].Add(pos);

            if (!pos2Group.ContainsKey(pos.ToString()))
            {
                pos2Group.Add(pos.ToString(), groupName);
            }
        }

        internal static AsmUtils.Detour compileIDDetour;

        public static void InstallDetour()
        {
            MethodBase origFunction = typeof(HierarchyUtil).GetMethod("CompileID", BindingFlags.Static | BindingFlags.Public);
            MethodBase newFunction = typeof(SDTest).GetMethod("NewCompileID", BindingFlags.Static | BindingFlags.Public);

            compileIDDetour = new AsmUtils.Detour(origFunction, newFunction);
            compileIDDetour.Install();
        }



        public static string NewCompileID(Transform trf, string rootName)
        {
            GameObject instance = trf.gameObject;

            string returnString = compileID(trf, "", rootName).Replace(" ", "");
            Log.Normal("Return String: " + returnString);

            return returnString;
        }

        private static string compileID(Transform trf, string tId, string rootName)
        {
            if (trf.name != rootName)
            {
                if (trf.parent != null)
                {
                    tId = tId + compileID(trf.parent, tId, rootName) + "/";
                }
                //else if (!string.IsNullOrEmpty(rootName))
                //{
                //    //Debug.LogError("[HierarchyUtil]: CompileID failed because it reached the scene root without finding a 'SpaceCenter' game object", trf);
                //}
            }
            tId += trf.name;
            return tId;
        }

        private static void ScanGroups()
        {
            string group = "";
            string key = "";

            group2Center.Add("KSCUpgrades", new Vector3(157000, -1000, -570000));

            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {
                group = (string)instance.Group;
                key = instance.RadialPosition.ToString();

                switch (group)
                {
                    case "KSCUpgrades":
                        break;
                    case "Ungrouped":
                        break;
                    default:
                        // we only update the groupcenter when we are not KSCUgrades or Ungrouped
                        if (!group2Center.ContainsKey(group))
                        {
                            group2Center.Add(group, Vector3.zero);
                        }
                        int count = (groupMembers.ContainsKey(group)) ? groupMembers[group].Count : 0;
                        group2Center[group] = group2Center[group] * count / (count + 1) + instance.RadialPosition / (count + 1);
                        break;
                }

                Add2Group(group, instance.RadialPosition);
            }
            initialized = true;
        }




        /// <summary>
        /// Returns Vector3.zero if no group is found, else the Groups Center
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector3 GetCenter(Vector3 position)
        {
            if (!initialized)
            {
                ScanGroups();
            }

            if (!pos2Group.ContainsKey(position.ToString()))
            {
                return Vector3.zero;
            }
            string groupName = pos2Group[position.ToString()];

            if (groupName == "Ungrouped")
            {
                return position;
            }

            return group2Center[groupName];
        }


        /// <summary>
        /// Prints out the usage of each model
        /// </summary>
        public static void GetModelStats()
        {
            int modelcount = 0;

            int vertexcount = 0;
            int triangles = 0;

            Log.Normal("111: Modelname , Number of instances , Vertex count , triangles");
            foreach (StaticModel model in StaticDatabase.allStaticModels)
            {
                if (model.isSquad)
                    continue;
                modelcount = StaticDatabase.GetInstancesFromModel(model).Count();
                // Log.Normal("Model " + model.configPath + " has: " + modelcount);
                var meshfilter = model.prefab.GetComponentsInChildren<MeshFilter>(true);
                if (meshfilter == null)
                    continue;

                foreach (var mf in meshfilter)
                {
                    Mesh mesh = mf.mesh;
                    triangles += mesh.triangles.Length;
                    vertexcount += mesh.vertexCount;

                }
                Log.Normal("111:" + model.configPath + " , " + model.title + " , " + modelcount + " , " + vertexcount + " , " + triangles / 3);
                vertexcount = 0;
                triangles = 0;
            }
        }


        internal static void WriteTextures()
        {

            Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "GameData/KKTextures/");
            Texture2D mainTexture = null;
            Texture2D emissiveTexture = null;
            HashSet<Texture2D> mytextures = new HashSet<Texture2D>();


            foreach (StaticModel model in StaticDatabase.allStaticModels)
            {
                // we don't want to rebuild the sqad models, so we don't need the textures
                if (model.isSquad)
                    continue;


                int texCount = 0;
                foreach (var renderer in model.prefab.GetComponentsInChildren<MeshRenderer>(true))
                {
                    texCount++;
                    mainTexture = renderer.material.GetTexture("_MainTex") as Texture2D;

                    if (mainTexture != null && !mytextures.Contains(mainTexture))
                    {
                        var mainTextureblob = CopyTexture(mainTexture).EncodeToPNG();
                        // For testing purposes, also write to a file in the project folder
                        File.WriteAllBytes(KSPUtil.ApplicationRootPath + "GameData/KKTextures/" + model.name + "_MainTex" + texCount.ToString() + ".png", mainTextureblob);
                        mytextures.Add(mainTexture);
                    }

                    emissiveTexture = renderer.material.GetTexture("_Emissive") as Texture2D;


                    if (emissiveTexture != null && !mytextures.Contains(emissiveTexture))
                    {
                        var emissiveTextureblob = CopyTexture(emissiveTexture).EncodeToPNG();
                        // For testing purposes, also write to a file in the project folder
                        File.WriteAllBytes(KSPUtil.ApplicationRootPath + "GameData/KKTextures/" + model.name + "_Emissive" + texCount.ToString() + ".png", emissiveTextureblob);
                        mytextures.Add(emissiveTexture);
                    }

                }

            }



        }

        internal static void WriteTexture(Texture2D texture)
        {
            var mainTextureblob = CopyTexture(texture).EncodeToPNG();
            // For testing purposes, also write to a file in the project folder
            File.WriteAllBytes(KSPUtil.ApplicationRootPath + "GameData/KKTextures/" + texture.name + ".png", mainTextureblob);
        }

        private static Texture2D CopyTexture(Texture2D texture)
        {

            RenderTexture tmp = RenderTexture.GetTemporary(
            texture.width,
            texture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture, tmp);

            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;

            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;

            // Create a new readable Texture2D to copy the pixels to it
            Texture2D myTexture2D = new Texture2D(texture.width, texture.height);

            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply(false,false);

            // Reset the active RenderTexture
            RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);

            UnityEngine.Object.Destroy(tmp);

            return myTexture2D;
        }




        internal static void DumpBuildInTextures()
        {
            foreach (var texture in Resources.FindObjectsOfTypeAll<Texture>())
            {
                Log.Normal("Texture: " + texture.name);
            }
        }



    }
}
