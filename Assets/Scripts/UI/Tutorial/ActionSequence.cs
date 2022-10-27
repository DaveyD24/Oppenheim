using System.Collections;
using System.Collections.Generic;
using EventSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Play a list of actions in sequence, waiting until the action is complete before continuing with the next one.
/// </summary>
public class ActionSequence : MonoBehaviour
{
    private Node<ActionSequence> topNode;

    [field: Header("Spawn In Settings")]
    [field: SerializeField] public GameObject[] DeadPlayers { get; private set; }

    [field: SerializeField] public GameObject[] AlivePlayers { get; private set; }

    [Header("oppenheim settings")]
#pragma warning disable SA1201 // Elements should appear in the correct order
    [SerializeField] private float talkSpeed;
#pragma warning restore SA1201 // Elements should appear in the correct order
    [SerializeField] private float sentencePauseTime;
    [SerializeField] public TextMeshProUGUI oppenheimText;
    [SerializeField] public GameObject oppenheimObj;
    [SerializeField] private ParticleSystem lifeParticles;

    [field: SerializeField] public float MaxDistanceAway { get; set; }

    [Header("Dialogue Lists")]
#pragma warning disable SA1201 // Elements should appear in the correct order
    [SerializeField] private List<string> welcomeDialogue;
#pragma warning restore SA1201 // Elements should appear in the correct order
    [SerializeField] private List<string> rotateDialogue;
    [SerializeField] private List<string> workOrderDialoguePrt1;
    [SerializeField] private List<string> workOrderDialoguePrt2;
    [SerializeField] private List<string> cameraDialogue;

    public bool BDoorOpen { get; set; } = false;

    private void Start()
    {
        // BuildSequenceTree();
    }

    private void BuildSequenceTree()
    {
        WaitSpawnPlayers waitSpawnPlayers = new WaitSpawnPlayers(this);
        PlayDialogue welcomDialogue = new PlayDialogue(this, welcomeDialogue, oppenheimText, talkSpeed, sentencePauseTime); // play the intro welcome text
        MoveCloser checkCloseEnough = new MoveCloser(this, oppenheimObj);
        PlayDialogue firstChallengeDialogue = new PlayDialogue(this, workOrderDialoguePrt2, oppenheimText, talkSpeed, sentencePauseTime); // play the dialogue explaining how to rotate character
        CycleCamera cycleCamera = new CycleCamera(this, 5);
        InstructionBookSequence instructionBookSequence = new InstructionBookSequence(this);

        // wait for input to be recieved
        topNode = new Sequence<ActionSequence>(new List<Node<ActionSequence>> { waitSpawnPlayers, welcomDialogue, checkCloseEnough, firstChallengeDialogue, cycleCamera, instructionBookSequence });
        lifeParticles.Play();
    }

    // Update is called once per frame
    private void Update()
    {
        if (topNode != null)
        {
            topNode.Execute();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(oppenheimObj.transform.position, MaxDistanceAway);
    }

    private void OnEnable()
    {
        UIEvents.OnBeginAnnoucement += BuildSequenceTree;
    }

    private void OnDisable()
    {
        UIEvents.OnBeginAnnoucement -= BuildSequenceTree;
    }
}
