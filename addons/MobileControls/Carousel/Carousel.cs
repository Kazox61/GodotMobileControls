using Godot;

namespace GodotMobileControls.Carousel;

[Tool, GlobalClass, Icon("res://addons/MobileControls/Icons/HorizontalCarousel.svg")]
public partial class Carousel : Control {
	private int _gap;
	[Export] public int Gap {
		get => _gap;
		set {
			_gap = value;
			//_itemsContainer.AddThemeConstantOverride("separation", _gap);
		}
	}
}