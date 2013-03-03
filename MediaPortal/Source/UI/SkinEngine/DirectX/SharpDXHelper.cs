using System.ComponentModel;
using System.Drawing;
using Color = SharpDX.Color;
using Rectangle = SharpDX.Rectangle;
using RectangleF = SharpDX.RectangleF;

namespace MediaPortal.UI.SkinEngine.DirectX
{
  public static class SharpDXHelper
  {
    public static Size Size(this RectangleF rectangle)
    {
      return new Size((int) rectangle.Width, (int) rectangle.Height);
    }
    public static Size Size(this Rectangle rectangle)
    {
      return new Size(rectangle.Width, rectangle.Height);
    }
    public static SizeF SizeF(this RectangleF rectangle)
    {
      return new SizeF(rectangle.Width, rectangle.Height);
    }
    public static PointF Location(this RectangleF rectangle)
    {
      return new PointF(rectangle.X, rectangle.Y);
    }
    public static void SetLeft(this RectangleF rectangle, float left)
    {
      float oldWidth = rectangle.Width;
      rectangle.Left = left;
      rectangle.Right = left + oldWidth;
    }
    public static void SetTop(this RectangleF rectangle, float top)
    {
      float oldHeight = rectangle.Height;
      rectangle.Top = top;
      rectangle.Bottom = top + oldHeight;
    }
    public static void SetWidth(this RectangleF rectangle, float width)
    {
      rectangle.Right = rectangle.Left + width;
    }
    public static void SetHeight(this RectangleF rectangle, float height)
    {
      rectangle.Bottom = rectangle.Top + height;
    }
    public static bool IsEmpty(this RectangleF rectangle)
    {
      return rectangle.Width == 0f && rectangle.Height == 0f;
    }
    public static System.Drawing.Rectangle ToRectF(this Rectangle rectangle)
    {
      return new System.Drawing.Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }
    public static System.Drawing.RectangleF ToRectF(this RectangleF rectangle)
    {
      return new System.Drawing.RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }
    public static RectangleF FromRectF(this System.Drawing.RectangleF rectangle)
    {
      return new RectangleF(rectangle.X, rectangle.Y, rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height);
    }
    public static Rectangle FromRect(this System.Drawing.RectangleF rectangle)
    {
      return new Rectangle((int) rectangle.X, (int) rectangle.Y, (int) (rectangle.X + rectangle.Width), (int) (rectangle.Y + rectangle.Height));
    }
    public static Rectangle FromRect(this System.Drawing.Rectangle rectangle)
    {
      return new Rectangle(rectangle.X, rectangle.Y, (rectangle.X + rectangle.Width), (rectangle.Y + rectangle.Height));
    }
    public static RectangleF CreateRectangleF(PointF location, SizeF size)
    {
      return new RectangleF(location.X, location.Y, location.X + size.Width, location.Y + size.Height);
    }
    public static RectangleF CreateRectangleF(float x, float y, float width, float height)
    {
      return new RectangleF(x, y, x + width, y + height);
    }
    public static Rectangle CreateRectangle(PointF location, SizeF size)
    {
      return new Rectangle((int) location.X, (int) location.Y, (int) (location.X + size.Width), (int) (location.Y + size.Height));
    }
    public static Rectangle CreateRectangle(int x, int y, int width, int height)
    {
      return new Rectangle(x, y, x + width, y + height);
    }
    public static Color FromArgb(int alpha, Color color)
    {
      // Attention: the documentation of this constructor is not correct!
      return new Color(color.G, color.B, alpha, color.R);
    }
    public static Color FromArgb(int alpha, int r, int g, int b)
    {
      // Attention: the documentation of this constructor is not correct!
      return new Color(g, b, alpha, r);
    }
    public static Color ToColor(this System.Drawing.Color color)
    {
      // Attention: the documentation of this constructor is not correct!
      return new Color(color.G, color.B, color.A, color.R);
    }
    public static Color ToColor(string colorName)
    {
      return ((System.Drawing.Color) TypeDescriptor.GetConverter(typeof(System.Drawing.Color)).ConvertFromString(colorName)).ToColor();
    }
  }
}
