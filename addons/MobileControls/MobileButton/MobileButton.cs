using System;
using System.Linq;
using System.Threading.Tasks;
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
				OnTouchDisabledChanged?.Invoke(value);
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

			if (value && IsInstanceValid(MobileButtonGroup)) {
				if (IsInstanceValid(MobileButtonGroup.PressedButton)) {
					MobileButtonGroup.PressedButton.ButtonPressed = false;
				}
				MobileButtonGroup.PressedButton = this;
			}
			
			_buttonPressed = value;
			
			QueueRedraw();
			EmitSignalToggled(_buttonPressed);
		}
	}

	[Export] public bool LongPressEnabled;
	[Export] public float LongPressActivationTime = 0.3f;

	private MobileButtonGroup _mobileButtonGroup;
	[Export] public MobileButtonGroup MobileButtonGroup {
		get => _mobileButtonGroup;
		set => SetMobileButtonGroup(value);
	}

	[ExportGroup("Animation")] 
	[Export] public bool Animated = true;

	[Export] public float Duration = 0.2f;
	[Export] public Vector2 ButtonDownScale = new(0.9f, 0.9f);
	[Export] public Vector2 ButtonUpScale = new(1.05f, 1.05f);

	public enum PivotPosition {
		Start,
		Center,
		End
	}

	[Export] public PivotPosition HPivotPosition = PivotPosition.Center;
	[Export] public PivotPosition VPivotPosition = PivotPosition.Center;

	public bool LongPressed;

	private bool _isPressing;
	private float _dragDistance;
	private bool _isCanceled;
	private float _touchDuration;

	private TaskCompletionSource<bool> _taskCompletionSource;
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
	
	public event Action<bool> OnTouchDisabledChanged;

	public override void _EnterTree() {
		FocusMode = FocusModeEnum.All;
		
		SetPivotDeferred();

		Resized += SetPivotDeferred;
	}

	public override void _ExitTree() {
		Scale = Vector2.One;

		Resized -= SetPivotDeferred;

		_currentTween?.Kill();
	}

	public override void _GuiInput(InputEvent @event) {
		if (TouchDisabled) {
			return;
		}
		
		switch (@event) {
			case InputEventScreenTouch touch: {
				if (touch.IsPressed()) {
					OnScreenTouchStart(touch);
				}

				if (touch.IsReleased()) {
					OnScreenTouchEnd(touch);
				}

				break;
			}
			case InputEventScreenDrag drag:
				OnScreenDrag(drag);
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

		if (_touchDuration > LongPressActivationTime && _dragDistance < GlobalSettings.MinDragCancelDistance) {
			LongPressed = true;
			EmitSignalTouchLongPressStart();
		}
	}

	private void OnScreenTouchStart(InputEventScreenTouch touch) {
		_isPressing = true;
		_touchDuration = 0f;
		_dragDistance = 0f;
		_isCanceled = false;
		
		EmitSignalTouchDown();

		if (Animated) {
			_ = PlayShrinkAnimation();
		}
	}

	private void OnScreenDrag(InputEventScreenDrag drag) {
		_dragDistance += drag.Relative.Length();

		if (LongPressed) {
			EmitSignalTouchLongPressDrag(drag);
		}

		if (_dragDistance < GlobalSettings.MinDragCancelDistance || _isCanceled) {
			return;
		}

		_isCanceled = true;
		_taskCompletionSource.TrySetResult(false);
		EmitSignalTouchCancel();
		if (LongPressed) {
			EmitSignalTouchLongPressCancel();
		}
	}

	private void OnScreenTouchEnd(InputEventScreenTouch touch) {
		_isPressing = false;

		_taskCompletionSource?.TrySetResult(!_isCanceled);

		if (!_isCanceled) {
			if (ToggleMode) {
				ButtonPressed = !ButtonPressed;
			}
			EmitSignalTouchPress();
			
			if (LongPressed) {
				EmitSignalTouchLongPress();
			}
		}

		if (LongPressed) {
			EmitSignalTouchLongPressEnd(touch);
		}
		
		EmitSignalTouchUp();

		_touchDuration = 0f;
		LongPressed = false;
	}

	private void SetPivotDeferred() {
		Callable.From(SetPivot).CallDeferred();
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

	private async Task PlayShrinkAnimation() {
		_currentTween?.Kill();

		_taskCompletionSource = new TaskCompletionSource<bool>();

		_currentTween = CreateTween()
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);

		_currentTween.TweenProperty(this, "scale", ButtonDownScale, Duration);
		_currentTween.Play();

		var pressed = await _taskCompletionSource.Task;
		_currentTween?.Kill();
		if (!pressed) {
			Scale = Vector2.One;
			return;
		}

		PlayGrowAnimation();
	}

	private void PlayGrowAnimation() {
		_currentTween?.Kill();

		_currentTween = CreateTween()
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);

		_currentTween.TweenProperty(this, "scale", ButtonUpScale, Duration * 0.5f);
		_currentTween.TweenProperty(this, "scale", Vector2.One, Duration * 0.5f);
		_currentTween.Play();
	}
	
	private void SetMobileButtonGroup(MobileButtonGroup mobileButtonGroup) {
		MobileButtonGroup?.Buttons.Remove(this);
		_mobileButtonGroup = mobileButtonGroup;
		MobileButtonGroup?.Buttons.Add(this);

		
		if (ButtonPressed && IsInstanceValid(MobileButtonGroup)) {
			foreach (var mobileButton in MobileButtonGroup.Buttons.Where(mobileButton => mobileButton != this)) {
				mobileButton.ButtonPressed = false;
				mobileButton.QueueRedraw();
			}

			MobileButtonGroup.PressedButton = this;
		}
		
		QueueRedraw();
	}
}