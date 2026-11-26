# Battle City

Консольная игра аналог игры Battle City с NES на C# и .NET 10. Игрок управляет танком на игровом поле, защищая базу от противников.

## Возможности

- Игровое поле с кирпичными и стальными препятствиями и водой.
- Движение танка, стрельба, столкновения и получение урона.
- Противники, очки, жизни и условия для победы или поражения.
- Пауза и сохранение/загрузка состояния игры.
- Модульные тесты MSTest для игровых классов и основных сценариев.

## Требования

- .NET 10 SDK.
- Терминал с поддержкой UTF-8 и ANSI-последовательностей.
- Для игрового экрана рекомендуется окно не меньше 70 столбцов на 31 строку. При уменьшении окна во время игры появится подсказка, чтобы продолжить игру, нужно увеличить окно.

## Запуск игры

Из корня репозитория выполните:

```powershell
dotnet run --project .\BattleCity\BattleCity.csproj
```

В главном меню:

- `1` — начать новую игру;
- `2` — загрузить сохранение;
- `3` — выйти.

## Управление

| Клавиши | Действие |
|---|---|
| `W`, `A`, `S`, `D` или стрелки | Перемещение танка |
| `Пробел` | Выстрел |
| `P` | Пауза или продолжение |
| `F5` | Сохранить игру |
| `F9` | Загрузить игру |
| `Q` | Сохранить игру и вернуться в меню |

Состояние игры сохраняется в `savegame.json` в текущей рабочей папке.

## Сборка и тесты

Собрать всё решение:

```powershell
dotnet build .\BattleCity\BattleCity.sln
```

Запустить тесты:

```powershell
dotnet test .\BattleCity\BattleCity.sln
```

Тесты находятся в проекте `BattleCity.Tests` и используют MSTest.

## Сборка исполняемого exe файла для Windows x64

Создать автономный exe:

```powershell
dotnet publish .\BattleCity\BattleCity.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o .\BattleCity\publish
```

После создания приложение находится в `BattleCity\publish\BattleCity.exe`. Автономная публикация включает .NET Runtime.

## Структура репозитория

```text
Lab3/
├── BattleCity/
│   ├── Models/
|   ├── publish/
│   ├── BattleCity.csproj
│   ├── BattleCity.sln
│   ├── GameSerializer.cs
│   └── Program.cs
├── BattleCity.Tests/
│   ├── TestResults/
│   ├── BattleCity.Tests.csproj
│   ├── CoreTests.cs
│   ├── MSTestSettings.cs
│   └── Test1.cs
└── README.md
```
