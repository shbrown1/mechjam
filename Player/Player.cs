using Godot;

public class Player : KinematicBody
{
    public bool IsPlayingCinematic = true;
    public float MouseSensitivity = 0.2f;
    public bool IsFightingBoss;

    private AnimationTree _animationTree;
    private Spatial _cameraHolder;
    private Camera _camera;

    private MovementSystem _movementSystem;
    private CPUParticles[] _jetParticles;
    private AudioStreamPlayer3D _baseEngineSoundPlayer;

    public bool _isAttacking;
    private float _attackTimer;
    private bool _isHit;
    private float _hitTimer;
    private CPUParticles _hitParticles;

    private Spatial _currentTarget;

    private MechaGundam _mechaBoss;
    private Spatial _mechaBossTarget;

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

        _mechaBoss = GetTree().Root.FindNode("MechaGundam", true, false) as MechaGundam;
        _mechaBossTarget = _mechaBoss.GetNode<Spatial>("CameraTarget");

        _baseEngineSoundPlayer = GetNode<AudioStreamPlayer3D>("BaseEngineSoundPlayer");
        _jetParticles[0].ScaleAmount = 0;
        _jetParticles[1].ScaleAmount = 0;
        _hitParticles.Emitting = false;
        _baseEngineSoundPlayer.Playing = true;
        UpdateJetEngineLevel(0);
    }

    public override void _Process(float delta)
    {
        if (IsPlayingCinematic)
            return;

        if (Input.IsActionJustPressed("mouse_capture"))
        {
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
        }

        if (IsFightingBoss)
        {
            PlayerLookAt(_mechaBossTarget.GlobalTranslation);
        }

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
            if (_currentTarget != null && Object.IsInstanceValid(_currentTarget))
            {
                PlayerLookAt(_currentTarget.GlobalTransform.origin);
            }

            if (_currentTarget != null && _movementSystem.MoveVector == Vector3.Zero)
                _animationTree.Set("parameters/Transition/current", 4);

            _attackTimer += delta;
            if (_attackTimer > 1.5f)
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
                    if (body is ThrownPeice thrownPeice && !thrownPeice.CameraLockOn)
                    {
                        _currentTarget = thrownPeice;
                        thrownPeice.ReadyForHit();
                    }

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
        _hitParticles.Emitting = _isHit || _movementSystem.IsBoosting() || _isAttacking;

        UpdateJetEngineLevel(jetScale / .3f);

        var translation = GlobalTransform.origin;
        translation.y = 0;
        GlobalTranslation = translation;
    }

    public override void _Input(InputEvent @event)
    {
        if (_isHit || IsPlayingCinematic)
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
            if (_currentTarget is Enemy enemy)
                enemy.GetHit(GlobalTransform.origin);
            else if (_currentTarget is ThrownPeice thrownPeice)
                thrownPeice.GetHit();
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

    public void Hit(Vector3 hitPosition, float force = 15f)
    {
        _isHit = true;
        _hitTimer = .5f;
        _isAttacking = false;
        _animationTree.Set("parameters/Transition/current", 5);
        _movementSystem.GetHit(Vector3.Forward * force);
        HideObjectEvent("Attack1Slash/Attack1Swing1");
        HideObjectEvent("Attack1Slash/Attack1Swing2");
        hitPosition.y = GlobalTransform.origin.y;
        PlayerLookAt(hitPosition);
    }

    public void StartFightingBoss()
    {
        IsFightingBoss = true;
        var camera = _camera as CameraSystem;
        camera.SetCameraState(CameraSystem.CameraState.LookingAtBoss);
    }

    public void UpdateJetEngineLevel(float level)
    {
        float volumeDb = Mathf.Lerp(-80f, 5f, level);
        _baseEngineSoundPlayer.UnitDb = volumeDb;
    }

    public void EmitAttackParticles()
    {
        var particles = GetNode<CPUParticles>("AttackParticles").Emitting = true;
    }
}