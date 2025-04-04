using System;
using System.Threading.Tasks;
using Godot;

namespace GodotMobileControls;

[Tool, GlobalClass, Icon("res://addons/MobileControls/Icons/MobileButton.svg")]
public partial class MobileButton : Control {
	private const float MinDragCancelDistance = 25f;

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

	[Export] public bool ToggleMode;
	
	private bool _buttonPressed;
	[Export]
	public bool ButtonPressed {
		get => _buttonPressed;
		set {
			if (!ToggleMode) {
				_buttonPressed = false;
				return;
			}
			
			if (value == _buttonPressed) {
				return;
			}
			
			_buttonPressed = value;
			UpdateVisual();
			EmitSignalToggled(_buttonPressed);
		}
	}

	[Export] public bool LongPressEnabled;
	[Export] public float LongPressActivationTime = 0.3f;

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
		
		if (@event is InputEventScreenTouch touch) {
			if (touch.IsPressed()) {
				OnScreenTouchStart(touch);
			}

			if (touch.IsReleased()) {
				OnScreenTouchEnd(touch);
			}
		}

		if (@event is InputEventScreenDrag drag) {
			OnScreenDrag(drag);
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

		if (_touchDuration > LongPressActivationTime && _dragDistance < MinDragCancelDistance) {
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

		if (_dragDistance < MinDragCancelDistance || _isCanceled) {
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
			EmitSignalTouchPress();
			if (ToggleMode) {
				ButtonPressed = !ButtonPressed;
			}
			
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

		_currentTween = CreateTween();
		_currentTween.SetTrans(Tween.TransitionType.Sine);
		_currentTween.SetEase(Tween.EaseType.InOut);

		_currentTween.TweenProperty(this, "scale", ButtonDownScale, Duration);

		var pressed = await _taskCompletionSource.Task;
		if (!pressed) {
			Scale = Vector2.One;
			return;
		}

		PlayGrowAnimation();
	}

	private void PlayGrowAnimation() {
		_currentTween?.Kill();

		_currentTween = CreateTween();
		_currentTween.SetTrans(Tween.TransitionType.Sine);
		_currentTween.SetEase(Tween.EaseType.InOut);

		_currentTween.TweenProperty(this, "scale", ButtonUpScale, Duration * 0.5f);
		_currentTween.TweenProperty(this, "scale", Vector2.One, Duration * 0.5f);
	}
	
	protected virtual void UpdateVisual() { }
}