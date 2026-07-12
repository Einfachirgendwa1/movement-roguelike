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


        GridMap topMap = gridMap.HasNode("TopMap") ? gridMap.GetNode<GridMap>("TopMap") : CreateTopMap();

        (Vector3I start, Vector3I end) = GetGridMapBounds();
        int xDist = end.X - start.X;
        int zDist = end.Z - start.Z;
        end.Y += Math.Min(xDist, zDist);

        EditorUndoRedoManager? undoRedo = GetUndoRedo();
        undoRedo.CreateAction("Build Shell");

        for (int x = start.X; x <= end.X; x++) {
            for (int y = start.Y; y <= end.Y; y++) {
                for (int z = start.Z; z <= end.Z; z++) {
                    if (OnShell(x, start, end, y, z) && gridMap.GetCellItem(new Vector3I(x, y, z)) != 0) {
                        Vector3I cell = new(x, y, z);
                        int oldItem = gridMap.GetCellItem(cell);

                        undoRedo.AddDoMethod(topMap, GridMap.MethodName.SetCellItem, cell, 1);
                        undoRedo.AddUndoMethod(topMap, GridMap.MethodName.SetCellItem, cell, oldItem);
                    }
                }
            }
        }

        undoRedo.CommitAction();
    }

    private GridMap CreateTopMap() {
        GridMap topMap = new();
        topMap.MeshLibrary = gridMap!.MeshLibrary;
        gridMap.AddChild(topMap);
        topMap.Owner = EditorInterface.Singleton.GetEditedSceneRoot();
        topMap.CellSize = gridMap.CellSize;
        topMap.Name = "TopMap";

        return topMap;
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