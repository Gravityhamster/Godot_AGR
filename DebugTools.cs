using Godot;
using System;

public partial class DebugTools : Node3D
{
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Debug-Reload"))
        {
            GetTree().ReloadCurrentScene();
        }
    }
}
