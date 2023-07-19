using Godot;
using System;

public class HitEffect : Spatial
{

    public override void _Ready()
    {
        base._Ready();
        GetNode<CPUParticles>("ExplodeEffect1").Emitting = true;
        GetNode<CPUParticles>("ExplodeEffect2").Emitting = true;
    }
}
