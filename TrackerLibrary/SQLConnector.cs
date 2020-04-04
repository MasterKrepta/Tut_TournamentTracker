using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
    public class SQLConnector : IDataConnection
    {
        //TODO make the CreatePrize Save to DB
        /// <summary>
        /// Saves a new prize to the DB
        /// </summary>
        /// <param name="model"></param>
        /// <returns>The prize info, including the unique identifier</returns>
        public PrizeModel CreatePrize(PrizeModel model)
        {
            model.Id = 1;

            return model;
        }
    }
}
