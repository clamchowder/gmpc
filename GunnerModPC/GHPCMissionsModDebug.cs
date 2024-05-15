using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using GHCPMissionsMod;
using GHPC;
using GHPC.AI;
using GHPC.Mission;
using GHPC.Mission.Data;
using GHPC.Player;
using GHPC.UI;
using GHPC.Vehicle;
using GHPC.Weaponry.Artillery;
using GHPC.Weaponry.CAS;
using GHPC.Weapons;
using GHPC.Weapons.Artillery;
using GHPC.World;
using GHPCMissionsMod;
using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GHPCMissionsMod
{
    /// <summary>
    /// Container for debugging methods
    /// </summary>
    public partial class GHPCMissionsMod : MelonMod
    {
        /// <summary>
        /// Logs existing vehicles and their positions
        /// </summary>
        void DumpVehiclePositions()
        {
            IEnumerable<Vehicle> vehicles = Resources.FindObjectsOfTypeAll<Vehicle>();
            foreach (Vehicle vehicle in vehicles)
            {
                LoggerInstance.Msg("Found vehicle " + vehicle.name);
                if (vehicle.transform != null)
                {
                    Quaternion rot = vehicle.transform.rotation;
                    Vector3 pos = vehicle.transform.position;
                    LoggerInstance.Msg($"  Vehicle is at ({pos.x}, {pos.y}, {pos.z}), rotation ({rot.x}, {rot.y}, {rot.z}, {rot.w})");
                }
            }
        }

        /// <summary>
        /// Logs close air support available for a mission
        /// </summary>
        void DumpCasInfo()
        {
            CasSupportManager casSupportManager = Resources.FindObjectsOfTypeAll<CasSupportManager>().FirstOrDefault();
            FieldInfo casMissionsAvailable = typeof(CasAirframeUnit).GetField("_missionsAvailable", BindingFlags.Instance | BindingFlags.NonPublic);

            if (casSupportManager == null)
            {
                LoggerInstance.Msg("No CAS support manager");
                return;
            }

            if (casSupportManager.BlueCasAirframes != null)
            {
                foreach (CasAirframeUnit casUnit in casSupportManager.BlueCasAirframes)
                {
                    if (casUnit != null) LoggerInstance.Msg("Blue has cas unit " + casUnit.FriendlyName + " with " + casMissionsAvailable.GetValue(casUnit) + " missions available");
                }
            }

            if (casSupportManager.RedCasAirframes != null)
            {
                foreach (CasAirframeUnit casUnit in casSupportManager.RedCasAirframes)
                {
                    if (casUnit != null) LoggerInstance.Msg("Red has cas unit " + casUnit.FriendlyName + " with " + casMissionsAvailable.GetValue(casUnit) + " missions available");
                }
            }
        }

        /// <summary>
        /// Logs artillery battery parameters
        /// </summary>
        void DumpFireSupportInfo()
        {
            FireMissionManager fireMissionManager = Resources.FindObjectsOfTypeAll<FireMissionManager>().FirstOrDefault();
            FieldInfo remainingMissionFieldInfo = typeof(ArtilleryBattery).GetField("_missionsAvailable", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo fireMissionShotsFieldInfo = typeof(ArtilleryBattery).GetField("_shots", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo shotDelayIntervalFieldInfo = typeof(ArtilleryBattery).GetField("_interShotDelaySeconds", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo batteryMunitionsFieldInfo = typeof(ArtilleryBattery).GetField("_munitions", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo callDelayFieldInfo = typeof(ArtilleryBattery).GetField("_onCallImpactDelay", BindingFlags.Instance | BindingFlags.NonPublic);
            LoggerInstance.Msg("Blue artillery batteries:");
            foreach (ArtilleryBattery artilleryBattery in fireMissionManager.BlueArtilleryBatteries)
            {
                LoggerInstance.Msg("Artillery battery: " + artilleryBattery.FriendlyName + " has " + remainingMissionFieldInfo.GetValue(artilleryBattery) + " missions, shots: " + fireMissionShotsFieldInfo.GetValue(artilleryBattery));
                LoggerInstance.Msg("  Inter-shot delay: " + shotDelayIntervalFieldInfo.GetValue(artilleryBattery));
                LoggerInstance.Msg("  Shot spawn height: " + artilleryBattery.SpawnHeight);
                LoggerInstance.Msg("  Shot spawn angle: " + artilleryBattery.SpawnAngle);
                LoggerInstance.Msg("  On-call delay: " + callDelayFieldInfo.GetValue(artilleryBattery));
            }

            LoggerInstance.Msg("Red artillery batteries:");
            foreach (ArtilleryBattery artilleryBattery in fireMissionManager.RedArtilleryBatteries)
            {
                LoggerInstance.Msg("Artillery battery: " + artilleryBattery.FriendlyName + " has " + remainingMissionFieldInfo.GetValue(artilleryBattery) + " missions, shots: " + fireMissionShotsFieldInfo.GetValue(artilleryBattery));
                LoggerInstance.Msg("  Inter-shot delay: " + shotDelayIntervalFieldInfo.GetValue(artilleryBattery));
                LoggerInstance.Msg("  Shot spawn height: " + artilleryBattery.SpawnHeight);
                LoggerInstance.Msg("  Shot spawn angle: " + artilleryBattery.SpawnAngle);
                LoggerInstance.Msg("  On-call delay: " + callDelayFieldInfo.GetValue(artilleryBattery));
            }
        }

        void DumpVehicleLoadout(Vehicle vehicle)
        {
            if (vehicle.LoadoutManager == null)
            {
                LoggerInstance.Msg($"{vehicle.name} has no LoadoutManager\n");
                return;
            }

            LoggerInstance.Msg(vehicle.name + ":");
            int[] totalAmmoCounts = vehicle.LoadoutManager.TotalAmmoCounts;
            for (int i = 0; i < totalAmmoCounts.Length; i++)
            {
                LoggerInstance.Msg("    " + vehicle.LoadoutManager.LoadedAmmoTypes[i].name + ": " + totalAmmoCounts[i]);
            }
        }

        /// <summary>
        /// Logs unit spawner prefabs (vehicles)
        /// </summary>
        /// <param name="unitSpawner"></param>
        public void DumpUnitSpawnerPrefabs(UnitSpawner unitSpawner)
        {
            if (unitSpawner == null)
            {
                return;
            }

            FieldInfo prefabsFieldInfo = typeof(UnitSpawner).GetField("_prefabLookupCached", BindingFlags.Instance | BindingFlags.NonPublic);
            Dictionary<string, GameObject> prefabs = prefabsFieldInfo.GetValue(unitSpawner) as Dictionary<string, GameObject>;
            if (prefabs != null)
            {
                foreach (KeyValuePair<string, GameObject> pair in prefabs)
                {
                    LoggerInstance.Msg("  -> " + pair.Key);
                }
            }
            else
            {
                LoggerInstance.Msg("Prefabs dictionary is null");
            }

            if (unitSpawner.PrefabLookup != null)
            {
                LoggerInstance.Msg("  unit spawner has prefab lookup");
                foreach (var unitData in unitSpawner.PrefabLookup.AllUnits)
                {
                    LoggerInstance.Msg("    Prefab has unit " + unitData.Name);
                }
            }
        }

        // Crazy experiments
        private static BindingFlags InstanceBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private struct SerializeStackEntry
        {
            public object obj;  // object ref
            public string type; // object type
            public string name; // name to use in generated code
        }

        public string SerializeVehicle(Vehicle input, int maxStack)
        {
            // Stuff that doesn't need traversal
            List<string> simpleTypes = new List<string>() {
                "System.Boolean", "System.Single", "System.Double", "System.Int32", "System.Int64", "System.Int16", "System.String"};
            List<KeyValuePair<object, int>> createdObjs = new List<KeyValuePair<object, int>>(); // duplicate detection, would love to use a hashtable but eh
            StringBuilder sb = new StringBuilder();

            // dfs stack
            Stack<SerializeStackEntry> stk = new Stack<SerializeStackEntry>();
            SerializeStackEntry firstEntry = new SerializeStackEntry();
            firstEntry.obj = input;
            firstEntry.type = "GHPC.Vehicle.Vehicle"; // do I need to save this
            firstEntry.name = "input";
            stk.Push(firstEntry);
            int valueIndex = 0;
            Assembly currentAssembly = typeof(Vehicle).Assembly;

            while (stk.Count > 0)
            {
                SerializeStackEntry currentEntry = stk.Pop();
                sb.AppendLine($"// --------\n// Visiting {currentEntry.name} type {currentEntry.type}\n\n");
                FieldInfo[] fields = currentEntry.obj.GetType().GetFields(InstanceBindingFlags);
                foreach (FieldInfo field in fields)
                {
                    object v = field.GetValue(currentEntry.obj);
                    string fieldType = field.FieldType.FullName;
                    Type tFieldType = currentAssembly.GetType(fieldType); // just hope it's all in the same assembly

                    if (v == null)
                    {
                        LoggerInstance.Msg($"Skipping: {fieldType} {currentEntry.name} => {fieldType} v{valueIndex} = NULL");
                        continue;
                    }

                    // Determine if it's a simple type
                    bool isSimpleType = false;
                    foreach (string simpleType in simpleTypes)
                    {
                        if (simpleType.Equals(fieldType))
                        {
                            isSimpleType = true;
                            break;
                        }
                    }

                    if (isSimpleType)
                    {
                        LoggerInstance.Msg($"Simple type: {currentEntry.obj.GetType()} {currentEntry.name}.{field.Name} ==> {fieldType} v{valueIndex} = {v}");
                        sb.Append($"{fieldType} v{valueIndex} = {v};\n");
                        sb.Append($"typeof({currentEntry.obj.GetType()}).GetField(\"{field.Name}\", InstanceBindingFlags).SetValue({currentEntry.name}, v{valueIndex});\n");
                    }
                    else if (tFieldType != null && tFieldType.IsEnum)
                    {
                        LoggerInstance.Msg($"Enumerated type: {currentEntry.obj.GetType()} {currentEntry.name}.{field.Name} ==> {fieldType} v{valueIndex} = {fieldType}.{v}");
                        sb.Append($"typeof({currentEntry.obj.GetType()}).GetField(\"{field.Name}\", InstanceBindingFlags).SetValue({currentEntry.name}, {fieldType}.{v});\n");
                    }
                    else if (tFieldType != null && tFieldType.IsArray)
                    {
                        foreach (object arrayItem in (Array)v)
                        {

                        }
                    }
                    else
                    {
                        // Avoid duplicates
                        bool duplicateHandled = false;
                        for (int i = 0; i < createdObjs.Count; i++)
                        {
                            if (ReferenceEquals(createdObjs[i].Key, v))
                            {
                                LoggerInstance.Msg($"Duplicate ref: {currentEntry.obj.GetType()} {currentEntry.name}.{field.Name} ==> v{valueIndex}");
                                sb.Append($"typeof({currentEntry.obj.GetType()}).GetField(\"{field.Name}\", InstanceBindingFlags).SetValue(input, v{createdObjs[i].Value});\n");
                                duplicateHandled = true;
                                break;
                            }
                        }

                        if (!duplicateHandled)
                        {
                            if (stk.Count > maxStack)
                            {
                                sb.Append($"// Covering {fieldType} {field.Name} would exceed stack limit\n");
                                continue;
                            }

                            string refComment = $"Reference type: {currentEntry.obj.GetType()} {currentEntry.name}.{field.Name} ==> {fieldType} v{valueIndex} = {v}";
                            sb.Append("// " + refComment + "\n");
                            sb.Append($"{fieldType} v{valueIndex} = typeof({currentEntry.obj.GetType()}).GetField(\"{field.Name}\", InstanceBindingFlags).GetValue({currentEntry.name});\n");
                            LoggerInstance.Msg(refComment);

                            SerializeStackEntry newEntry = new SerializeStackEntry();
                            newEntry.obj = v;
                            newEntry.type = fieldType;
                            newEntry.name = "v" + valueIndex;
                            stk.Push(newEntry);

                            createdObjs.Add(new KeyValuePair<object, int>(v, valueIndex));
                        }
                    }

                    valueIndex++;
                }
            }


            return sb.ToString();
        }
    }
}
