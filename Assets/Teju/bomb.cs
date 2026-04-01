using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class bomb : MonoBehaviour
{
  public int bombCount = 0;
  public TMP_Text bombText;
  public Transform cam;
  Vector3 originalPos;

  void Start()
  {
    bombText.text = "0/3";
    originalPos = cam.position;
  }
  void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Bomb"))
    {
      Destroy(other.gameObject);
      bombCount++;
      bombText.text = bombCount + "/3";

      StartCoroutine(Shake());
    }
  }
  IEnumerator Shake()
  {
    float time = 0.3f;
    float a = 0f;
    while (a < time)
    {
      float x = Random.Range(-0.1f, 0.1f);
      float y = Random.Range(-0.1f, 0.1f);
      cam.position = originalPos + new Vector3(x, y, 0);
      a += Time.deltaTime;
      yield return null;
    }
    cam.position = originalPos;
  }
}
