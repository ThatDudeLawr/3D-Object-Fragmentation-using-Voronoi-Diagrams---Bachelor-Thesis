
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GK {
	public static class VectorExtensions 
	{

		public static bool EsteReal(this float f) 
		{
			if (!float.IsInfinity(f) && !float.IsNaN(f))
				return true;
			else
				return false;
		}

		public static bool EsteReal(this Vector2 v2) 
		{
			if (v2.x.EsteReal() && v2.y.EsteReal())
				return true;
			else
				return false;
		}

		public static bool EsteReal(this Vector3 v3) 
		{
			if (v3.x.EsteReal() && v3.y.EsteReal() && v3.z.EsteReal())
				return true;
			else
				return false;
		}

	}
}
