using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class CreateTournamentForm : Form, IPrizeRequester, ITeamRequester
    {
        List<TeamModel> availableTeams = GlobalConfig.Connection.GetTeam_All();
        List<TeamModel> selectedTeams = new List<TeamModel>();
        List<PrizeModel> availablePrizes = new List<PrizeModel>();

        public CreateTournamentForm()
        {
            InitializeComponent();
            WireUpList();
        }

        private void WireUpList()
        {
            selectTeamDropDown.DataSource = null;
            selectTeamDropDown.DataSource = availableTeams;
            selectTeamDropDown.DisplayMember = "TeamName";

            tournamentTeamsListBox.DataSource = null;
            tournamentTeamsListBox.DataSource = selectedTeams;
            tournamentTeamsListBox.DisplayMember = "TeamName";

            prizesListBox.DataSource = null;
            prizesListBox.DataSource = availablePrizes;
            prizesListBox.DisplayMember = "PlaceName";
        }

        private void teamOneScoreValue_TextChanged(object sender, EventArgs e)
        {

        }

        private void CreateTournamentButton_Click(object sender, EventArgs e)
        {
            decimal fee = 0;
            bool acceptablefee = decimal.TryParse(entryFeeValue.Text, out fee);

            if (!acceptablefee)
            {
                MessageBox.Show("You need to entre a valid Entry Fee",
                    "Invalid Fee", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            TournamentModel tm = new TournamentModel();
            tm.TournamentName = tournamentNameValue.Text;
            tm.EntryFee = fee;
            tm.Prizes = availablePrizes;
            tm.TeamEntered = selectedTeams;
            GlobalConfig.Connection.CreateTournament(tm);

        }

        private void addTeamButton_Click(object sender, EventArgs e)
        {
            TeamModel t = (TeamModel)selectTeamDropDown.SelectedItem;
            if (t != null)
            {
                availableTeams.Remove(t);
                selectedTeams.Add(t);
                WireUpList();
            }

        }

        private void removeSelectedTeamButton_Click(object sender, EventArgs e)
        {
            TeamModel t = (TeamModel)tournamentTeamsListBox.SelectedItem;
            if (t != null)
            {
                availableTeams.Add(t);
                selectedTeams.Remove(t);
                WireUpList();
            }
        }

        private void createPrizeButton_Click(object sender, EventArgs e)
        {
            // call createprizeform
            CreatePrizeForm frm = new CreatePrizeForm(this);
            frm.Show();                     

            
        }

        public void PrizeComplete(PrizeModel model)
        {
            // get the prize model from form
            availablePrizes.Add(model);
            WireUpList();
        }

        public void TeamComplete(TeamModel model)
        {
            selectedTeams.Add(model);
            WireUpList();
        }

        private void addNewTeamLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            CreateTeamForm frm = new CreateTeamForm(this);
            frm.Show();
        }

        private void deleteSelectedPrizeButton_Click(object sender, EventArgs e)
        {
            PrizeModel p = (PrizeModel)prizesListBox.SelectedItem;
            if (p != null)
            {
                availablePrizes.Remove(p);
                WireUpList();
            }
        }
    }
}
