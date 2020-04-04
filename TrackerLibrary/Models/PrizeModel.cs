using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary.Models
{
    public class PrizeModel
    {
        public int Id { get; set; }
        public int PlaceNumber { get; set; }
        public string PlaceName { get; set; }
        public decimal PrizeAmount { get; set; }
        public double PrizePercentage { get; set; }

        public PrizeModel()
        {

        }

        public PrizeModel(string placeName, string placeNumber, string prizeAmount, string prizePercentage)
        {
            PlaceName = placeName;

            int placeNumValue = 0;
            int.TryParse(placeNumber, out placeNumValue);
            PlaceNumber = placeNumValue;

            decimal prizeAmountVal = 0;
            decimal.TryParse(prizeAmount, out prizeAmountVal);
            PrizeAmount = prizeAmountVal;

            double prizePercentageVal = 0;
            double.TryParse(prizePercentage, out prizePercentageVal);
            PrizePercentage = prizePercentageVal;


        }
    }

    
}
