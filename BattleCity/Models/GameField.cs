using System.Collections.Generic;
using System.Linq;

namespace BattleCity.Models
{
    // Игровое поле хранит размеры, положение базы и препятствия.
    public class GameField
    {
        // Ширина и высота игрового поля в клетках.
        public int Width { get; set; }
        public int Height { get; set; }

        // Координаты базы на поле.
        public int BaseX { get; set; }
        public int BaseY { get; set; }

        // Препятствия, размещённые на игровом поле.
        public List<Obstacle> Obstacles { get; set; }

        // Создать поле стандартного размера.
        public GameField()
        {
            Width = 30;
            Height = 20;

            Obstacles = new List<Obstacle>();
        }

        // Создать поле заданного размера и разместить базу.
        public GameField(int width, int height)
        {
            Width = width;
            Height = height;

            BaseX = width / 2;
            BaseY = height - 2;

            Obstacles = new List<Obstacle>();
        }
        // Проверить, находится ли клетка внутри поля.
        public bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width &&
                   y >= 0 && y < Height;
        }

        // Найти неповреждённое препятствие по координатам.
        public Obstacle? GetObstacleAt(int x, int y)
        {
            return Obstacles.FirstOrDefault(
                obstacle => obstacle.X == x &&
                            obstacle.Y == y &&
                            !obstacle.IsDestroyed);
        }

        // Добавить препятствие, если его координаты находятся внутри поля.
        public void AddObstacle(Obstacle obstacle)
        {
            if (IsInside(obstacle.X, obstacle.Y))
            {
                Obstacles.Add(obstacle);
            }
        }
    }
}
