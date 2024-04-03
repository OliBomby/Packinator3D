using System;
using System.Collections.Generic;
using Godot;
using Packinator3D.datastructure;
using Packinator3D.scenes.puzzle;

namespace Packinator3D.scenes.menus.main;

public partial class PuzzleBackground : Node3D {
    private Camera3D camera;

    private float cameraRadius = 10.0f;
    private readonly List<Vector3> cogs = new();
    private float currentAngle;
    private readonly List<Vector3> endPositions = new();

    private float explodeFactor = 2.0f;
    private bool explodeForwards = true;
    private float explodeSpeed = 0.1f;
    private float explodeState;
    private bool loaded;
    private PuzzleNode puzzleNode;
    private float rotationSpeed = 0.1f;
    private readonly List<Vector3> startingPositions = new();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        puzzleNode = new PuzzleNode();
        camera = GetNode<Camera3D>("../Camera3D");
        AddChild(puzzleNode);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        // This fixes some race condition
        if (!loaded && SaveManager.SaveData.Puzzles.Count > 0) {
            loaded = true;
            var random = new Random();
            puzzleNode.LoadData(SaveManager.SaveData.Puzzles[random.Next(SaveManager.SaveData.Puzzles.Count)], 0);

            ComputeCenterOfGravityPieces();
            var centerOfGravity = ComputeCenterOfGravity();
            puzzleNode.Transform = puzzleNode.Transform.Translated(-centerOfGravity);

            var i = 0;
            foreach (var ppn in puzzleNode.PuzzlePieceNodes) {
                startingPositions.Add(ppn.Position);
                endPositions.Add(cogs[i] * explodeFactor);
                i += 1;
            }

            centerOfGravity = ComputeCenterOfGravity();
        }

        currentAngle += (float)delta * rotationSpeed;
        var cam_pos = camera.Position;
        cam_pos.X = cameraRadius * (float)Math.Cos(currentAngle);
        cam_pos.Z = cameraRadius * (float)Math.Sin(currentAngle);
        camera.Position = cam_pos;

        camera.LookAt(new Vector3());

        if (explodeForwards) {
            explodeState += explodeSpeed * (float)delta;
            if (explodeState > 1.0f) explodeForwards = false;
        }
        else {
            explodeState -= explodeSpeed * (float)delta;
            if (explodeState < 0.0f) explodeForwards = true;
        }

        for (var i = 0; i < puzzleNode.PuzzlePieceNodes.Count; i++)
            puzzleNode.PuzzlePieceNodes[i].Position =
                startingPositions[i].Lerp(endPositions[i], explodeState);
    }

    private Vector3 ComputeCenterOfGravity() {
        Vector3 sum = new();
        foreach (var cog in cogs) sum += cog;

        return sum / cogs.Count;
    }

    private void ComputeCenterOfGravityPieces() {
        cogs.Clear();
        foreach (var ppn in puzzleNode.PuzzlePieceNodes) {
            Vector3 sum = new();
            var shape = ppn.PieceData.Shape;
            foreach (var p in shape) sum += ppn.Transform * p;
            cogs.Add(sum / shape.Count);
        }
    }
}