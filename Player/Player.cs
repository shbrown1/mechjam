using Godot;
using System;

public class Player : KinematicBody
{
    public float MouseSensitivity = 0.2f;

    private AnimationTree _animationTree;
    private Spatial _cameraHolder;
    private Camera _camera;

    private MovementSystem _movementSystem;
    private CPUParticles[] _jetParticles;

    private bool _isAttacking;
    private float _attackTimer;
    private bool _isHit;
    private float _hitTimer;
    private CPUParticles _hitParticles;

    private Spatial _currentTarget;

    public override void _Ready()
    {
        _movementSystem = new MovementSystem();
        _animationTree = GetNode<AnimationTree>("AnimationTree");
        _cameraHolder = GetNode<Spatial>("CameraHolder");
        _camera = GetViewport().GetCamera() as Camera;
        Input.MouseMode = Input.MouseModeEnum.Captured;

        _jetParticles = new CPUParticles[2];
        _jetParticles[0] = GetNode<CPUParticles>("playergundam/Armature/Skeleton/BoneAttachment4/Jets/JetParticles0");
        _jetParticles[1] = GetNode<CPUParticles>("playergundam/Armature/Skeleton/BoneAttachment4/Jets/JetParticles1");

        _hitParticles = GetNode<CPUParticles>("HitParticles");
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("mouse_capture"))
        {
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_isHit)
        {
            _movementSystem.ProcessHit(delta);
            _hitTimer -= delta;
            if (_hitTimer < 0)
            {
                _isHit = false;
                _animationTree.Set("parameters/Transition/current", 0);
            }
        }
        else if (_isAttacking)
        {
            _movementSystem.ProcessAttack(this, _currentTarget);
            if (_currentTarget != null)
            {
                PlayerLookAt(_currentTarget.GlobalTransform.origin);
            }

            if (_currentTarget != null && _movementSystem.MoveVector == Vector3.Zero)
                _animationTree.Set("parameters/Transition/current", 4);

            _attackTimer += delta;
            if (_attackTimer > 1)
                AttackCompletedEvent();
        }
        else if (!_isAttacking)
        {
            var inputDirection = new Vector2(
                Input.GetActionRawStrength("input_right") - Input.GetActionRawStrength("input_left"),
                Input.GetActionRawStrength("input_forward") - Input.GetActionRawStrength("input_back")
            );

            _movementSystem.ProcessInput(inputDirection, delta);

            if (_movementSystem.BoostHasStarted())
            {
                _animationTree.Set("parameters/Transition/current", 1);
            }

            if (_movementSystem.BoostHasEnded())
            {
                _animationTree.Set("parameters/Transition/current", 0);
            }

            if (Input.IsActionJustPressed("attack"))
            {
                _isAttacking = true;
                var area = GetNode<Area>("AttackArea");
                var bodies = area.GetOverlappingBodies();
                _currentTarget = null;
                foreach (var body in bodies)
                {
                    if (body is Enemy enemy)
                    {
                        if (enemy.IsDead)
                            continue;

                        var enemyIsCloser = _currentTarget != null
                            && _currentTarget.GlobalTransform.origin.DistanceTo(GlobalTransform.origin) > enemy.GlobalTransform.origin.DistanceTo(GlobalTransform.origin);
                        if (_currentTarget == null || enemyIsCloser)
                            _currentTarget = enemy as Spatial;
                    }
                }

                if (_currentTarget != null)
                {
                    _animationTree.Set("parameters/Transition/current", 3);
                }
                else
                {
                    _animationTree.Set("parameters/Transition/current", 2);
                }
            }
        }

        _animationTree.Set("parameters/NormalBlend/blend_position", _movementSystem.AnimationVector);
        _animationTree.Set("parameters/BoostBlend/blend_position", _movementSystem.AnimationVector);

        var moveDirection = _movementSystem.MoveVector;

        // Create a new basis with Y direction always up
        Basis horizontalBasis = new Basis(_camera.GlobalTransform.basis.x, Vector3.Up, -_camera.GlobalTransform.basis.z);
        // Orthogonalize the basis to ensure X and Z axes are perpendicular
        horizontalBasis = horizontalBasis.Orthonormalized();
        Vector3 globalDirection = horizontalBasis.Xform(moveDirection);

        MoveAndSlide(globalDirection);
        var jetScale = Mathf.Max(Mathf.Abs(moveDirection.x), Mathf.Abs(moveDirection.z)) / 70f;

        if (_isHit)
            jetScale = 0f;
        _jetParticles[0].ScaleAmount = jetScale;
        _jetParticles[1].ScaleAmount = jetScale;
        _hitParticles.Emitting = _isHit || _movementSystem.IsBoosting();
    }

    public override void _Input(InputEvent @event)
    {
        if (_isHit)
            return;

        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            // _cameraHolder.RotateX(Mathf.Deg2Rad(mouseMotionEvent.Relative.y * MouseSensitivity));
            RotateY(-Mathf.Deg2Rad(mouseMotionEvent.Relative.x * MouseSensitivity));
        }
    }

    public void PlayerLookAt(Vector3 target)
    {
        target.y = GlobalTransform.origin.y;
        Vector3 direction = GlobalTransform.origin - target;
        LookAt(GlobalTransform.origin + direction, Vector3.Up);
    }

    public void AttackCompletedEvent()
    {
        _isAttacking = false;
        _attackTimer = 0f;
        _animationTree.Set("parameters/Transition/current", 0);
    }

    public void HitTargetEvent()
    {
        if (_currentTarget != null)
        {
            var enemy = _currentTarget as Enemy;
            enemy.GetHit(GlobalTransform.origin);
        }
    }

    public void ShowObjectEvent(string nodePath)
    {
        GetNode<Spatial>(nodePath).Visible = true;
    }

    public void HideObjectEvent(string nodePath)
    {
        GetNode<Spatial>(nodePath).Visible = false;
    }

    public void Hit(Vector3 hitPosition)
    {
        _isHit = true;
        _hitTimer = .5f;
        _isAttacking = false;
        _animationTree.Set("parameters/Transition/current", 5);
        _movementSystem.GetHit(Vector3.Forward * 15f);
        hitPosition.y = GlobalTransform.origin.y;
        PlayerLookAt(hitPosition);
    }
}