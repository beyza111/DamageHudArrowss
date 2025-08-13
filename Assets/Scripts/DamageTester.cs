/*using UnityEngine;

public class DamageTester : MonoBehaviour
{
    public DamageHUDArrowsController arrows;
    public Transform player;
    public Transform[]sources;
    int i = 0;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && sources.Length > 0)
        {
            arrows.ReportDamage(sources[i].position);
            i = (i + 1) % sources.Length;
        }
            
    }
}
*/