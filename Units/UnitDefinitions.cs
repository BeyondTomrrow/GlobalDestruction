using System.Collections.Generic;
using WorldNMilSim.Components;

namespace WorldNMilSim.Units;

public record UnitDef(
    string Id,
    string Name,
    UnitType Type,
    bool IsStationary,
    double MaxHealth,
    double MaxFuel,
    double FuelConsumptionPerHour,
    double MaxAmmo,
    double SupplyRangeKm,
    double DetectionRadiusKm,
    double StealthSignature,
    double WeaponRangeKm,
    double WeaponDamage,
    bool IsNuclearCapable
);

public static class UnitDefinitions
{
    public static readonly Dictionary<string, UnitDef> All = new()
    {
        ["silo"] = new("silo", "ICBM Silo", UnitType.Silo, true,
            MaxHealth: 100, MaxFuel: 0, FuelConsumptionPerHour: 0, MaxAmmo: 10,
            SupplyRangeKm: 0, DetectionRadiusKm: 50, StealthSignature: 1.0,
            WeaponRangeKm: 12000, WeaponDamage: 500, IsNuclearCapable: true),

        ["radar_station"] = new("radar_station", "Radar Station", UnitType.RadarStation, true,
            MaxHealth: 60, MaxFuel: 0, FuelConsumptionPerHour: 0, MaxAmmo: 0,
            SupplyRangeKm: 0, DetectionRadiusKm: 1500, StealthSignature: 1.0,
            WeaponRangeKm: 0, WeaponDamage: 0, IsNuclearCapable: false),

        ["airbase"] = new("airbase", "Airbase", UnitType.Airbase, true,
            MaxHealth: 120, MaxFuel: 500, FuelConsumptionPerHour: 0, MaxAmmo: 40,
            SupplyRangeKm: 0, DetectionRadiusKm: 300, StealthSignature: 1.0,
            WeaponRangeKm: 2500, WeaponDamage: 80, IsNuclearCapable: true),

        ["destroyer"] = new("destroyer", "Destroyer", UnitType.Destroyer, false,
            MaxHealth: 80, MaxFuel: 1000, FuelConsumptionPerHour: 8, MaxAmmo: 60,
            SupplyRangeKm: 3000, DetectionRadiusKm: 400, StealthSignature: 0.8,
            WeaponRangeKm: 150, WeaponDamage: 40, IsNuclearCapable: false),

        ["submarine"] = new("submarine", "Submarine (SSBN)", UnitType.Submarine, false,
            MaxHealth: 50, MaxFuel: 2000, FuelConsumptionPerHour: 5, MaxAmmo: 16,
            SupplyRangeKm: 5000, DetectionRadiusKm: 150, StealthSignature: 0.2,
            WeaponRangeKm: 10000, WeaponDamage: 500, IsNuclearCapable: true),

        ["carrier"] = new("carrier", "Carrier Group", UnitType.Carrier, false,
            MaxHealth: 150, MaxFuel: 1500, FuelConsumptionPerHour: 12, MaxAmmo: 100,
            SupplyRangeKm: 4000, DetectionRadiusKm: 700, StealthSignature: 1.0,
            WeaponRangeKm: 400, WeaponDamage: 60, IsNuclearCapable: false),
    };
}