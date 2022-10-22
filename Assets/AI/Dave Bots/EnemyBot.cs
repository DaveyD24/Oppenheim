using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBot : MonoBehaviour
{

    float health;
    public float maxHealth;
    float damagePerHit;

    float movementSpeed = 5.0f;
    bool isActive;

    //MovementNode spawnNode;
    public MovementNode currentNode;
    MovementNode nextNode;

    // Start is called before the first frame update
    void Start()
    {
        //nextNode = currentNode.GetNextNode();
        isActive = true;
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        while (isActive)
        {
            if (health <= 0)
            {
                OnDeath();
            }
            MoveToNextNode();
        }
    }

    void MoveToNextNode()
    {
        if (currentNode != null)
        {
            Debug.Log("hmmm");
            nextNode = currentNode.CalculateNextNode();
            Vector3.Lerp(this.transform.position, nextNode.transform.position, movementSpeed * Time.deltaTime);
            currentNode = nextNode;
        }
    }

    void TakeDamage()
    {
        health = health - damagePerHit;
    }

    void OnDeath()
    {
        isActive = false;
        //do some death stuff
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            TakeDamage();
        }
    }
}
