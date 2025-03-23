using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using Szeminarium;

namespace GrafikaSzeminarium
{
    internal class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        //private static ModelObjectDescriptor cube;
        private static RubikCubeArrangementModel rubikCube;

        private static CameraDescriptor camera = new CameraDescriptor();

        private static CubeArrangementModel cubeArrangementModel;

        private const string ModelMatrixVariableName = "uModel";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

        uniform mat4 uModel;
        uniform mat4 uView;
        uniform mat4 uProjection;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = uProjection*uView*uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0);
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

        private static uint program;

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Labor 2_01";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;
            graphicWindow.Closing += GraphicWindow_Closing;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Closing()
        {
            rubikCube.Dispose();
            Gl.DeleteProgram(program);
        }

        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            var inputContext = graphicWindow.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            // rubik kocka inicializalas
            rubikCube = new RubikCubeArrangementModel(Gl);
            if (rubikCube == null)
            {
                Console.WriteLine("Nagy hiba: nem sikerult peldanyositani!");
            }

            Gl.ClearColor(System.Drawing.Color.White);
            
            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(TriangleFace.Back);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);


            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);
            Gl.GetShader(fshader, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int)GLEnum.True)
                throw new Exception("Fragment shader failed to compile: " + Gl.GetShaderInfoLog(fshader));

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);

            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
            if ((ErrorCode)Gl.GetError() != ErrorCode.NoError)
            {

            }

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                // kamera logika
                case Key.Left:
                    camera.DecreaseZYAngle();
                    break;
                case Key.Right:
                    camera.IncreaseZYAngle();
                    break;
                case Key.Down:
                    camera.IncreaseDistance();
                    break;
                case Key.Up:
                    camera.DecreaseDistance();
                    break;
                case Key.U:
                    camera.IncreaseZXAngle();
                    break;
                case Key.D:
                    camera.DecreaseZXAngle();
                    break;

                // forgatasi logika

                // fuggoleges
                // R -> jobb oldal elore
                case Key.Q:
                    rubikCube.rotating = true;
                    rotateRubikCube("R-VERTICAL", 1);
                    break;

                // T -> jobb oldal hatra
                case Key.W:
                    rubikCube.rotating = true;
                    rotateRubikCube("R-VERTICAL", 0);
                    break;

                // E -> kozep elore
                case Key.E:
                    rubikCube.rotating = true;
                    rotateRubikCube("M-VERTICAL", 1);
                    break;

                // R -> kozep hatra
                case Key.R:
                    rubikCube.rotating = true;
                    rotateRubikCube("M-VERTICAL", 0);
                    break;

                // T -> bal elore
                case Key.T:
                    rubikCube.rotating = true;
                    rotateRubikCube("L-VERTICAL", 1);
                    break;

                // Y -> bal hatra
                case Key.Y:
                    rubikCube.rotating = true;
                    rotateRubikCube("L-VERTICAL", 0);
                    break;

                // vizszintes
                case Key.F:
                    rubikCube.rotating = true;
                    rotateRubikCube("T-HORIZONTAL", 1);
                    break;

                case Key.G:
                    rubikCube.rotating = true;
                    rotateRubikCube("T-HORIZONTAL", 0);
                    break;

                case Key.H:
                    rubikCube.rotating = true;
                    rotateRubikCube("M-HORIZONTAL", 1);
                    break;

                case Key.J:
                    rubikCube.rotating = true;
                    rotateRubikCube("M-HORIZONTAL", 0);
                    break;

                case Key.K:
                    rubikCube.rotating = true;
                    rotateRubikCube("B-HORIZONTAL", 1);
                    break;

                case Key.L:
                    rubikCube.rotating = true;
                    rotateRubikCube("B-HORIZONTAL", 0);
                    break;

                // elulso oldalak
                case Key.Z:
                    rubikCube.rotating = true;
                    rotateRubikCube("FRONT", 1);
                    break;

                case Key.X:
                    rubikCube.rotating = true;
                    rotateRubikCube("FRONT", 0);
                    break;

                case Key.C:
                    rubikCube.rotating = true;
                    rotateRubikCube("MIDDLE", 1);
                    break;

                case Key.V:
                    rubikCube.rotating = true;
                    rotateRubikCube("MIDDLE", 0);
                    break;

                case Key.B:
                    rubikCube.rotating = true;
                    rotateRubikCube("BACK", 1);
                    break;

                case Key.N:
                    rubikCube.rotating = true;
                    rotateRubikCube("BACK", 0);
                    break;
            }
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO OpenGL
            // make it threadsafe
            // cubeArrangementModel.AdvanceTime(deltaTime);
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            // kamera
            var viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Target, camera.UpVector);
            SetMatrix(viewMatrix, ViewMatrixVariableName);

            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)(Math.PI / 2), 1024f / 768f, 0.1f, 100f);
            SetMatrix(projectionMatrix, ProjectionMatrixVariableName);

            // rubik kocka
            for (int i = 0; i < rubikCube.Cubes.Count; i++)
            {
                var cube = rubikCube.Cubes[i];
                var transform = rubikCube.Transformations[i];

                SetMatrix(transform, "uModel");
                DrawModelObject(cube);
            }
        }

        private static unsafe void DrawModelObject(ModelObjectDescriptor modelObject)
        {
            Gl.BindVertexArray(modelObject.Vao);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, modelObject.Indices);
            Gl.DrawElements(PrimitiveType.Triangles, modelObject.IndexArrayLength, DrawElementsType.UnsignedInt, null);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(0);
        }

        private static unsafe void SetMatrix(Matrix4X4<float> mx, string uniformName)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&mx);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }

        private static void rotateRubikCube(String type, uint direction)
        {
            float target = direction == 1 ? (float)Math.PI / 2f : (float)Math.PI / 2f * -1f;
            int priorityIdx = 0;
            int priorityPos = 0;
            char axis = 'Y';

            switch (type)
            {
                // X tengely korul: fuggoleges
                case "R-VERTICAL": priorityIdx = 0; priorityPos = 1; axis = 'X'; break;
                case "M-VERTICAL": priorityIdx = 0; priorityPos = 0; axis = 'X'; break;
                case "L-VERTICAL": priorityIdx = 0; priorityPos = -1; axis = 'X'; break;

                // Y tengely korul: vizszintes
                case "T-HORIZONTAL": priorityIdx = 1; priorityPos = 1; axis = 'Y'; break;
                case "M-HORIZONTAL": priorityIdx = 1; priorityPos = 0; axis = 'Y'; break;
                case "B-HORIZONTAL": priorityIdx = 1; priorityPos = -1; axis = 'Y'; break;

                // Z tengely korul: elulso
                case "FRONT": priorityIdx = 2; priorityPos = 1; axis = 'Z'; break;
                case "MIDDLE": priorityIdx = 2; priorityPos = 0; axis = 'Z'; break;
                case "BACK": priorityIdx = 2; priorityPos = -1; axis = 'Z'; break;

                default:
                    return;
            }

            // forgatasi matrix felepitese
            Matrix4X4<float> rotation = axis switch
            {
                'X' => Matrix4X4.CreateRotationX(target),
                'Y' => Matrix4X4.CreateRotationY(target),
                'Z' => Matrix4X4.CreateRotationZ(target)
            };

            // transzformalunk + beallitjuk az uj poziciot
           var oldLogicalPositions = rubikCube.LogicalPositions.Select(pos => (float[])pos.Clone()).ToList();

            for (int i = 0; i < rubikCube.Cubes.Count; i++)
            {
                var logicalPosition = oldLogicalPositions[i];

                if (logicalPosition[priorityIdx] == priorityPos)
                {
                    // forgatunk es poziciot valtunk
                    rubikCube.Transformations[i] = rubikCube.Transformations[i] * rotation;
                    rubikCube.LogicalPositions[i] = ChangeLogicalPosition(logicalPosition, axis, direction);
                }
            }

            for (int i = 0; i < rubikCube.Cubes.Count; i++)
            {
                Console.Write(oldLogicalPositions[i][0] + " " + oldLogicalPositions[i][1] + " " + oldLogicalPositions[i][2] + " =>> ");
                Console.WriteLine(rubikCube.LogicalPositions[i][0] + " " + rubikCube.LogicalPositions[i][1] + " " + rubikCube.LogicalPositions[i][2]);
            }
            Console.WriteLine("\n________---------_______---------________\n");



        }

        private static float[] ChangeLogicalPosition(float[] position, char axis, uint direction)
        {
            float x = position[0], y = position[1], z = position[2];
            float newX = x, newY = y, newZ = z;
            float sign = direction == 1 ? 1f : -1f;

            switch (axis)
            {
                case 'X': // X tengely 
                    newY = -sign * z;
                    newZ = sign * y;
                    break;

                case 'Y': // Y tengely
                    newX = sign * z;
                    newZ = -sign * x;
                    break;

                case 'Z': // Z tengely 
                    newX = -sign * y;
                    newY = sign * x;
                    break;
            }

            float[] res = {newX, newY, newZ};
            return res;
        }
    }


}