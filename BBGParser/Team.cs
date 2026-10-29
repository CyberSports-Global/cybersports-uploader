using System.Drawing;

namespace BBGParser
{
    public class Team
    {
        public string HeadCoach;
        public string TeamName;
        public string NameAbbreviation;
        public string SchoolCity;
        public string AssistantCoaches;
        public string AthleticDirector;
        public string AthleticDirectorRosterTitle;
        public string SchoolRosterTitle;
        public string ClassRosterTitle;
        public string ExperienceRosterTitle;
        public string LeagueRecord;
        public string OverallRecord;
        public string Comments;
        public string ProgramPostscript;
        public string League;
        public int Id;
        public int Bytes;
        public Stats Stats;
        public List<Player> Players;
        public bool IsMens;
        public bool ShowClassInRoster;
        public bool ShowExperienceInRoster;
        public bool ShowSchoolInRoster;
        public bool ShowAthleticDirectorInRoster;
        public bool ShowMajorInRoster;
        public Color BackgroundColor;
        public Color TextColor;

        private const short WOMEN_TEAM = 0b0001000000000000;
        private const short CLASS_IN_ROSTER = 0b0000000100000000;
        private const short EXPERIENCE_IN_ROSTER = 0b0000000000000001;
        private const short SCHOOL_IN_ROSTER = 0b0000001000000000;
        private const short ATHLETIC_DIRECTOR_IN_ROSTER = 0b0000100000000000;
        private const short MAJOR_IN_ROSTER = 0b0000010000000000;


        private Team()
        {
            HeadCoach = string.Empty;
            TeamName = string.Empty;
            NameAbbreviation = string.Empty;
            SchoolCity = string.Empty;
            AssistantCoaches = string.Empty;
            AthleticDirector = string.Empty;
            AthleticDirectorRosterTitle = string.Empty;
            SchoolRosterTitle = string.Empty;
            ClassRosterTitle = string.Empty;
            ExperienceRosterTitle = string.Empty;
            LeagueRecord = string.Empty;
            OverallRecord = string.Empty;
            Comments = string.Empty;
            ProgramPostscript = string.Empty;
            League = string.Empty;
            Id = int.MinValue;
            Bytes = 0;
            Stats = new Stats();
            Players = new List<Player>();
        }

        internal static Team FromStream(BinaryReader reader)
        {
            var byteCount = 0;
            var team = new Team();

            //HeadCoach
            var readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            team.HeadCoach = new string(reader.ReadChars(readCount));
            //TeamName
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            team.TeamName = new string(reader.ReadChars(readCount));
            //NameAbbreviation
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            team.NameAbbreviation = new string(reader.ReadChars(readCount));
            //SchoolCity
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            team.SchoolCity = new string(reader.ReadChars(readCount));
            //AssistantCoaches
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            team.AssistantCoaches = new string(reader.ReadChars(readCount));
            //Unknown
            byteCount += 1;
            _ = reader.ReadBytes(1);
            //AthleticDirector
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            team.AthleticDirector = new string(reader.ReadChars(readCount));
            //AthleticDirectorRosterTitle
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            team.AthleticDirectorRosterTitle = new string(reader.ReadChars(readCount));
            //SchoolRosterTitle
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            team.SchoolRosterTitle = new string(reader.ReadChars(readCount));
            //Unknown
            byteCount += 2;
            var flags = reader.ReadInt16();
            team.IsMens = (flags & WOMEN_TEAM) != WOMEN_TEAM;
            team.ShowAthleticDirectorInRoster = (flags & ATHLETIC_DIRECTOR_IN_ROSTER) != ATHLETIC_DIRECTOR_IN_ROSTER;
            team.ShowClassInRoster = (flags & CLASS_IN_ROSTER) != CLASS_IN_ROSTER;
            team.ShowExperienceInRoster = (flags & EXPERIENCE_IN_ROSTER) != EXPERIENCE_IN_ROSTER;
            team.ShowMajorInRoster = (flags & MAJOR_IN_ROSTER) != MAJOR_IN_ROSTER;
            team.ShowSchoolInRoster = (flags & SCHOOL_IN_ROSTER) != SCHOOL_IN_ROSTER;
            //ClassRosterTitle
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            team.ClassRosterTitle = new string(reader.ReadChars(readCount));
            //ExperienceRosterTitle
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            team.ExperienceRosterTitle = new string(reader.ReadChars(readCount));
            //LeagueRecord
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            team.LeagueRecord = new string(reader.ReadChars(readCount));
            //OverallRecord
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            team.OverallRecord = new string(reader.ReadChars(readCount));
            //Unknown
            byteCount += 39;
            _ = reader.ReadBytes(39);
            //Background Color
            byteCount += 3;
            var backgroundRed = reader.ReadByte();
            var backgroundGreen = reader.ReadByte();
            var backgroundBlue = reader.ReadByte();
            team.BackgroundColor = Color.FromArgb(backgroundRed, backgroundGreen, backgroundBlue);
            //Text Color
            byteCount += 3;
            var textRed = reader.ReadByte();
            var textGreen = reader.ReadByte();
            var textBlue = reader.ReadByte();
            team.BackgroundColor = Color.FromArgb(textRed, textGreen, textBlue);
            //Unknown
            byteCount += 2;
            _ = reader.ReadBytes(2);
            //Comments
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            team.Comments = new string(reader.ReadChars(readCount));
            //ProgramPostscript
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            team.ProgramPostscript = new string(reader.ReadChars(readCount));
            //League
            readCount = reader.ReadByte();
            byteCount += 1;
            byteCount += readCount;
            team.League = new string(reader.ReadChars(readCount));
            //Id
            team.Id = reader.ReadInt32();
            byteCount += 4;

            //var endBytes = reader.ReadBytes(2);
            //reader.BaseStream.Seek(-2, SeekOrigin.Current);
            //if (endBytes[0] != 0xFF && endBytes[1] != 0xFF)
            //{
            //    throw new Exception("Should have been the end of the player record");
            //}
            byteCount += 2;
            _ = reader.ReadBytes(2);
            byteCount += 266;
            team.Stats = Stats.FromStream(reader);
            team.Bytes = byteCount;

            return team;
        }
    }
}
