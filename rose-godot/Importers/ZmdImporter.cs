using Godot;
using Godot.Collections;
using Revise.ZMD;
using RoseImporter;

namespace Importers
{
    [Tool]
    public class ZmdImporter : EditorImportPlugin
    {
        public override Array GetImportOptions(int preset) => new Array();

        public override int GetImportOrder() => 0;

        public override string GetImporterName() => "rose.zmd.import";

        public override bool GetOptionVisibility(string option, Dictionary options) => true;

        public override int GetPresetCount() => 0;

        public override string GetPresetName(int preset) => "Default";

        public override float GetPriority() => 1;

        public override Array GetRecognizedExtensions() => new Array { "zmd" };

        public override string GetResourceType() => "PackedScene";

        public override string GetSaveExtension() => "tscn";

        public override string GetVisibleName() => "ROSE Online ZMD";

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
                GD.Print($"{bone.Name}");
            }

                for (int i = 0; i < zmd.Bones.Count; ++i)
            {
                Bone bone = zmd.Bones[i];
                var bone_transform = new Transform(new Basis(Utils.Rose2GodotRotation(bone.Rotation)), Utils.Rose2GodotPosition(bone.Translation) / 100f);

                if (i > 0)
                    skel.SetBoneParent(i, bone.Parent);

                skel.SetBoneRest(i, bone_transform);
            }

            for (int i = 0; i < zmd.DummyBones.Count; ++i)
            {
                Bone bone = zmd.Bones[i];
                var bone_transform = new Transform(new Basis(Utils.Rose2GodotRotation(bone.Rotation)), Utils.Rose2GodotPosition(bone.Translation) / 100f);
                skel.SetBoneRest(i, bone_transform);
            }

            var spatial = new Spatial();
            spatial.AddChild(skel);
            spatial.Name = "Armature";
            skel.Owner = spatial;
            skel.Name = "Skeleton";

            var scene = new PackedScene();
            scene.Pack(spatial);

            string file = $"{savePath}.{GetSaveExtension()}";
            return (int)ResourceSaver.Save(file, scene);
        }
    }
}
