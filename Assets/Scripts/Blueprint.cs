using UnityEngine;
using UnityEngine.SceneManagement;
using EventSystem;

public class Blueprint : MonoBehaviour
{
	[SerializeField] float PeakHeight = 1f;
	[SerializeField] float Speed = 1f;
	[SerializeField] float RotationSpeed = 1f;

	[field: SerializeField] public string NextScene { get; private set; }

	void Update()
	{
		float Sine = Mathf.Sin(Time.time * Speed) * Time.deltaTime;
		Vector3 BobTheBuilder = Sine * Vector3.up;

		transform.position += BobTheBuilder * PeakHeight;
		transform.eulerAngles += RotationSpeed * Time.deltaTime * Vector3.up;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Player"))
		{
			UIEvents.SceneChange(NextScene);
			Destroy(this.gameObject.GetComponent<SpriteRenderer>());
		}
	}
}
