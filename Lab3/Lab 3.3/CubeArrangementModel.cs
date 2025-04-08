using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;

namespace Szeminarium
{
    internal class CubeArrangementModel
    {

        public bool Animating { get; set; } = false;

        // rotation sizes
        public static float CurrentRotationAngle = 0.0f;
        public const float TargetRotationAngle = (float)Math.PI / 2f;
        public static float ActualRotationAngle = 0.0f;

        // rotation speed
        public float RotationSpeed = (float)Math.PI / 2f;
        public float RandomRotationSpeed = (float)Math.PI * 2;

        // rotation angle and direction
        public static String RotationType { get; set; }
        public static int RotationDirection;

        // Animation types to randomize
        private Random randomGenerator = new Random();
        public int randomCounter = 0;
        public bool Random { get; set; } = false;
        public static readonly string[] Animations =
        {
            "R-VERTICAL",
            "R-VERTICAL",
            "M-VERTICAL",
            "M-VERTICAL",
            "L-VERTICAL",
            "L-VERTICAL",
            "T-HORIZONTAL",
            "T-HORIZONTAL",
            "M-HORIZONTAL",
            "M-HORIZONTAL",
            "B-HORIZONTAL",
            "B-HORIZONTAL",
            "FRONT",
            "FRONT",
            "MIDDLE",
            "MIDDLE",
            "BACK",
            "BACK"
        };


        // pulze settings
        public bool Pulzing { get; set; } = false;
        public static float scaleFactor = 1.0f;
        public double time = 0;
        public int pulzeCounter = 0;

        internal void AdvanceTime(double deltaTime, ref RubikCubeArrangementModel rubikCube)
        {
            // ha pul
            if (Pulzing)
            {
                if (pulzeCounter < 370)
                {
                    //Console.WriteLine("Pulze counter: " + pulzeCounter);
                    time += deltaTime * 3;
                    scaleFactor = 1.0f + (float)Math.Sin(time) * 0.009f;

                    pulzeCounter++;
                    scaleCube(ref rubikCube);
                }
                else
                {
                    for (int i = 0; i < rubikCube.Cubes.Count; i++)
                    {
                        rubikCube.Transformations[i] = rubikCube.OriginalTransformations[i];
                    }
                    Pulzing = false;
                    pulzeCounter = 0;
                }

                return;
            }

            if (!Animating)
            {
                return;
            }


            float speed = Random ? RandomRotationSpeed : RotationSpeed;
            ActualRotationAngle = speed * (float)deltaTime;

            // egy forgatason tul, vagyis leallas
            if (CurrentRotationAngle + ActualRotationAngle >= TargetRotationAngle)
            {

                Console.WriteLine("ROTATED! STOP");
                ActualRotationAngle = TargetRotationAngle - CurrentRotationAngle;
                CurrentRotationAngle = 0;
                rotateRubikCube(ref rubikCube, true);
                Animating = false;

                // random case
                if (Random)
                {
                    randomCounter++;
                    RandomizeCube();
                    Console.WriteLine("Random round: " + randomCounter);
                    if (randomCounter == 30) Random = false;
                    else Animating = true;
                }

                if (rubikCube.isSolved())
                {
                    Pulzing = true;
                }

            }
            else
            {
                CurrentRotationAngle += ActualRotationAngle;
                rotateRubikCube(ref rubikCube, false);
            }


        }

        public void setRandom(bool rand)
        {
            Random = rand;
            randomCounter = 0;
            Animating = true;
            RandomizeCube();

        }

        public void RandomizeCube()
        {
            int randIdx = randomGenerator.Next(Animations.Length);
            RotationDirection = randIdx % 2 == 0 ? 1 : 0;
            RotationType = Animations[randIdx];
        }

        public void enableAnimation(String type, int direction)
        {
            Animating = true;
            RotationType = type;
            RotationDirection = direction;
        }

        public void setRandomizedCube(bool value)
        {
            Random = value;
        }

        private static void rotateRubikCube(ref RubikCubeArrangementModel rubikCube, bool changePosition)
        {
            float target = RotationDirection == 1 ? ActualRotationAngle : ActualRotationAngle * -1f;
            int priorityIdx = 0;
            int priorityPos = 0;
            char axis = 'Y';

            switch (RotationType)
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

            // transzformalunk
            for (int i = 0; i < rubikCube.Cubes.Count; i++)
            {

                if (rubikCube.LogicalPositions[i][priorityIdx] == priorityPos)
                {
                    rubikCube.Transformations[i] = rubikCube.Transformations[i] * rotation;
                }
            }

            if (changePosition)
            {
                for (int i = 0; i < rubikCube.Cubes.Count; i++)
                {

                    if (rubikCube.LogicalPositions[i][priorityIdx] == priorityPos)
                    {
                        rubikCube.LogicalPositions[i] = ChangeLogicalPosition(rubikCube.LogicalPositions[i], axis, RotationDirection);
                    }
                }
            }
        }

        private static float[] ChangeLogicalPosition(float[] position, char axis, int direction)
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

            float[] res = { newX, newY, newZ };
            return res;
        }

        private static void scaleCube(ref RubikCubeArrangementModel rubikCube)
        {
            Matrix4X4<float> scaleMatrix = Matrix4X4.CreateScale(scaleFactor, scaleFactor, scaleFactor);
            for (int i = 0; i < rubikCube.Cubes.Count; i++)
            {
                rubikCube.Transformations[i] = rubikCube.Transformations[i] * scaleMatrix;
            }
        }
    }
}
