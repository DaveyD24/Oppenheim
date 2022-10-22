using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBot : MonoBehaviour
{

    public float health;
    public float maxHealth;
    float damagePerHit = 10f;

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

    void CreateScanner()
    {

        Vector3 Position = new Vector3(this.transform.position.x + (Width / 2), this.transform.position.y, this.transform.position.z);
        Vector3 Scale = new Vector3(this.transform.localScale.x * 2, this.transform.localScale.y, this.transform.localScale.z);

        Collider[] Colliders = Physics.OverlapBox(Position, Scale, this.transform.rotation);
        foreach (Collider C in Colliders)
        {
            if (C.gameObject.CompareTag("Player"))
            {
                Debug.Log("Player Detected!");
                if (!hasSpawned)
                {
                    SpawnArmedBots();
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

    void SpawnArmedBots()
    {

            NodeManager nodeManager = GameObject.FindObjectOfType<NodeManager>();

            MovementNode shortestNode = new MovementNode();
            float shortestDistance = 99999f;
            foreach (MovementNode M in nodeManager.SpawnPoints)
            {
                if (Vector3.Distance(this.transform.position, M.transform.position) < shortestDistance)
                {
                    shortestDistance = Vector3.Distance(this.transform.position, M.transform.position);
                    shortestNode = M;
                }
            }

        GameObject ArmedEnemy = Instantiate(nodeManager.armedBotPrefab, shortestNode.transform.position, Quaternion.identity);
        ArmedEnemy.GetComponent<ArmedBot>().currentNode = shortestNode;
        hasSpawned = true;

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
        Gizmos.color = Color.red;
        Vector3 Scale = new Vector3(this.transform.localScale.x * 2, this.transform.localScale.y, this.transform.localScale.z);
        Vector3 Position = new Vector3(this.transform.position.x + (Width / 2), this.transform.position.y, this.transform.position.z);
        Gizmos.DrawWireCube(Position, Scale);
    }
}
