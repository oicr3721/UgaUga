using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance {  get; private set; }

    [SerializeField] private List<TeammateCandidate> candidates = new();
    [SerializeField] private TeammateCandidate candidatePrefab;

    [SerializeField] private float lineSpace; //candidates 줄세워둘 간격
    [SerializeField] private Transform exitTransform;
    [SerializeField] private Transform cutlineTransform;
    [SerializeField] private Vector2 waitPosRange = new Vector2(3, 8); //팀에 안 들어온 애들이 서있을 위치

    public ObservableValue RequiredMeat;

    public bool IsTeamValid => 
        (candidates.Count <= GameManager.Instance.MaxTeammateCount)
        && RequiredMeat.CurrentValue <= GameManager.Instance.PlayerMeat.CurrentValue;


    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        LoadTeam();

        SetCutLine();
    }

    //이미 있는 팀원들 시작 시 출구에 세워두기
    void LoadTeam()
    {
        candidates.Clear();
        for(int i = 0; i < GameManager.Instance.TeammateDatas.Count; i++)
        {
            TeammateCandidate tc = Instantiate(candidatePrefab);
            tc.Data = GameManager.Instance.TeammateDatas[i];
            tc.Recruited = true;
            candidates.Add(tc);

            Vector2 targetPos = (Vector2)exitTransform.position + Vector2.right * ((i + 1) * lineSpace);
            tc.transform.position = targetPos;
        }

        UpdateRequiredMeat();

    }

    void SetCutLine()
    {
        int maxTeammateCount = GameManager.Instance.MaxTeammateCount;
        cutlineTransform.position = exitTransform.position + Vector3.right * ((maxTeammateCount + 0.5f) * lineSpace);
    }

    public void AddCandidate(TeammateCandidate tc)
    {
        if (candidates.Contains(tc))
            return;

        candidates.Add(tc);
        UpdateCandidatesLine();
        UpdateRequiredMeat();
    }

    public void RemoveCandidate(TeammateCandidate tc)
    {
        candidates.Remove(tc);

        // 일단 임시로 아무 곳으로 보내기
        // 나중엔 애들끼리 안 겹치게 처리할 것.
        Vector2 randomPos =
            (Vector2)exitTransform.position +
            Vector2.right * Random.Range(waitPosRange.x, waitPosRange.y);

        tc.SetDestination(randomPos);

        UpdateCandidatesLine();
        UpdateRequiredMeat();
    }

    public void UpdateCandidatesLine()
    {
        for (int i = 0; i < candidates.Count; i++)
        {
            Vector2 targetPos =
                (Vector2)exitTransform.position +
                Vector2.right * ((i+1) * lineSpace);

            candidates[i].SetDestination(targetPos);
        }
    }

    public void UpdateRequiredMeat()
    {
        //필요 고기 갯수
        int requiredMeat = 0;
        foreach (TeammateCandidate tc in candidates)
        {
            requiredMeat += tc.Data.meatCount;
        }
        RequiredMeat.SetValue(requiredMeat);
    }

    //팀 확정
    public void ConfirmTeam()
    {
        GameManager.Instance.PlayerMeat.SubtractValue(RequiredMeat.CurrentValue);
        GameManager.Instance.SetTeam(candidates.Select(x => x.Data).ToList());
    }
}
