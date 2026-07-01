using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance {  get; private set; }

    [SerializeField] private List<TeammateCandidate> candidates = new();

    [SerializeField] private float lineSpace; //candidates 줄세워둘 간격
    [SerializeField] private Vector2 waitPosRange = new Vector2(3, 8); //팀에 안 들어온 애들이 서있을 위치

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCandidate(TeammateCandidate tc)
    {
        if (candidates.Contains(tc))
            return;

        candidates.Add(tc);
        UpdateCandidatesLine();
    }

    public void RemoveCandidate(TeammateCandidate tc)
    {
        candidates.Remove(tc);

        // 일단 임시로 아무 곳으로 보내기
        Vector2 randomPos =
            (Vector2)transform.position +
            Vector2.right * Random.Range(waitPosRange.x, waitPosRange.y);

        tc.SetDestination(randomPos);

        UpdateCandidatesLine();
    }

    public void UpdateCandidatesLine()
    {
        for (int i = 0; i < candidates.Count; i++)
        {
            Vector2 targetPos =
                (Vector2)transform.position +
                Vector2.right * (i * lineSpace);

            candidates[i].SetDestination(targetPos);
        }
    }
}
