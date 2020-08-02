using Godot;
using Godot.Collections;
using Revise.ZMO;
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
            var row = new Dictionary();
            row.Add("name", "skeleton");
            row.Add("default_value", "");
            row.Add("property_hint", PropertyHint.File);
            return new Array
        {
            row
        };
        }

        public override bool GetOptionVisibility(string option, Dictionary options)
        {
            return true;
        }

        public override string GetPresetName(int preset)
        {
            return "Default";
        }

        public override int GetPresetCount()
        {
            return 1;
        }

        public override float GetPriority()
        {
            return 1;
        }

        public override Array GetRecognizedExtensions()
        {
            return new Array
        {
            "zmo"
        };
        }

        public override string GetResourceType()
        {
            return "Animation";
        }

        public override string GetSaveExtension()
        {
            return "anim";
        }

        public override string GetVisibleName()
        {
            return "ROSE Online ZMO";
        }

        public override int Import(string sourceFile, string savePath, Dictionary options, Array platformVariants, Array genFiles)
        {
            string skel_path = options["skeleton"].ToString();
            if (skel_path == null || skel_path == "")
            {
                return (int)Error.FileNotFound;
            }
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
                Step = (float)(1.0 / zmo.FramesPerSecond),
                Length = (float)(zmo.FrameCount / (float)zmo.FramesPerSecond),
                Loop = true
            };

            var tracks = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<float, Track>>();
            for (int i = 0; i < zmo.ChannelCount; ++i)
            {
                var channel = zmo[i];
                var bone = zmd.Bones[channel.Index];
                string track_name = "Skeleton:" + bone.Name;
                if (!tracks.ContainsKey(track_name))
                {
                    tracks.Add(track_name,
                               new System.Collections.Generic.Dictionary<float, Track>());
                }
                var track = tracks[track_name];
                for (int j = 0; j < zmo.FrameCount; ++j)
                {
                    var time = j * anim.Step;
                    if (!track.ContainsKey(time))
                    {
                        track.Add(time, new Track()
                        {
                            Loc = Utils.Rose2GodotPosition(bone.Translation) / 10000,
                            Rot = Quat.Identity,
                            Scale = new Vector3(1, 1, 1)
                        });
                    }

                    if (channel.Type == ChannelType.Position)
                    {
                        Revise.ZMO.Channels.PositionChannel pchannel = channel as Revise.ZMO.Channels.PositionChannel;
                        track[time].Loc = Utils.Rose2GodotPosition(pchannel.Frames[j]) / 10000;
                    }
                    else if (channel.Type == ChannelType.Rotation)
                    {
                        Revise.ZMO.Channels.RotationChannel rchannel = channel as Revise.ZMO.Channels.RotationChannel;
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

            string file = savePath + "." + GetSaveExtension();
            return (int)ResourceSaver.Save(file, anim);
        }
    }
}