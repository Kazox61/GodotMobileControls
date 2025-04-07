using Godot;

namespace GodotMobileControls;

[GlobalClass, Icon("res://addons/MobileControls/Icons/SafeAreaContainer.svg")]
public partial class SafeAreaExpand : Control {
	public enum ExpandPositionEnum {
		Left,
		Top,
		Right,
		Bottom
	}

	[Export] public ExpandPositionEnum ExpandPosition = ExpandPositionEnum.Top;

	public override void _Ready() {
		CustomMinimumSize = GetMinSize();
	}

	public override Vector2 _GetMinimumSize() {
		return GetMinSize();
	}

	private Vector2 GetMinSize() {
		var safeArea = DisplayServer.GetDisplaySafeArea();
		var screenSize = DisplayServer.ScreenGetSize();

		return ExpandPosition switch {
			ExpandPositionEnum.Top => new Vector2(0, safeArea.Position.Y),
			ExpandPositionEnum.Left => new Vector2(safeArea.Position.X, 0),
			ExpandPositionEnum.Bottom => new Vector2(0, screenSize.Y - safeArea.End.Y),
			ExpandPositionEnum.Right => new Vector2(screenSize.X - safeArea.End.X, 0),
			_ => Vector2.Zero
		};
	}
}