using Godot.Collections;

#if TOOLS
// ReSharper disable once IdentifierTypo
namespace MovementRoguelike3D.addons.roombuilderplugin;

[Tool]
public partial class RoomBuilderPlugin : EditorPlugin {
    private Button? buildButton;
    private GridMap? gridMap;

    public override void _EnterTree() {
        buildButton = new Button();
        buildButton.Text = "Build Room";
        buildButton.Pressed += BuildButtonOnPressed;
        buildButton.Visible = false;


        AddControlToContainer(CustomControlContainer.SpatialEditorMenu, buildButton);
    }

    public override void _ExitTree() {
        if (buildButton is not null) {
            RemoveControlFromContainer(CustomControlContainer.SpatialEditorMenu, buildButton);
            buildButton.Free();
        }
    }

    public override bool _Handles(GodotObject @object) => @object is GridMap;

    public override void _Edit(GodotObject @object) {
        gridMap = (GridMap)@object;
    }

    public override void _MakeVisible(bool visible) {
        if (buildButton is not null) buildButton.Visible = visible;
    }

    // ReSharper disable once CognitiveComplexity
    private void BuildButtonOnPressed() {
        if (gridMap == null) return;

        (Vector3I start, Vector3I end) = GetGridMapBounds();
        int xDist = end.X - start.X;
        int zDist = end.Z - start.Z;
        end.Y += Math.Min(xDist, zDist);

        EditorUndoRedoManager? undoRedo = GetUndoRedo();
        undoRedo.CreateAction("Build Shell");

        Material material = ResourceLoader.Load<StandardMaterial3D>("res://Levels/LevelBuilding/wall_material.tres");

        Vector3 localStart = gridMap.MapToLocal(new Vector3I(start.X, start.Y, start.Z));
        Vector3 localEnd = gridMap.MapToLocal(new Vector3I(end.X, end.Y, end.Z));

        GetMesh("WallZLow", material,
            localStart.Lerp(new Vector3(localEnd.X, localEnd.Y, localStart.Z - 2), 0.5f),
            new Vector3(localEnd.X - localStart.X + 1, localEnd.Y - localStart.Y, 1),
            undoRedo
        );

        GetMesh("WallZHigh", material,
            localEnd.Lerp(new Vector3(localStart.X, localStart.Y, localEnd.Z + 2), 0.5f),
            new Vector3(localEnd.X - localStart.X + 1, localEnd.Y - localStart.Y, 1),
            undoRedo
        );

        GetMesh("WallXLow", material,
            localStart.Lerp(new Vector3(localStart.X - 2, localEnd.Y, localEnd.Z), 0.5f),
            new Vector3(1, localEnd.Y - localStart.Y, localEnd.Z - localStart.Z + 1),
            undoRedo
        );

        GetMesh("WallXHigh", material,
            localEnd.Lerp(new Vector3(localEnd.X + 2, localStart.Y, localStart.Z), 0.5f),
            new Vector3(1, localEnd.Y - localStart.Y, localEnd.Z - localStart.Z + 1),
            undoRedo
        );

        GetMesh("Roof", material,
            localEnd.Lerp(new Vector3(localStart.X, localEnd.Y + 1, localStart.Z), 0.5f),
            new Vector3(localEnd.X - localStart.X + 1, 1, localEnd.Z - localStart.Z + 1),
            undoRedo
        );

        undoRedo.CommitAction();
    }

    private void GetMesh(string name, Material material, Vector3 position, Vector3 scale,
        EditorUndoRedoManager undoRedo) {
        if (gridMap is null) return;

        if (gridMap.HasNode(name)) {
            undoRedo.AddDoMethod(gridMap, Node.MethodName.RemoveChild, gridMap.GetNode(name));
            undoRedo.AddUndoMethod(gridMap, Node.MethodName.AddChild, gridMap.GetNode(name));
        }

        StaticBody3D staticBody3D = new();
        staticBody3D.Name = name;

        MeshInstance3D mesh = new();
        mesh.Name = "MeshInstance3D";

        mesh.Mesh = new BoxMesh();
        mesh.Mesh.SurfaceSetMaterial(0, material);

        staticBody3D.Position = position;
        mesh.Scale = scale;

        CollisionShape3D collisionShape3D = new();
        collisionShape3D.Name = "CollisionShape3D";

        BoxShape3D boxShape3D = new();
        collisionShape3D.Shape = boxShape3D;
        boxShape3D.Size = scale;

        UndoRedoParent(undoRedo, staticBody3D, gridMap);
        UndoRedoParent(undoRedo, mesh, staticBody3D);
        UndoRedoParent(undoRedo, collisionShape3D, staticBody3D);
    }

    private static void UndoRedoParent(EditorUndoRedoManager undoRedo, Node child, Node parent) {
        undoRedo.AddDoMethod(parent, Node.MethodName.AddChild, child);
        undoRedo.AddDoMethod(child, Node.MethodName.SetOwner, EditorInterface.Singleton.GetEditedSceneRoot());
        undoRedo.AddUndoMethod(parent, Node.MethodName.RemoveChild, child);
    }

    private static bool OnShell(int x, Vector3I start, Vector3I end, int y, int z) => x == start.X
        || x == end.X
        || y == start.Y
        || y == end.Y
        || z == start.Z
        || z == end.Z;

    private (Vector3I start, Vector3I end) GetGridMapBounds() {
        Array<Vector3I>? usedCells = gridMap!.GetUsedCells();
        if (usedCells.Count == 0) return (Vector3I.Zero, Vector3I.Zero);

        Vector3I minPos = usedCells[0];
        Vector3I maxPos = usedCells[0];

        foreach (Vector3I cell in usedCells) {
            minPos.X = Mathf.Min(minPos.X, cell.X);
            minPos.Y = Mathf.Min(minPos.Y, cell.Y);
            minPos.Z = Mathf.Min(minPos.Z, cell.Z);
            maxPos.X = Mathf.Max(maxPos.X, cell.X);
            maxPos.Y = Mathf.Max(maxPos.Y, cell.Y);
            maxPos.Z = Mathf.Max(maxPos.Z, cell.Z);
        }

        return (minPos, maxPos);
    }
}
#endif