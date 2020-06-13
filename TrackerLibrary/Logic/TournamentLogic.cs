using System;
using System.Collections.Generic;
using System.Configuration;
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

        public static void UpdateTournamentResults(TournamentModel model)
        {
            int startingRound = model.CheckCurrentRound();
            List<MatchupModel> toScore = new List<MatchupModel>();
            foreach (var round in model.Rounds)
            {
                foreach (var rm in round)
                {
                    if (rm.Winner == null &&  (rm.Entries.Any(x => x.Score != 0) || rm.Entries.Count == 1)) // if anyone has a score or if theres a bye week
                    {
                        toScore.Add(rm);
                    }
                }
            }

            MarkWinnersInMatchups(toScore);

            AdvanceWinners(toScore, model);

            toScore.ForEach(x => GlobalConfig.Connection.UpdateMatchup(x));
            int endingRound = model.CheckCurrentRound();

            if (endingRound > startingRound)
            {
                model.AlertUsersToNewRound();
            }
            
        }

        public static void AlertUsersToNewRound(this TournamentModel model)
        {
            int currentRoundNumber = model.CheckCurrentRound();

            List<MatchupModel> currentRound = model.Rounds.Where(x => x.First().MatchupRound == currentRoundNumber).First();

            foreach (MatchupModel m in currentRound)
            {
                foreach (MatchupEntryModel me in m.Entries)
                {
                    foreach (PersonModel person in me.TeamCompeting.TeamMembers)
                    {
                        AlertPersonToNewRound(person, me.TeamCompeting.TeamName, m.Entries.Where(x => x.TeamCompeting != me.TeamCompeting).FirstOrDefault());
                    }
                }
            }
        }

        private static void AlertPersonToNewRound(PersonModel person, string teamName, MatchupEntryModel competitor)
        {
            if (person.EmailAddress.Length == 0)
            {
                return;
            }

            
            string to = "";
            string subject = "";
            
            StringBuilder body = new StringBuilder();

            if (competitor != null)
            {
                subject = $"You have a new matchup with { competitor.TeamCompeting.TeamName }";
                
                body.AppendLine("<h1>You have a new matchup</h1>");
                body.Append($"<strong>Competitor: </strong>");
                body.Append(competitor.TeamCompeting.TeamName);
                body.AppendLine();
                body.AppendLine();
                body.Append("Have a great time!");
                body.Append("~Tournament Tracker");
            }
            else
            {
                subject = $"You have a bye week this round";

                body.AppendLine("Enjoy your week Off!");
                body.AppendLine("~Tournament Tracker");
            }


            to = person.EmailAddress;

            EmailLogic.SendEmail(to, subject, body.ToString());
        }

        private static int CheckCurrentRound(this TournamentModel model)
        {
            int output = 1;

            foreach (var round in model.Rounds)
            {
                if (round.All(x => x.Winner != null))
                {
                    output++;
                }
                else
                {
                    return output;
                }
            }

            //torunament is complete if we get here
            CompleteTournment(model);
            return output -1;
        }

        private static void CompleteTournment(TournamentModel model)
        {
            GlobalConfig.Connection.CompleteTorunament(model);
            MatchupModel championshipRound = model.Rounds.Last().First();

            TeamModel winners = championshipRound.Winner;
            TeamModel runnerUp = championshipRound.Entries.Where(x => x.TeamCompeting != winners).First().TeamCompeting;

            decimal winnerPrize = 0;
            decimal runnerupPrize = 0;


            if (model.Prizes.Count > 0)
            {
                decimal totalIncome = model.EnteredTeams.Count * model.EntryFee;

                PrizeModel firstPlacePrize = model.Prizes.Where(x => x.PlaceNumber == 1).FirstOrDefault();
                PrizeModel secondPlacePrize = model.Prizes.Where(x => x.PlaceNumber == 2).FirstOrDefault();

                if (firstPlacePrize != null)
                {
                    winnerPrize = firstPlacePrize.CalculatePrizePayout(totalIncome);
                }

                if (secondPlacePrize != null)
                {
                    runnerupPrize = secondPlacePrize.CalculatePrizePayout(totalIncome);
                }
            }

            //Send Email
            string to = "";
            string subject = "";

            StringBuilder body = new StringBuilder();

            
            subject = $"In { model.TournamentName }, { winners.TeamName } has won!!!";

            body.AppendLine("<h1>You have a WINNER!</h1>");
            body.AppendLine($"<p>Congratulations to <strong>{ winners.TeamName }</strong> on a great tournament.</p>");
            body.AppendLine("<br />");
            body.AppendLine();
            body.AppendLine();
            
            if (winnerPrize > 0 || runnerupPrize > 0)
            {
                body.AppendLine($"<p>{winners.TeamName } will receive ${ winnerPrize }.</p>");
                body.AppendLine($"<p>{runnerUp.TeamName } will receive ${ runnerupPrize }.</p>");
            }

            body.AppendLine("~Tournament Tracker");

            List<string> bcc = new List<string>();
            foreach (var t in model.EnteredTeams)
            {
                foreach (var p in t.TeamMembers)
                {
                    if (p.EmailAddress.Length > 0)
                    {
                        bcc.Add(p.EmailAddress); 
                    }
                }
            }

            EmailLogic.SendEmail(new List<string>(), bcc, subject, body.ToString());
            model.CompleteTournament();
        }


        private static decimal CalculatePrizePayout(this PrizeModel prize, decimal totalIncome)
        {
            decimal output = 0;
            if (prize.PrizeAmount > 0)
            {
                output = prize.PrizeAmount;
            }
            else
            {
                output =  decimal.Multiply(totalIncome, Convert.ToDecimal(prize.PrizePercentage / 100));
            }

            return output;
        }
        private static void AdvanceWinners(List<MatchupModel> models, TournamentModel tournament)
        {

            foreach (var m in models)
            {
                foreach (var round in tournament.Rounds)
                {
                    foreach (var rm in round)
                    {
                        foreach (var me in rm.Entries)
                        {
                            if (me.ParentMatchup != null)
                            {
                                if (me.ParentMatchup.Id == m.Id)
                                {
                                    me.TeamCompeting = m.Winner;
                                    GlobalConfig.Connection.UpdateMatchup(rm);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void MarkWinnersInMatchups(List<MatchupModel> models)
        {
            string greaterWins = ConfigurationManager.AppSettings["greaterWins"];

            //0 means falls, or low score wins
            foreach (   var m in models)
            {
                //check for bye week
                if (m.Entries.Count == 1)
                {
                    m.Winner = m.Entries[0].TeamCompeting;
                    continue;
                }

                if (greaterWins == "0")
                {
                    if (m.Entries[0].Score < m.Entries[1].Score)
                    {
                        m.Winner = m.Entries[0].TeamCompeting;
                    }
                    else if(m.Entries[1].Score < m.Entries[0].Score)
                    {
                        m.Winner = m.Entries[1].TeamCompeting;
                    }
                    else
                    {
                        throw new NotSupportedException("We dont handle tie games");
                    }
                }
                else
                {
                    //1 means true, high score wins
                    if (m.Entries[0].Score > m.Entries[1].Score)
                    {
                        m.Winner = m.Entries[0].TeamCompeting;
                    }
                    else if (m.Entries[1].Score > m.Entries[0].Score)
                    {
                        m.Winner = m.Entries[1].TeamCompeting;
                    }
                    else
                    {
                        throw new NotSupportedException("We dont handle tie games");
                    }
                } 
            }

     
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
