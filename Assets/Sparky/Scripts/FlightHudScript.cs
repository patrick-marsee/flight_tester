using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FlightHudScript : MonoBehaviour {

    public enum VelocityUnit { metersPerSecond, feetPerSecond, knots, milesPerHour, kilometersPerHour }
    public enum DistanceUnit { meters, feet }
    private float[] VEL_CONVERSIONS = new float[5] { 1f, 3.28084f, 1.94384f, 2.23694f, 3.6f };
    private float[] DIST_CONVERSIONS = new float[2] { 1f, 3.28084f };

    public VelocityUnit velocityUnit = VelocityUnit.metersPerSecond;
    public DistanceUnit distanceUnit = DistanceUnit.meters;

    [SerializeField]
    private GameObject player;
    [SerializeField]
    private Color32 hudColor;
    [SerializeField]
    private GameObject angleRule;
    private Atmosphere atmo;
    private Text speedometer;
    private Text tasMeter;
    private Text thrustMeter;
    private Text machMeter;
    private Text altimeter;
    private Image vector;
    private GameObject pitch, roll;
    private ILiftingBody playerPlane;
    private Vector2 screenres;

    private float aspectRatioCorrection; // This keeps the velocity vector image correct on the x-axis.

	// Use this for initialization
	void Start () {
        playerPlane = player.GetComponent<ILiftingBody>();
        speedometer = GetComponentInChildren<Text>();
        vector = GameObject.Find("Vector").GetComponent<Image>();
        atmo = FindObjectOfType<Atmosphere>();
        pitch = GameObject.Find("Pitch");
        roll = GameObject.Find("Roll");
        playerPlane.ConnectThrustSet(UpdateThrustMeter);

        Rect getRect = GetComponent<RectTransform>().rect;
        screenres = getRect.size;
        aspectRatioCorrection = getRect.height / getRect.width;
        pitch.GetComponent<RectTransform>().sizeDelta = new Vector2(pitch.GetComponent<RectTransform>().sizeDelta.x, screenres.y * 3);

        for (int i = 1; i <= 18; i++)
        {
            GameObject nextAngleRule = Instantiate(angleRule, pitch.transform);
            Text[] nums = nextAngleRule.GetComponentsInChildren<Text>();
            for (int j = 0; j < nums.Length; j++)
                nums[j].text = (-i * 5).ToString();
            RectTransform rt = nextAngleRule.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f - i / 36f);
            rt.anchorMax = new Vector2(1, 0.5f - i / 36f);
        }
        for (int i = 1; i <= 18; i++)
        {
            GameObject nextAngleRule = Instantiate(angleRule, pitch.transform);
            Text[] nums = nextAngleRule.GetComponentsInChildren<Text>();
            for (int j = 0; j < nums.Length; j++)
                nums[j].text = (i * 5).ToString();
            RectTransform rt = nextAngleRule.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f + i / 36f);
            rt.anchorMax = new Vector2(1, 0.5f + i / 36f);
        }

        Image[] images = GetComponentsInChildren<Image>();
        Text[] texts = GetComponentsInChildren<Text>();
        tasMeter = texts[1];
        thrustMeter = texts[2];
        machMeter = texts[3];
        altimeter = texts[4];
        for (int i = 0; i < images.Length; i++) images[i].color = hudColor;
        for (int i = 0; i < texts.Length; i++) texts[i].color = hudColor;
	}
	
	// Update is called once per frame
	void Update () {
        speedometer.text = string.Format("{0:F1}", playerPlane.ias * VEL_CONVERSIONS[(int)velocityUnit]);
        tasMeter.text = string.Format("True Air Speed: {0:F1}", playerPlane.tas * VEL_CONVERSIONS[(int)velocityUnit]);
        machMeter.text = string.Format("{0:F2}", atmo.Mach(player.transform.position.y, playerPlane.tas));
        altimeter.text = string.Format("Alt: {0:F1}", player.transform.position.y * atmo.Scale * DIST_CONVERSIONS[(int)distanceUnit]);
        //float xAngle = Vector3.Angle(Vector2.right, new Vector2(playerPlane.velocity.z, playerPlane.velocity.x)) * Mathf.Sign(playerPlane.velocity.x);
        //float yAngle = Vector3.Angle(Vector2.right, new Vector2(playerPlane.velocity.z, playerPlane.velocity.y)) * Mathf.Sign(playerPlane.velocity.y);
        float xAngle = playerPlane.sideslip * Mathf.Rad2Deg;
        float yAngle = -playerPlane.AoA * Mathf.Rad2Deg;
        vector.rectTransform.anchorMin = new Vector2(xAngle / 60 * aspectRatioCorrection + 0.5f, yAngle / 60 + 0.5f);
        vector.rectTransform.anchorMax = new Vector2(xAngle / 60 * aspectRatioCorrection + 0.5f, yAngle / 60 + 0.5f);

        float pitchCoeff = screenres.y / 60f;
        float actualPitch = player.transform.rotation.eulerAngles.x;
        if (actualPitch > 180) actualPitch = actualPitch - 360;
        pitch.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.5f, actualPitch * pitchCoeff);
        roll.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, -player.transform.rotation.eulerAngles.z);
        //vector.rectTransform.anchoredPosition = new Vector2(xAngle * 8, yAngle * 8);
        //vector.rectTransform.position = new Vector3(playerPlane.velocity.x, -playerPlane.velocity.y, 0);
	}
    
    void UpdateThrustMeter(float aThrust)
    {
        thrustMeter.text = string.Format("Total Thrust: {0:F1} kN", aThrust * 0.001f);
    }
}
