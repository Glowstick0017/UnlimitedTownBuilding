﻿using BepInEx;
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
        public const string VERSION = "1.0.0";

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

    }
}
