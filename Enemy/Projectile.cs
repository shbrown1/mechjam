using Godot;

public class Projectile : CurvedSpatial
{
    [Export]
    public float Speed = 60f;

    private bool _isMoving;
    private Camera _camera;

    public override void _Ready()
    {
        _camera = GetViewport().GetCamera();
        base._Ready();
    }

    public override void _Process(float delta)
    {
        if (_isMoving)
            base._Process(delta);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_isMoving)
        {
            Translate(Vector3.Forward * Speed * delta);
            if (_camera.GlobalTranslation.DistanceSquaredTo(GlobalTranslation) > 200 * 200)
            {
                QueueFree();
            }
        }
    }

    public void StartMoving()
    {
        _isMoving = true;
        UpdateY(GlobalTranslation.y);
    }
}