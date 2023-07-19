using Godot;

public class Projectile : CurvedSpatial
{
    [Export]
    public float Speed = 60f;

    private bool _isMoving;
    private Camera _camera;
    private float _killTimer = 1f;
    private CPUParticles _explodeEffect1;
    private CPUParticles _explodeEffect2;
    private CSGMesh _circle;

    public override void _Ready()
    {
        _camera = GetViewport().GetCamera();
        _explodeEffect1 = GetNode<CPUParticles>("ExplodeEffect1");
        _explodeEffect1.Visible = false;
        _explodeEffect2 = GetNode<CPUParticles>("ExplodeEffect2");
        _explodeEffect2.Visible = false;
        _circle = GetNode<CSGMesh>("Circle");
        base._Ready();
    }

    public override void _Process(float delta)
    {
        if (_isMoving || _explodeEffect1.Visible)
            base._Process(delta);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_explodeEffect1.Visible)
        {
            _circle.Scale += Vector3.One * -30 * delta;
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
            _explodeEffect1.Visible = true;
            _explodeEffect1.Emitting = true;
            _explodeEffect2.Visible = true;
            _explodeEffect2.Emitting = true;
            _circle.Visible = true;
            _circle.Scale *= 8;

            var audioPlayer = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
            audioPlayer.Play();
        }
    }
}