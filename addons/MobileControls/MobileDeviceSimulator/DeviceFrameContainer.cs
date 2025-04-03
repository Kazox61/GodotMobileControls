using Godot;

namespace GodotMobileControls;

[Tool]
public partial class DeviceFrameContainer : TextureRect {
	private Node _parent;
	private bool _landscapeOrientation;

	public override void _EnterTree() {
		_parent = GetParent();
		
		_parent.ChildEnteredTree += OnChildEnteredTree;
	}

	private void OnChildEnteredTree(Node node) {
		Callable.From(() => _parent.MoveChild(this, _parent.GetChildCount() - 1)).CallDeferred();
	}

	public override void _ExitTree() {
		_parent.ChildEnteredTree -= OnChildEnteredTree;
	}

	public void RotateToLandscape() {
		if (_landscapeOrientation) {
			return;
		}
		
		_landscapeOrientation = true;
		RotationDegrees = -90;
		PivotOffset = Texture.GetSize() / 2;
	}
	
	public void RotateToPortrait() {
		if (!_landscapeOrientation) {
			return;
		}
		
		_landscapeOrientation = false;
		RotationDegrees = 0;
		PivotOffset = Texture.GetSize() / 2;
	}

	public void UpdateDevice(DeviceSetup deviceSetup) {
		Texture = deviceSetup.DeviceFrame;
		
		PivotOffset = Texture.GetSize() / 2;
		GD.Print(PivotOffset);
	}
}