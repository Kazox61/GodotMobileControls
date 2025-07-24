using Godot;

namespace GodotMobileControls;

[Tool, GlobalClass, Icon("res://addons/MobileControls/Icons/SafeAreaContainer.svg")]
public partial class SafeAreaContainer : MarginContainer {
	public override void _Notification(int what) {
		if (what == NotificationEnterTree) {
			Callable.From(ConfigureFullScreenLayout).CallDeferred();
			ApplySafeAreaMargins();
		}
	}
	
	private void ConfigureFullScreenLayout() {
		LayoutMode = 1; // Anchors
		SetAnchorsPreset(LayoutPreset.FullRect);
		OffsetLeft = 0f;
		OffsetTop = 0f;
		OffsetRight = 0f;
		OffsetBottom = 0f;
	}

	private void ApplySafeAreaMargins() {
		if (!OS.HasFeature("mobile")) {
			return;
		}
		
		var safeArea = DisplayServer.GetDisplaySafeArea();
		var screenSize = DisplayServer.ScreenGetSize();

		AddThemeConstantOverride("margin_top", safeArea.Position.Y);
		AddThemeConstantOverride("margin_left", safeArea.Position.X);
		AddThemeConstantOverride("margin_bottom", screenSize.Y - safeArea.End.Y);
		AddThemeConstantOverride("margin_right", screenSize.X - safeArea.End.X);
	}
}