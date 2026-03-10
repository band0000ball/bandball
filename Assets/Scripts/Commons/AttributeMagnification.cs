using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using UnityEngine;

namespace Commons
{
    public static class AttributeMagnification
    {
        public enum Attribute
        {
            Frame,
            Aqua,
            Electric,
            Plant,
            Ground,
            Ice,
            Oil,
            Wind,
            Toxin,
            Spirit,
            None,
        }

        private static readonly Color Brown = new(0.525f, 0.29f, 0.169f);
        // private static readonly Color Purple = new(0.59f, 0.23f, 0.66f);
        private static readonly Color Purple = new(0.498f, 0.067f, 0.518f);
        private static readonly Color Navy = new(0.137f, 0.231f, 0.424f);

        public static Dictionary<Attribute, Color> AttColor = new()
        {
            { Attribute.Frame, Color.red },
            { Attribute.Aqua, Color.blue },
            { Attribute.Electric, Color.yellow },
            { Attribute.Plant, Color.green },
            { Attribute.Ground, Brown },
            { Attribute.Ice, Color.cyan },
            { Attribute.Oil, Color.black },
            { Attribute.Wind, Color.white },
            { Attribute.Toxin, Purple },
            { Attribute.Spirit, Navy },
            { Attribute.None, Color.gray}
        };
        
        private static readonly float[][] Magnification = {
            new[] {1.0f, 2.0f, 1.0f, 1.0f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f},
            new[] {0.5f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f},
            new[] {1.0f, 0.5f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f},
            new[] {1.0f, 1.0f, 0.5f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f},
            new[] {2.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f},
            new[] {0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 2.0f, 1.0f},
            new[] {1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f},
            new[] {1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 1.0f},
            new[] {1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f},
            new[] {1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 0.5f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f},
            new[] {1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f}
        };

        public static float Choice(Attribute myAttribute, Attribute enemyAttribute) => Magnification[(int)enemyAttribute][(int)myAttribute];

        public static float Calc(float[] attribute, Attribute enemyAttribute)
        {
            if (enemyAttribute is Attribute.None) return 1;
            float[] magn = Magnification[(int)enemyAttribute];
            return magn.Zip(attribute, (a, b) => a * b).Sum();
        }
    }
}