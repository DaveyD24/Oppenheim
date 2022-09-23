using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventSystem;

public class WaterScript : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("xd");
            //CharacterController soldier = other.GetComponent<CharacterController>();
            //SoldierMovement soldier = other.GetComponent<SoldierMovement>();
            //soldier.isSwimming = true;

            if (other.TryGetComponent(out SoldierMovement soldier))
            {
                soldier.isSwimming = true;

                // When we're a Soldier, get Bob the Builder and make the Y-position variable.
                if (soldier.TryGetComponent(out BobScript bobTheBuilder))
                {
                    bobTheBuilder.yPosition = transform.position.y;
                }
            }
            else if (other.TryGetComponent(out PlayerController nonSoldier))
            {
                // Kill non-Soldier Players.
                nonSoldier.OnDeath();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //CharacterController soldier = other.GetComponent<CharacterController>();
            //SoldierMovement soldier = other.GetComponent<SoldierMovement>();
            //soldier.isSwimming = false;
            if (other.TryGetComponent(out SoldierMovement soldier))
            {
                soldier.isSwimming = false;

                Debug.Log("Stahp swimming");
            }
            else if (other.TryGetComponent(out PlayerController nonSoldier))
            {
                // Kill non-Soldier Players.
                nonSoldier.OnDeath();
            }
        }
    }

    public void Drain()
    {
        StartCoroutine(StartDraining());
    }

    IEnumerator StartDraining()
    {
        const float kTimeToDrain = .333333f; // 3 seconds (1 / 3).
        float t = 0f;
        Vector3 preDrainPosition = transform.position;

        while (t <= 1f)
        {
            t += Time.deltaTime * kTimeToDrain;
            Vector3 scale = transform.localScale;
            transform.localScale = Vector3.Lerp(scale, new Vector3(scale.x, 0f, scale.z), t);
            transform.position = Vector3.Lerp(preDrainPosition, preDrainPosition + Vector3.down * 1.5f, t);

            yield return null;
        }

        transform.localScale = Vector3.zero;
    }
}
