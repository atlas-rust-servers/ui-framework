using JetBrains.Annotations;
using UnityEngine;

namespace Oxide.Ext.UiFramework.Offsets;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public readonly struct UiOffset
{
    public static readonly UiOffset None = new(0, 0, 0, 0);
    public static readonly UiOffset Scaled = new(1280, 720);

    public readonly Vector2 Min;
    public readonly Vector2 Max;

    public Vector2 Size => Max - Min;
    public float Width => Max.x - Min.x;
    public float Height => Max.y - Min.y;

    public UiOffset(int width, int height) : this(-width / 2f, -height / 2f, width / 2f, height / 2f)
    {
    }

    public UiOffset(float xMin, float yMin, float xMax, float yMax)
    {
        Min = new Vector2(xMin, yMin);
        Max = new Vector2(xMax, yMax);
    }

    public static UiOffset Horizontal(float xMin, float xMax) => new(xMin, 0, xMax, 0);

    public static UiOffset Vertical(float yMin, float yMax) => new(0, yMin, 0, yMax);

    public static UiOffset XMin(float xMin) => new(xMin, 0, 0, 0);
    public static UiOffset XMin(float xMin, float width) => new(xMin, 0, xMin + width, 0);

    public static UiOffset XMax(float xMax) => new(0, 0, xMax, 0);
    public static UiOffset XMax(float xMax, float width) => new(xMax - width, 0, xMax, 0);

    public static UiOffset YMin(float yMin) => new(0, yMin, 0, 0);
    public static UiOffset YMin(float yMin, float height) => new(0, yMin, 0, yMin + height);

    public static UiOffset YMax(float yMax) => new(0, 0, 0, yMax);
    public static UiOffset YMax(float yMax, float height) => new(0, 0, 0, yMax - height);
    
    public static UiOffset XYMin(float xMin, float yMin) => new(xMin, yMin, 0, 0);
    public static UiOffset XYMax(float xMax, float yMax) => new(0, 0, xMax, yMax);
    public static UiOffset XMinYMax(float xMin, float yMax) => new(xMin, 0, 0, yMax);
    public static UiOffset XMaxYMin(float xMax, float yMin) => new(0, yMin, xMax, 0);
    
    public static UiOffset FromPoint(float x, float y, float width, float height) => new(x, y, x + width, y + height);
    
    

    public override string ToString()
    {
        return $"({Min.x:0}, {Min.y:0}) ({Max.x:0}, {Max.y:0}) WxH:({Width} x {Height})";
    }
}