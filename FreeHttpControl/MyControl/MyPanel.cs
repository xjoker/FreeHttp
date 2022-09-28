using System.Windows.Forms;

namespace FreeHttp.FreeHttpControl
{
    internal class MyPanel : Panel
    {
        public MyPanel()
        {
            //this.SetStyle(System.Windows.Forms.ControlStyles.UserPaint |System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,true);
            SetStyle(
                ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint,
                true);
            UpdateStyles();
        }
    }
}