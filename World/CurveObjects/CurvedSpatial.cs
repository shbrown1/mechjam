using Godot;
using System;

public class CurvedSpatial : Spatial
{
    public static float Curvature = .001f;

    private Spatial _camera;
    private float _originalY;
    private bool _hasCurvedShader;

    public override void _Ready()
    {
        _camera = GetTree().Root.FindNode("Camera", true, false) as Spatial;
        _originalY = GlobalTranslation.y;
    }

    public override void _Process(float delta)
    {
        var worldPosition = new Vector3(GlobalTranslation.x, 0, GlobalTranslation.z);
        var cameraPosition = new Vector3(_camera.GlobalTranslation.x, 0, _camera.GlobalTranslation.z);
        var distance = worldPosition.DistanceTo(cameraPosition);
        var y = distance * distance * Curvature;
        GlobalTranslation = new Vector3(GlobalTranslation.x, _originalY - y, GlobalTranslation.z);
    }
}