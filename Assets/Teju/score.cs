using UnityEngine;
using TMPro;
public class score : MonoBehaviour
{
  public int scoreValue = 0;
  public TMP_Text scoreText;

  void Start()
  {
    scoreText.text = "Score: 0";
  }

  void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Trash"))
    {
      Destroy(other.gameObject);
      scoreValue += 10;
      scoreText.text = "Score: " + scoreValue;
    }
  }

}
