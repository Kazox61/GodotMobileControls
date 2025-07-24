using Godot;

namespace GodotMobileControls;

[Tool, GlobalClass, Icon("res://addons/MobileControls/Icons/MobileButton.svg")]
public partial class StyleBoxMobileButton : MobileButton {
	private StyleBox _normal;

	[Export]
	public StyleBox Normal {
		get => _normal;
		set {
			if (_normal != null) {
				_normal.Changed -= QueueRedraw;
			}

			_normal = value;

			if (_normal != null) {
				_normal.Changed += QueueRedraw;
			}

			QueueRedraw();
		}
	}

	private StyleBox _pressed;

	[Export]
	public StyleBox Pressed {
		get => _pressed;
		set {
			if (_pressed != null) {
				_pressed.Changed -= QueueRedraw;
			}

			_pressed = value;

			if (_pressed != null) {
				_pressed.Changed += QueueRedraw;
			}

			QueueRedraw();
		}
	}

	private StyleBox _disabled;

	[Export]
	public StyleBox Disabled {
		get => _disabled;
		set {
			if (_disabled != null) {
				_disabled.Changed -= QueueRedraw;
			}

			_disabled = value;

			if (_disabled != null) {
				_disabled.Changed += QueueRedraw;
			}

			QueueRedraw();
		}
	}

	private StyleBox _current;

	public override void _Draw() {
		_current = _normal;
		if (_disabled != null && TouchDisabled) {
			_current = _disabled;
		}
		else if (_pressed != null && ButtonPressed) {
			_current = _pressed;
		}

		if (_current == null) {
			return;
		}

		var rect = new Rect2(Vector2.Zero, Size);
		DrawStyleBox(_current, rect);
	}
}