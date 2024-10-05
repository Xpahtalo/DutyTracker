using DutyTracker.Enums;

namespace DutyTracker.Services.PlayerCharacter;

internal class CachedPartyMember(string name, uint hp, Alliance alliance)
{
    public readonly string Name = name;
    public uint Hp = hp;
    public readonly Alliance Alliance = alliance;

    public override string ToString() => $"Name: {Name}, HP: {Hp}, Alliance: {Alliance}";
}