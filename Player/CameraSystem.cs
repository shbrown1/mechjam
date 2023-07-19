using Godot;
using System;

public class CameraSystem : Camera
{
    [Export]
    private NodePath _playerNodePath;
    [Export]
    private NodePath _mechaGundamNodePath;

    public enum CameraState
    {
        Off,
        FollowingPlayer,
        PlayingCinematic,
        LookingAtBoss,
    }

    private CameraState _cameraState = CameraState.FollowingPlayer;
    private Player _player;
    private Spatial _playerCameraHolder;
    private Spatial _mechaCameraTarget;

    public override void _Ready()
    {
        _player = GetNode(_playerNodePath) as Player;
        _playerCameraHolder = _player.GetNode<Spatial>("CameraHolder");

        var mechaGundam = GetNode(_mechaGundamNodePath);
        _mechaCameraTarget = mechaGundam.GetNode<Spatial>("CameraTarget");

        SnapToPlayer();
    }

    public override void _Process(float delta)
    {
        if (_cameraState == CameraState.FollowingPlayer)
        {
            var _cameraMovementLerpSpeed = 20f;
            var cameraHolderTranslation = _playerCameraHolder.GlobalTranslation;
            GlobalTranslation = GlobalTranslation.LinearInterpolate(cameraHolderTranslation, Mathf.Clamp(delta * _cameraMovementLerpSpeed, 0, 1));

            var _cameraRotationLerpSpeed = 20f;
            var cameraHolderBasis = new Basis(_playerCameraHolder.GlobalRotation);
            var currentBasis = new Basis(GlobalRotation);
            currentBasis = currentBasis.Slerp(cameraHolderBasis, Mathf.Clamp(delta * _cameraRotationLerpSpeed, 0, 1));
            GlobalRotation = currentBasis.GetEuler();
        }
        else if (_cameraState == CameraState.LookingAtBoss)
        {
            var _cameraMovementLerpSpeed = 16f;
            var mechaTarget = _mechaCameraTarget.GlobalTranslation;
            var direction = _player.GlobalTranslation - new Vector3(mechaTarget.x, _player.GlobalTranslation.y, mechaTarget.z);
            direction = direction.Normalized();
            var desiredTranlation = _player.GlobalTranslation + (Vector3.Up * 2) + (direction * 2f);
            GlobalTranslation = GlobalTranslation.LinearInterpolate(desiredTranlation, Mathf.Clamp(delta * _cameraMovementLerpSpeed, 0, 1));
            var _cameraRotationLerpSpeed = 15f;
            var currentBasis = new Basis(GlobalRotation);
            LookAt(_mechaCameraTarget.GlobalTranslation, Vector3.Up);
            var desiredBasis = new Basis(GlobalRotation);
            currentBasis = currentBasis.Slerp(desiredBasis, Mathf.Clamp(delta * _cameraRotationLerpSpeed, 0, 1));
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