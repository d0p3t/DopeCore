using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;

namespace DopeCoreClient
{
    public class SpawnManagement : BaseScript
    {
        private static readonly Vector3 HospitalPosition = new Vector3(0f,0f,0f); // Get coords viewing Pillbox entrance
        private static readonly Vector3 PlayerHousePosition = new Vector3(0f,0f,0f); // Get coords of Legion Square
        
        private bool _isDead = false;
        private bool _hasSpawned = false;

        private Scaleform _deadMessage;
        
        public SpawnManagement()
        {
            
            // Dynamically load player's house position
            // Select player's house from database. If null, set on street

            Tick += OnTick;
            Tick += OnIsDead;
        }
        
        private async Task OnTick()
        {
            var playerExists = (Game.PlayerPed.Handle != 0);
            var playerActive = NetworkIsPlayerActive(PlayerId());
            
            // First Spawn
            if (playerExists && playerActive && !_hasSpawned)
            {
                SpawnPlayer();
                _hasSpawned = true;
            }
            
            if (Game.Player.IsDead && !_isDead)
            {
                var playerPed = Game.PlayerPed;
                _deadMessage = new Scaleform("MP_BIG_MESSAGE_FREEMODE");
                _isDead = true;
                Screen.Effects.Start(ScreenEffect.DeathFailMpIn);
                Screen.Fading.FadeOut(500);
                await Delay(3000);
                SetEntityVisible(playerPed.Handle, false, false);
                playerPed.Position = HospitalPosition;
                FreezeEntityPosition(playerPed.Handle, true);
                Screen.Fading.FadeIn(500);
            }

            await Task.FromResult(0);
        }
        
        private async Task OnIsDead()
        {
            await Task.FromResult(0);

            if (_isDead)
            {       
                _deadMessage.CallFunction("SHOW_SHARD_WASTED_MP_MESSAGE", "~r~Wasted", "Press ~INPUT_CONTEXT~ to respawn", -1, true, true);
                _deadMessage.Render2D();
                
                if (Game.IsControlJustReleased(0, Control.Context))
                {
                    var playerPed = Game.PlayerPed;
                    SetEntityVisible(playerPed.Handle, true, true);
                    FreezeEntityPosition(playerPed.Handle, false);
                    Screen.Fading.FadeOut(500);
                    playerPed.Position = PlayerHousePosition;
                    playerPed.Resurrect();
                    Screen.Effects.Stop(ScreenEffect.DeathFailMpIn);
                    _isDead = false;
                    _deadMessage.Dispose();
                }
            }
        }

        private async void SpawnPlayer()
        {
            Screen.Fading.FadeOut(100);
            
            var playerPed = Game.PlayerPed;
            var player = Game.Player;
            
            FreezePlayer(playerPed, player, true);
            
            RequestCollisionAtCoord(PlayerHousePosition.X, PlayerHousePosition.Y, PlayerHousePosition.Z);
            
            await player.ChangeModel(PedHash.Bankman);
            playerPed.PositionNoOffset = PlayerHousePosition;
            playerPed.Heading = 0.0f;
            
            ClearPedTasksImmediately(playerPed.Handle);
            ClearPlayerWantedLevel(player.Handle);

            while (!HasCollisionLoadedAroundEntity(playerPed.Handle))
                await Delay(0);
            
            ShutdownLoadingScreen();
                        
            FreezePlayer(playerPed, player, false);
            
            Screen.Fading.FadeIn(500);
            TriggerEvent("playerSpawned");
        }
        
        private static void FreezePlayer(Ped playerPed,Player player, bool freeze)
        {
            player.CanControlCharacter = !freeze;

            playerPed.IsVisible = !freeze;
            SetEntityCollision(playerPed.Handle, !freeze, false);
            FreezeEntityPosition(playerPed.Handle, freeze);
            SetPlayerInvincible(player.Handle, freeze);
            
            if(!playerPed.IsInjured)
                ClearPedTasksImmediately(playerPed.Handle);
        }
    }
}