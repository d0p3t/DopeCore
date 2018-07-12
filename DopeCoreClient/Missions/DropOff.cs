// DropOff Mission
// Stages
// -------
// 1: No enemies or obstacles
// 2: Five enemies, Zero obstacles
// 3: Five enemies, Two obstacles
// 4: Ten enemies, Two obstacles
// 5: Twenty-Five enemies, Five Obstacles

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;

namespace DopeCoreClient.Missions
{
    public enum Stage
    {
        None = 0,
        First = 1,
        Second = 2,
        Third = 3,
        Fourth = 4,
        Fifth = 5
    }
    
    internal class DropOff : BaseScript
    {
        private static readonly Vector3 DropOffMarker = new Vector3(0f,0f,0f);
        private static readonly Color DropOffColor = Color.FromArgb(150, 255, 255, 0);
        private static readonly Vector3 DropOffStartPosition = new Vector3(0f,0f,0f);
        private static readonly Vector3 DropOffVehicleStartPosition = new Vector3(0f,0f,0f);
        private static readonly Vector3 DropOffEndPosition = new Vector3(0f,0f,0f);
        
        private static readonly List<Vector3> DropOffLocations = new List<Vector3>
        {
            new Vector3(0,0,0),
            new Vector3(0,0,0),
            new Vector3(0,0,0)
        };

        private Stage _currentStage = Stage.None;
        
        private bool _isNear = false;
        private bool _isStarted = false;

        private int _totalEnemies = 45;
        private int _totalPackages = 5;
        
        private int _enemiesKilled = 0;
        private int _packagesDelivered = 0;
        private int _foundEasterEgg = 0;
        private int _enemiesKilledComplete = 0;
        private int _packagesDeliveredComplete = 0;
        
        private Scaleform _dropOffMarkerScaleform;
        private Scaleform _dropOffProgressScaleform;
        
        private Vehicle vehicle = null;
        private Blip vehicleBlip;
        private Blip deliveryBlip;
        
        internal DropOff()
        {
            _dropOffMarkerScaleform = new Scaleform("mp_mission_name_freemode");
            _dropOffProgressScaleform = new Scaleform("mission_complete");

            Tick += OnTick;
            Tick += OnMissionScaleFormTick;
            Tick += OnMarkerTick;
        }

        private async Task OnTick()
        {
            if (vehicle != null)
            {
                vehicleBlip.Position = vehicle.Position;
                vehicleBlip.ShowRoute = false;
                vehicleBlip.Sprite = BlipSprite.BigCircle;
                vehicleBlip.Color = BlipColor.Yellow;
            }
            else
            {
                vehicleBlip.Delete();
            }
            
            if (Game.IsControlJustReleased(0, Control.Context) && _isNear && _isStarted == false)
            {
                var playerPed = Game.PlayerPed;
                _isStarted = true;
                
                Screen.Fading.FadeOut(500);
                if (playerPed.IsInVehicle()) playerPed.CurrentVehicle.Delete();
                playerPed.Position = DropOffStartPosition;
                
                vehicle = await World.CreateVehicle(VehicleHash.Faggio, DropOffVehicleStartPosition);
                FreezeEntityPosition(playerPed.Handle, true);
                
                Screen.Fading.FadeIn(500);
                Audio.PlaySoundFrontend("5s_To_Event_Start_Countdown", "GTAO_FM_Events_Soundset");
                await Delay(5000);
                FreezeEntityPosition(playerPed.Handle, false);
            }

            if (_isStarted)
            {
                var playerPed = Game.PlayerPed;
                if (playerPed.CurrentVehicle != vehicle)
                {
                    switch (_currentStage)
                    {
                        case Stage.None:
                            break;
                        case Stage.First:
                            Screen.ShowSubtitle("Hop onto the Faggio to START");
                            break;
                        case Stage.Second:
                            break;
                        case Stage.Third:
                            break;
                        case Stage.Fourth:
                            break;
                        case Stage.Fifth:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    DrawMissionProgressScaleForm();
                    
                    switch (_currentStage)
                    {
                        case Stage.None:
                            Screen.ShowSubtitle("Deliver the package");
                            _currentStage = Stage.First;
                            break;
                        case Stage.First:
                            StageOne();
                            break;
                        case Stage.Second:
                            StageTwo();
                            break;
                        case Stage.Third:
                            StageThree();
                            break;
                        case Stage.Fourth:
                            StageFour();
                            break;
                        case Stage.Fifth:
                            StageFive();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private async Task OnMissionScaleFormTick()
        {
            await Task.FromResult(0);

            if (!_isNear)
            {
                if(_dropOffMarkerScaleform.IsLoaded) _dropOffMarkerScaleform.Dispose();
                return;
            }

            var playerPed = Game.PlayerPed;
            
            _dropOffMarkerScaleform.CallFunction("SET_MISSION_INFO", "Drop Off", "Action", "Player Info", "", "", true, 1,
                999, 9999, "");
            _dropOffMarkerScaleform.Render3D(playerPed.GetOffsetPosition(new Vector3(0, 2f, 0)), - playerPed.Rotation, Vector3.One);
        }

        private async Task OnMarkerTick()
        {
            await Task.FromResult(0);

            if (Game.PlayerPed.Position.DistanceToSquared(DropOffMarker) > 1000) return;
            
            World.DrawMarker(MarkerType.HorizontalCircleSkinny, DropOffMarker, Vector3.Zero, Vector3.Zero, Vector3.One, DropOffColor);
            _isNear = true;
        }

        private async void StageOne()
        {
            await Task.FromResult(0);

            var dropOffLocation = DropOffLocations.First();
            var playerPos = Game.PlayerPed.Position;
            
            if (!deliveryBlip.Exists())
            {
                deliveryBlip.Color = BlipColor.Red;
                deliveryBlip.ShowRoute = true;
                deliveryBlip.Position = dropOffLocation;
                deliveryBlip.Sprite = BlipSprite.CaptureBriefcase;
            }

            if (playerPos.DistanceToSquared(dropOffLocation) < 1000)
                World.DrawMarker(MarkerType.HorizontalCircleSkinny, dropOffLocation, Vector3.Zero, Vector3.Zero, Vector3.One, DropOffColor);
            
            if (playerPos.DistanceToSquared(dropOffLocation) < 4)
            {
                if(Game.PlayerPed.IsInVehicle()) 
                    Screen.ShowSubtitle("Get out of vehicle to deliver package");
                else
                {
                    Screen.DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to Deliver Package");

                    if (Game.IsControlJustReleased(0, Control.Context))
                    {
                        DeliverPackage(Stage.Second);
                    }
                }
            }
        }

        private async void StageTwo()
        {
            await Task.FromResult(0);

            var playerPed = Game.PlayerPed;
            
            var dropOffLocation = DropOffLocations[1];

            if (!deliveryBlip.Exists())
            {
                deliveryBlip.Color = BlipColor.Red;
                deliveryBlip.ShowRoute = true;
                deliveryBlip.Position = dropOffLocation;
                deliveryBlip.Sprite = BlipSprite.CaptureBriefcase;
            }
            
            // TODO Handle enemy spawns
            if (playerPed.Position.DistanceToSquared(dropOffLocation) == 75)
            {
                var enemyPed = new Ped(CreatePed(0,(uint)PedHash.BikeHire01, dropOffLocation.X, dropOffLocation.Y, dropOffLocation.Z, 0f, true, false));
                // TODO Create 5 enemies in a certain area or at certain positions and make them aggressive towards PlayerPed.Handle
                
            }

            if (playerPed.Position.DistanceToSquared(dropOffLocation) < 1000)
                World.DrawMarker(MarkerType.HorizontalCircleSkinny, dropOffLocation, Vector3.Zero, Vector3.Zero, Vector3.One, DropOffColor);

            if (playerPed.Position.DistanceToSquared(dropOffLocation) < 4)
            {
                if (playerPed.IsInCombat)
                {
                    Screen.ShowSubtitle("You are in combat");
                }
                else if (playerPed.IsInVehicle())
                {
                    Screen.ShowSubtitle("Get out of vehicle to deliver package");
                }
                else
                {
                    DeliverPackage(Stage.Third);
                }
            }
        }

        private async void StageThree()
        {
            await Delay(0);
        }

        private async void StageFour()
        {
            await Delay(0);
        }

        private async void StageFive()
        {
            await Delay(0);
        }

        private async void DeliverPackage(Stage nextStage)
        {
            Screen.DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to Deliver Package");

            if (Game.IsControlJustReleased(0, Control.Context))
            {
                TaskPlayAnim(Game.PlayerPed.Handle, "mp_safehouselost@", "package_dropoff", 1f, 1f, 1500, 0, 1, true,
                    true, true);
                await Delay(1500);
                Audio.PlaySoundFrontend("Beast_Checkpoint", "APT_BvS_Soundset");
                deliveryBlip.Delete();
                _packagesDelivered++;
                _currentStage = nextStage;
                Screen.ShowSubtitle("Delivery Complete! Deliver next package");
            }
        }

        private void DrawMissionProgressScaleForm()
        {
            // From: https://pastebin.com/SyyMaQD1
            
            _dropOffProgressScaleform.CallFunction("SET_MISSION_TITLE","Drop Off Mission Title","Drop Off Description");
            
            // numOfObjective,completed?,objectiveTypeId(?),current,out of,title
            if (_packagesDelivered == _totalPackages) _packagesDeliveredComplete = 1;
            if (_enemiesKilled == _totalEnemies) _enemiesKilledComplete = 1;
            
            var completion = Math.Round(((_packagesDelivered / _totalPackages) + (_enemiesKilled / _totalEnemies) +
                              (_foundEasterEgg / 1)) * 100.0f);
            
            _dropOffProgressScaleform.CallFunction("SET_DATA_SLOT", 0, _packagesDeliveredComplete, 2, _packagesDelivered, _totalPackages, "Delivered Packages");
            _dropOffProgressScaleform.CallFunction("SET_DATA_SLOT", 1, _enemiesKilledComplete, 2, _enemiesKilled, _totalEnemies, "Killed Enemies");
            
            // numOfObjected,completed?,title
            _dropOffProgressScaleform.CallFunction("SET_DATA_SLOT", 2, 0, "Easter Egg");
            
            // medal type, completion percentage, title
            _dropOffProgressScaleform.CallFunction("SET_TOTAL", 1, completion, "Completion - Gold");
            _dropOffProgressScaleform.CallFunction("SET_MEDAL", 1.0, -1.0, -1.0, -1.0, -1.0);
            _dropOffProgressScaleform.CallFunction("DRAW_MENU_LIST");
            
            _dropOffProgressScaleform.Render2D();
            
        }
    }
}