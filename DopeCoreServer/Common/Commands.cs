using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Hosting;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace DopeCoreServer.Common
{
    public class Commands : BaseScript
    {
        
        private static List<string> commandList = new List<string>();

        public Commands()
        {
            //RegisterCommand("info", new Action<int, List<object>, string>((source, arguments, raw) =>
            //{
            //    
            //}), false);
        }
    }
}