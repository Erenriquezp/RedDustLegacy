using UnityEngine;
using UnityEngine;

public class LeviatanCore : MonoBehaviour
{
    public LeviatanAI boss;

    public void TakeDamage(int damage)
    {
        if (boss != null)
        {
            boss.TakeCoreDamage(damage);
        }
    }

}
