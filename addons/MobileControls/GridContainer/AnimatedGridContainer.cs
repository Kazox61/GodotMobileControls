using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using GodotMobileControls.Extensions;

namespace GodotMobileControls;

[Tool, GlobalClass, Icon("res://addons/MobileControls/Icons/AnimatedGridContainer.svg")]
public partial class AnimatedGridContainer : Container {
	private int _columns;
	[Export] public int Columns {
		get => _columns;
		set {
			_columns = value;
			PositionChildren();
		}
	}

	private int _hSeparation;
	[Export] public int HSeparation {
		get => _hSeparation;
		set {
			_hSeparation = value;
			PositionChildren();
		}
	}

	private int _vSeparation;
	[Export] public int VSeparation {
		get => _vSeparation;
		set {
			_vSeparation = value;
			PositionChildren();
		}
	}

	public enum OrderDirectionEnum {
		Begin,
		End
	}

	private OrderDirectionEnum _orderDirection = OrderDirectionEnum.Begin;
	[Export] public OrderDirectionEnum OrderDirection {
		get => _orderDirection;
		set {
			_orderDirection = value;
			PositionChildren();
		}
	}

	/*
	 * If true, the width of the first row element will be used as width of the missing row elements.
	 */
	private bool _useFirstRowElementWidth;
	[Export] public bool UseFirstRowElementWidth {
		get => _useFirstRowElementWidth;
		set {
			_useFirstRowElementWidth = value;
			PositionChildren();
		}
	}

	private bool _animateChildOrderDisabled = true;
	[Export] public bool AnimateChildOrderDisabled;

	public bool UpdateChildOrderDisabled;

	private CancellationTokenSource _animationCancellation = new();
	
	public event Action OnChildOrderChanged;

	public override void _Ready() {
		PositionChildren();
		_animateChildOrderDisabled = false;
	}

	public override void _Notification(int what) {
		if (UpdateChildOrderDisabled) {
			return;
		}
		
		if (what != NotificationSortChildren && what != NotificationChildOrderChanged) {
			return;
		}
		
		OnChildOrderChanged?.Invoke();

		_animationCancellation?.Cancel();
		_animationCancellation = new CancellationTokenSource();

		PositionChildren();
	}

	public void PositionChildren() {
		var rows = Mathf.CeilToInt(GetChildren().Count / (float)Columns);

		var rowStartY = 0f;
		var maxWidth = 0f;

		for (var i = 0; i < rows; i++) {
			if (i != 0) {
				rowStartY += VSeparation;
			}

			var rowStartIndex = i * Columns;

			var width = HandleRow(rowStartIndex, rowStartY);

			if (width > maxWidth) {
				maxWidth = width;
			}

			var maxHeight = GetMaxHeightRow(rowStartIndex);

			rowStartY += maxHeight;
		}

		CustomMinimumSize = new Vector2(maxWidth, rowStartY);
	}

	private float HandleRow(int rowStartIndex, float rowStartY) {
		var tasks = new List<Task>();

		var rowWidth = GetRowWidth(rowStartIndex);
		var startX = GetStartX(rowWidth);

		for (var i = rowStartIndex; i < rowStartIndex + Columns; i++) {
			if (i >= GetChildren().Count) {
				return startX;
			}

			if (i != rowStartIndex) {
				startX += HSeparation;
			}

			var child = GetChild(i);

			var position = new Vector2(startX, rowStartY);
			if (!_animateChildOrderDisabled && !AnimateChildOrderDisabled && !Engine.IsEditorHint()) {
				GD.Print("Animate");
				var task = AnimatePositionChange(child, position);
				tasks.Add(task);
			}
			else {
				child.Position = position;
			}

			startX += child.Size.X;
		}

		_ = Task.WhenAll(tasks);
		return startX;
	}

	private float GetMaxHeightRow(int rowStartIndex) {
		var maxHeight = 0f;

		for (var i = rowStartIndex; i < rowStartIndex + Columns; i++) {
			if (i >= GetChildren().Count) {
				return maxHeight;
			}

			var child = GetChild(i);

			if (child.Size.Y > maxHeight) {
				maxHeight = child.Size.Y;
			}
		}

		return maxHeight;
	}

	private float GetRowWidth(int rowStartIndex) {
		var rowWidth = 0f;
		var firstElementWidth = 0f;

		for (var i = rowStartIndex; i < rowStartIndex + Columns; i++) {
			if (i >= GetChildren().Count) {
				if (UseFirstRowElementWidth && firstElementWidth > 0) {
					rowWidth += firstElementWidth + HSeparation;
				}
				continue;
			}

			var child = GetChild(i);
			if (i == rowStartIndex) {
				firstElementWidth = child.Size.X;
			}

			rowWidth += child.Size.X;

			if (i != rowStartIndex) {
				rowWidth += HSeparation;
			}
		}

		return rowWidth;
	}

	private float GetStartX(float rowWidth) {
		var availableWidth = Size.X;
		return (availableWidth - rowWidth) / 2;
	}

	private List<Control> GetChildren() {
		var children = new List<Control>();
		for (var i = 0; i < GetChildCount(); i++) {
			var child = GetChild<Control>(i);

			if (child.IsVisibleInTree()) {
				children.Add(child);
			}
		}

		return children;
	}

	private Control GetChild(int index) {
		var children = GetChildren();

		return OrderDirection switch {
			OrderDirectionEnum.Begin => children[index],
			OrderDirectionEnum.End => children[children.Count - 1 - index],
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private Task AnimatePositionChange(Control child, Vector2 newPosition) {
		var tween = child
			.CreateTween()
			.SetEase(Tween.EaseType.InOut)
			.SetTrans(Tween.TransitionType.Cubic);

		tween.TweenProperty(child, "position", newPosition, 0.4f);

		return tween.PlayAsync(_animationCancellation.Token);
	}
}