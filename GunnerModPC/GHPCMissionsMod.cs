using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using GHPC;
using GHPC.AI;
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
using JetBrains.Annotations;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(GHPCMissionsMod.GHPCMissionsMod), "GHPC Missions Mod", "0.0.1", "Clamchowder")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace GHPCMissionsMod
{
    public class GHPCMissionsMod : MelonMod
    {
        public static MelonPreferences_Category config;
        public static MelonPreferences_Entry<bool> t3485GrafenwoehrPatchEnabled;
        public static MelonPreferences_Entry<bool> extraPactTargetsGrafenwoehrPatchEnabled;
        public static MelonPreferences_Entry<bool> extraNatoTargetsGrafenwoehrPatchEnabled;
        public static MelonPreferences_Entry<bool> reduceExtraTargetFlammability;
        public static MelonPreferences_Entry<bool> extraHeAmmoVehiclesGrafenWoehrPatchEnabled;
        public static MelonPreferences_Entry<bool> writeDebugTxt;

        public List<Vehicle> Claustrophobia_t34_list;
        public List<Vector3> Claustrophobia_t34_positions;
        public IEnumerable<Vehicle> Claustrophobia_t55_list;

        public override void OnInitializeMelon()
        {
            config = MelonPreferences.CreateCategory("GMPCConfig");
            t3485GrafenwoehrPatchEnabled = config.CreateEntry<bool>("t3485GrafenwoehrPatchEnabled", true);
            extraPactTargetsGrafenwoehrPatchEnabled = config.CreateEntry<bool>("extraPactTargetsGrafenwoehrPatchEnabled", true);
            extraNatoTargetsGrafenwoehrPatchEnabled = config.CreateEntry<bool>("extraNatoTargetsGrafenwoehrPatchEnabled", true);
            extraHeAmmoVehiclesGrafenWoehrPatchEnabled = config.CreateEntry<bool>("extraHeAmmoVehiclesGrafenWoehrPatchEnabled", true);
            reduceExtraTargetFlammability = config.CreateEntry<bool>("reduceExtraTargetFlammability", true);
            writeDebugTxt = config.CreateEntry<bool>("writeDebugTxt", false);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            LoggerInstance.Msg($"Loaded scene {sceneName}");
            if (sceneName.StartsWith("MainMenu") || sceneName.Equals("t64_menu"))
            {
                // Make sure mission descriptions are slightly more accurate
                MissionMenuSetup missionData = Resources.FindObjectsOfTypeAll<MissionMenuSetup>().FirstOrDefault();
                FieldInfo scriptableMissionField = typeof(MissionMenuSetup).GetField("_allMissionsScriptable", BindingFlags.Instance | BindingFlags.NonPublic);
                AllMissionsMetaDataScriptable scriptableMissionData = (AllMissionsMetaDataScriptable)scriptableMissionField.GetValue(missionData);

                FieldInfo missionDescription = typeof(FactionMissionInfo).GetField("_description", BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (MissionTheaterScriptable theater in scriptableMissionData.Theaters)
                {
                    // LoggerInstance.Msg("Theater: " + theater.name);
                    foreach (MissionMetaData missionMetaData in theater.Missions)
                    {
                        if (missionMetaData.MissionName.Equals("Reservist Recon"))
                        {
                            missionMetaData.MissionName = "Reservist Spoiling Attack (modded)";

                            // there's only one faction
                            FactionMissionInfo missionInfo = missionMetaData.FactionInfo[0];
                            string newDesc = "Situation - Comrade, the enemy is assembling for a major attack and must be disrupted. ";
                            newDesc += "We would use artillery, but the truck delivering 152mm shells took a turn too fast and flipped over.\n\n";
                            newDesc += "Fortunately another reservist unit with T-34-85s is available to assist. Since 85mm is almost 100mm which is almost 125mm which is almost 152mm, command has judged available firepower to be adequate.\n\n";
                            newDesc += "Enemy - Tanks, APCs, and helicopters. Expect a screen of APCs backed up by patrolling tanks and helicopters.\n\n";
                            newDesc += "Friendly - 2x PT-76, 4x T-34\n\n";
                            newDesc += "Mission - Find the enemy assembly point. Then use the T-34's powerful 85mm gun to mercilessly destroy them!\n\n";
                            newDesc += "Other - The mission will be considered complete once you spot the enemy's main body and return to the start point. Do not return until you've destroyed the enemy!";
                            missionDescription.SetValue(missionInfo, newDesc);
                        }
                        else if (missionMetaData.MissionName.Equals("Kinetic Key"))
                        {
                            missionMetaData.MissionName = "Kinetic Key (modded)";
                            FactionMissionInfo missionInfo = missionMetaData.FactionInfo[0];
                            // LoggerInstance.Msg("Mission info: " + missionInfo.Description);
                            string newDesc = "Situation - You are an M60A3 platoon tasked with interdicting a large enemy attack.\n\n";
                            newDesc += "Enemy - Lots of T-72s and BMP-1s\n\n";
                            newDesc += "Mission - Stop them before they reach Objective June and steal our cheese. Need I say more?\n\n";
                            newDesc += "Other - A congressional dispute over whether pizza is a fruit or a vegetable has led to a government shutdown. ";
                            newDesc += "Unfortunately that means a shortage of M833 ammo. We'll have to use M774 for now. On the bright side, the Air Force is aware of this situation and has additional aircraft ready to support us.\n\n";
                            newDesc += "End Conditions-\n";
                            newDesc += "-Blue Victory- 85% enemies destroyed\n";
                            newDesc += "-Blue Defeat- 100% friendlies destroyed\n";
                            missionDescription.SetValue(missionInfo, newDesc);
                        }
                        else if (missionMetaData.MissionName.Equals("Replen Reaper"))
                        {
                            missionMetaData.MissionName = "Replen Reaper (modded)";
                            FactionMissionInfo missionInfo = missionMetaData.FactionInfo[0];
                            string newDesc = "Situation - You are a M2 scout section. The enemy has set up a replenishment line in the town over the hill and you are to destroy them.\n\n";
                            newDesc += "Enemy - 1x Company with attached screen, 3x BMP-1 platoons, 1x BDRM-2 section, various trucks. That's it...right?\n\n";
                            newDesc += "Friendly - 2x M2 Bradley in your section.\n\n";
                            newDesc += "Mission - Should you choose to accept it, your section is to locate and destroy the enemy company.\n\n";
                            newDesc += "Other - Command has heard the rumors about Soviets showing up at the replenishment point. Additional artillery batteries have been made available just in case. ";
                            newDesc += "The rumors are probably nothing but if you see any T-80Bs, do not approach them as they may hurt you. Call in artillery and get out of there.";
                            missionDescription.SetValue(missionInfo, newDesc);
                        }
                        else if (missionMetaData.MissionName.Equals("Claustrophobia"))
                        {
                            missionMetaData.MissionName = "Claustrophobia (modded)";
                            FactionMissionInfo missionInfo = missionMetaData.FactionInfo[0];
                            string newDesc = "Situation - You are a M2 platoon tasked with defending a village crucial to the local cheese production economy. The enemy is expected to use all means at their disposal to take the cheese.\n\n";
                            newDesc += "Enemy - 1x Company with attached tanks, 3x BMP-1 platoons, 1x T-55 section.\n\n";
                            newDesc += "Friendly - 2x M2 Bradley in your section. 1x M1, somewhere.\n\n";
                            newDesc += "Mission - Your platoon is to defend the town and prevent East Germany from addressing their cheese shortage.\n\n";
                            newDesc += "Other - SIGINT reports East German reservists are furious at receiving pizza without cheese. They may be joining the assault against your position. A M1 has been sent to provide overwatch in case things get out of hand.\n\n";
                            newDesc += "End Conditions -\n\n-Blue Victory: 70% enemies destroyed OR 50% enemies destroyed (they're unable to take the cheese back) and friendlies retreated\n-Blue Defeat: 75% friendlies destroyed";
                            missionDescription.SetValue(missionInfo, newDesc);
                        }

                        // LoggerInstance.Msg("  Mission: " + missionMetaData.MissionName);
                    }
                }
            }

            Claustrophobia_t55_list = null;
            Claustrophobia_t34_list = null;
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            LoggerInstance.Msg($"Initialized scene {sceneName}, trying to patch game...");

            if (sceneName == "TR01_showcase")
            {
                // try to enumerate vehicles
                GameObject t3485 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "T-34-85").FirstOrDefault() as GameObject;
                GameObject t72 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "T72M1").FirstOrDefault() as GameObject;
                GameObject bmp1 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "BMP1").FirstOrDefault() as GameObject;
                GameObject t55 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "T55A").FirstOrDefault() as GameObject;
                GameObject m60a1 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "M60A1").FirstOrDefault() as GameObject;
                GameObject bmp2 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "BMP2").FirstOrDefault() as GameObject;
                GameObject m1ip = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "M1IP").FirstOrDefault() as GameObject;
                GameObject bdrm = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "BRDM2").FirstOrDefault() as GameObject;
                GameObject btr60 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "BTR60PB").FirstOrDefault() as GameObject;
                GameObject pt76 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "PT76").FirstOrDefault() as GameObject;
                GameObject t64a = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "T64A").FirstOrDefault() as GameObject;
                GameObject t62 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "T62").FirstOrDefault() as GameObject;
                GameObject t80 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "T80B").FirstOrDefault() as GameObject;

                if (t3485GrafenwoehrPatchEnabled.Value)
                {
                    if (t3485 != null)
                    {
                        SpawnNeutralVehicle(t3485, new Vector3(1159f, 22f, 1614f), new Quaternion(0f, 0.8f, 0f, -0.8f), false, out _);
                    }
                    else
                    {
                        LoggerInstance.Error("T-34-85 object not found, T-34-85 Grafenwoehr patch could not be activated!");
                    }
                }

                if (extraPactTargetsGrafenwoehrPatchEnabled.Value)
                {
                    // spawn some targets
                    SpawnNeutralVehicle(t64a, new Vector3(600f, 12f, 1514f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // front center, 600M range
                    SpawnNeutralVehicle(bmp2, new Vector3(200f, 10f, 1484f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // far to the right, 1000M range, a bit low
                    SpawnNeutralVehicle(t64a, new Vector3(-900f, 12f, 1720f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // to the right, 2100M range
                    SpawnNeutralVehicle(t62, new Vector3(-1400f, 12f, 1620f), new Quaternion(0f, 0.2f, 0f, -0.8f), practiceTarget: true, out _);  // far left field, slightly behind ridge, 2600M range
                    SpawnNeutralVehicle(t64a, new Vector3(-500f, 8f, 1650f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // front center, 1700M range
                    SpawnNeutralVehicle(bmp2, new Vector3(-600f, 10f, 1690f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // 1800M range, front left, slightly off ground
                    SpawnNeutralVehicle(t64a, new Vector3(-1500f, 12f, 1720f), new Quaternion(0f, -0.2f, 0f, -0.8f), practiceTarget: true, out _); // near farthest trees, 2700M range
                    SpawnNeutralVehicle(t55, new Vector3(-1200f, 12f, 1740f), new Quaternion(0f, -0.3f, 0f, -0.8f), practiceTarget: true, out _); // slightly to the right, 2400M range
                    SpawnNeutralVehicle(t64a, new Vector3(36.5558f, 2.7727f, 1567.677f), new Quaternion(0f, -0.3f, 0f, -0.8f), practiceTarget: true, out _); // slightly to the left, 1200M range, kind of hidden with bushes
                    SpawnNeutralVehicle(t62, new Vector3(-1000f, 8f, 1760f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // 2250M, slightly to the right
                    SpawnNeutralVehicle(t64a, new Vector3(-1400f, 12f, 1780f), new Quaternion(0f, 0f, 0f, -0.8f), practiceTarget: true, out _); // 2200M, slightly more to the right
                    SpawnNeutralVehicle(t72, new Vector3(-1500f, 12f, 1780f), new Quaternion(0f, 0f, 0f, -0.8f), practiceTarget: true, out _);  // 2700M, near right of farthest tree cluster
                    SpawnNeutralVehicle(bmp2, new Vector3(-1450f, 12f, 1700f), new Quaternion(0f, 0f, 0f, -0.8f), practiceTarget: true, out _); // 2560M, left of far tree cluster
                    SpawnNeutralVehicle(bmp2, new Vector3(-200f, 4f, 1700f), new Quaternion(0f, 0f, 0f, -0.8f), practiceTarget: true, out _);  // 1400M, right field
                    SpawnNeutralVehicle(bdrm, new Vector3(600f, 12f, 1495f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // front center, ~600M
                    SpawnNeutralVehicle(bdrm, new Vector3(-200f, 4f, 1680f), new Quaternion(0f, 0f, 0f, -0.8f), practiceTarget: true, out _);  // 1400M, right field
                    SpawnNeutralVehicle(bmp1, new Vector3(-200f, 4f, 1620f), new Quaternion(0f, 0f, 0f, -0.8f), practiceTarget: true, out _);  // 1400M, right field
                    SpawnNeutralVehicle(t80, new Vector3(-1500f, 12f, 1690f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // 
                    SpawnNeutralVehicle(t80, new Vector3(-640f, 8f, 1700f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // somewhere in the middle
                    SpawnNeutralVehicle(t80, new Vector3(600f, 12f, 1550f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _);
                }

                if (extraNatoTargetsGrafenwoehrPatchEnabled.Value)
                {
                    SpawnNeutralVehicle(m60a1, new Vector3(600f, 12f, 1614f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _);
                    SpawnNeutralVehicle(m60a1, new Vector3(-660f, 12f, 1600f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _);
                    SpawnNeutralVehicle(m1ip, new Vector3(-820f, 12.5f, 1600f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _);
                    SpawnNeutralVehicle(m1ip, new Vector3(-700f, 12.5f, 1650f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _);
                }

                if (extraHeAmmoVehiclesGrafenWoehrPatchEnabled.Value)
                {
                    SpawnNeutralVehicle(m60a1, new Vector3(1179f, 22f, 1654f), new Quaternion(0f, 0.8f, 0f, -0.8f), false, out Vehicle nerfedM60A1);
                    SpawnNeutralVehicle(t72, new Vector3(1220f, 24f, 1574f), new Quaternion(0f, 0.8f, 0f, -0.8f), false, out Vehicle t72_he);
                    SpawnNeutralVehicle(t55, new Vector3(1220f, 25f, 1524f), new Quaternion(0f, 0.8f, 0f, -0.8f), false, out Vehicle t55_he);
                    SpawnNeutralVehicle(t64a, new Vector3(1220f, 25f, 1474f), new Quaternion(0f, 0.8f, 0f, -0.8f), false, out Vehicle t64a_3bm22);
                    SpawnNeutralVehicle(t72, new Vector3(1220f, 24f, 1424f), new Quaternion(0f, 0.8f, 0f, -0.8f), false, out Vehicle t72_3bm32);

                    SetM774Ammo(nerfedM60A1);
                    SetAmmoCount(t72_he, new int[] { 1, 1, 42 });
                    SetT55APHE(t55_he);
                    SetAmmoCount(t55_he, new int[] { 20, 1, 21});
                    Set3BM22Ammo(t64a_3bm22);
                    SetT72ApfsdsAmmo(t72_3bm32);
                }
            }
            else if (sceneName == "GT01_Reservist_Recon")
            {
                // It's basically the same thing as artillery. Right?
                GameObject t3485 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "T-34-85").FirstOrDefault() as GameObject;
                SpawnVehicle(t3485, new Vector3(460f, 130.7279f, -2576.2f), new Quaternion(0, -0.6166884f, 0, 0.7872075f), false, Faction.Red, out Vehicle t34_1);
                SpawnVehicle(t3485, new Vector3(480f, 130.7279f, -2576.2f), new Quaternion(0, -0.6166884f, 0, 0.7872075f), false, Faction.Red, out Vehicle t34_2);
                SpawnVehicle(t3485, new Vector3(500f, 130.7279f, -2576.2f), new Quaternion(0, -0.6166884f, 0, 0.7872075f), false, Faction.Red, out Vehicle t34_3);
                SpawnVehicle(t3485, new Vector3(520f, 130.7279f, -2576.2f), new Quaternion(0, -0.6166884f, 0, 0.7872075f), false, Faction.Red, out Vehicle t34_4);

                // There should only be one fire mission manager
                FireMissionManager fireMissionManager = Resources.FindObjectsOfTypeAll<FireMissionManager>().FirstOrDefault();
                FieldInfo remainingMissionFieldInfo = typeof(ArtilleryBattery).GetField("_missionsAvailable", BindingFlags.Instance | BindingFlags.NonPublic);
                remainingMissionFieldInfo.SetValue(fireMissionManager.RedArtilleryBatteries[0], 0); // who needs artillery anyway
            }
            else if (sceneName == "GT02_kinetic_key")
            {
                IEnumerable<Vehicle> vehicles = Resources.FindObjectsOfTypeAll<Vehicle>();
                foreach (Vehicle vehicle in vehicles)
                {
                    // budget cuts
                    if (vehicle.name.StartsWith("M60A3")) SetM774Ammo(vehicle);
                }

                CasSupportManager casSupportManager = Resources.FindObjectsOfTypeAll<CasSupportManager>().FirstOrDefault();
                FieldInfo casMissionsAvailable = typeof(CasAirframeUnit).GetField("_missionsAvailable", BindingFlags.Instance | BindingFlags.NonPublic);
                CasAirframeUnit[] airframes = new CasAirframeUnit[6];
                int airframeIdx = 0;
                foreach (CasAirframeUnit casUnit in casSupportManager.BlueCasAirframes)
                {
                    // Moar planes?
                    casMissionsAvailable.SetValue(casUnit, 2);
                    if (airframeIdx < airframes.Length) airframes[airframeIdx] = casUnit;
                    airframeIdx++;
                }

                for (; airframeIdx < airframes.Length;airframeIdx++)
                {
                    CasAirframeUnit newAirframe = new CasAirframeUnit();
                    newAirframe.airframePrefab = airframes[0].airframePrefab;
                    newAirframe.Loadout = airframes[0].Loadout;
                    newAirframe.flyoverType = airframes[0].flyoverType;
                    newAirframe.rechargeTime = 60f;
                    airframes[airframeIdx] = newAirframe;
                }

                casSupportManager.BlueCasAirframes = airframes;
            }
            else if (sceneName == "GT02_replen_reaper")
            {
                // Add another artillery battery so the player can stack them for an uber barrage (or call one and keep the other in reserve)
                FireMissionManager fireMissionManager = Resources.FindObjectsOfTypeAll<FireMissionManager>().FirstOrDefault();
                FieldInfo remainingMissionFieldInfo = typeof(ArtilleryBattery).GetField("_missionsAvailable", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo fireMissionShotsFieldInfo = typeof(ArtilleryBattery).GetField("_shots", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo shotDelayIntervalFieldInfo = typeof(ArtilleryBattery).GetField("_interShotDelaySeconds", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo batteryMunitionsFieldInfo = typeof(ArtilleryBattery).GetField("_munitions", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo callDelayFieldInfo = typeof(ArtilleryBattery).GetField("_onCallImpactDelay", BindingFlags.Instance | BindingFlags.NonPublic);
                ArtilleryBattery[] updatedBlueBatteries = new ArtilleryBattery[3];

                // There should be two batteries in the mission (HE, smoke)
                int batteryIdx = 0;
                foreach (ArtilleryBattery artilleryBattery in fireMissionManager.BlueArtilleryBatteries)
                {
                    LoggerInstance.Msg("Artillery battery: " + artilleryBattery.FriendlyName + " has " + remainingMissionFieldInfo.GetValue(artilleryBattery) + " missions, shots: " + fireMissionShotsFieldInfo.GetValue(artilleryBattery));
                    LoggerInstance.Msg("  Inter-shot delay: " + shotDelayIntervalFieldInfo.GetValue(artilleryBattery));
                    LoggerInstance.Msg("  Shot spawn height: " + artilleryBattery.SpawnHeight);
                    LoggerInstance.Msg("  Shot spawn angle: " + artilleryBattery.SpawnAngle);
                    LoggerInstance.Msg("  On-call delay: " + callDelayFieldInfo.GetValue(artilleryBattery));
                    if (batteryIdx < updatedBlueBatteries.Length) updatedBlueBatteries[batteryIdx] = artilleryBattery;
                    batteryIdx++;
                }

                for (;batteryIdx < updatedBlueBatteries.Length; batteryIdx++)
                {
                    BatteryMunitionsChoice[] munitions = (BatteryMunitionsChoice[])batteryMunitionsFieldInfo.GetValue(updatedBlueBatteries[0]);
                    ArtilleryBattery artilleryBattery = new ArtilleryBattery(shots: 36, interShotDelay: 0.8f, spawnHeight: 300f, spawnAngle: 45f, cooldown: 25f, munitionsChoices: munitions, label: "M109");
                    callDelayFieldInfo.SetValue(artilleryBattery, 30f);
                    remainingMissionFieldInfo.SetValue(artilleryBattery, 8);
                    updatedBlueBatteries[batteryIdx] = artilleryBattery;
                }

                remainingMissionFieldInfo.SetValue(updatedBlueBatteries[0], 6);
                fireMissionManager.BlueArtilleryBatteries = updatedBlueBatteries;

                GameObject t80 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "T80B").FirstOrDefault() as GameObject;
                SpawnVehicle(t80, new Vector3(-292.7939f, 82.7014f, -900.1545f), new Quaternion(-9.112011E-05f, 0.6990631f, -0.0002299712f, 0.71506f), false, Faction.Red, out Vehicle t80_1);
                SpawnVehicle(t80, new Vector3(-332.6814f, 83.4028f, -900.2035f), new Quaternion(-9.112011E-05f, 0.6990631f, -0.0002299712f, 0.71506f), false, Faction.Red, out Vehicle t80_2);
                SpawnVehicle(t80, new Vector3(-323.2041f, 83.9161f, -878.7783f), new Quaternion(-9.112011E-05f, 0.6990631f, -0.0002299712f, 0.71506f), false, Faction.Red, out Vehicle t80_3);

                List<Unit> unimportantUnits = new List<Unit>();
                unimportantUnits.Add(t80.GetComponent<Vehicle>());
                unimportantUnits.Add(t80_1);
                unimportantUnits.Add(t80_2);
                unimportantUnits.Add(t80_3);
                AppendUnimportantUnits(unimportantUnits);
            }
            else if (sceneName == "GT02_claustrophobia")
            {
                GameObject m1 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "M1").FirstOrDefault() as GameObject;
                SpawnVehicle(m1, new Vector3(-2244.504f, 97.5607f, -912.041f), new Quaternion(-9.112011E-05f, 0.6990631f, -0.0002299712f, 0.71506f), false, Faction.Blue, out Vehicle m1_1);
                SetAmmoCount(m1_1, new int[] { 50, 2 });
                SetM774Ammo(m1_1);

                GameObject t3485 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "T-34-85").FirstOrDefault() as GameObject;
                SpawnVehicle(t3485, new Vector3(-430.446f, 93.2921f, -669.5197f), new Quaternion(0, -0.6166884f, 0, 0.7872075f), false, Faction.Red, out Vehicle t34_1);
                SpawnVehicle(t3485, new Vector3(-476.9093f, 100.7423f, -619.7258f), new Quaternion(0, -0.6166884f, 0, 0.7872075f), false, Faction.Red, out Vehicle t34_2);
                SpawnVehicle(t3485, new Vector3(-543.8735f, 103.2393f, -654.0811f), new Quaternion(0, -0.6166884f, 0, 0.7872075f), false, Faction.Red, out Vehicle t34_3);
                SpawnVehicle(t3485, new Vector3(-586.396f, 104.4196f, -678.4468f), new Quaternion(0, -0.6166884f, 0, 0.7872075f), false, Faction.Red, out Vehicle t34_4);
                Claustrophobia_t34_list = new List<Vehicle>();
                Claustrophobia_t34_list.Add(t34_1);
                Claustrophobia_t34_list.Add(t34_2);
                Claustrophobia_t34_list.Add(t34_3);
                Claustrophobia_t34_list.Add(t34_4);

                Claustrophobia_t34_positions = new List<Vector3>();
                Claustrophobia_t34_positions.Add(new Vector3(-430.446f, 93.2921f, -669.5197f));
                Claustrophobia_t34_positions.Add(new Vector3(-476.9093f, 100.7423f, -619.7258f));
                Claustrophobia_t34_positions.Add(new Vector3(-543.8735f, 103.2393f, -654.0811f));
                Claustrophobia_t34_positions.Add(new Vector3(-586.396f, 104.4196f, -678.4468f));

                Claustrophobia_t55_list = Resources.FindObjectsOfTypeAll<Vehicle>().Where(o => o.name == "T55A");

                // allow mission completion without killing t-34s
                List<Unit> unimportantUnits = new List<Unit>();
                unimportantUnits.Add(t3485.GetComponent<Vehicle>());
                unimportantUnits.AddRange(Claustrophobia_t34_list);
                AppendUnimportantUnits(unimportantUnits);
            }

            if (writeDebugTxt.Value)
            {
                IEnumerable<Object> allObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
                StringBuilder sb = new StringBuilder();
                foreach (Object obj in allObjects)
                {
                    GameObject gObj = obj as GameObject;
                    Vehicle possibleVehicle = gObj.GetComponent<GHPC.Vehicle.Vehicle>();
                    VehicleInfo possibleVehicleInfo = gObj.GetComponent<VehicleInfo>();
                    AmmoClipCodexScriptable possibleAmmo = gObj.GetComponent<AmmoClipCodexScriptable>();
                    string msg = "GameObject has name " + gObj.name;
                    if (possibleVehicle != null) msg += " => is vehicle";
                    if (possibleVehicleInfo != null) msg += " => is VehicleInfo";
                    if (possibleAmmo != null) msg += " => is AmmoClipCodexScriptable";
                    sb.AppendLine(msg);
                }

                // change this
                System.IO.File.WriteAllText("C:\\git\\GunnerTestPC\\objs.txt", sb.ToString());

                VehicleInfo[] vehicleInfoArray = Object.FindObjectsOfType<VehicleInfo>();
                LoggerInstance.Msg("VehicleInfoCount: " + vehicleInfoArray.Length);
                foreach (VehicleInfo vehicleInfo in vehicleInfoArray)
                {
                    LoggerInstance.Msg("VehicleInfo: " + vehicleInfo.name + ", gun " + vehicleInfo.Gun.name);
                }
            }
        }

        private bool startingCheckMessage = false;
        public override void OnLateUpdate()
        {
            // Platoons don't seem to be initialized on vehicle spawn
            if (Claustrophobia_t34_list != null && Claustrophobia_t55_list != null)
            {
                if (!startingCheckMessage)
                {
                    LoggerInstance.Msg($"Found {Claustrophobia_t55_list.Count()} T55As, will update platoons once they're initialized");
                }
                
                bool platoonAssignmentWorked = false;
                foreach (Vehicle t55 in Claustrophobia_t55_list)
                {
                    // attach to the platoon
                    if (t55.Platoon != null)
                    {
                        FieldInfo mobileUnitsFieldInfo = typeof(GHPC.AI.Platoons.PlatoonData).GetField("MobileUnits", BindingFlags.Instance | BindingFlags.NonPublic);
                        FieldInfo formationTargetsFieldInfo = typeof(GHPC.AI.Platoons.PlatoonData).GetField("FormationTargets", BindingFlags.Instance | BindingFlags.NonPublic);
                        LoggerInstance.Msg("Attaching T34s to " + t55.Platoon.name);
                        TransformWaypoint transformWaypointTemplate = GameObject.FindObjectsOfType<TransformWaypoint>().FirstOrDefault();
                        int t34_idx = 0;
                        foreach (Vehicle t34 in Claustrophobia_t34_list)
                        {
                            t34.Platoon = t55.Platoon;
                            t34.Platoon.Units.Add(t34);
                            List<Unit> mobileUnits = (List<Unit>)mobileUnitsFieldInfo.GetValue(t34.Platoon);
                            mobileUnits.Add(t34);
                            mobileUnitsFieldInfo.SetValue(t34.Platoon, mobileUnits);

                            var formationTargets = formationTargetsFieldInfo.GetValue(t34.Platoon);
                            Type formationMarkerInfoType = typeof(GHPC.AI.Platoons.PlatoonData).Assembly.GetType("GHPC.AI.Platoons.PlatoonData+FormationMarkerInfo");
                            FieldInfo formationMarkerIndexFieldInfo = formationMarkerInfoType.GetField("Index", BindingFlags.Instance | BindingFlags.Public);
                            FieldInfo formationMarkerUnitField = formationMarkerInfoType.GetField("AssignedUnit", BindingFlags.Instance | BindingFlags.Public);
                            FieldInfo formationMarkerField = formationMarkerInfoType.GetField("Marker", BindingFlags.Instance | BindingFlags.Public);

                            object formationMarkerInfo = Activator.CreateInstance(formationMarkerInfoType);
                            formationMarkerUnitField.SetValue(formationMarkerInfo, t34);
                            formationMarkerIndexFieldInfo.SetValue(formationMarkerInfo, t34_idx);


                            TransformWaypoint marker = GameObject.Instantiate(transformWaypointTemplate, Claustrophobia_t34_positions[t34_idx], new Quaternion(0, -0.6166884f, 0, 0.7872075f));
                            formationMarkerField.SetValue(formationMarkerInfo, marker);
                            marker.FollowMode = WaypointHolder.FollowModes.Forward;
                            marker.MaxSpeed = 0f;
                            marker.CompletionRadius = 1.4f;

                            MethodInfo listAddMethodInfo = formationTargets.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                            listAddMethodInfo.Invoke(formationTargets, new object[] { formationMarkerInfo });
                            //formationTargets.Add(formationMarkerInfo);
                            t34_idx++;
                        }

                        t55.Platoon.SetFormation(GHPC.AI.Platoons.FormationType.Line);
                        platoonAssignmentWorked = true;
                        break;
                    }
                    else if (!startingCheckMessage) LoggerInstance.Msg("T55 has no platoon?");
                }

                startingCheckMessage = true;

                if (platoonAssignmentWorked)
                {
                    Claustrophobia_t55_list = null;
                    Claustrophobia_t34_list = null;
                }
            }
        }

        void SetAmmoCount(Vehicle vehicle, int[] customAmmoCount)
        {
            if (vehicle.LoadoutManager == null)
            {
                LoggerInstance.Msg($"{vehicle.name} has no LoadoutManager\n");
                return;
            }

            int[] totalAmmoCounts = vehicle.LoadoutManager.TotalAmmoCounts;
            for (int i = 0; i < totalAmmoCounts.Length && i < customAmmoCount.Length; i++) totalAmmoCounts[i] = customAmmoCount[i];
            SetLoadout(vehicle);
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
            Vehicle vehicleComp = vehicle.GetComponent<Vehicle>();
            instantiatedVehicle = null;
            if (vehicle != null)
            {
                vehicleComp.Allegiance = faction;
                GameObject instantiatedObj = GameObject.Instantiate(vehicle, position, rotation);
                instantiatedVehicle = instantiatedObj.GetComponent<Vehicle>();

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

                // LoggerInstance.Msg($"{vehicle.name} successfully spawned at {vehicle.transform.position}");
            }
            else
            {
                LoggerInstance.Error($"Could not find Vehicle component in {vehicle.name} GameObject!");
            }
        }

        void SetLoadout(Vehicle vehicle)
        {
            for (int i = 0; i < vehicle.LoadoutManager.RackLoadouts.Length; i++) EmptyRack(vehicle.LoadoutManager.RackLoadouts[i].Rack);
            vehicle.LoadoutManager.SpawnCurrentLoadout();

            // https://github.com/thebeninator/US-Reduced-Lethality/blob/master/ReducedLethality.cs with modifications
            WeaponSystem mainGun = vehicle.WeaponsManager.Weapons[0].Weapon;
            PropertyInfo roundInBreech = typeof(AmmoFeed).GetProperty("AmmoTypeInBreech");
            roundInBreech.SetValue(mainGun.Feed, null);

            MethodInfo refreshBreech = typeof(AmmoFeed).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            refreshBreech.Invoke(mainGun.Feed, new object[] { });

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
        void AppendUnimportantUnits(List<Unit> units)
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
    }
}
