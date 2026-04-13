using SkiaSharp;

namespace project_Telephone_directory.Controls;

internal static class ShaderRuntime
{
    /// <summary>Бегущая «кирпичная» сетка + подсветка у uPointer.</summary>
    internal static SKRuntimeEffect? TryCreateMain(out string? error)
    {
        const string sksl =
            "uniform float2 uResolution;" +
            "uniform float  uTime;" +
            "uniform float2 uPointer;" +
            "half4 main(float2 fragCoord) {" +
            "  float2 uv = fragCoord / uResolution;" +
            "  float aspect = uResolution.x / max(uResolution.y, 1.0);" +
            "  float2 p = float2(uv.x * aspect, uv.y) * 12.0;" +
            "  float t = uTime * 0.58;" +
            "  p.x += t * 1.05;" +
            "  p.y += t * 0.36;" +
            "  float2 id = floor(p);" +
            "  float2 f = fract(p) - 0.5;" +
            "  float edge = max(abs(f.x), abs(f.y));" +
            "  float tile = smoothstep(0.42, 0.48, edge);" +
            "  float pat = fract(0.5 * (id.x + id.y)) * 2.0;" +
            "  float3 ca = float3(0.12, 0.07, 0.30);" +
            "  float3 cb = float3(0.26, 0.11, 0.40);" +
            "  float3 col = mix(ca, cb, pat * (1.0 - tile * 0.55) + tile * 0.12);" +
            "  float3 hi = float3(0.30, 0.50, 0.92);" +
            "  col = mix(col, hi, tile * 0.22);" +
            "  float2 pu = uv - uPointer;" +
            "  col += float3(0.07, 0.10, 0.16) * exp(-dot(pu, pu) * 9.0);" +
            "  return half4(col, 1.0);" +
            "}";

        return global::SkiaSharp.SKRuntimeEffect.CreateShader(sksl, out error);
    }

    /// <summary>Горизонтальные «бегущие» полосы + сетка точек + реакция на uTyping.</summary>
    internal static SKRuntimeEffect? TryCreateSearch(out string? error)
    {
        const string sksl =
            "uniform float2 uResolution;" +
            "uniform float  uTime;" +
            "uniform float  uTyping;" +
            "half4 main(float2 fragCoord) {" +
            "  float2 uv = fragCoord / uResolution;" +
            "  float t = uTime * 1.4;" +
            "  float scan = sin(uv.y * 72.0 - t * 8.0) * 0.5 + 0.5;" +
            "  float2 g = fract(uv * float2(22.0, 13.0) + float2(t * 0.92, t * 0.2));" +
            "  float dots = step(0.93, length(g - 0.5));" +
            "  float2 c = (uv - 0.5) * float2(uResolution.x / max(uResolution.y, 1.0), 1.0);" +
            "  float ring = sin(length(c) * 11.0 - t * 3.6 + uTyping * 5.5) * 0.5 + 0.5;" +
            "  float3 baseCol = float3(0.04, 0.07, 0.15);" +
            "  float3 acc = float3(0.18, 0.62, 0.92);" +
            "  float3 col = mix(baseCol, acc, scan * 0.14 + ring * 0.22 + uTyping * 0.22);" +
            "  col += dots * float3(0.12, 0.16, 0.22);" +
            "  return half4(col, 1.0);" +
            "}";

        return global::SkiaSharp.SKRuntimeEffect.CreateShader(sksl, out error);
    }

    /// <summary>Вертикальные «бегущие» полосы (смещение по синусу по Y).</summary>
    internal static SKRuntimeEffect? TryCreateSoft(out string? error)
    {
        const string sksl =
            "uniform float2 uResolution;" +
            "uniform float  uTime;" +
            "half4 main(float2 fragCoord) {" +
            "  float2 uv = fragCoord / uResolution;" +
            "  float t = uTime * 0.92;" +
            "  float cols = 16.0;" +
            "  float x = uv.x * cols + sin(uv.y * 5.0 + t * 2.0) * 0.45 + t * 1.5;" +
            "  float band = fract(x);" +
            "  float stripe = smoothstep(0.06, 0.11, band) * smoothstep(0.89, 0.94, band);" +
            "  float3 c0 = float3(0.07, 0.09, 0.15);" +
            "  float3 c1 = float3(0.32, 0.24, 0.52);" +
            "  float3 c2 = float3(0.18, 0.35, 0.58);" +
            "  float3 col = mix(c0, c1, stripe * 0.75);" +
            "  col = mix(col, c2, stripe * (0.5 + 0.5 * sin(uv.y * 11.0 + t * 1.5)) * 0.25);" +
            "  return half4(col, 1.0);" +
            "}";

        return global::SkiaSharp.SKRuntimeEffect.CreateShader(sksl, out error);
    }

    /// <summary>Диагональные бегущие полосы + оттенок по uValid.</summary>
    internal static SKRuntimeEffect? TryCreateAddForm(out string? error)
    {
        const string sksl =
            "uniform float2 uResolution;" +
            "uniform float  uTime;" +
            "uniform float  uValid;" +
            "half4 main(float2 fragCoord) {" +
            "  float2 uv = fragCoord / uResolution;" +
            "  float t = uTime * 1.4;" +
            "  float d = (uv.x + uv.y) * 22.0 - t * 4.0;" +
            "  float march = abs(fract(d * 0.5) - 0.5) * 2.0;" +
            "  float stripe = smoothstep(0.12, 0.22, march);" +
            "  float3 ok = float3(0.10, 0.22, 0.14);" +
            "  float3 bad = float3(0.20, 0.10, 0.12);" +
            "  float3 baseCol = float3(0.06, 0.07, 0.11);" +
            "  float3 tint = mix(bad, ok, uValid);" +
            "  float3 col = mix(baseCol, tint, stripe * 0.35 + uValid * 0.12);" +
            "  col += float3(0.03) * sin(t * 1.45 + uv.y * 16.0);" +
            "  return half4(col, 1.0);" +
            "}";

        return global::SkiaSharp.SKRuntimeEffect.CreateShader(sksl, out error);
    }

    /// <summary>Ромбовая сетка с медленным скольжением (настройки).</summary>
    internal static SKRuntimeEffect? TryCreateSettings(out string? error)
    {
        const string sksl =
            "uniform float2 uResolution;" +
            "uniform float  uTime;" +
            "half4 main(float2 fragCoord) {" +
            "  float2 uv = fragCoord / uResolution;" +
            "  float t = uTime * 0.4;" +
            "  float2 p = (uv - 0.5) * float2(uResolution.x / max(uResolution.y, 1.0), 1.0) * 9.0;" +
            "  float2 q = float2(p.x + p.y, p.x - p.y) * 0.7071;" +
            "  q.x += t * 1.0;" +
            "  q.y -= t * 0.58;" +
            "  float2 f = fract(q) - 0.5;" +
            "  float cell = abs(f.x) + abs(f.y);" +
            "  float m = smoothstep(0.38, 0.48, cell);" +
            "  float3 a = float3(0.10, 0.05, 0.20);" +
            "  float3 b = float3(0.42, 0.18, 0.50);" +
            "  float3 c = float3(0.22, 0.12, 0.35);" +
            "  float3 col = mix(a, b, m);" +
            "  col = mix(col, c, (1.0 - m) * 0.15);" +
            "  return half4(col, 1.0);" +
            "}";

        return global::SkiaSharp.SKRuntimeEffect.CreateShader(sksl, out error);
    }

    /// <summary>Крупная «плитка» + волна (карточка контакта).</summary>
    internal static SKRuntimeEffect? TryCreateDetail(out string? error)
    {
        const string sksl =
            "uniform float2 uResolution;" +
            "uniform float  uTime;" +
            "half4 main(float2 fragCoord) {" +
            "  float2 uv = fragCoord / uResolution;" +
            "  float t = uTime * 0.52;" +
            "  float2 g = uv * float2(9.0, 9.0 * uResolution.y / max(uResolution.x, 1.0));" +
            "  g += float2(t * 0.26, -t * 0.17);" +
            "  float2 f = fract(g) - 0.5;" +
            "  float edge = max(abs(f.x), abs(f.y));" +
            "  float tile = smoothstep(0.40, 0.48, edge);" +
            "  float wave = sin(uv.x * 6.2831 + uv.y * 4.0 + t * 2.3) * 0.5 + 0.5;" +
            "  float3 c0 = float3(0.06, 0.07, 0.12);" +
            "  float3 c1 = float3(0.12, 0.28, 0.42);" +
            "  float3 c2 = float3(0.25, 0.45, 0.62);" +
            "  float3 col = mix(c0, c1, (1.0 - tile) * 0.6 + wave * 0.2);" +
            "  col = mix(col, c2, tile * 0.25);" +
            "  return half4(col, 1.0);" +
            "}";

        return global::SkiaSharp.SKRuntimeEffect.CreateShader(sksl, out error);
    }
}
