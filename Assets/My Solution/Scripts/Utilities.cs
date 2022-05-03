using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
	private const float CurvatureRadiusFactor = 2.23693629f; // CarController.CurrentSpeed multiplies speed by this value giving the wrong radius, so I'm dividing by it to get rid of it

	public static Pose ToPose(this Transform transform) => new(transform.position, transform.rotation);

	public static void ForEach<T>(this IEnumerable<T> collection, System.Action<T, int> action)
	{
		int i = 0;
		foreach (var item in collection)
		{
			action.Invoke(item, i++);
		}
	}

	public static float GetRadiusOfCurvature(float speed, float turnRate) => speed / (Mathf.Deg2Rad * turnRate * CurvatureRadiusFactor);
	public static Vector3 GetCenterOfCurvature(this Transform transform, float Radius, float steeringAngle) => transform.TransformPoint(Mathf.Sign(steeringAngle) * Radius * Vector3.right);
}
