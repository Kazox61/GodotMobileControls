using Godot;

namespace GodotMobileControls.Examples;

public partial class Example : Control {
	[Export] private AnimatedGridContainer _animatedGridContainer;
	[Export] private StyleBoxMobileButton _styleBoxMobileButton;

	public override void _EnterTree() {
		_styleBoxMobileButton.TouchPress += OnTouchPress;
	}

	public override void _ExitTree() {
		_styleBoxMobileButton.TouchPress -= OnTouchPress;
	}
	
	private void OnTouchPress() {
		var childCount = _animatedGridContainer.GetChildCount();
		var randomIndex = GD.RandRange(0, childCount - 1);
		var randomChild = _animatedGridContainer.GetChild(randomIndex);
		_animatedGridContainer.RemoveChild(randomChild);
	}
}