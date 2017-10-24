using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMesh.Samplers
{
    public class MinSampler : PixelSampler
    {
        public MinSampler()
        {
            Radius = 1;
        }

        public override Color Sample(Bitmap b, Point p)
        {
            List<Color> samples = new List<Color>();

            for(int y = p.Y - Radius; y < p.Y + Radius; y++)
            {
                for(int x = p.X - Radius; x < p.X + Radius; x++)
                {
                    if(x >= 0 && y >= 0 && x < b.Width && y < b.Height)
                    {
                        samples.Add(b.GetPixel(x, y));
                    }
                }
            }

            Color min = samples[0];

            for(int i = 1; i < samples.Count; i++)
            {
                Color c = samples[i];

                if(c.R < min.R || c.B < min.B || c.G < min.G)
                {
                    min = c;
                }
            }

            return min;
        }
    }
}
