using System.Collections.Generic;
using System.Linq;

namespace BattleCity.Models
{
    //  ласс, описывающий игровое поле, хранит размеры пол€, координаты базы и список преп€тствий
    public class GameField
    {
        // Ўирина и высота игрового пол€
        public int Width { get; set; }
        public int Height { get; set; }

        //  оординаты базы игрока
        public int BaseX { get; set; }
        public int BaseY { get; set; }

        // —писок всех преп€тствий, расположенных на игровом поле
        public List<Obstacle> Obstacles { get; set; }

        //  онструктор по умолчанию
        public GameField()
        {
            Width = 30;
            Height = 20;

            Obstacles = new List<Obstacle>();
        }

        // —оздаЄт игровое поле с заданными размерами
        public GameField(int width, int height)
        {
            Width = width;
            Height = height;

            BaseX = width / 2;
            BaseY = height - 2;

            Obstacles = new List<Obstacle>();
        }
        // ѕровер€ет, наход€тс€ ли указанные координаты в пределах игрового пол€
        public bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width &&
                   y >= 0 && y < Height;
        }

        // Ќаходит преп€тствие по указанным координатам
        public Obstacle GetObstacleAt(int x, int y)
        {
            return Obstacles.FirstOrDefault(
                obstacle => obstacle.X == x &&
                            obstacle.Y == y &&
                            !obstacle.IsDestroyed);
        }

        // ƒобавл€ет преп€тствие на игровое поле, если его координаты наход€тс€ в пределах пол€
        public void AddObstacle(Obstacle obstacle)
        {
            if (IsInside(obstacle.X, obstacle.Y))
            {
                Obstacles.Add(obstacle);
            }
        }
    }
}