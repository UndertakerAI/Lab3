namespace BattleCity.Models
{
    //  ласс, описывающий снар€д, выпущенный танком, хранит его координаты, направление движени€ и характеристики
    public class Bullet
    {
        //  оордината снар€да по горизонтали
        public int X { get; set; }
        //  оордината снар€да по вертикали
        public int Y { get; set; }

        // Ќаправление движени€ снар€да
        public Direction Direction { get; set; }

        // ”казывает, принадлежит ли снар€д игроку
        public bool IsPlayerBullet { get; set; }

        //  оличество урона, наносимого снар€дом при попадании
        public int Damage { get; set; }

        // ќпредел€ет, активен ли снар€д
        public bool IsActive { get; set; }

        //  онструктор по умолчанию
        public Bullet()
        {
            Damage = 1;
            IsActive = true;
        }

        // —оздаЄт новый снар€д с заданными координатами, направлением движени€ и принадлежностью
        public Bullet(int x, int y, Direction direction, bool isPlayerBullet)
        {
            X = x;
            Y = y;
            Direction = direction;

            IsPlayerBullet = isPlayerBullet;

            Damage = 1;
            IsActive = true;
        }

        // ѕеремещает снар€д на одну клетку игрового пол€
        public void Move()
        {
            switch (Direction)
            {
                case Direction.Up:
                    Y--;
                    break;

                case Direction.Down:
                    Y++;
                    break;

                case Direction.Left:
                    X--;
                    break;

                case Direction.Right:
                    X++;
                    break;
            }
        }
    }
}