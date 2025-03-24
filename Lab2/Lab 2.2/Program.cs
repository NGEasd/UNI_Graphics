using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
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

        private static CubeArrangementModel cubeArrangementModel = new CubeArrangementModel();
        private static bool initialize = false;

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

            }

            // forgatasi logika
            if (!cubeArrangementModel.Animating)
            {
                switch (key)
                {
                    
                    // fuggoleges
                    // R -> jobb oldal elore
                    case Key.Q:
                        cubeArrangementModel.enableAnimation("R-VERTICAL", 1);
                        break;

                    // T -> jobb oldal hatra
                    case Key.W:
                        cubeArrangementModel.enableAnimation("R-VERTICAL", 0);
                        break;

                    // E -> kozep elore
                    case Key.E:
                        cubeArrangementModel.enableAnimation("M-VERTICAL", 1);
                        break;

                    // R -> kozep hatra
                    case Key.R:
                        cubeArrangementModel.enableAnimation("M-VERTICAL", 0);
                        break;

                    // T -> bal elore
                    case Key.T:
                        cubeArrangementModel.enableAnimation("L-VERTICAL", 1);
                        break;

                    // Y -> bal hatra
                    case Key.Y:
                        cubeArrangementModel.enableAnimation("L-VERTICAL", 0);
                        break;

                    // vizszintes
                    case Key.F:
                        cubeArrangementModel.enableAnimation("T-HORIZONTAL", 1);
                        break;

                    case Key.G:
                        cubeArrangementModel.enableAnimation("T-HORIZONTAL", 0);
                        break;

                    case Key.H:
                        cubeArrangementModel.enableAnimation("M-HORIZONTAL", 1);
                        break;

                    case Key.J:
                        cubeArrangementModel.enableAnimation("M-HORIZONTAL", 0);
                        break;

                    case Key.K:
                        cubeArrangementModel.enableAnimation("B-HORIZONTAL", 1);
                        break;

                    case Key.L:
                        cubeArrangementModel.enableAnimation("B-HORIZONTAL", 0);
                        break;

                    // elulso oldalak
                    case Key.Z:
                        cubeArrangementModel.enableAnimation("FRONT", 1);
                        break;

                    case Key.X:
                        cubeArrangementModel.enableAnimation("FRONT", 0);
                        break;

                    case Key.C:
                        cubeArrangementModel.enableAnimation("MIDDLE", 1);
                        break;

                    case Key.V:
                        cubeArrangementModel.enableAnimation("MIDDLE", 0);
                        break;

                    case Key.B:
                        cubeArrangementModel.enableAnimation("BACK", 1);
                        break;

                    case Key.N:
                        cubeArrangementModel.enableAnimation("BACK", 0);
                        break;
                }
            }
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO OpenGL
            // make it threadsafe
            cubeArrangementModel.AdvanceTime(deltaTime, ref rubikCube);
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
    }


}