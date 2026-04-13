using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace project_Telephone_directory.Controls;

public sealed class ShaderBackgroundView : SKCanvasView
{
    public static readonly BindableProperty ProfileProperty = BindableProperty.Create(
        nameof(Profile),
        typeof(ShaderProfile),
        typeof(ShaderBackgroundView),
        ShaderProfile.MainGradient,
        propertyChanged: OnProfileChanged);

    public static readonly BindableProperty TypingIntensityProperty = BindableProperty.Create(
        nameof(TypingIntensity),
        typeof(double),
        typeof(ShaderBackgroundView),
        0d);

    public static readonly BindableProperty ValidationStrengthProperty = BindableProperty.Create(
        nameof(ValidationStrength),
        typeof(double),
        typeof(ShaderBackgroundView),
        1d,
        propertyChanged: OnShaderInputBindingChanged);

    private IDispatcherTimer? _animationTimer;
    private SKRuntimeEffect? _effect;
    private DateTime _start = DateTime.UtcNow;
    private SKPoint _pointer = new(0.5f, 0.5f);

    public ShaderBackgroundView()
    {
        EnableTouchEvents = true; // для MainGradient; остальные профили отключают в ApplyProfile
        // Меньше пикселей на высоком DPI — легче для GPU; координаты совпадают с layout.
        IgnorePixelScaling = true;
        PaintSurface += OnPaintSurface;
        Touch += OnTouch;

        Loaded += OnViewLoaded;
        Unloaded += OnViewUnloaded;
    }

    public ShaderProfile Profile
    {
        get => (ShaderProfile)GetValue(ProfileProperty);
        set => SetValue(ProfileProperty, value);
    }

    public double TypingIntensity
    {
        get => (double)GetValue(TypingIntensityProperty);
        set => SetValue(TypingIntensityProperty, value);
    }

    public double ValidationStrength
    {
        get => (double)GetValue(ValidationStrengthProperty);
        set => SetValue(ValidationStrengthProperty, value);
    }

    private void OnViewLoaded(object? sender, EventArgs e)
    {
        EnsureAnimationTimer();
        ApplyProfile(Profile);
    }

    private void OnViewUnloaded(object? sender, EventArgs e)
    {
        StopAnimationTimer();
    }

    private void EnsureAnimationTimer()
    {
        if (_animationTimer != null)
            return;

        var dispatcher = Dispatcher ?? Application.Current?.Dispatcher;
        if (dispatcher == null)
            return;

        _animationTimer = dispatcher.CreateTimer();
        _animationTimer.Interval = GetAnimationInterval(Profile);
        _animationTimer.Tick += OnAnimationTick;
        _animationTimer.Start();
    }

    private static void OnShaderInputBindingChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is ShaderBackgroundView view)
            view.InvalidateSurface();
    }

    /// <summary>Интервалы перерисовки: выше FPS — быстрее смена uTime; форма редактирования остаётся реже, чтобы не мешать вводу.</summary>
    private static TimeSpan GetAnimationInterval(ShaderProfile profile) =>
        profile switch
        {
            ShaderProfile.AddFormDynamic => TimeSpan.FromMilliseconds(240),
            ShaderProfile.ContactDetailAmbient => TimeSpan.FromMilliseconds(1000.0 / 24.0),
            _ => TimeSpan.FromMilliseconds(1000.0 / 30.0)
        };

    private void SyncAnimationInterval()
    {
        if (_animationTimer != null)
            _animationTimer.Interval = GetAnimationInterval(Profile);
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        if (!IsVisible)
            return;
        InvalidateSurface();
    }

    private void StopAnimationTimer()
    {
        if (_animationTimer == null)
            return;

        _animationTimer.Stop();
        _animationTimer.Tick -= OnAnimationTick;
        _animationTimer = null;
    }

    private static void OnProfileChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ShaderBackgroundView view)
            view.ApplyProfile((ShaderProfile)newValue);
    }

    private void ApplyProfile(ShaderProfile profile)
    {
        EnableTouchEvents = profile == ShaderProfile.MainGradient;

        _effect?.Dispose();
        _effect = profile switch
        {
            ShaderProfile.MainGradient => ShaderRuntime.TryCreateMain(out _),
            ShaderProfile.SearchReactive => ShaderRuntime.TryCreateSearch(out _),
            ShaderProfile.ContactSoft => ShaderRuntime.TryCreateSoft(out _),
            ShaderProfile.AddFormDynamic => ShaderRuntime.TryCreateAddForm(out _),
            ShaderProfile.SettingsAmbient => ShaderRuntime.TryCreateSettings(out _),
            ShaderProfile.ContactDetailAmbient => ShaderRuntime.TryCreateDetail(out _),
            _ => ShaderRuntime.TryCreateMain(out _)
        };

        SyncAnimationInterval();
        InvalidateSurface();
    }

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        if (e.ActionType == SKTouchAction.Pressed || e.ActionType == SKTouchAction.Moved)
        {
            var w = Width;
            var h = Height;
            if (w > 0 && h > 0)
            {
                _pointer = new SKPoint((float)(e.Location.X / w), (float)(e.Location.Y / h));
                InvalidateSurface();
            }
        }

        e.Handled = true;
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Black);

        try
        {
            if (_effect == null)
            {
                DrawFallback(canvas, e.Info.Rect);
                return;
            }

            var info = e.Info;
            using var uniforms = new SKRuntimeEffectUniforms(_effect);
            uniforms["uResolution"] = new[] { (float)info.Width, (float)info.Height };
            var elapsed = (float)(DateTime.UtcNow - _start).TotalSeconds;
            uniforms["uTime"] = elapsed;

            if (Profile == ShaderProfile.MainGradient)
            {
                uniforms["uPointer"] = new[] { _pointer.X, _pointer.Y };
            }
            else if (Profile == ShaderProfile.SearchReactive)
            {
                uniforms["uTyping"] = (float)Math.Clamp(TypingIntensity, 0, 1);
            }
            else if (Profile == ShaderProfile.AddFormDynamic)
            {
                uniforms["uValid"] = (float)Math.Clamp(ValidationStrength, 0, 1);
            }

            // Нельзя передавать null во второй аргумент: ToShader(uniforms, children)
            // внутри вызывает children.ToArray() и падает с NRE (Android/JavaProxyThrowable).
            using var shader = _effect.ToShader(uniforms);
            using var paint = new SKPaint { Shader = shader };
            canvas.DrawRect(info.Rect, paint);
        }
        catch
        {
            DrawFallback(canvas, e.Info.Rect);
        }
    }

    private static void DrawFallback(SKCanvas canvas, SKRectI rect)
    {
        using var fallback = new SKPaint { Color = new SKColor(20, 24, 40) };
        canvas.DrawRect(rect, fallback);
    }
}
