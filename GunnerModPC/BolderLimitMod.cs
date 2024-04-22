using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using GHCPMissionsMod;
using GHPC;
using GHPC.AI;
using GHPC.Mission;
using GHPC.Mission.Data;
using GHPC.Player;
using GHPC.State.Interfaces;
using GHPC.UI;
using GHPC.UI.Hud;
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GHPCMissionsMod
{
    public partial class GHPCMissionsMod : MelonMod
    {
        public int BolderLimitCount = 0;
        public volatile int BolderLimitKilledVehicles = 0;
        public bool IsModifiedBolderLimit = false;
        public bool BolderLimitNextWave = false;
        public List<Vehicle> BolderLimitExtraVehiclesList;
        public Stack<string> BolderLimitExtraVehicleTypes;
        public Stack<string> BolderLimitMessages;
        Vector3[] BolderLimitSpawnPositions =
        {
            //3984.022f, 87.9567f, 2283.522f
            new Vector3(3751.748f, 95.2313f, 2022.171f),
            new Vector3(3764.023f, 95.2384f, 2070.675f),
            new Vector3(3744.537f, 96.1197f, 1972.968f),

            // Platoon 1 at 3863.944f, 98.2398f, 2347.308f
            new Vector3(3863.944f, 98.2398f, 2347.308f),
            new Vector3(3884.802f, 98.7436f, 2393.446f),
            new Vector3(3852.643f, 96.2397f, 2298.714f),
        };

        Vector3[] BolderLimitDestinations =
        {
            new Vector3(1041.873f, 71.0616f, 2922.854f),
            new Vector3(1041.873f, 71.0616f, 2922.854f),
            new Vector3(1041.873f, 71.0616f, 2922.854f),
            new Vector3(1041.873f, 71.0616f, 2922.854f),
            new Vector3(1041.873f, 71.0616f, 2922.854f),
            new Vector3(1041.873f, 71.0616f, 2922.854f),
        };

        Quaternion BolderLimitDefaultRotation = new Quaternion(0.0022f, -.6004f, -.0436f, .7985f);
        UnitSpawner BolderLimitUnitSpawner = null;

        public void InitializeBolderLimit(UnitSpawner unitSpawner)
        {
            BolderLimitUnitSpawner = unitSpawner;

            BolderLimitExtraVehicleTypes = new Stack<string>();
            BolderLimitExtraVehicleTypes.Push("T3485");
            BolderLimitExtraVehicleTypes.Push("T54A");
            BolderLimitExtraVehicleTypes.Push("T62");
            BolderLimitExtraVehicleTypes.Push("T80B");
            BolderLimitExtraVehicleTypes.Push("T80B");
            SpawnBolderLimitVehicles(BolderLimitExtraVehicleTypes.Pop());

            BolderLimitMessages = new Stack<string>();
            BolderLimitMessages.Push("Soviet Commander: Ha ha ha ha haa");
            BolderLimitMessages.Push("Soviet Commander: For great Union of Soviets");
            BolderLimitMessages.Push("Soviet Commander: You have no chance to survive make your time");
            BolderLimitMessages.Push("Soviet Commander: All your base are belong to us");

            IEnumerable<Vehicle> m1Tanks = GameObject.FindObjectsOfType<Vehicle>().Where(o => o.name.StartsWith("M1"));
            int[] increasedDartCount = new int[] { 46, 6 };
            foreach (Vehicle m1 in m1Tanks)
            {
                SetAmmoCount(m1, increasedDartCount, feed: true);
            }

            IsModifiedBolderLimit = true;

            // remove smoke from red to even the odds
            FireMissionManager fireMissionManager = Resources.FindObjectsOfTypeAll<FireMissionManager>().FirstOrDefault();
            FieldInfo remainingMissionFieldInfo = typeof(ArtilleryBattery).GetField("_missionsAvailable", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (ArtilleryBattery battery in fireMissionManager.RedArtilleryBatteries)
            {
                remainingMissionFieldInfo.SetValue(battery, 0);
            }

            ModMessageStyle = new GUIStyle();
            ModMessageStyle.fontSize = 36;
            ModMessageStyle.normal.textColor = Color.white;

            BolderLimitKilledVehicles = 0;
        }

        private bool NextBolderLimitMessage = false;
        public void BolderLimitUpdate()
        {
            if (BolderLimitNextWave && (BolderLimitExtraVehicleTypes.Count() > 0))
            {
                BolderLimitNextWave = false;
                SpawnBolderLimitVehicles(BolderLimitExtraVehicleTypes.Pop());
                NextBolderLimitMessage = true;
            }
        }

        private Stopwatch BolderLimitMessageStopwatch;
        private string CurrentBolderLimitMessage = null;
        private GUIStyle ModMessageStyle;

        public void BolderLimitMessage()
        {
            if (NextBolderLimitMessage && BolderLimitMessages.Count() > 0)
            {
                NextBolderLimitMessage = false;
                BolderLimitMessageStopwatch = Stopwatch.StartNew();
                LoggerInstance.Msg("Test message");
                CurrentBolderLimitMessage = BolderLimitMessages.Pop();
            }

            if (BolderLimitMessageStopwatch != null)
            {
                GUI.Label(new Rect((float)Screen.width / 2.5f, (float)Screen.height / 1.5f, 550f, 50f), CurrentBolderLimitMessage, ModMessageStyle);
                if (BolderLimitMessageStopwatch.ElapsedMilliseconds > 6000) BolderLimitMessageStopwatch = null;
            }
        }

        void SpawnBolderLimitVehicles(string unitSpawnerName)
        {
            LoggerInstance.Msg("Initiated spawn of " + unitSpawnerName);
            WaypointHolder wpHolderTemplate = GameObject.FindFirstObjectByType(typeof(WaypointHolder)) as WaypointHolder;
            if (BolderLimitExtraVehiclesList == null)
            {
                BolderLimitExtraVehiclesList = new List<Vehicle>();
            }

            for (int i = 0; i < BolderLimitSpawnPositions.Length; i++)
            {
                UnitMetaData metaData = new UnitMetaData();
                metaData.Name = "T80B_" + BolderLimitCount + "_" + i;
                metaData.Allegiance = Faction.Red;
                metaData.Position = BolderLimitSpawnPositions[i];
                metaData.Rotation = BolderLimitDefaultRotation;
                metaData.UnitType = UnitType.GroundVehicle;

                WaypointHolder waypointHolder = GameObject.Instantiate(wpHolderTemplate);
                waypointHolder.waypoints = new IWaypoint[1];
                waypointHolder.waypoints[0] = new VectorWaypoint(BolderLimitDestinations[i]);
                Vehicle t80 = BolderLimitUnitSpawner.SpawnUnit(unitSpawnerName, metaData, waypointHolder) as Vehicle;
                t80.Killed += HandleVehicleKilled;
                BolderLimitExtraVehiclesList.Add(t80);
            }

            BolderLimitCount++;
        }

        void HandleVehicleKilled()
        {
            // LoggerInstance.Msg("Test: Invoked Killed");
            Interlocked.Increment(ref BolderLimitKilledVehicles);

            // Don't wait for it to hit 0 because that's the mission end condition
            if (BolderLimitExtraVehiclesList.Count - BolderLimitKilledVehicles <= 1)
            {
                BolderLimitNextWave = true;
            }
        }
    }
}
