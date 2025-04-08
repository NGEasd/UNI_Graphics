﻿using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Dynamic;
using System.Numerics;
using System.Reflection;
using Szeminarium;

namespace GrafikaSzeminarium
{
    internal class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static ImGuiController imGuiController;

        private static RubikCubeArrangementModel rubikCube;

        private static CameraDescriptor camera = new CameraDescriptor();

        private static CubeArrangementModel cubeArrangementModel = new CubeArrangementModel();


        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private const string LightColorVariableName = "uLightColor";
        private const string LightPositionVariableName = "uLightPos";
        private const string ViewPositionVariableName = "uViewPos";

        private const string ShinenessVariableName = "uShininess";
        private const string AmbientVariableName = "uAmbientStrength";
        private const string DiffuseVariableName = "uDiffuseStrength";
        private const string SpecularVariableName = "uSpecularStrength";

        private static float shininess = 50;
        private static Vector3 ambientStrength = new Vector3(0.1f, 0.1f, 0.1f);
        private static Vector3 diffuseStrength = new Vector3(0.3f, 0.3f, 0.3f);
        private static Vector3 specularStrength = new Vector3(0.6f, 0.6f, 0.6f);

        private static float lightPosX = 0f;
        private static float lightPosY = 1.5f;
        private static float lightPosZ = 0f;

        private static uint program;

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Lab 3.3";
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

            // Handle resizes
            graphicWindow.FramebufferResize += s =>
            {
                // Adjust the viewport to the new window size
                Gl.Viewport(s);
            };



            imGuiController = new ImGuiController(Gl, graphicWindow, inputContext);
            rubikCube = new RubikCubeArrangementModel(Gl);

            Gl.ClearColor(System.Drawing.Color.White);
            
            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(TriangleFace.Back);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);


            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, GetEmbeddedResourceAsString("Shaders.VertexShader.vert"));
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, GetEmbeddedResourceAsString("Shaders.FragmentShader.frag"));
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

        private static string GetEmbeddedResourceAsString(string resourceRelativePath)
        {
            string resourceFullPath = Assembly.GetExecutingAssembly().GetName().Name + "." + resourceRelativePath;

            using (var resStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceFullPath))
            using (var resStreamReader = new StreamReader(resStream))
            {
                var text = resStreamReader.ReadToEnd();
                return text;
            }
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.W:
                    camera.MoveForward();
                    break;
                case Key.S:
                    camera.MoveBackward();
                    break;
                case Key.A:
                    camera.MoveLeft();
                    break;
                case Key.D:
                    camera.MoveRight();
                    break;
                case Key.Space:
                    camera.MoveUp();
                    break;
                case Key.ShiftLeft:
                    camera.MoveDown();
                    break;

                case Key.Left:
                    camera.Rotate(-5, 0);
                    break;
                case Key.Right:
                    camera.Rotate(5, 0);
                    break;
                case Key.Up:
                    camera.Rotate(0, 5);
                    break;
                case Key.Down:
                    camera.Rotate(0, -5);
                    break;
            }
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO OpenGL
            // make it threadsafe
            cubeArrangementModel.AdvanceTime(deltaTime, ref rubikCube);
            imGuiController.Update((float)deltaTime);
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            SetUniform3(LightColorVariableName, new Vector3(1f, 1f, 1f));
            SetUniform3(LightPositionVariableName, new Vector3(lightPosX, lightPosY, lightPosZ));
            SetUniform3(ViewPositionVariableName, new Vector3(camera.Position.X, camera.Position.Y, camera.Position.Z));
            SetUniform1(ShinenessVariableName, shininess);

            SetUniform3(AmbientVariableName, ambientStrength);
            SetUniform3(DiffuseVariableName, diffuseStrength);
            SetUniform3(SpecularVariableName, specularStrength);

            var viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Position + camera.ForwardVector, camera.UpVector);
            SetMatrix(viewMatrix, ViewMatrixVariableName);

            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)(Math.PI / 2), 1024f / 768f, 0.1f, 100f);
            SetMatrix(projectionMatrix, ProjectionMatrixVariableName);


            for (int i = 0; i < rubikCube.Cubes.Count; i++)
            {
                var cube = rubikCube.Cubes[i];
                var transform = rubikCube.Transformations[i];

                SetModelMatrix(transform);
                DrawModelObject(cube);
            }

            //ImGuiNET.ImGui.ShowDemoWindow();
            ImGuiNET.ImGui.Begin("Lighting", ImGuiNET.ImGuiWindowFlags.AlwaysAutoResize | ImGuiNET.ImGuiWindowFlags.NoCollapse);
            ImGuiNET.ImGui.SliderFloat("Shininess", ref shininess, 5, 100);
            ImGui.SliderFloat3("Ambient Strength", ref ambientStrength, 0.0f, 1.0f);
            ImGui.SliderFloat3("Diffuse Strength", ref diffuseStrength, 0.0f, 1.0f);
            ImGui.SliderFloat3("Specular Strength", ref specularStrength, 0.0f, 1.0f);

            // kamera pozicionalasa
            ImGui.Begin("Light position");
            ImGui.InputFloat("X", ref lightPosX);
            ImGui.InputFloat("Y", ref lightPosY);
            ImGui.InputFloat("Z", ref lightPosZ);
            ImGui.End();

            // forgatasi gombok
            ImGui.Begin("Rotate: X");
            // X - Angle
            if (ImGui.Button("Forward: RIGHT"))
            {
                cubeArrangementModel.enableAnimation("R-VERTICAL", 1);
            }
            if (ImGui.Button("Backward:MIDDLE"))
            {
                cubeArrangementModel.enableAnimation("R-VERTICAL", 0);
            }

            if (ImGui.Button("Forward: MIDDLE"))
            {
                cubeArrangementModel.enableAnimation("M-VERTICAL", 1);
            }
            if (ImGui.Button("Backward:RIGHT"))
            {
                cubeArrangementModel.enableAnimation("M-VERTICAL", 0);
            }

            if (ImGui.Button("Forward: LEFT"))
            {
                cubeArrangementModel.enableAnimation("L-VERTICAL", 1);
            }
            if (ImGui.Button("Backward: LEFT"))
            {
                cubeArrangementModel.enableAnimation("L-VERTICAL", 0);
            }
            ImGui.End();

            ImGui.Begin("Rotate: Y");
            if (ImGui.Button("Forward: TOP"))
            {
                cubeArrangementModel.enableAnimation("T-HORIZONTAL", 1);
            }
            if (ImGui.Button("Backward: TOP"))
            {
                cubeArrangementModel.enableAnimation("T-HORIZONTAL", 0);
            }

            if (ImGui.Button("Forward: MIDDLE"))
            {
                cubeArrangementModel.enableAnimation("M-HORIZONTAL", 1);
            }
            if (ImGui.Button("Backward: MIDDLE"))
            {
                cubeArrangementModel.enableAnimation("M-HORIZONTAL", 0);
            }

            if (ImGui.Button("Forward: BOTTOM"))
            {
                cubeArrangementModel.enableAnimation("B-HORIZONTAL", 1);
            }
            if (ImGui.Button("Backward: BOTTOM"))
            {
                cubeArrangementModel.enableAnimation("B-HORIZONTAL", 0);
            }
            ImGui.End();

            ImGui.Begin("Rotate: Z");
            if (ImGui.Button("Forward: FRONT"))
            {
                cubeArrangementModel.enableAnimation("FRONT", 1);
            }
            if (ImGui.Button("Backward: FRONT"))
            {
                cubeArrangementModel.enableAnimation("FRONT", 0);
            }

            if (ImGui.Button("Forward: MIDDLE"))
            {
                cubeArrangementModel.enableAnimation("MIDDLE", 1);
            }
            if (ImGui.Button("Backward: MIDDLE"))
            {
                cubeArrangementModel.enableAnimation("MIDDLE", 0);
            }

            if (ImGui.Button("Forward: BACK"))
            {
                cubeArrangementModel.enableAnimation("BACK", 1);
            }
            if (ImGui.Button("Backward: BACK"))
            {
                cubeArrangementModel.enableAnimation("BACK", 0);
            }
            ImGui.End();

            ImGui.Begin("30 RANDOM MOVE!");
            // X - Angle
            if (ImGui.Button("RANDOM"))
            {
                cubeArrangementModel.setRandom(true);
            }
            ImGui.End();

            imGuiController.Render();
        }

        private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            SetMatrix(modelMatrix, ModelMatrixVariableName);

            // set also the normal matrix
            int location = Gl.GetUniformLocation(program, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }

            // G = (M^-1)^T
            var modelMatrixWithoutTranslation = new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);
            modelMatrixWithoutTranslation.M41 = 0;
            modelMatrixWithoutTranslation.M42 = 0;
            modelMatrixWithoutTranslation.M43 = 0;
            modelMatrixWithoutTranslation.M44 = 1;

            Matrix4X4<float> modelInvers;
            Matrix4X4.Invert<float>(modelMatrixWithoutTranslation, out modelInvers);
            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));

            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
        }

        private static unsafe void SetUniform1(string uniformName, float uniformValue)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{uniformName} uniform not found on shader.");
            }

            Gl.Uniform1(location, uniformValue);
            CheckError();
        }

        private static unsafe void SetUniform3(string uniformName, Vector3 uniformValue)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{uniformName} uniform not found on shader.");
            }

            Gl.Uniform3(location, uniformValue);
            CheckError();
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
                throw new Exception($"{uniformName} uniform not found on shader.");
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