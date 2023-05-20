using System;

namespace DutyTracker.Enums;

[Flags]
public enum AllianceType : byte
{
    None       = 0,
    ThreeParty = 1,
    SixParty   = 3,
}
