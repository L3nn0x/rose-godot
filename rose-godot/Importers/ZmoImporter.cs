using Godot;
using Godot.Collections;
using Revise.ZMO;
using Revise.ZMO.Channels;
using Revise.ZMD;
using RoseImporter;

namespace Importers
{
    [Tool]
    public class ZmoImporter : EditorImportPlugin
    {
        class Track
        {
            public Vector3 Loc;
            public Quat Rot;
            public Vector3 Scale;
        };

        public override string GetImporterName()
        {
            return "rose.zmo.import";
        }

        public override int GetImportOrder()
        {
            return 0;
        }

        public override Array GetImportOptions(int preset)
        {
            var row = new Dictionary
            {
                { "name", "skeleton" },
                { "default_value", "" },
                { "property_hint", PropertyHint.File }
            };
            return new Array {row};
        }

        public override bool GetOptionVisibility(string option, Dictionary options) => true;

        public override string GetPresetName(int preset) => "Default";

        public override int GetPresetCount() => 1;

        public override float GetPriority() => 1;

        public override Array GetRecognizedExtensions() => new Array { "zmo" };

        public override string GetResourceType() => "Animation";

        public override string GetSaveExtension() => "anim";

        public override string GetVisibleName() => "ROSE Online ZMO";

        public override int Import(string sourceFile, string savePath, Dictionary options, Array platformVariants, Array genFiles)
        {
            GD.Print($"ZMO import: \"{sourceFile}\"");

            string skel_path = options["skeleton"].ToString();

            if (string.IsNullOrEmpty(skel_path))
            {
                GD.PrintErr($"\"{sourceFile}\" - Reference ZMD file path is empty!");
                return (int)Error.FileNotFound;
            }
            GD.Print($"Skeleton path: {skel_path}");

            var zmd = new BoneFile();
            var zmo = new MotionFile();
            try
            {
                var f = new File();
                // TODO: find a better way to get the full path
                if (f.Open(skel_path, File.ModeFlags.Read) != Error.Ok)
                {
                    throw new System.IO.FileNotFoundException();
                }
                skel_path = f.GetPathAbsolute();
                f.Close();
                zmd.Load(skel_path);
            }
            catch (System.IO.FileNotFoundException)
            {
                return (int)Error.FileNotFound;
            }

            GD.Print("Loaded");

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
                zmo.Load(sourceFile);
            }
            catch (System.IO.FileNotFoundException)
            {
                return (int)Error.FileNotFound;
            }

            var anim = new Animation
            {
                Step = 1.0f / zmo.FramesPerSecond,
                Length = zmo.FrameCount / (float)zmo.FramesPerSecond,
                Loop = true
            };

            var tracks = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<float, Track>>();
            for (int i = 0; i < zmo.ChannelCount; ++i)
            {
                var channel = zmo[i];
                int c_idx = channel.Index;
                Bone bone;
                if (c_idx > zmd.Bones.Count - 1)
                {
                    int dummy_idx = channel.Index - zmd.Bones.Count;
                    GD.Print($"Dummy [{dummy_idx}]");
                    bone = zmd.DummyBones[dummy_idx];
                }
                else
                    bone = zmd.Bones[channel.Index];

                string track_name = ".:" + bone.Name;
                if (!tracks.ContainsKey(track_name))
                {
                    tracks.Add(track_name, new System.Collections.Generic.Dictionary<float, Track>());
                }
                var track = tracks[track_name];
                for (int j = 0; j < zmo.FrameCount; ++j)
                {
                    var time = j * anim.Step;
                    if (!track.ContainsKey(time))
                    {
                        track.Add(time, new Track()
                        {
                            Loc = Utils.Rose2GodotPosition(bone.Translation) / 10000f,
                            Rot = Quat.Identity,
                            Scale = new Vector3(1, 1, 1)
                        });
                    }

                    if (channel.Type == ChannelType.Position)
                    {
                        PositionChannel pchannel = channel as PositionChannel;
                        track[time].Loc = Utils.Rose2GodotPosition(pchannel.Frames[j]) / 10000f;
                    }
                    else if (channel.Type == ChannelType.Rotation)
                    {
                        RotationChannel rchannel = channel as RotationChannel;
                        var inverted = System.Numerics.Quaternion.Inverse(bone.Rotation);
                        var res = inverted * rchannel.Frames[j];
                        track[time].Rot = new Quat(res.X, res.Z, res.Y, -res.W);
                    }
                }
            }

            foreach (var kvp in tracks)
            {
                int i = anim.AddTrack(Animation.TrackType.Transform);
                anim.TrackSetImported(i, true);
                anim.TrackSetPath(i, kvp.Key);

                foreach (var tt in kvp.Value)
                {
                    anim.TransformTrackInsertKey(i,
                        tt.Key,
                        tt.Value.Loc,
                        tt.Value.Rot,
                        tt.Value.Scale);
                }
            }

            string file = $"{savePath}.{GetSaveExtension()}";
            return (int)ResourceSaver.Save(file, anim);
        }
    }
}