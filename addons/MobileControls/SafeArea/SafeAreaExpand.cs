using Godot;

namespace GodotMobileControls;

[Tool, GlobalClass, Icon("res://addons/MobileControls/Icons/SafeAreaContainer.svg")]
public partial class SafeAreaExpand : Control {
	public enum ExpandPositionEnum {
		Left,
		Top,
		Right,
		Bottom
	}

	private ExpandPositionEnum _expandPosition = ExpandPositionEnum.Top;

	[Export]
	public ExpandPositionEnum ExpandPosition {
		get => _expandPosition;
		set {
			if (_expandPosition == value) {
				return;
			}

			_expandPosition = value;
			UpdateMinimumSize();
		}
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