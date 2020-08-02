using Godot;
using Godot.Collections;
using Revise.ZMD;
using RoseImporter;

namespace Importers
{
    [Tool]
    public class ZmdImporter : EditorImportPlugin
    {
        public override Array GetImportOptions(int preset)
        {
            return new Array();
        }

        public override int GetImportOrder()
        {
            return 0;
        }

        public override string GetImporterName()
        {
            return "rose.zmd.import";
        }

        public override bool GetOptionVisibility(string option, Dictionary options)
        {
            return true;
        }

        public override int GetPresetCount()
        {
            return 0;
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
            return new Array
        {
            "zmd"
        };
        }

        public override string GetResourceType()
        {
            return "PackedScene";
        }

        public override string GetSaveExtension()
        {
            return "tscn";
        }

        public override string GetVisibleName()
        {
            return "ROSE Online ZMD";
        }

        public override int Import(string sourceFile, string savePath, Dictionary options, Array platformVariants, Array genFiles)
        {
            var zmd = new BoneFile();
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
                zmd.Load(sourceFile);
            }
            catch (System.IO.FileNotFoundException)
            {
                return (int)Error.FileNotFound;
            }
            var skel = new Skeleton();
            foreach (Bone bone in zmd.Bones)
            {
                skel.AddBone(bone.Name);
            }

            for (int i = 0; i < zmd.Bones.Count; ++i)
            {
                Bone bone = zmd.Bones[i];
                var t = new Transform()
                {
                    basis = new Basis(Utils.Rose2GodotRotation(bone.Rotation)),
                    origin = Utils.Rose2GodotPosition(bone.Translation) / 100
                };
                if (i > 0)
                {
                    skel.SetBoneParent(i, bone.Parent);
                }
                skel.SetBoneRest(i, t);
            }

            var spatial = new Spatial();
            spatial.AddChild(skel);
            spatial.Name = "Spatial";
            skel.Owner = spatial;
            skel.Name = "Skeleton";

            var scene = new PackedScene();
            scene.Pack(spatial);

            string file = savePath + "." + GetSaveExtension();
            return (int)ResourceSaver.Save(file, scene);
        }
    }
}