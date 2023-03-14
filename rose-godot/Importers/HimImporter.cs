using Godot;
using Godot.Collections;
using Revise.HIM;

namespace Importers
{
    [Tool]
    public class HimImporter : EditorImportPlugin
    {
        public override Array GetImportOptions(int preset) => new Array();

        public override int GetImportOrder() => 0;

        public override string GetImporterName() => "rose.him.import";

        public override bool GetOptionVisibility(string option, Dictionary options) => true;

        public override int GetPresetCount() => 0;

        public override string GetPresetName(int preset) => "Default";

        public override float GetPriority() => 1;

        public override Array GetRecognizedExtensions() => new Array { "him" };

        public override string GetResourceType() => "Mesh";

        public override string GetSaveExtension() => "mesh";

        public override string GetVisibleName() => "ROSE Online HIM";

        public override int Import(string sourceFile, string savePath, Dictionary options, Array platformVariants, Array genFiles)
        {
            GD.Print($"HIM import: \"{sourceFile}\"");

            ArrayMesh work_mesh = new ArrayMesh();

            HeightmapFile him_file = new HeightmapFile();

            try
            {
                var io_file = new File();
                // TODO: find a better way to get the full path
                if (io_file.Open(sourceFile, File.ModeFlags.Read) != Error.Ok)
                {
                    throw new System.IO.FileNotFoundException();
                }
                sourceFile = io_file.GetPathAbsolute();
                io_file.Close();
                him_file.Load(sourceFile);
            }
            catch (System.IO.FileNotFoundException)
            {
                return (int)Error.FileNotFound;
            }


            SurfaceTool surface_tool = new SurfaceTool();
            surface_tool.Begin(Mesh.PrimitiveType.Triangles);

            float[,] dataTerrain = new float[him_file.Width, him_file.Height];
            for (int i = 0; i < him_file.Width - 1; i++)
                for (int j = 0; j < him_file.Height - 1; j++)
                    dataTerrain[j, i] = him_file[j, i] / 200f;

            int maxI = dataTerrain.GetLength(0);
            int maxJ = dataTerrain.GetLength(1);
            int quadCount = 0;

            //1º rows, 2º columns
            for (int i = 0; i < maxI - 1; i++)
            {
                for (int j = 0; j < maxJ - 1; j++)
                {
                    Quat quat = new Quat
                    {
                        x = dataTerrain[i, j],
                        y = dataTerrain[i, j + 1],
                        z = dataTerrain[i + 1, j],
                        w = dataTerrain[i + 1, j + 1]
                    }; // heights to Quat

                    // to mesh
                    Vector3 offset = new Vector3(j, 0, i);
                    CreateQuad(surface_tool, offset, quat);
                    quadCount++;
                }
            }

            // finally
            surface_tool.Index();
            surface_tool.GenerateNormals();
            surface_tool.GenerateTangents();
            surface_tool.Commit(work_mesh);

            string file = $"{savePath}.{GetSaveExtension()}";
            return (int)ResourceSaver.Save(file, work_mesh);
        }


        private SurfaceTool CreateQuad(SurfaceTool st, Vector3 pos, Quat q)
        {
            //1 Quad = 4 points = 2 triangles
            Vector3 v1 = new Vector3(0, q.x, -1) + pos;
            Vector3 v2 = new Vector3(1, q.y, -1) + pos;
            Vector3 v3 = new Vector3(1, q.w, 0) + pos;
            Vector3 v4 = new Vector3(0, q.z, 0) + pos;

            //GD.Print(pos / 64f, new Vector3(pos.x + 1f, 0, pos.y + 1f) / 64f);

            float factor = 1f / 64f;
            Vector2 top_left = new Vector2(pos.x * factor, pos.y * factor);
            Vector2 top_right = new Vector2((pos.x + 1f) * factor, pos.y * factor);
            Vector2 bottom_left = new Vector2(pos.x * factor, (pos.y + 1f) * factor);
            Vector2 bottom_right = new Vector2((pos.x + 1f) * factor, (pos.y + 1f) * factor);

            // triangle 1
            st.AddUv(top_left);
            st.AddVertex(v1);

            st.AddUv(top_right);
            st.AddVertex(v2);

            st.AddUv(bottom_right);
            st.AddVertex(v4);

            // triangle 2
            st.AddUv(top_left);
            st.AddVertex(v2);

            st.AddUv(bottom_left);
            st.AddVertex(v3);

            st.AddUv(bottom_right);
            st.AddVertex(v4);

            return st;
        }
    }
}