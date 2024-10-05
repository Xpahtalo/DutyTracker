using DutyTracker.Enums;

namespace DutyTracker.Extensions;

internal static class TerritoryExtensions
{
    public static TerritoryIntendedUseEnum GetIntendedUseEnum(
        this Lumina.Excel.GeneratedSheets.TerritoryType territoryType) =>
        (TerritoryIntendedUseEnum)territoryType.TerritoryIntendedUse;

    public static bool HasAlliance(this TerritoryIntendedUseEnum territory) =>
        territory switch
        {
            TerritoryIntendedUseEnum.AllianceRaid => true,
            TerritoryIntendedUseEnum.Frontlines => true,
            TerritoryIntendedUseEnum.RivalWings => true,
            _ => false,
        };

    public static bool UsesBothGroupManagers(this TerritoryIntendedUseEnum territory) =>
        territory switch
        {
            TerritoryIntendedUseEnum.RivalWings => true,
            TerritoryIntendedUseEnum.DelebrumReginae => true,
            TerritoryIntendedUseEnum.DelebrumReginaeSavage => true,
            _ => false,
        };

    public static bool IsRaidOrTrial(this TerritoryIntendedUseEnum territory) =>
        territory switch
        {
            TerritoryIntendedUseEnum.Trial => true,
            TerritoryIntendedUseEnum.Raid1 => true,
            TerritoryIntendedUseEnum.Raid2 => true,
            _ => false,
        };

    public static AllianceType GetAllianceType(this TerritoryIntendedUseEnum territory) =>
        territory switch
        {
            TerritoryIntendedUseEnum.RivalWings => AllianceType.SixParty,
            TerritoryIntendedUseEnum.AllianceRaid => AllianceType.ThreeParty,
            TerritoryIntendedUseEnum.Frontlines => AllianceType.ThreeParty,
            _ => AllianceType.None,
        };

    internal static bool ShouldTrack(this TerritoryIntendedUseEnum territory) =>
        territory switch
        {
            TerritoryIntendedUseEnum.DungeonsAndGuildhests => true,
            TerritoryIntendedUseEnum.VariantDungeon => true,
            TerritoryIntendedUseEnum.AllianceRaid => true,
            TerritoryIntendedUseEnum.Trial => true,
            TerritoryIntendedUseEnum.Raid1 => true,
            TerritoryIntendedUseEnum.Raid2 => true,
            TerritoryIntendedUseEnum.Frontlines => true,
            TerritoryIntendedUseEnum.LordOfVerminion => true,
            TerritoryIntendedUseEnum.SmallScalePvp => true,
            TerritoryIntendedUseEnum.DeepDungeon => true,
            TerritoryIntendedUseEnum.MapPortal => true,
            TerritoryIntendedUseEnum.HolidayDuty => true,
            TerritoryIntendedUseEnum.PvpCustomMatch => true,
            TerritoryIntendedUseEnum.RivalWings => true,
            TerritoryIntendedUseEnum.Eureka => true,
            TerritoryIntendedUseEnum.MaskedCarnivale => true,
            TerritoryIntendedUseEnum.Bozja => true,
            TerritoryIntendedUseEnum.DelebrumReginae => true,
            TerritoryIntendedUseEnum.DelebrumReginaeSavage => true,
            TerritoryIntendedUseEnum.CriterionDungeon => true,
            TerritoryIntendedUseEnum.CriterionDungeonSavage => true,
            _ => false,
        };
}