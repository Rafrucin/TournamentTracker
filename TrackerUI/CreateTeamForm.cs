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
    public partial class CreateTeamForm : Form
    {
        private List<PersonModel> availableTeamMembers = GlobalConfig.Connection.GetPerson_All();
        private List<PersonModel> selectedTeamMembers = new List<PersonModel>();
        private ITeamRequester callingForm;

        public CreateTeamForm(ITeamRequester caller)
        {
            InitializeComponent();

            callingForm = caller;

            //CreateSampleData();

            WireUpList();
        }


        private void CreateSampleData()
        {
            availableTeamMembers.Add(new PersonModel { FirstName = "Tim", LastName = "Corey" });
            availableTeamMembers.Add(new PersonModel { FirstName = "Raf", LastName = "Ruci" });
            availableTeamMembers.Add(new PersonModel { FirstName = "Luna", LastName = "Dog" });

            selectedTeamMembers.Add(new PersonModel { FirstName = "Milka", LastName = "Cat" });
            selectedTeamMembers.Add(new PersonModel { FirstName = "Woj", LastName = "Ruci" });
        }

        private void WireUpList()
        {
            selectTeamMemberDropDown.DataSource = null;

            selectTeamMemberDropDown.DataSource = availableTeamMembers;
            selectTeamMemberDropDown.DisplayMember = "FullName";

            teamMemebersListBox.DataSource = null;
            teamMemebersListBox.DataSource = selectedTeamMembers;
            teamMemebersListBox.DisplayMember = "FullName";
        }

        private void createMemberButton_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
            {
                MessageBox.Show("Please fill all fields");
            }
            else
            {
                PersonModel p = new PersonModel();
                p.FirstName = firstNameValue.Text;
                p.LastName = lastNameValue.Text;
                p.EmailAddress = emailValue.Text;
                p.Phone = mobileValue.Text;

                p = GlobalConfig.Connection.CreatePerson(p);

                selectedTeamMembers.Add(p);

                WireUpList();

                firstNameValue.Text = "";
                lastNameValue.Text = "";
                emailValue.Text = "";
                mobileValue.Text = "";
            }
        }

        private bool ValidateForm()
        {
            if (firstNameValue.Text.Length==0)
            {
                return false;
            }
            
            if (lastNameValue.Text.Length==0)
            {
                return false;
            }

            if (emailValue.Text.Length==0)
            {
                return false;
            }

            if (mobileValue.Text.Length==0)
            {
                return false;
            }

            return true;
        }

        private void CreateTeamForm_Load(object sender, EventArgs e)
        {

        }

        private void addTeamMemberButton_Click(object sender, EventArgs e)
        {
            PersonModel p = (PersonModel)selectTeamMemberDropDown.SelectedItem;

            if (p!=null)
            {
                availableTeamMembers.Remove(p);
                selectedTeamMembers.Add(p);

                WireUpList(); 
            }
        }

        private void removeMemberButton_Click(object sender, EventArgs e)
        {
            PersonModel p = (PersonModel)teamMemebersListBox.SelectedItem;

            if (p!=null)
            {
                selectedTeamMembers.Remove(p);
                availableTeamMembers.Add(p);

                WireUpList(); 
            }
        }

        private void createTeamButton_Click(object sender, EventArgs e)
        {
            if (teamNameValue.Text!="" && selectedTeamMembers.Count!=0)
            {
                TeamModel t = new TeamModel();
                t.TeamName = teamNameValue.Text;
                t.TeamMembers = selectedTeamMembers;
                GlobalConfig.Connection.CreateTeam(t);
                callingForm.TeamComplete(t);
                this.Close();
            }

        }
    }
}
