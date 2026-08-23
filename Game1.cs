using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WorldNMilSim.Core;
using WorldNMilSim.Components;
using WorldNMilSim.Map;
using WorldNMilSim.Units;
using WorldNMilSim.Systems;
using WorldNMilSim.Rendering;
using System.Collections.Generic;

namespace WorldNMilSim;

public class Game1 : Game
{

    private readonly System.Random _random = new();
    
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _worldMapTexture;
    private SpriteFont _font;

    private World _world;
    private Dictionary<string, Entity> _territories;
    private MapDebugRenderer _mapRenderer;
    private UnitDebugRenderer _unitRenderer;
    private Camera2D _camera;

    private SystemManager _systems;
    private Entity _playerFaction;

    private bool _debugShowAllUnits;
    private MouseState _previousMouseState;
    private KeyboardState _previousKeyboardState;

    private PostProcessPipeline _postProcess;

    private Entity? _selectedUnit;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;

        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        _graphics.IsFullScreen = false;
        _graphics.HardwareModeSwitch = false; // borderless-style fullscreen at desktop res, avoids display-mode-switch flicker/alt-tab issues
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        _world = new World();
        _territories = MapBuilder.Build(_world);
        CityBuilder.Build(_world, _territories);

        // Player faction
        _playerFaction = _world.CreateEntity();
        _world.Set(_playerFaction, new FactionComponent { Name = "United Coalition", Color = Color.CornflowerBlue, IsPlayerControlled = true });

        _world.Get<OwnershipComponent>(_territories["na_east"])!.Owner = _playerFaction;
        _world.Get<OwnershipComponent>(_territories["na_west"])!.Owner = _playerFaction;

        UnitFactory.SpawnAtTerritory(_world, "silo", _playerFaction, _territories["na_east"]);
        UnitFactory.SpawnAtTerritory(_world, "radar_station", _playerFaction, _territories["na_east"], radarFacingDegrees: 0);
        UnitFactory.SpawnAtTerritory(_world, "airbase", _playerFaction, _territories["na_west"]);
        UnitFactory.Spawn(_world, "submarine", _playerFaction, 35, -60); // mid-Atlantic patrol
        UnitFactory.Spawn(_world, "carrier", _playerFaction, 28, -45);

        // Rival faction, for testing detection/fog-of-war
        var rivalFaction = _world.CreateEntity();
        _world.Set(rivalFaction, new FactionComponent { Name = "Red Bloc", Color = Color.OrangeRed, IsPlayerControlled = false });

        _world.Get<OwnershipComponent>(_territories["e_europe"])!.Owner = rivalFaction;
        UnitFactory.SpawnAtTerritory(_world, "silo", rivalFaction, _territories["e_europe"]);
        UnitFactory.Spawn(_world, "destroyer", rivalFaction, 33, -42); // close to your submarine, to test detection ranges

        _systems = new SystemManager()
            .Add(new MovementSystem())
            .Add(new NuclearImpactSystem())
            .Add(new LogisticsSystem())
            .Add(new DetectionSystem())
            .Add(new CombatSystem())
            .Add(new DecoySystem());

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _worldMapTexture = Content.Load<Texture2D>("world_map");
        _font = Content.Load<SpriteFont>("MapFont");
        var mapStylizeEffect = Content.Load<Effect>("MapStylize");
        var blurEffect = Content.Load<Effect>("Blur");
        var finalCompositeEffect = Content.Load<Effect>("FinalComposite");

        _postProcess = new PostProcessPipeline(GraphicsDevice, _spriteBatch, mapStylizeEffect, blurEffect, finalCompositeEffect);
        _mapRenderer = new MapDebugRenderer(GraphicsDevice);
        _unitRenderer = new UnitDebugRenderer(GraphicsDevice);
        _camera = new Camera2D(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();
        var keyboardState = Keyboard.GetState();

        int scrollDelta = mouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
        if (scrollDelta != 0)
        {
            float zoomMultiplier = scrollDelta > 0 ? 1.1f : 0.9f;
            _camera.ZoomAt(zoomMultiplier, new Vector2(mouseState.X, mouseState.Y));
        }

        if (mouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Pressed)
        {
            var screenDelta = new Vector2(mouseState.X - _previousMouseState.X, mouseState.Y - _previousMouseState.Y);
            _camera.Pan(-screenDelta / _camera.ZoomLevel);
        }

        float panSpeed = 600f * (float)gameTime.ElapsedGameTime.TotalSeconds / _camera.ZoomLevel;
        var keyPan = Vector2.Zero;
        if (keyboardState.IsKeyDown(Keys.Left)) keyPan.X -= panSpeed;
        if (keyboardState.IsKeyDown(Keys.Right)) keyPan.X += panSpeed;
        if (keyboardState.IsKeyDown(Keys.Up)) keyPan.Y -= panSpeed;
        if (keyboardState.IsKeyDown(Keys.Down)) keyPan.Y += panSpeed;
        if (keyPan != Vector2.Zero) _camera.Pan(keyPan);

        if (keyboardState.IsKeyDown(Keys.Tab) && !_previousKeyboardState.IsKeyDown(Keys.Tab))
            _debugShowAllUnits = !_debugShowAllUnits;

        if (keyboardState.IsKeyDown(Keys.P) && !_previousKeyboardState.IsKeyDown(Keys.P) && _selectedUnit.HasValue)
        {
            var sensors = _world.Get<SensorsComponent>(_selectedUnit.Value);
            if (sensors != null)
            {
                foreach (var sensor in sensors.Sensors)
                {
                    if (sensor.Type == SensorType.Sonar)
                        sensor.Mode = sensor.Mode == SonarMode.Passive ? SonarMode.Active : SonarMode.Passive;
                }
            }
        }

        if (keyboardState.IsKeyDown(Keys.E) && !_previousKeyboardState.IsKeyDown(Keys.E) && _selectedUnit.HasValue)
        {
            var unit = _selectedUnit.Value;
            if (_world.Has<EmconComponent>(unit))
            {
                _world.Remove<EmconComponent>(unit);
            }
            else
            {
                _world.Set(unit, new EmconComponent());
                var sensors = _world.Get<SensorsComponent>(unit);
                if (sensors != null)
                {
                    foreach (var sensor in sensors.Sensors)
                        if (sensor.Type == SensorType.Sonar)
                            sensor.Mode = SonarMode.Passive; // can't ping while running dark
                }

                var jammer = _world.Get<JammerComponent>(unit);
                if (jammer != null)
                    jammer.IsActive = false; // can't jam while running dark
            }
        }

        if (keyboardState.IsKeyDown(Keys.J) && !_previousKeyboardState.IsKeyDown(Keys.J) && _selectedUnit.HasValue)
        {
            var jammer = _world.Get<JammerComponent>(_selectedUnit.Value);
            if (jammer != null)
            {
                jammer.IsActive = !jammer.IsActive;
                if (jammer.IsActive)
                    _world.Remove<EmconComponent>(_selectedUnit.Value); // can't run dark while actively broadcasting jamming noise
            }
        }

        if (keyboardState.IsKeyDown(Keys.D) && !_previousKeyboardState.IsKeyDown(Keys.D) && _selectedUnit.HasValue)
        {
            DeployDecoy(_selectedUnit.Value);
        }

        bool cursorInWindow = mouseState.X >= 0 && mouseState.X < GraphicsDevice.Viewport.Width &&
                               mouseState.Y >= 0 && mouseState.Y < GraphicsDevice.Viewport.Height;

        if (IsActive && cursorInWindow &&
            mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
        {
            bool nuclearModifier = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
            HandleLeftClick(new Vector2(mouseState.X, mouseState.Y), nuclearModifier);
        }

        _previousMouseState = mouseState;
        _previousKeyboardState = keyboardState;

        _systems.Update(_world, gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        void DrawMapLayer(Effect effect)
        {
            _spriteBatch.Begin(effect: effect, transformMatrix: _camera.GetViewMatrix(), samplerState: SamplerState.LinearClamp);
            _spriteBatch.Draw(_worldMapTexture, new Rectangle(0, 0, MapSpace.WIDTH, MapSpace.HEIGHT), Color.White);
            _spriteBatch.End();
        }

        void DrawUiLayer()
        {
            _spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix(), samplerState: SamplerState.LinearClamp);
            _mapRenderer.Draw(_spriteBatch, _world);
            _unitRenderer.Draw(_spriteBatch, _world, _playerFaction, _debugShowAllUnits);
            _unitRenderer.DrawSelection(_spriteBatch, _world, _selectedUnit);
            _unitRenderer.DrawIncomingStrikes(_spriteBatch, _world);
            _unitRenderer.DrawRadarCones(_spriteBatch, _world, _playerFaction);
            _unitRenderer.DrawJammingRadius(_spriteBatch, _world, _playerFaction);
            _spriteBatch.End();
        }

        _postProcess.Render(DrawMapLayer, DrawUiLayer);

        // Labels drawn last, directly to the backbuffer - stays crisp, not blurred/vignetted.
        _spriteBatch.Begin();
        _mapRenderer.DrawLabels(_spriteBatch, _world, _camera, _font);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void HandleLeftClick(Vector2 screenPosition, bool nuclearStrikeModifier)
    {
        const float selectRadiusPixels = 18f;

        Entity? clickedUnit = null;
        float closestDist = selectRadiusPixels;

        foreach (var (entity, unit, position, ownership) in _world.Query<UnitComponent, PositionComponent, OwnershipComponent>())
        {
            if (ownership.Owner != _playerFaction) continue;

            var unitScreenPos = _camera.WorldToScreen(GeoMath.Project(position.Latitude, position.Longitude));
            float dist = Vector2.Distance(unitScreenPos, screenPosition);
            if (dist < closestDist)
            {
                closestDist = dist;
                clickedUnit = entity;
            }
        }

        if (clickedUnit.HasValue)
        {
            _selectedUnit = _selectedUnit == clickedUnit ? null : clickedUnit;
            return;
        }

        if (!_selectedUnit.HasValue) return;

        var worldPos = _camera.ScreenToWorld(screenPosition);
        var (lat, lon) = GeoMath.Unproject(worldPos);

        if (nuclearStrikeModifier)
        {
            LaunchNuclearStrike(_selectedUnit.Value, lat, lon);
            return;
        }

        if (_world.Has<MovementComponent>(_selectedUnit.Value))
        {
            _world.Set(_selectedUnit.Value, new MoveOrderComponent { TargetLatitude = lat, TargetLongitude = lon });
        }
    }

    private void LaunchNuclearStrike(Entity launcher, double targetLat, double targetLon)
    {
        var weapon = _world.Get<WeaponComponent>(launcher);
        var ownership = _world.Get<OwnershipComponent>(launcher);
        var position = _world.Get<PositionComponent>(launcher);
        var logistics = _world.Get<LogisticsComponent>(launcher);

        if (weapon == null || !weapon.IsNuclear) return;
        if (weapon.CooldownRemaining > 0) return;
        if (logistics != null && logistics.Ammo < weapon.AmmoPerShot) return;
        if (ownership?.Owner is not { } faction) return;
        if (position == null) return;

        if (logistics != null) logistics.Ammo -= weapon.AmmoPerShot;
        weapon.CooldownRemaining = weapon.RateOfFireSeconds;

        var missile = _world.CreateEntity();
        _world.Set(missile, new PositionComponent { Latitude = position.Latitude, Longitude = position.Longitude });
        _world.Set(missile, new MovementComponent { MaxSpeedKmh = 7500 });
        _world.Set(missile, new MoveOrderComponent { TargetLatitude = targetLat, TargetLongitude = targetLon });
        _world.Set(missile, new IncomingStrikeComponent
        {
            AttackerFaction = faction,
            BlastRadiusKm = weapon.BlastRadiusKm,
            MaxCasualties = weapon.Damage
        });
    }

    private void DeployDecoy(Entity unit)
    {
        var launcher = _world.Get<DecoyLauncherComponent>(unit);
        var position = _world.Get<PositionComponent>(unit);
        var ownership = _world.Get<OwnershipComponent>(unit);
        var unitInfo = _world.Get<UnitComponent>(unit);

        if (launcher == null || position == null || unitInfo == null) return;
        if (ownership?.Owner is not { } faction) return;
        if (launcher.RemainingDecoys <= 0 || launcher.CooldownRemaining > 0) return;

        launcher.RemainingDecoys--;
        launcher.CooldownRemaining = launcher.CooldownSeconds;

        double offsetLat = position.Latitude + (_random.NextDouble() * 2 - 1) * 0.3;
        double offsetLon = position.Longitude + (_random.NextDouble() * 2 - 1) * 0.3;

        DecoyFactory.Spawn(_world, faction, offsetLat, offsetLon, unitInfo.Domain);
    }
}