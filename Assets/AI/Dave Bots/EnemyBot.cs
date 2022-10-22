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
    bool doLerp = true;

    //MovementNode spawnNode;
    public MovementNode currentNode;
    public MovementNode nextNode;

    float StartTime;
    float EndTime;

    float HeightOffset;

    // Start is called before the first frame update
    void Start()
    {
        //nextNode = currentNode.GetNextNode();
        isActive = true;
        health = maxHealth;

        //InvokeRepeating("MoveToNextNode", 0.1f, 4f);
        //MoveToNextNode();
        StartTime = Time.time;
        EndTime = StartTime + 2.0f;
        do
        {
            nextNode = currentNode.CalculateNextNode();
        }
        while (currentNode.CalculateNextNode().platformIndex != currentNode.platformIndex);

        HeightOffset = this.GetComponent<BoxCollider>().bounds.size.y / 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
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
            //this.transform.position = currentNode.transform.position;
            //Debug.Log("hmmm");
            

            // this.transform.position = new Vector3(Vector3.Lerp(this.transform.position, nextNode.transform.position, 0.2f).x, 0, Vector3.Lerp(this.transform.position, nextNode.transform.position, 0.2f).z);




            if (Time.time < EndTime)
            {
                //Debug.Log("inwhile");
                float TimeProgressd = (Time.time - StartTime) / 0.4f;
                this.transform.position = new Vector3(Vector3.Lerp(currentNode.transform.position, nextNode.transform.position, TimeProgressd).x, currentNode.transform.position.y + HeightOffset, Vector3.Lerp(currentNode.transform.position, nextNode.transform.position, TimeProgressd).z);
                Debug.Log(TimeProgressd);
            }
            else
            {
                currentNode = nextNode;
                do
                {
                    nextNode = currentNode.CalculateNextNode();
                }
                while (currentNode.CalculateNextNode().platformIndex != currentNode.platformIndex);
                StartTime = Time.time;
                EndTime = StartTime + 0.4f;
            }



            //this.transform.position = Vector3.Lerp(currentNode.transform.position, nextNode.transform.position, 0.2f);
            //this.transform.position = Vector3.move

            //currentNode = nextNode;
            //Debug.Log("Part2");
        }
        //yield return new WaitForFixedUpdate();
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
