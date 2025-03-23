using GrafikaSzeminarium;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Collections;
using System.Numerics;
using Szeminarium;

internal class RubikCubeArrangementModel
{
    // a rubik kocka 27 kiskockabol all
    public List<ModelObjectDescriptor> Cubes { get; private set; }
    
    // minden kockat eltolunk a poziciojaban de az origoban modellezunk
    public List<Matrix4X4<float>> Transformations { get;  set; }

    // minden kockanak lesz egy logikai pozicioja
    // ennek segitsegevel ki lehet szurni a forgatni kivant oldalt
    public List<float[]> LogicalPositions { get; set; }

    private GL Gl;

    public Boolean rotating { get; set; }

    // statikus szinmeghatarozo
    private static readonly List<float[]> CubeColors = new List<float[]>
    {
        new float[] { 1.0f, 0.0f, 0.0f, 1.0f },  // 0. piros
        new float[] { 0.0f, 1.0f, 0.0f, 1.0f },  // 1. zold
        new float[] { 0.0f, 0.0f, 1.0f, 1.0f },  // 2. kek
        new float[] { 1.0f, 1.0f, 0.0f, 1.0f },  // 3. sarga
        new float[] { 1.0f, 0.5f, 0.0f, 1.0f },  // 4. narancs
        new float[] { 1.0f, 1.0f, 1.0f, 1.0f },  // 5. feher
        new float[] { 0.0f, 0.0f, 0.0f, 1.0f }   // 6. fekete -> ha nem kell szin
    };

    public RubikCubeArrangementModel(GL gl)
    {
        Gl = gl;
        rotating = false;
        Cubes = new List<ModelObjectDescriptor>();
        Transformations = new List<Matrix4X4<float>>();
        LogicalPositions = new List<float[]>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    // szin meghatarozasa
                    var colorList = DetermineColor(x, y, z);

                    // eltolas meghatarozasa + 0.1 terkoz
                    var translation = Matrix4X4.CreateTranslation(x * 1.1f, y * 1.1f, z * 1.1f);
                    Transformations.Add((Matrix4X4<float>)translation);

                    // logikai poziciok
                    float[] pos = { x, y, z };
                    LogicalPositions.Add(pos);

                    var cube = ModelObjectDescriptor.CreateCube(Gl, colorList);
                    Cubes.Add(cube);
                }
            }
        }
        


    }
    private float[] DetermineColor(int x, int y, int z)
    {
        List<float> colors = new List<float>();

        // itt ami a fontos: vertex array szerint vegigmenni
        // taktika: ha adott koordinatan van -> szinezzuk, kulonben -> fekete szin az adott oldal
        // az oldalakon vegigmenes fixalt
        
        // 1. hatso oldal: ha y == 1 
        for (int i = 0; i < 4; i++)
            colors.AddRange(y == 1 ? CubeColors[0] : CubeColors[6]);

        // 2. felso oldal: ha z == 1
        for (int i = 0; i < 4; i++)
            colors.AddRange(z == 1 ? CubeColors[1] : CubeColors[6]);

        // 3. bal oldal: ha x = -1
        for (int i = 0; i < 4; i++)
            colors.AddRange(x == -1 ? CubeColors[2] : CubeColors[6]);

        // 4. elulso oldal: ha y = -1
        for (int i = 0; i < 4; i++)
            colors.AddRange(y == -1 ? CubeColors[3] : CubeColors[6]);

        // 5. also oldad: ha z = -1
        for (int i = 0; i < 4; i++)
            colors.AddRange(z == -1 ? CubeColors[4] : CubeColors[6]);

        // 6. jobb oldal: ha x = 1
        for (int i = 0; i < 4; i++)
            colors.AddRange(x == 1 ? CubeColors[5] : CubeColors[6]);

        return colors.ToArray();
    }

    public void Dispose()
    {
        foreach (var cube in Cubes)
        {
            cube.Dispose(); // Ha van ilyen metódusa
        }
        Cubes.Clear();
    }

}
