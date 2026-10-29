using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace BBGParser
{
    public class Game
    {
        public int Id;
        public byte[] Unknown01; //12
        public string Date;
        public string Time;
        public string Location;
        public string Notes;
        public string Refs;
        public byte[] Unknown02; //6
        public GameType Type;
        public byte[] Unknown03; //5
        public string HomeDisplay;
        public string AwayDisplay;
        public DisplayType Display;
        public byte[] Unknown04; //97
        public string ReportTitle;
        public string ProgramTitle;
        public string ProgramPostscript;
        public string Attendance;
        public bool NeutralSite;
        public string WebcastRemarks;
        public int Bytes;
        public string FilePath;

        public Team Home;
        public Team Away;
        public List<Play> Plays;

        private Game()
        {
            Id = int.MinValue;
            Unknown01 = new byte[12];
            Date = string.Empty;
            Time = string.Empty;
            Location = string.Empty;
            Notes = string.Empty;
            Refs = string.Empty;
            Unknown02 = new byte[6];
            Type = GameType.League;
            Unknown03 = new byte[5];
            HomeDisplay = string.Empty;
            AwayDisplay = string.Empty;
            Display = DisplayType.FullName;
            Unknown04 = new byte[97];
            ReportTitle = string.Empty;
            ProgramTitle = string.Empty;
            ProgramPostscript = string.Empty;
            Attendance = string.Empty;
            NeutralSite = false;
            WebcastRemarks = string.Empty;
            Bytes = -1;
            Home = null!;
            Away = null!;
            Plays = [];
            FilePath = string.Empty;
        }

        public static Game FromFile(string fileName)
        {
            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            
            var reader = new BinaryReader(stream);
            var players = new List<Player>();
            int playCount = -1;
            var plays = new List<Play>();
            Team homeTeam = null;
            Team awayTeam = null;
            bool firstPlayer = true;
            Game game = null;
            bool shouldLoop = true;
            while (shouldLoop)
            {
                byte[] startBytes = null;
                if (firstPlayer)
                {
                    _ = reader.ReadBytes(16); //Discard first 16 bytes;
                    firstPlayer = false;
                    startBytes = [0x01,0x80];
                    reader.BaseStream.Seek(-2, SeekOrigin.Current);
                }
                else
                {
                    startBytes = reader.ReadBytes(2);
                    reader.BaseStream.Seek(-2, SeekOrigin.Current);
                }

                
                if (startBytes[0] == 0x01 && startBytes[1] == 0x80)
                {
                    reader.BaseStream.Seek(2, SeekOrigin.Current);
                    players.Add(Player.FromStream(reader));
                }
                else
                {
                    if (homeTeam == null)
                    {
                        homeTeam = Team.FromStream(reader);
                        homeTeam.Players.AddRange(players);
                        players.Clear();
                        reader.BaseStream.Seek(2, SeekOrigin.Current);
                    }
                    else if (awayTeam == null)
                    {
                        awayTeam = Team.FromStream(reader);
                        awayTeam.Players.AddRange(players);
                        players.Clear();
                    }
                    else if (playCount == -1)
                    {
                        playCount = reader.ReadUInt16();
                        while (plays.Count < playCount)
                        {
                            plays.Add(Play.FromStream(reader));
                        }
                    }
                    else
                    {
                        game = Game.FromStream(reader);
                        game.FilePath = fileName;
                        game.Home = homeTeam;
                        game.Away = awayTeam;
                        game.Plays.AddRange(plays);
                        shouldLoop = false;
                    }
                }
            }
            reader.Close();
            stream.Close();
            return game!;
        }

        internal static Game FromStream(BinaryReader reader)
        {
            var byteCount = 0;
            var readCount = 0;

            var game = new Game
            {
                Id = reader.ReadInt32(),
                Unknown01 = reader.ReadBytes(12)
            };
            //Date
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            game.Date = new string(reader.ReadChars(readCount));
            //Time
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            game.Time = new string(reader.ReadChars(readCount));
            //Location
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            game.Location = new string(reader.ReadChars(readCount));
            //Notes
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            game.Notes = new string(reader.ReadChars(readCount));
            //Refs
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            game.Refs = new string(reader.ReadChars(readCount));

            game.Unknown02 = reader.ReadBytes(6);
            game.Type = (GameType)reader.ReadByte();
            game.Unknown03 = reader.ReadBytes(5);

            //HomeDisplay
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            game.HomeDisplay = new string(reader.ReadChars(readCount));
            //AwayDisplay
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            game.AwayDisplay = new string(reader.ReadChars(readCount));

            game.Display = (DisplayType)reader.ReadByte();
            game.Unknown04 = reader.ReadBytes(97);

            //ReportTitle/ProgramTitle
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            var temp = new string(reader.ReadChars(readCount));
            var parseIndex = temp.LastIndexOf("==");
            game.ReportTitle = temp[..parseIndex];
            game.ProgramTitle = temp[(parseIndex+2)..];

            //ProgramPostscript
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            game.ProgramPostscript = new string(reader.ReadChars(readCount));
            //Attendance
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            game.Attendance = new string(reader.ReadChars(readCount));
            if (game.Attendance.StartsWith('N'))
            {
                game.NeutralSite = true;
                game.Attendance = game.Attendance[1..];
            }
            else
            {
                game.NeutralSite = false;
            }
            //WebcastRemarks
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            game.WebcastRemarks = new string(reader.ReadChars(readCount));

            game.Bytes = byteCount;
            return game;
        }
    }
}
