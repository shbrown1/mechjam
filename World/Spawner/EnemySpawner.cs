using Godot;

public class EnemySpawner : Spatial
{
    [Export]
    public PackedScene EnemyToSpawn;
    [Export]
    public int NumberOfEnemiesToSpawn = 30;

    public override void _Ready()
    {
        base._Ready();

        for (int i = 0; i < NumberOfEnemiesToSpawn; i++)
        {
            var enemy = EnemyToSpawn.Instance() as Enemy;
            enemy.GlobalTransform = GlobalTransform;
            enemy.Translate(new Vector3((float)GD.RandRange(-10, 10), 0, (float)GD.RandRange(-10, 10)));
            GetNode("../").CallDeferred("add_child", enemy);
        }
    }
}