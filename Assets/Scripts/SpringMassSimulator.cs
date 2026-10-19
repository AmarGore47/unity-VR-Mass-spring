using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SpringMassSimulator : MonoBehaviour
{
    [Header("1. Drag Objects From Hierarchy Here")]
    [Tooltip("Hierarchy se 'Helical_Spring' yahan drag karo")]
    public Transform helicalSpringMesh;

    [Tooltip("Hierarchy se 'Weight_Snap_Socket' yahan drag karo")]
    public Transform massHanger;

    [Tooltip("Agar Red Pointer needle hai toh 'Scale_Red_Pointer_Needle' yahan drag karo (Optional)")]
    public Transform redPointerNeedle;

    [Header("2. Spring Physics Settings")]
    public float springConstantK = 25f;      // Spring stiffness (N/m)
    public float dampingB = 0.45f;           // Rokne wali friction
    public float hangerBaseMassKg = 0.05f;   // Khali hanger ka weight (50g)
    public float gravity = 9.81f;

    [Header("3. Live Practical Output (Play mode mein dekho)")]
    public float attachedMassKg = 0f;
    public float currentExtensionCm = 0f;
    public float timePeriodSeconds = 0f;

    private XRSocketInteractor socket;
    private Vector3 springStartScale;
    private Vector3 hangerStartLocalPos;
    private Vector3 pointerStartLocalPos;
    private float naturalSpringLength = 0.12f;
    private float displacementY = 0f;
    private float velocityY = 0f;

    void Awake()
    {
        // Khud hi Weight_Snap_Socket par laga XRSocketInteractor dhoondh lega!
        socket = GetComponent<XRSocketInteractor>();
        if (socket == null && massHanger != null)
            socket = massHanger.GetComponentInChildren<XRSocketInteractor>();

        if (massHanger == null)
            massHanger = this.transform;
    }

    void OnEnable()
    {
        if (socket != null)
        {
            socket.selectEntered.AddListener(OnWeightAttached);
            socket.selectExited.AddListener(OnWeightDetached);
        }
    }

    void OnDisable()
    {
        if (socket != null)
        {
            socket.selectEntered.RemoveListener(OnWeightAttached);
            socket.selectExited.RemoveListener(OnWeightDetached);
        }
    }

    void Start()
    {
        if (helicalSpringMesh != null)
            springStartScale = helicalSpringMesh.localScale;

        if (massHanger != null)
            hangerStartLocalPos = massHanger.localPosition;

        if (redPointerNeedle != null)
            pointerStartLocalPos = redPointerNeedle.localPosition;
    }

    void FixedUpdate()
    {
        float totalMass = hangerBaseMassKg + attachedMassKg;

        // Physics Formula: F = m*g - k*x - b*v
        float weightForce = totalMass * gravity;
        float restoringForce = -springConstantK * displacementY;
        float dampingForce = -dampingB * velocityY;

        float netForce = weightForce + restoringForce + dampingForce;
        float acceleration = netForce / totalMass;

        velocityY += acceleration * Time.fixedDeltaTime;
        displacementY += velocityY * Time.fixedDeltaTime;

        if (displacementY < -0.08f)
        {
            displacementY = -0.08f;
            velocityY = 0f;
        }

        currentExtensionCm = displacementY * 100f;
        timePeriodSeconds = 2f * Mathf.PI * Mathf.Sqrt(totalMass / springConstantK);

        // 1. Helical_Spring ko lamba (stretch) karo
        if (helicalSpringMesh != null)
        {
            float stretchRatio = (naturalSpringLength + displacementY) / naturalSpringLength;
            helicalSpringMesh.localScale = new Vector3(
                springStartScale.x,
                springStartScale.y * Mathf.Max(0.2f, stretchRatio),
                springStartScale.z
            );
        }

        // 2. Weight_Snap_Socket (Hanger) ko niche move karo
        if (massHanger != null)
        {
            massHanger.localPosition = hangerStartLocalPos + new Vector3(0f, -displacementY, 0f);
        }

        // 3. Scale_Red_Pointer_Needle ko bhi saath mein niche move karo
        if (redPointerNeedle != null)
        {
            redPointerNeedle.localPosition = pointerStartLocalPos + new Vector3(0f, -displacementY, 0f);
        }
    }

    private void OnWeightAttached(SelectEnterEventArgs args)
    {
        LabWeight w = args.interactableObject.transform.GetComponent<LabWeight>();
        if (w != null)
        {
            attachedMassKg += w.MassInKg;
        }
    }

    private void OnWeightDetached(SelectExitEventArgs args)
    {
        LabWeight w = args.interactableObject.transform.GetComponent<LabWeight>();
        if (w != null)
        {
            attachedMassKg = Mathf.Max(0f, attachedMassKg - w.MassInKg);
        }
    }
}