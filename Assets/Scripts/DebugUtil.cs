using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugUtil {

	public static void DrawDebugCross(Vector3 pos, float rad, Color col, float dur)
	{
		Debug.DrawRay(pos - Vector3.up * rad/2f, Vector3.up * rad, col, dur);
		Debug.DrawRay(pos - Vector3.right * rad/2f, Vector3.right * rad, col, dur);
		Debug.DrawRay(pos - Vector3.forward * rad/2f, Vector3.forward * rad, col, dur);
	}

}
