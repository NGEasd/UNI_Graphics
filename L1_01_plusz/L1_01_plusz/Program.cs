using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Szeminarium1
{
    internal static class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static uint program;

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);

        }
        ";


        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
		
		in vec4 outCol;

        void main()
        {
            FragColor = outCol;
        }
        ";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Lab1 - 01: Teszt";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Load()
        {
            // egszeri beallitasokat
            //Console.WriteLine("Loaded");

            Gl = graphicWindow.CreateOpenGL();

            Gl.ClearColor(System.Drawing.Color.White);

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }

        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO GL
            // make it threadsave
            //Console.WriteLine($"Update after {deltaTime} [s]");
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s]");

            Gl.Clear(ClearBufferMask.ColorBufferBit);

            uint cubeVao = Gl.GenVertexArray();
            uint linesVao = Gl.GenVertexArray();
            Gl.BindVertexArray(cubeVao);

            float[] vertexArray = [

                
                // "atskalazzuk" -> konnyebb szamitas

                // elso oldal
                // H1
                -0.3f, 0.0f, 0.0f,
                0.3f, 0.0f, 0.0f,
                0.3f, 0.6f, 0.0f,  
        
                // H2
                -0.3f, 0.0f, 0.0f,
                0.3f, 0.6f, 0.0f,
                -0.3f, 0.6f, 0.0f, 

                // masodik oldal
                // H3
                0.3f, 0.0f, 0.0f,
                0.6f, 0.3f, 0.0f,
                0.6f, 0.9f, 0.0f,  
        
                // H4
                0.3f, 0.0f, 0.0f,
                0.6f, 0.9f, 0.0f,
                0.3f, 0.6f, 0.0f,  

                // harmadik oldal
                // H5
                -0.3f, 0.6f, 0.0f,
                0.0f, 0.9f, 0.0f,
                0.3f, 0.6f, 0.0f,  
        
                // H6
                0.3f, 0.6f, 0.0f,
                0.6f, 0.9f, 0.0f,
                0.0f, 0.9f, 0.0f
            ];

            float[] colorArray = [

                // H1
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,

                // H2
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,

                // H3
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,

                // H4
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,

                // H5
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,

                // H6
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,

            ];

            uint[] indexArray = [
                0, 1, 2,
                3, 4, 5,
                6, 7, 8,
                9, 10, 11,
                12, 13, 14,
                15, 16, 17
            ];


            // vonalak
            float[] lineVertexArray = new float[1444];
            uint[] lineIndexArray = new uint[72];
            float[] blackColorArray =
            {
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,

                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,

                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,

                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,

                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,

                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,

                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,

                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,

                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,

                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,

                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,

                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f,
                 0.0f, 0.0f, 0.0f, 1.0f

            };


            uint lineIndex = 0;
            uint triangleIndex = 0;

            // "kirajzolas"
            // 1. oldal
            writeHorizontalPoints(ref lineIndex, [-0.3f, 0.2f, 0.6f], ref lineVertexArray, 0.0f);
            writeHorizontalPoints(ref lineIndex, [-0.3f, 0.4f, 0.6f], ref lineVertexArray, 0.0f);

            writeVerticalPoints(ref lineIndex, [-0.1f, 0.0f, 0.6f], ref lineVertexArray, 0.0f);
            writeVerticalPoints(ref lineIndex, [0.1f, 0.0f, 0.6f], ref lineVertexArray, 0.0f);

            markTriangle(ref triangleIndex, 0, ref lineIndexArray);
            markTriangle(ref triangleIndex, 4, ref lineIndexArray);
            markTriangle(ref triangleIndex, 8, ref lineIndexArray);
            markTriangle(ref triangleIndex, 12, ref lineIndexArray);

            // 2. oldal
            writeHorizontalPoints(ref lineIndex, [0.3f, 0.2f, 0.3f], ref lineVertexArray, 0.3f);
            writeHorizontalPoints(ref lineIndex, [0.3f, 0.4f, 0.3f], ref lineVertexArray, 0.3f);

            writeVerticalPoints(ref lineIndex, [0.4f, 0.1f, 0.6f], ref lineVertexArray, 0.0f);
            writeVerticalPoints(ref lineIndex, [0.5f, 0.2f, 0.6f], ref lineVertexArray, 0.0f);

            markTriangle(ref triangleIndex, 16, ref lineIndexArray);
            markTriangle(ref triangleIndex, 20, ref lineIndexArray);
            markTriangle(ref triangleIndex, 24, ref lineIndexArray);
            markTriangle(ref triangleIndex, 28, ref lineIndexArray);

            // 3. oldal
            writeHorizontalPoints(ref lineIndex, [-0.2f, 0.7f, 0.6f], ref lineVertexArray, 0.0f);
            writeHorizontalPoints(ref lineIndex, [-0.1f, 0.8f, 0.6f], ref lineVertexArray, 0.0f);

            writeVerticalPoints(ref lineIndex, [-0.1f, 0.6f, 0.3f], ref lineVertexArray, 0.3f);
            writeVerticalPoints(ref lineIndex, [0.1f, 0.6f, 0.3f], ref lineVertexArray, 0.3f);

            markTriangle(ref triangleIndex, 32, ref lineIndexArray);
            markTriangle(ref triangleIndex, 36, ref lineIndexArray);
            markTriangle(ref triangleIndex, 40, ref lineIndexArray);
            markTriangle(ref triangleIndex, 44, ref lineIndexArray);


            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            Gl.UseProgram(program);

            Gl.DrawElements(GLEnum.Triangles, (uint)indexArray.Length, GLEnum.UnsignedInt, null);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(cubeVao);

            // lines
            uint lines = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, lines);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)lineVertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            uint lineColors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, lineColors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)blackColorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint lineIndices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, lineIndices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)lineIndexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            Gl.UseProgram(program);

            Gl.DrawElements(GLEnum.Triangles, (uint)lineIndexArray.Length, GLEnum.UnsignedInt, null);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(linesVao);

            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);

            Gl.DeleteBuffer(lines);
            Gl.DeleteBuffer(lineColors);
            Gl.DeleteBuffer(lineIndices);

            Gl.DeleteVertexArray(cubeVao);
            Gl.DeleteVertexArray(linesVao);
        }

        private static void writeHorizontalPoints(ref uint index, float[] coordinates, ref float[] location, float offset)
        {
            // coordinates
            // [st_x, st_y, dist]
            location[index++] = coordinates[0];
            location[index++] = coordinates[1] - 0.01f;
            location[index++] = 0.0f;

            location[index++] = coordinates[0];
            location[index++] = coordinates[1] + 0.01f;
            location[index++] = 0.0f;

            location[index++] = coordinates[0] + coordinates[2];
            location[index++] = coordinates[1] + offset +0.01f;
            location[index++] = 0.0f;

            location[index++] = coordinates[0] + coordinates[2];
            location[index++] = coordinates[1] + offset - 0.01f;
            location[index++] = 0.0f;
        }

        private static void writeVerticalPoints(ref uint index, float[] coordinates, ref float[] location, float offset)
        {
            // coordinates
            // [st_x, st_y, dist]
            location[index++] = coordinates[0] - 0.01f;
            location[index++] = coordinates[1];
            location[index++] = 0.0f;

            location[index++] = coordinates[0] + offset - 0.01f;
            location[index++] = coordinates[1] + coordinates[2];
            location[index++] = 0.0f;

            location[index++] = coordinates[0] + offset + 0.01f;
            location[index++] = coordinates[1] + coordinates[2];
            location[index++] = 0.0f;

            location[index++] = coordinates[0] + 0.01f;
            location[index++] = coordinates[1];
            location[index++] = 0.0f;
        }

        private static void markTriangle(ref uint i, uint vertex, ref uint[] indexes)
        {
            indexes[i++] = vertex;
            indexes[i++] = vertex + 1;
            indexes[i++] = vertex + 2;

            indexes[i++] = vertex;
            indexes[i++] = vertex + 3;
            indexes[i++] = vertex + 2;
        }
    }
    
}
