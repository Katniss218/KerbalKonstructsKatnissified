﻿using KerbalKonstructs.Core;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Upgradeables;

namespace KerbalKonstructs.Modules
{
    internal static class CareerState
    {
        private static void LoadFacilitiesLegacy(ConfigNode facilityNodes)
        {

            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {
                if (!instance.hasFacilities)
                {
                    continue;
                }

                if (!facilityNodes.HasNode(CareerUtils.KeyFromString(instance.RadialPosition.ToString())))
                {
                    Log.UserWarning("No entry found in savegame: " + instance.gameObject.name);
                    continue;
                }

                ConfigNode instanceNode = facilityNodes.GetNode(CareerUtils.KeyFromString(instance.RadialPosition.ToString()));
                foreach (var facNode in instanceNode.GetNodes())
                {
                    int index = int.Parse(facNode.GetValue("Index"));
                    if (instance.myFacilities[index].FacilityType == facNode.name)
                    {
                        //Log.Normal("Load State: " + instance.pqsCity.name + " : "  + facNode.name);
                        instance.myFacilities[index].LoadCareerConfig(facNode);

                    }
                    else
                    {
                        Log.UserError("Facility Index Missmatch in fac: " + instance.gameObject.name);
                    }
                }

            }

        }


        private static void LoadFacilitiesUUID(ConfigNode facilityNodes)
        {

            foreach (ConfigNode instanceNode in facilityNodes.nodes)
            {
                if (!StaticDatabase.instancedByUUID.ContainsKey(instanceNode.name))
                {
                    Log.UserWarning("No entry found in database for UUID: " + instanceNode.name);
                    continue;
                }

                StaticInstance instance = StaticDatabase.instancedByUUID[instanceNode.name];

                foreach (var facNode in instanceNode.GetNodes())
                {
                    int index = int.Parse(facNode.GetValue("Index"));
                    if (instance.myFacilities[index].FacilityType == facNode.name)
                    {
                        //Log.Normal("Load State: " + instance.pqsCity.name + " : "  + facNode.name);
                        instance.myFacilities[index].LoadCareerConfig(facNode);

                    }
                    else
                    {
                        Log.UserError("Facility Index Missmatch in fac: " + instance.gameObject.name);
                    }
                }

            }

        }

        /// <summary>
        /// saves the facility settings to the cfg file
        /// </summary>
        internal static void SaveFacilities(ConfigNode facilityNodes)
        {

            foreach (StaticInstance instance in StaticDatabase.GetAllStatics())
            {
                if (!instance.hasFacilities)
                {
                    continue;
                }

                ConfigNode instanceNode = facilityNodes.AddNode(instance.UUID);
                instanceNode.SetValue("FacilityName", instance.gameObject.name, true);
                instanceNode.SetValue("FacilityType", instance.facilityType.ToString(), true);

                for (int i = 0; i < instance.myFacilities.Count; i++)
                {
                    ConfigNode facnode = instanceNode.AddNode(instance.myFacilities[i].FacilityType);
                    facnode.SetValue("Index", i, true);
                    instance.myFacilities[i].SaveCareerConfig(facnode);
                }
            }
        }


        /// <summary>
        /// Loads the state of the LauchSites
        /// </summary>
        internal static void LoadLaunchSitesLegacy(ConfigNode launchSiteNodes)
        {
            foreach (KKLaunchSite site in LaunchSiteManager.allLaunchSites)
            {
                //Log.Normal("Loading LS: " + site.LaunchSiteName + " " + site.isOpen);
                ConfigNode lsNode;
                if (launchSiteNodes.HasNode(CareerUtils.LSKeyFromName(site.LaunchSiteName)))
                {
                    lsNode = launchSiteNodes.GetNode(CareerUtils.LSKeyFromName(site.LaunchSiteName));
                    LaunchSiteParser.LoadCareerConfig(site, lsNode);
                }
                //Log.Normal("Loading LS: " + site.LaunchSiteName + " " + site.isOpen);
            }



        }

        /// <summary>
        /// Loads the state of all facilities and LaunchSites
        /// </summary>
        internal static void Load(ConfigNode kkcfgNode)
        {
            ConfigNode facNode;
            ConfigNode lsNode;
            Log.PerfStart("Loading");

            bool useUUID = kkcfgNode.HasValue("useUUID");

            if (kkcfgNode.HasNode("Facilities"))
            {
                facNode = kkcfgNode.GetNode("Facilities");
                if (useUUID)
                {
                    LoadFacilitiesUUID(facNode);
                }
                else
                {
                    LoadFacilitiesLegacy(facNode);
                }
            }
            if (kkcfgNode.HasNode("LaunchSites"))
            {
                lsNode = kkcfgNode.GetNode("LaunchSites");
                LoadLaunchSitesLegacy(lsNode);
            }
            Log.PerfStop("Loading");

        }


        internal static void SaveLaunchsites(ConfigNode launchSiteNode)
        {
            string name = null;

            foreach (KKLaunchSite site in LaunchSiteManager.allLaunchSites)
            {
                //Log.Normal("Saving LS: " + site.LaunchSiteName + " " + site.isOpen);
                name = CareerUtils.LSKeyFromName(site.LaunchSiteName);
                ConfigNode lsNode = launchSiteNode.AddNode(name);
                LaunchSiteParser.SaveCareerConfig(site, lsNode);
            }
        }

        internal static void Save(ConfigNode kkcfgNode)
        {
            ConfigNode facNode;
            ConfigNode lsNode;

            kkcfgNode.SetValue("useUUID", true, true);

            if (kkcfgNode.HasNode("Facilities"))
            {
                facNode = kkcfgNode.GetNode("Facilities");
                facNode.ClearData();
            }
            else
            {
                facNode = kkcfgNode.AddNode("Facilities");
            }
            SaveFacilities(facNode);

            if (kkcfgNode.HasNode("LaunchSites"))
            {
                lsNode = kkcfgNode.GetNode("LaunchSites");
                lsNode.ClearData();
            }
            else
            {
                lsNode = kkcfgNode.AddNode("LaunchSites");
            }

            SaveLaunchsites(lsNode);




        }

        internal static void ResetFacilitiesOpenState()
        {
            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {
                if (instance.hasFacilities)
                {
                    foreach (KKFacility facility in instance.myFacilities)
                    {
                        facility.ResetToDefaultState();
                    }
                }
                if (instance.hasLauchSites)
                {
                    instance.launchSite.ResetToDefaultState();
                }
            }
        }


        internal static void FixKSCFacilities()
        {
            if ((HighLogic.LoadedScene == GameScenes.SPACECENTER) && (!KerbalKonstructs.InitialisedFacilities))
            {
                bool newGame = false;
                string saveConfigPath = string.Format("{0}saves/{1}/persistent.sfs", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);
                if (File.Exists(saveConfigPath))
                {
                    Log.Debug("Found persistent.sfs");
                    ConfigNode rootNode = ConfigNode.Load(saveConfigPath);
                    ConfigNode rootrootNode = rootNode.GetNode("GAME");

                    foreach (ConfigNode ins in rootrootNode.GetNodes("SCENARIO"))
                    {
                        // Debug.Log("KK: ConfigNode is " + ins);
                        if (ins.GetValue("name") == "ScenarioUpgradeableFacilities")
                        {
                            Log.Debug("Found ScenarioUpgradeableFacilities in persistent.sfs");

                            foreach (string kscBuilding in new List<string> {
                                    "SpaceCenter/LaunchPad",
                                    "SpaceCenter/Runway",
                                    "SpaceCenter/VehicleAssemblyBuilding",
                                    "SpaceCenter/SpaceplaneHangar",
                                    "SpaceCenter/TrackingStation",
                                    "SpaceCenter/AstronautComplex",
                                    "SpaceCenter/MissionControl",
                                    "SpaceCenter/ResearchAndDevelopment",
                                    "SpaceCenter/Administration",
                                    "SpaceCenter/FlagPole",
                                    "SpaceCenter/Observatory" })
                            {
                                if (!ins.HasNode(kscBuilding))
                                {
                                    Log.Normal("Could not find " + kscBuilding + " node. Creating node.");
                                    ConfigNode node = ins.AddNode(kscBuilding);
                                    node.AddValue("lvl", 0);
                                    rootNode.Save(saveConfigPath);
                                    newGame = true;
                                }
                            }
                            break;
                        }
                    }

                    if (newGame)
                    {
                        Log.Normal("Resetting Facilitiy Levels");
                        rootNode.Save(saveConfigPath);
                        foreach (UpgradeableFacility facility in GameObject.FindObjectsOfType<UpgradeableFacility>())
                        {
                            facility.SetLevel(0);
                        }
                    }


                    KerbalKonstructs.InitialisedFacilities = true;
                }
            }
        }

    }
}
