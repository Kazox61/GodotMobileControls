using System.Threading.Tasks;
using Godot;

namespace GodotMobileControls;

[GlobalClass, Icon("res://addons/MobileControls/Icons/MobileScrollContainer.svg")]
public partial class MobileScrollContainer : Container {
	public enum Direction {
		Vertical = 0,
		Horizontal = 1
	}

	private Direction _direction = Direction.Vertical;

	[Export]
	public Direction DirectionProperty {
		get => _direction;
		set => _direction = value;
	}
	
	[Signal]
	public delegate void ScrollStartEventHandler();
	
	protected Control _scrollView;
	
	private bool _scrollStarted;
	private bool _minDragDistanceReached;
	private Vector2 _dragDistance;
	private Vector2 _dragVelocity;
	private Vector2 _scrollOffset;

	public override void _Notification(int what) {
		switch ((long)what) {
			case NotificationReady:
				_scrollView = GetChild<Control>(0);
				RepositionChildren();
				break;
			case NotificationSortChildren:
				RepositionChildren();
				break;
		}
	}

	public override string[] _GetConfigurationWarnings() {
		var warnings = new System.Collections.Generic.List<string>(base._GetConfigurationWarnings());

		var found = 0;
		for (var i = 0; i < GetChildCount(); i++) {
			var child = GetChild(i);
			if (child is Control c && !c.IsSetAsTopLevel() && c.IsVisible()) {
				found++;
			}
		}

		if (found != 1) {
			warnings.Add("MobileScrollContainer is intended to work with a single child control.\nUse a container as child (VBox, HBox, etc.), or a Control and set the custom minimum size manually.");
		}

		return warnings.ToArray();
	}
	
	public async Task ScrollToTop(float duration = 1f) {
		var tween = CreateTween();

		tween.SetEase(Tween.EaseType.InOut);
		tween.SetTrans(Tween.TransitionType.Cubic);
		
		var targetOffset = new Vector2(_scrollOffset.X, 0);
		tween.TweenMethod(Callable.From<Vector2>(SetScrollOffset), _scrollOffset, targetOffset, duration);

		await tween.ToSignal(tween, Tween.SignalName.Finished);
	}
	
	public async Task ScrollToPosition(Vector2 position, float duration = 1f) {
		var tween = CreateTween();

		tween.SetEase(Tween.EaseType.InOut);
		tween.SetTrans(Tween.TransitionType.Cubic);
		
		tween.TweenMethod(Callable.From<Vector2>(SetScrollOffset), _scrollOffset, position, duration);

		await tween.ToSignal(tween, Tween.SignalName.Finished);
	}

	private void SetScrollOffset(Vector2 offset) {
		_scrollOffset = offset;
		QueueSort();
	}
	
	public override void _GuiInput(InputEvent @event) {
		if (@event == null) {
			return;
		}

		switch (@event) {
			case InputEventScreenTouch touch: {
				if (touch.IsPressed()) {
					HandleScreenTouchStart(touch);
				}
				else {
					HandleScreenTouchEnd(touch);
				}
				break;
			}
			case InputEventScreenDrag drag:
				HandleScreenDrag(drag);
				break;
		}
	}

	private void HandleScreenTouchStart(InputEventScreenTouch touch) {
		_scrollStarted = false;
		_minDragDistanceReached = false;
		_dragDistance = Vector2.Zero;
	}

	private void HandleScreenTouchEnd(InputEventScreenTouch touch) {
		if (!_scrollStarted) {
			return;
		}

		switch (_direction) {
			case Direction.Vertical:
				ClampScrollViewVertical();
				break;
			case Direction.Horizontal:
				ClampScrollViewHorizontal();
				break;
		}
	}

	private void HandleScreenDrag(InputEventScreenDrag drag) {
		_dragDistance += drag.Relative;

		if (_dragDistance.Length() >= 25.0f && !_minDragDistanceReached) {
			_minDragDistanceReached = true;
			
			switch (_direction) {
				case Direction.Horizontal:
					if (Mathf.Abs(_dragDistance.Y) > Mathf.Abs(_dragDistance.X)) {
						return;
					}
					break;
				case Direction.Vertical:
					if (Mathf.Abs(_dragDistance.X) > Mathf.Abs(_dragDistance.Y)) {
						return;
					}
					break;
			}
			
			_scrollStarted = true;
			EmitSignal(SignalName.ScrollStart);
		}
		
		if (!_scrollStarted) {
			return;
		}
		
		_dragVelocity = drag.Velocity;
		
		switch (_direction) {
			case Direction.Vertical:
				_scrollOffset.Y += drag.Relative.Y;
				break;
			case Direction.Horizontal:
				_scrollOffset.X += drag.Relative.X;
				break;
		}
		
		QueueSort();
	}
	
	private void ClampScrollViewVertical() {
		var scrollViewRectSize = _scrollView.GetRect().Size;
		var containerRectSize = GetRect().Size;

		var maxScrollY = -scrollViewRectSize.Y + containerRectSize.Y;
		maxScrollY = Mathf.Min(0.0f, maxScrollY);

		float tweenPositionY;

		if (_scrollView.Position.Y > 0) {
			tweenPositionY = 0.0f;
		}
		else if (_scrollView.Position.Y < maxScrollY) {
			tweenPositionY = maxScrollY;
		}
		else {
			tweenPositionY = _scrollView.Position.Y + _dragVelocity.Y * 0.25f;
			tweenPositionY = Mathf.Clamp(tweenPositionY, maxScrollY, 0.0f);
		}

		_scrollOffset.Y = tweenPositionY;

		var tween = CreateTween()
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Cubic);

		tween.TweenProperty(_scrollView, "position:y", tweenPositionY, 0.25f);
		tween.Play();
	}
	
	private void ClampScrollViewHorizontal() {
		var scrollViewRectSize = _scrollView.GetRect().Size;
		var containerRectSize = GetRect().Size;

		var maxScrollX = -scrollViewRectSize.X + containerRectSize.X;
		maxScrollX = Mathf.Min(0.0f, maxScrollX);

		float tweenPositionX;

		if (_scrollView.Position.X > 0) {
			tweenPositionX = 0.0f;
		}
		else if (_scrollView.Position.X < maxScrollX) {
			tweenPositionX = maxScrollX;
		}
		else {
			tweenPositionX = _scrollView.Position.X + _dragVelocity.X * 0.25f;
			tweenPositionX = Mathf.Clamp(tweenPositionX, maxScrollX, 0.0f);
		}

		_scrollOffset.X = tweenPositionX;

		var tween = CreateTween()
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Cubic);

		tween.TweenProperty(_scrollView, "position:x", tweenPositionX, 0.25f);
		tween.Play();
	}

	private void RepositionChildren() {
		if (_scrollView == null || !_scrollView.IsVisibleInTree()) {
			return;
		}

		var containerSize = GetSize();
		var contentSize = _scrollView.GetCombinedMinimumSize();

		var rect = new Rect2 {
			Position = _scrollOffset,
			Size = contentSize
		};

		if (_scrollView.GetHSizeFlags().HasFlag(SizeFlags.Expand)) {
			rect.Size = rect.Size with { X = Mathf.Max(rect.Size.X, containerSize.X) };
		}
		if (_scrollView.GetVSizeFlags().HasFlag(SizeFlags.Expand)) {
			rect.Size = rect.Size with { Y = Mathf.Max(rect.Size.Y, containerSize.Y) };
		}

		FitChildInRect(_scrollView, rect);
	}
}