namespace DutyTracker.Enums;

public enum TerritoryIntendedUse : byte
{
    Unknown                 = 0,
    Overworld1              = 1,
    InnRoom                 = 2,
    Dungeon                 = 3,
    VariantDungeon          = 4,
    MordianGaol             = 5,
    NewCharacterCity        = 6,
    AllianceRaid            = 8,
    Overworld2              = 9,
    Trial                   = 10,
    HousingWard             = 13,
    HousingInstance         = 14,
    Raid1                   = 16,
    Raid2                   = 17,
    AlliancePvp             = 18,
    ChocoboSquare1          = 19,
    ChocoboSquare2          = 20,
    TheFirmament            = 21,
    SanctumOfTheTwelve      = 22,
    LordOfVerminion         = 25,
    TheDiadem1              = 26,
    HallOfTheNovice         = 27,
    CrystallineConflict1    = 28,
    MsqSoloDuty             = 29,
    GrandCompanyBarracks    = 30,
    DeepDungeon             = 31,
    HolidayInstance         = 32,
    MapPortal               = 33,
    HolidayDuty             = 34,
    TripleTriadBattlehall   = 35,
    CrystallineConflict2    = 37,
    TheDiademHuntingGrounds = 38,
    RivalWings              = 39,
    Eureka                  = 41,
    TheCalamityRetold       = 43,
    LeapOfFaith             = 44,
    MaskedCarnivale         = 45,
    OceanFishing            = 46,
    TheDiadem2              = 47,
    Bozja                   = 48,
    TripleTriadTournament   = 50,
    TripleTriadParlor       = 51,
    DelebrumReginae         = 52,
    DelebrumReginaeSavage   = 53,
    CriterionDungeon        = 57,
    CriterionDungeonSavage  = 58,
}

public static class TerritoryIntendedUseExtensions
{
    public static bool HasAlliance(this TerritoryIntendedUse territory) =>
        territory switch
        {
            TerritoryIntendedUse.AllianceRaid => true,
            TerritoryIntendedUse.AlliancePvp  => true,
            TerritoryIntendedUse.RivalWings   => true,
            _                                 => false,
        };

    public static bool UsesBothGroupManagers(this TerritoryIntendedUse territory) =>
        territory switch
        {
            TerritoryIntendedUse.RivalWings            => true,
            TerritoryIntendedUse.DelebrumReginae       => true,
            TerritoryIntendedUse.DelebrumReginaeSavage => true,
            _                                          => false,
        };

    public static bool IsRaidOrTrial(this TerritoryIntendedUse territory) =>
        territory switch
        {
            TerritoryIntendedUse.Trial => true,
            TerritoryIntendedUse.Raid1 => true,
            TerritoryIntendedUse.Raid2 => true,
            _                          => false,
        };

    public static bool ShouldTrack(this TerritoryIntendedUse territory) =>
        territory switch
        {
            TerritoryIntendedUse.Dungeon                 => true,
            TerritoryIntendedUse.VariantDungeon          => true,
            TerritoryIntendedUse.AllianceRaid            => true,
            TerritoryIntendedUse.Trial                   => true,
            TerritoryIntendedUse.Raid1                   => true,
            TerritoryIntendedUse.Raid2                   => true,
            TerritoryIntendedUse.AlliancePvp             => true,
            TerritoryIntendedUse.LordOfVerminion         => true,
            TerritoryIntendedUse.TheDiadem1              => true,
            TerritoryIntendedUse.CrystallineConflict1    => true,
            TerritoryIntendedUse.DeepDungeon             => true,
            TerritoryIntendedUse.MapPortal               => true,
            TerritoryIntendedUse.HolidayDuty             => true,
            TerritoryIntendedUse.CrystallineConflict2    => true,
            TerritoryIntendedUse.TheDiademHuntingGrounds => true,
            TerritoryIntendedUse.RivalWings              => true,
            TerritoryIntendedUse.Eureka                  => true,
            TerritoryIntendedUse.TheCalamityRetold       => true,
            TerritoryIntendedUse.MaskedCarnivale         => true,
            TerritoryIntendedUse.TheDiadem2              => true,
            TerritoryIntendedUse.Bozja                   => true,
            TerritoryIntendedUse.DelebrumReginae         => true,
            TerritoryIntendedUse.DelebrumReginaeSavage   => true,
            TerritoryIntendedUse.CriterionDungeon        => true,
            TerritoryIntendedUse.CriterionDungeonSavage  => true,
            _                                            => false,
        };
}
