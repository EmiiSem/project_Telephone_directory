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

    /// <summary>Карточка контакта — конверт (3D в шапке).</summary>
    public const string Envelope = "73faa986225f4241809b922f92a48884";

    /// <summary>
    /// Прямая ссылка на страницу embed.
    /// Все известные флаги ui_* выключены (см. Sketchfab Viewer embed options).
    /// Важно: скрытие watermark и панелей управления по правилам Sketchfab доступно, если у владельца модели есть Premium+;
    /// иначе плеер может игнорировать эти параметры — тогда срабатывает лёгкий zoom+clip в <see cref="BuildEmbedHtml"/>.
    /// </summary>
    public static string GetDirectEmbedUrl(string modelUid)
    {
        var uid = string.IsNullOrWhiteSpace(modelUid) ? PhoneOrBook : modelUid.Trim();
        return "https://sketchfab.com/models/" + uid +
               "/embed?autostart=1&autospin=0.35&preload=1&camera=0&transparent=1&dnt=1" +
               "&annotations_visible=0&annotation_tooltip_visible=0" +
               "&ui_theme=dark&ui_hint=0&ui_fadeout=1" +
               "&ui_controls=0&ui_general_controls=0" +
               "&ui_infos=0&ui_watermark=0&ui_watermark_link=0" +
               "&ui_annotations=0&ui_animations=0" +
               "&ui_help=0&ui_inspector=0&ui_settings=0&ui_stop=0" +
               "&ui_loading=0&ui_start=0" +
               "&ui_ar=0&ui_vr=0&ui_ar_help=0&ui_ar_qrcode=0&ui_fullscreen=0";
    }

    public static string BuildEmbedHtml(string modelUid, int height = 220)
    {
        var uid = string.IsNullOrWhiteSpace(modelUid) ? PhoneOrBook : modelUid.Trim();
        var url = GetDirectEmbedUrl(uid).Replace("&", "&amp;", StringComparison.Ordinal);
        // Небольшой zoom с обрезкой: убирает типичные угловые кнопки/watermark, если Sketchfab не применил ui_* (бесплатный embed).
        const string iframeCss =
            "position:absolute;left:50%;top:50%;width:100%;height:100%;border:0;background:transparent;" +
            "transform:translate(-50%,-50%) scale(1.34);transform-origin:center center;";
        return
            "<!DOCTYPE html><html><head>" +
            "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />" +
            "<style>html,body{margin:0;padding:0;background:transparent!important;overflow:hidden;height:100%;}" +
            ".wrap{position:relative;width:100%;height:" + height + "px;overflow:hidden;background:transparent;border-radius:18px;}" +
            "iframe{" + iframeCss + "}" +
            "</style>" +
            "</head><body>" +
            "<div class=\"wrap\">" +
            "<iframe title=\"Sketchfab\" allow=\"autoplay; fullscreen; xr-spatial-tracking\" allowfullscreen " +
            "mozallowfullscreen=\"true\" webkitallowfullscreen=\"true\" " +
            "src=\"" + url + "\"></iframe>" +
            "</div>" +
            "</body></html>";
    }
}
