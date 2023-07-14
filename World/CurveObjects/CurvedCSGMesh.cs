using Godot;
using System;

[Tool]
public class CurvedCSGMesh : CSGMesh
{
    public override void _Ready()
    {
        var material = (ShaderMaterial)Material;
        if (material != null && material.GetShaderParam("curvature") != null)
        {
            if (Engine.EditorHint)
                material.SetShaderParam("curvature", 0);
            else
                material.SetShaderParam("curvature", CurvedSpatial.Curvature);
        }
    }
}
