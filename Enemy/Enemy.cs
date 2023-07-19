using Godot;
using System;

public class Enemy : CurvedSpatial
{
    [Export]
    private PackedScene _baseProjectile;
    [Export]
    private NodePath _projectilePath;

    public bool IsDead;

    private Spatial _projectile;
    private AnimationPlayer _animationPlayer;
    private bool _isBeingKnockedBack;
    private Vector3 _velocity;
    private bool _isActive;
    private float _activeWaitTimer;
    private bool _projectileCharging;
    private Player _player;
    private Vector3 _startPosition;
    private float _shootTimer;

    public override void _Ready()
    {
        _startPosition = GlobalTranslation;
        _projectile = GetNode<Spatial>(_projectilePath);
        _projectile.Visible = false;
        _animationPlayer = GetNode<AnimationPlayer>("enemy/AnimationPlayerEvents");
        _player = GetTree().Root.FindNode("Player", true, false) as Player;
        base._Ready();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_isBeingKnockedBack)
        {
            _velocity += Vector3.Back * 10 * delta;
            Translate(_velocity * delta);
        }
        else if (!_isActive)
        {
            if (_activeWaitTimer > 0)
            {
                _activeWaitTimer -= delta;
                if (_activeWaitTimer < 0)
                {
                    _activeWaitTimer = 0;
                    _isActive = true;
                }
            }
            if (_player.GlobalTranslation.DistanceSquaredTo(GlobalTranslation) < 20 * 20)
            {
                _isActive = true;
                _animationPlayer.Play("Shoot");
                _activeWaitTimer = (float)GD.RandRange(100, 1000) / 10f;
            }
        }
        else if (_isActive)
        {
            if (_projectileCharging)
            {
                _projectile.Scale += Vector3.One * 1f * delta;
            }
            LookAt(new Vector3(_player.GlobalTranslation.x, GlobalTranslation.y, _player.GlobalTranslation.z), Vector3.Up);

            if (_shootTimer > 0)
            {
                _shootTimer -= delta;
                if (_shootTimer < 0)
                {
                    _animationPlayer.Play("Shoot");
                    _shootTimer = 0;
                }
            }
        }

        if (_player.GlobalTranslation.z - GlobalTranslation.z > 100)
        {
            ResetEnemy();
            ResetEnemy();
        }

        if (_player.IsFightingBoss)
        {
            QueueFree();
        }
    }

    public void GetHit(Vector3 hitPosition)
    {
        _animationPlayer.Play("Hit");
        _isBeingKnockedBack = true;
        IsDead = true;
        _velocity = Vector3.Back * 100;
        hitPosition.y = GlobalTransform.origin.y;
        LookAt(hitPosition, Vector3.Up);
    }

    public void StartChargingProjectileEvent()
    {
        _projectile.Scale = Vector3.One * .1f;
        _projectileCharging = true;
        _projectile.Visible = true;
    }

    public void ShootProjectileEvent()
    {
        _projectileCharging = false;
        _projectile.Visible = false;
        var newProjectile = _baseProjectile.Instance() as Projectile;
        var parent = GetNode("../");
        parent.AddChild(newProjectile);
        newProjectile.GlobalTranslation = _projectile.GlobalTranslation;
        newProjectile.GlobalRotation = GlobalRotation;
        newProjectile.StartMoving();
        newProjectile.UpdateY(1.2f);
        newProjectile.Scale = Vector3.One * .4f;
        _shootTimer = 1f;
    }

    public void KnockBackCompletedEvent()
    {
        _isBeingKnockedBack = false;
        ResetEnemy();
    }

    private void ResetEnemy()
    {
        _isBeingKnockedBack = false;
        _isActive = false;
        _activeWaitTimer = 0;
        _projectileCharging = false;
        _projectile.Visible = false;
        _projectile.Scale = Vector3.One * .1f;
        IsDead = false;
        _velocity = Vector3.Zero;
        GlobalTranslation = _startPosition + Vector3.Back * 100;
        _startPosition = GlobalTranslation;
        _animationPlayer.Play("Idle_loop");

        if (_startPosition.z > 500)
        {
            QueueFree();
        }
    }

    public void ReturnToIdle()
    {
        _animationPlayer.Play("Idle_loop");
    }
}