using UnityEngine;

public class Blueprint : MonoBehaviour
{
	[SerializeField] float PeakHeight = 1f;
	[SerializeField] float Speed = 1f;
	[SerializeField] float RotationSpeed = 1f;

	void Update()
	{
		float Sine = Mathf.Sin(Time.time * Speed) * Time.deltaTime;
		Vector3 BobTheBuilder = Sine * Vector3.up;

		transform.position += BobTheBuilder * PeakHeight;
		transform.eulerAngles += RotationSpeed * Time.deltaTime * Vector3.up;
	}
}
