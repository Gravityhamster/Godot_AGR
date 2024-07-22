using Godot;
using System;

public partial class CameraDampener : Node3D
{
    [Export] NodePath TargetNodepath = null;
    Node3D targetNode;
    [Export] public float lerpSpeedX = 0.25f;
    [Export] public float lerpSpeedY = 0.25f;
    [Export] public float lerpSpeedZ = 0.25f;
    [Export] public float angLerpSpeedX = 0.15f;
    [Export] public float angLerpSpeedY = 0.15f;
    [Export] public float angLerpSpeedZ = 0.15f;



    
    
    Camera3D cam;
    
    Vector3 originalPos;


    public override void _Ready()
    {
        cam = GetNode<Camera3D>("Camera3D");
        originalPos = Position;
        targetNode = GetNode<Node3D>(TargetNodepath);
    }

    public override void _Process(double delta)
    {


        Vector3 cameraPosition;
        cameraPosition.X = Mathf.Lerp(Position.X, targetNode.Position.X, lerpSpeedX);

        cameraPosition.Y = Mathf.Lerp(Position.Y, targetNode.Position.Y, lerpSpeedY);

        cameraPosition.Z = Mathf.Lerp(Position.Z, targetNode.Position.Z, lerpSpeedZ);

        Vector3 cameraRotation;

        cameraRotation.X = Mathf.LerpAngle(Rotation.X, targetNode.Rotation.X, angLerpSpeedX) /*+ rotOffsetX*/;
        cameraRotation.Y = Mathf.LerpAngle(Rotation.Y, targetNode.Rotation.Y, angLerpSpeedY) /*+ rotOffsetY*/;
        cameraRotation.Z = Mathf.LerpAngle(Rotation.Z, targetNode.Rotation.Z, angLerpSpeedZ) /*+ rotOffsetZ*/;



        Position = cameraPosition;
        Rotation = cameraRotation;
        //Rotation = targetNode.Rotation;
    }
}
