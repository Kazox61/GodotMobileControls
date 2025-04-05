using Godot;
using GodotMobileControls.Extensions;

namespace GodotMobileControls;

[GlobalClass, Icon("res://addons/MobileControls/Icons/MobileScrollContainer.svg")]
public partial class MobileScrollContainer : Control {
	public enum DirectionEnum {
		Vertical,
		Horizontal
	}

	[Export] private DirectionEnum _direction = DirectionEnum.Vertical;
	
	[Signal]
	public delegate void ScrollStartEventHandler();
	
	private Control _scrollView;
	
	private bool _scrollStarted;
	private float _dragDistance;
	private Vector2 _dragVelocity;

	public override void _Ready() {
		_scrollView = GetChild<Control>(0);
	}
	
	public async void ScrollToTop(float duration = 1f) {
		var tween = CreateTween();

		tween.SetEase(Tween.EaseType.InOut);
		tween.SetTrans(Tween.TransitionType.Cubic);
		
		tween.TweenProperty(_scrollView, "position", new Vector2(_scrollView.Position.X, 0), duration);

		await tween.PlayAsync();
	}
	
	public async void ScrollToPosition(Vector2 position, float duration = 1f) {
		var tween = CreateTween();

		tween.SetEase(Tween.EaseType.InOut);
		tween.SetTrans(Tween.TransitionType.Cubic);
		
		tween.TweenProperty(_scrollView, "position", position, duration);

		await tween.PlayAsync();
	}
	
	public override void _GuiInput(InputEvent @event) {
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
	
	private void ClampScrollViewVertical() {
		var scrollViewRectSize = _scrollView.GetRect().Size;
		var containerRectSize = GetRect().Size;

		var maxScrollY = -scrollViewRectSize.Y + containerRectSize.Y;

		if (maxScrollY > 0) {
			maxScrollY = 0;
		}

		if (_scrollView.Position.Y > 0) {
			var tween = CreateTween();
			tween.SetEase(Tween.EaseType.Out);
			tween.SetTrans(Tween.TransitionType.Cubic);
			tween.TweenProperty(_scrollView, "position", new Vector2(_scrollView.Position.X, 0f), 0.25f);
			tween.Play();
			return;
		}

		if (_scrollView.Position.Y < maxScrollY) {
			var tween = CreateTween();
			tween.SetEase(Tween.EaseType.Out);
			tween.SetTrans(Tween.TransitionType.Cubic);
			tween.TweenProperty(_scrollView, "position", new Vector2(_scrollView.Position.X, maxScrollY), 0.25f);
			tween.Play();
			return;
		}
		
		var targetY = _scrollView.Position.Y + _dragVelocity.Y * 0.25f;
		targetY = Mathf.Clamp(targetY, maxScrollY, 0);
		
		var tween2 = CreateTween();
		tween2.SetEase(Tween.EaseType.Out);
		tween2.SetTrans(Tween.TransitionType.Cubic);
		tween2.TweenProperty(_scrollView, "position", new Vector2(_scrollView.Position.X, targetY), 0.25f);
	}
	
	private void ClampScrollViewHorizontal() {
		if (_scrollView.Position.X > 0) {
			var tween = CreateTween();
			tween.SetEase(Tween.EaseType.Out);
			tween.SetTrans(Tween.TransitionType.Cubic);
			tween.TweenProperty(_scrollView, "position", new Vector2(0f, _scrollView.Position.Y), 0.25f);
			tween.Play();
			return;
		}
		
		var scrollViewRectSize = _scrollView.GetRect().Size;
		var containerRectSize = GetRect().Size;
		if (_scrollView.Position.X < -scrollViewRectSize.X + containerRectSize.X) {
			var tween = CreateTween();
			tween.SetEase(Tween.EaseType.Out);
			tween.SetTrans(Tween.TransitionType.Cubic);
			tween.TweenProperty(_scrollView, "position", new Vector2(-scrollViewRectSize.X + containerRectSize.X, _scrollView.Position.Y), 0.25f);
			tween.Play();
			return;
		}
		
		var targetX = _scrollView.Position.X + _dragVelocity.X * 0.25f;
		if (targetX > 0) {
			targetX = 0;
		}
		else if (targetX < -scrollViewRectSize.X + containerRectSize.X) {
			targetX = -scrollViewRectSize.X + containerRectSize.X;
		}
		
		var tween2 = CreateTween();
		tween2.SetEase(Tween.EaseType.Out);
		tween2.SetTrans(Tween.TransitionType.Cubic);
		tween2.TweenProperty(_scrollView, "position", new Vector2(targetX, _scrollView.Position.Y), 0.25f);
		tween2.Play();
	}
	
	private void OnScreenTouchStart(InputEventScreenTouch touch) {
		_scrollStarted = false;
		_dragDistance = 0f;
	}
	
	private void OnScreenDrag(InputEventScreenDrag drag) {
		_dragDistance += drag.Relative.Length();

		if (_dragDistance < GlobalSettings.MinDragCancelDistance) {
			return;
		}
		
		if (!_scrollStarted) {
			_scrollStarted = true;
			EmitSignalScrollStart();
		}
		
		_dragVelocity = drag.Velocity;
		
		var position = _scrollView.Position;
		switch (_direction) {
			case DirectionEnum.Vertical:
				position.Y += drag.Relative.Y;
				break;
			case DirectionEnum.Horizontal:
				position.X += drag.Relative.X;
				break;
		}
		
		_scrollView.Position = position;
	}
	
	private void OnScreenTouchEnd(InputEventScreenTouch touch) {
		if (!_scrollStarted) {
			return;
		}

		switch (_direction) {
			case DirectionEnum.Vertical:
				ClampScrollViewVertical();
				break;
			case DirectionEnum.Horizontal:
				ClampScrollViewHorizontal();
				break;
		}
	}
}