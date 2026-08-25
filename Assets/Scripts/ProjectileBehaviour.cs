using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileBehaviour : MonoBehaviour
{
	[Header("Movement")]
	public float speed = 50f;

	private Rigidbody _rb;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
	}

	void Update()
	{
		Vector3 movement = transform.forward * (speed * Time.deltaTime);
		_rb.MovePosition(transform.position + movement);
	}

	void OnTriggerEnter(Collider theCollider)
	{
		if (theCollider.tag == "Enemy" || theCollider.tag == "Environment")
			RemoveProjectile();
	}

	void RemoveProjectile()
	{
		Destroy(gameObject);
	}
}
