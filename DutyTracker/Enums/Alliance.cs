using System;
using System.Runtime.InteropServices;

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

public static class AllianceExtensions
{
    public static Alliance ToAlliance(this string allianceString) =>
        allianceString switch
        {
            "Alliance A" => Alliance.A,
            "Alliance B" => Alliance.B,
            "Alliance C" => Alliance.C,
            "Alliance D" => Alliance.D,
            "Alliance E" => Alliance.E,
            "Alliance F" => Alliance.F,
            _            => Alliance.None,
        };

    public static unsafe Alliance ToAlliance(byte* allianceString) =>
        Marshal.PtrToStringUTF8(new IntPtr(allianceString)) switch
        {
            "Alliance A" => Alliance.A,
            "Alliance B" => Alliance.B,
            "Alliance C" => Alliance.C,
            "Alliance D" => Alliance.D,
            "Alliance E" => Alliance.E,
            "Alliance F" => Alliance.F,
            _            => Alliance.None,
        };
}
