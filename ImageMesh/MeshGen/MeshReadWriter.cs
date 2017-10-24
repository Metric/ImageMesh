using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace ImageMesh.MeshGen
{
    public class MeshReadWriter
    {
        public static MeshG ParseMeshString(string data)
        {
            MeshG m = new MeshG();

            string[] lines = data.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            bool inVerts = false;
            bool inTriangles = false;

            for(int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if(line.ToLower().Equals("vertices"))
                {
                    inVerts = true;
                    inTriangles = false;
                    continue;
                }
                else if(line.ToLower().Equals("triangles"))
                {
                    inVerts = false;
                    inTriangles = true;
                    continue;
                }

                if(inVerts)
                {
                    ParseVertex(line, ref m);
                }
                else if(inTriangles)
                {
                    ParseTriangle(line, ref m);
                }
            }

            return m;
        }

        protected static void ParseTriangle(string line, ref MeshG m)
        {
            string[] components = line.Split(new char[] { ' ' });

            if(components.Length != 3)
            {
                return;
            }

            uint idx = uint.Parse(components[0]);
            uint idx2 = uint.Parse(components[1]);
            uint idx3 = uint.Parse(components[2]);

            m.Triangles.Add(idx);
            m.Triangles.Add(idx2);
            m.Triangles.Add(idx3);
        }

        protected static void ParseVertex(string line, ref MeshG m)
        {
            string[] components = line.Split(new char[] { ' ' });

            //all components are needed position normal color
            if(components.Length != 9)
            {
                return;
            }

            float x = float.Parse(components[0]);
            float y = float.Parse(components[1]);
            float z = float.Parse(components[2]);

            float nx = float.Parse(components[3]);
            float ny = float.Parse(components[4]);
            float nz = float.Parse(components[5]);

            float r = float.Parse(components[6]);
            float g = float.Parse(components[7]);
            float b = float.Parse(components[8]);

            m.Vertices.Add(x);
            m.Vertices.Add(y);
            m.Vertices.Add(z);

            m.Normals.Add(nx);
            m.Normals.Add(ny);
            m.Normals.Add(nz);

            m.Colors.Add(r);
            m.Colors.Add(g);
            m.Colors.Add(b);
        }

        public static void WriteMeshToFile(MeshG m, string path)
        {
            if (m == null) return;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("vertices");

            for(int i = 0; i < m.Vertices.Count - 2; i+=3)
            {
                string line = "";

                float x = m.Vertices[i];
                float r = m.Colors[i];
                float nx = m.Normals[i];

                float y = m.Vertices[i + 1];
                float g = m.Colors[i + 1];
                float ny = m.Normals[i + 1];

                float z = m.Vertices[i + 2];
                float b = m.Colors[i + 2];
                float nz = m.Normals[i + 2];
                
                //each vertice line contains the following x y z nx ny nz r g b
                //or basically the position normal color
                //they are all floats
                line = x + " " + y + " " + z + " " + nx + " " + ny + " " + nz + " " + r + " " + g + " " + b;
                builder.AppendLine(line);
            }

            builder.AppendLine("triangles");

            //each line corresponds to a triangle
            //with each: index(v1) index2(v2) index3(v3)
            //triangles are CW orientation
            //all values are uints
            for(int i = 0; i < m.Triangles.Count - 2; i+=3)
            {
                string line = "";
                uint idx1 = m.Triangles[i];
                uint idx2 = m.Triangles[i + 1];
                uint idx3 = m.Triangles[i + 2];

                line = idx1 + " " + idx2 + " " + idx3;
                builder.AppendLine(line);
            }

            File.WriteAllText(path, builder.ToString());
        }
    }
}
