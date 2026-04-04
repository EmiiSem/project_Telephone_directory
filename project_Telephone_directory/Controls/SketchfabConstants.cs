namespace project_Telephone_directory.Controls;

/// <summary>
/// Публичные модели Sketchfab для встраивания (UID из URL вида sketchfab.com/models/&lt;uid&gt;/embed).
/// </summary>
public static class SketchfabConstants
{
    /// <summary>Главная — низкополигональный iPhone (легче грузится во встроенном WebView).</summary>
    public const string PhoneOrBook = "be0b13b395bd4d3ebbfd0f95e46311aa";

    /// <summary>Список / поиск — лупа (emoji-стиль).</summary>
    public const string Magnifier = "c42c2a14d34244f2b3712c09c94befd7";

    /// <summary>Настройки — шестерёнки.</summary>
    public const string Gear = "4c6cbdb5854d4b8ebd59772deff1f728";

    /// <summary>Карточка контакта — телефон.</summary>
    public const string ContactPhone = "3441f993365a4f18af05b319598ff5a5";

    /// <summary>Карточка контакта — конверт.</summary>
    public const string Envelope = "73faa986225f4241809b922f92a48884";

    /// <summary>Прямая ссылка на страницу embed (предпочтительно для WebView).</summary>
    public static string GetDirectEmbedUrl(string modelUid)
    {
        var uid = string.IsNullOrWhiteSpace(modelUid) ? PhoneOrBook : modelUid.Trim();
        return "https://sketchfab.com/models/" + uid +
               "/embed?autostart=1&ui_theme=dark&ui_hint=0&preload=1&transparent=1";
    }

    public static string BuildEmbedHtml(string modelUid, int height = 220)
    {
        var uid = string.IsNullOrWhiteSpace(modelUid) ? PhoneOrBook : modelUid.Trim();
        return
            "<!DOCTYPE html><html><head>" +
            "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />" +
            "<style>html,body{margin:0;padding:0;background:#1a1d2e;overflow:hidden;height:100%;}" +
            "iframe{border:0;width:100%;height:" + height + "px;}</style>" +
            "</head><body>" +
            "<iframe title=\"Sketchfab\" allow=\"autoplay; fullscreen; xr-spatial-tracking\" allowfullscreen " +
            "mozallowfullscreen=\"true\" webkitallowfullscreen=\"true\" " +
            "src=\"https://sketchfab.com/models/" + uid + "/embed?autostart=1&ui_hint=0&ui_theme=dark\"></iframe>" +
            "</body></html>";
    }
}
