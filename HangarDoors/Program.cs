using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // Graf's Cool Hangar Door Script
        // ============================
        // Version: 1.1
        // Date: 2022-09-17

        // =======================================================================================
        //                                                                            --- Config ---
        // =======================================================================================

        // --- Target Blocks ---
        // =======================================================================================

        // The group containing all your door blocks to open/close
        string doorGroupName = "Door Group Name";

        // The group containing all the lights you want to turn on while the doors are operating
        string lightGroupname = "Light Group Name";

        // The name of the soundblock set up to play a cool alarm sound
        string soundBlockName = "Sound block Name";


        // --- Behaivour ---
        // =======================================================================================

        // Should the lights flash? You might want to turn this off for rotating lights!
        bool flashLights = false;

        // Is the sound block set up to loop or play a long sound? If false, we'll play the sound multiple times
        bool loopingAlarmSound = false; //If the sound to play is a long or looping sound, set true


        // --- Timing ---
        // =======================================================================================
        
        // Total time until the doors can be triggered again
        float totalAlarmTime = 13f;

        //Delay after alarm begins that door starts moving
        float delayBeforeDoorOpening = 3f;

        //Speed to flash the lights at, if flashLights is false
        float lightToggleInterval = 0.5f;

        //Time between alarm sounds, if loopingAlarmSound is false;
        float alarmSoundInterval = 2f; 


        // =======================================================================================
        //                                                                      --- DANGER ZONE ---
        //                                                        Everything past this point is script worky-bits!
        // =======================================================================================

        List<IMyDoor> doors = new List<IMyDoor>();
        List<IMyLightingBlock> lights = new List<IMyLightingBlock>();
        IMySoundBlock sound;
        bool isRunning, doorsOpening;
        float timer;
        float deltaTime;
        float alarmSoundTimer;
        float lightToggleTimer;

        public Program()
        {
            // Configure this program to run the Main method every 10 update ticks
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            //Set up the doors group
            IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(doorGroupName);
            if (group != null)
            {
                doors.Clear();
                group.GetBlocksOfType(doors);
            }

            //Set up the lights group
            IMyBlockGroup group2 = GridTerminalSystem.GetBlockGroupWithName(lightGroupname);
            if (group2 != null)
            {
                lights.Clear();
                group2.GetBlocksOfType(lights);
            }

            sound = GridTerminalSystem.GetBlockWithName(soundBlockName) as IMySoundBlock;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            {
                if ((updateSource & (UpdateType.Trigger | UpdateType.Terminal)) != 0)
                {
                    RunCommand(argument);
                }

                if ((updateSource & UpdateType.Update10) != 0)
                {
                    deltaTime = (float)Runtime.TimeSinceLastRun.TotalMilliseconds / 1000;
                    RunContinuousLogic();
                }
            }
        }

        void RunCommand(string argument)
        {
            if (isRunning) return;
            isRunning = true;
            sound.Play();
            SetLights(true);
        }

        void RunContinuousLogic()
        {
            if (!isRunning) return;

            timer += deltaTime;
            if (timer >= totalAlarmTime)
            {
                isRunning = false;
                doorsOpening = false;
                timer = 0f;
                alarmSoundTimer = 0f;
                lightToggleTimer = 0f;
                SetLights(false);
                sound.Stop();
                return;
            }

            //if we're ready to open the doors, open them.
            if (timer > delayBeforeDoorOpening && !doorsOpening)
            {
                doorsOpening = true;
                foreach (IMyAirtightHangarDoor door in doors)
                {
                    door.ToggleDoor();
                }
            }

            //Lights
            if (flashLights)
            {
                lightToggleTimer += deltaTime;
                if (lightToggleTimer >= lightToggleInterval)
                {
                    lightToggleTimer = 0f;
                    ToggleLights();
                }
            }

            //Soundblock
            if (!loopingAlarmSound)
            {
                alarmSoundTimer += deltaTime;
                if (alarmSoundTimer >= alarmSoundInterval)
                {
                    alarmSoundTimer = 0f;
                    sound.Play();
                }
            }
        }

        private void ToggleLights()
        {
            SetLights(!lights[0].Enabled);
        }

        private void SetLights(bool on)
        {
            foreach (IMyLightingBlock light in lights)
            {
                light.Enabled = on;
            }
        }
    }
}
