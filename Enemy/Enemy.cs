using Godot;
using System;

public class Enemy : CurvedSpatial
{
    public bool IsDead;

    private AnimationPlayer _animationPlayer;
    private bool _isBeingKnockedBack;
    private Vector3 _velocity;

    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("enemy/AnimationPlayerEvents");
        base._Ready();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_isBeingKnockedBack)
        {
            _velocity += Vector3.Forward * 10 * delta;
            Translate(_velocity * delta);
        }
    }

    public void GetHit(Vector3 hitPosition)
    {
        _animationPlayer.Play("Hit");
        _isBeingKnockedBack = true;
        IsDead = true;
        _velocity = Vector3.Forward * 100;
        hitPosition.y = GlobalTransform.origin.y;
        Vector3 direction = GlobalTransform.origin - hitPosition;
        LookAt(GlobalTransform.origin + direction, Vector3.Up);
    }

    public void KnockBackCompletedEvent()
    {
        _isBeingKnockedBack = false;
        QueueFree();
    }
}