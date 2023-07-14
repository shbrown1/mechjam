using Godot;
using System;

public class MechaGundam : Spatial
{
    [Export]
    private NodePath _fallingSprite;
    [Export]
    private NodePath _mechaGundam;
    private CameraSystem _camera;
    private Player _player;
    private bool _hasFallen = false;
    private bool _playerHasEntered = false;

    public override void _Ready()
    {
        _camera = GetTree().Root.FindNode("Camera", true, false) as CameraSystem;
        _camera.SetCameraState(CameraSystem.CameraState.Off);
        _player = GetTree().Root.FindNode("Player", true, false) as Player;

        var fallenSprite = GetNode<Spatial>(_mechaGundam);
        fallenSprite.Visible = false;
    }

    public override void _Process(float delta)
    {
        if (!_hasFallen)
        {
            _camera = GetTree().Root.FindNode("Camera", true, false) as CameraSystem;
            _camera.LookAt(GetNode<Spatial>(_fallingSprite).GlobalTranslation, Vector3.Up);

            var fallingSprite = GetNode<Spatial>(_fallingSprite);
            fallingSprite.Translate(Vector3.Down * 8 * delta);

            if (fallingSprite.GlobalTranslation.y < -5)
            {
                FinishFallingAnimation();
            }
        }
        else if (!_playerHasEntered)
        {
            var fallenSprite = GetNode<Spatial>(_mechaGundam);
            fallenSprite.GlobalTranslation = new Vector3(fallenSprite.GlobalTranslation.x, fallenSprite.GlobalTranslation.x, _player.GlobalTranslation.z + 100);
            fallenSprite.Scale = Vector3.One * fallenSprite.GlobalTranslation.z / 10;
            if (fallenSprite.GlobalTranslation.z > 600)
            {
                _playerHasEntered = true;
            }
        }
    }

    public void FinishFallingAnimation()
    {
        _hasFallen = true;

        _camera = GetTree().Root.FindNode("Camera", true, false) as CameraSystem;
        _camera.SetCameraState(CameraSystem.CameraState.FollowingPlayer);

        var fallenSprite = GetNode<Spatial>(_mechaGundam);
        var fallingSprite = GetNode<Spatial>(_fallingSprite);
        fallenSprite.Visible = true;
        fallingSprite.Visible = false;
        fallenSprite.GlobalTranslation = new Vector3(fallingSprite.GlobalTranslation.x, fallenSprite.GlobalTranslation.y, 100);
    }
}