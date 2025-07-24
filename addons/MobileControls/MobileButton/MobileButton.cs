using System;
using Godot;

namespace GodotMobileControls;

[Tool, GlobalClass, Icon("res://addons/MobileControls/Icons/MobileButton.svg")]
public partial class MobileButton : Control {
	private bool _touchDisabled;

	[Export]
	public bool TouchDisabled {
		get => _touchDisabled;
		set {
			var oldValue = _touchDisabled;
			_touchDisabled = value;

			if (oldValue != value) {
				QueueRedraw();
			}
		}
	}

	private bool _toggleMode;

	[Export]
	public bool ToggleMode {
		get => _toggleMode;
		set {
			var oldValue = _toggleMode;
			_toggleMode = value;
			if (!_toggleMode && oldValue != _toggleMode) {
				ButtonPressed = false;
			}
		}
	}

	private bool _buttonPressed;

	[Export]
	public bool ButtonPressed {
		get => _buttonPressed;
		set {
			if (!ToggleMode) {
				_buttonPressed = false;
				QueueRedraw();
				return;
			}

			if (value == _buttonPressed) {
				return;
			}

			_buttonPressed = value;

			if (_buttonPressed) {
				UnpressGroup();
				if (IsInstanceValid(MobileButtonGroup)) {
					MobileButtonGroup.EmitSignal(MobileButtonGroup.SignalName.Pressed, this);
				}
			}

			QueueRedraw();
			EmitSignal(SignalName.Toggled, _buttonPressed);
		}
	}

	[Export] public bool LongPressEnabled;
	[Export] public float LongPressActivationTime = 0.3f;

	private MobileButtonGroup _mobileButtonGroup;

	[Export]
	public MobileButtonGroup MobileButtonGroup {
		get => _mobileButtonGroup;
		set => SetMobileButtonGroup(value);
	}

	[ExportGroup("Animation")] 
	[Export] public bool Animated = true;

	[Export] public float AnimationDuration = 0.2f;
	[Export] public Vector2 ButtonDownScale = new(0.9f, 0.9f);
	[Export] public Vector2 ButtonUpScale = new(1.05f, 1.05f);

	public enum PivotPosition {
		Start = 0,
		Center = 1,
		End = 2
	}

	[Export] public PivotPosition HPivotPosition = PivotPosition.Center;
	[Export] public PivotPosition VPivotPosition = PivotPosition.Center;

	public bool LongPressed;

	private bool _isPressing;
	private float _dragDistance;
	private bool _isCanceled;
	private float _touchDuration;

	private Tween _currentTween;

	public bool IsPressing => _isPressing;

	[Signal]
	public delegate void TouchDownEventHandler();

	[Signal]
	public delegate void TouchUpEventHandler();

	[Signal]
	public delegate void TouchCancelEventHandler();

	[Signal]
	public delegate void TouchPressEventHandler();

	[Signal]
	public delegate void TouchLongPressStartEventHandler();

	[Signal]
	public delegate void TouchLongPressDragEventHandler(InputEventScreenDrag drag);

	[Signal]
	public delegate void TouchLongPressEndEventHandler(InputEventScreenTouch touch);

	[Signal]
	public delegate void TouchLongPressCancelEventHandler();

	[Signal]
	public delegate void TouchLongPressEventHandler();

	[Signal]
	public delegate void ToggledEventHandler(bool toggledOn);

	public override void _Notification(int what) {
		switch ((long)what) {
			case NotificationReady:
				SetPivot();
				break;
			case NotificationResized:
				SetPivot();
				break;
		}
	}

	public override void _ExitTree() {
		Scale = Vector2.One;
		_currentTween?.Kill();

		if (IsInstanceValid(MobileButtonGroup)) {
			MobileButtonGroup.RemoveButton(this);
		}
	}

	public override void _GuiInput(InputEvent @event) {
		if (TouchDisabled) {
			return;
		}

		switch (@event) {
			case InputEventScreenTouch touch: {
				if (touch.IsPressed()) {
					HandleScreenTouchStart(touch);
				}

				if (touch.IsReleased()) {
					HandleScreenTouchEnd(touch);
				}

				break;
			}
			case InputEventScreenDrag drag:
				HandleScreenDrag(drag);
				break;
		}
	}

	public override void _Process(double delta) {
		if (!_isPressing) {
			return;
		}

		_touchDuration += (float)delta;

		if (!LongPressEnabled || LongPressed) {
			return;
		}

		if (_touchDuration > LongPressActivationTime && _dragDistance < 25.0f) {
			LongPressed = true;
			EmitSignal(SignalName.TouchLongPressStart);
		}
	}

	private void HandleScreenTouchStart(InputEventScreenTouch touch) {
		_isPressing = true;
		_touchDuration = 0f;
		_dragDistance = 0f;
		_isCanceled = false;
		LongPressed = false;

		EmitSignal(SignalName.TouchDown);

		if (Animated) {
			PlayShrinkAnimation();
		}
	}

	private void HandleScreenDrag(InputEventScreenDrag drag) {
		_dragDistance += drag.Relative.Length();

		if (LongPressed) {
			EmitSignal(SignalName.TouchLongPressDrag, drag);
		}

		if (_dragDistance < 25.0f || _isCanceled) {
			return;
		}

		_isCanceled = true;

		if (_currentTween != null) {
			_currentTween.Kill();
			Scale = Vector2.One;
		}

		EmitSignal(SignalName.TouchCancel);
		if (LongPressed) {
			EmitSignal(SignalName.TouchLongPressCancel);
		}
	}

	private void HandleScreenTouchEnd(InputEventScreenTouch touch) {
		_isPressing = false;

		if (_isCanceled) {
			if (_currentTween != null) {
				_currentTween.Kill();
				Scale = Vector2.One;
			}
		}
		else {
			if (Animated) {
				PlayGrowAnimation();
			}
			else {
				Scale = Vector2.One;
			}
		}

		if (!_isCanceled) {
			if (ToggleMode) {
				ButtonPressed = !ButtonPressed;
			}

			EmitSignal(SignalName.TouchPress);

			if (LongPressed) {
				EmitSignal(SignalName.TouchLongPress);
			}
		}

		if (LongPressed) {
			EmitSignal(SignalName.TouchLongPressEnd, touch);
		}

		EmitSignal(SignalName.TouchUp);

		_touchDuration = 0f;
		LongPressed = false;
	}

	private void SetPivot() {
		try {
			var pivot = Vector2.Zero;
			switch (HPivotPosition) {
				case PivotPosition.Start:
					pivot.X = 0;
					break;
				case PivotPosition.Center:
					pivot.X = Size.X * 0.5f;
					break;
				case PivotPosition.End:
					pivot.X = Size.X;
					break;
			}

			switch (VPivotPosition) {
				case PivotPosition.Start:
					pivot.Y = 0;
					break;
				case PivotPosition.Center:
					pivot.Y = Size.Y * 0.5f;
					break;
				case PivotPosition.End:
					pivot.Y = Size.Y;
					break;
			}

			PivotOffset = pivot;
		}
		catch (ObjectDisposedException) { }
	}

	private void UnpressGroup() {
		if (!IsInstanceValid(MobileButtonGroup)) {
			return;
		}

		if (ToggleMode && !MobileButtonGroup.IsAllowUnpress()) {
			_buttonPressed = true;
		}

		foreach (var button in MobileButtonGroup.GetButtonsInternal()) {
			if (button == this) {
				continue;
			}

			button.ButtonPressed = false;
		}
	}

	private void PlayShrinkAnimation() {
		_currentTween?.Kill();

		_currentTween = CreateTween()
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);

		_currentTween.TweenProperty(this, "scale", ButtonDownScale, AnimationDuration);
		_currentTween.Play();
	}

	private void PlayGrowAnimation() {
		_currentTween?.Kill();

		_currentTween = CreateTween()
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);

		_currentTween.TweenProperty(this, "scale", ButtonUpScale, AnimationDuration * 0.5f);
		_currentTween.TweenProperty(this, "scale", Vector2.One, AnimationDuration * 0.5f);
		_currentTween.Play();
	}

	private void SetMobileButtonGroup(MobileButtonGroup mobileButtonGroup) {
		MobileButtonGroup?.RemoveButton(this);

		_mobileButtonGroup = mobileButtonGroup;

		MobileButtonGroup?.AddButton(this);

		QueueRedraw();
	}
}