using XpahtaLib.DalamudUtilities.UsefulEnums;

namespace DutyTracker.Extensions;

internal static class InternalTerritoryIntendedUseExtensions
{
    
    
    internal static bool ShouldTrack(this TerritoryIntendedUseEnum territory) =>
        territory switch
        {
            TerritoryIntendedUseEnum.DungeonsAndGuildhests  => true,
            TerritoryIntendedUseEnum.VariantDungeon         => true,
            TerritoryIntendedUseEnum.AllianceRaid           => true,
            TerritoryIntendedUseEnum.Trial                  => true,
            TerritoryIntendedUseEnum.Raid1                  => true,
            TerritoryIntendedUseEnum.Raid2                  => true,
            TerritoryIntendedUseEnum.Frontlines             => true,
            TerritoryIntendedUseEnum.LordOfVerminion        => true,
            TerritoryIntendedUseEnum.SmallScalePvp          => true,
            TerritoryIntendedUseEnum.DeepDungeon            => true,
            TerritoryIntendedUseEnum.MapPortal              => true,
            TerritoryIntendedUseEnum.HolidayDuty            => true,
            TerritoryIntendedUseEnum.PvpCustomMatch         => true,
            TerritoryIntendedUseEnum.RivalWings             => true,
            TerritoryIntendedUseEnum.Eureka                 => true,
            TerritoryIntendedUseEnum.MaskedCarnivale        => true,
            TerritoryIntendedUseEnum.Bozja                  => true,
            TerritoryIntendedUseEnum.DelebrumReginae        => true,
            TerritoryIntendedUseEnum.DelebrumReginaeSavage  => true,
            TerritoryIntendedUseEnum.CriterionDungeon       => true,
            TerritoryIntendedUseEnum.CriterionDungeonSavage => true,
            _                                               => false,
        };
}
