# Телефонный справочник

Кроссплатформенное приложение на **.NET MAUI** для ведения локального справочника контактов: имя, телефон, email, заметки, фото (аватар). Данные хранятся в **SQLite** на устройстве; на Android доступна работа с контактами устройства (по возможностям платформы).

## Возможности

- Вкладки: **Главная**, **Контакты**, **Поиск**, **Настройки**
- Просмотр карточки контакта, добавление и редактирование, удаление
- Поиск по полям контакта
- Оформление: анимированные фоны (SkiaSharp), в ряде экранов — встраиваемые 3D-модели Sketchfab в WebView

## Требования к среде разработки

- **Visual Studio 2022 или новее** (в том числе **Visual Studio 2026**, если у вас установлена эта версия)
- Рабочая нагрузка (workload) **Разработка мобильных приложений на .NET** (включает MAUI, Android SDK, при разработке под iOS — связка с Mac при необходимости)
- Для **Windows**: Windows 10 версии 1809 (17763) или новее
- Для **Android**: Android SDK (ставится с VS), эмулятор или физическое устройство с USB-отладкой
- Установленный **.NET SDK**, соответствующий целевым framework проекта (в проекте указаны `net10.0-android`, `net10.0-windows…`, при сборке не на Linux также `net10.0-ios` и `net10.0-maccatalyst`)

## Структура репозитория

- `project_Telephone_directory/` — основной проект MAUI (файл `project_Telephone_directory.csproj`)

Откройте в Visual Studio **папку репозитория** (**File → Open → Folder…**) или дважды щёлкните по **`.csproj`**, если решения (`.sln`) нет.

## Запуск и отладка в Visual Studio

1. Откройте проект (папку или `.csproj`).
2. В верхней панели выберите **конфигурацию** (обычно **Debug**) и **целевой framework**:
   - для ПК: **net10.0-windows10.0.19041.0** (или отображаемое имя вроде **Windows Machine**);
   - для телефона Android: **net10.0-android** и устройство/эмулятор в списке рядом.
3. Нажмите **F5** (или **Debug → Start Debugging**) для запуска с отладчиком, **Ctrl+F5** — запуск без отладки.

### Запуск на ПК (Windows)

- Выберите целевой фреймворк **Windows** и запустите проект. При `WindowsPackageType=None` приложение собирается как **unpackaged** и запускается как обычный WinUI-процесс из выходного каталога сборки.

### Запуск на телефоне (Android)

1. Включите на устройстве **режим разработчика** и **отладку по USB** (или используйте эмулятор из Android Device Manager в Visual Studio).
2. В списке устройств выберите телефон или эмулятор.
3. Установите целевой фреймворк **Android** и нажмите **F5** — APK установится и запустится на устройстве.

Первый запуск Android-сборки может занять несколько минут (Gradle, пакеты SDK).

## Сборка из командной строки

Из каталога с файлом `project_Telephone_directory.csproj`:

```powershell
cd project_Telephone_directory

# Сборка под Windows (на машине с Windows)
dotnet build -f net10.0-windows10.0.19041.0 -c Debug

# Сборка под Android (нужен установленный Android SDK / workload)
dotnet build -f net10.0-android -c Debug
```

Запуск (после успешной сборки), пример для Windows:

```powershell
dotnet run -f net10.0-windows10.0.19041.0 -c Debug
```

Для Android чаще удобнее развёртывание через Visual Studio или `dotnet build` с последующей установкой APK из артефактов сборки.

## Публикация (кратко)

- **Windows**: можно собрать Release и распространять артефакты сборки; для Microsoft Store нужна отдельная упаковка (в проекте сейчас `WindowsPackageType=None`).
- **Android**: Release-сборка, подпись keystore, загрузка в Google Play — по [документации MAUI](https://learn.microsoft.com/dotnet/maui/).

## Полезные ссылки

- [Документация .NET MAUI](https://learn.microsoft.com/dotnet/maui/)
- [Развертывание на Android](https://learn.microsoft.com/dotnet/maui/android/deployment/)
- [Развертывание на Windows](https://learn.microsoft.com/dotnet/maui/windows/deployment/)

---

*Если в Visual Studio не отображаются целевые framework’ы MAUI, проверьте установку рабочей нагрузки «Разработка мобильных приложений на .NET» в установщике Visual Studio.*
