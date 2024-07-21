using Godot;
using System;

public partial class visual_part_of_car : MeshInstance3D
{
	[Export]
	public CarBuiltInPhysics mycar = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Transform3D xform = mycar.GlobalTransform;
		xform = xform.Rotated(xform.Basis.X, (float)(90 * (Math.PI / 180)));//Rotate(GlobalTransform.Basis.X, (float)(90 * (Math.PI / 180)));
		GlobalTransform = GlobalTransform.InterpolateWith(xform, (float)(5 * delta));
		GlobalPosition = mycar.GlobalPosition;

		Rotate(GlobalTransform.Basis.Z.Normalized(), -mycar.rotateChange);
	}
}
