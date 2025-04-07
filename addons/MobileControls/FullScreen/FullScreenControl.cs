using Godot;

namespace GodotMobileControls;

[Tool, GlobalClass, Icon("res://addons/MobileControls/Icons/FullScreenControl.svg")]
public partial class FullScreenControl : Control {
	public override void _Ready() {
		if (!IsInsideTree()) {
			return;
		}

		var window = GetTree()?.Root;
		if (window == null) {
			return;
		}

		CustomMinimumSize = GetScreenSize(window);
	}

	public override Vector2 _GetMinimumSize() {
		if (!IsInsideTree()) {
			return CustomMinimumSize;
		}

		var window = GetTree()?.Root;
		return window != null ? GetScreenSize(window) : CustomMinimumSize;
	}
	
	private Vector2 GetScreenSize(Window window) {
		if (window == null) {
			return Vector2.Zero;
		}
		
		Vector2 minimumSize;
		if (Engine.IsEditorHint()) {
			var width = ProjectSettings.GetSetting("display/window/size/viewport_width").AsInt32();
			var height = ProjectSettings.GetSetting("display/window/size/viewport_height").AsInt32();
			minimumSize = new Vector2(width, height);
			return minimumSize;
		}
		
		var aspectSize = window.Size.Aspect();
		var aspectContentScaleSize = window.ContentScaleSize.Aspect();
		var desiredRes = window.ContentScaleSize;

		minimumSize = Vector2.Zero;
		if (aspectContentScaleSize < aspectSize) {
			minimumSize.X = desiredRes.Y * aspectSize;
			minimumSize.Y = desiredRes.Y;
		}
		else {
			minimumSize.X = desiredRes.X;
			minimumSize.Y = desiredRes.X / aspectSize;
		}

		return minimumSize;
	}
}