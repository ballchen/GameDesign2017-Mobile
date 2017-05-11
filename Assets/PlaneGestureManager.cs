using UnityEngine;
using System.Collections;
using TouchScript.Gestures;
using TouchScript.Hit;
using DG.Tweening;


public class PlaneGestureManager : MonoBehaviour
{

    public TapGesture singleTap;
    public TapGesture doubleTap;
    public ScreenTransformGesture ManipulationGesture;
    public FlickGesture flickGesture;

    public float PanSpeed = 200f;
    public float RotationSpeed = 200f;
    public float ZoomSpeed = 10f;

    public int displayModel = 0;

    public Animator modelAnimator;
    public GameObject[] models;


    public Rigidbody Box;


    float accelerometerUpdateInterval = 1.0f / 60.0f;
    // The greater the value of LowPassKernelWidthInSeconds, the slower the filtered value will converge towards current input sample (and vice versa).
    float lowPassKernelWidthInSeconds = 1.0f;
    // This next parameter is initialized to 2.0 per Apple's recommendation, or at least according to Brady! ;)
    float shakeDetectionThreshold = 4.0f;

    private float lowPassFilterFactor;
    private Vector3 lowPassValue = Vector3.zero;
    private Vector3 acceleration;
    private Vector3 deltaAcceleration;

    private Transform pivot;
    private Transform cam;

    private void Awake()
    {
        pivot = transform.Find("Pivot");
        cam = transform.Find("Pivot/Camera");
    }

    private void OnEnable()
    {
        ManipulationGesture.Transformed += manipulationTransformedHandler;
        flickGesture.Flicked += (object sender, System.EventArgs e) =>
        {
            TouchHit hit;
            singleTap.GetTargetHitResult(out hit);
            Debug.Log("Flick");

            if(displayModel < models.Length-1)
            {
                displayModel++;
            } else if(displayModel >= models.Length -1)
            {
                displayModel = 0;
            }

            foreach(GameObject obj in models)
            {
               
                obj.SetActive(false);
            }

            models[displayModel].SetActive(true);
        };

    }

    private void manipulationTransformedHandler(object sender, System.EventArgs e)
    {
        var rotation = Quaternion.Euler(ManipulationGesture.DeltaPosition.y / Screen.height * RotationSpeed,
                -ManipulationGesture.DeltaPosition.x / Screen.width * RotationSpeed,
                ManipulationGesture.DeltaRotation);
        pivot.localRotation *= rotation;
        cam.transform.localPosition += Vector3.forward * (ManipulationGesture.DeltaScale - 1f) * ZoomSpeed;
    }


    // Use this for initialization
    void Start()
    {

        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
        shakeDetectionThreshold *= shakeDetectionThreshold;
        lowPassValue = Input.acceleration;

        singleTap.Tapped += (object sender, System.EventArgs e) =>
        {
            TouchHit hit;
            singleTap.GetTargetHitResult(out hit);
            Debug.Log("One");

            models[displayModel].GetComponent<Animator>().SetTrigger("OneTap");
        };

        doubleTap.Tapped += (object sender, System.EventArgs e) =>
        {
            TouchHit hit;
            doubleTap.GetTargetHitResult(out hit);

            Debug.Log("Two");

            models[displayModel].GetComponent<Animator>().SetTrigger("TwoTap");
        };
    }

    void Update()
    {
        acceleration = Input.acceleration;
        lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
        deltaAcceleration = acceleration - lowPassValue;
        if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold)
        {
            Debug.Log("Shake");
            models[displayModel].GetComponent<Animator>().SetTrigger("Shake");
        }
    }
}
