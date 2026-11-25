using System;
using System.Text;
using System.Threading.Tasks;
using BattleCity.Models;

namespace BattleCity
{
    internal class Program
    {
        private static Game game;
        private static string notificationMessage = "";
        private static int notificationTicks = 0;

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;

            while (true)
            {
                Console.Clear();

                Console.WriteLine();
                Console.WriteLine("BATTLE CITY");
                Console.WriteLine();
                Console.WriteLine("1. Новая игра");
                Console.WriteLine("2. Продолжить игру");
                Console.WriteLine("3. Выход");
                Console.WriteLine();

                Console.Write("Выберите действие: ");

                ConsoleKey key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.D1 ||
                    key == ConsoleKey.NumPad1)
                {
                    game = new Game();
                    await RunGame();
                }
                else if (key == ConsoleKey.D2 ||
                         key == ConsoleKey.NumPad2)
                {
                    if (GameSerializer.SaveExists())
                    {
                        game = GameSerializer.Load();

                        if (game != null)
                            await RunGame();
                        else
                            ShowMessage("Не удалось загрузить игру.");
                    }
                    else
                    {
                        ShowMessage("Сохранение не найдено.");
                    }
                }
                else if (key == ConsoleKey.D3 ||
                         key == ConsoleKey.NumPad3)
                {
                    break;
                }
            }

            Console.CursorVisible = true;
            Console.Clear();
        }

        // Основной игровой цикл
        private static async Task RunGame()
        {
            Console.Clear();

            while (game.Status == GameStatus.Playing || game.Status == GameStatus.Paused)
            {
                bool continueGame = ProcessInput();

                if (!continueGame)
                    return;

                game.Update();

                Render();

                if (notificationTicks > 0)
                {
                    notificationTicks--;
                }

                await Task.Delay(100);
            }

            Render();

            Console.SetCursorPosition(0, 25);

            if (game.Status == GameStatus.Won)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("ПОБЕДА, ИГРА ЗАВЕРШЕНА");
            }
            else if (game.Status == GameStatus.Lost)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ПОРАЖЕНИЕ, ИГРА ЗАВЕРШЕНА");
            }
            Console.ResetColor();

            Console.WriteLine("Нажмите любую клавишу...");

            Console.ReadKey();

            if (game.Status == GameStatus.Paused)
                return;
        }

        // Обработка нажатий клавиш
        private static bool ProcessInput()
        {
            while (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.P:
                        if (game.Status == GameStatus.Playing)
                            game.Status = GameStatus.Paused;
                        else if (game.Status == GameStatus.Paused)
                            game.Status = GameStatus.Playing;

                        continue;

                    case ConsoleKey.F5:
                        if (GameSerializer.Save(game))
                            ShowNotification("Успешно сохранено");
                        else
                            ShowNotification("Ошибка сохранения");

                        continue;

                    case ConsoleKey.F9:
                        Game loadedGame = GameSerializer.Load();

                        if (loadedGame != null)
                        {
                            game = loadedGame;
                            ShowNotification("Успешно загружено");
                        }
                        else
                        {
                            ShowNotification("Ошибка загрузки");
                        }

                        continue;

                    case ConsoleKey.Q:
                        if (GameSerializer.Save(game))
                            ShowNotification("Успешно сохранено");
                        else
                            ShowNotification("Ошибка сохранения");

                        return false;
                }

                if (game.Status != GameStatus.Playing)
                    continue;

                switch (key)
                {
                    case ConsoleKey.W:
                    case ConsoleKey.UpArrow:
                        game.MovePlayer(Direction.Up);
                        break;

                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        game.MovePlayer(Direction.Down);
                        break;

                    case ConsoleKey.A:
                    case ConsoleKey.LeftArrow:
                        game.MovePlayer(Direction.Left);
                        break;

                    case ConsoleKey.D:
                    case ConsoleKey.RightArrow:
                        game.MovePlayer(Direction.Right);
                        break;

                    case ConsoleKey.Spacebar:
                        game.Shoot(game.Player);
                        break;
                }
            }

            return true;
        }

        // Отрисовка игрового поля
        private static void Render()
        {
            Console.SetCursorPosition(0, 1);

            int width = game.Field.Width;
            int height = game.Field.Height;

            char[,] map = new char[height, width];
            ConsoleColor[,] colors = new ConsoleColor[height, width];

            // Пустое поле
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[y, x] = ' ';
                    colors[y, x] = ConsoleColor.Gray;
                }
            }

            // Препятствия
            foreach (Obstacle obstacle in game.Field.Obstacles)
            {
                if (obstacle.IsDestroyed)
                    continue;

                map[obstacle.Y, obstacle.X] = obstacle.Type switch
                {
                    "Brick" => '#',
                    "Steel" => 'H',
                    "Water" => '~',
                    _ => '?'
                };

                colors[obstacle.Y, obstacle.X] = obstacle.Type switch
                {
                    "Brick" => ConsoleColor.DarkYellow,
                    "Steel" => ConsoleColor.Gray,
                    "Water" => ConsoleColor.Cyan,
                    _ => ConsoleColor.White
                };
            }

            // База
            map[game.Field.BaseY, game.Field.BaseX] =
                game.BaseAlive ? 'B' : 'X';

            colors[game.Field.BaseY, game.Field.BaseX] =
                game.BaseAlive ? ConsoleColor.Green : ConsoleColor.Red;

            // Танк игрока
            if (game.Player.IsAlive)
            {
                map[game.Player.Y, game.Player.X] =
                    GetTankSymbol(game.Player.Direction);

                colors[game.Player.Y, game.Player.X] =
                    ConsoleColor.Green;
            }

            // Вражеские танки
            foreach (Tank enemy in game.Enemies)
            {
                if (!enemy.IsAlive)
                    continue;

                map[enemy.Y, enemy.X] =
                    GetTankSymbol(enemy.Direction);

                colors[enemy.Y, enemy.X] =
                    ConsoleColor.Red;
            }

            // Снаряды
            foreach (Bullet bullet in game.Bullets)
            {
                if (!bullet.IsActive)
                    continue;

                map[bullet.Y, bullet.X] = '*';

                colors[bullet.Y, bullet.X] =
                    ConsoleColor.Yellow;
            }

            // Верхняя граница
            Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.Write("+");

            Console.Write(new string('-', width));

            Console.WriteLine("+");

            // Игровое поле
            for (int y = 0; y < height; y++)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("|");

                for (int x = 0; x < width; x++)
                {
                    Console.ForegroundColor = colors[y, x];
                    Console.Write(map[y, x]);
                }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("|");
            }

            // Нижняя граница
            Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.Write("+");

            Console.Write(new string('-', width));

            Console.WriteLine("+");

            // Информация об игре
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(
                $"Жизни: {game.Lives} | " +
                $"Здоровье: {game.Player.Health} | " +
                $"Очки: {game.Score}".PadRight(70));

            Console.WriteLine(
                $"Уничтожено противников: " +
                $"{game.EnemiesDestroyed}/{game.TotalEnemies}".PadRight(70));

            Console.Write("База: ");

            Console.ForegroundColor = game.BaseAlive
                ? ConsoleColor.Green
                : ConsoleColor.Red;

            Console.WriteLine(
                (game.BaseAlive ? "Цела" : "Уничтожена").PadRight(60));

            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine(
                "Управление: WASD / стрелки - движение".PadRight(70));

            Console.WriteLine(
                "Пробел - выстрел | P - пауза | Q - выход".PadRight(70));

            Console.WriteLine(
                "F5 - сохранить | F9 - загрузить".PadRight(70));

            Console.SetCursorPosition(0, 28);
            if (game.Status == GameStatus.Paused)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine(
                    "             ИГРА ПРИОСТАНОВЛЕНА".PadRight(70));
            }
            else
            {
                Console.WriteLine(new string(' ', 70));
            }

            Console.ResetColor();

            Console.SetCursorPosition(0, 29);

            if (notificationTicks > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(notificationMessage.PadRight(70));
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(new string(' ', 70));
            }
        }

        // Символ танка в зависимости от направления
        private static char GetTankSymbol(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return '^';

                case Direction.Down:
                    return 'v';

                case Direction.Left:
                    return '<';

                case Direction.Right:
                    return '>';

                default:
                    return '^';
            }
        }

        private static void ShowMessage(string message)
        {
            Console.Clear();
            Console.WriteLine(message);
            Console.WriteLine("Нажмите любую клавишу...");
            Console.ReadKey(true);
        }

        private static void ShowNotification(string message)
        {
            notificationMessage = message;
            notificationTicks = 20;
        }
    }
}