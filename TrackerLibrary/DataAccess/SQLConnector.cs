using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;
using Dapper;
using System.Data;

namespace TrackerLibrary.DataAccess
{
    public class SQLConnector : IDataConnection
    {
        private const string db = "Tournaments";
        public PersonModel CreatePerson(PersonModel model)
        {
            using (IDbConnection conn = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString("Tournaments")))
            {
                var p = new DynamicParameters();
                p.Add("@FirstName", model.FirstName);
                p.Add("@Lastname", model.Lastname);
                p.Add("@EmailAddress", model.EmailAddress);
                p.Add("@CellPhoneNumber", model.CellPhoneNumber);
                p.Add("@id", 0, DbType.Int32, direction: ParameterDirection.Output);

                conn.Execute("spPeople_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");

                return model;
            }
        }

        /// <summary>
        /// Saves a new prize to the DB
        /// </summary>
        /// <param name="model"></param>
        /// <returns>The prize info, including the unique identifier</returns>
        public PrizeModel CreatePrize(PrizeModel model)
        {
            using (IDbConnection conn = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString("Tournaments")))
            {
                var p = new DynamicParameters();
                p.Add("@PlaceNumber", model.PlaceNumber);
                p.Add("@PlaceName", model.PlaceName);
                p.Add("@PrizeAmount", model.PrizeAmount);
                p.Add("@PrizePercentage", model.PrizePercentage);
                p.Add("@id", 0, DbType.Int32, direction: ParameterDirection.Output);

                conn.Execute("spPrizes_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");

                return model;
            }
            
        }

        public TeamModel CreateTeam(TeamModel model)
        {
            using (IDbConnection conn = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString("Tournaments")))
            {
                var p = new DynamicParameters();
                p.Add("@TeamName", model.TeamName);
                p.Add("@id", 0, DbType.Int32, direction: ParameterDirection.Output);

                conn.Execute("spTeams_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");

                foreach (var tm in model.TeamMembers)
                {
                    p = new DynamicParameters();
                    p.Add("@TeamId", model.Id);
                    p.Add("@PersonId", tm.Id);

                    conn.Execute("spTeamMembers_Insert", p, commandType: CommandType.StoredProcedure);

                    
                }



                return model;
            }
        }

        public void CreateTournament(TournamentModel model)
        {
            using (IDbConnection conn = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString("Tournaments")))
            {
                SaveTournament(model, conn);
                SaveTournamentPrizes(model, conn);
                SaveTournamentEntries(model, conn);
                SaveTournamentRounds(model, conn);
            }
        }

        private static void SaveTournamentRounds(TournamentModel model, IDbConnection conn)
        {
            //loop rounds
            foreach (List<MatchupModel> round in model.Rounds)
            {
                //loop entries
                
                foreach (MatchupModel matchup in round)
                {
                    //Save matchup
                    var p = new DynamicParameters();
                    p.Add("@TournamentId", model.Id);
                    p.Add("@matchupRound", matchup.MatchupRound);
                    p.Add("@id", 0, DbType.Int32, direction: ParameterDirection.Output);

                    conn.Execute("spMatchups_Insert", p, commandType: CommandType.StoredProcedure);

                    matchup.Id = p.Get<int>("@id");

                    foreach (MatchupEntryModel entry in matchup.Entries)
                    {
                         p = new DynamicParameters();
                        p.Add("@matchupId", matchup.Id);
                        if (entry.ParentMatchup == null) 
                        {
                            p.Add("@parentMatchupId", null);
                        }
                        else
                        {
                            p.Add("@parentMatchupId", entry.ParentMatchup.Id);
                        }
                        
                        if (entry.TeamCompeting == null)
                        {
                            p.Add("@teamCompeting", null);
                        }
                        else
                        {
                            p.Add("@teamCompeting", entry.TeamCompeting.Id);
                        }
                        
                        p.Add("@id", 0, DbType.Int32, direction: ParameterDirection.Output);

                        conn.Execute("spMatchupEntries_Insert", p, commandType: CommandType.StoredProcedure);

                    }
                }
            }
        }

        private static void SaveTournamentEntries(TournamentModel model, IDbConnection conn)
        {
            foreach (var tm in model.EnteredTeams)
            {
                var p = new DynamicParameters();
                p.Add("@TournamentId", model.Id);
                p.Add("@TeamId", tm.Id);
                p.Add("@id", 0, DbType.Int32, direction: ParameterDirection.Output);

                conn.Execute("spTournamentEntries_Insert", p, commandType: CommandType.StoredProcedure);
            }
        }

        private static void SaveTournamentPrizes(TournamentModel model, IDbConnection conn)
        {
            foreach (var pz in model.Prizes)
            {
                var p = new DynamicParameters();
                p.Add("@tournamentId", model.Id);
                p.Add("@prizeId", pz.Id);
                p.Add("@id", 0, DbType.Int32, direction: ParameterDirection.Output);

                conn.Execute("spTournamentPrizes_Insert", p, commandType: CommandType.StoredProcedure);
            }
        }

        private static void SaveTournament(TournamentModel model, IDbConnection conn)
        {
            var p = new DynamicParameters();
            p.Add("@TournamentName", model.TournamentName);
            p.Add("@EntryFee", model.EntryFee);
            p.Add("@id", 0, DbType.Int32, direction: ParameterDirection.Output);

            conn.Execute("spTournaments_Insert", p, commandType: CommandType.StoredProcedure);

            model.Id = p.Get<int>("@id");
        }

        public List<PersonModel> GetPerson_All()
        {
            List<PersonModel> output;
            using (IDbConnection conn = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString("Tournaments")))
            {
                output = conn.Query<PersonModel>("dbo.spPeople_GetAll").ToList();
            }
            return output;
        }

        public List<TeamModel> GetTeam_All()
        {
            List<TeamModel> output;
            using (IDbConnection conn = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString("Tournaments")))
            {
                output = conn.Query<TeamModel>("dbo.spTeam_GetAll").ToList();

                foreach (var t in output)
                {
                    var p = new DynamicParameters();
                    p.Add("@TeamId", t.Id);

                    t.TeamMembers = conn.Query<PersonModel>("spTeamMembers_GetByTeam", p, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            return output;
        }

        public List<TournamentModel> GetTournament_All()
        {
            List<TournamentModel> output;
            using (IDbConnection conn = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString("Tournaments")))
            {
                var p = new DynamicParameters();
                output = conn.Query<TournamentModel>("dbo.spTournaments_GetAll").ToList();
                foreach (TournamentModel t in output)
                {
                    p = new DynamicParameters();
                    p.Add("@TournamentId", t.Id);
                    t.Prizes = conn.Query<PrizeModel>("spPrizes_GetByTournament", p, commandType: CommandType.StoredProcedure).ToList();

                    //populate teams                    
                    t.EnteredTeams = conn.Query<TeamModel>("dbo.spTeams_GetByTournament", p, commandType: CommandType.StoredProcedure).ToList();
                    foreach (var team in t.EnteredTeams)
                    {
                        p = new DynamicParameters();
                        p.Add("@TeamId", t.Id);

                        team.TeamMembers = conn.Query<PersonModel>("spTeamMembers_GetByTeam", p, commandType: CommandType.StoredProcedure).ToList();
                    }

                    //populate rounds
                    p = new DynamicParameters();
                    p.Add("@TournamentId", t.Id);

                    List<MatchupModel> matchups = conn.Query<MatchupModel>("spMatchups_GetByTournament", p, commandType: CommandType.StoredProcedure).ToList();

                    foreach (var m in matchups)
                    {
                        p = new DynamicParameters();
                        p.Add("@matchupId", t.Id);
                        
                        m.Entries = conn.Query<MatchupEntryModel>("spMatchupEntries_GetByMatchup", p, commandType: CommandType.StoredProcedure).ToList();

                        List<TeamModel> allTeams = GetTeam_All();
                        //populate each entry

                        if (m.WinnerId > 0)
                        {
                            m.Winner = allTeams.Where(x => x.Id == m.WinnerId).First();
                        }
                        foreach (var me in m.Entries)
                        {
                            if (me.TeamCompetingId > 0)
                            {
                                me.TeamCompeting = allTeams.Where(x => x.Id == me.TeamCompetingId).First();
                            }

                            if (me.ParentMatchupId > 0)
                            {
                                me.ParentMatchup = matchups.Where(x => x.Id == me.ParentMatchupId).First();
                            }
                        }
                    }
                    //List<list<matchupmodel>>
                    List<MatchupModel> currRow = new List<MatchupModel>();
                    int currRound = 1;
                    foreach (MatchupModel m in matchups)
                    {
                        if (m.MatchupRound > currRound)
                        {
                            t.Rounds.Add(currRow);
                            currRow = new List<MatchupModel>();
                            currRound++;
                        }
                        currRow.Add(m);
                    }
                    t.Rounds.Add(currRow);
                }

            }
            return output;
        }
    }
}
