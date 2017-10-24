using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ImageMesh.Samplers
{
    public class PixelSampler : IPixelSampler
    {
        public int Radius { get; set; }
        public virtual float Power { get; set; }

        public virtual Color Sample(Bitmap b, Point p)
        {
            return b.GetPixel(p.X, p.Y);
        }
    }
}
