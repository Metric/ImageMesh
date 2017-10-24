using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMesh.ThreeMath
{
    static class VectorUtils
    {
        /// <summary>
        /// 
        /// The point must be in a -1 to 1 space for this to work properly
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="radius"></param>
        public static void SpherePoint(Vector3f p, float radius, float dw, float dy)
        {
            float u = p.x * dw;
            float v = p.y * dy;

            float lon = (float)Math.PI * u;
            float lat = (float)Math.PI * v * 0.5f;
            float r = radius + p.z;

            float nx = r * (float)Math.Cos(lat) * (float)Math.Cos(lon);
            float ny = r * (float)Math.Cos(lat) * (float)Math.Sin(lon);
            float nz = r * (float)Math.Sin(lat);

            p.x = nx;
            p.y = ny;
            p.z = nz;
        }
    }
}
