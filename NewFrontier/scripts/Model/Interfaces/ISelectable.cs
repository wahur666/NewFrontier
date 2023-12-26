using NewFrontier.scripts.UI;

namespace NewFrontier.scripts.Model.Interfaces;

public interface ISelectable {
	protected SelectionRect SelectionRect { get; }

	public bool Selected { get; set; }
	public void SetSelection(bool selected) {
		SelectionRect.Selected = selected;
	}
}
