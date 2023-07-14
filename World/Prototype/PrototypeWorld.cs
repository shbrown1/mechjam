using Godot;
using System.Collections.Generic;

public class PrototypeWorld : Spatial
{
    [Export]
    private NodePath _floorNodePath;

    [Export]
    private List<Spatial> _randomizedObjects;

    private float _gridSize = 100f;
    private Player _player;
    private Spatial _randomizedObjectPool;

    public override void _Ready()
    {
        _player = GetTree().Root.FindNode("Player", true, false) as Player;
        _randomizedObjectPool = GetNode<Spatial>("RandomizedObjectPool");
    }

    public override void _Process(float delta)
    {
        var floor = GetNode<Spatial>(_floorNodePath);
        var xDistance = _player.GlobalTranslation.x - floor.GlobalTranslation.x;
        if (xDistance > _gridSize / 2f)
            MoveWorld(Vector3.Right);
        else if (xDistance < -_gridSize / 2f)
            MoveWorld(Vector3.Left);

        var zDistance = _player.GlobalTranslation.z - floor.GlobalTranslation.z;
        if (zDistance > _gridSize / 2f)
            MoveWorld(Vector3.Back);
        else if (zDistance < -_gridSize / 2f)
            MoveWorld(Vector3.Forward);
    }

    public void MoveWorld(Vector3 direction)
    {
        var floor = GetNode<Spatial>(_floorNodePath);
        floor.Translate(direction * _gridSize);

        foreach (var child in _randomizedObjectPool.GetChildren())
        {
            var spatial = child as Spatial;
            if (spatial == null)
                continue;

            if (Mathf.Max(Mathf.Abs(spatial.GlobalTranslation.x - floor.GlobalTranslation.x), Mathf.Abs(spatial.GlobalTranslation.z - floor.GlobalTranslation.z)) > _gridSize * 1.5)
            {
                spatial.Translate(direction * _gridSize * 1.5f * 2f);
            }
        }
    }
}