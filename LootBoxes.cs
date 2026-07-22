using MovementRoguelike3D.Player;

public partial class LootBoxes : Area3D {
        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
            BodyEntered += OnBodyEntered;
        }

        private void OnBodyEntered(Node3D body) {
            GD.Print("Entered");
            AnimationPlayer animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            animationPlayer.Play("chestOpen");
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) { }
}