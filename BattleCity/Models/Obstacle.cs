namespace BattleCity.Models
{
    //  ласс, описывающий преп€тствие на игровом поле, хранит координаты, тип преп€тстви€ и его прочность
    public class Obstacle
    {
        //  оординаты преп€тстви€ на игровом поле
        public int X { get; set; }
        public int Y { get; set; }

        // “ип преп€тстви€: Brick (кирпич), Steel (металл) или Water (вода)
        public string Type { get; set; }

        // ѕрочность преп€тстви€. ќпредел€ет количество попаданий, необходимых дл€ его уничтожени€
        public int Health { get; set; }

        // ѕровер€ет, уничтожено ли преп€тствие
        public bool IsDestroyed => Health <= 0;

        //  онструктор по умолчанию
        public Obstacle()
        {
            Type = "Brick";
            Health = 1;
        }

        // —оздаЄт преп€тствие с заданными координатами и типом
        public Obstacle(int x, int y, string type)
        {
            X = x;
            Y = y;
            Type = type;

            switch (type)
            {
                case "Brick":
                    Health = 1;
                    break;

                case "Steel":
                case "Water":
                    Health = int.MaxValue;
                    break;

                default:
                    Type = "Brick";
                    Health = 1;
                    break;
            }
        }

        // Ќаносит преп€тствию урон
        public void TakeDamage()
        {
            if (Type == "Brick" && Health > 0)
            {
                Health--;
            }
        }

        // ќпредел€ет, блокирует ли преп€тствие движение танков
        public bool BlocksTank()
        {
            return Type == "Brick" ||
                   Type == "Steel" ||
                   Type == "Water";
        }

        // ќпредел€ет, блокирует ли преп€тствие движение снар€дов
        public bool BlocksBullet()
        {
            return Type == "Brick" ||
                   Type == "Steel";
        }
    }
}