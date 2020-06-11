using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code {
    static class ColorExtentions {

        public static uint ToUInt(Color color) {
            Color32 color32 = (Color32)color;
            return (uint)((color32[0] << 24) | (color32[1] << 16) | (color32[2] << 8) | color32[3]);
        }

        public static Color FromUInt(uint value) {
            return new Color32(
                (byte)((value >> 24) & 0xFF),
                (byte)((value >> 16) & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)((value) & 0xFF)
            );
        }
    }
}
