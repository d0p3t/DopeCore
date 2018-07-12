using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace DopeCoreClient.Shops
{
    internal class VehicleDealershipShop : BaseScript
    {
        private static readonly Vector3 VehicleDealershipLocation = new Vector3(0f, 0f, 0f);
        private static readonly Vector3 VehicleDealershipDoorLocation = new Vector3(0f,0f,0f);

        private bool _hasEntered = false;
        
        internal VehicleDealershipShop()
        {
            Tick += OnTick;
        }

        private async Task OnTick()
        {
            await Task.FromResult(0);
        }
    }
}