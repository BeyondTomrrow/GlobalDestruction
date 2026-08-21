using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldNMilSim.Components;
using WorldNMilSim.Core;
using WorldNMilSim.Map;
using WorldNMilSim.Rendering;
using WorldNMilSim.Systems;
using WorldNMilSim.Units;

namespace WorldNMilSim;

public class Game1 : Game
{


    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _worldMapTexture;

    private SystemManager _systems;
    private Entity _playerFaction;

    private World _world;
    private Dictionary<string, Entity> _territories;
    private MapDebugRenderer _mapRenderer;
    private UnitDebugRenderer _unitRenderer;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1800;
        _graphics.PreferredBackBufferHeight = 900;
        _graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _world = new World();
        _territories = MapBuilder.Build(_world);

        _playerFaction = _world.CreateEntity();
        _world.Set(_playerFaction, new FactionComponent { Name = "United Coalition", Color = Color.CornflowerBlue, IsPlayerControlled = true });

        _world.Get<OwnershipComponent>(_territories["na_east"])!.Owner = _playerFaction;

        UnitFactory.SpawnAtTerritory(_world, "silo", _playerFaction, _territories["na_east"]);
        UnitFactory.SpawnAtTerritory(_world, "radar_station", _playerFaction, _territories["na_east"]);
        var sub = UnitFactory.Spawn(_world, "submarine", _playerFaction, 35, -60); // mid atlantic(ish)


        _systems = new SystemManager()
            .Add(new LogisticsSystem());

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _mapRenderer = new MapDebugRenderer(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        _unitRenderer = new UnitDebugRenderer(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        _worldMapTexture = Content.Load<Texture2D>("world_map");
    }

    protected override void Update(GameTime gameTime)
    {
        _systems.Update(_world, gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        var destRect = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        _spriteBatch.Draw(_worldMapTexture, destRect, Color.White);

        _mapRenderer.Draw(_spriteBatch, _world);
        _unitRenderer.Draw(_spriteBatch, _world);
        _spriteBatch.End();
    }
}