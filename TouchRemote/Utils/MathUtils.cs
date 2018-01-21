using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Utils
{
    public static class MathUtils
    {
        public static float Scale(this float normalizedValue, float limitMin, float limitMax)
        {
            return (limitMax - limitMin) * normalizedValue + limitMin;
        }

        public static float Normalize(this float rawValue, float limitMin, float limitMax)
        {
            return (rawValue - limitMin) / (limitMax - limitMin);
        }
    }
}
