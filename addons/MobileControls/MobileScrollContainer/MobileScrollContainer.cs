using System.Threading.Tasks;
using Godot;
using GodotMobileControls.Extensions;

namespace GodotMobileControls;

[GlobalClass, Icon("res://addons/MobileControls/Icons/MobileScrollContainer.svg")]
public partial class MobileScrollContainer : Control {
	public enum DirectionEnum {
		Vertical,
		Horizontal
	}

	[Export] public DirectionEnum Direction = DirectionEnum.Vertical;
	
	[Signal]
	public delegate void ScrollStartEventHandler();
	
	protected Control ScrollView;
	
	private bool _scrollStarted;
	private bool _minDragDistanceReached;
	private Vector2 _dragDistance;
	private Vector2 _dragVelocity;

	public override void _Ready() {
		ScrollView = GetChild<Control>(0);
	}
	
	public async Task ScrollToTop(float duration = 1f) {
		var tween = CreateTween();

		tween.SetEase(Tween.EaseType.InOut);
		tween.SetTrans(Tween.TransitionType.Cubic);
		
		tween.TweenProperty(ScrollView, "position", new Vector2(ScrollView.Position.X, 0), duration);

		await tween.PlayAsync();
	}
	
	public async Task ScrollToPosition(Vector2 position, float duration = 1f) {
		var tween = CreateTween();

		tween.SetEase(Tween.EaseType.InOut);
		tween.SetTrans(Tween.TransitionType.Cubic);
		
		tween.TweenProperty(ScrollView, "position", position, duration);

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
		var scrollViewRectSize = ScrollView.GetRect().Size;
		var containerRectSize = GetRect().Size;

		var maxScrollY = -scrollViewRectSize.Y + containerRectSize.Y;

		if (maxScrollY > 0) {
			maxScrollY = 0;
		}

		if (ScrollView.Position.Y > 0) {
			var tween = CreateTween();
			tween.SetEase(Tween.EaseType.Out);
			tween.SetTrans(Tween.TransitionType.Cubic);
			tween.TweenProperty(ScrollView, "position", new Vector2(ScrollView.Position.X, 0f), 0.25f);
			tween.Play();
			return;
		}

		if (ScrollView.Position.Y < maxScrollY) {
			var tween = CreateTween();
			tween.SetEase(Tween.EaseType.Out);
			tween.SetTrans(Tween.TransitionType.Cubic);
			tween.TweenProperty(ScrollView, "position", new Vector2(ScrollView.Position.X, maxScrollY), 0.25f);
			tween.Play();
			return;
		}
		
		var targetY = ScrollView.Position.Y + _dragVelocity.Y * 0.25f;
		targetY = Mathf.Clamp(targetY, maxScrollY, 0);
		
		var tween2 = CreateTween();
		tween2.SetEase(Tween.EaseType.Out);
		tween2.SetTrans(Tween.TransitionType.Cubic);
		tween2.TweenProperty(ScrollView, "position", new Vector2(ScrollView.Position.X, targetY), 0.25f);
	}
	
	private void ClampScrollViewHorizontal() {
		if (ScrollView.Position.X > 0) {
			var tween = CreateTween();
			tween.SetEase(Tween.EaseType.Out);
			tween.SetTrans(Tween.TransitionType.Cubic);
			tween.TweenProperty(ScrollView, "position", new Vector2(0f, ScrollView.Position.Y), 0.25f);
			tween.Play();
			return;
		}
		
		var scrollViewRectSize = ScrollView.GetRect().Size;
		var containerRectSize = GetRect().Size;
		if (ScrollView.Position.X < -scrollViewRectSize.X + containerRectSize.X) {
			var tween = CreateTween();
			tween.SetEase(Tween.EaseType.Out);
			tween.SetTrans(Tween.TransitionType.Cubic);
			tween.TweenProperty(ScrollView, "position", new Vector2(-scrollViewRectSize.X + containerRectSize.X, ScrollView.Position.Y), 0.25f);
			tween.Play();
			return;
		}
		
		var targetX = ScrollView.Position.X + _dragVelocity.X * 0.25f;
		if (targetX > 0) {
			targetX = 0;
		}
		else if (targetX < -scrollViewRectSize.X + containerRectSize.X) {
			targetX = -scrollViewRectSize.X + containerRectSize.X;
		}
		
		var tween2 = CreateTween();
		tween2.SetEase(Tween.EaseType.Out);
		tween2.SetTrans(Tween.TransitionType.Cubic);
		tween2.TweenProperty(ScrollView, "position", new Vector2(targetX, ScrollView.Position.Y), 0.25f);
		tween2.Play();
	}
	
	private void OnScreenTouchStart(InputEventScreenTouch touch) {
		_scrollStarted = false;
		_minDragDistanceReached = false;
		_dragDistance = Vector2.Zero;
	}
	
	private void OnScreenDrag(InputEventScreenDrag drag) {
		_dragDistance += drag.Relative;

		if (_dragDistance.Length() >= GlobalSettings.MinDragCancelDistance && !_minDragDistanceReached) {
			_minDragDistanceReached = true;
			switch (Direction) {
				case DirectionEnum.Horizontal:
					if (Mathf.Abs(_dragDistance.Y) > Mathf.Abs(_dragDistance.X)) {
						return;
					}
					break;
				case DirectionEnum.Vertical:
					if (Mathf.Abs(_dragDistance.X) > Mathf.Abs(_dragDistance.Y)) {
						return;
					}
					break;
			}
			
			_scrollStarted = true;
			EmitSignalScrollStart();
		}
		
		if (!_scrollStarted) {
			return;
		}
		
		_dragVelocity = drag.Velocity;
		
		var position = ScrollView.Position;
		switch (Direction) {
			case DirectionEnum.Vertical:
				position.Y += drag.Relative.Y;
				break;
			case DirectionEnum.Horizontal:
				position.X += drag.Relative.X;
				break;
		}
		
		ScrollView.Position = position;
	}
	
	private void OnScreenTouchEnd(InputEventScreenTouch touch) {
		if (!_scrollStarted) {
			return;
		}

		switch (Direction) {
			case DirectionEnum.Vertical:
				ClampScrollViewVertical();
				break;
			case DirectionEnum.Horizontal:
				ClampScrollViewHorizontal();
				break;
		}
	}
}