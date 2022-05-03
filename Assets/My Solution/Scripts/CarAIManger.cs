using Opsive.UltimateCharacterController.Demo.UnityStandardAssets.Vehicles.Car;
using Opsive.UltimateCharacterController.Objects.CharacterAssist;
using UnityEngine;

[RequireComponent(typeof(CarCollisionDetection), typeof(CarUserControl), typeof(CarAIFollowPath))]
[RequireComponent(typeof(IDriveSource))]
public class CarAIManger : MonoBehaviour
{
	[Min(0)] public float CarStateUpdateTimer = 2f;

	private CarCollisionDetection CarCollisionDetection;
	private CarUserControl CarUserControl;
	private CarAIFollowPath CarAIFollowPath;
	private IDriveSource IDriveSource;

	private void Awake()
	{
		CarCollisionDetection = GetComponent<CarCollisionDetection>();
		CarUserControl = GetComponent<CarUserControl>();
		CarAIFollowPath = GetComponent<CarAIFollowPath>();
		IDriveSource = GetComponent<IDriveSource>();
	}

	private void OnEnable() => InvokeRepeating(nameof(UpdateCarControlState), 0, CarStateUpdateTimer);
	private void OnDisable() => CancelInvoke(nameof(UpdateCarControlState));

	private void UpdateCarControlState()
	{
		if (!(IDriveSource as MonoBehaviour).enabled) // player isn't using this car
		{
			return;
		}

		var IsPathBlocked = CarCollisionDetection.IsCurrentPathBlocked();
		CarAIFollowPath.enabled = IsPathBlocked;
		CarUserControl.enabled = !IsPathBlocked;
	}
}
