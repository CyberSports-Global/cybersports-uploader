using BBGParser;
using System.Collections.Generic;
using System.Numerics;

namespace BBGParserCLI
{
    internal class Program
    {
        private const string DIRECTORY_PATH = "C:\\Users\\james\\Dropbox\\Stats\\2024-2025\\Basketball\\WIAA\\Yakima\\Boys_Testing";
        static void Main(string[] args)
        {
            var gameFiles = Directory.GetFiles(DIRECTORY_PATH, "*.bbg", SearchOption.AllDirectories);
            //var gameFiles = new[] { @"C:\Users\james\Dropbox\Stats\Training\AllActionsTest.BBG" };
            var games = new List<Game>();
            foreach (var gameFile in gameFiles)
            {
                try
                {
                    games.Add(Game.FromFile(gameFile));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{gameFile} - {e.Message}");
                }
            }
            CheckGames(games);
        }

        private static void CheckGames(IEnumerable<Game> games)
        {
            foreach (var game in games.OrderBy(g => g.Id))
            {
                Console.WriteLine($"{game.Id} - {game.Away.SchoolCity} {game.Away.TeamName} @ {game.Home.SchoolCity} {game.Home.TeamName} - {game.FilePath.Replace(DIRECTORY_PATH,"")}");
            }
        }

        private static void CheckTeams(IEnumerable<Game> games)
        {
            var teamIdsByName = new Dictionary<string,List<int>>();
            var playersByTeamName = new Dictionary<string,List<Player>>();
            
            foreach (var game in games)
            {
                var homeName = $"{game.Home.SchoolCity} {game.Home.TeamName}";
                var awayName = $"{game.Home.SchoolCity} {game.Home.TeamName}";

                if (!teamIdsByName.ContainsKey(homeName))
                {
                    teamIdsByName.Add(homeName, []);
                }

                if (!teamIdsByName[homeName].Contains(game.Home.Id))
                {
                    teamIdsByName[homeName].Add(game.Home.Id);
                }

                if (!playersByTeamName.ContainsKey(homeName))
                {
                    playersByTeamName.Add(homeName, []);
                }

                foreach (var player in game.Home.Players.Where(player => !playersByTeamName[homeName].Contains(player)))
                {
                    playersByTeamName[homeName].Add(player);
                }

                if (!teamIdsByName.ContainsKey(awayName))
                {
                    teamIdsByName.Add(awayName, []);
                }

                if (!teamIdsByName[awayName].Contains(game.Away.Id))
                {
                    teamIdsByName[awayName].Add(game.Away.Id);
                }

                if (!playersByTeamName.ContainsKey(awayName))
                {
                    playersByTeamName.Add(awayName, []);
                }

                foreach (var player in game.Away.Players.Where(player => !playersByTeamName[awayName].Contains(player)))
                {
                    playersByTeamName[awayName].Add(player);
                }
            }

            Console.WriteLine("Teams by Name:");
            foreach (var keyValuePair in teamIdsByName.OrderBy(e => e.Key))
            {
                var idList = keyValuePair.Value.Aggregate(string.Empty, (current, id) => current + $"{id}, ");
                idList = idList[..^2];
                Console.WriteLine($"{keyValuePair.Key}: {idList}");

                Console.WriteLine("Players");
                foreach (var player in playersByTeamName[keyValuePair.Key].OrderBy(p => p.Name))
                {
                    Console.WriteLine($"{player.Name} ({player.HomeNumber}, {player.AwayNumber}) - {player.Id}");
                }
            }

            var players = new Dictionary<Player, List<string>>();

            Console.WriteLine("Players by Team");
            foreach (var entry in playersByTeamName)
            {
                foreach (var player in entry.Value)
                {
                    if (!players.ContainsKey(player))
                    {
                        players.Add(player, []);
                    }
                    players[player].Add(entry.Key);
                }
            }

            foreach (var entry in players)
            {
                var idList = entry.Value.Aggregate(string.Empty, (current, id) => current + $"{id}, ");
                idList = idList[..^2];
                Console.WriteLine($"{entry.Key.Name} - {entry.Key.Id}: {idList}");
            }
        }
    }
}
