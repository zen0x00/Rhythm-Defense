using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playermovement : MonoBehaviour
{
  public float speed = 5f;

  void Update()
  {
    if (Input.GetKey(KeyCode.W))
    {
      transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
  }
}

