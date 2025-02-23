using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    // Reference to the sliders in the UI
    public Slider radiusSlider;
    public Slider pointAmountSlider;
    public Slider moveSpeedSlider;

    // Reference to the RuleManager and PlacePoints scripts
    public PlacePoints placePoints;
    public RuleManager ruleManager;

    // Reference to the Toggle (Checkbox) for SimulateTriangleRule
    public Toggle simulateTriangleToggle;

    // Reference to the Toggle (Checkbox) for moveCamera
    public Toggle moveCameraToggle;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize slider values with current values from PlacePoints and RuleManager
        radiusSlider.value = placePoints.radius;
        pointAmountSlider.value = placePoints.points;
        moveSpeedSlider.value = ruleManager.moveSpeed;

        // Add listeners for value changes on the sliders
        radiusSlider.onValueChanged.AddListener(UpdateRadius);
        moveSpeedSlider.onValueChanged.AddListener(UpdateMoveSpeed);
        pointAmountSlider.onValueChanged.AddListener((value) => UpdatePointAmount((int)value));

        // Initialize the toggle value based on SimulateTriangleRule and moveCamera in RuleManager
        simulateTriangleToggle.isOn = ruleManager.SimulateTriangleRule;
        moveCameraToggle.isOn = ruleManager.moveCamera;

        // Add listeners for value changes on the toggles
        simulateTriangleToggle.onValueChanged.AddListener(UpdateSimulateTriangleRule);
        moveCameraToggle.onValueChanged.AddListener(UpdateMoveCamera);
    }

    // Update the radius variable in PlacePoints when the slider value changes
    void UpdateRadius(float value)
    {
        placePoints.radius = value;  // Update the radius in PlacePoints
    }

    // Update the moveSpeed variable in RuleManager when the slider value changes
    void UpdateMoveSpeed(float value)
    {
        ruleManager.moveSpeed = value;  // Update the moveSpeed in RuleManager
    }

    // Update the pointAmount variable in PlacePoints when the slider value changes
    void UpdatePointAmount(int value)
    {
        placePoints.points = value;  // Update the points in PlacePoints
    }

    // Update the SimulateTriangleRule boolean in RuleManager when the toggle state changes
    void UpdateSimulateTriangleRule(bool value)
    {
        ruleManager.SimulateTriangleRule = value;  // Update the SimulateTriangleRule in RuleManager
    }

    // Update the moveCamera boolean in RuleManager when the toggle state changes
    void UpdateMoveCamera(bool value)
    {
        ruleManager.moveCamera = value;  // Update the moveCamera in RuleManager
    }
}
