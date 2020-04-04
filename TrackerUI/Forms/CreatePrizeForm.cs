using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary.Models;
using TrackerLibrary;

namespace TrackerUI
{
    public partial class CreatePrizeForm : Form
    {
        public CreatePrizeForm()
        {
            InitializeComponent();
        }

        private void createPrizeBtn_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                PrizeModel model = new PrizeModel(placeNameValue.Text, 
                                                    placeNumberValue.Text, 
                                                    prizeAmountValue.Text, 
                                                    prizePercentageValue.Text);


                GlobalConfig.Connection.CreatePrize(model);


                placeNameValue.Text = "";
                placeNumberValue.Text = "";
                prizeAmountValue.Text = "0";
                prizePercentageValue.Text = "0";

            }
            else
            {
                MessageBox.Show("This form is invalid");
            }
        }

        private bool ValidateForm()
        {
            bool output = true;
            int placeNum = 0;
            if (int.TryParse(placeNumberValue.Text, out placeNum) == false)
            {
                output = false;
            }

            if (placeNum < 1)
            {
                output = false;
            }

            if (placeNameValue.Text.Length == 0)
            {
                output = false;

            }
            decimal prizeAmount = 0;
            double prizePercentage = 0;

            if (!decimal.TryParse(prizeAmountValue.Text, out prizeAmount) || 
                !double.TryParse(prizePercentageValue.Text, out prizePercentage))
            {
                output = false;
            }

            if (prizePercentage <=0 && prizeAmount <= 0)
            {
                output = false;
            }
            if (prizePercentage >= 100)
            {
                output = false;
            }

            return output;
        }
    }
}
