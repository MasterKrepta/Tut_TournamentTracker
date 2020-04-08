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
                p.Add("@TeamId", model.Id);
                p.Add("@PersonId", pz.Id);
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
    }
}
