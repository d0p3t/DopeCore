using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;

namespace DopeCoreClient
{
    public class WantedLevelHandler : BaseScript
    {
        private bool _currentWantedLevelEnable;
        private bool _currentPvPEnable;
        
        public WantedLevelHandler()
        {
            EventHandlers.Add("WantedLevel:SetEnable", new Action<bool>(EnableWantedLevel));
            EventHandlers.Add("PvP:SetEnable", new Action<bool>(EnablePvP));
            
            _currentWantedLevelEnable = false;
            _currentPvPEnable = false;
            
            Tick += OnTick;
        }

        private async Task OnTick()
        {
            try
            {
                var player = Game.Player;
   
                // WantedLevel
                if (!_currentWantedLevelEnable)
                {
                    if(player.WantedLevel > 0)
                        player.WantedLevel = 0;
                    player.DispatchsCops = false;
                }
                else
                {
                    player.DispatchsCops = true;
                }
                
                // PvP
                if (!_currentPvPEnable)
                {
                    SetCanAttackFriendly(Game.PlayerPed.Handle, false, false);
                    NetworkSetFriendlyFireOption(false);
                }
                else
                {
                    SetCanAttackFriendly(Game.PlayerPed.Handle, true, false);
                    NetworkSetFriendlyFireOption(true);
                }

                // Also hide some HudComponents
                Screen.Hud.HideComponentThisFrame(HudComponent.AreaName);
                Screen.Hud.HideComponentThisFrame(HudComponent.StreetName);
                Screen.Hud.HideComponentThisFrame(HudComponent.VehicleName);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            await Task.FromResult(0);
        }
        
        private void EnableWantedLevel(bool enabled)
        {
            _currentWantedLevelEnable = enabled;
        }

        private void EnablePvP(bool enabled)
        {
            _currentPvPEnable = enabled;
        }
    }
}