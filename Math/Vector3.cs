using System;

namespace DSMM.Math
{
    [Serializable]
    public class Vector3
    {
        public float X, Y, Z;

        public Vector3(float x, float y, float z) 
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3(UnityEngine.Vector3 v)
        { 
            this.X = v.x;
            this.Y = v.y;
            this.Z = v.z;
        }

        public UnityEngine.Vector3 GetVector3()
        {
            return new UnityEngine.Vector3(this.X, this.Y, this.Z);
        }
    }
}
