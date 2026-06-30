using System.Collections.Generic;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    [SerializeField] private List<Transform> backgrounds;

    [SerializeField] private float width = 20f;
    [SerializeField] private float speed = 5f;

    private void Start()
    {
        // 오른쪽 이동 기준으로 왼쪽부터 배치
        for (int i = 0; i < backgrounds.Count; i++)
        {
            Vector3 pos = backgrounds[i].position;
            pos.x = -width * i;
            backgrounds[i].position = pos;
        }
    }

    private void Update()
    {
        foreach (Transform bg in backgrounds)
        {
            Move(bg);
        }
    }

    private void Move(Transform bg)
    {
        bg.Translate(Vector3.right * speed * Time.deltaTime, Space.World);

        // 화면 오른쪽을 벗어나면 가장 왼쪽으로 이동
        if (bg.position.x >= width)
        {
            bg.position += Vector3.left * width * backgrounds.Count;
        }
    }
}