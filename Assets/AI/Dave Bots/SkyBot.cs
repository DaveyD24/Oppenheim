using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyBot : MonoBehaviour
{

    public float health;
    public float maxHealth;
    float damagePerHit = 25f;

    float movementSpeed = 5.0f;
    bool isActive;
    bool doLerp = true;

    //MovementNode spawnNode;
    public MovementNode currentNode;
    public MovementNode nextNode;

    float StartTime;
    float EndTime;

    float HeightOffset;

    float Width;
    float Height;

    int spawnCount = 3;
    bool hasSpawned = false;

    float ShelfScale;

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

        HeightOffset = this.GetComponent<BoxCollider>().bounds.size.y / 2.0f;
        Width = this.GetComponent<BoxCollider>().bounds.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            if (health <= 0)
            {
                isActive = false;
            }

            MoveToNextNode();

            Vector3 targetPosition = new Vector3(nextNode.transform.position.x, this.transform.position.y, nextNode.transform.position.z);
            this.transform.LookAt(targetPosition);

            GetNearestShelf();
            CreateScanner();

        }
        else
        {
            OnDeath();
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
                //Debug.Log(TimeProgressd);
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

    void GetNearestShelf()
    {

        Vector3 Scale = new Vector3(this.transform.localScale.x, this.transform.localScale.y * 20, this.transform.localScale.z);
        Vector3 Position = new Vector3(this.transform.position.x, this.transform.position.y - (Scale.y / 2) + HeightOffset, this.transform.position.z);
        Collider[] Colliders = Physics.OverlapBox(Position, Scale, this.transform.rotation);

        float closestDistance = 999999;

        foreach(Collider C in Colliders)
        {
            if (C.gameObject.CompareTag("Shelf"))
            {
                if (Vector3.Distance(C.gameObject.transform.position, this.transform.position) < closestDistance)
                {
                    closestDistance = Vector3.Distance(C.gameObject.transform.position, this.transform.position);
                    ShelfScale = Vector3.Distance(C.gameObject.transform.position, this.transform.position);
                }
            }
        }
    }

    void CreateScanner()
    {
        
        

        Vector3 Scale = new Vector3(this.transform.localScale.x, (this.transform.localScale.y * 0) + ShelfScale, this.transform.localScale.z);
        Vector3 Position = new Vector3(this.transform.position.x, this.transform.position.y - (Scale.y / 2) + HeightOffset, this.transform.position.z);

        Collider[] Colliders = Physics.OverlapBox(Position, Scale, this.transform.rotation);
        foreach (Collider C in Colliders)
        {
            if (C.gameObject.CompareTag("Player"))
            {
                Debug.Log("Sky Bot Has Deteced The Player!!!!!!!");
                if (!hasSpawned)
                {
                    //do something else
                }
            }
        }
    }

    void TakeDamage()
    {
        health = health - damagePerHit;
    }

    void OnDeath()
    {
        //do some death stuff
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            TakeDamage();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;

        Vector3 Scale = new Vector3(this.transform.localScale.x, (this.transform.localScale.y * 0) + ShelfScale, this.transform.localScale.z);
        Vector3 Position = new Vector3(this.transform.position.x, this.transform.position.y - (Scale.y / 2) + HeightOffset, this.transform.position.z);

        Gizmos.DrawWireCube(Position, Scale);
    }
}
