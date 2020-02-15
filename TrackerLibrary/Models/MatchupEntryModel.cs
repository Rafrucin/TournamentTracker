using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary.Models
{
    public class MatchupEntryModel
    {
        public TeamModel TeamCompiting { get; set; }
        public double score { get; set; }
        public MatchupModel ParentMatchup { get; set; }
    }
}
