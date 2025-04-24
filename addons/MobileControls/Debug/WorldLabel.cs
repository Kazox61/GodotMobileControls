using Godot;

namespace GodotMobileControls.Debug;

[GlobalClass]
public partial class WorldToScreen : Node {
	[Export] private Node3D _anchor;
	
	public override void _Process(double delta) {
		GetParent<Control>().GlobalPosition = GetViewport().GetCamera3D().UnprojectPosition(_anchor.Position);
	}
}