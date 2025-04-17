using System.Threading.Tasks;
using Godot;

namespace GodotMobileControls;

[Tool, GlobalClass]
public partial class MobileItemScrollContainer : MobileScrollContainer {
	[Export] public Control ItemsContainer;
	
	private int _visibleItemsCount = 1;
	[Export] public int VisibleItemsCount {
		get => _visibleItemsCount;
		set {
			if (value == _visibleItemsCount) {
				return;
			}
			
			_visibleItemsCount = value;
			UpdateCustomMinimumSize();
		}
	}
	
	private int _firstVisibleItemIndex;
	private int FirstVisibleItemIndex {
		get => _firstVisibleItemIndex;
		set {
			if (value == _firstVisibleItemIndex) {
				return;
			}
			
			_firstVisibleItemIndex = value;
			UpdateCustomMinimumSize();
		}
	}
	
	private int LastVisibleItemIndex => FirstVisibleItemIndex + VisibleItemsCount - 1;

	private Control FirstVisibleItem => ItemsContainer.GetChildOrNull<Control>(FirstVisibleItemIndex);
	private Control LastVisibleItem => ItemsContainer.GetChildOrNull<Control>(LastVisibleItemIndex);


	public async Task ScrollToItem(int direction) {
		var itemsCount = ItemsContainer.GetChildCount();

		var nextChildIndex = FirstVisibleItemIndex + direction;

		if (nextChildIndex < 0 || nextChildIndex > itemsCount - VisibleItemsCount) {
			return;
		}
		
		FirstVisibleItemIndex = nextChildIndex;
		await ScrollToPosition(FirstVisibleItem.Position * -1, 0.5f);
	}
	
	public void ScrollToPreviousItem() {
		_ = ScrollToItem(-1);
	}
	
	public async Task ScrollToPreviousItemAsync() {
		await ScrollToItem(-1);
	}
	
	public void ScrollToNextItem() {
		_ = ScrollToItem(1);
	}
	
	public async Task ScrollToNextItemAsync() {
		await ScrollToItem(1);
	}

	private void UpdateCustomMinimumSize() {
		if (FirstVisibleItem == null || LastVisibleItem == null) {
			return;
		}
		
		CustomMinimumSize = LastVisibleItem.Position + LastVisibleItem.Size - FirstVisibleItem.Position;
		Callable.From(() => Size = CustomMinimumSize).CallDeferred();
	}
}