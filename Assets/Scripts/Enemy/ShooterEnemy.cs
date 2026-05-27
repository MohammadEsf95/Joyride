using System.Collections;
using UnityEngine;

public class ShooterEnemy: MonoBehaviour
{
    public int minMoves = 3;
    public int maxMoves = 6;
    public float moveDistance;
    public float moveDuration;
    public AnimationCurve moveCurve;
    
    IEnumerator EnemyRoutine()
    {
        int count = Random.Range(minMoves, maxMoves);
        
        for (int i = 0; i < count; i++)
        {
            yield return StartCoroutine(MoveStep(new Vector3(transform.position.x,
                transform.position.y + Random.Range(0,2) == 0 ? moveDistance : -moveDistance,
                0)));
            // yield return new WaitForSeconds(moveDuration);
            Debug.Log("Shoot");
        }
    }

    IEnumerator MoveStep(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            float curvedT = moveCurve.Evaluate(t);
            transform.position = Vector3.Lerp(startPos, targetPos, curvedT);
            yield return null;
        }   
    }
}
