using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMesh.Samplers
{
    public class GaussianSampler : PixelSampler
    {
        static int grad = 1; //actual radius to sample from

        float _power = 0.01f;
        protected float[] kernel = new float[]
        {
            0.01f, 0.01f, 0.01f,
            0.01f, 0.92f, 0.01f,
            0.01f, 0.01f, 0.01f
        };

        public override float Power
        {
            get
            {
                return _power;
            }
            set
            {
                _power = value;
                UpdateKernel();
            }
        }

        public GaussianSampler() {}

        public override Color Sample(Bitmap b, Point p)
        {
            float rc = 0, gc = 0, bc = 0;
            int cx = 0;
            int cy = 0;

            for(int y = p.Y - grad; y < p.Y + grad; y++)
            {
                cx = 0;
                for (int x = p.X - grad; x < p.X + grad; x++)
                {
                    float blur = kernel[cx + cy * 3];
                    if (x >= 0 && y >= 0 && x < b.Width && y < b.Height)
                    {
                        Color c = b.GetPixel(x, y);
                        rc += c.R * blur;
                        gc += c.G * blur;
                        bc += c.B * blur;
                    }
                    cx++;
                }
                cy++;
            }

            return Color.FromArgb((int)rc, (int)gc, (int)bc);
        }

        void UpdateKernel()
        {
            float max = 1.0f - (Power * 8);
            kernel[0] = Power;
            kernel[1] = Power;
            kernel[2] = Power;
            kernel[3] = Power;
            kernel[4] = max;
            kernel[5] = Power;
            kernel[6] = Power;
            kernel[7] = Power;
            kernel[8] = Power;
        }
    }
}
