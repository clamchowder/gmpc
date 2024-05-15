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

[assembly: MelonInfo(typeof(GHPCMissionsMod.GHPCMissionsMod), "GHPC Missions Mod", "0.0.1", "Clamchowder")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace GHPCMissionsMod
{
    public partial class GHPCMissionsMod : MelonMod
    {
        public static MelonPreferences_Category config;
        public static MelonPreferences_Entry<bool> t3485GrafenwoehrPatchEnabled;

        public static MelonPreferences_Entry<bool> practiceBmp2Targets;
        public static MelonPreferences_Entry<bool> practiceT64Targets;
        public static MelonPreferences_Entry<bool> practiceBdrmTargets;
        public static MelonPreferences_Entry<bool> practiceT80Targets;
        public static MelonPreferences_Entry<bool> practiceM60Targets;
        public static MelonPreferences_Entry<bool> practiceM1Targets;
        public static MelonPreferences_Entry<bool> practiceT62Targets;
        public static MelonPreferences_Entry<bool> practiceT55Targets;
        public static MelonPreferences_Entry<bool> practiceT72Targets;
        public static MelonPreferences_Entry<bool> practiceBmp1Targets;

        public static MelonPreferences_Entry<bool> reduceExtraTargetFlammability;
        public static MelonPreferences_Entry<bool> extraHeAmmoVehiclesGrafenWoehrPatchEnabled;
        public static MelonPreferences_Entry<bool> writeDebugTxt;

        public List<Vehicle> Claustrophobia_t34_list;
        public List<Vector3> Claustrophobia_t34_positions;
        public IEnumerable<Vehicle> Claustrophobia_t55_list;

        public bool SetTimeHack = false;

        public SceneUnitsManager currentSceneUnitsManager = null;

        public override void OnInitializeMelon()
        {
            config = MelonPreferences.CreateCategory("GHPCMissionsModConfig");
            t3485GrafenwoehrPatchEnabled = config.CreateEntry<bool>("t3485GrafenwoehrPatchEnabled", true);
            practiceBmp2Targets = config.CreateEntry<bool>("practiceBmp2Targets", true);
            practiceT64Targets = config.CreateEntry<bool>("practiceT64Targets", true);
            practiceBdrmTargets = config.CreateEntry<bool>("practiceBdrmTargets", true);
            practiceT80Targets = config.CreateEntry<bool>("practiceT80Targets", true);
            practiceM1Targets = config.CreateEntry<bool>("practiceM1Targets", true);
            practiceT62Targets = config.CreateEntry<bool>("practiceT62Targets", true);
            practiceT55Targets = config.CreateEntry<bool>("practiceT55Targets", true);
            practiceT72Targets = config.CreateEntry<bool>("practiceT72Targets", true);
            practiceBmp1Targets = config.CreateEntry<bool>("practiceBmp1Targets", true);
            practiceM60Targets = config.CreateEntry<bool>("practiceM60Targets", true);

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

                            // this is a hack for missions that don't have day time options.
                            // make the options show up (they don't work because there's no RandomEnvironment GameObject, maybe), then sort it out on mission load
                            missionMetaData.TimeOptions = new RandomEnvironment.EnvSettingOption[2];
                            missionMetaData.TimeOptions[0] = new RandomEnvironment.EnvSettingOption();
                            missionMetaData.TimeOptions[0].Time = 260f;
                            missionMetaData.TimeOptions[1] = new RandomEnvironment.EnvSettingOption();
                            missionMetaData.TimeOptions[1].Time = 464.3f;
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

                            missionMetaData.TimeOptions = new RandomEnvironment.EnvSettingOption[2];
                            missionMetaData.TimeOptions[0] = new RandomEnvironment.EnvSettingOption();
                            missionMetaData.TimeOptions[0].Time = 260f;
                            missionMetaData.TimeOptions[1] = new RandomEnvironment.EnvSettingOption();
                            missionMetaData.TimeOptions[1].Time = 464.3f;
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

                            missionMetaData.TimeOptions = new RandomEnvironment.EnvSettingOption[2];
                            missionMetaData.TimeOptions[0] = new RandomEnvironment.EnvSettingOption();
                            missionMetaData.TimeOptions[0].Time = 260f;
                            missionMetaData.TimeOptions[1] = new RandomEnvironment.EnvSettingOption();
                            missionMetaData.TimeOptions[1].Time = 464.3f;
                        }
                        else if (missionMetaData.MissionName.Equals("Bolder Limit"))
                        {
                            missionMetaData.MissionName = "Bolder Limit (modded)";
                            FactionMissionInfo missionInfo = missionMetaData.FactionInfo[0];
                            string newDesc = "Situation - You are Alpha Company and you've been bad. Recon indicates a nearby Soviet commander is massing forces against you. It seems personal.\n";
                            newDesc += "\nEnemy - Yes\n";
                            newDesc += "\nFriendly - 4x M1 Abrams, 2X M113. Reinforcement of 2x M1 Abrams. Support assets are 8x fire missions, 5x Smoke missions, and 4x air support.\n";
                            newDesc += "\nMission - Hold Objective Jolly\n";
                            newDesc += "\nCoordinating instructions - Two tanks and APCs are dug in around OBJ Jolly. You are out of position to the north of OBJ Jolly, you must move to get into a firing position.\n";
                            newDesc += "\nOther - Units will become available over the course of the mission, they are not all immediately available at the start.";
                            newDesc += "\nEnd Conditions:\n-Blue Victory- Enemy attack is repulsed and you hold OBJ Jolly.\n-Blue Defeat-\nEnemy controls OBJ Jolly or 90% of your force is destroyed";
                            missionDescription.SetValue(missionInfo, newDesc);

                            missionMetaData.TimeOptions = new RandomEnvironment.EnvSettingOption[2];
                            missionMetaData.TimeOptions[0] = new RandomEnvironment.EnvSettingOption();
                            missionMetaData.TimeOptions[0].Time = 260f;
                            missionMetaData.TimeOptions[1] = new RandomEnvironment.EnvSettingOption();
                            missionMetaData.TimeOptions[1].Time = 464.3f;
                        }

                        /*LoggerInstance.Msg("  Mission: " + missionMetaData.MissionName);
                        LoggerInstance.Msg("    Default time: " + missionMetaData.DefaultTime);
                        foreach (RandomEnvironment.EnvSettingOption opt in  missionMetaData.TimeOptions)
                        {
                            LoggerInstance.Msg("    Time option: " + opt.Time + " has weight " + opt.RandomWeight);
                        }

                        foreach (FactionMissionInfo missionInfo in missionMetaData.FactionInfo)
                        {
                            LoggerInstance.Msg($"  Mission description ({missionInfo.Allegiance}): " + missionInfo.Description);
                        }

                        Eflatun.SceneReference.SceneReference sceneReference = missionMetaData.MissionSceneReference;
                        LoggerInstance.Msg("    Path: " + sceneReference.Path);*/
                    }
                }
            }

            Claustrophobia_t55_list = null;
            Claustrophobia_t34_list = null;
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            LoggerInstance.Msg($"Initialized scene {sceneName}, trying to patch game...");
            this.IsModifiedBolderLimit = false;
            currentSceneUnitsManager = Object.FindObjectOfType<SceneUnitsManager>();
            UnitSpawner unitSpawner = Object.FindAnyObjectByType<UnitSpawner>();

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
                GameObject t64b = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "T64B").FirstOrDefault() as GameObject;
                GameObject t62 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "T62").FirstOrDefault() as GameObject;
                GameObject t80 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "T80B").FirstOrDefault() as GameObject;
                GameObject t54 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "T54A").FirstOrDefault() as GameObject;

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

                // spawn some targets
                if (practiceBmp2Targets.Value)
                {
                    SpawnNeutralVehicle(bmp2, new Vector3(-1450f, 12f, 1700f), new Quaternion(0f, 0f, 0f, -0.8f), practiceTarget: true, out _); // 2560M, left of far tree cluster
                    SpawnNeutralVehicle(bmp2, new Vector3(-200f, 4f, 1700f), new Quaternion(0f, 0f, 0f, -0.8f), practiceTarget: true, out _);  // 1400M, right field
                    SpawnNeutralVehicle(bmp2, new Vector3(200f, 10f, 1484f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // far to the right, 1000M range, a bit low
                    SpawnNeutralVehicle(bmp2, new Vector3(-600f, 10f, 1690f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // 1800M range, front left, slightly off ground
                }
                
                if (practiceBmp1Targets.Value)
                {
                    SpawnNeutralVehicle(bmp1, new Vector3(-200f, 4f, 1620f), new Quaternion(0f, 0f, 0f, -0.8f), practiceTarget: true, out _);  // 1400M, right field
                }
                
                if (practiceBdrmTargets.Value)
                {
                    SpawnNeutralVehicle(bdrm, new Vector3(600f, 12f, 1495f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // front center, ~600M
                    SpawnNeutralVehicle(bdrm, new Vector3(-200f, 4f, 1680f), new Quaternion(0f, 0f, 0f, -0.8f), practiceTarget: true, out _);  // 1400M, right field
                }
                
                if (practiceT64Targets.Value)
                {
                    SpawnNeutralVehicle(t64a, new Vector3(600f, 12f, 1514f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // front center, 600M range
                    SpawnNeutralVehicle(t64a, new Vector3(-900f, 12f, 1720f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // to the right, 2100M range
                    SpawnNeutralVehicle(t64a, new Vector3(-500f, 8f, 1650f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // front center, 1700M range
                    SpawnNeutralVehicle(t64a, new Vector3(-1500f, 12f, 1720f), new Quaternion(0f, -0.2f, 0f, -0.8f), practiceTarget: true, out _); // near farthest trees, 2700M range
                    SpawnNeutralVehicle(t64a, new Vector3(36.5558f, 2.7727f, 1567.677f), new Quaternion(0f, -0.3f, 0f, -0.8f), practiceTarget: true, out _); // slightly to the left, 1200M range, kind of hidden with bushes
                    SpawnNeutralVehicle(t64a, new Vector3(-1400f, 12f, 1780f), new Quaternion(0f, 0f, 0f, -0.8f), practiceTarget: true, out _); // 2200M, slightly more to the right
                }

                if (practiceT80Targets.Value)
                {
                    SpawnNeutralVehicle(t80, new Vector3(-1500f, 12f, 1690f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _);
                    SpawnNeutralVehicle(t80, new Vector3(-640f, 8f, 1700f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // somewhere in the middle
                    SpawnNeutralVehicle(t80, new Vector3(600f, 12f, 1550f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _);
                }

                if (practiceT62Targets.Value)
                {
                    SpawnNeutralVehicle(t62, new Vector3(-1400f, 12f, 1620f), new Quaternion(0f, 0.2f, 0f, -0.8f), practiceTarget: true, out _);  // far left field, slightly behind ridge, 2600M range
                    SpawnNeutralVehicle(t62, new Vector3(-1000f, 8f, 1760f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _); // 2250M, slightly to the right
                }

                if(practiceT55Targets.Value)
                {
                    SpawnNeutralVehicle(t55, new Vector3(-1200f, 12f, 1740f), new Quaternion(0f, -0.3f, 0f, -0.8f), practiceTarget: true, out _); // slightly to the right, 2400M range
                }
                    
                if (practiceT72Targets.Value)
                {
                    SpawnNeutralVehicle(t72, new Vector3(-1500f, 12f, 1780f), new Quaternion(0f, 0f, 0f, -0.8f), practiceTarget: true, out _);  // 2700M, near right of farthest tree cluster
                }

                if (practiceM1Targets.Value)
                {
                    SpawnNeutralVehicle(m1ip, new Vector3(-820f, 12.5f, 1600f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _);
                    SpawnNeutralVehicle(m1ip, new Vector3(-700f, 12.5f, 1650f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _);
                }

                if (practiceM60Targets.Value)
                {
                    SpawnNeutralVehicle(m60a1, new Vector3(600f, 12f, 1614f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _);
                    SpawnNeutralVehicle(m60a1, new Vector3(-660f, 12f, 1600f), new Quaternion(0f, -0.8f, 0f, -0.8f), practiceTarget: true, out _);
                }

                if (extraHeAmmoVehiclesGrafenWoehrPatchEnabled.Value)
                {
                    SpawnNeutralVehicle(m60a1, new Vector3(1179f, 22f, 1654f), new Quaternion(0f, 0.8f, 0f, -0.8f), false, out Vehicle nerfedM60A1);
                    SpawnNeutralVehicle(t72, new Vector3(1220f, 24f, 1574f), new Quaternion(0f, 0.8f, 0f, -0.8f), false, out Vehicle t72_he);
                    SpawnNeutralVehicle(t55, new Vector3(1220f, 25f, 1524f), new Quaternion(0f, 0.8f, 0f, -0.8f), false, out Vehicle t55_he);
                    SpawnNeutralVehicle(t64b, new Vector3(1220f, 24f, 1424f), new Quaternion(0f, 0.8f, 0f, -0.8f), false, out Vehicle t64_missile);
                    SpawnNeutralVehicle(t54, new Vector3(1220f, 25f, 1474f), new Quaternion(0f, 0.8f, 0f, -0.8f), false, out Vehicle t54_1);

                    SetM774Ammo(nerfedM60A1);
                    SetAmmoCount(t72_he, new int[] { 1, 1, 42 });
                    SetT55APHE(t55_he);
                    SetAmmoCount(t55_he, new int[] { 20, 1, 21});
                    //SetT72ApfsdsAmmo(t64_missile);
                    SetAmmoCount(t64_missile, new int[] { 2, 2, 2, 31 });
                }
            }
            else if (sceneName == "GT01_Reservist_Recon")
            {
                // It's basically the same thing as artillery. Right?
                //GameObject t3485 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "T-34-85").FirstOrDefault() as GameObject;
                GameObject t3485 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "T-34-85").FirstOrDefault() as GameObject;
                SpawnVehicle(t3485, new Vector3(460f, 130.7279f, -2576.2f), new Quaternion(0, -0.6166884f, 0, 0.7872075f), false, Faction.Red, out Vehicle t34_1);
                SpawnVehicle(t3485, new Vector3(480f, 130.7279f, -2576.2f), new Quaternion(0, -0.6166884f, 0, 0.7872075f), false, Faction.Red, out Vehicle t34_2);
                SpawnVehicle(t3485, new Vector3(500f, 130.7279f, -2576.2f), new Quaternion(0, -0.6166884f, 0, 0.7872075f), false, Faction.Red, out Vehicle t34_3);
                SpawnVehicle(t3485, new Vector3(520f, 130.7279f, -2576.2f), new Quaternion(0, -0.6166884f, 0, 0.7872075f), false, Faction.Red, out Vehicle t34_4);

                IEnumerable<AmmoClipCodexScriptable> ammoTypes = Resources.FindObjectsOfTypeAll<AmmoClipCodexScriptable>();
                foreach(AmmoClipCodexScriptable ammoClipCodexScriptable in ammoTypes)
                {
                    LoggerInstance.Msg("AmmoClipCodexScriptable: " +  ammoClipCodexScriptable.name);
                }

                // There should only be one fire mission manager
                FireMissionManager fireMissionManager = Resources.FindObjectsOfTypeAll<FireMissionManager>().FirstOrDefault();
                FieldInfo remainingMissionFieldInfo = typeof(ArtilleryBattery).GetField("_missionsAvailable", BindingFlags.Instance | BindingFlags.NonPublic);
                remainingMissionFieldInfo.SetValue(fireMissionManager.RedArtilleryBatteries[0], 0); // who needs artillery anyway

                /*DumpVehicleLoadout(t34_1);
                int[] ammoCount = new int[] { 38, 1 };
                SetAmmoCount(t34_1, ammoCount);
                SetAmmoCount(t34_2, ammoCount);
                SetAmmoCount(t34_3, ammoCount);
                SetAmmoCount(t34_4, ammoCount);*/
            }
            else if (sceneName == "GT02_kinetic_key")
            {
                SetTimeHack = true;

                IEnumerable<Vehicle> vehicles = Resources.FindObjectsOfTypeAll<Vehicle>();
                foreach (Vehicle vehicle in vehicles)
                {
                    // budget cuts
                    if (vehicle.name.StartsWith("M60A3")) SetM774Ammo(vehicle);
                }

                CasSupportManager casSupportManager = Resources.FindObjectsOfTypeAll<CasSupportManager>().FirstOrDefault();
                FieldInfo casMissionsAvailable = typeof(CasAirframeUnit).GetField("_missionsAvailable", BindingFlags.Instance | BindingFlags.NonPublic);
                CasAirframeUnit[] airframes = new CasAirframeUnit[6];

                // keep the CAS missions already present
                int airframeIdx = 0;
                foreach (CasAirframeUnit casUnit in casSupportManager.BlueCasAirframes)
                {
                    if (airframeIdx < airframes.Length) airframes[airframeIdx] = casUnit;
                    airframeIdx++;
                }

                for (; airframeIdx < airframes.Length;airframeIdx++)
                {
                    // 0 is an A-10 and it misses its bombs every time
                    // 1 is a F-4
                    CasAirframeUnit newAirframe = new CasAirframeUnit();
                    newAirframe.airframePrefab = airframes[1].airframePrefab;
                    newAirframe.Loadout = airframes[1].Loadout;
                    newAirframe.flyoverType = airframes[1].flyoverType;
                    // newAirframe.rechargeTime = 60f;
                    casMissionsAvailable.SetValue(newAirframe, 3);
                    airframes[airframeIdx] = newAirframe;
                }

                casSupportManager.BlueCasAirframes = airframes;
            }
            else if (sceneName == "GT02_replen_reaper")
            {
                SetTimeHack = true;

                // Add another artillery battery so the player can stack them for an uber barrage (or call one and keep the other in reserve)
                FireMissionManager fireMissionManager = Resources.FindObjectsOfTypeAll<FireMissionManager>().FirstOrDefault();
                FieldInfo remainingMissionFieldInfo = typeof(ArtilleryBattery).GetField("_missionsAvailable", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo fireMissionShotsFieldInfo = typeof(ArtilleryBattery).GetField("_shots", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo shotDelayIntervalFieldInfo = typeof(ArtilleryBattery).GetField("_interShotDelaySeconds", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo batteryMunitionsFieldInfo = typeof(ArtilleryBattery).GetField("_munitions", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo callDelayFieldInfo = typeof(ArtilleryBattery).GetField("_onCallImpactDelay", BindingFlags.Instance | BindingFlags.NonPublic);
                ArtilleryBattery[] updatedBlueBatteries = new ArtilleryBattery[4];

                // There should be two batteries in the mission (HE, smoke)
                int batteryIdx = 0;
                foreach (ArtilleryBattery artilleryBattery in fireMissionManager.BlueArtilleryBatteries)
                {
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

                List<IUnit> unimportantUnits = new List<IUnit>();
                if (unitSpawner != null)
                {
                    List<Vector3> pos = new List<Vector3>();
                    pos.Add(new Vector3(-368.3634f, 83.613f, -987.1602f));
                    pos.Add(new Vector3(-353.2653f, 83.1939f, -955.3156f));
                    pos.Add(new Vector3(-336.1085f, 83.0931f, -920.1476f));
                    pos.Add(new Vector3(-296.9652f, 80.9326f, -958.4582f));

                    List<Quaternion> rot = new List<Quaternion>();
                    rot.Add(new Quaternion(0.0065f, 0.3052f, -.0022f, .9523f));
                    rot.Add(new Quaternion(0.0041f, .2215f, -0.0048f, 0.9751f));
                    rot.Add(new Quaternion(0.0001f, 0.226f, -.0018f, 0.9741f));
                    rot.Add(new Quaternion(0.0131f, .9972f, -.0096f, -.0726f));

                    for (int i = 0;i < pos.Count; i++)
                    {
                        UnitMetaData md = new UnitMetaData();
                        md.Name = "ExtraUnit" + i;
                        md.Allegiance = Faction.Red;
                        md.UnitType = UnitType.GroundVehicle;
                        md.Position = pos[i];
                        md.Rotation = rot[i];
                        IUnit spawnedUnit = unitSpawner.SpawnUnit("T64B", md);
                        SetAmmoCount((Vehicle)spawnedUnit, new int[] { 2, 2, 2, 31 });
                        unimportantUnits.Add(spawnedUnit);
                    }
                }

                unimportantUnits.Add(t80.GetComponent<Vehicle>());
                unimportantUnits.Add(t80_1);
                unimportantUnits.Add(t80_2);
                unimportantUnits.Add(t80_3);
                AppendUnimportantUnits(unimportantUnits);
            }
            else if (sceneName == "GT02_claustrophobia")
            {
                SetTimeHack = true;

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
                List<IUnit> unimportantUnits = new List<IUnit>();
                unimportantUnits.Add(t3485.GetComponent<Vehicle>());
                unimportantUnits.AddRange(Claustrophobia_t34_list);
                AppendUnimportantUnits(unimportantUnits);
            }
            else if (sceneName == "GT01_Retro_Rumble_P1")
            {
                /*Vehicle m47 = Resources.FindObjectsOfTypeAll<Vehicle>().Where(o => o.name == "M47").First();
                string test = SerializeVehicle(m47, 4);
                System.IO.File.WriteAllText("C:\\git\\GunnerTestPC\\serializationtest.txt", test);*/
            }
            else if (sceneName == "GT02_Bolder_Limit")
            {
                InitializeBolderLimit(unitSpawner);
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


            if (unitSpawner != null)
            {
                LoggerInstance.Msg("Scene has a unit spawner");
            }
            else
            {
                LoggerInstance.Msg("Scene does not have a unit spawner");
            }
        }

        public override void OnGUI()
        {
            if (IsModifiedBolderLimit)
            {
                BolderLimitMessage();
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

            if (SetTimeHack)
            {
                // Directly set the mission time instead of trying to figure out how to instantiate a Unity object (RandomizeEnvironment)
                // Mission initialization overrides the time at some point before the "loading..." screen goes away
                // So wait until the loading screen goes away, then set the mission time
                SceneController sceneController = Object.FindAnyObjectByType<SceneController>();
                if (sceneController != null)
                {
                    SetTimeHack = sceneController.LoaderCanvasElement.activeSelf;
                }

                if (!SetTimeHack)
                {
                    SetMissionTime(260f, 464.3f);
                }
            }

            if (IsModifiedBolderLimit)
            {
                BolderLimitUpdate();
            }
        }
    }
}
