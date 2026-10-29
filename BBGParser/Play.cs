namespace BBGParser
{
    public class Play
    {
        public PlayType Type;
        public int PlayerId;
        public short SecondsIntoPeriod;
        public byte Period;
        public sbyte PositionX;
        public sbyte PositionY;
        public PlayQualifier Qualifier;
        public byte Unknown;
        public DateTime ModifiedDateTime;

        internal static Play FromStream(BinaryReader reader)
        {
            var play = new Play
            {
                Type = (PlayType)reader.ReadInt16(),
                PlayerId = reader.ReadInt32()
            };
            var timeBytes = reader.ReadBytes(2);
            
            var periodByte = (byte)((timeBytes[1] & 0xF0) >> 4);
            var secondsBytes = timeBytes.ToArray();
            secondsBytes[1] = (byte)(secondsBytes[1] & 0x0F);
            play.SecondsIntoPeriod = BitConverter.ToInt16(secondsBytes);
            play.Period = periodByte;
            play.PositionX = reader.ReadSByte();
            play.PositionY = reader.ReadSByte();
            play.Qualifier = (PlayQualifier)reader.ReadByte();
            play.Unknown = reader.ReadByte();
            var unixTime = DateTimeOffset.FromUnixTimeSeconds(reader.ReadInt32());
            play.ModifiedDateTime = unixTime.DateTime.ToLocalTime();
            return play;
        }
    }
}
