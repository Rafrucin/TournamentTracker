using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary.Models
{
    public class TournamentModel
    {
        public int Id { get; set; }
        public string TournamentName { get; set; }
        public decimal EntryFee { get; set; }
        public List<TeamModel> TeamEntered { get; set; } = new List<TeamModel>();
        public List<List<MatchupModel>> Rounds { get; set; } = new List<List<MatchupModel>>();
        public List<PrizeModel> Prizes { get; set; } = new List<PrizeModel>();
    }
}
