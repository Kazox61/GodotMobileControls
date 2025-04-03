using Godot;

namespace GodotMobileControls;

[Tool, GlobalClass, Icon("res://addons/MobileControls/Icons/TextureMobileButton.svg")]
public partial class TextureMobileButton : MobileButton {
	[ExportGroup("Textures")]
	
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
			
			UpdateCurrentTexture();
		}
	}
	
	private Texture2D _pressed; 
	[Export] public Texture2D Pressed {
		get => _pressed;
		set {
			if (_pressed != null) {
				_pressed.Changed -= QueueRedraw;
			}
			
			_pressed = value;
			
			if (_pressed != null) {
				_pressed.Changed += QueueRedraw;
			}
			
			UpdateCurrentTexture();
		}
	}
	
	private Texture2D _hover;
	[Export] public Texture2D Hover {
		get => _hover;
		set {
			if (_hover != null) {
				_hover.Changed -= QueueRedraw;
			}
			
			_hover = value;
			
			if (_hover != null) {
				_hover.Changed += QueueRedraw;
			}
			
			UpdateCurrentTexture();
		}
	}
	
	private Texture2D _disabled;
	[Export] public Texture2D Disabled {
		get => _disabled;
		set {
			if (_disabled != null) {
				_disabled.Changed -= QueueRedraw;
			}
			
			_disabled = value;
			
			if (_disabled != null) {
				_disabled.Changed += QueueRedraw;
			}
			
			UpdateCurrentTexture();
		}
	}

	private Texture2D _currentTexture;

	public override void _EnterTree() {
		base._EnterTree();

		OnTouchDisabledChanged += OnDisabledChanged;
	}

	public override void _ExitTree() {
		base._ExitTree();
		
		OnTouchDisabledChanged -= OnDisabledChanged;
	}

	public override void _Draw() {
		if (_currentTexture == null) {
			return;
		}
		
		var rect = new Rect2(0, 0, Size);
		DrawTextureRect(_currentTexture, rect, false);
	}
	
	private void OnDisabledChanged(bool disabled) {
		UpdateCurrentTexture();
	}

	private void UpdateCurrentTexture() {
		_currentTexture = _normal;
		if (_disabled != null && TouchDisabled) {
			_currentTexture = _disabled;
		}
		
		QueueRedraw();
	}
}