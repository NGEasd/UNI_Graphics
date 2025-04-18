using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Globalization;
using System.Reflection;

namespace Szeminarium1_24_02_17_2
{
    internal class ObjResourceReader
    {

        public static unsafe GlObject CreateColoredObject(GL Gl, float[] faceColor)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            List<float[]> objVertices;
            List<int[]> objFaces;
            List<float[]> normals;
            List<int[]> normalPlaces;

            ReadObjData(out objVertices, out objFaces, out normals, out normalPlaces);

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            CreateGlArraysFromObjArrays(faceColor, objVertices, objFaces, normalPlaces, normals, glVertices, glColors, glIndices);

            return CreateOpenGlObject(Gl, vao, glVertices, glColors, glIndices);
        }

        private static unsafe GlObject CreateOpenGlObject(GL Gl, uint vao, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint vertexSize = offsetNormal + (3 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glVertices.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glColors.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)glIndices.ToArray().AsSpan(), GLEnum.StaticDraw);

            // release array buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)glIndices.Count;

            return new GlObject(vao, vertices, colors, indices, indexArrayLength, Gl);
        }

        private static unsafe void CreateGlArraysFromObjArrays(float[] faceColor, List<float[]> objVertices, List<int[]> objFaces, List<int[]> normalPlaces, List<float[]> objNormals, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            Dictionary<string, int> glVertexIndices = new Dictionary<string, int>();

            bool existNormal = (objNormals.Count > 0);
            
            Console.WriteLine(existNormal);

            for (int faceIndex = 0; faceIndex < objFaces.Count; faceIndex++)
            {
                var objFace = objFaces[faceIndex];
                var normalIndicesForFace = normalPlaces[faceIndex];
                var normal = new Vector3D<float>();

                if (!existNormal || normalIndicesForFace == null || normalIndicesForFace.Length != objFace.Length)
                {
                    // Számoljuk a felületi normált
                    if (objFace.Length >= 3)
                    {
                        var aObjVertex = objVertices[objFace[0] - 1];
                        var a = new Vector3D<float>(aObjVertex[0], aObjVertex[1], aObjVertex[2]);
                        var bObjVertex = objVertices[objFace[1] - 1];
                        var b = new Vector3D<float>(bObjVertex[0], bObjVertex[1], bObjVertex[2]);
                        var cObjVertex = objVertices[objFace[2] - 1];
                        var c = new Vector3D<float>(cObjVertex[0], cObjVertex[1], cObjVertex[2]);
                        normal = Vector3D.Normalize(Vector3D.Cross(b - a, c - a));
                    }
                }

                // process vertices
                for (int i = 0; i < objFace.Length; ++i)
                {
                    var objVertex = objVertices[objFace[i] - 1];

                    Vector3D<float> currentNormal = normal;

                    if (existNormal && normalIndicesForFace != null && normalIndicesForFace.Length > i)
                    {
                        int normalIndex = normalIndicesForFace[i];
                        if (normalIndex > 0 && normalIndex <= objNormals.Count)
                        {
                            currentNormal = new Vector3D<float>(
                                objNormals[normalIndex - 1][0],
                                objNormals[normalIndex - 1][1],
                                objNormals[normalIndex - 1][2]
                            );
                        }
                    }

                    // create gl description of vertex
                    List<float> glVertex = new List<float>();
                    glVertex.AddRange(objVertex);
                    glVertex.Add(currentNormal.X);
                    glVertex.Add(currentNormal.Y);
                    glVertex.Add(currentNormal.Z);
                    // add textrure, color

                    // check if vertex exists
                    var glVertexStringKey = string.Join(" ", glVertex);
                    if (!glVertexIndices.ContainsKey(glVertexStringKey))
                    {
                        glVertices.AddRange(glVertex);
                        glColors.AddRange(faceColor);
                        glVertexIndices.Add(glVertexStringKey, glVertexIndices.Count);
                    }

                    // add vertex to triangle indices
                    glIndices.Add((uint)glVertexIndices[glVertexStringKey]);
                }
            }
        }

        private static unsafe void ReadObjData(out List<float[]> objVertices, out List<int[]> objFaces, out List<float[]> normVectors, out List<int[]> normalPlaces)
        {
            objVertices = new List<float[]>();
            objFaces = new List<int[]>();
            normVectors = new List<float[]>();
            normalPlaces = new List<int[]>();

            var assembly = typeof(ObjResourceReader).Assembly;
            string[] resourceNames = assembly.GetManifestResourceNames();


            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream("Szeminarium1_24_02_17_2.Resources.minicooper.obj"))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();

                    if (String.IsNullOrEmpty(line) || line.Trim().StartsWith("#"))
                        continue;

                   
                    if (!(line.StartsWith("v") || line.StartsWith("vn") || line.StartsWith("f")))
                        continue;
                    
                    var lineClassifier = line.Substring(0, line.IndexOf(' '));
                    var lineData = line.Substring(lineClassifier.Length).Trim().Split(' ');

                    switch (lineClassifier)
                    {
                        case "v":
                            float[] vertex = new float[3];
                            for (int i = 0; i < vertex.Length; ++i)
                                vertex[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objVertices.Add(vertex);
                            break;

                        case "vn":
                            float[] nVector = new float[3];
                            for (int i = 0; i < 3; ++i)
                                nVector[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            normVectors.Add(nVector);
                            break;

                        case "f":
                            int[] face = new int[3];
                            int[] normal = new int[3];

                            for (int i = 0; i < 3; i++)
                            {
                                string[] parts = lineData[i].Split('/');
                                face[i] = int.Parse(parts[0]);
                                normal[i] = int.Parse(parts[2]);
                            }
                            objFaces.Add(face);
                            normalPlaces.Add(normal);
                            break;

                        default: continue;
                    }
                }
            }
        }
    }
}
