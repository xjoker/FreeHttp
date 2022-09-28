using System.Drawing;
using System.Drawing.Drawing2D;

namespace FreeHttp.FreeHttpControl.ControlHelper
{
    internal class LoadBitmap
    {
        private readonly Color _circleColor = Color.Red;

        private readonly float _circleSize = 0.8f;

        //private int count = -1;
        //private ArrayList images = new ArrayList();
        //public Bitmap[] bitmap = new Bitmap[8];
        private readonly int _value = 1;
        private int Height;
        private int Width;

        public LoadBitmap(Size size)
        {
            Width = size.Width;
            Height = size.Height;
        }

        internal void SetSize(int size)
        {
            SetSize(new Size(size, size));
        }

        public void SetSize(Size size)
        {
            Width = size.Width;
            Height = size.Height;
        }

        public Bitmap DrawCircle(int j)
        {
            const float angle = 360.0F / 8;
            var map = new Bitmap(Width, Height);
            var g = Graphics.FromImage(map);

            g.TranslateTransform(Width / 2.0F, Height / 2.0F);
            g.RotateTransform(angle * _value);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var a = new int[8] { 25, 50, 75, 100, 125, 150, 175, 200 };
            for (var i = 1; i <= 8; i++)
            {
                var alpha = a[(i + j - 1) % 8];
                var drawColor = Color.FromArgb(alpha, _circleColor);
                using (var brush = new SolidBrush(drawColor))
                {
                    var sizeRate = 3.5F / _circleSize;
                    var size = Width / (6 * sizeRate);

                    var diff = Width / 10.0F - size;

                    var x = Width / 80.0F + diff;
                    var y = Height / 80.0F + diff;
                    g.FillEllipse(brush, x, y, size, size);
                    g.RotateTransform(angle);
                }
            }

            //g.DrawLine(new Pen(Color.Red),1,1,10,10);
            //g.Save();
            return map;
        }
    }
}