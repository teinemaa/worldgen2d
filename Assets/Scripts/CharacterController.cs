using UnityEngine;
using System.Collections;

public class CharacterController : MonoBehaviour
{
    private void Update ()
	{

        Vector2 speed = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);

        rigidbody2D.velocity = -0.4f* speed;
	}
}
