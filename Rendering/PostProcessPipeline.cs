using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldNMilSim.Rendering;

// Multi-pass stylization: darkens/desaturates the base map, adds a soft glow to UI elements
// (dots/routes/icons), then composites with a vignette + scanline overlay for an ops-room feel.
public class PostProcessPipeline
{
    private readonly GraphicsDevice _device;
    private readonly SpriteBatch _spriteBatch;

    private readonly Effect _mapStylizeEffect;
    private readonly Effect _blurEffect;
    private readonly Effect _finalCompositeEffect;

    private RenderTarget2D _sceneTarget;
    private RenderTarget2D _glowSourceTarget;
    private RenderTarget2D _blurTargetA;
    private RenderTarget2D _blurTargetB;

    public PostProcessPipeline(GraphicsDevice device, SpriteBatch spriteBatch, Effect mapStylizeEffect, Effect blurEffect, Effect finalCompositeEffect)
    {
        _device = device;
        _spriteBatch = spriteBatch;
        _mapStylizeEffect = mapStylizeEffect;
        _blurEffect = blurEffect;
        _finalCompositeEffect = finalCompositeEffect;

        ApplyEffectDefaults();
        CreateRenderTargets();
    }

    private void ApplyEffectDefaults()
    {
        // MonoGame's effect compiler drops inline HLSL default values ("float Foo = 0.5;") -
        // every parameter starts at 0 at runtime unless explicitly set here.
        _mapStylizeEffect.Parameters["Desaturation"].SetValue(0.45f);
        _mapStylizeEffect.Parameters["Darken"].SetValue(0.20f);

        _blurEffect.Parameters["BlurAmount"].SetValue(5.0f);

        _finalCompositeEffect.Parameters["GlowIntensity"].SetValue(1.5f);
        _finalCompositeEffect.Parameters["VignetteStrength"].SetValue(0.55f);
        _finalCompositeEffect.Parameters["ScanlineIntensity"].SetValue(0.08f);
        _finalCompositeEffect.Parameters["ScanlineDensity"].SetValue(400f);
    }

    private void CreateRenderTargets()
    {
        int width = _device.PresentationParameters.BackBufferWidth;
        int height = _device.PresentationParameters.BackBufferHeight;

        _sceneTarget?.Dispose();
        _glowSourceTarget?.Dispose();
        _blurTargetA?.Dispose();
        _blurTargetB?.Dispose();

        _sceneTarget = new RenderTarget2D(_device, width, height);
        _glowSourceTarget = new RenderTarget2D(_device, width, height);
        _blurTargetA = new RenderTarget2D(_device, width, height);
        _blurTargetB = new RenderTarget2D(_device, width, height);
    }

    private void EnsureSized()
    {
        if (_sceneTarget.Width != _device.PresentationParameters.BackBufferWidth ||
            _sceneTarget.Height != _device.PresentationParameters.BackBufferHeight)
        {
            CreateRenderTargets();
        }
    }

    // drawMap: draws just the background map texture, using the given effect (world space).
    // drawUi: draws dots/routes/unit icons, default effect (world space).
    public void Render(System.Action<Effect> drawMap, System.Action drawUi)
    {
        EnsureSized();

        // Pass 1: base scene = stylized map + UI drawn normally, into one target.
        _device.SetRenderTarget(_sceneTarget);
        _device.Clear(Color.Black);
        drawMap(_mapStylizeEffect);
        drawUi();

        // Pass 2: UI drawn again, alone, as the glow source.
        _device.SetRenderTarget(_glowSourceTarget);
        _device.Clear(Color.Transparent);
        drawUi();

        // Pass 3: separable blur - horizontal then vertical.
        float texelX = 1f / _glowSourceTarget.Width;
        float texelY = 1f / _glowSourceTarget.Height;

        _device.SetRenderTarget(_blurTargetA);
        _device.Clear(Color.Transparent);
        _blurEffect.Parameters["Direction"].SetValue(new Vector2(texelX, 0));
        _spriteBatch.Begin(effect: _blurEffect, blendState: BlendState.Opaque, samplerState: SamplerState.LinearClamp);
        _spriteBatch.Draw(_glowSourceTarget, Vector2.Zero, Color.White);
        _spriteBatch.End();

        _device.SetRenderTarget(_blurTargetB);
        _device.Clear(Color.Transparent);
        _blurEffect.Parameters["Direction"].SetValue(new Vector2(0, texelY));
        _spriteBatch.Begin(effect: _blurEffect, blendState: BlendState.Opaque, samplerState: SamplerState.LinearClamp);
        _spriteBatch.Draw(_blurTargetA, Vector2.Zero, Color.White);
        _spriteBatch.End();

        // Pass 4: composite scene + blurred glow, apply vignette/scanlines, to the backbuffer.
        _device.SetRenderTarget(null);
        _device.Clear(Color.Black);
        // _finalCompositeEffect.Parameters["ScreenHeight"].SetValue((float)_device.PresentationParameters.BackBufferHeight);
        _finalCompositeEffect.Parameters["GlowSampler"].SetValue(_blurTargetB);
        _spriteBatch.Begin(effect: _finalCompositeEffect, blendState: BlendState.Opaque, samplerState: SamplerState.LinearClamp);
        _spriteBatch.Draw(_sceneTarget, Vector2.Zero, Color.White);
        _spriteBatch.End();
    }
}