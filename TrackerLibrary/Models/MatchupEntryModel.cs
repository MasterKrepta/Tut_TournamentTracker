﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary.Models
{
    public class MatchupEntryModel
    {
        /// <summary>
        /// Represents a team in the matchup
        /// </summary>
        public TeamModel TeamCompeting { get; set; }

        /// <summary>
        /// Score for this team
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Matchup that this team came from
        /// </summary>
        public MatchupEntryModel ParentMatchup { get; set; }

    }
}