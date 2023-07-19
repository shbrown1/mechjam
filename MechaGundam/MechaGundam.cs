using Godot;
using System;
using System.Collections.Generic;

public class MechaGundam : CurvedSpatial
{
    [Export]
    private NodePath _fallingSprite;
    [Export]
    private NodePath _throwSpawnNodePath;
    [Export]
    private PackedScene _thrownPeice;
    [Export]
    private PackedScene _hitEffect;

    private AnimationPlayer _animationPlayer;
    private CameraSystem _camera;
    private Player _player;
    private bool _hasFallen = false;
    private bool _playerHasEntered = false;
    private bool _isActive = false;
    private float _throwCooldown;
    private Spatial _throwSpawn;
    private Vector3 _cameraTargetStartPosition;
    private Spatial _cameraTarget;
    private ThrownPeice _currentThrownPeice;
    private int _health = 3;
    private bool _isDead;
    private bool _titleScreenIsOpen = true;
    private List<ThrownPeice> _allThrownPeices = new List<ThrownPeice>();
    private float _explosionDeathTimer;
    private float _missionAccomplishedTimer = 5f;
    private float _resetGameTimer = 20f;

    public override void _Ready()
    {
        base._Ready();

        _camera = GetTree().Root.FindNode("Camera", true, false) as CameraSystem;
        _camera.SetCameraState(CameraSystem.CameraState.Off);
        _player = GetTree().Root.FindNode("Player", true, false) as Player;

        _animationPlayer = GetNode<AnimationPlayer>("mecha-gundam/AnimationPlayerEvents");
        HideObjectEvent("mecha-gundam/SwingCurve1");
        HideObjectEvent("mecha-gundam/SwingCurve2");

        _throwSpawn = GetNode<Spatial>(_throwSpawnNodePath);
        _throwCooldown = 5f;
        _cameraTarget = GetNode<Spatial>("CameraTarget");
    }

    public override void _Process(float delta)
    {
        if (_titleScreenIsOpen)
            return;

        if (_isDead)
        {
            GlobalTranslation += Vector3.Down * 2 * delta;
            _explosionDeathTimer -= delta;
            if (_explosionDeathTimer < 0)
            {
                var hit = _hitEffect.Instance() as HitEffect;
                AddChild(hit);
                hit.GlobalTranslation = GlobalTranslation;
                _explosionDeathTimer = (float)GD.RandRange(10, 20) / 10f;
            }

            if (_missionAccomplishedTimer > 0)
                _missionAccomplishedTimer -= delta;
            if (_missionAccomplishedTimer < 0)
            {
                var userInterface = GetTree().Root.FindNode("UserInterface", true, false) as UserInterface;
                userInterface.ShowMissionAccomplished();
            }

            _resetGameTimer -= delta;
            if (_resetGameTimer < 0)
            {
                GetTree().ChangeScene("Main.tscn");
            }

        }
        else if (!_hasFallen)
        {
            var audioPlayer = GetTree().Root.FindNode("FallingSound", true, false) as AudioStreamPlayer;
            if (!audioPlayer.Playing)
                audioPlayer.Play();
            _camera = GetTree().Root.FindNode("Camera", true, false) as CameraSystem;
            _camera.LookAt(GetNode<Spatial>(_fallingSprite).GlobalTranslation, Vector3.Up);

            var fallingSprite = GetNode<Spatial>(_fallingSprite);
            fallingSprite.Translate(Vector3.Down * 10f * delta);

            if (fallingSprite.GlobalTranslation.y < -15)
            {
                FinishFallingAnimation();
            }
        }
        else if (!_playerHasEntered)
        {
            GlobalTranslation = new Vector3(GlobalTranslation.x, GlobalTranslation.x, _player.GlobalTranslation.z + 100);
            Scale = Vector3.One * GlobalTranslation.z / 10;
            base._Process(delta);
            if (GlobalTranslation.z > 600)
            //if (GlobalTranslation.z > 200)
            {
                Scale = Vector3.One * 60;
                _playerHasEntered = true;
                _isActive = true;
                _animationPlayer.Play("Start");
                var player = _player as Player;
                player.StartFightingBoss();
                _cameraTargetStartPosition = _cameraTarget.GlobalTranslation;
            }
        }
        else if (_isActive)
        {
            base._Process(delta);
            LookAt(_player.GlobalTranslation, Vector3.Up);
            var playerDistance = GlobalTranslation.DistanceTo(_player.GlobalTranslation);
            if (playerDistance < 15 && _animationPlayer.CurrentAnimation != "Swipe")
            {
                _animationPlayer.Play("Swipe");
            }

            _throwCooldown -= delta;
            if (_throwCooldown < 0 && _animationPlayer.CurrentAnimation != "Swipe")
            {
                _throwCooldown = 5f;
                _animationPlayer.Play("Throw");
            }
        }

        _cameraTarget.GlobalTranslation = GlobalTranslation + Vector3.Up * 4;
        foreach (var thrownPeice in _allThrownPeices)
        {
            if (thrownPeice.CameraLockOn)
            {
                _cameraTarget.GlobalTranslation = thrownPeice.GlobalTranslation;
                break;
            }
        }
    }

    public void FinishFallingAnimation()
    {
        _hasFallen = true;

        _camera = GetTree().Root.FindNode("Camera", true, false) as CameraSystem;
        _camera.SetCameraState(CameraSystem.CameraState.FollowingPlayer);

        var fallingSprite = GetNode<Spatial>(_fallingSprite);
        Visible = true;
        fallingSprite.Visible = false;
        GlobalTranslation = new Vector3(fallingSprite.GlobalTranslation.x, GlobalTranslation.y, 100);

        var particles = GetNode<CPUParticles>("LandParticles");
        particles.GlobalTranslation = GlobalTranslation;
        particles.Emitting = true;

        var audioPlayer = GetTree().Root.FindNode("MusicStreamPlayer", true, false) as AudioStreamPlayer;
        audioPlayer.Play();

        var userInterface = GetTree().Root.FindNode("UserInterface", true, false) as UserInterface;
        userInterface.ShowObjective();
    }

    public void ShowObjectEvent(string nodePath)
    {
        GetNode<Spatial>(nodePath).Visible = true;
    }

    public void HideObjectEvent(string nodePath)
    {
        GetNode<Spatial>(nodePath).Visible = false;
    }

    public void SwipeEvent()
    {
        var area = GetNode<Area>("SwipeBox");
        foreach (var body in area.GetOverlappingBodies())
        {
            if (body is Player player)
            {
                player.Hit(GlobalTranslation, 40f);
            }
        }
    }

    public void ThrowEvent()
    {
        _currentThrownPeice = _thrownPeice.Instance() as ThrownPeice;
        _allThrownPeices.Add(_currentThrownPeice);
        _currentThrownPeice.GlobalTransform = _throwSpawn.GlobalTransform;
        GetNode("../").CallDeferred("add_child", _currentThrownPeice);
    }

    public void OnThrownPeiceBodyEntered(Node body)
    {
        if (body is Player)
        {
            var player = body as Player;
            if (player._isAttacking)
                return;
            player.Hit(GlobalTranslation);
            _allThrownPeices.Remove(_currentThrownPeice);
            _currentThrownPeice.QueueFree();
            _currentThrownPeice = null;
        }
    }

    public void GetHit(ThrownPeice thrownPeice)
    {
        var effect = _hitEffect.Instance() as HitEffect;
        AddChild(effect);
        effect.GlobalTranslation = thrownPeice.GlobalTranslation;
        _allThrownPeices.Remove(thrownPeice);
        thrownPeice.QueueFree();
        _animationPlayer.Play("Hit");
        _health--;
        if (_health <= 0)
        {
            _isDead = true;
            var cloudParticles = GetNode<CPUParticles>("CloudParticles");
            cloudParticles.Emitting = true;
            _animationPlayer.Play("Dead");
        }
    }

    public void CloseTitleScreen()
    {
        _titleScreenIsOpen = false;
    }
}