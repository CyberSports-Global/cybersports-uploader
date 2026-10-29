namespace BBGParser;

public class Player
{
    public string Name;
    public string Position;
    public bool IsActive;
    public string Height;
    public int Weight;
    public string Class;
    public string Experience;
    public string SchoolHometown;
    public string HomeNumber;
    public string AwayNumber;
    public string Remarks;
    public int Id;
    public int Bytes;
    public Stats Stats;

    private Player()
    {
        Name = string.Empty;
        Position = string.Empty;
        IsActive = false;
        Height = string.Empty;
        Weight = 0;
        Class = string.Empty;
        Experience = string.Empty;
        SchoolHometown = string.Empty;
        HomeNumber = string.Empty;
        AwayNumber = string.Empty;
        Remarks = string.Empty;
        Id = int.MinValue;
        Bytes = 0;
        Stats = new Stats();
    }

    internal static Player FromStream(BinaryReader reader)
    {
        var byteCount = 0;
        
        var player = new Player();
        //Name
        var readCount = reader.ReadByte();
        byteCount += 1;
        byteCount += readCount;
        player.Name = new string(reader.ReadChars(readCount));
        //Position
        readCount = reader.ReadByte();
        byteCount += 1;
        byteCount += readCount;
        player.Position = new string(reader.ReadChars(readCount));
        //Height
        readCount = reader.ReadByte();
        byteCount += 1;
        byteCount += readCount;
        player.Height = new string(reader.ReadChars(readCount));
        //Class
        readCount = reader.ReadByte();
        byteCount += 1;
        byteCount += readCount;
        player.Class = new string(reader.ReadChars(readCount));
        //Experience
        readCount = reader.ReadByte();
        byteCount += 1;
        byteCount += readCount;
        player.Experience = new string(reader.ReadChars(readCount));
        //SchoolHometown
        readCount = reader.ReadByte();
        byteCount += 1;
        byteCount += readCount;
        player.SchoolHometown = new string(reader.ReadChars(readCount));
        //Unknown
        byteCount += 12;
        _ = reader.ReadBytes(12);
        //Weight
        byteCount += 4;
        player.Weight = reader.ReadInt32();
        //Home Number (INT)
        byteCount += 4;
        var homeNumberInt = reader.ReadInt32();
        //Away Number (INT)
        byteCount += 4;
        var awayNumberInt = reader.ReadInt32();
        //Is Active
        byteCount += 4;
        player.IsActive = reader.ReadInt32() == 1;
        //Unknown
        byteCount += 38;
        _ = reader.ReadBytes(38);
        //Home Number
        readCount = reader.ReadByte();
        byteCount += 1;
        byteCount += readCount;
        player.HomeNumber = new string(reader.ReadChars(readCount));
        //Away Number
        readCount = reader.ReadByte();
        byteCount += 1;
        byteCount += readCount;
        player.AwayNumber = new string(reader.ReadChars(readCount));
        //Unknown
        byteCount += 1;
        _ = reader.ReadBytes(1);
        //Remarks
        readCount = reader.ReadByte();
        byteCount += 1;
        byteCount += readCount;
        player.Remarks = new string(reader.ReadChars(readCount));
        //Unknown
        byteCount += 1;
        _ = reader.ReadBytes(1);
        //Id
        player.Id = reader.ReadInt32();
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
        player.Stats = Stats.FromStream(reader);
        player.Bytes = byteCount;
        return player;
    }

    public override bool Equals(object? obj)
    {
        if (obj is Player other)
        {
            return Equals(other);
        }

        return false;
    }

    protected bool Equals(Player other)
    {
        return Name == other.Name && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Id);
    }
}