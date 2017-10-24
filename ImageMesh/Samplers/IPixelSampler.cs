using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ImageMesh.Samplers
{
    public interface IPixelSampler
    {
        Color Sample(Bitmap b, Point p);
    }
}
