using UnityEngine;
using System.Collections;

// 확장 메소드.
namespace MathExtension {

	static class Vector {

		public static Vector3	XZ(this Vector3 v, float x, float z)
		{
			return(new Vector3(x, v.y, z));
		}

		// Vector3.Y()
		// Y성분을 설정합니다.
		public static Vector3	Y(this Vector3 v, float y)
		{
			return(new Vector3(v.x, y, v.z));
		}

		// Vector3.xz()
		// xz 성분으로 Vector2를 만듭니다.
		public static Vector2	xz(this Vector3 v)
		{
			return(new Vector2(v.x, v.z));
		}
	};
};
