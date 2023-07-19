using Godot;
using System;

public class ThrownPeice : Spatial
{
    public bool CameraLockOn;

    private Vector3 _rotationAxis;
    private float _rotationSpeed;
    private float _speed = 60f;
    private Vector3 _direction;
    private bool _isHit;
    private bool _isStopped;
    private Vector3 _target;
    private MechaGundam _mechaGundam;

    public override void _Ready()
    {
        var player = GetTree().Root.FindNode("Player", true, false) as Player;
        _direction = (player.GlobalTranslation - GlobalTranslation).Normalized();

        _rotationSpeed = (float)GD.RandRange(10, 20);
        _rotationAxis = new Vector3((float)GD.RandRange(-1, 1), (float)GD.RandRange(-1, 1), (float)GD.RandRange(-1, 1)).Normalized();

        _mechaGundam = GetTree().Root.FindNode("MechaGundam", true, false) as MechaGundam;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if (GlobalTranslation.y > 1 || _isStopped)
        {
            GlobalTranslation += _direction * _speed * delta;
            Rotate(_rotationAxis, _rotationSpeed * delta);
            if (GlobalTranslation.y < 1)
            {
                var audioPlayer = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
                audioPlayer.Play();
            }
        }

        if (_isHit)
        {
            GlobalTranslation += _direction * _speed * delta;
            if (_target.DistanceTo(GlobalTranslation) < 1)
            {
                _mechaGundam.GetHit(this);
            }
        }
    }

    public void GetHit()
    {
        _target = _mechaGundam.GlobalTranslation + Vector3.Up * 4;
        _direction = (_target - GlobalTranslation).Normalized();
        _isHit = true;
        _isStopped = false;
    }

    public void ReadyForHit()
    {
        _isStopped = true;
        GlobalTranslation = new Vector3(GlobalTranslation.x, 1, GlobalTranslation.z);
        _direction = Vector3.Zero;
        CameraLockOn = true;
    }
}
