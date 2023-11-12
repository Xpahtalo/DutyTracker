using DutyTracker.Enums;

namespace DutyTracker.Services.PlayerCharacter;

internal class CachedPartyMember
{
    public string   Name     { get; }
    public uint     ObjectId { get; }
    public uint     Hp       { get; set; }
    public Alliance Alliance { get; }

    public CachedPartyMember(string name, uint objectId, uint hp, Alliance alliance)
    {
        Name     = name;
        ObjectId = objectId;
        Hp       = hp;
        Alliance = alliance;
    }

    public override string ToString() => $"Name: {Name}, ObjectId: {ObjectId}, HP: {Hp}, Alliance: {Alliance}";
}
