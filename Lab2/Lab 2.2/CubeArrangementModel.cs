using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Szeminarium
{
    internal class CubeArrangementModel
    {

        public bool Animating { get; set; } = false;
        public bool Rotating { get; set; } = false;
        public static float CurrentRotationAngle = 0.0f;
        public const float TargetRotationAngle = (float)Math.PI / 2f;
        public static float ActualRotationAngle = 0.0f;
        public const float RotationSpeed = (float)Math.PI / 2f;

        // Rotation angle and direction
        public static String RotationType { get; set; }
        public static uint RotationDirection;


        public double DiamondCubeLocalAngle { get; private set; } = 0;
        public double DiamondCubeGlobalYAngle { get; private set; } = 0;

        internal void AdvanceTime(double deltaTime, ref RubikCubeArrangementModel rubikCube)
        {
            if (!Animating)
                return;

            {
                ActualRotationAngle = RotationSpeed * (float)deltaTime;
                if (CurrentRotationAngle + ActualRotationAngle >= TargetRotationAngle)
                {
                    Console.WriteLine("ROTATED! STOP");
                    ActualRotationAngle = TargetRotationAngle - CurrentRotationAngle;
                    CurrentRotationAngle = 0;
                    rotateRubikCube(ref rubikCube, true);
                    Animating = false;
                    Rotating = false;
                }
                else
                {
                    CurrentRotationAngle += ActualRotationAngle;
                    rotateRubikCube(ref rubikCube, false);
                    Console.WriteLine("ROTATING");
                }
            }
            
        }

        public void enableAnimation(String type, uint direction)
        {
            Animating = true;
            RotationType = type;
            RotationDirection = direction;
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

            float[] res = { newX, newY, newZ };
            return res;
        }
    }
}
