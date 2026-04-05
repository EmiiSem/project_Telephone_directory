using SkiaSharp;

namespace project_Telephone_directory.Controls;

internal static class ShaderRuntime
{
    internal static SKRuntimeEffect? TryCreateMain(out string? error)
    {
        const string sksl =
            "uniform float2 uResolution;" +
            "uniform float  uTime;" +
            "uniform float2 uPointer;" +
            "float hash(float2 p) { return fract(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123); }" +
            "float noise(float2 p) {" +
            "  float2 i = floor(p);" +
            "  float2 f = fract(p);" +
            "  float a = hash(i);" +
            "  float b = hash(i + float2(1.0, 0.0));" +
            "  float c = hash(i + float2(0.0, 1.0));" +
            "  float d = hash(i + float2(1.0, 1.0));" +
            "  float2 u = f * f * (3.0 - 2.0 * f);" +
            "  return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);" +
            "}" +
            "half4 main(float2 fragCoord) {" +
            "  float2 uv = fragCoord / uResolution;" +
            "  float t = uTime * 0.28;" +
            "  float2 p = uv * 3.0 + float2(t * 0.2, -t * 0.15);" +
            "  float n = noise(p);" +
            "  float3 c1 = float3(0.12, 0.08, 0.35);" +
            "  float3 c2 = float3(0.95, 0.35, 0.45);" +
            "  float3 c3 = float3(0.25, 0.55, 0.95);" +
            "  float w1 = sin(t + uv.x * 6.2831) * 0.5 + 0.5;" +
            "  float w2 = cos(t * 1.3 + uv.y * 6.2831) * 0.5 + 0.5;" +
            "  float3 col = mix(c1, c2, uv.x + n * 0.25);" +
            "  col = mix(col, c3, w1 * 0.35 + w2 * 0.25);" +
            "  float pulse = length(uv - uPointer);" +
            "  col += float3(0.08, 0.12, 0.18) * exp(-pulse * 6.0);" +
            "  return half4(col, 1.0);" +
            "}";

        return global::SkiaSharp.SKRuntimeEffect.CreateShader(sksl, out error);
    }

    internal static SKRuntimeEffect? TryCreateSearch(out string? error)
    {
        const string sksl =
            "uniform float2 uResolution;" +
            "uniform float  uTime;" +
            "uniform float  uTyping;" +
            "half4 main(float2 fragCoord) {" +
            "  float2 uv = fragCoord / uResolution;" +
            "  float2 p = (uv - 0.5) * float2(uResolution.x / uResolution.y, 1.0);" +
            "  float len = length(p);" +
            "  float wave = sin(len * 12.0 - uTime * 2.5 + uTyping * 8.0) * 0.5 + 0.5;" +
            "  float3 baseCol = float3(0.05, 0.09, 0.18);" +
            "  float3 accent = float3(0.25, 0.75, 0.95);" +
            "  float3 col = mix(baseCol, accent, wave * 0.35 + uTyping * 0.25);" +
            "  col += float3(0.05) * exp(-len * 3.0);" +
            "  return half4(col, 1.0);" +
            "}";

        return global::SkiaSharp.SKRuntimeEffect.CreateShader(sksl, out error);
    }

    internal static SKRuntimeEffect? TryCreateSoft(out string? error)
    {
        const string sksl =
            "uniform float2 uResolution;" +
            "uniform float  uTime;" +
            "half4 main(float2 fragCoord) {" +
            "  float2 uv = fragCoord / uResolution;" +
            "  float t = uTime * 0.2;" +
            "  float3 c1 = float3(0.10, 0.12, 0.18);" +
            "  float3 c2 = float3(0.22, 0.30, 0.55);" +
            "  float3 c3 = float3(0.45, 0.35, 0.65);" +
            "  float w = sin(uv.x * 3.1415 + t) * 0.5 + 0.5;" +
            "  float w2 = cos(uv.y * 3.1415 - t * 0.8) * 0.5 + 0.5;" +
            "  float3 col = mix(c1, c2, w * 0.6 + w2 * 0.2);" +
            "  col = mix(col, c3, (uv.x + uv.y) * 0.15);" +
            "  float glow = exp(-distance(uv, float2(0.5)) * 3.0) * 0.08;" +
            "  col += glow;" +
            "  return half4(col, 1.0);" +
            "}";

        return global::SkiaSharp.SKRuntimeEffect.CreateShader(sksl, out error);
    }

    internal static SKRuntimeEffect? TryCreateAddForm(out string? error)
    {
        const string sksl =
            "uniform float2 uResolution;" +
            "uniform float  uTime;" +
            "uniform float  uValid;" +
            "half4 main(float2 fragCoord) {" +
            "  float2 uv = fragCoord / uResolution;" +
            "  float t = uTime * 0.6;" +
            "  float border = sin(uv.x * 26.0 + t) * 0.5 + 0.5;" +
            "  float3 ok = float3(0.15, 0.55, 0.35);" +
            "  float3 bad = float3(0.55, 0.18, 0.22);" +
            "  float3 baseCol = float3(0.08, 0.09, 0.14);" +
            "  float3 accent = mix(bad, ok, uValid);" +
            "  float3 col = mix(baseCol, accent, border * 0.12 + uValid * 0.08);" +
            "  col += float3(0.04) * sin(t * 0.85 + uv.y * 14.0);" +
            "  return half4(col, 1.0);" +
            "}";

        return global::SkiaSharp.SKRuntimeEffect.CreateShader(sksl, out error);
    }
}
