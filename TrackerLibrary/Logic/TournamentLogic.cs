using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
    //Randomly order list
    //check if its big enough - add in byes if not 2^4th power
    //create first round
    //create rest of the rounds
    public static class TournamentLogic
    {
        public static void CreateRounds(TournamentModel model)
        {
            List<TeamModel> randomTeams = RandomizeTeamOrder(model.EnteredTeams);
            int rounds = FindNumberOfRounds(randomTeams.Count);
            int byes = NumberOfByes(rounds, randomTeams.Count);

            model.Rounds.Add(CreateFirstRound(byes, randomTeams));
            CreateOtherRounds(model, rounds);

        }

        private static void CreateOtherRounds(TournamentModel model, int rounds)
        {
            int round = 2;
            List<MatchupModel> previousRound = model.Rounds[0];
            List<MatchupModel> currRound = new List<MatchupModel>();
            MatchupModel currMatchup = new MatchupModel();

            while (round <= rounds)
            {
                foreach (var match in previousRound)
                {
                    currMatchup.Entries.Add(new MatchupEntryModel { ParentMatchup = match });
                    if (currMatchup.Entries.Count > 1)
                    {
                        currMatchup.MatchupRound = round;
                        currRound.Add(currMatchup);
                        currMatchup = new MatchupModel();
                    }
                }

                model.Rounds.Add(currRound);
                previousRound = currRound;
                currRound = new List<MatchupModel>();
                round++;

            }
        }

        private static List<MatchupModel> CreateFirstRound(int byes, List<TeamModel> teams)
        {
            List<MatchupModel> output = new List<MatchupModel>();
            MatchupModel current = new MatchupModel();

            foreach (var team in teams)
            {
                current.Entries.Add(new MatchupEntryModel { TeamCompeting = team });

                if (byes > 0 || current.Entries.Count > 1)
                {
                    current.MatchupRound = 1;
                    output.Add(current);
                    current = new MatchupModel();

                    if (byes > 0)
                    {
                        byes--;
                    }
                }
            }
            return output;
        }

        private static int NumberOfByes(int rounds, int numOfTeams)
        {
            int output = 0;
            int totalTeams = 1;

            for (int i = 1; i <= rounds; i++)
            {
                totalTeams *= 2;
            }

            output = totalTeams - numOfTeams;

            return output;
        }

        private static int FindNumberOfRounds(int teamCount)
        {
            int output = 1;
            int val = 2;

            while (val < teamCount)
            {
                output += 1;
                val *= 2;
            }

            return output;
        }

        private static List<TeamModel> RandomizeTeamOrder(List<TeamModel> teams)
        {
            var output = teams.OrderBy(x => Guid.NewGuid()).ToList();
            return output;
        }
    }

}
