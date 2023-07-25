using Godot;
using System;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts;

public partial class MapGrid : Node2D {
    public override void _Draw() {
        const int radius = 13;
        const int diameter = radius * 2;
        const int size = 20;
        var center = new Vector2(radius + 0.5f, radius + 0.5f);
        for (int i = 0; i < diameter + 1; i++) {
            for (int j = 0; j < diameter + 1; j++) {
                DrawRect(new Rect2(i * size, j * size, size, size), Color.FromHtml("#FF0000"), false, 2);
            }
        }

        for (int i = 0; i < diameter + 1; i++) {
            for (int j = 0; j < diameter + 1; j++) {
                if (CircleHelper.IsVector2InsideCircle(new Vector2(i + 0.5f, j + 0.5f), center, radius + 0.5f))
                    DrawRect(new Rect2(i * size, j * size, size, size), Color.FromHtml("#0000FF"), false, 2);
            }
        }
    }
}
