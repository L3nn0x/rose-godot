using Godot;
using Godot.Collections;
using Revise.ZMS;
using RoseImporter;

namespace Importers
{
    [Tool]
    public class ZmsImporter : EditorImportPlugin
    {
        public override Array GetImportOptions(int preset)
        {
            Array res = new Array();
            Dictionary row = new Dictionary();
            row.Add("name", "texture");
            row.Add("default_value", "");
            row.Add("property_hint", Godot.PropertyHint.File);
            res.Add(row);
            return res;
        }

        public override int GetImportOrder()
        {
            return 0;
        }

        public override string GetImporterName()
        {
            return "rose.zms.import";
        }

        public override bool GetOptionVisibility(string option, Dictionary options)
        {
            return true;
        }

        public override int GetPresetCount()
        {
            return 1;
        }

        public override string GetPresetName(int preset)
        {
            return "Default";
        }

        public override float GetPriority()
        {
            return 1;
        }

        public override Array GetRecognizedExtensions()
        {
            Array res = new Array
        {
            "zms"
        };
            return res;
        }

        public override string GetResourceType()
        {
            return "Mesh";
        }

        public override string GetSaveExtension()
        {
            return "mesh";
        }

        public override string GetVisibleName()
        {
            return "ROSE Online ZMS";
        }

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

            SurfaceTool st = new SurfaceTool();
            // load surface
            st.Begin(Mesh.PrimitiveType.Triangles);
            foreach (ModelVertex vertice in zms.Vertices)
            {
                if (zms.NormalsEnabled)
                {
                    st.AddNormal(Utils.Convert(vertice.Normal));
                }
                if (zms.ColoursEnabled)
                {
                    st.AddColor(Utils.Convert(vertice.Colour));
                }
                if (zms.BonesEnabled)
                {
                    int[] bones = new int[4];
                    for (int i = 0; i < bones.Length; ++i)
                    {
                        bones[i] = zms.BoneTable[vertice.BoneIndices[i]];
                    }
                    st.AddBones(bones);
                    float[] weights = new float[4];
                    vertice.BoneWeights.CopyTo(weights);
                    st.AddWeights(weights);
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
                    st.AddTangent(t);
                }
                if (zms.TextureCoordinates1Enabled)
                {
                    st.AddUv(Utils.Convert(vertice.TextureCoordinates[0]));
                }
                if (zms.TextureCoordinates2Enabled)
                {
                    st.AddUv2(Utils.Convert(vertice.TextureCoordinates[1]));
                }

                // must come last
                if (zms.PositionsEnabled)
                {
                    st.AddVertex(Utils.Rose2GodotPosition(vertice.Position));
                }
            }
            for (int i = 0; i < zms.Indices.Count; ++i)
            {
                st.AddIndex(zms.Indices[i].X);
                st.AddIndex(zms.Indices[i].Y);
                st.AddIndex(zms.Indices[i].Z);
            }

            st.Index();
            st.GenerateNormals();
            st.GenerateTangents();
            var mesh = st.Commit();
            // load texture
            string path = options["texture"].ToString();
            if (path == null || path == "")
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(sourceFile);
                string ext = ".png";
                if (System.IO.Path.GetExtension(sourceFile) == ".ZMS")
                {
                    ext = ".PNG";
                }
                if (name.BeginsWith("m_"))
                {
                    char[] tmp = name.ToCharArray();
                    tmp[0] = 't';
                    name = new string(tmp);
                }
                if (name.RFind("_") == name.Length - 2)
                {
                    System.Text.StringBuilder tmp = new System.Text.StringBuilder(name);
                    tmp.Erase(name.Length - 2, 2);
                    name = tmp.ToString();
                }
                path = System.IO.Path.GetDirectoryName(sourceFile) + "/" + name + ext;
            }
            var dir = new Directory();
            if (dir.FileExists(path) && mesh.GetSurfaceCount() == 1)
            {
                var tex = ResourceLoader.Load<Texture>(path);
                var mat = new SpatialMaterial()
                {
                    FlagsUnshaded = true,
                    AlbedoTexture = tex
                };
                mesh.SurfaceSetMaterial(0, mat);
            }

            string file = savePath + "." + GetSaveExtension();
            return (int)ResourceSaver.Save(file, mesh);
        }
    }
}