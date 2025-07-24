using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GodotMobileControls;

[Tool, GlobalClass, Icon("res://addons/MobileControls/Icons/AnimatedGridContainer.svg")]
public partial class AnimatedGridContainer : Container {
	public enum OrderDirectionEnum {
		Begin,
		End
	}

	public enum Alignment {
		Start,
		Center,
		End
	}

	private int _columns = 1;
	[Export] public int Columns {
		get => _columns;
		set {
			_columns = value;
			QueueSort();
		}
	}

	private int _hSeparation;
	[Export] public int HSeparation {
		get => _hSeparation;
		set {
			_hSeparation = value;
			QueueSort();
		}
	}

	private int _vSeparation;
	[Export] public int VSeparation {
		get => _vSeparation;
		set {
			_vSeparation = value;
			QueueSort();
		}
	}

	private OrderDirectionEnum _orderDirection = OrderDirectionEnum.Begin;
	[Export] public OrderDirectionEnum OrderDirection {
		get => _orderDirection;
		set {
			_orderDirection = value;
			QueueSort();
		}
	}

	private Alignment _horizontalAlignment = Alignment.Center;
	[Export] public Alignment HorizontalAlignment {
		get => _horizontalAlignment;
		set {
			_horizontalAlignment = value;
			QueueSort();
		}
	}

	private Alignment _verticalAlignment = Alignment.Center;
	[Export] public Alignment VerticalAlignment {
		get => _verticalAlignment;
		set {
			_verticalAlignment = value;
			QueueSort();
		}
	}

	[Export] public bool AnimateChildOrderDisabled;

	private bool _useFirstRowElementWidth;
	[Export] public bool UseFirstRowElementWidth {
		get => _useFirstRowElementWidth;
		set {
			_useFirstRowElementWidth = value;
			QueueSort();
		}
	}
	
	public bool UpdateChildOrderDisabled;

	private bool _firstSortDone;
	private Vector2 _minimumSize;

	private readonly List<Tween> _activeTweens = [];

	[Signal]
	public delegate void OnChildOrderChangedEventHandler();

	public void SwapChildren(Control child1, Control child2) {
		if (child1 == null || child2 == null) {
			return;
		}

		var index1 = child1.GetIndex();
		var index2 = child2.GetIndex();

		UpdateChildOrderDisabled = true;

		MoveChild(child1, index2);
		MoveChild(child2, index1);

		UpdateChildOrderDisabled = false;
		QueueSort();
	}

	public List<Control> GetVisibleChildren() {
		var visibleChildren = new List<Control>();
		var childCount = GetChildCount();

		for (var i = 0; i < childCount; i++) {
			var child = GetChild<Control>(i);
			if (child != null && child.IsVisibleInTree()) {
				visibleChildren.Add(child);
			}
		}

		return visibleChildren;
	}

	private Vector2 GetContainerSize() {
		var visibleCount = GetVisibleChildrenCount();
		var rows = Mathf.CeilToInt(visibleCount / (float)_columns);

		var height = 0.0f;
		var largestWidth = 0.0f;

		for (var rowIndex = 0; rowIndex < rows; rowIndex++) {
			if (rowIndex != 0) {
				height += _vSeparation;
			}

			var rowStartIndex = rowIndex * _columns;
			var width = GetRowWidth(rowStartIndex);

			if (width > largestWidth) {
				largestWidth = width;
			}

			height += GetMaxHeightRow(rowStartIndex);
		}

		return new Vector2(largestWidth, height);
	}

	public int GetVisibleChildrenCount() {
		var count = 0;
		var childCount = GetChildCount();

		for (var i = 0; i < childCount; i++) {
			var child = GetChild<Control>(i);
			if (child != null && child.IsVisibleInTree()) {
				count++;
			}
		}

		return count;
	}

	public Control GetVisibleChild(int index) {
		var children = GetVisibleChildren();
		var count = children.Count;

		if (index < 0 || index >= count) {
			return null;
		}

		return _orderDirection switch {
			OrderDirectionEnum.Begin => children[index],
			OrderDirectionEnum.End => children[count - 1 - index],
			_ => children[index]
		};
	}

	public int ConvertIndex(int index) {
		return _orderDirection switch {
			OrderDirectionEnum.Begin => index,
			OrderDirectionEnum.End => GetVisibleChildrenCount() - 1 - index,
			_ => index
		};
	}

	private void UpdateRow(int rowStartIndex, float rowStartY, float sizeX) {
		var rowWidth = GetRowWidth(rowStartIndex);
		var startX = GetRowStartX(rowWidth, sizeX);
		var visibleChildrenCount = GetVisibleChildrenCount();

		for (var childIndex = rowStartIndex; childIndex < rowStartIndex + _columns; childIndex++) {
			if (childIndex >= visibleChildrenCount) {
				return;
			}

			if (childIndex != rowStartIndex) {
				startX += _hSeparation;
			}

			var child = GetVisibleChild(childIndex);
			if (child == null) {
				continue;
			}

			var position = new Vector2(startX, rowStartY);

			if (_firstSortDone && !AnimateChildOrderDisabled) {
				AnimatePositionChange(child, position);
			}
			else {
				child.Position = position;
			}

			startX += child.Size.X;
		}
	}

	private float GetMaxHeightRow(int rowStartIndex) {
		var maxHeight = 0.0f;
		var visibleChildrenCount = GetVisibleChildrenCount();

		for (var i = rowStartIndex; i < rowStartIndex + _columns; i++) {
			if (i >= visibleChildrenCount) {
				return maxHeight;
			}

			var child = GetVisibleChild(i);
			if (child == null)
				continue;

			if (child.Size.Y > maxHeight) {
				maxHeight = child.Size.Y;
			}
		}

		return maxHeight;
	}

	private float GetRowWidth(int rowStartIndex) {
		var rowWidth = 0.0f;
		var firstElementWidth = 0.0f;
		var childrenCount = GetVisibleChildrenCount();

		for (var i = rowStartIndex; i < rowStartIndex + _columns; i++) {
			if (i >= childrenCount) {
				if (_useFirstRowElementWidth && firstElementWidth > 0) {
					rowWidth += firstElementWidth + _hSeparation;
				}
				continue;
			}

			var child = GetVisibleChild(i);
			if (child == null)
				continue;

			if (i == rowStartIndex) {
				firstElementWidth = child.Size.X;
			}

			rowWidth += child.Size.X;

			if (i != rowStartIndex) {
				rowWidth += _hSeparation;
			}
		}

		return rowWidth;
	}

	private float GetRowStartX(float rowWidth, float sizeX) {
		return _horizontalAlignment switch {
			Alignment.Start => 0.0f,
			Alignment.Center => (sizeX - rowWidth) / 2.0f,
			Alignment.End => sizeX - rowWidth,
			_ => 0.0f
		};
	}

	private float GetStartY(float containerSizeY, float customMinimumSizeY) {
		return _verticalAlignment switch {
			Alignment.Start => 0.0f,
			Alignment.Center => (customMinimumSizeY - containerSizeY) / 2.0f,
			Alignment.End => customMinimumSizeY - containerSizeY,
			_ => 0.0f
		};
	}

	private void AnimatePositionChange(Control child, Vector2 newPosition) {
		if (child == null) {
			return;
		}

		var tween = child.CreateTween();
		tween.SetEase(Tween.EaseType.InOut);
		tween.SetTrans(Tween.TransitionType.Cubic);
		tween.TweenProperty(child, "position", newPosition, 0.4f);

		_activeTweens.Add(tween);
	}

	private void CancelAllAnimations() {
		foreach (var tween in _activeTweens.Where(tween => tween != null && IsInstanceValid(tween))) {
			tween.Kill();
		}

		_activeTweens.Clear();
	}

	public void Update() {
		_minimumSize = GetContainerSize();

		var visibleCount = GetVisibleChildrenCount();
		var rows = Mathf.CeilToInt(visibleCount / (float)_columns);
		var startY = GetStartY(_minimumSize.Y, Size.Y);

		for (var i = 0; i < rows; i++) {
			if (i != 0) {
				startY += _vSeparation;
			}

			var rowStartIndex = i * _columns;
			UpdateRow(rowStartIndex, startY, Size.X);

			var maxHeight = GetMaxHeightRow(rowStartIndex);
			startY += maxHeight;
		}

		_firstSortDone = true;
	}

	public override void _Notification(int what) {
		if (what == NotificationSortChildren) {
			if (UpdateChildOrderDisabled) {
				return;
			}

			EmitSignal(SignalName.OnChildOrderChanged);

			CancelAllAnimations();

			Update();
		}
	}

	public override Vector2 _GetMinimumSize() {
		return _minimumSize;
	}

	public override void _ExitTree() {
		CancelAllAnimations();
	}
}