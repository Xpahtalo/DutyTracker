using System.Runtime.InteropServices;
using DutyTracker.Enums;

namespace DutyTracker.Extensions;

public static class AllianceExtensions
{
    public static unsafe Alliance ToAlliance(byte* allianceString) =>
        Marshal.PtrToStringUTF8((nint) allianceString) switch
        {
            "Alliance A" => Alliance.A,
            "Alliance B" => Alliance.B,
            "Alliance C" => Alliance.C,
            "Alliance D" => Alliance.D,
            "Alliance E" => Alliance.E,
            "Alliance F" => Alliance.F,
            _ => Alliance.None,
        };
}