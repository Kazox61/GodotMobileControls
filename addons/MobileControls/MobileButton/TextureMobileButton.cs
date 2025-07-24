using Godot;

namespace GodotMobileControls;

[Tool, GlobalClass, Icon("res://addons/MobileControls/Icons/TextureMobileButton.svg")]
public partial class TextureMobileButton : MobileButton {
	private Texture2D _normal;

	[Export]
	public Texture2D Normal {
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

	private Texture2D _pressed;

	[Export]
	public Texture2D Pressed {
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

	private Texture2D _disabled;

	[Export]
	public Texture2D Disabled {
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

	private Texture2D _current;

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
		DrawTextureRect(_current, rect, false);
	}
}