using Godot;
using System;

public class TitleScreenScript : CanvasLayer
{
    public override void _PhysicsProcess(float delta)
    {
        foreach (KeyList key in Enum.GetValues(typeof(KeyList)))
        {
            if ((int)key != 0 && Input.IsKeyPressed((int)key))
            {
                var mechaGundam = GetTree().Root.FindNode("MechaGundam", true, false) as MechaGundam;
                mechaGundam.CloseTitleScreen();
                QueueFree();
                break;
            }
        }
    }
}