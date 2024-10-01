using System;

namespace DutyTracker.Enums;

public enum Alliance
{
    None,
    A,
    B,
    C,
    D,
    E,
    F,
}

[Flags]
public enum AllianceType : byte
{
    None = 0,
    ThreeParty = 1,
    SixParty = 3,
}