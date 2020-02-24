using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess.TextHelpers
{
    public static class TextConnectorProcessor
    {
        public static string FullFilePath(this string fileName ) // file path + file name
        {
            return $"{ConfigurationManager.AppSettings["filePath"] }\\{fileName }";
        }
        
        public static List<string> LoadFile(this string file)
        {
            if(!File.Exists(file))
            {
                return new List<string>();
            }

            return File.ReadAllLines(file).ToList();
        }
        // FOR PRIZE MODELS
        public static List<PrizeModel> ConvertToPrizeModel(this List<string> lines)
        {
            List<PrizeModel> output = new List<PrizeModel>();
            foreach (string line in lines)
            {
                string[] cols = line.Split(',');
                PrizeModel p = new PrizeModel();
                p.Id = int.Parse(cols[0]);
                p.PlaceNumber = int.Parse(cols[1]);
                p.PlaceName = cols[2];
                p.PrizeAmount = decimal.Parse(cols[3]);
                p.PrizePercentage = double.Parse(cols[4]);
                output.Add(p);
            }
            return output; 
        }
        public static void SaveToPrizeFile (this List<PrizeModel> models, string fileName)
        {
            List<string> lines = new List<string>();
            foreach (PrizeModel p in models)
            {
                lines.Add($"{ p.Id },{ p.PlaceNumber },{ p.PlaceName },{ p.PrizeAmount },{ p.PrizePercentage }");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines); 
        }
        // FOR PERSON MODELS
        public static List<PersonModel> ConvertToPersonModel(this List<string> lines)
        {
            List<PersonModel> output = new List<PersonModel>();
            foreach (string line in lines)
            {
                string[] cols = line.Split(',');
                PersonModel p = new PersonModel();
                p.Id = int.Parse(cols[0]);
                p.FirstName = cols[1];
                p.LastName = cols[2];
                p.EmailAddress = cols[3];
                p.Phone = cols[4];
                output.Add(p);
            }
            return output;
        }

        public static List<TeamModel> ConvertToTeamModels(
            this List<string> lines, string peopleFileName)
        {
            List<TeamModel> output = new List<TeamModel>();
            List<PersonModel> people = peopleFileName.FullFilePath().LoadFile().ConvertToPersonModel();
            
            foreach (var line in lines)
            {
                string[] cols = line.Split(',');

                TeamModel t = new TeamModel();
                t.Id = int.Parse(cols[0]);
                t.TeamName = cols[1];

                string[] personIds = cols[2].Split('|');

                foreach (string id in personIds)
                {
                    t.TeamMembers.Add(people.Where(x => x.Id == int.Parse(id)).First());
                }
                output.Add(t);

            }

            return output;
        }

        public static List<TournamentModel> ConvertToTournamentModels(this List<string> lines, 
            string teamFileName, string peopleFileName, string prizeFileName)
        { 
            //id,tournamentName,tournamentFee, idt|idt|idt|idt, idp|idp|idp, idm^idm^idm|idm^idm^idm|idm^idm^idm
            List<TournamentModel> output = new List<TournamentModel>();
            List<TeamModel> teams = teamFileName.FullFilePath().LoadFile().ConvertToTeamModels(peopleFileName);
            List<PrizeModel> prizes = prizeFileName.FullFilePath().LoadFile().ConvertToPrizeModel();
            foreach (string line in lines)
            {
                string[] cols = line.Split(',');
                TournamentModel tm = new TournamentModel();
                tm.Id = int.Parse(cols[0]);
                tm.TournamentName = cols[1];
                tm.EntryFee = decimal.Parse(cols[2]);

                string[] teamsIds = cols[3].Split('|');
                foreach (string id in teamsIds)
                {
                    tm.TeamEntered.Add(teams.Where(x=> x.Id == int.Parse(id)).First());
                }

                string[] prizesIds = cols[4].Split('|');
                foreach (string id in prizesIds)
                {
                    tm.Prizes.Add(prizes.Where(x=> x.Id == int.Parse(id)).First());
                }

                //todo - rounds
                output.Add(tm);
            }
            return output;
        }
        public static void SaveToPeopleFile(this List<PersonModel> models, string fileName)
        {
            List<string> lines = new List<string>();
            foreach (PersonModel p in models)
            {
                lines.Add($"{ p.Id },{ p.FirstName },{ p.LastName },{ p.EmailAddress },{ p.Phone }");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines);
        }
        public static void SaveToTeamFile(this List<TeamModel> models, string fileName)
        {
            List<string> lines = new List<string>();

            foreach (TeamModel t in models)
            {
                lines.Add($"{ t.Id },{ t.TeamName },{ ConvertPeopleListToString(t.TeamMembers) }");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines);
        }
        public static void SaveToTournamentFile(this List<TournamentModel> models, string fileName)
        {
            List<string> lines = new List<string>();
            foreach (TournamentModel tm in models)
            {
                lines.Add($@"{ tm.Id },
                { tm.TournamentName },
                { tm.EntryFee },
                { ConvertTeamListToString(tm.TeamEntered) },
                { ConvertPrizeListToString(tm.Prizes) },
                { ConvertRoundListToString(tm.Rounds) }");
            }
            File.WriteAllLines(fileName.FullFilePath(), lines);
        }
        private static string ConvertTeamListToString(List<TeamModel> team)
        {
            string output = "";

            if (team.Count == 0)
            {
                return "";
            }
            foreach (TeamModel t in team)
            {
                output += $"{ t.Id }|";
            }
            output = output.Substring(0, output.Length - 1);
            return output;
        }

        private static string ConvertRoundListToString(List<List<MatchupModel>> rounds)
        {
           //idm^idm^idm|idm^idm^idm|idm^idm^idm
            string output = "";

            if (rounds.Count == 0)
            {
                return "";
            }
            foreach (List<MatchupModel> r in rounds)
            {
                output += $"{ ConvertMatchUpListToString(r) }|";
            }
            output = output.Substring(0, output.Length - 1);
            return output;
        }

        private static string ConvertMatchUpListToString(List<MatchupModel> matchUp)
        {
            string output = "";

            if (matchUp.Count == 0)
            {
                return "";
            }
            foreach (MatchupModel m in matchUp)
            {
                output += $"{ m.Id }^";
            }
            output = output.Substring(0, output.Length - 1);
            return output;

        }


        private static string ConvertPrizeListToString(List<PrizeModel> prize)
        {
            string output = "";

            if (prize.Count == 0)
            {
                return "";
            }
            foreach (PrizeModel p in prize)
            {
                output += $"{ p.Id }|";
            }
            output = output.Substring(0, output.Length - 1);
            return output;

        }

        private static string ConvertPeopleListToString(List<PersonModel> people)
        {
            string output = "";

            if (people.Count==0)
            {
                return "";
            }

            foreach (PersonModel p in people)
            {
                output += $"{ p.Id }|";
            }
            output = output.Substring(0, output.Length - 1);

            return output;
        }
    }
}
 