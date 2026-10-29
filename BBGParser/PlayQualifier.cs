namespace BBGParser;

public enum PlayQualifier : byte
{
    Traveling = 0x52,
    DeadBallRebound = 0x59,
    BadPass = 0x4A,
    Fumble = 0x49,
    PointsInTransition = 0x41,
    ThreeSecondViolation = 0x56,
    DoubleDribble = 0x54,
    Palming = 0x53,
    StartOfGame = 0x00,
    None = 0x40,
    ShotClock = 0x55,
    PointsInSetOffense = 0x42,
    FreeLance = 0x43,
    PointsFromPress = 0x44,
    SidelineOutOfBoundsPlay = 0x45,
    BaselineOutOfBoundsPlay = 0x46,
    FullCourtOutOfBoundsPlay = 0x47,
    Violation = 0x48,
    FiveSecondViolation = 0x57,
    TenSecondViolation = 0x58,
    GoalTend = 0x5A,
    Administrative = 0x5C,
    Flagrant = 0x5D,
    CoachTechnical = 0x5E,
    TeamTechnical = 0x5F,
    PartialTimeOut = 0x4D
}