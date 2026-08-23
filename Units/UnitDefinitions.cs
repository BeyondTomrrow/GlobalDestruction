using System.Collections.Generic;
using WorldNMilSim.Components;

namespace WorldNMilSim.Units;

public record UnitDef(
    string Id,
    string Name,
    UnitType Type,
    UnitDomain Domain,
    bool IsStationary,
    double MaxHealth,
    double MaxFuel,
    double FuelConsumptionPerHour,
    double MaxAmmo,
    double SupplyRangeKm,
    double RadarSignature,
    double AcousticSignature,
    double WeaponRangeKm,
    double WeaponDamage,
    bool IsNuclearCapable,
    double MaxSpeedKmh,
    double RateOfFireSeconds,
    double BlastRadiusKm
);

public static class UnitDefinitions
{
    public static readonly Dictionary<string, UnitDef> All = new()
    {
        ["silo"] = new("silo", "ICBM Silo", UnitType.Silo, UnitDomain.Land, true,
            MaxHealth: 100, MaxFuel: 0, FuelConsumptionPerHour: 0, MaxAmmo: 10,
            SupplyRangeKm: 0, RadarSignature: 1.0, AcousticSignature: 0,
            WeaponRangeKm: 12000, WeaponDamage: 80, IsNuclearCapable: true, MaxSpeedKmh: 0,
            RateOfFireSeconds: 15, BlastRadiusKm: 150),

        ["radar_station"] = new("radar_station", "Radar Station", UnitType.RadarStation, UnitDomain.Land, true,
            MaxHealth: 60, MaxFuel: 0, FuelConsumptionPerHour: 0, MaxAmmo: 0,
            SupplyRangeKm: 0, RadarSignature: 1.0, AcousticSignature: 0,
            WeaponRangeKm: 0, WeaponDamage: 0, IsNuclearCapable: false, MaxSpeedKmh: 0,
            RateOfFireSeconds: 0, BlastRadiusKm: 0),

        ["airbase"] = new("airbase", "Airbase", UnitType.Airbase, UnitDomain.Land, true,
            MaxHealth: 120, MaxFuel: 500, FuelConsumptionPerHour: 0, MaxAmmo: 40,
            SupplyRangeKm: 0, RadarSignature: 1.0, AcousticSignature: 0,
            WeaponRangeKm: 2500, WeaponDamage: 40, IsNuclearCapable: true, MaxSpeedKmh: 0,
            RateOfFireSeconds: 25, BlastRadiusKm: 60),

        ["destroyer"] = new("destroyer", "Destroyer", UnitType.Destroyer, UnitDomain.Surface, false,
            MaxHealth: 80, MaxFuel: 1000, FuelConsumptionPerHour: 8, MaxAmmo: 60,
            SupplyRangeKm: 3000, RadarSignature: 1.0, AcousticSignature: 0.7,
            WeaponRangeKm: 150, WeaponDamage: 40, IsNuclearCapable: false, MaxSpeedKmh: 65,
            RateOfFireSeconds: 4, BlastRadiusKm: 0),

        ["submarine"] = new("submarine", "Submarine (SSBN)", UnitType.Submarine, UnitDomain.Submerged, false,
            MaxHealth: 50, MaxFuel: 2000, FuelConsumptionPerHour: 5, MaxAmmo: 16,
            SupplyRangeKm: 5000, RadarSignature: 0.0, AcousticSignature: 0.2,
            WeaponRangeKm: 10000, WeaponDamage: 70, IsNuclearCapable: true, MaxSpeedKmh: 46,
            RateOfFireSeconds: 20, BlastRadiusKm: 120),

        ["carrier"] = new("carrier", "Carrier Group", UnitType.Carrier, UnitDomain.Surface, false,
            MaxHealth: 150, MaxFuel: 1500, FuelConsumptionPerHour: 12, MaxAmmo: 100,
            SupplyRangeKm: 4000, RadarSignature: 1.0, AcousticSignature: 0.8,
            WeaponRangeKm: 400, WeaponDamage: 60, IsNuclearCapable: false, MaxSpeedKmh: 56,
            RateOfFireSeconds: 6, BlastRadiusKm: 0),
    };
}