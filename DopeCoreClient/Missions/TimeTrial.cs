using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;

namespace DopeCoreClient.Missions
{
    internal class TimeTrial : BaseScript
    {
        private static readonly List<Tuple<int, string, string, Vector3, Vector3, Vector3>> TimeTrials = new List<Tuple<int, string, string, Vector3, Vector3, Vector3>>
        {
            //          ID        Title         Description        MarkerPos             TrialStartPos            TrialEndPos
            Tuple.Create(1, "TimeTrialTitle1", "Description", new Vector3(0f,0f,0f), new Vector3(0f,0f,0f), new Vector3(0f,0f,0f)),
            Tuple.Create(2, "TimeTrialTitle2", "Description", new Vector3(0f,0f,0f), new Vector3(0f,0f,0f), new Vector3(0f,0f,0f)),
            Tuple.Create(3, "TimeTrialTitle3", "Description", new Vector3(0f,0f,0f), new Vector3(0f,0f,0f), new Vector3(0f,0f,0f))
        };

        private static readonly Color TimeTrialMarkerColor = Color.FromArgb(255, 255, 255, 255);

        private Scaleform _timetrialMarkerScaleform;
        private Scaleform _timetrialCountdownScaleform;
        private Scaleform _timetrialCompletedScaleform;

        private bool _isNearTimeTrial;
        private bool _hasStarted;
        private bool _countDownDone;

        private int _selectedTimeTrial;

        private long _gameTimeAtStart;
        private long _currentTime;
        private long _finalTime;
        
        internal TimeTrial()
        {
            _timetrialMarkerScaleform = new Scaleform("mp_mission_name_freemode");
            _timetrialCountdownScaleform = new Scaleform("countdown");
            
            _hasStarted = false;
            _currentTime = 0;
            _selectedTimeTrial = 0;
            _countDownDone = false;
            
            Tick += OnTick;
            Tick += OnTimeTrialTick;
            Tick += OnMarkerTick;
        }

        private async Task OnTick()
        {
            try
            {
                var playerPed = Game.PlayerPed;

                if (!playerPed.IsInVehicle() &&
                    GetPedInVehicleSeat(playerPed.CurrentVehicle.Handle, -1) != playerPed.Handle) return;
                
                _isNearTimeTrial = false; // Reset boolean before check
                
                foreach (var timetrial in TimeTrials)
                {
                    
                    if (playerPed.Position.DistanceToSquared(timetrial.Item4) < 10)
                    {
                        Screen.Effects.Start(ScreenEffect.RaceTurbo, 0, true); // Or something else
                        Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to Start TimeTrial ({timetrial.Item2})");

                        // TODO Scaleform.Dispose() is performed trashy. Find another way
                        _timetrialMarkerScaleform.CallFunction("SET_MISSION_INFO", $"{timetrial.Item2}", "Time Trial", $"{timetrial.Item3}", "", "", true, 1,
                            999, 9999, "");
                        _timetrialMarkerScaleform.Render3D(timetrial.Item4, - playerPed.Rotation, Vector3.One);
                        _isNearTimeTrial = true;

                        if (!Game.IsControlJustReleased(0, Control.Context)) continue;
                        
                        Screen.Fading.FadeOut(500);
                        playerPed.CurrentVehicle.PositionNoOffset = timetrial.Item5;
                        _hasStarted = true;
                        _selectedTimeTrial = timetrial.Item1;
                        Screen.Fading.FadeIn(500);
                    }
                }

                if (!_isNearTimeTrial)
                {
                    if (!Screen.Effects.IsActive(ScreenEffect.RaceTurbo))
                        Screen.Effects.Stop(ScreenEffect.RaceTurbo);
                    _timetrialMarkerScaleform.Dispose();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            await Task.FromResult(0);
        }

        private async Task OnTimeTrialTick()
        {
            try
            {
                if (_hasStarted)
                {
                    var playerPed = Game.PlayerPed;

                    if (!_countDownDone)
                    {
                        var countdownTime = 10;

                        while (countdownTime != 0)
                        {
                            Game.DisableControlThisFrame(0, Control.VehicleSubTurnHardLeft);
                            Game.DisableControlThisFrame(0, Control.VehicleSubTurnHardRight);
                            SetVehicleForwardSpeed(playerPed.CurrentVehicle.Handle, 0f);

                            Audio.PlaySoundFromEntity(playerPed, "CHECKPOINT_AHEAD", "HUD_MINI_GAME_SOUNDSET");
                            _timetrialCountdownScaleform.CallFunction("SET_MESSAGE", $"{countdownTime}", 255, 255, 255,
                                true);
                            _timetrialCountdownScaleform.Render2D();
                            countdownTime--;

                            await Delay(1000);
                        }
                        _timetrialCountdownScaleform.CallFunction("SET_MESSAGE", "GO!", 255, 255, 255, true);
                        _timetrialCountdownScaleform.Render2D();
                        
                        
                        _gameTimeAtStart = GetGameTimer();
                        
                        _countDownDone = true;
                    }
                    
                    if(_timetrialCountdownScaleform.IsLoaded)
                        _timetrialCountdownScaleform.Dispose();
                    
                    if (playerPed.CurrentVehicle.Position.DistanceToSquared(TimeTrials[_selectedTimeTrial].Item6) < 1)
                    {
                        _finalTime = GetGameTimer() - _gameTimeAtStart;
                        // TODO RaceComplete Scaleform _timetrialCompletedScaleform
                        
                        // Reset variables
                        _hasStarted = false;
                        _countDownDone = false;
                        _currentTime = 0;
                        return;
                    }

                    _currentTime = GetGameTimer() - _gameTimeAtStart;
                    
                    // TODO Draw RaceTimer Scaleform
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            await Task.FromResult(0);
        }
        private async Task OnMarkerTick()
        {
            try
            {
                foreach (var tuple in TimeTrials)
                {
                    if(Game.PlayerPed.Position.DistanceToSquared(tuple.Item4) < 1000)
                        World.DrawMarker(MarkerType.CheckeredFlagCircle, tuple.Item4, Vector3.Zero, Vector3.Zero, Vector3.One, TimeTrialMarkerColor);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            
            await Task.FromResult(0);
        }
    }
}