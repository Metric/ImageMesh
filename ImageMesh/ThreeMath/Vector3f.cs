using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ImageMesh.ThreeMath
{
    public class Vector3f
    {
        public float x;
        public float y;
        public float z;

        public int index;

        public Color color;

        public float ro;

        public float Length
        {
            get
            {
                return Math.Abs(this.Magnitude());
            }
        }

        public float LengthSqr
        {
            get
            {
                return Math.Abs(this.MagnitudeSqr());
            }
        }

        public Vector3f()
        {
            this.x = 0;
            this.y = 0;
            this.z = 0;
            ro = -1;
            index = -1;
        }

        public Vector3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            ro = -1;
            index = -1;
        }

        public Vector3f Clone()
        {
            Vector3f n = new Vector3f(this.x, this.y, this.z);
            n.color = this.color;
            n.index = this.index;
            n.ro = this.ro;
            return n;
        }

        public Color AverageColor(Vector3f p)
        {
            float r = p.color.R;
            float g = p.color.G;
            float b = p.color.B;

            r += color.R;
            g += color.G;
            b += color.B;

            r *= 0.5f;
            g *= 0.5f;
            b *= 0.5f;

            return Color.FromArgb((int)r, (int)g, (int)b);
        }

        public float Magnitude()
        {
            return (float)Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
        }

        public float MagnitudeSqr()
        {
            return this.x * this.x + this.y * this.y + this.z * this.z;
        }

        public float Dot(Vector3f o)
        {
            return o.x * this.x + o.y * this.y + o.z * this.z;
        }

        public Vector3f Cross(Vector3f p)
        {
            float x = this.y * p.z - p.y * this.z;
            float y = -this.x * p.z + p.x * this.z;
            float z = this.x * p.y - p.x * this.y;

            return new Vector3f(x, y, z);
        }

        public float Cross2D(Vector3f p)
        {
            return this.x * p.y - this.y * p.x;
        }

        public float Angle(Vector3f p)
        {
            return (float)Math.Acos(this.Dot(p) / (this.Magnitude() * p.Magnitude()));
        }

        public Vector3f Normalize()
        {
            float mag = 1.0f / this.Magnitude();

            return new Vector3f(this.x * mag, this.y * mag, this.z * mag);
        }

        public float Distance(Vector3f other)
        {
            float dx = other.x - this.x;
            float dy = other.y - this.y;
            float dz = other.z - this.z;

            return Math.Abs((float)Math.Sqrt(dx * dx + dy * dy + dz * dz));
        }

        public float DistanceSqr(Vector3f other)
        {
            float dx = other.x - this.x;
            float dy = other.y - this.y;
            float dz = other.z - this.z;

            return Math.Abs(dx * dx + dy * dy + dz * dz);
        }

        public bool Equals(Vector3f v)
        {
            return v.x == x && v.y == y && v.z == z;
        }

        public static int Compare(Vector3f x, Vector3f y)
        {
            if(x < y)
            {
                return -1;
            }
            else if(x > y)
            {
                return 1;
            }

            return 0;
        }

        public static bool operator < (Vector3f p1, Vector3f p2)
        {
            if (p1.ro == p2.ro)
            {
                if (p1.x == p2.x)
                {
                    return p1.y < p2.y;
                }

                return p1.x < p2.x;
            }

            return p1.ro < p2.ro;
        }

        public static bool operator > (Vector3f p1, Vector3f p2)
        {
            if (p1.ro == p2.ro)
            {
                if (p1.x == p2.x)
                {
                    return p1.y > p2.y;
                }

                return p1.x > p2.x;
            }

            return p1.ro > p2.ro;
        }

        public static Vector3f operator - (Vector3f p1, Vector3f p2)
        {
            return new Vector3f(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);
        }

        public static Vector3f operator - (Vector3f p1, float v)
        {
            return new Vector3f(p1.x - v, p1.y - v, p1.z - v);
        }

        public static Vector3f operator + (Vector3f p1, Vector3f p2)
        {
            return new Vector3f(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);
        }

        public static Vector3f operator + (Vector3f p1, float v)
        {
            return new Vector3f(p1.x + v, p1.y + v, p1.z + v);
        }

        public static float operator * (Vector3f p1, Vector3f p2)
        {
            return p1.Dot(p2);
        }

        public static Vector3f operator * (Vector3f p1, float v)
        {
            return new Vector3f(p1.x * v, p1.y * v, p1.z * v);
        }

        public static Vector3f operator * (float v, Vector3f p1)
        {
            return new Vector3f(p1.x * v, p1.y * v, p1.z * v);
        }

        public static Vector3f operator / (Vector3f p1, float v)
        {
            float vm = Math.Max(v, 1);
            return new Vector3f(p1.x / vm, p1.y / vm, p1.z / vm);
        }

        public static Vector3f operator / (Vector3f p1, Vector3f p2)
        {
            return new Vector3f(p1.x / Math.Max(p2.x,1), p1.y / Math.Max(p2.y,1), p1.z / Math.Max(p2.z,1));
        }
    }
}
