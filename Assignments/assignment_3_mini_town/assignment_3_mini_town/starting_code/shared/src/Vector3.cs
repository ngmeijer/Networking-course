using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared.src
{
    [Serializable]
    public class Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float pX = 0, float pY = 0, float pZ = 0)
        {
            x = pX;
            y = pY;
            z = pZ;
        }

        public Vector3 Zero()
        {
            x = 0;
            y = 0;
            z = 0;
            return this;
        }

        public override string ToString()
        {
            return $"{x}, {y}, {z}";
        }
    }
}
