using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;

namespace DopeCoreClient.FunEvents
{
    internal class ClubEvent : BaseScript
    {
        private static readonly Vector3 ClubEventLocation = new Vector3(0f,0f,0f);
        private static readonly Vector3 ClubEventDoorEnterLocation = new Vector3(0f,0f,0f);
        private static readonly Vector3 ClubEventDoorExitLocation = new Vector3(0f,0f,0f);
        
        private static readonly List<Vector3> ClubEventPedSpawnLocations = new List<Vector3>
        {
            new Vector3(0f,0f,0f),
            new Vector3(0f,0f,0f),
            new Vector3(0f,0f,0f),
            new Vector3(0f,0f,0f),
            new Vector3(0f,0f,0f)            
        };
        
        private bool _isEnabled;
        private bool _hasEntered;
      
        internal ClubEvent()
        {
            EventHandlers.Add("ClubEvent:Sync", new Action<bool>(SyncClubEventStatus));

            _isEnabled = false;
            _hasEntered = false;

            Tick += OnTick;
            Tick += OnClubEventTick;
        }

        private async Task OnTick()
        {
            try
            {
                var playerPos = Game.PlayerPed.Position;

                if (playerPos.DistanceToSquared(ClubEventDoorEnterLocation) < 2)
                {
                    Screen.Fading.FadeOut(250);
                    playerPos = ClubEventDoorExitLocation;
                    Screen.Fading.FadeIn(250);
                    
                    if (!_isEnabled)
                    {
                        _isEnabled = true;
                        _hasEntered = true;
                        SetupClubEvent();
                        TriggerServerEvent("ClubEvent:Enable", true);
                    }
                }
                
                if (playerPos.DistanceToSquared(ClubEventDoorExitLocation) < 2)
                {
                    Screen.Fading.FadeOut(250);
                    playerPos = ClubEventDoorEnterLocation;
                    Screen.Fading.FadeIn(250);

                    _hasEntered = false;
                    
                    TriggerServerEvent("ClubEvent:UpdateAttendance", false);
                }                
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            await Delay(0);
        }

        private async Task OnClubEventTick()
        {
            try
            {
                if (_hasEntered)
                {
                    // TODO Interactions with Peds
                    
                    // TODO Purchase Drinks/Food
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            await Task.FromResult(0);
        }

        private async void SetupClubEvent()
        {
            // TODO Better Club Scenario
            await World.CreatePed(PedHash.ClubHouseBar01, ClubEventPedSpawnLocations[0]);
            await World.CreatePed(PedHash.ClubHouseBar01, ClubEventPedSpawnLocations[1]);
            await World.CreatePed(PedHash.ClubHouseBar01, ClubEventPedSpawnLocations[2]);
            await World.CreatePed(PedHash.ClubHouseBar01, ClubEventPedSpawnLocations[3]);
            await World.CreatePed(PedHash.ClubHouseBar01, ClubEventPedSpawnLocations[4]);
            
        }
        
        private void SyncClubEventStatus(bool status)
        {
            _isEnabled = status;
        }
    }
}