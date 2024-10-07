using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MapMagic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnlimitedTownBuilding
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "Glowstick.UnlimitedTownBuilding";
        public const string NAME = "UnlimitedTownBuilding";
        public const string VERSION = "1.2.0";

        internal static ManualLogSource Log;

        public static ConfigEntry<bool> ignoreColliders;
        public static ConfigEntry<bool> infiniteResources;
        public static ConfigEntry<KeyCode> destroyKey;
        public static ConfigEntry<bool> bypassDestroyConfirmation;
        public static ConfigEntry<bool> instantConstruction;
        
        public static Building buildingToDestroy { get; private set; }

        internal void Awake()
        {
            Log = this.Logger;
            Log.LogMessage($"Hello world from {NAME} {VERSION}!");
            
            ignoreColliders = Config.Bind("General", "Ignore Colliders", true, "Ignore colliders when placing buildings, when off it will be hard to place buildings outside new sirocco.");
            infiniteResources = Config.Bind("General", "Infinite Resources", false, "Infinite resources when placing buildings. WARNING: If you turn this off you will have all your resources set to 0.");
            destroyKey = Config.Bind("General", "Destroy Key", KeyCode.Delete, "Keybind to remove buildings.");
            bypassDestroyConfirmation = Config.Bind("General", "Bypass Destroy Confirmation", false, "Bypass the confirmation dialog when destroying buildings.");
            instantConstruction = Config.Bind("General", "Instant Construction", false, "Buildings construct and upgrade instantly.");
            
            new Harmony(GUID).PatchAll();
        }

        internal void Update()
        {
            if (Input.GetKeyDown(destroyKey.Value))
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    DestroyUnityObject(hit);
                }
            }
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
                if (ignoreColliders.Value)
                {
                    __instance.m_detectionScript.Disable();
                }
            }
        }
        
        // Show blue preview of placement
        [HarmonyPatch(typeof(OrientOnTerrain), "IsValid", MethodType.Getter)]
        public class OrientOnTerrain_IsValid
        {
            static void Prefix(OrientOnTerrain __instance)
            {
                if (ignoreColliders.Value)
                {
                    __instance.m_placementValid = true;
                }
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
                if (infiniteResources.Value)
                {
                    __instance.m_resources[0].Value = 99999;
                    __instance.m_resources[1].Value = 99999;
                    __instance.m_resources[2].Value = 99999;
                    __instance.m_resources[3].Value = 99999;
                } else {
                    __instance.m_resources[0].Value = 0;
                    __instance.m_resources[1].Value = 0;
                    __instance.m_resources[2].Value = 0;
                    __instance.m_resources[3].Value = 0;
                }
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
        
        
        // instantly finish construction
        [HarmonyPatch(typeof(Building), "ActivateLevelVisuals", new Type[] {})]
        public class Building_ActivateLevelVisuals
        {
            static void Prefix(Building __instance)
            {
                if (instantConstruction.Value)
                {
                    int num = 5;
                    while (num >= 0 && __instance.CurrentPhase.ConstructionType == Building.ConstructionPhase.Type.WIP)
                    {
                        bool flag = __instance.m_currentBasicPhaseIndex < 0;
                        if (flag)
                        {
                            __instance.m_currentBasicPhaseIndex = 0;
                        }
                        else
                        {
                            __instance.m_currentBasicPhaseIndex++;
                        }

                        num--;
                    }
                    __instance.m_remainingConstructionTime = 0f;
                }
            }
        }
        
        [HarmonyPatch(typeof(Building), "StartUpgrade")]
        public class Building_StartUpgrade
        {
            static void Postfix(Building __instance)
            {
                if (instantConstruction.Value)
                {
                    __instance.m_remainingConstructionTime = 0f;
                    __instance.m_currentUpgradePhaseIndex = __instance.m_pendingUpgradePhaseIndex;
                    __instance.m_pendingUpgradePhaseIndex = -1;
                    __instance.UpdateConstruction(0f);
                }
            }
        }
        
        // destroy unity object of the closest building to the raycast hit
        public static void DestroyUnityObject(RaycastHit hit)
        {
            Building[] buildings = Resources.FindObjectsOfTypeAll<Building>();
            Building closestBuilding = null;
            float closestDistance = float.MaxValue;
            foreach (Building building in buildings)
            {
                float distance = Vector3.Distance(building.transform.position, hit.point);
                if (distance < closestDistance && distance < 30)
                {
                    closestBuilding = building;
                    closestDistance = distance;
                }
            }
            if (closestBuilding != null)
            {
                buildingToDestroy = closestBuilding;
                
                CharacterManager characterManager = CharacterManager.Instance;
                Character character = characterManager.GetFirstLocalCharacter();
                CharacterUI characterUI = character.CharacterUI;
                if (characterUI != null)
                {
                    if (bypassDestroyConfirmation.Value)
                    {
                        DestroyBuilding();
                        return;
                    }
                    characterUI.MessagePanel.Show("Destroy building " + closestBuilding.DisplayName + "?", "Confirm", DestroyBuilding, null);
                }
            }
        }

        private static void DestroyBuilding()
        {
            Log.LogMessage($"Destroying building {buildingToDestroy.name}");
            BuildingResourcesManager.Instance.UnregisterBuiding(buildingToDestroy);
            ItemManager.Instance.SendDestroyItem(buildingToDestroy.UID);
            Destroy(buildingToDestroy.gameObject);
        }
    }
}
