using Godot;

namespace GodotMobileControls.MobileJoystick;

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

	[Export(PropertyHint.Range, "0, 200, 1")]
	public float DeadZoneSize = 10f;

	[Export(PropertyHint.Range, "0, 500, 1")]
	public float ClampZoneSize = 90f;

	[Export] public EJoystickMode JoystickMode = EJoystickMode.Fixed;
	[Export] public EVisibilityMode VisibilityMode = EVisibilityMode.Always;

	[Export] public bool UseInputActions = true;
	[Export] public string ActionLeft = "ui_left";
	[Export] public string ActionRight = "ui_right";
	[Export] public string ActionUp = "ui_up";
	[Export] public string ActionDown = "ui_down";

	public bool IsPressed;
	public Vector2 InputDirection = Vector2.Zero;

	private int _touchIndex = -1;
	private TextureRect _base;
	private TextureRect _tip;
	private Vector2 _baseDefaultPosition;
	private Vector2 _tipDefaultPosition;
	private Color _defaultColor;

	public override void _Ready() {
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
		return _base.Size * _base.GetGlobalTransformWithCanvas().Scale / 2;
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

		if (vector.LengthSquared() > DeadZoneSize * DeadZoneSize) {
			IsPressed = true;
			InputDirection = (vector - (vector.Normalized() * DeadZoneSize)) / (ClampZoneSize - DeadZoneSize);
		}
		else {
			IsPressed = false;
			InputDirection = Vector2.Zero;
		}

		if (UseInputActions) {
			HandleInputActions();
		}
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
	}
}