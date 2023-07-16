using Godot;

public class Projectile : CurvedSpatial
{
    [Export]
    public float Speed = 60f;

    private bool _isMoving;
    private Camera _camera;
    private float _killTimer = 1f;
    private CPUParticles _explodeEffect;

    public override void _Ready()
    {
        _camera = GetViewport().GetCamera();
        _explodeEffect = GetNode<CPUParticles>("ExplodeEffect");
        _explodeEffect.Visible = false;
        base._Ready();
    }

    public override void _Process(float delta)
    {
        if (_isMoving || _explodeEffect.Visible)
            base._Process(delta);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_explodeEffect.Visible)
        {
            _killTimer -= delta;
            if (_killTimer < 0)
                QueueFree();
        }

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
        var area = GetNode<Area>("Area");
        area.Connect("body_entered", this, nameof(OnBodyEntered));
    }

    public void OnBodyEntered(Node body)
    {
        if (body is Player)
        {
            var player = body as Player;
            player.Hit(GlobalTranslation);
            var area = GetNode<Area>("Area");
            area.QueueFree();
            _isMoving = false;
            foreach (var child in GetChildren())
            {
                if (child is Spatial spatial)
                {
                    spatial.Visible = false;
                }
            }
            _explodeEffect.Visible = true;
            _explodeEffect.Emitting = true;
        }
    }
}