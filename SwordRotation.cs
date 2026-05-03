using UnityEngine;
using UnityEngine.Animations;

public class SwordRotation : MonoBehaviour
{
  float rotationSpeed;

  void Start()
  {
    rotationSpeed = GetComponentInParent<PlayerStats>().attackSpeed;
  }
  void Update()
  {
    rotationSpeed = GetComponentInParent<PlayerStats>().attackSpeed;
    if(rotationSpeed > 500f)
    {
      rotationSpeed = 500f;
    }
    transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
  }
}
