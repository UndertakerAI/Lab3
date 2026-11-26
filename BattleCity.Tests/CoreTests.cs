using BattleCity.Models;

namespace BattleCity.Tests;

[TestClass]
public class BulletTests
{
    [TestMethod]
    public void Constructor_SetsStateAndMoveAdvancesInDirection()
    {
        var bullet = new Bullet(4, 7, Direction.Left, true);
        Assert.AreEqual(4, bullet.X);
        Assert.AreEqual(7, bullet.Y);
        Assert.AreEqual(1, bullet.Damage);
        Assert.IsTrue(bullet.IsActive);
        Assert.IsTrue(bullet.IsPlayerBullet);

        bullet.Move();
        Assert.AreEqual(3, bullet.X);
        Assert.AreEqual(7, bullet.Y);
    }

    [TestMethod]
    [DataRow(Direction.Up, 3, 4)]
    [DataRow(Direction.Down, 3, 6)]
    [DataRow(Direction.Left, 2, 5)]
    [DataRow(Direction.Right, 4, 5)]
    public void Move_AdvancesOneCell(Direction direction, int x, int y)
    {
        var bullet = new Bullet(3, 5, direction, false);
        bullet.Move();
        Assert.AreEqual(x, bullet.X);
        Assert.AreEqual(y, bullet.Y);
    }
}

[TestClass]
public class ObstacleTests
{
    [TestMethod]
    public void Types_HaveImplementedHealthAndBlockingRules()
    {
        var brick = new Obstacle(1, 2, "Brick");
        var steel = new Obstacle(3, 4, "Steel");
        var water = new Obstacle(5, 6, "Water");
        Assert.AreEqual(1, brick.Health);
        Assert.AreEqual(int.MaxValue, steel.Health);
        Assert.IsTrue(brick.BlocksTank());
        Assert.IsTrue(steel.BlocksTank());
        Assert.IsTrue(water.BlocksTank());
        Assert.IsTrue(brick.BlocksBullet());
        Assert.IsTrue(steel.BlocksBullet());
        Assert.IsFalse(water.BlocksBullet());

        brick.TakeDamage();
        steel.TakeDamage();
        water.TakeDamage();
        Assert.IsTrue(brick.IsDestroyed);
        Assert.IsFalse(steel.IsDestroyed);
        Assert.IsFalse(water.IsDestroyed);
    }
}

[TestClass]
public class GameFieldTests
{
    [TestMethod]
    public void Constructor_SetsSizeBaseAndEmptyObstacleList()
    {
        var field = new GameField(12, 8);
        Assert.AreEqual(12, field.Width);
        Assert.AreEqual(8, field.Height);
        Assert.AreEqual(6, field.BaseX);
        Assert.AreEqual(6, field.BaseY);
        Assert.IsEmpty(field.Obstacles);
    }

    [TestMethod]
    [DataRow(0, 0, true)]
    [DataRow(11, 7, true)]
    [DataRow(-1, 0, false)]
    [DataRow(12, 0, false)]
    [DataRow(0, 8, false)]
    public void IsInside_UsesZeroBasedBounds(int x, int y, bool expected)
        => Assert.AreEqual(expected, new GameField(12, 8).IsInside(x, y));

    [TestMethod]
    public void AddObstacle_IgnoresOutsideAndLookupSkipsDestroyed()
    {
        var field = new GameField(5, 5);
        var inside = new Obstacle(2, 2, "Brick");
        field.AddObstacle(new Obstacle(5, 2, "Brick"));
        field.AddObstacle(inside);
        Assert.HasCount(1, field.Obstacles);
        Assert.AreSame(inside, field.GetObstacleAt(2, 2));
        inside.TakeDamage();
        Assert.IsNull(field.GetObstacleAt(2, 2));
    }
}

[TestClass]
public class GameTests
{
    [TestMethod]
    public void MovePlayer_MovesAndTurnsWhileBlockedMoveOnlyTurns()
    {
        var game = new Game();
        game.Field.Obstacles.Clear();
        game.MovePlayer(Direction.Right);
        Assert.AreEqual(16, game.Player.X);
        Assert.AreEqual(15, game.Player.Y);
        Assert.AreEqual(Direction.Right, game.Player.Direction);

        game.Field.Obstacles.Add(new Obstacle(17, 15, "Water"));
        game.MovePlayer(Direction.Right);
        Assert.AreEqual(16, game.Player.X);
        Assert.AreEqual(Direction.Right, game.Player.Direction);
    }

    [TestMethod]
    public void MovePlayer_CannotLeaveField()
    {
        var game = new Game();
        game.Player.X = 0;
        game.Player.Y = 1;
        game.MovePlayer(Direction.Left);
        Assert.AreEqual(0, game.Player.X);
    }

    [TestMethod]
    public void Shoot_CreatesBulletAndEnforcesCooldown()
    {
        var game = new Game();
        game.Shoot(game.Player);
        Assert.HasCount(1, game.Bullets);
        Assert.AreEqual(15, game.Bullets[0].X);
        Assert.AreEqual(15, game.Bullets[0].Y);
        Assert.AreEqual(Direction.Up, game.Bullets[0].Direction);
        Assert.AreEqual(5, game.Player.ShootCooldown);
        game.Shoot(game.Player);
        Assert.HasCount(1, game.Bullets);
    }

    [TestMethod]
    public void Update_PlayerBulletDamagesBrick()
    {
        var game = new Game();
        game.Field.Obstacles.Clear();
        var brick = new Obstacle(5, 4, "Brick");
        game.Field.AddObstacle(brick);
        game.Bullets.Add(new Bullet(5, 5, Direction.Up, true));
        game.Update();
        Assert.IsTrue(brick.IsDestroyed);
        Assert.IsEmpty(game.Bullets);
    }

    [TestMethod]
    public void Update_EnemyBulletDamagesPlayer()
    {
        var game = new Game();
        game.Field.Obstacles.Clear();
        game.Enemies.Clear();
        game.Player.X = 5;
        game.Player.Y = 5;
        game.Bullets.Add(new Bullet(5, 4, Direction.Down, false));
        game.Update();
        Assert.AreEqual(2, game.Player.Health);
        Assert.AreEqual(3, game.Lives);
        Assert.IsEmpty(game.Bullets);
    }

    [TestMethod]
    public void Update_PlayerBulletDestroysEnemyAndWins()
    {
        var game = new Game();
        game.Field.Obstacles.Clear();
        game.TotalEnemies = 1;
        game.Enemies.Add(new Tank(6, 5, false));
        game.Bullets.Add(new Bullet(5, 5, Direction.Right, true));
        game.Update();
        Assert.AreEqual(100, game.Score);
        Assert.AreEqual(1, game.EnemiesDestroyed);
        Assert.IsEmpty(game.Enemies);
        Assert.AreEqual(GameStatus.Won, game.Status);
    }

    [TestMethod]
    public void Update_LastLifeLossAndBaseDestructionLoseGame()
    {
        var playerGame = new Game();
        playerGame.Field.Obstacles.Clear();
        playerGame.Enemies.Clear();
        playerGame.Player.X = 5;
        playerGame.Player.Y = 5;
        playerGame.Player.Health = 1;
        playerGame.Lives = 1;
        playerGame.Bullets.Add(new Bullet(5, 4, Direction.Down, false));
        playerGame.Update();
        Assert.AreEqual(0, playerGame.Lives);
        Assert.AreEqual(GameStatus.Lost, playerGame.Status);

        var baseGame = new Game();
        baseGame.Field.Obstacles.Clear();
        baseGame.Enemies.Clear();
        baseGame.Bullets.Add(new Bullet(baseGame.Field.BaseX, baseGame.Field.BaseY - 1, Direction.Down, false));
        baseGame.Update();
        Assert.IsFalse(baseGame.BaseAlive);
        Assert.AreEqual(GameStatus.Lost, baseGame.Status);
    }

    [TestMethod]
    public void Update_DoesNothingWhenPaused()
    {
        var game = new Game { Status = GameStatus.Paused };
        game.Update();
        Assert.AreEqual(0, game.TickCounter);
    }
}

[TestClass]
public class GameSerializerTests
{
    [TestMethod]
    public void SaveAndLoad_RoundTripsGameState()
    {
        GameSerializer.DeleteSave();
        try
        {
            var game = new Game();
            game.Player.X = 9;
            game.Score = 250;
            game.Enemies.Add(new Tank(2, 3, false));
            Assert.IsTrue(GameSerializer.Save(game));
            Assert.IsTrue(GameSerializer.SaveExists());

            var loaded = GameSerializer.Load();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(9, loaded.Player.X);
            Assert.AreEqual(250, loaded.Score);
            Assert.HasCount(1, loaded.Enemies);
            Assert.AreEqual(GameStatus.Playing, loaded.Status);
        }
        finally
        {
            GameSerializer.DeleteSave();
        }
    }
}