namespace BattleCity.Models
{
    // ѕеречисление возможных направлений движени€ танка
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    //  ласс, описывающий танк в игре
    public class Tank
    {
        //  оординаты танка
        public int X { get; set; }
        public int Y { get; set; }

        // “екущее направление, в котором повЄрнут танк
        public Direction Direction { get; set; }

        // ќпредел€ет, €вл€етс€ ли танк управл€емым игроком
        public bool IsPlayer { get; set; }

        // “екущее количество очков здоровь€ танка
        public int Health { get; set; }

        // ћаксимальное количество очков здоровь€ танка
        public int MaxHealth { get; set; }

        // ѕровер€ет, жив ли танк
        public bool IsAlive => Health > 0;

        // ¬рем€ перезар€дки оруди€ танка
        public int ShootCooldown { get; set; }

        //  онструктор по умолчанию
        public Tank()
        {
            Direction = Direction.Up;
            Health = 3;
            MaxHealth = 3;
        }

        //  онструктор дл€ создани€ танка с заданными координатами и указанием того, принадлежит ли он игроку
        public Tank(int x, int y, bool isPlayer)
        {
            X = x;
            Y = y;
            IsPlayer = isPlayer;

            Direction = Direction.Up;

            MaxHealth = isPlayer ? 3 : 1;
            Health = MaxHealth;
        }

        // ћетод получени€ урона танком
        public void TakeDamage(int damage)
        {
            Health -= damage;

            if (Health < 0)
                Health = 0;
        }

        // ћетод изменени€ направлени€ танка
        public void Rotate(Direction direction)
        {
            Direction = direction;
        }
    }
}