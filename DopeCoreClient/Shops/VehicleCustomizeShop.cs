using System;
using System.Drawing;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;

namespace DopeCoreClient.Shops
{
    internal class VehicleCustomizeShop : BaseScript
    {
        private static readonly Vector3 VehicleCustomizeShopLocation = new Vector3(0f,0f,0f);
        private static readonly Vector3 VehicleCustomizeShopDoorLocation = new Vector3(0f,0f,0f);
        private static readonly Vector3 VehicleCustomizeShopSafePositionOnEntry = new Vector3(0f,0f,0f);

        private Blip _vehicleCustomizeShopBlip;

        private bool _hasEntered = false;
        private bool exiting = false; // temp bool
        
        internal VehicleCustomizeShop()
        {
            _vehicleCustomizeShopBlip.Name = "Motorsports";
            _vehicleCustomizeShopBlip.IsShortRange = true;
            _vehicleCustomizeShopBlip.Sprite = BlipSprite.Simeon;
            _vehicleCustomizeShopBlip.Color = BlipColor.White;
            _vehicleCustomizeShopBlip.Position = VehicleCustomizeShopLocation;

            Tick += OnTick;
        }

        private async Task OnTick()
        {
            await Task.FromResult(0);

            if (!Game.PlayerPed.IsInVehicle()) return;
            
            var playerPed = Game.PlayerPed;
            
            if (playerPed.Position.DistanceToSquared(VehicleCustomizeShopDoorLocation) < 4 && !_hasEntered)
            {
                Game.DisableAllControlsThisFrame(0);
                Screen.Fading.FadeOut(50);
                
                playerPed.CurrentVehicle.Speed = 0f;
                playerPed.CurrentVehicle.PositionNoOffset = VehicleCustomizeShopSafePositionOnEntry;
                
                Screen.Fading.FadeIn(250);
                
                playerPed.Task.DriveTo(playerPed.CurrentVehicle, VehicleCustomizeShopLocation, 1f, 10f);
                
                _hasEntered = true;
                
                // TODO Add synchronization of shop occupancy
                TriggerServerEvent("VehicleCustomizeShop:Occupancy", true);
            }

            if (_hasEntered)
            {
                // Disable controls until car is in place.
                if(playerPed.Position.DistanceToSquared(VehicleCustomizeShopLocation) > 1f)
                    Game.DisableAllControlsThisFrame(0);
                else
                {
                    DisableVehicleControls();
                    // If menu is invisible, make visible
                    // Do camera stuff with menu to apply new mods.
                    // TODO Menu + Camera stuff
                    
                    // If Exit is pressed, drive backwards out of the garage
                    // Temp bool
                    if (exiting)
                    {
                        Game.DisableAllControlsThisFrame(0);
                        playerPed.Task.DriveTo(playerPed.CurrentVehicle, VehicleCustomizeShopDoorLocation, 1f, 10f,
                            (int) DrivingStyle.Backwards);
                    }

                    // TODO Find a way so that entering procedure doesn't get triggered again after leaving
                    
                    if (playerPed.CurrentVehicle.Position.DistanceToSquared(VehicleCustomizeShopDoorLocation) > 400f) // Temp Fix
                    {
                        _hasEntered = false;
                        TriggerServerEvent("VehicleCustomizeShop:Occupancy", false);
                    }
                }
            }
        }

        private static void DisableVehicleControls()
        {
            Game.DisableControlThisFrame(0, Control.VehicleAccelerate);
            Game.DisableControlThisFrame(0, Control.VehicleBrake);
            Game.DisableControlThisFrame(0, Control.VehicleExit);
            Game.DisableControlThisFrame(0, Control.VehicleSubTurnHardLeft);
            Game.DisableControlThisFrame(0, Control.VehicleSubTurnHardRight);
            Game.DisableControlThisFrame(0, Control.VehicleSubMouseControlOverride);
            Game.DisableControlThisFrame(0, Control.VehicleAim);
        }
    }
}