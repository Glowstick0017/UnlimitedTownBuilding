using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnlimitedTownBuilding
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "Glowstick.UnlimitedTownBuilding";
        public const string NAME = "UnlimitedTownBuilding";
        public const string VERSION = "1.1.2";

        internal static ManualLogSource Log;

        internal void Awake()
        {
            Log = this.Logger;
            Log.LogMessage($"Hello world from {NAME} {VERSION}!");

            new Harmony(GUID).PatchAll();
        }

        // Allow unlimited buildings
        [HarmonyPatch(typeof(BuildingResourcesManager), "GetCanDeployNewBuilding")]
        public class BuildingResourcesManager_GetCanDeployNewBuilding
        {
            static void Prefix(BuildingResourcesManager __instance)
            {
                __instance.m_specializedBuildingCount = 0;
                __instance.m_houseCount = 0;
            }
        }

        // Allow placement without collider
        [HarmonyPatch(typeof(OrientOnTerrain), "HasEnoughRoom", MethodType.Getter)]
        public class OrientOnTerrain_HasEnoughRoom
        {
            static void Prefix(OrientOnTerrain __instance)
            {
                __instance.m_detectionScript.Disable();
            }
        }
        
        // Show blue preview of placement
        [HarmonyPatch(typeof(OrientOnTerrain), "IsValid", MethodType.Getter)]
        public class OrientOnTerrain_IsValid
        {
            static void Prefix(OrientOnTerrain __instance)
            {
                __instance.m_placementValid = true;
            }
        }
        
        // Build blueprints anywhere
        [HarmonyPatch(typeof(BuildingResourcesManager), "CanBaseBuildInCurrentScene", MethodType.Getter)]
        public class BuildingResourcesManager_CanBaseBuildInCurrentScene
        {
            static void Prefix(BuildingResourcesManager __instance)
            {
                __instance.m_canBaseBuildInCurrentScene = true;
            }
        }
        
        // pretend all buildings are houses in order to place more than one of the same specialized building
        [HarmonyPatch(typeof(Building), "IsHouse", MethodType.Getter)]
        public class Building_IsHouse
        {
            static void Prefix(Building __instance)
            {
                __instance.BuildingType = Building.BuildingTypes.House;
            }
        }
        
        // Remove contruction costs
        [HarmonyPatch(typeof(BuildingResourcesManager), "GetResourceValue")]
        public class BuildingResourcesManager_GetResourceValue
        {
            static void Prefix(BuildingResourcesManager __instance)
            {
                __instance.m_resources[0].Value = 99999;
                __instance.m_resources[1].Value = 99999;
                __instance.m_resources[2].Value = 99999;
                __instance.m_resources[3].Value = 99999;
            }
        }
        
        // remove housing requirements
        [HarmonyPatch(typeof(BuildingResourcesManager), "HousingValue", MethodType.Getter)]
        public class BuildingResourcesManager_HousingValue
        {
            static void Prefix(BuildingResourcesManager __instance)
            {
                __instance.m_housingValue = 999;
            }
        }
        
        // do not destroy buildings after 7 days
        [HarmonyPatch(typeof(AreaManager), "IsAreaExpired")]
        public class AreaManager_IsAreaExpired
        {
            static void Postfix(ref bool __result)
            {
                __result = false;
            }
        }
        
        // construction time is finishes in 1 day
        [HarmonyPatch(typeof(Building), "StartConstructionTimer")]
        public class Building_StartConstructionTimer
        {
            static void Postfix(Building __instance)
            {
                __instance.m_remainingConstructionTime = 1;
            }
        }

    }
}
