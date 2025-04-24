using Godot;

namespace GodotMobileControls.MobileJoystick;

[Tool, GlobalClass, Icon("res://addons/MobileControls/Icons/MobileJoystick.svg")]
public partial class MobileJoystick : Control {
	public enum EJoystickMode {
		Fixed,
		Dynamic,
		Following
	}

	public enum EVisibilityMode {
		Always,
		TouchscreenOnly,
		WhenTouched
	}

	[Export] public Color PressedColor = Colors.White;

	[Export(PropertyHint.Range, "0, 1, 0.01")]
	public float DeadZone = 0.2f;

	public float DeadZoneSize => DeadZone * ClampZoneSize;

	[Export(PropertyHint.Range, "0, 1, 0.01")]
	public float ClampZone = 0.9f;
	
	public float ClampZoneSize => ClampZone * GetBaseRadius().X;

	[Export] public EJoystickMode JoystickMode = EJoystickMode.Fixed;
	[Export] public EVisibilityMode VisibilityMode = EVisibilityMode.Always;

	[Export] public bool UseInputActions = true;
	[Export] public string ActionLeft = "ui_left";
	[Export] public string ActionRight = "ui_right";
	[Export] public string ActionUp = "ui_up";
	[Export] public string ActionDown = "ui_down";

	[Export] public bool Debug = true;

	[ExportToolButton("Refresh Debug")]
	private Callable RefreshDebug => Callable.From(QueueRedraw);

	public bool IsPressed;
	public Vector2 InputDirection = Vector2.Zero;
	public bool IsFlickCanceled;

	private int _touchIndex = -1;
	private TextureRect _base;
	private TextureRect _tip;
	private Vector2 _baseDefaultPosition;
	private Vector2 _tipDefaultPosition;
	private Color _defaultColor;
	
	[Signal]
	public delegate void ReleasedEventHandler(Vector2 inputDirection);

	[Signal]
	public delegate void TappedEventHandler();
	
	[Signal]
	public delegate void FlickedEventHandler(Vector2 inputDirection);
	
	[Signal]
	public delegate void FlickCanceledEventHandler();

	public override void _Ready() {
#if TOOLS
		if (Engine.IsEditorHint()) {
			return;
		}
#endif
		
		_base = GetNode<TextureRect>("Base");
		_tip = GetNode<TextureRect>("Base/Tip");

		if (!DisplayServer.IsTouchscreenAvailable() && VisibilityMode == EVisibilityMode.TouchscreenOnly) {
			Hide();
		}

		if (VisibilityMode == EVisibilityMode.WhenTouched) {
			Hide();
		}
		
		Callable.From(() => {
			_baseDefaultPosition = _base.Position;
			_tipDefaultPosition = _tip.Position;
			_defaultColor = _tip.Modulate;
			
			ResetJoystick();
		}).CallDeferred();
	}

	public override void _Input(InputEvent @event) {
#if TOOLS
		if (Engine.IsEditorHint()) {
			return;
		}
#endif
		
		switch (@event) {
			case InputEventScreenTouch eventTouch when eventTouch.Pressed: {
				if (IsPointInsideJoystickArea(eventTouch.Position) && _touchIndex == -1) {
					if (
						JoystickMode == EJoystickMode.Dynamic ||
						JoystickMode == EJoystickMode.Following ||
						(JoystickMode == EJoystickMode.Fixed && IsPointInsideBase(eventTouch.Position))
					) {
						if (JoystickMode == EJoystickMode.Dynamic || JoystickMode == EJoystickMode.Following) {
							MoveBase(eventTouch.Position);
						}

						if (VisibilityMode == EVisibilityMode.WhenTouched) {
							Show();
						}

						_touchIndex = eventTouch.Index;
						_tip.Modulate = PressedColor;

						UpdateJoystick(eventTouch.Position);
					}
				}

				break;
			}
			case InputEventScreenTouch eventTouch: {
				if (eventTouch.Index == _touchIndex) {
					EmitSignalReleased(InputDirection);
					
					if (!IsPressed && !IsFlickCanceled) {
						EmitSignalTapped();
					}
					else if (IsPressed) {
						EmitSignalFlicked(InputDirection);
					}
					
					ResetJoystick();
					
					if (VisibilityMode == EVisibilityMode.WhenTouched) {
						Hide();
					}
				}

				break;
			}
			case InputEventScreenDrag eventDrag: {
				if (eventDrag.Index == _touchIndex) {
					UpdateJoystick(eventDrag.Position);
				}

				break;
			}
		}
	}

	public override void _Draw() {
		if (!Debug) {
			return;
		}
		
		var rectBase = _base?.GetRect() ?? GetNode<TextureRect>("Base").GetRect();
		var centerBase = rectBase.GetCenter();
		var centerTip = rectBase.Position 
		                + (_tip?.GetRect().GetCenter() ?? GetNode<TextureRect>("Base/Tip").GetRect().GetCenter());
		
		DrawCircle(centerBase, DeadZoneSize, new Color("19FF19"), false, 1f, true);
		DrawCircle(centerBase, ClampZoneSize, new Color("1919FF"), false, 2f, true);
		
		DrawCircle(centerTip, 4f, new Color("#FF1919"));
	}
	
	private void MoveBase(Vector2 newPosition) {
		_base.GlobalPosition = newPosition - (_base.PivotOffset * GetGlobalTransformWithCanvas().Scale);
	}

	private void MoveTip(Vector2 newPosition) {
		_tip.GlobalPosition = newPosition - (_tip.PivotOffset * _base.GetGlobalTransformWithCanvas().Scale);
	}

	private bool IsPointInsideJoystickArea(Vector2 point) {
		var x = point.X >= GlobalPosition.X &&
		        point.X <= GlobalPosition.X + (Size.X * GetGlobalTransformWithCanvas().Scale.X);
		var y = point.Y >= GlobalPosition.Y &&
		        point.Y <= GlobalPosition.Y + (Size.Y * GetGlobalTransformWithCanvas().Scale.Y);

		return x && y;
	}

	private Vector2 GetBaseRadius() {
		var b = _base ?? GetNode<TextureRect>("Base");
		
		return b.Size * b.GetGlobalTransformWithCanvas().Scale / 2;
	}

	private bool IsPointInsideBase(Vector2 point) {
		var baseRadius = GetBaseRadius();
		var center = _base.GlobalPosition + baseRadius;

		return (point - center).LengthSquared() <= baseRadius.X * baseRadius.X;
	}

	private void UpdateJoystick(Vector2 touchPosition) {
		var baseRadius = GetBaseRadius();
		var center = _base.GlobalPosition + baseRadius;
		var vector = (touchPosition - center).LimitLength(ClampZoneSize);

		if (JoystickMode == EJoystickMode.Following && touchPosition.DistanceTo(center) > ClampZoneSize) {
			MoveBase(touchPosition - vector);
		}

		MoveTip(center + vector);

		var wasPressed = IsPressed;
		
		if (vector.LengthSquared() > DeadZoneSize * DeadZoneSize) {
			IsPressed = true;
			InputDirection = vector / ClampZoneSize;
		}
		else {
			IsPressed = false;
			InputDirection = Vector2.Zero;
		}
		
		if (!IsFlickCanceled && wasPressed && !IsPressed) {
			IsFlickCanceled = true;
			EmitSignalFlickCanceled();
		}
		
		if (IsFlickCanceled && !wasPressed && IsPressed) {
			IsFlickCanceled = false;
		}

		if (UseInputActions) {
			HandleInputActions();
		}
		
		QueueRedraw();
	}

	private void HandleInputActions() {
		if (InputDirection.X >= 0 && Input.IsActionPressed(ActionLeft)) {
			Input.ActionRelease(ActionLeft);
		}
		
		if (InputDirection.X <= 0 && Input.IsActionPressed(ActionRight)) {
			Input.ActionRelease(ActionRight);
		}
		
		if (InputDirection.Y >= 0 && Input.IsActionPressed(ActionUp)) {
			Input.ActionRelease(ActionUp);
		}
		
		if (InputDirection.Y <= 0 && Input.IsActionPressed(ActionDown)) {
			Input.ActionRelease(ActionDown);
		}

		if (InputDirection.X < 0) {
			Input.ActionPress(ActionLeft, -InputDirection.X);
		}
		
		if (InputDirection.X > 0) {
			Input.ActionPress(ActionRight, InputDirection.X);
		}
		
		if (InputDirection.Y < 0) {
			Input.ActionPress(ActionUp, -InputDirection.Y);
		}
		
		if (InputDirection.Y > 0) {
			Input.ActionPress(ActionDown, InputDirection.Y);
		}
	}

	private void ResetJoystick() {
		IsPressed = false;
		InputDirection = Vector2.Zero;
		IsFlickCanceled = false;

		_touchIndex = -1;
		_tip.Modulate = _defaultColor;
		_base.Position = _baseDefaultPosition;
		_tip.Position = _tipDefaultPosition;

		if (!UseInputActions) {
			return;
		}
		
		foreach (var action in new[] { ActionLeft, ActionRight, ActionUp, ActionDown }) {
			Input.ActionRelease(action);
		}
		
		QueueRedraw();
	}
}