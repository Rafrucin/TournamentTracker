using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
    public static class TournamentLogic
    {
        public static void CreateRounds (TournamentModel model)
        {
            List<TeamModel> randomizedTeams = RandomizeTeamOrder(model.TeamEntered);
            int rounds = FindNuberOfRounds(randomizedTeams.Count);
            int byes = NumberOfByes(rounds, randomizedTeams.Count);
            model.Rounds.Add(CreateFirstRound(byes, randomizedTeams));
            CreateOtherRounds(model, rounds);
            // todo 22:35
            

        }

        public static void UpdateTournamentResults(TournamentModel model)
        {
            int startingRound = model.CheckCurrentRound();
            List<MatchupModel> toScore = new List<MatchupModel>();
            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel rm in round)
                {
                    if (rm.Winner==null && (rm.Entries.Any(x=>x.Score !=0) || rm.Entries.Count==1))
                    {
                        toScore.Add(rm);
                    }
                }
            }

            MarkWinnerInMatchups(toScore);

            AdvanceWinners(toScore, model);

            toScore.ForEach(x => GlobalConfig.Connection.UpdateMatchup(x));
            int endingRound = model.CheckCurrentRound();
            if (endingRound > startingRound)
            {
                //EmailLogic.SendEmail();
            }

        }

        private static void AlertUsersToNewRound (this TournamentModel model)
        {
            int currentRoundNumber = model.CheckCurrentRound();
            List<MatchupModel> currentRound = model.Rounds.Where(x => x.First().MatchupRound == currentRoundNumber).First();

            foreach (MatchupModel matchup in currentRound)
            {
                foreach (MatchupEntryModel me in matchup.Entries)
                {
                    foreach (PersonModel p in me.TeamCompiting.TeamMembers)
                    {
                        AlertUsersToNewRound(p, me.TeamCompiting.TeamName, matchup.Entries.Where(x => x.TeamCompiting != me.TeamCompiting).FirstOrDefault());
                    }
                }
            }
        
        }

        private static void AlertUsersToNewRound(PersonModel p, string teamName, MatchupEntryModel competitor)
        {
            if (p.EmailAddress.Length == 0)
            {
                return;
            }
            string from = "";
            List<string> to= new List<string>();
            string subject = "";

            StringBuilder body = new StringBuilder();
            
            if (competitor != null)
            {
                subject = $"You have a new matchup with { competitor.TeamCompiting.TeamName }";
                
                body.AppendLine("<h1>You have a new matchup</h1>");
                body.Append("<strong>Competitor: </strong>");
                body.Append(competitor.TeamCompiting.TeamName);
                body.AppendLine();
                body.AppendLine();
                body.AppendLine("Have a great time!");
                body.AppendLine("~Tournament Tracker");
            }
            else
            {
                subject = "You have a bye this round";
                
                body.AppendLine("Enjoy your round Off!");
                body.AppendLine("~Tournament Tracker");
            }

            to.Add(p.EmailAddress);



            EmailLogic.SendEmail(from, to, subject, body.ToString());
        }

        private static int CheckCurrentRound(this TournamentModel model)
        {
            int output = 1;
            foreach (List<MatchupModel> round in model.Rounds)
            {
                if (round.All(x => x.Winner != null))
                {
                    output += 1;
                }
            }
            return output;
        }
        private static void AdvanceWinners (List<MatchupModel> model, TournamentModel tournament)
        {
            foreach (MatchupModel m in model)
            {
                foreach (List<MatchupModel> round in tournament.Rounds)
                {
                    foreach (MatchupModel rm in round)
                    {
                        foreach (MatchupEntryModel me in rm.Entries)
                        {
                            if (me.ParentMatchup != null)
                            {
                                if (me.ParentMatchup.Id == m.Id)
                                {
                                    me.TeamCompiting = m.Winner;
                                    GlobalConfig.Connection.UpdateMatchup(rm);
                                }
                            }
                        }
                    }
                }
            }



        }
        private static void MarkWinnerInMatchups(List<MatchupModel> models)
        {
            string GreaterWins = ConfigurationManager.AppSettings["greaterWins"];

            foreach (MatchupModel m in models)
            {
                if (m.Entries.Count==1)
                {
                    m.Winner = m.Entries[0].TeamCompiting;
                    continue;
                }
               
                if (GreaterWins == "0")
                {
                    if (m.Entries[0].Score < m.Entries[1].Score)
                    {
                        m.Winner = m.Entries[0].TeamCompiting;
                    }
                    else if (m.Entries[0].Score > m.Entries[1].Score)
                    {
                        m.Winner = m.Entries[1].TeamCompiting;
                    }
                    else
                    {
                        throw new Exception("We do not allow ties games in this aplication.");
                    }
                }
                else
                {
                    if (m.Entries[0].Score > m.Entries[1].Score)
                    {
                        m.Winner = m.Entries[0].TeamCompiting;
                    }
                    else if (m.Entries[0].Score < m.Entries[1].Score)
                    {
                        m.Winner = m.Entries[1].TeamCompiting;
                    }
                    else
                    {
                        throw new Exception("We do not allow ties games in this aplication.");
                    }
                } 
            }
            //if (teamOneScore > teamTwoScore)
            //{
            //    m.Winner = m.Entries[0].TeamCompiting;
            //}
            //else if (teamOneScore < teamTwoScore)
            //{
            //    m.Winner = m.Entries[1].TeamCompiting;
            //}
            //else
            //{
            //    MessageBox.Show("I do not handle tie games.");
            //}
        }
        private static void CreateOtherRounds(TournamentModel model, int rounds)
        {
            int round = 2;
            List<MatchupModel> previousRound = model.Rounds[0];
            List<MatchupModel> currRound = new List<MatchupModel>();
            MatchupModel currMatchup = new MatchupModel();

            while (round <= rounds)
            {
                foreach (MatchupModel match in previousRound)
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
                round += 1;
            }
        }

        private static List<MatchupModel>CreateFirstRound(int byes, List<TeamModel> teams)
        {
            List<MatchupModel> output = new List<MatchupModel>();
            MatchupModel curr = new MatchupModel();

            foreach (TeamModel team in teams)
            {
                curr.Entries.Add(new MatchupEntryModel { TeamCompiting = team });
                if (byes > 0 || curr.Entries.Count>1)
                {
                    curr.MatchupRound = 1;
                    output.Add(curr);
                    curr = new MatchupModel();
                    if (byes > 0 )
                    {
                        byes -= 1;
                    }
                }

            }
            return output;
        }

        private static int NumberOfByes (int rounds, int numberOfTeams)
        {
            int output = 0;
            int totalTeams = 1;

            for (int i = 1; i <= rounds; i++)
            {
                totalTeams *= 2;
            }

            output = totalTeams - numberOfTeams;
            return output;

        }

        private static int FindNuberOfRounds (int teamCount)
        {
            int output = 1;
            int val = 2;

            while (val<teamCount)
            {
                output += 1;
                val *= 2;
            }

            return output;
        }

        private static List<TeamModel> RandomizeTeamOrder(List<TeamModel> teams)
        {
            return teams.OrderBy(x => Guid.NewGuid()).ToList();
        }


    }
}
