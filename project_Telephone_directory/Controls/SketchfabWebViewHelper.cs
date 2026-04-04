namespace project_Telephone_directory.Controls;

/// <summary>
/// WebView2 в MAUI на Windows часто не отрисовывает внешний iframe из локального HTML.
/// Прямая навигация на URL embed обычно работает стабильнее.
/// </summary>
public static class SketchfabWebViewHelper
{
    public static void LoadModel(WebView webView, string modelUid, double heightRequest)
    {
        webView.HeightRequest = heightRequest;
        webView.Source = new UrlWebViewSource
        {
            Url = SketchfabConstants.GetDirectEmbedUrl(modelUid)
        };
    }
}
