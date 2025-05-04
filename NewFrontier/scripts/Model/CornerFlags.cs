using System;

namespace NewFrontier.scripts.Model;

[Flags]
public enum CornerFlags {
	TopLeft = 1,
	TopRight = 2,
	BottomLeft = 4,
	BottomRight = 8
}
