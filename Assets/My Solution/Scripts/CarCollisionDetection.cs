using Opsive.UltimateCharacterController.Demo.UnityStandardAssets.Vehicles.Car;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarCollisionDetection : MonoBehaviour
{
	public Vector3 CarBoundarySize;
	public Vector3 CarBoundaryCenterOffset;

	public uint PredictedPointsCount = 3;
	public float PredictedPointsCountSpeedMultiplier;
	public LayerMask ObstacleLayers;
	[Min(0)] public float GroundCheckRaycastLength = 1;

	[SerializeField] [HideInInspector] private CarController CarController;

	private void Reset() => CarController = GetComponent<CarController>();

	public IEnumerable<Pose> PredictPointsAlongPath()
	{
		Pose[] Result = new Pose[GetPredictedPointsCount()];
		var Radius = Utilities.GetRadiusOfCurvature(CarController.CurrentSpeed, Mathf.Abs(CarController.CurrentSteerAngle));

		//if car is moving forward, stationary or the radius of curvature is too low
		if (CarController.CurrentSteerAngle == 0f || CarController.CurrentSpeed == 0f || Radius <= CarBoundarySize.x)
		{
			// Predict a path straight forward
			Result.ForEach((_, i) => Result[i] = new Pose(transform.position + (CarBoundarySize.z * (i + 1) * transform.forward), transform.rotation));
		}
		else
		{
			// else Predict a path that curves with the car's current turn rate
			var Center = transform.GetCenterOfCurvature(Radius, CarController.CurrentSteerAngle);
			var DirectionToCar = transform.position - Center;

			var AngleStep = 300f / Radius;
			Result.ForEach((_, i) =>
			{
				Vector3 RotatedDirection = Quaternion.Euler(0, Mathf.Sign(CarController.CurrentSteerAngle) * (i + 1) * AngleStep, 0) * DirectionToCar;
				var Position = Center + RotatedDirection;

				var CurvatureTangent = Vector3.Cross(RotatedDirection, transform.up);
				var Rotation = Quaternion.LookRotation(CurvatureTangent, transform.up);

				Result[i] = new Pose(Position, Rotation);
			});
		}

		return Result;
	}

	private uint GetPredictedPointsCount() => PredictedPointsCount + (uint)(PredictedPointsCountSpeedMultiplier * CarController.CurrentSpeed);

	public bool ObstaclesExistAtPoint(Pose point) => Physics.OverlapBox(point.position + CarBoundaryCenterOffset, CarBoundarySize / 2f, point.rotation, ObstacleLayers.value).Length > 0;
	public bool GroundExistsAtPoint(Pose point) => Physics.BoxCast(point.position + CarBoundaryCenterOffset, CarBoundarySize / 2f, Vector3.down, point.rotation, GroundCheckRaycastLength, ObstacleLayers.value);
	public bool IsPathClearAtPoint(Pose point) => GroundExistsAtPoint(point) && !ObstaclesExistAtPoint(point);

	public bool IsCurrentPathBlocked() => PredictPointsAlongPath().Any(point => !IsPathClearAtPoint(point));

	private void DrawPointGizmo(Pose Point, Color color)
	{
		var M = Gizmos.matrix;
		M.SetTRS(Point.position, Point.rotation, Vector3.one);
		Gizmos.matrix = M;

		Gizmos.color = color;

		Gizmos.DrawWireCube(CarBoundaryCenterOffset, CarBoundarySize);
	}

	private void OnDrawGizmosSelected()
	{
		DrawPointGizmo(transform.ToPose(), Color.blue);
		if (Application.isPlaying)
		{
			PredictPointsAlongPath().ForEach((point, _) =>
			{
				var color = IsPathClearAtPoint(point) ? Color.green : Color.red;
				DrawPointGizmo(point, color);
			});
		}
	}
}
