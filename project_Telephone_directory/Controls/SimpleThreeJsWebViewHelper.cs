namespace project_Telephone_directory.Controls;

/// <summary>
/// Мини-viewer на Three.js (cdnjs): примитив + MeshNormalMaterial, прозрачный фон, плавное вращение (на Android — быстрее).
/// </summary>
public static class SimpleThreeJsWebViewHelper
{
    public enum PrimitiveKind
    {
        Cube,
        Torus,
        Octahedron
    }

    public static void LoadPrimitive(WebView webView, PrimitiveKind kind, double heightRequest)
    {
        webView.HeightRequest = heightRequest;
        webView.Source = new HtmlWebViewSource
        {
            Html = BuildHtml(kind, (int)Math.Round(heightRequest))
        };
    }

    public static void Clear(WebView webView) => webView.Source = null;

    // Тор с меньшим числом сегментов — на Android WebView тяжёлый меш сильно снижает FPS.
    private static string GeometryJs(PrimitiveKind kind) =>
        kind switch
        {
            PrimitiveKind.Cube => "new THREE.BoxGeometry(1,1,1)",
            PrimitiveKind.Torus => "new THREE.TorusGeometry(0.72,0.32,12,28)",
            PrimitiveKind.Octahedron => "new THREE.OctahedronGeometry(0.92)",
            _ => "new THREE.BoxGeometry(1,1,1)"
        };

    private static string BuildHtml(PrimitiveKind kind, int heightPx)
    {
        var geom = GeometryJs(kind);
        var hStr = heightPx.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return
            "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/>" +
            "<meta name=\"viewport\" content=\"width=device-width,initial-scale=1,maximum-scale=1,user-scalable=no\"/>" +
            "<style>html,body{margin:0;padding:0;height:" + hStr +
            "px;overflow:hidden;background:transparent;}canvas{width:100%;height:100%;display:block;outline:none;touch-action:none;}</style>" +
            "<script src=\"https://cdnjs.cloudflare.com/ajax/libs/three.js/r134/three.min.js\"></script>" +
            "</head><body><script>" +
            "(function(){var fixedH=" + hStr + ";" +
            "var ua=navigator.userAgent||'';" +
            "var android=/Android/i.test(ua);" +
            "var mobile=/Android|iPhone|iPad|iPod|Mobile|Silk|Kindle/i.test(ua);" +
            "var rx=android?0.018:0.007;var ry=android?0.028:0.011;" +
            "var w=window.innerWidth||320;" +
            "var h=Math.max(window.innerHeight||0,fixedH)||fixedH;" +
            "var scene=new THREE.Scene();" +
            "var camera=new THREE.PerspectiveCamera(50,w/h,0.1,100);camera.position.set(0,0,3.2);" +
            "var pr=mobile?1:Math.min(window.devicePixelRatio||1,2);" +
            "var renderer=new THREE.WebGLRenderer({alpha:true,antialias:!mobile,powerPreference:mobile?'default':'low-power'});" +
            "renderer.setPixelRatio(pr);renderer.setClearColor(0,0);renderer.setSize(w,h);" +
            "document.body.appendChild(renderer.domElement);" +
            "var geom=" + geom + ";" +
            "var mesh=new THREE.Mesh(geom,new THREE.MeshNormalMaterial({flatShading:!!mobile}));scene.add(mesh);" +
            "window.addEventListener('resize',function(){" +
            "w=window.innerWidth||320;h=Math.max(window.innerHeight||0,fixedH)||fixedH;camera.aspect=w/h;camera.updateProjectionMatrix();renderer.setSize(w,h);});" +
            "var vis=true;document.addEventListener('visibilitychange',function(){vis=!document.hidden;});" +
            "function tick(){requestAnimationFrame(tick);if(!vis)return;mesh.rotation.x+=rx;mesh.rotation.y+=ry;renderer.render(scene,camera);}" +
            "tick();})();" +
            "</script></body></html>";
    }
}
