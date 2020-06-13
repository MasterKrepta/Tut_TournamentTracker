using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
    public interface IDataConnection
    {
        void CreatePrize(PrizeModel model);
        void CreatePerson(PersonModel model);
        void CreateTeam(TeamModel model);
        void CreateTournament(TournamentModel tm);
        void UpdateMatchup(MatchupModel model);
        void CompleteTorunament(TournamentModel model);

        List<TournamentModel> GetTournament_All();
        List<PersonModel> GetPerson_All();
        List<TeamModel> GetTeam_All();
    
    }
}
