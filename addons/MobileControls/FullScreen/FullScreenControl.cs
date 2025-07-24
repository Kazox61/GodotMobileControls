using Godot;

namespace GodotMobileControls;

[Tool, GlobalClass, Icon("res://addons/MobileControls/Icons/FullScreenControl.svg")]
public partial class FullScreenControl : Control {
	public override void _Notification(int what) {
		if (what == NotificationReady) {
			UpdateMinimumSize();
		}
	}

	public override Vector2 _GetMinimumSize() {
		if (!IsInsideTree()) {
			return Vector2.Zero;
		}

		var tree = GetTree();
		var window = tree?.Root;

		if (window == null || Engine.IsEditorHint()) {
			var width = ProjectSettings.GetSetting("display/window/size/viewport_width").AsInt32();
			var height = ProjectSettings.GetSetting("display/window/size/viewport_height").AsInt32();
			return new Vector2(width, height);
		}

		var aspectSize = window.Size;
		var aspectRatio = aspectSize.Aspect();

		var desiredRes = window.ContentScaleSize;
		var desiredAspect = desiredRes.Aspect();

		var minimumSize = Vector2.Zero;

		if (desiredAspect < aspectRatio) {
			minimumSize.X = desiredRes.Y * aspectRatio;
			minimumSize.Y = desiredRes.Y;
		} 
		else {
			minimumSize.X = desiredRes.X;
			minimumSize.Y = desiredRes.X / aspectRatio;
		}

		return minimumSize;
	}
}