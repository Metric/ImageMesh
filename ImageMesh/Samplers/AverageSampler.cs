using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMesh.Samplers
{
    public class AverageSampler : PixelSampler
    {
        public AverageSampler()
        {
            Radius = 1;
        }

        public override Color Sample(Bitmap b, Point p)
        {
            float rc = 0, gc = 0, bc = 0;
            int samples = 0;

            for(int y = p.Y - Radius; y < p.Y + Radius; y++)
            {
                for (int x = p.X - Radius; x < p.X + Radius; x++)
                {
                    if (x >= 0 && y >= 0 && x < b.Width && y < b.Height)
                    {
                        Color c = b.GetPixel(x, y);
                        rc += c.R;
                        gc += c.G;
                        bc += c.B;
                        samples++;
                    }
                }
            }

            float sMax = 1.0f / samples;

            rc *= sMax;
            gc *= sMax;
            bc *= sMax;

            return Color.FromArgb((int)rc, (int)gc, (int)bc);
        }
    }
}
