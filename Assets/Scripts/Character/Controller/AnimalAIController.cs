using UnityEngine;

public class AnimalAIController : MonoBehaviour
{
    [SerializeField] private Animal animal;

    private void Update()
    {
        animal.SetMoveInput(Vector2.right);
    }
}