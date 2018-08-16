namespace Nsg.Viewer.OSXWindow
{
    public struct CGSize
    {
        public double width;
        public double height;

        public CGSize(double width, double height)
        {
            this.width = width;
            this.height = height;
        }

        public override string ToString() => string.Format("{0} x {1}", width, height);
    }
}