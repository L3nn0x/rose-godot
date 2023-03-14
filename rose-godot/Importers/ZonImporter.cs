using Godot;
using Godot.Collections;
using Revise.ZON;

namespace Importers
{
    [Tool]
    public class ZonImporter : EditorImportPlugin
    {
        public override Array GetImportOptions(int preset) => new Array();

        public override int GetImportOrder() => 0;

        public override string GetImporterName() => "rose.zon.import";

        public override bool GetOptionVisibility(string option, Dictionary options) => true;

        public override int GetPresetCount() => 0;

        public override string GetPresetName(int preset) => "Default";

        public override float GetPriority() => 1;

        public override Array GetRecognizedExtensions() => new Array { "zon" };

        public override string GetResourceType() => "Resource";

        public override string GetSaveExtension() => "tscn";

        public override string GetVisibleName() => "ROSE Online ZON";

        public override int Import(string sourceFile, string savePath, Dictionary options, Array platformVariants, Array genFiles)
        {
            GD.Print("ZON import");

            ZoneFile zon = new ZoneFile();

            try
            {
                var f = new File();
                // TODO: find a better way to get the full path
                if (f.Open(sourceFile, File.ModeFlags.Read) != Error.Ok)
                {
                    throw new System.IO.FileNotFoundException();
                }
                sourceFile = f.GetPathAbsolute();
                f.Close();
                zon.Load(sourceFile);
            }
            catch (System.IO.FileNotFoundException)
            {
                return (int)Error.FileNotFound;
            }


            var scene = new PackedScene();

            var spatial = new Spatial();
            var mesh_instance = new MeshInstance();

            SurfaceTool surface_tool = new SurfaceTool();
            surface_tool.Begin(Mesh.PrimitiveType.Triangles);



            //surface_tool.Index();
            //surface_tool.GenerateNormals();
            //surface_tool.GenerateTangents();
            ArrayMesh array_mesh = surface_tool.Commit();

            mesh_instance.Mesh = array_mesh;

            spatial.AddChild(mesh_instance);

            scene.Pack(spatial);

            string file = $"{savePath}.{GetSaveExtension()}";
            return (int)ResourceSaver.Save(file, scene);
        }
    }
}