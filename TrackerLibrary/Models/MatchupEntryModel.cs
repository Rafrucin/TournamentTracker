using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary.Models
{
    public class MatchupEntryModel
    {
        public int Id { get; set; }
        public TeamModel TeamCompiting { get; set; }
        public int TeamCompetingId { get; set; }
        public double score { get; set; }
        public MatchupModel ParentMatchup { get; set; }
        public int ParentMatchupId { get; set; }
    }
}
