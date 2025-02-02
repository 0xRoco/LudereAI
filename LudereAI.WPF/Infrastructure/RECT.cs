using System.Drawing;
using System.Runtime.InteropServices;

namespace LudereAI.WPF.Infrastructure;

/// <summary>
/// Represents a rectangle structure used for Win32 interop operations.
/// Implements value semantics and provides safe conversion operations.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Rect : IEquatable<Rect>
{
    public Rect(Rect rectangle)
        : this(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom)
    {
    }

    public Rect(int left, int top, int right, int bottom)
    {
        if (right < left)
            throw new ArgumentException("Right cannot be less than left", nameof(right));
        if (bottom < top)
            throw new ArgumentException("Bottom cannot be less than top", nameof(bottom));

        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public int Left { get; }

    public int Top { get; }

    public int Right { get; }

    public int Bottom { get; }

    public int X => Left;
    public int Y => Top;
    public int Height => Bottom - Top;
    public int Width => Right - Left;

    public Point Location => new(Left, Top);
    public Size Size => new(Width, Height);

    public static Rect FromRectangle(Rectangle rectangle) 
        => new(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);

    public Rectangle ToRectangle() 
        => new(Left, Top, Width, Height);

    public static implicit operator Rectangle(Rect rectangle) 
        => rectangle.ToRectangle();

    public static explicit operator Rect(Rectangle rectangle) 
        => FromRectangle(rectangle);

    public static bool operator ==(Rect left, Rect right) 
        => left.Equals(right);

    public static bool operator !=(Rect left, Rect right) 
        => !left.Equals(right);

    public bool Equals(Rect other) 
        => Left == other.Left && 
           Top == other.Top && 
           Right == other.Right && 
           Bottom == other.Bottom;

    public override bool Equals(object? obj) 
        => obj switch
        {
            Rect rect => Equals(rect),
            Rectangle rectangle => Equals(FromRectangle(rectangle)),
            _ => false
        };

    public override int GetHashCode() 
        => HashCode.Combine(Left, Top, Right, Bottom);

    public override string ToString() 
        => $"{{Left: {Left}; Top: {Top}; Right: {Right}; Bottom: {Bottom}}}";
}