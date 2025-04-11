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
			Update();
		}
	}

	private int _hSeparation;
	[Export] public int HSeparation {
		get => _hSeparation;
		set {
			_hSeparation = value;
			Update();
		}
	}

	private int _vSeparation;
	[Export] public int VSeparation {
		get => _vSeparation;
		set {
			_vSeparation = value;
			Update();
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
			Update();
		}
	}
	
	public enum AlignmentEnum {
		Start,
		Center,
		End
	}
	private AlignmentEnum _horizontalAlignment = AlignmentEnum.Center;
	[Export] public AlignmentEnum HorizontalAlignment {
		get => _horizontalAlignment;
		set {
			_horizontalAlignment = value;
			Update();
		}
	}
	private AlignmentEnum _verticalAlignment = AlignmentEnum.Center;
	[Export] public AlignmentEnum VerticalAlignment {
		get => _verticalAlignment;
		set {
			_verticalAlignment = value;
			Update();
		}
	}

	private bool _animateChildOrderDisabled = true;
	[Export] public bool AnimateChildOrderDisabled;

	/*
	 * If true, the width of the first row element will be used as width of the missing row elements.
	 */
	private bool _useFirstRowElementWidth;
	[Export] public bool UseFirstRowElementWidth {
		get => _useFirstRowElementWidth;
		set {
			_useFirstRowElementWidth = value;
			Update();
		}
	}
	
	private Vector2 ContainerSize {
		get {
			var rows = Mathf.CeilToInt(GetChildren().Count / (float)Columns);
		
			var height = 0f;
			var largestWidth = 0f;

			for (var rowIndex = 0; rowIndex < rows; rowIndex++) {
				if (rowIndex != 0) {
					height += VSeparation;
				}
			
				var rowStartIndex = rowIndex * Columns;
			
				var width = GetRowWidth(rowStartIndex);
				if (width > largestWidth) {
					largestWidth = width;
				}
			
				height += GetMaxHeightRow(rowStartIndex);
			}
		
			return new Vector2(largestWidth, height);
		}
	}

	public bool UpdateChildOrderDisabled;

	private CancellationTokenSource _animationCancellation = new();
	
	private Vector2 _minimumSize;
	
	public event Action OnChildOrderChanged;

	public override void _Ready() {
		Update();
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

		Update();
	}
	
	public void SwapChildren(Control child1, Control child2) {
		var index1 = child1.GetIndex();
		var index2 = child2.GetIndex();
			
		UpdateChildOrderDisabled = true;

		MoveChild(child1, index2);
		MoveChild(child2, index1);
			
		UpdateChildOrderDisabled = false;
		Update();
	}

	public void Update() {
		_minimumSize = ContainerSize;
		CustomMinimumSize = _minimumSize;
		
		var rows = Mathf.CeilToInt(GetChildren().Count / (float)Columns);
		var startY = GetStartY(_minimumSize.Y, Size.Y);
		
		for (var i = 0; i < rows; i++) {
			if (i != 0) {
				startY += VSeparation;
			}

			var rowStartIndex = i * Columns;

			UpdateRow(rowStartIndex, startY, Size.X);

			var maxHeight = GetMaxHeightRow(rowStartIndex);

			startY += maxHeight;
		}
	}

	private void UpdateRow(int rowStartIndex, float rowStartY, float sizeX) {
		var tasks = new List<Task>();

		var rowWidth = GetRowWidth(rowStartIndex);
		var startX = GetRowStartX(rowWidth, sizeX);

		for (var childIndex = rowStartIndex; childIndex < rowStartIndex + Columns; childIndex++) {
			if (childIndex >= GetChildren().Count) {
				return;
			}

			if (childIndex != rowStartIndex) {
				startX += HSeparation;
			}

			var child = GetChild(childIndex);

			var position = new Vector2(startX, rowStartY);
			if (!_animateChildOrderDisabled && !AnimateChildOrderDisabled && !Engine.IsEditorHint()) {
				var task = AnimatePositionChange(child, position);
				tasks.Add(task);
			}
			else {
				child.Position = position;
			}

			startX += child.Size.X;
		}

		_ = Task.WhenAll(tasks);
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

		var childrenCount = GetChildren().Count;
		
		for (var i = rowStartIndex; i < rowStartIndex + Columns; i++) {
			if (i >= childrenCount) {
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

	private float GetRowStartX(float rowWidth, float sizeX) {
		return HorizontalAlignment switch {
			AlignmentEnum.Start => 0,
			AlignmentEnum.Center => (sizeX - rowWidth) / 2,
			AlignmentEnum.End => sizeX - rowWidth,
			_ => 0
		};
	}
	
	private float GetStartY(float containerSizeY, float customMinimumSizeY) {
		return VerticalAlignment switch {
			AlignmentEnum.Start => 0,
			AlignmentEnum.Center => (customMinimumSizeY - containerSizeY) / 2,
			AlignmentEnum.End => customMinimumSizeY - containerSizeY,
			_ => 0
		};
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

	public override Vector2 _GetMinimumSize() {
		return _minimumSize;
	}
}