namespace project_Telephone_directory.Controls;

public enum ShaderProfile
{
    /// <summary>Главная — бегущая сетка «плиток» + реакция на касание.</summary>
    MainGradient,

    /// <summary>Поиск — горизонтальные «сканирующие» полосы и сетка точек.</summary>
    SearchReactive,

    /// <summary>Список контактов — вертикальные бегущие полосы.</summary>
    ContactSoft,

    /// <summary>Форма редактирования — диагональные полосы + uValid.</summary>
    AddFormDynamic,

    /// <summary>Настройки — «ромбовая» сетка с медленным сдвигом.</summary>
    SettingsAmbient,

    /// <summary>Карточка контакта — мягкая волна + крупная сетка.</summary>
    ContactDetailAmbient
}
