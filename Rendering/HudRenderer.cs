using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldNMilSim.Core;
using WorldNMilSim.Systems;
using WorldNMilSim.Components;
using System.Collections.Generic;

namespace WorldNMilSim.Rendering;

public class HudRenderer
{
    private static readonly string[] LegendLines =
    {
        "1: Silo | 2: Radar Station | 3: Airbase",
        "4: Destroyer | 5: Submarine | 6: Carrier | 7: Army",
        "Click empty ground/water to place | Esc: Cancel placement",
        "Click unit: Select/Move | Shift+Click: Nuclear strike",
        "P: Sonar active/passive | E: EMCON | J: Jamming | D: Decoy",
        "Scroll: Zoom | Right-drag/Arrows: Pan | Tab: Debug reveal"
    };

    public void Draw(SpriteBatch spriteBatch, World world, Entity playerFaction, Entity? selectedUnit, Entity defconEntity, Entity tensionEntity, Entity diplomacyEntity, Entity chatterEntity, UnitType? placementSelection, SpriteFont font, GraphicsDevice device)
    {
        DrawDefcon(spriteBatch, world, defconEntity, font);

        var tension = world.Get<TensionComponent>(tensionEntity);
        if (tension != null)
            DrawShadowedText(spriteBatch, font, $"Tension: {(int)tension.Tension}", new Vector2(20, 44), Color.OrangeRed);

        DrawFactionStatus(spriteBatch, world, playerFaction, font);
        DrawKeybindLegend(spriteBatch, font, device);
        DrawDiplomacy(spriteBatch, world, playerFaction, diplomacyEntity, font, device);
        DrawChatterLog(spriteBatch, world, chatterEntity, font, device);

        if (placementSelection.HasValue)
            DrawShadowedText(spriteBatch, font, $"Placing: {placementSelection.Value} (click to place, Esc to cancel)", new Vector2(20, 160), Color.Cyan);

        if (selectedUnit.HasValue)
            DrawUnitPanel(spriteBatch, world, selectedUnit.Value, font, device);
    }

    private void DrawChatterLog(SpriteBatch spriteBatch, World world, Entity chatterEntity, SpriteFont font, GraphicsDevice device)
    {
        var log = world.Get<ChatterLogComponent>(chatterEntity);
        if (log == null || log.Messages.Count == 0) return;

        float x = device.Viewport.Width / 2f - 250;
        float y = 20;

        for (int i = log.Messages.Count - 1; i >= 0; i--)
        {
            var message = log.Messages[i];
            float alpha = (float)System.Math.Min(1.0, message.RemainingSeconds / 2.0); // fade out in the last 2 seconds
            DrawShadowedText(spriteBatch, font, message.Text, new Vector2(x, y), Color.LightYellow * alpha);
            y += 20;
        }
    }

    private void DrawDiplomacy(SpriteBatch spriteBatch, World world, Entity playerFaction, Entity diplomacyEntity, SpriteFont font, GraphicsDevice device)
    {
        var diplomacy = world.Get<DiplomacyComponent>(diplomacyEntity);
        if (diplomacy == null) return;

        var lines = new List<(string text, Color color)>();
        int index = 1;
        foreach (var (factionEntity, faction) in world.Query<FactionComponent>())
        {
            if (faction.IsPlayerControlled) continue;

            var stance = diplomacy.GetStance(playerFaction, factionEntity);
            var stanceColor = stance switch
            {
                RelationStance.War => Color.Red,
                RelationStance.Neutral => Color.Yellow,
                _ => Color.LightGreen
            };
            lines.Add(($"F{index}: {faction.Name} - {stance}", stanceColor));
            index++;
        }

        float x = device.Viewport.Width - 320;
        float y = device.Viewport.Height - 20 - lines.Count * 20;
        foreach (var (text, color) in lines)
        {
            DrawShadowedText(spriteBatch, font, text, new Vector2(x, y), color);
            y += 20;
        }
    }

    private void DrawDefcon(SpriteBatch spriteBatch, World world, Entity defconEntity, SpriteFont font)
    {
        var defcon = world.Get<DefconComponent>(defconEntity);
        if (defcon == null) return;

        var color = defcon.Level switch
        {
            5 => Color.LightGreen,
            4 => Color.Yellow,
            3 => Color.Orange,
            2 => Color.OrangeRed,
            _ => Color.Red
        };
        DrawShadowedText(spriteBatch, font, $"DEFCON {defcon.Level}", new Vector2(20, 20), color);
    }

    private void DrawFactionStatus(SpriteBatch spriteBatch, World world, Entity faction, SpriteFont font)
    {
        var factionInfo = world.Get<FactionComponent>(faction);
        var budget = world.Get<PlacementBudgetComponent>(faction);
        float y = 72;

        if (factionInfo != null)
        {
            DrawShadowedText(spriteBatch, font, factionInfo.Name, new Vector2(20, y), factionInfo.Color);
            y += 22;
        }
        if (budget != null)
        {
            DrawShadowedText(spriteBatch, font, $"Budget: {(int)budget.Points}", new Vector2(20, y), Color.White);
            y += 22;
        }
        if (factionInfo != null)
        {
            DrawShadowedText(spriteBatch, font, $"Casualties inflicted: {factionInfo.TotalCasualtiesInflicted}", new Vector2(20, y), Color.White);
            y += 22;
            DrawShadowedText(spriteBatch, font, $"Casualties suffered: {factionInfo.TotalCasualtiesSuffered}", new Vector2(20, y), Color.White);
        }
    }

    private void DrawKeybindLegend(SpriteBatch spriteBatch, SpriteFont font, GraphicsDevice device)
    {
        float y = device.Viewport.Height - 20 - LegendLines.Length * 20;
        foreach (var line in LegendLines)
        {
            DrawShadowedText(spriteBatch, font, line, new Vector2(20, y), Color.LightGray);
            y += 20;
        }
    }

    private void DrawUnitPanel(SpriteBatch spriteBatch, World world, Entity unit, SpriteFont font, GraphicsDevice device)
    {
        var unitInfo = world.Get<UnitComponent>(unit);
        if (unitInfo == null) return;

        float x = device.Viewport.Width - 320;
        float y = 20;

        DrawShadowedText(spriteBatch, font, unitInfo.Name, new Vector2(x, y), Color.White);
        y += 24;

        var health = world.Get<HealthComponent>(unit);
        if (health != null)
        {
            DrawShadowedText(spriteBatch, font, $"Health: {(int)health.CurrentHealth}/{(int)health.MaxHealth}", new Vector2(x, y), Color.White);
            y += 20;
        }

        var logistics = world.Get<LogisticsComponent>(unit);
        if (logistics != null)
        {
            if (logistics.MaxFuel > 0)
            {
                DrawShadowedText(spriteBatch, font, $"Fuel: {(int)logistics.Fuel}/{(int)logistics.MaxFuel}", new Vector2(x, y), Color.White);
                y += 20;
            }
            if (logistics.MaxAmmo > 0)
            {
                DrawShadowedText(spriteBatch, font, $"Ammo: {(int)logistics.Ammo}/{(int)logistics.MaxAmmo}", new Vector2(x, y), Color.White);
                y += 20;
            }
            DrawShadowedText(spriteBatch, font, logistics.IsSupplied ? "Supplied" : "OUT OF SUPPLY", new Vector2(x, y), logistics.IsSupplied ? Color.LightGreen : Color.OrangeRed);
            y += 20;
        }

        var sensors = world.Get<SensorsComponent>(unit);
        if (sensors != null)
        {
            foreach (var sensor in sensors.Sensors)
            {
                string label = sensor.Type == SensorType.Radar
                    ? $"Radar: {sensor.DetectionRadiusKm:0}km"
                    : $"Sonar: {sensor.Mode} ({sensor.EffectiveRangeKm:0}km)";
                DrawShadowedText(spriteBatch, font, label, new Vector2(x, y), Color.LightBlue);
                y += 20;
            }
        }

        if (world.Has<EmconComponent>(unit))
        {
            DrawShadowedText(spriteBatch, font, "EMCON: ACTIVE", new Vector2(x, y), Color.Gray);
            y += 20;
        }

        var jammer = world.Get<JammerComponent>(unit);
        if (jammer != null)
        {
            DrawShadowedText(spriteBatch, font, $"Jammer: {(jammer.IsActive ? "ACTIVE" : "off")}", new Vector2(x, y), jammer.IsActive ? Color.Magenta : Color.Gray);
            y += 20;
        }

        var decoyLauncher = world.Get<DecoyLauncherComponent>(unit);
        if (decoyLauncher != null)
        {
            DrawShadowedText(spriteBatch, font, $"Decoys: {decoyLauncher.RemainingDecoys}/{decoyLauncher.MaxDecoys}", new Vector2(x, y), Color.White);
            y += 20;
        }

        var interceptor = world.Get<InterceptorComponent>(unit);
        if (interceptor != null)
        {
            DrawShadowedText(spriteBatch, font, $"Interceptors: {interceptor.RemainingInterceptors}/{interceptor.MaxInterceptors}", new Vector2(x, y), Color.LightGreen);
            y += 20;
        }

        var weapon = world.Get<WeaponComponent>(unit);
        if (weapon != null)
        {
            string weaponStatus = weapon.CooldownRemaining > 0 ? $"Reloading ({weapon.CooldownRemaining:0}s)" : "Ready";
            DrawShadowedText(spriteBatch, font, $"Weapon: {weaponStatus}{(weapon.IsNuclear ? " [NUCLEAR]" : "")}", new Vector2(x, y), weapon.IsNuclear ? Color.Red : Color.White);
        }
    }

    private void DrawShadowedText(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color)
    {
        spriteBatch.DrawString(font, text, position + Vector2.One, Color.Black * 0.7f);
        spriteBatch.DrawString(font, text, position, color);
    }

    public void DrawMatchResult(SpriteBatch spriteBatch, World world, Entity playerFaction, MatchState state, SpriteFont font, GraphicsDevice device)
    {
        if (state == MatchState.InProgress) return;

        string title = state == MatchState.PlayerWon ? "VICTORY" : "DEFEAT";
        var color = state == MatchState.PlayerWon ? Color.LightGreen : Color.Red;

        var titleSize = font.MeasureString(title) * 3f;
        var titlePos = new Vector2(device.Viewport.Width / 2f - titleSize.X / 2f, device.Viewport.Height / 2f - titleSize.Y / 2f - 40);

        spriteBatch.DrawString(font, title, titlePos + Vector2.One * 3, Color.Black * 0.8f, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
        spriteBatch.DrawString(font, title, titlePos, color, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);

        var factionInfo = world.Get<FactionComponent>(playerFaction);
        if (factionInfo != null)
        {
            string stats = $"Casualties inflicted: {factionInfo.TotalCasualtiesInflicted}   Casualties suffered: {factionInfo.TotalCasualtiesSuffered}";
            var statsSize = font.MeasureString(stats);
            var statsPos = new Vector2(device.Viewport.Width / 2f - statsSize.X / 2f, titlePos.Y + titleSize.Y + 20);
            DrawShadowedText(spriteBatch, font, stats, statsPos, Color.White);
        }
    }
}