using Opsive.UltimateCharacterController.Demo.UnityStandardAssets.Vehicles.Car;
using Opsive.UltimateCharacterController.Objects.CharacterAssist;
using Pathfinding;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CarController), typeof(Seeker), typeof(IDriveSource))]
public class CarAIFollowPath : MonoBehaviour
{
	private const float CutOffDistance = 0.5f;

	[Min(0)] public float RandomPointSearchRadius = 20f;
	[Min(0)] public float PathUpdateTimer = 1f;

	[Space]
	public LayerMask GroundLayers;
	[Min(0)] public float GroundCheckRaycastLength = 1;

	private CarController CarController;
	private Seeker Seeker;
	private IDriveSource IDriveSource;

	private Coroutine AutoPathfindingCOR;
	private Vector3 TargetPoint;

	private void Awake()
	{
		CarController = GetComponent<CarController>();
		Seeker = GetComponent<Seeker>();
		IDriveSource = GetComponent<IDriveSource>();
		enabled = false;
	}

	private void OnEnable()
	{
		if ((IDriveSource as MonoBehaviour).enabled)
		{
			CarController.Move(0, 0, 0, 0);
		}

		TargetPoint = GetRandomReachablePoint();

		InvokeRepeating(nameof(UpdateCarPath), 0, PathUpdateTimer); // to avoid updating the path on every frame
	}

	private void OnDisable()
	{
		CancelInvoke(nameof(UpdateCarPath));
		if (AutoPathfindingCOR != null)
		{
			StopCoroutine(AutoPathfindingCOR);
		}
	}

	private void UpdateCarPath()
	{
		if (AutoPathfindingCOR != null)
		{
			StopCoroutine(AutoPathfindingCOR);
		}

		AutoPathfindingCOR = StartCoroutine(FindAndFollowPath(TargetPoint));
	}

	private IEnumerator FindAndFollowPath(Vector3 point)
	{
		var path = Seeker.StartPath(transform.position, point);
		yield return StartCoroutine(path.WaitForPath());

		for (int i = 0; i < path.vectorPath.Count;)
		{
			// steer the car in the direction of the next point in the path
			var DirectionToNextPoint = path.vectorPath[i] - transform.position;
			var AngleFromFowardOfCar = Vector3.SignedAngle(transform.forward, DirectionToNextPoint, Vector3.up);

			float steering = Mathf.Clamp(AngleFromFowardOfCar / 90f, -1, 1);
			CarController.Move(steering, 0.3f, 1f, 0f);

			if (Vector3.Distance(transform.position, path.vectorPath[i]) <= CutOffDistance)
			{
				i++; // start navigating to next point after reaching current point
			}

			yield return null;
		}
	}

	private Vector3 GetRandomReachablePoint()
	{
		Vector3 point;
		do
		{
			point = GetRandomWorldPoint();
		} while (IsPointInsideUnreachableZone(point) && !IsPointOverGround(point));
		return point;
	}

	private Vector3 GetRandomWorldPoint()
	{
		var randomAngle = Random.Range(70f, 150); // behind or to the side of the car
		randomAngle *= -Mathf.Sign(CarController.CurrentSteerAngle); // choose a point on the opposite side the car is already turning to
		var rotatedForwardDirection = Quaternion.Euler(0, randomAngle, 0) * transform.forward;
		return transform.position + (rotatedForwardDirection * RandomPointSearchRadius);
	}

	private bool IsPointInsideUnreachableZone(Vector3 point)
	{
		var Radius = Utilities.GetRadiusOfCurvature(CarController.CurrentSpeed, CarController.MaxSteerAngle);
		var RightCenter = transform.GetCenterOfCurvature(Radius, CarController.MaxSteerAngle);
		var LeftCenter = transform.GetCenterOfCurvature(Radius, -CarController.MaxSteerAngle);

		return (Vector3.Distance(point, RightCenter) <= Radius) || (Vector3.Distance(point, LeftCenter) <= Radius);
	}

	private bool IsPointOverGround(Vector3 point) => Physics.Raycast(point, Vector3.down, GroundCheckRaycastLength, GroundLayers.value);
}
