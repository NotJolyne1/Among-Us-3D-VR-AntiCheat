using AntiCheat.Config;
using Il2CppSG.Airlock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntiCheat
{
    internal class Helpers
    {
        public static PlayerState GetPlayerstateFromID(int id)
        {
            foreach (PlayerState player in GameReferences.Spawn!.ActivePlayerStates)
                if (player.PlayerId == id)
                    return player;

            return null;
        }
    }
}
