using Godot;
using System;

public partial class visual_part_of_car : MeshInstance3D
{
	[Export]
	public CarBuiltInPhysics mycar = null;
	public float VerticalDisplacementHard = 0;
	public float VerticalDisplacementVelocity = 0;
	public float DisplacementDampen = 1/1.0625f;
	public float VerticalDisplacementSoft = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// /* old stable rotation code: 
		Transform3D xform = mycar.GlobalTransform;
		xform = xform.Rotated(xform.Basis.X, (float)(90 * (Math.PI / 180)));//Rotate(GlobalTransform.Basis.X, (float)(90 * (Math.PI / 180)));
		GlobalTransform = xform;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// /* old stable rotation code: 
		Transform3D xform = mycar.GlobalTransform;
		xform = xform.Rotated(xform.Basis.X, (float)(90 * (Math.PI / 180)));//Rotate(GlobalTransform.Basis.X, (float)(90 * (Math.PI / 180)));
		GlobalTransform = GlobalTransform.InterpolateWith(xform, (float)(5 * delta));
		// */
		//GlobalTransform = GlobalTransform.Rotated(xform.Basis.Z, GlobalTransform.Basis.Y.Dot(xform.Basis.Z));
		//GlobalTransform = GlobalTransform.Rotated(xform.Basis.X, GlobalTransform.Basis.X.Dot(xform.Basis.X));

		// Set the position of the model
		GlobalPosition = mycar.GlobalPosition;

		// Suspension physics - Vertical_Linear
		// Set vertical force
		VerticalDisplacementHard += mycar.WasYVel/50;
		// Apply force overtime to approach rest
		VerticalDisplacementVelocity += VerticalDisplacementHard / 100;
		VerticalDisplacementHard += -VerticalDisplacementVelocity;
		// Dampen
		VerticalDisplacementVelocity *= DisplacementDampen;
		// Apply suspension force		
		VerticalDisplacementSoft += (VerticalDisplacementHard - VerticalDisplacementSoft) * 0.4f;
		VerticalDisplacementSoft = Math.Clamp(VerticalDisplacementSoft, -0.5f, 2f);
		GlobalPosition = GlobalPosition + VerticalDisplacementSoft * -GlobalTransform.Basis.Z;

		// Rotate model with rotation from car
		Rotate(GlobalTransform.Basis.Z.Normalized(), -mycar.rotateChange);
		Rotate(GlobalTransform.Basis.Y.Normalized(), mycar.rollChange);
		Rotate(GlobalTransform.Basis.X.Normalized(), mycar.pitchChange);

	}
}
