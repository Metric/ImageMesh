using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using ImageMesh.Samplers;
using ImageMesh.ThreeMath;

namespace ImageMesh.MeshGen
{
    public class Meshifier
    {
        /// <summary>
        /// Basicaly grayscale takes average of all 3 components
        /// Where as the others take the one or average of 2 of them
        /// </summary>
        public enum DisplacementMode
        {
            Grayscale = 0,
            Red = 1,
            Blue = 2,
            Green = 3,
            RedGreen = 4,
            BlueGreen = 5,
            RedBlue = 6
        }

        public int Tolerance
        {
            get;set;
        }

        public float Decimation
        {
            get;set;
        }

        public PixelSampler Sampler
        {
            get; set;
        }

        public bool UseDisplacement
        {
            get; set;
        }

        public DisplacementMode DisplaceMode { get; set; }

        public bool Sphererize { get; set; }

        public float DisplacementPower { get; set; }

        public int TriangleCount { get; protected set; }

        protected Bitmap original;
        protected Bitmap img;
        protected float imageScale;

        public Meshifier(Bitmap b)
        {
            img = original = b;
            Decimation = 1;
            Tolerance = 1;
            Sampler = new PixelSampler();
            DisplacementPower = 1.0f;
            imageScale = 1;
        }

        public void ScaleImage(float perc)
        {
            perc = perc <= 0 ? 1.0f : perc;
            img = new Bitmap(original, (int)(original.Width * perc), (int)(original.Height * perc));
            imageScale = perc;
        }

        public async Task<MeshG> Generate()
        {
            return await Task.Run<MeshG>(() =>
            {
                MeshG m = new MeshG();

                CreateMesh(ref m);

                return m;
            });
        }

        //This is what actually creates the mesh
        protected void CreateMesh(ref MeshG m)
        {
            float d2 = 2.0f / (img.Width - 1);
            float d3 = 2.0f / (img.Height - 1);
            float cx = (img.Width - 1) * 0.5f;
            float cy = (img.Height - 1) * 0.5f;

            List<Vector3f> points = new List<Vector3f>();
            List<Vector3f> graph = new List<Vector3f>();

            float? lastPixel = null;

            for(int u = 0; u < img.Height; u++)
            {
                lastPixel = null;
                for(int v = 0; v < img.Width; v++)
                {
                    Color c = Sampler.Sample(img, new Point(v, u));
                    float g = (float)(c.R + c.G + c.B) / 3.0f;

                    Vector3f p = new Vector3f(0,0,0);
                    //store the index for faster processing of triangle generation
                    p.index = points.Count;
                    p.color = c;

                    p.x = v;
                    p.y = u;

                    if (lastPixel == null)
                    {
                        lastPixel = g;
                        graph.Add(p);
                        points.Add(p);
                    }
                    else
                    {
                        float len = Math.Abs(g - lastPixel.Value);

                        bool lengthFit = (u == 0 || u == img.Height - 1);
                        bool heightFit = (v == 0 || v == img.Width - 1);

                        if (len > Tolerance || lengthFit || heightFit)
                        {
                            lastPixel = g;
                            graph.Add(p);
                            points.Add(p);
                        }
                        else
                        {
                            //as a placeholder for an invalid spot in the graph
                            p.index = -1;
                            graph.Add(p);
                        }
                    }
                }
            }

            //create triangles while flat still
            //it just makes it easier
            CreateTriangles(ref m, graph, ref points);

            //Displace the points if necessary
            if(UseDisplacement)
            {
                Displace(points);
            }

            //Colors, points, and normals share the same amount of data
            for(int i = 0; i < points.Count; i++)
            {

                Vector3f p = points[i];
                Color c = p.color;
                m.Colors.Add((float)c.R / 255.0f);
                m.Colors.Add((float)c.G / 255.0f);
                m.Colors.Add((float)c.B / 255.0f);

                //Convert to sphere coordinates if needed
                if(Sphererize)
                {
                    //move to center
                    p.x -= cx;
                    p.y -= cy;

                    VectorUtils.SpherePoint(p, 1.0f, d2, d3);
                }

                m.Vertices.Add(p.x);
                m.Vertices.Add(p.y);
                m.Vertices.Add(p.z);

                Vector3f normal = p.Normalize();
                m.Normals.Add(normal.x);
                m.Normals.Add(normal.y);
                m.Normals.Add(normal.z);
            }
        }

        //Displaces via Z axis
        protected void Displace(List<Vector3f> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Vector3f p = points[i];
                Color c = points[i].color;

                float g = 0;

                switch(DisplaceMode)
                {
                    case DisplacementMode.Red:
                        g = c.R;
                        break;
                    case DisplacementMode.Green:
                        g = c.G;
                        break;
                    case DisplacementMode.Blue:
                        g = c.B;
                        break;
                    case DisplacementMode.RedGreen:
                        g = (float)(c.R + c.G) / 2.0f;
                        break;
                    case DisplacementMode.RedBlue:
                        g = (float)(c.R + c.B) / 2.0f;
                        break;
                    case DisplacementMode.BlueGreen:
                        g = (float)(c.B + c.G) / 2.0f;
                        break;
                    case DisplacementMode.Grayscale:
                    default:
                        g = (float)(c.R + c.B + c.G) / 3.0f;
                        break;
                }

                p.z = g / 255.0f * DisplacementPower;
            }
        }

        //Helper function
        protected Vector3f GetPoint(List<Vector3f> points, int x, int y)
        {
            int idx = x + y * img.Width;
            
            if(idx >= 0 && idx < points.Count)
            {
                return points[idx];
            }

            return null;
        }

        //Generates the Triangles / Indices
        protected void CreateTriangles(ref MeshG m, List<Vector3f> graph, ref List<Vector3f> points)
        {
            TriangleCount = 0;

            DelaunayTriangulator.Triangulator trify = new DelaunayTriangulator.Triangulator();

            List<DelaunayTriangulator.Vertex> vertices = new List<DelaunayTriangulator.Vertex>();

            foreach(Vector3f p in points)
            {
                vertices.Add(new DelaunayTriangulator.Vertex(p.x, p.y));
            }

            List<DelaunayTriangulator.Triad> triads = trify.Triangulation(vertices);
            TriangleCount = triads.Count;

            foreach(DelaunayTriangulator.Triad t in triads)
            {
                Vector3f a = points[t.a];
                Vector3f b = points[t.b];
                Vector3f c = points[t.c];

                //due to how the triangle flipping is done in the triangulation part
                //it does not guarantee 
                //triangles in the proper facing directions
                //therefore we make sure the ordering of the indices
                //is proper so all triangles face the same direction
                float ab = a.Cross2D(b);
                float bc = b.Cross2D(c);
                float ac = c.Cross2D(a);

                float sum = (ab + bc + ac) * 0.5f;

                if (sum <= 0)
                {
                    m.Triangles.Add((uint)t.a);
                    m.Triangles.Add((uint)t.b);
                    m.Triangles.Add((uint)t.c);
                }
                else
                {
                    m.Triangles.Add((uint)t.c);
                    m.Triangles.Add((uint)t.b);
                    m.Triangles.Add((uint)t.a);
                }
            }
        }
    }
}
