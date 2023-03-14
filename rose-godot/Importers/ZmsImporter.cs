using Godot;
using Godot.Collections;
using Revise.ZMS;
using RoseImporter;
using System.Text;

namespace Importers
{
    [Tool]
    public class ZmsImporter : EditorImportPlugin
    {
        public override Array GetImportOptions(int preset)
        {
            Array res = new Array();
            Dictionary row = new Dictionary
            {
                { "name", "texture" },
                { "default_value", "" },
                { "property_hint", PropertyHint.File }
            };
            res.Add(row);
            return res;
        }

        public override int GetImportOrder() => 0;

        public override string GetImporterName() => "rose.zms.import";

        public override bool GetOptionVisibility(string option, Dictionary options) => true;

        public override int GetPresetCount() => 1;

        public override string GetPresetName(int preset) => "Default";

        public override float GetPriority() => 1;

        public override Array GetRecognizedExtensions() => new Array { "zms", "ZMS" };

        public override string GetResourceType() => "Mesh";

        public override string GetSaveExtension() => "mesh";

        public override string GetVisibleName() => "ROSE Online ZMS";

        public override int Import(string sourceFile, string savePath, Dictionary options, Array platformVariants, Array genFiles)
        {
            ModelFile zms = new ModelFile();
            try
            {
                File f = new File();
                // TODO: find a better way to get the full path
                if (f.Open(sourceFile, File.ModeFlags.Read) != Error.Ok)
                {
                    throw new System.IO.FileNotFoundException();
                }
                sourceFile = f.GetPathAbsolute();
                f.Close();
                zms.Load(sourceFile);
            }
            catch (System.IO.FileNotFoundException)
            {
                return (int)Error.FileNotFound;
            }

            SurfaceTool surface_tool = new SurfaceTool();
            // load surface
            surface_tool.Begin(Mesh.PrimitiveType.Triangles);
            foreach (ModelVertex vertice in zms.Vertices)
            {
                if (zms.NormalsEnabled)
                {
                    surface_tool.AddNormal(Utils.Convert(vertice.Normal));
                }

                if (zms.ColoursEnabled)
                {
                    surface_tool.AddColor(Utils.Convert(vertice.Colour));
                }

                if (zms.BonesEnabled)
                {
                    int[] bones = new int[4];
                    for (int i = 0; i < bones.Length; ++i)
                    {
                        bones[i] = zms.BoneTable[vertice.BoneIndices[i]];
                    }
                    surface_tool.AddBones(bones);
                    float[] weights = new float[4];
                    vertice.BoneWeights.CopyTo(weights);
                    surface_tool.AddWeights(weights);
                }

                if (zms.TangentsEnabled)
                {
                    var t = new Plane()
                    {
                        Normal = Utils.Convert(vertice.Tangent),
                        x = vertice.Position.X,
                        y = vertice.Position.Z,
                        z = vertice.Position.Y
                    };
                    surface_tool.AddTangent(t);
                }

                if (zms.TextureCoordinates1Enabled)
                {
                    surface_tool.AddUv(Utils.Convert(vertice.TextureCoordinates[0]));
                }

                if (zms.TextureCoordinates2Enabled)
                {
                    surface_tool.AddUv2(Utils.Convert(vertice.TextureCoordinates[1]));
                }

                // must come last
                if (zms.PositionsEnabled)
                {
                    surface_tool.AddVertex(Utils.Rose2GodotPosition(vertice.Position));
                }
            }
            for (int i = 0; i < zms.Indices.Count; ++i)
            {
                surface_tool.AddIndex(zms.Indices[i].X);
                surface_tool.AddIndex(zms.Indices[i].Y);
                surface_tool.AddIndex(zms.Indices[i].Z);
            }

            surface_tool.Index();
            surface_tool.GenerateNormals();
            surface_tool.GenerateTangents();
            ArrayMesh array_mesh = surface_tool.Commit();
            // load texture
            string path = options["texture"].ToString();
            if (string.IsNullOrEmpty(path))
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(sourceFile);
                string ext = ".png";
                if (System.IO.Path.GetExtension(sourceFile).Equals(".ZMS"))
                {
                    ext = ".PNG";
                    name = System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(sourceFile), ".png");
                }
                /*
                if (name.BeginsWith("m_"))
                {
                    char[] tmp = name.ToCharArray();
                    tmp[0] = 't';
                    name = new string(tmp);
                }

                if (name.RFind("_") == name.Length - 2)
                {
                    StringBuilder tmp = new StringBuilder(name);
                    tmp.Erase(name.Length - 2, 2);
                    name = tmp.ToString();
                }
                */
                path = System.IO.Path.GetDirectoryName(sourceFile) + "/" + name + ext;
            }

            var dir = new Directory();
            if (dir.FileExists(path) && array_mesh.GetSurfaceCount() == 1)
            {
                var tex = ResourceLoader.Load<Texture>(path);
                var mat = new SpatialMaterial()
                {
                    FlagsUnshaded = true,
                    AlbedoTexture = tex
                };
                array_mesh.SurfaceSetMaterial(0, mat);
            }

            string file = savePath + "." + GetSaveExtension();
            return (int)ResourceSaver.Save(file, array_mesh);
        }
    }
}