using Godot;

namespace GodotMobileControls;

[Tool, GlobalClass, Icon("res://addons/MobileControls/Icons/SafeAreaContainer.svg")]
public partial class SafeAreaContainer : MarginContainer {

#if TOOLS
	public override void _EnterTree() {
		if (!Engine.IsEditorHint()) {
			return;
		}

		Callable.From(ConfigureFullScreenLayout).CallDeferred();
	}
#endif
	
	public override void _Ready() {
		ApplySafeAreaMargins();
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