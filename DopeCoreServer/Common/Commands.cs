using System;
using System.Collections.Generic;
using System.ComponentModel;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace DopeCoreServer.Common
{
    public static class Commands
    {
        
        private static List<string> commandList = new List<string>();

        public static void Add(string commandName, Action handler, bool restricted = false)
        {
            API.RegisterCommand(commandName, handler, restricted);
        }
    }
}