using Godot;

namespace GodotMobileControls;

[Tool, GlobalClass, Icon("res://addons/MobileControls/Icons/MobileButton.svg")]
public partial class StyleBoxMobileButton : MobileButton {
	[ExportGroup("StyleBoxes")]
	
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

			if (TouchDisabled) {
				return;
			}
			
			_currentStyleBox = _normal;
			QueueRedraw();
		}
	}
	
	private StyleBox _pressed; 
	[Export] public StyleBox Pressed {
		get => _pressed;
		set {
			if (_pressed != null) {
				_pressed.Changed -= QueueRedraw;
			}
			
			_pressed = value;
			
			if (_pressed != null) {
				_pressed.Changed += QueueRedraw;
			}
		}
	}
	
	private StyleBox _hover;
	[Export] public StyleBox Hover {
		get => _hover;
		set {
			if (_hover != null) {
				_hover.Changed -= QueueRedraw;
			}
			
			_hover = value;
			
			if (_hover != null) {
				_hover.Changed += QueueRedraw;
			}
		}
	}
	
	private StyleBox _disabled;
	[Export] public StyleBox Disabled {
		get => _disabled;
		set {
			if (_disabled != null) {
				_disabled.Changed -= QueueRedraw;
			}
			
			_disabled = value;
			
			if (_disabled != null) {
				_disabled.Changed += QueueRedraw;
			}
			
			if (!TouchDisabled) {
				return;
			}
			
			_currentStyleBox = _disabled;
			QueueRedraw();
		}
	}

	private StyleBox _currentStyleBox;

	public override void _EnterTree() {
		base._EnterTree();

		OnTouchDisabledChanged += OnDisabledChanged;
	}

	public override void _ExitTree() {
		base._ExitTree();
		
		OnTouchDisabledChanged -= OnDisabledChanged;
	}

	public override void _Draw() {
		if (_currentStyleBox == null) {
			return;
		}
		
		var rect = new Rect2(0, 0, Size);
		DrawStyleBox(_currentStyleBox, rect);
	}
	
	private void OnDisabledChanged(bool disabled) {
		_currentStyleBox = disabled ? Disabled : Normal;
		QueueRedraw();
	}
}