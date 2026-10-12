using System.Collections.Generic;
using UnityEngine;

public class KaisaProjectile : MonoBehaviour
{
    public float duration = 1.0f;
    private float timer = 0f;
    private Vector3 startpos;

    [SerializeField] private Health targetEnemy;

    List<Vector3> pointList = new List<Vector3>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (targetEnemy != null)
            UseSkill(targetEnemy);
    }

    public void UseSkill(Health enmey)
    {
        targetEnemy = enmey;
        startpos = transform.position;

        Vector3 dir = enmey.transform.position - transform.position;
        dir.y = 0;
        dir.Normalize();

        Vector3 point2 = startpos + dir * 2 + Vector3.up * Random.Range(4f,5f);
        Vector3 point3 = startpos + dir * 4 + Vector3.up * Random.Range(2f, 3.5f);

        pointList.Add(startpos);
        pointList.Add(point2);
        pointList.Add(point3);
        pointList.Add(targetEnemy.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if(targetEnemy == null)
        {
            return;
        }

        timer += Time.deltaTime;
        pointList[^1] = targetEnemy.transform.position;

        transform.position = DeCastelijau(pointList, timer / duration);

        if(timer >= duration)
        {
            //적 한테 도착
            targetEnemy.TakeDamage(2);
            Destroy(gameObject  );
        }
    }
    //4차 이상 부터
    private Vector3 DeCastelijau(List<Vector3> p, float t)
    {
        while (p.Count > 1)                  //남은 point가 1이 될 때 까지 반복 (모든 point가 lerp 된 상황 까지)
        {
            int last = p.Count - 1;         //마지막 요소의 인덱스

            List<Vector3> next = new List<Vector3>();
            for (int i = 0; i < last; i++)                                  //0부터 마지막 요소(-1)까지 순회
            {
                next.Add(Vector3.Lerp(p[i], p[i + 1], t));                  //ab,bc,cd 형식으로 계속 Lerp
            }

            p = next;                       //p(다음 순회의 대상)을 현재의 결과로 변경
        }

        return p[0];                        //마지막 남은 위치가 Bezier Curve의 결과
    }
}
