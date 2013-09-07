using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.BFHRconProtocol;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Interfaces;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;

namespace Kernel.BFHAdmin.Module.DefaultScript
{
    public class PlayerHighlightNotifier : IAmAModule
    {

        private RconClient _rconClient;
        public void Register(RconClient rconClient)
        {
            _rconClient = rconClient;

        }

        public void ModuleLoadComplete()
        {
        }

        private void PlayerListCommandOnPlayerLeft(object sender, Player player)
        {
        }

        private void PlayerListCommandOnPlayerJoined(object sender, Player player)
        {
        }

        private void ScoreOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs, Player player)
        {
            //var score = _playerScore[player];
            //if (score != player.Score.Score)
            //{
            //    var scoreDiff = player.Score.Score - score;
            //    string scoreDiffStr = scoreDiff.ToString();
            //    if (scoreDiff > 0)
            //        scoreDiffStr = "+" + scoreDiffStr;
            //    _rconClient.SendMessagePlayer(player, "KD: Score: " + scoreDiffStr);
            //    _playerScore[player] = player.Score.Score;
            //}
        }



    }
}
