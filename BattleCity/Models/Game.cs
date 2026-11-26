using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleCity.Models
{
    public enum GameStatus
    {
        Playing,
        Paused,
        Won,
        Lost
    }

    public class Game
    {
        private static readonly Random Random = new Random();

        public GameField Field { get; set; } = null!;
        public Tank Player { get; set; } = null!;

        public List<Tank> Enemies { get; set; } = null!;
        public List<Bullet> Bullets { get; set; } = null!;

        public int Lives { get; set; }
        public int Score { get; set; }

        public int EnemiesDestroyed { get; set; }
        public int EnemiesSpawned { get; set; }
        public int TotalEnemies { get; set; }

        public int TickCounter { get; set; }
        public int SpawnTimer { get; set; }

        public bool BaseAlive { get; set; }

        public GameStatus Status { get; set; }

        public Game()
        {
            Initialize();
        }

        // Создать новое игровое поле и сбросить состояние игры.
        public void Initialize()
        {
            Field = new GameField(30, 20);

            Player = new Tank(15, 15, true);

            Enemies = new List<Tank>();
            Bullets = new List<Bullet>();

            Lives = 3;
            Score = 0;

            EnemiesDestroyed = 0;
            EnemiesSpawned = 0;
            TotalEnemies = 6;

            TickCounter = 0;
            SpawnTimer = 0;

            BaseAlive = true;

            Status = GameStatus.Playing;

            // Расположение защитных блоков вокруг базы.
            int bx = Field.BaseX;
            int by = Field.BaseY;

            Field.AddObstacle(new Obstacle(bx - 1, by, "Brick"));
            Field.AddObstacle(new Obstacle(bx + 1, by, "Brick"));
            Field.AddObstacle(new Obstacle(bx, by - 1, "Brick"));

            // Препятствия в верхней левой части поля.
            Field.AddObstacle(new Obstacle(5, 3, "Brick"));
            Field.AddObstacle(new Obstacle(6, 3, "Brick"));
            Field.AddObstacle(new Obstacle(7, 3, "Brick"));

            Field.AddObstacle(new Obstacle(20, 3, "Steel"));
            Field.AddObstacle(new Obstacle(21, 3, "Steel"));

            // Препятствия в средней части поля.
            Field.AddObstacle(new Obstacle(4, 7, "Brick"));
            Field.AddObstacle(new Obstacle(5, 7, "Brick"));
            Field.AddObstacle(new Obstacle(6, 7, "Steel"));

            Field.AddObstacle(new Obstacle(14, 7, "Brick"));
            Field.AddObstacle(new Obstacle(15, 7, "Brick"));
            Field.AddObstacle(new Obstacle(16, 7, "Brick"));

            Field.AddObstacle(new Obstacle(23, 7, "Steel"));
            Field.AddObstacle(new Obstacle(24, 7, "Brick"));

            // Препятствия в нижней части поля.
            Field.AddObstacle(new Obstacle(3, 12, "Brick"));
            Field.AddObstacle(new Obstacle(4, 12, "Steel"));
            Field.AddObstacle(new Obstacle(5, 12, "Brick"));

            Field.AddObstacle(new Obstacle(24, 12, "Brick"));
            Field.AddObstacle(new Obstacle(25, 12, "Brick"));
            Field.AddObstacle(new Obstacle(26, 12, "Steel"));

            // Водные участки игрового поля.
            Field.AddObstacle(new Obstacle(10, 5, "Water"));
            Field.AddObstacle(new Obstacle(11, 5, "Water"));
            Field.AddObstacle(new Obstacle(12, 5, "Water"));

            Field.AddObstacle(new Obstacle(10, 6, "Water"));
            Field.AddObstacle(new Obstacle(11, 6, "Water"));
            Field.AddObstacle(new Obstacle(12, 6, "Water"));

            Field.AddObstacle(new Obstacle(19, 14, "Water"));
            Field.AddObstacle(new Obstacle(20, 14, "Water"));
            Field.AddObstacle(new Obstacle(19, 15, "Water"));
            Field.AddObstacle(new Obstacle(20, 15, "Water"));
        }

        // Повернуть игрока и переместить его на соседнюю клетку, если путь свободен.
        public void MovePlayer(Direction direction)
        {
            if (Status != GameStatus.Playing ||
                Player == null || !Player.IsAlive)
                return;

            Player.Rotate(direction);

            int x = Player.X;
            int y = Player.Y;

            switch (direction)
            {
                case Direction.Up: y--; break;
                case Direction.Down: y++; break;
                case Direction.Left: x--; break;
                case Direction.Right: x++; break;
            }

            if (CanMoveTo(x, y, Player))
            {
                Player.X = x;
                Player.Y = y;
            }
        }

        // Проверить, может ли танк занять указанную клетку.
        private bool CanMoveTo(int x, int y, Tank? movingTank)
        {
            if (!Field.IsInside(x, y))
                return false;

            Obstacle? obstacle = Field.GetObstacleAt(x, y);

            if (obstacle != null && obstacle.BlocksTank())
                return false;

            if (x == Field.BaseX && y == Field.BaseY)
                return false;

            if (Player != movingTank &&
                Player.IsAlive &&
                Player.X == x && Player.Y == y)
                return false;

            foreach (Tank enemy in Enemies)
            {
                if (enemy != movingTank &&
                    enemy.IsAlive &&
                    enemy.X == x && enemy.Y == y)
                    return false;
            }

            return true;
        }

        // Создать снаряд, если танк жив и его оружие готово к выстрелу.
        public void Shoot(Tank? tank)
        {
            // Не стрелять, если игра остановлена или танк не может вести огонь.
            if (Status != GameStatus.Playing ||
                tank == null ||
                !tank.IsAlive ||
                tank.ShootCooldown > 0)
                return;

            // Определить клетку перед танком для проверки границы поля.
            int x = tank.X;
            int y = tank.Y;

            switch (tank.Direction)
            {
                case Direction.Up: y--; break;
                case Direction.Down: y++; break;
                case Direction.Left: x--; break;
                case Direction.Right: x++; break;
            }

            if (!Field.IsInside(x, y))
                return;

            // Добавить снаряд в список активных снарядов.
            Bullets.Add(new Bullet(
                tank.X,
                tank.Y,
                tank.Direction,
                tank.IsPlayer
            ));

            tank.ShootCooldown = tank.IsPlayer ? 5 : 12;
        }

        // Обновить состояние игры на один игровой такт.
        public void Update()
        {
            // Во время паузы состояние игры не меняется.
            if (Status != GameStatus.Playing)
                return;

            TickCounter++;

            if (Player.ShootCooldown > 0)
                Player.ShootCooldown--;

            foreach (Tank enemy in Enemies)
            {
                if (enemy.ShootCooldown > 0)
                    enemy.ShootCooldown--;
            }

            SpawnTimer++;

            if (SpawnTimer >= 25)
            {
                SpawnTimer = 0;
                SpawnEnemy();
            }

            UpdateEnemies();
            UpdateBullets();

            Enemies.RemoveAll(e => !e.IsAlive);
            Bullets.RemoveAll(b => !b.IsActive);

            CheckGameStatus();
        }

        // Добавить очередного противника, если достигнут интервал появления.
        private void SpawnEnemy()
        {
            if (EnemiesSpawned >= TotalEnemies ||
                Enemies.Count >= 4)
                return;

            int[] positions = { 2, 14, 27 };

            for (int i = 0; i < positions.Length; i++)
            {
                int x = positions[Random.Next(positions.Length)];

                if (!CanMoveTo(x, 0, null))
                    continue;

                Enemies.Add(new Tank(x, 0, false));
                EnemiesSpawned++;

                return;
            }
        }

        // Обновить поведение всех живых противников.
        private void UpdateEnemies()
        {
            foreach (Tank enemy in Enemies)
            {
                if (!enemy.IsAlive)
                    continue;

                // Если игрок на линии огня, противник целится и стреляет.
                if (TryAimAtPlayer(enemy))
                {
                    if (enemy.ShootCooldown == 0)
                        Shoot(enemy);

                    continue;
                }

                // Перемещать противников только на каждом третьем такте.
                if (TickCounter % 3 != 0)
                    continue;

                MoveEnemyTowardsTarget(enemy);
            }
        }

        // Проверить, может ли противник прицелиться в игрока по прямой линии.
        private bool TryAimAtPlayer(Tank enemy)
        {
            if (!Player.IsAlive)
                return false;

            Direction direction;

            if (enemy.X == Player.X)
            {
                direction = enemy.Y > Player.Y
                    ? Direction.Up : Direction.Down;
            }
            else if (enemy.Y == Player.Y)
            {
                direction = enemy.X > Player.X
                    ? Direction.Left : Direction.Right;
            }
            else
            {
                return false;
            }

            if (!HasClearShot(enemy, direction, Player.X, Player.Y))
                return false;

            enemy.Rotate(direction);

            return true;
        }

        // Проверить, свободен ли путь снаряда до цели.
        private bool HasClearShot(
            Tank tank,
            Direction direction,
            int targetX,
            int targetY)
        {
            int x = tank.X;
            int y = tank.Y;

            while (true)
            {
                switch (direction)
                {
                    case Direction.Up: y--; break;
                    case Direction.Down: y++; break;
                    case Direction.Left: x--; break;
                    case Direction.Right: x++; break;
                }

                if (!Field.IsInside(x, y))
                    return false;

                if (x == targetX && y == targetY)
                    return true;

                Obstacle? obstacle = Field.GetObstacleAt(x, y);

                if (obstacle != null && obstacle.BlocksBullet())
                    return false;
            }
        }

        // Переместить противника в сторону выбранной цели.
        private void MoveEnemyTowardsTarget(Tank enemy)
        {
            List<Direction> directions = new List<Direction>();

            int dx = Player.X - enemy.X;
            int dy = Player.Y - enemy.Y;

            // Выбрать ось, по которой расстояние до цели больше.
            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                if (dx > 0) directions.Add(Direction.Right);
                if (dx < 0) directions.Add(Direction.Left);

                if (dy > 0) directions.Add(Direction.Down);
                if (dy < 0) directions.Add(Direction.Up);
            }
            else
            {
                if (dy > 0) directions.Add(Direction.Down);
                if (dy < 0) directions.Add(Direction.Up);

                if (dx > 0) directions.Add(Direction.Right);
                if (dx < 0) directions.Add(Direction.Left);
            }

            // Оставить только направления движения, доступные противнику.
            List<Direction> alternatives = new List<Direction>
            {
                Direction.Up,
                Direction.Down,
                Direction.Left,
                Direction.Right
            };

            for (int i = alternatives.Count - 1; i > 0; i--)
            {
                int j = Random.Next(i + 1);

                (alternatives[i], alternatives[j]) =
                    (alternatives[j], alternatives[i]);
            }

            foreach (Direction direction in alternatives)
            {
                if (!directions.Contains(direction))
                    directions.Add(direction);
            }

            foreach (Direction direction in directions)
            {
                int x = enemy.X;
                int y = enemy.Y;

                switch (direction)
                {
                    case Direction.Up: y--; break;
                    case Direction.Down: y++; break;
                    case Direction.Left: x--; break;
                    case Direction.Right: x++; break;
                }

                if (CanMoveTo(x, y, enemy))
                {
                    enemy.Rotate(direction);
                    enemy.X = x;
                    enemy.Y = y;

                    return;
                }
            }

            // Если свободного направления нет, выбрать направление случайно.
            enemy.Rotate((Direction)Random.Next(4));
        }

        // Переместить снаряды и обработать столкновения за один такт.
        private void UpdateBullets()
        {
            var activeBullets = Bullets
                .Where(b => b.IsActive)
                .ToList();

            var oldPositions = new Dictionary<Bullet, (int X, int Y)>();
            var newPositions = new Dictionary<Bullet, (int X, int Y)>();

            // Сохранить активные снаряды перед обработкой столкновений.
            foreach (Bullet bullet in activeBullets)
            {
                oldPositions[bullet] = (bullet.X, bullet.Y);

                int x = bullet.X;
                int y = bullet.Y;

                switch (bullet.Direction)
                {
                    case Direction.Up: y--; break;
                    case Direction.Down: y++; break;
                    case Direction.Left: x--; break;
                    case Direction.Right: x++; break;
                }

                newPositions[bullet] = (x, y);
            }

            // Проверить попарные столкновения снарядов.
            for (int i = 0; i < activeBullets.Count; i++)
            {
                Bullet first = activeBullets[i];

                if (!first.IsActive)
                    continue;

                for (int j = i + 1; j < activeBullets.Count; j++)
                {
                    Bullet second = activeBullets[j];

                    if (!second.IsActive)
                        continue;

                    // Снаряды одной стороны не уничтожают друг друга.
                    if (first.IsPlayerBullet == second.IsPlayerBullet)
                        continue;

                    var firstOld = oldPositions[first];
                    var secondOld = oldPositions[second];

                    var firstNew = newPositions[first];
                    var secondNew = newPositions[second];

                    // Снаряды столкнулись в одной клетке.
                    bool sameCell =
                        firstNew.X == secondNew.X &&
                        firstNew.Y == secondNew.Y;

                    // Снаряды пересеклись, поменявшись клетками за один такт.
                    bool crossed =
                        firstOld.X == secondNew.X &&
                        firstOld.Y == secondNew.Y &&
                        secondOld.X == firstNew.X &&
                        secondOld.Y == firstNew.Y;

                    if (sameCell || crossed)
                    {
                        first.IsActive = false;
                        second.IsActive = false;
                    }
                }
            }

            // Переместить снаряд и проверить столкновение с игровыми объектами.
            foreach (Bullet bullet in activeBullets)
            {
                if (!bullet.IsActive)
                    continue;

                var position = newPositions[bullet];

                bullet.X = position.X;
                bullet.Y = position.Y;

                if (!Field.IsInside(bullet.X, bullet.Y))
                {
                    bullet.IsActive = false;
                    continue;
                }

                // Обработать попадание снаряда в препятствие.
                Obstacle? obstacle = Field.GetObstacleAt(
                    bullet.X, bullet.Y);

                if (obstacle != null && obstacle.BlocksBullet())
                {
                    obstacle.TakeDamage();
                    bullet.IsActive = false;
                    continue;
                }

                // Обработать попадание снаряда в базу.
                if (bullet.X == Field.BaseX &&
                    bullet.Y == Field.BaseY)
                {
                    if (!bullet.IsPlayerBullet)
                        BaseAlive = false;

                    bullet.IsActive = false;
                    continue;
                }

                // Обработать попадание вражеского снаряда в игрока.
                if (!bullet.IsPlayerBullet &&
                    Player.IsAlive &&
                    bullet.X == Player.X &&
                    bullet.Y == Player.Y)
                {
                    Player.TakeDamage(bullet.Damage);
                    bullet.IsActive = false;

                    if (!Player.IsAlive)
                    {
                        Lives--;

                        if (Lives > 0)
                        {
                            Player.Health = Player.MaxHealth;
                            Player.X = 15;
                            Player.Y = 15;
                            Player.Direction = Direction.Up;
                        }
                    }

                    continue;
                }

                // Обработать попадание снаряда игрока во вражеский танк.
                if (bullet.IsPlayerBullet)
                {
                    Tank? enemy = Enemies.FirstOrDefault(
                        e => e.IsAlive &&
                             e.X == bullet.X &&
                             e.Y == bullet.Y);

                    if (enemy != null)
                    {
                        enemy.TakeDamage(bullet.Damage);
                        bullet.IsActive = false;

                        if (!enemy.IsAlive)
                        {
                            Score += 100;
                            EnemiesDestroyed++;
                        }
                    }
                }
            }
        }

        // Определить, завершилась ли игра победой или поражением.
        private void CheckGameStatus()
        {
            if (Lives <= 0 || !BaseAlive)
            {
                Status = GameStatus.Lost;
                return;
            }

            if (EnemiesDestroyed >= TotalEnemies)
            {
                Status = GameStatus.Won;
            }
        }
    }
}
