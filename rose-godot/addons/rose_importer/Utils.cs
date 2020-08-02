using System.Numerics;

namespace RoseImporter {
    class Utils
    {
        public static Godot.Vector3 Convert(Vector3 vec)
        {
            return new Godot.Vector3()
            {
                x = vec.X,
                y = vec.Y,
                z = vec.Z
            };
        }

        public static Godot.Vector2 Convert(Vector2 vec)
        {
            return new Godot.Vector2()
            {
                x = vec.X,
                y = vec.Y
            };
        }

        public static Godot.Color Convert(Revise.Types.Color4 c)
        {
            return new Godot.Color()
            {
                r = c.Red,
                g = c.Green,
                b = c.Blue,
                a = c.Alpha
            };
        }

        // Converts from ROSE (Z-up) to Godot (Y-up)
        public static Godot.Vector3 Rose2GodotPosition(Vector3 pos)
        {
            return new Godot.Vector3()
            {
                x = pos.X,
                y = pos.Z,
                z = pos.Y
            };
        }

        // Converts from ROSE to Godot
        public static Godot.Quat Rose2GodotRotation(Quaternion rot)
        {
            return new Godot.Quat()
            {
                x = rot.X,
                y = rot.Z,
                z = rot.Y,
                w = -rot.W
            };
        }

        // Converts from ROSE to Godot
        public static Godot.Vector3 Rose2GodotScale(Vector3 scale)
        {
            return new Godot.Vector3()
            {
                x = scale.X,
                y = scale.Z,
                z = scale.Y
            };
        }
    }
}