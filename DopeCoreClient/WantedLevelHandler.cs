using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;

namespace DopeCoreClient
{
    public class WantedLevelHandler : BaseScript
    {
        private bool _currentEnable;
        
        public WantedLevelHandler()
        {
            EventHandlers.Add("WantedLevel:SetEnable", new Action<bool>(EnableWantedLevel));
            
            _currentEnable = false;
            
            Tick += OnTick;
        }

        private async Task OnTick()
        {
            try
            {
                var player = Game.Player;
    
                if (!_currentEnable)
                {
                    if(player.WantedLevel > 0)
                        player.WantedLevel = 0;
                    player.DispatchsCops = false;
                }
                else
                {
                    player.DispatchsCops = true;
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
            _currentEnable = enabled;
        }
    }
}