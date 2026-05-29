using UnityEngine;
using UnityEngine.InputSystem;

public class CarInput : MonoBehaviour
{
    [Header("Reference")]
    [Tooltip("InputActionAsset possesses Action Map 'Player' & Action 'Move'")]
    private PlayerInput inputActions;

    private CarPhysics Car;
    private Camera cam;

    private InputAction moveAction;//Action for movement
    private Vector2 MoveInput;

    private InputAction lookAction;//Action for look
    private Vector2 LookInput;

    private InputAction brakeAction;//Action for brake
    private float BrakeInput;

    private InputAction driftAction;//Action for drift
    private bool DriftInput;

    public bool canInput { get; private set; } = true;
    void Awake()
    {
        inputActions = GetComponent<PlayerInput>();
        Car ??= GetComponent<CarPhysics>();
        cam ??= FindAnyObjectByType<Camera>();
        var map = inputActions.actions.FindActionMap("CarControl");
        moveAction = map.FindAction("Move", true);
        lookAction = map.FindAction("Look", true);
        brakeAction = map.FindAction("Brake", true);
        driftAction = map.FindAction("Drift", true);
    }

    private void OnEnable()
    {
        inputActions.actions.FindActionMap("CarControl").Enable();
    }
    private void OnDisable()
    {
        inputActions.actions.FindActionMap("CarControl").Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (!canInput) return;
        if (Car == null) return;
        GetMoveinput(); 
        GetBrakeInput();
        GetDriftInput();
        if (cam != null)
        GetLookInput();


    }

    private void GetMoveinput()
    {
        //Variable get input from moveAction-Action for movement
        MoveInput = moveAction.ReadValue<Vector2>();
        Car.MoveInput(MoveInput);
    }

    private void GetLookInput()
    {
        LookInput = lookAction.ReadValue<Vector2>();
        cam.LookInput(LookInput);
    }

    private void GetBrakeInput()
    {
        BrakeInput = brakeAction.ReadValue<float>();
        Car.BrakeInput(BrakeInput);
    }

    private void GetDriftInput()
    {
        DriftInput = driftAction.IsPressed();
        Car.DriftInput(DriftInput);
    }    
}
