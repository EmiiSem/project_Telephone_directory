namespace project_Telephone_directory.Controls;

/// <summary>
/// Для подавления дефолтного UI Sketchfab используем HtmlWebViewSource c iframe.
/// </summary>
public static class SketchfabWebViewHelper
{
    public static void LoadModel(WebView webView, string modelUid, double heightRequest)
    {
        webView.HeightRequest = heightRequest;
        webView.Source = new HtmlWebViewSource
        {
            Html = SketchfabConstants.BuildEmbedHtml(modelUid, (int)Math.Round(heightRequest))
        };
    }

    /// <summary>Отпускает встроенный WebView2 при уходе со страницы — иначе несколько вкладок держат сотни МБ RAM.</summary>
    public static void Clear(WebView webView)
    {
        webView.Source = null;
    }
}
