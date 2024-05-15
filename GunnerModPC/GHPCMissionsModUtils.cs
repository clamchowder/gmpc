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
    public partial class GHPCMissionsMod : MelonMod
    {
        void SetAmmoCount(Vehicle vehicle, int[] customAmmoCount, bool feed = false)
        {
            if (vehicle.LoadoutManager == null)
            {
                LoggerInstance.Msg($"{vehicle.name} has no LoadoutManager\n");
                return;
            }

            int[] totalAmmoCounts = vehicle.LoadoutManager.TotalAmmoCounts;
            for (int i = 0; i < totalAmmoCounts.Length && i < customAmmoCount.Length; i++) totalAmmoCounts[i] = customAmmoCount[i];
            SetLoadout(vehicle, feed);
        }

        void SetT72ApfsdsAmmo(Vehicle instantiatedVehicle)
        {
            AmmoClipCodexScriptable codex_3bm32 = Resources.FindObjectsOfTypeAll<AmmoClipCodexScriptable>().Where(o => o.name == "clip_3BM32").FirstOrDefault();
            AmmoClipCodexScriptable codex_3bm22 = Resources.FindObjectsOfTypeAll<AmmoClipCodexScriptable>().Where(o => o.name == "clip_3BM22").FirstOrDefault();
            AmmoClipCodexScriptable codex_3bm15 = Resources.FindObjectsOfTypeAll<AmmoClipCodexScriptable>().Where(o => o.name == "clip_3BM15").FirstOrDefault();

            instantiatedVehicle.LoadoutManager.LoadedAmmoTypes[0] = codex_3bm32;
            instantiatedVehicle.LoadoutManager.LoadedAmmoTypes[1] = codex_3bm22;
            instantiatedVehicle.LoadoutManager.LoadedAmmoTypes[2] = codex_3bm15;
            for (int i = 0; i < instantiatedVehicle.LoadoutManager.RackLoadouts.Length; i++)
            {
                GHPC.Weapons.AmmoRack rack = instantiatedVehicle.LoadoutManager.RackLoadouts[i].Rack;
                rack.ClipTypes[0] = codex_3bm32.ClipType;
                rack.ClipTypes[1] = codex_3bm22.ClipType;
                rack.ClipTypes[2] = codex_3bm15.ClipType;
            }

            SetLoadout(instantiatedVehicle);
        }

        void Set3BM22Ammo(Vehicle instantiatedVehicle)
        {
            /*AmmoClipCodexScriptable[] clips = Resources.FindObjectsOfTypeAll<AmmoClipCodexScriptable>();
            foreach (AmmoClipCodexScriptable clip in clips)
            {
                LoggerInstance.Msg($"{clip.name} is AmmoClipCodexScriptable");
            }*/

            AmmoClipCodexScriptable codex_3bm22 = Resources.FindObjectsOfTypeAll<AmmoClipCodexScriptable>().Where(o => o.name == "clip_3BM22").FirstOrDefault();
            AmmoClipCodexScriptable codex_3bm15 = Resources.FindObjectsOfTypeAll<AmmoClipCodexScriptable>().Where(o => o.name == "clip_3BM15").FirstOrDefault();

            instantiatedVehicle.LoadoutManager.LoadedAmmoTypes[1] = codex_3bm22;
            instantiatedVehicle.LoadoutManager.LoadedAmmoTypes[2] = codex_3bm15;
            for (int i = 0; i < instantiatedVehicle.LoadoutManager.RackLoadouts.Length; i++)
            {
                GHPC.Weapons.AmmoRack rack = instantiatedVehicle.LoadoutManager.RackLoadouts[i].Rack;
                rack.ClipTypes[0] = codex_3bm22.ClipType;
                rack.ClipTypes[1] = codex_3bm15.ClipType;
            }

            SetLoadout(instantiatedVehicle);
        }

        void SetM774Ammo(Vehicle instantiatedVehicle)
        {
            AmmoClipCodexScriptable m774_codex = Resources.FindObjectsOfTypeAll<AmmoClipCodexScriptable>().Where(o => o.name == "clip_M774").FirstOrDefault();
            if (m774_codex == null)
            {
                LoggerInstance.Error($"Could not find M774 AmmoClipCodexScriptable\n");
                return;
            }

            if (instantiatedVehicle.LoadoutManager == null)
            {
                LoggerInstance.Error($"Could not find loadout manager for vehicle\n");
                return;
            }

            instantiatedVehicle.LoadoutManager.LoadedAmmoTypes[0] = m774_codex;
            for (int i = 0; i < instantiatedVehicle.LoadoutManager.RackLoadouts.Length; i++)
            {
                GHPC.Weapons.AmmoRack rack = instantiatedVehicle.LoadoutManager.RackLoadouts[i].Rack;
                rack.ClipTypes[0] = m774_codex.ClipType;
            }

            SetLoadout(instantiatedVehicle);
        }

        // Replaces APFSDS with APHE for T-55. Budget Cuts
        void SetT55APHE(Vehicle vehicle)
        {
            AmmoClipCodexScriptable br412_codex = Resources.FindObjectsOfTypeAll<AmmoClipCodexScriptable>().Where(o => o.name == "clip_BR-412D").FirstOrDefault();
            if (br412_codex == null)
            {
                LoggerInstance.Error($"Could not find Stalinium round\n");
                return;
            }

            vehicle.LoadoutManager.LoadedAmmoTypes[0] = br412_codex;
            for (int i = 0; i < vehicle.LoadoutManager.RackLoadouts.Length; i++)
            {
                GHPC.Weapons.AmmoRack rack = vehicle.LoadoutManager.RackLoadouts[i].Rack;
                rack.ClipTypes[0] = br412_codex.ClipType;
            }
        }

        public void SpawnNeutralVehicle(GameObject vehicle, Vector3 position, Quaternion rotation, bool practiceTarget, out Vehicle instantiatedVehicle)
        {
            SpawnVehicle(vehicle, position, rotation, practiceTarget, Faction.Neutral, out instantiatedVehicle);
        }

        /// <summary>
        /// Spawns a vehicle
        /// </summary>
        /// <param name="vehicle">Vehicle to spawn</param>
        /// <param name="position">Position vector.</param>
        /// <param name="rotation">Vehicle orientation</param>
        /// <param name="practiceTarget">If true, vehicle will be abandoned</param>
        /// <param name="faction">Faction</param>
        /// <param name="instantiatedVehicle">Instantiated Vehicle object</param>
        public void SpawnVehicle(GameObject vehicle, Vector3 position, Quaternion rotation, bool practiceTarget, Faction faction, out Vehicle instantiatedVehicle)
        {
            instantiatedVehicle = null;
            if (vehicle != null)
            {
                GameObject instantiatedObj = GameObject.Instantiate(vehicle, position, rotation);
                instantiatedVehicle = instantiatedObj.GetComponent<Vehicle>();
                instantiatedVehicle.Allegiance = faction;

                if (instantiatedVehicle == null)
                {
                    LoggerInstance.Msg($"{vehicle.name} could not be instantiated");
                    return;
                }

                if (instantiatedVehicle.WeaponsManager == null) LoggerInstance.Msg($"{vehicle.name} WeaponsManager is null");
                else
                {
                    // LoggerInstance.Msg($"{vehicle.name} WeaponsManager: {instantiatedVehicle.WeaponsManager.name}");
                    WeaponSystemInfo[] weaponsSystems = instantiatedVehicle.WeaponsManager.Weapons;
                    for (int i = 0; i < weaponsSystems.Length; i++)
                    {

                        // make sure weapon audio is started. otherwise we'll get issues when attempting to abandon the vehicle
                        MethodInfo startMethodInfo = typeof(WeaponAudio).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (startMethodInfo == null)
                        {
                            continue;
                        }

                        if (weaponsSystems[i].Weapon.WeaponSound == null)
                        {
                            continue;
                        }

                        startMethodInfo.Invoke(weaponsSystems[i].Weapon.WeaponSound, new object[] { });
                    }
                }

                if (practiceTarget)
                {
                    instantiatedVehicle.NoPlayerControl = true;
                    instantiatedVehicle.InvokeKilled();
                    if (instantiatedVehicle.FlammablesMgr != null && reduceExtraTargetFlammability.Value) instantiatedVehicle.FlammablesMgr.enabled = false;
                }

                LoggerInstance.Msg($"{vehicle.name} successfully spawned at {vehicle.transform.position}");
            }
            else
            {
                LoggerInstance.Error($"Could not find Vehicle component in {vehicle.name} GameObject!");
            }
        }

        void SetLoadout(Vehicle vehicle, bool feed = false)
        {
            for (int i = 0; i < vehicle.LoadoutManager.RackLoadouts.Length; i++) EmptyRack(vehicle.LoadoutManager.RackLoadouts[i].Rack);
            vehicle.LoadoutManager.SpawnCurrentLoadout();

            // https://github.com/thebeninator/US-Reduced-Lethality/blob/master/ReducedLethality.cs with modifications
            WeaponSystem mainGun = vehicle.WeaponsManager.Weapons[0].Weapon;
            PropertyInfo roundInBreech = typeof(AmmoFeed).GetProperty("AmmoTypeInBreech");
            roundInBreech.SetValue(mainGun.Feed, null);

            if (feed)
            {
                MethodInfo refreshBreech = typeof(AmmoFeed).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
                refreshBreech.Invoke(mainGun.Feed, new object[] { });
            }

            MethodInfo registerAllBallistics = typeof(LoadoutManager).GetMethod("RegisterAllBallistics", BindingFlags.Instance | BindingFlags.NonPublic);
            registerAllBallistics.Invoke(vehicle.LoadoutManager, new object[] { });
        }

        // https://github.com/thebeninator/US-Reduced-Lethality/blob/master/Util.cs with modifications
        void EmptyRack(GHPC.Weapons.AmmoRack rack)
        {
            rack.StoredClips.Clear();
            MethodInfo remove_vis = typeof(GHPC.Weapons.AmmoRack).GetMethod("RemoveAmmoVisualFromSlot", BindingFlags.Instance | BindingFlags.NonPublic);
            rack.SlotIndicesByAmmoType = new Dictionary<AmmoType, List<byte>>();

            foreach (Transform transform in rack.VisualSlots)
            {
                AmmoStoredVisual vis = transform.GetComponentInChildren<AmmoStoredVisual>();

                if (vis != null && vis.AmmoType != null)
                {
                    remove_vis.Invoke(rack, new object[] { transform });
                }
            }
        }

        // Allows all units to be destroyed victory condition to be met if certain "unimportant" units
        // are still active
        void AppendUnimportantUnits(List<IUnit> units)
        {
            SceneUnitsManager unitsManager = Resources.FindObjectsOfTypeAll<SceneUnitsManager>().FirstOrDefault();
            if (unitsManager == null)
            {
                LoggerInstance.Msg("Could not find mission's SceneUnitsManager");
                return;
            }

            int arrLen = units.Count;
            if (unitsManager.Meta.UnimportantUnits != null)
            {
                arrLen += unitsManager.Meta.UnimportantUnits.Length;
            }

            Unit[] unimportantUnitsArr = new Unit[arrLen];
            int unitIdx = 0;
            if (unitsManager.Meta.UnimportantUnits != null)
            {
                for (; unitIdx < unitsManager.Meta.UnimportantUnits.Length; unitIdx++)
                {
                    unimportantUnitsArr[unitIdx] = unitsManager.Meta.UnimportantUnits[unitIdx];
                }
            }

            foreach (Unit unit in units)
            {
                unimportantUnitsArr[unitIdx] = unit;
                unitIdx++;
            }

            unitsManager.Meta.UnimportantUnits = unimportantUnitsArr;
        }

        void SetMissionTime(float dayTime, float nightTime)
        {
            CelestialSky celestialSky = Object.FindObjectOfType<CelestialSky>();
            if (celestialSky != null)
            {
                celestialSky.t = SceneController.IsDaytime ? dayTime : nightTime;
                LoggerInstance.Msg("Set time to " + celestialSky.t + " (" + (SceneController.IsDaytime ? "Day" : "Night") + ")");
            }
            else
            {
                LoggerInstance.Msg("Could not find celestial sky");
            }
        }
    }
}
