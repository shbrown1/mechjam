using Godot;
using System;

public class CameraSystem : Camera
{
    [Export]
    private NodePath _playerNodePath;

    public enum CameraState
    {
        Off,
        FollowingPlayer,
        PlayingCinematic,
    }

    private CameraState _cameraState = CameraState.FollowingPlayer;
    private Spatial _playerCameraHolder;

    public override void _Ready()
    {
        var player = GetNode(_playerNodePath);
        _playerCameraHolder = player.GetNode<Spatial>("CameraHolder");
        SnapToPlayer();
    }

    public override void _Process(float delta)
    {
        if (_cameraState == CameraState.FollowingPlayer)
        {
            var _cameraMovementLerpSpeed = 50f;
            var cameraHolderTranslation = _playerCameraHolder.GlobalTranslation;
            GlobalTranslation = GlobalTranslation.LinearInterpolate(cameraHolderTranslation, Mathf.Clamp(delta * _cameraMovementLerpSpeed, 0, 1));

            var _cameraRotationLerpSpeed = 100f;
            var cameraHolderBasis = new Basis(_playerCameraHolder.GlobalRotation);
            var currentBasis = new Basis(GlobalRotation);
            currentBasis = currentBasis.Slerp(cameraHolderBasis, Mathf.Clamp(delta * _cameraRotationLerpSpeed, 0, 1));
            GlobalRotation = currentBasis.GetEuler();
        }
    }

    public void SetCameraState(CameraState cameraState)
    {
        _cameraState = cameraState;
    }

    public void SnapToPlayer()
    {
        GlobalTranslation = _playerCameraHolder.GlobalTranslation;
        GlobalRotation = _playerCameraHolder.GlobalRotation;
    }
}