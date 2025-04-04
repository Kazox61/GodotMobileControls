using Godot;

namespace GodotMobileControls;

[Tool]
public partial class DeviceFrameContainer : TextureRect {
	private DeviceSetup _activeDeviceSetup;
	private bool _landscapeOrientation;
	
	private Node _parent;

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
		
		QueueRedraw();
	}
	
	public void RotateToPortrait() {
		if (!_landscapeOrientation) {
			return;
		}
		
		_landscapeOrientation = false;
		RotationDegrees = 0;
		PivotOffset = Texture.GetSize() / 2;
		
		QueueRedraw();
	}

	public void UpdateDevice(DeviceSetup deviceSetup) {
		_activeDeviceSetup = deviceSetup;
		Texture = _activeDeviceSetup.DeviceFrame;
		
		PivotOffset = Texture.GetSize() / 2;
		
		QueueRedraw();
	}

	public override void _Draw() {
		// var center = Size / 2;
		// Rect2 safeArea;
		// if (_landscapeOrientation) {
		// 	var position = center - new Vector2(
		// 		_activeDeviceSetup.DeviceSize.Y + _activeDeviceSetup.SafeAreaLandscapeLeft, 
		// 		_activeDeviceSetup.DeviceSize.X + _activeDeviceSetup.SafeAreaLandscapeTop
		// 	) / 2;
		// 	var size = new Vector2(
		// 		_activeDeviceSetup.DeviceSize.Y - _activeDeviceSetup.SafeAreaLandscapeLeft - _activeDeviceSetup.SafeAreaLandscapeRight,
		// 		_activeDeviceSetup.DeviceSize.X - _activeDeviceSetup.SafeAreaLandscapeTop - _activeDeviceSetup.SafeAreaLandscapeBottom
		// 	);
		// 	safeArea = new Rect2(position, size);
		// }
		// else {
		// 	var position = center - new Vector2(
		// 		_activeDeviceSetup.DeviceSize.X + _activeDeviceSetup.SafeAreaPortraitLeft, 
		// 		_activeDeviceSetup.DeviceSize.Y + _activeDeviceSetup.SafeAreaPortraitTop
		// 	) / 2;
		// 	var size = new Vector2(
		// 		_activeDeviceSetup.DeviceSize.X - _activeDeviceSetup.SafeAreaPortraitLeft - _activeDeviceSetup.SafeAreaPortraitRight,
		// 		_activeDeviceSetup.DeviceSize.Y - _activeDeviceSetup.SafeAreaPortraitTop - _activeDeviceSetup.SafeAreaPortraitBottom
		// 	);
		// 	safeArea = new Rect2(position, size);
		// }
		// DrawRect(safeArea, new Color(0, 0, 0, 0.5f), false);
	}
}