using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMesh.ThreeMath;

namespace ImageMesh.MeshGen
{
    public class MeshG
    {
        //Storage for all the stuff

        //vertices are a tuple of <x,y,z>
        public List<float> Vertices { get; set; }
        
        //normals are tuple of <x,y,z>
        public List<float> Normals { get; set; }
        
        //colors are tuple of <r,g,b>
        public List<float> Colors { get; set; }

        //the indices for forming triangles based on vertices
        public List<uint> Triangles { get; set; }

        public Vector3f center;

        public MeshG()
        {
            Vertices = new List<float>();
            Normals = new List<float>();
            Colors = new List<float>();
            Triangles = new List<uint>();
            center = new Vector3f();
        }

        public void FindCenter()
        {
            float minX = Vertices[0];
            float minY = Vertices[1];
            float maxX = minX;
            float maxY = minY;

            for(int i = 0; i < Vertices.Count - 2; i+=3)
            {
                float x = Vertices[i];
                float y = Vertices[i + 1];

                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }

            center.x = (minX + maxX) * 0.5f;
            center.y = (minY + maxY) * 0.5f;
        }
    }
}
