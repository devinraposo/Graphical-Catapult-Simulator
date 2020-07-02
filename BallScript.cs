using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BallScript : MonoBehaviour
{
	//variable declarations
    bool launched, moving, swinging, funMode, audioOn, firstLaunch;
    public float velocity, gravity, initialY, startX, startY, time, armMass, distance, ballMass, armLength, weightMass, initialArmAngle,
        initialHeight, initialBallX, initialArmX, initialArmScale, x, initialYOffset, swingTime, swingSpeed;
    public Text launchButtonText, pauseButtonText;
    public TextMeshProUGUI timeText, currentVelocityText, initialArmAngleText, distanceText, ballMassText, currentDistanceText,
        armLengthText, armMassText, pivotElevationText, weightMassText;
    public GameObject pivot, weight, arm, currentCurve, oldCurves, curvePrefab, catapult, backArm;
    public Button pauseButton, clearFieldButton, launchButton, moreOptionsButton, helpButton, audioButton, formulaeButton;
    Quaternion pivotRotation, weightRotation;
    Vector2 weightPosition;
    public Toggle curveToggle;
    public Slider initialArmAngleSlider, distanceSlider, ballMassSlider, armLengthSlider, initialHeightSlider, armMassSlider;
    BoxCollider armCollider;
    public Sprite audioSprite, audioOffSprite;
    public AudioSource swing, grass, wind;
    int setFirstLaunch;
	//variable definitions
    void Awake()
    {
        weightRotation = weight.transform.rotation;
        launched = false;
        moving = false;
        swinging = false;
        funMode = true;
        audioOn = false;
        firstLaunch = false;
        setFirstLaunch = 0;
        velocity = 0.0f;
        time = 0.0f;
        swingTime = 0.0f;
        gravity = 9.81f;
        armMass = 0.12f; //in kg
        armLength = 0.3048f * 2.0f; //2 feet by default
        weightMass = 0.0f; //in kg
        ballMass = 0.01f; //in kg
        initialArmAngle = 0.0f;
        distance = 1.75f;
        startX = 0.0f;
        startY = 0.0f;
        swingSpeed = 0.0f;
        x = 0.0f;
        armCollider = arm.GetComponent<BoxCollider>();
        initialYOffset = 3.87f;
        initialArmAngleSlider.onValueChanged.AddListener(delegate { InitialArmAngleChangeCheck(); });
        distanceSlider.onValueChanged.AddListener(delegate { DistanceChangeCheck(); });
        ballMassSlider.onValueChanged.AddListener(delegate { BallMassChangeCheck(); });
        armLengthSlider.onValueChanged.AddListener(delegate { ArmLengthChangeCheck(); });
        initialHeightSlider.onValueChanged.AddListener(delegate { InitialHeightChangeCheck(); });
        armMassSlider.onValueChanged.AddListener(delegate { BeamMassChangeCheck(); });
        GetVelocity();
        DrawCurve();
    }
    private void FixedUpdate()
    {
		//projectile motion
        if (moving)
        {
            float y = initialY - ((1.0f / 2.0f) * gravity * Mathf.Pow(time, 2.0f));
			//if ball lands, stop, else projectile motion
            if (y <= 0.0f)
            {
                transform.position = new Vector2(x, 0.0f);
                moving = false;
                pauseButton.interactable = false;
                if (audioOn)
                {
                    wind.Stop();
                    grass.Play();
                }
                if (setFirstLaunch == 0) setFirstLaunch = 1;
            }
            else
            {
                x = velocity * time;
                transform.position = new Vector2(x, y);
                currentDistanceText.text = x.ToString("#.##");
                time += Time.unscaledDeltaTime;
                StringBuilder temp = new StringBuilder(time.ToString("#.##"));
                if (time < 1.0f) temp.Insert(0, '0');
                timeText.text = temp.ToString();
            }
        }
		//Runge-Kutta angular acceleration formula
        else if(swinging)
        {
            //returned in radians per second^2, angular acceleration so there's a constant change happening, convert
            //the return to angles with Mathf.Rad2Deg
            float angAccel = (weightMass - ballMass) * (armLength / 2.0f) * Mathf.Sin(swingTime * Mathf.Deg2Rad);
            angAccel /= (((1.0f / 12.0f) * armMass * Mathf.Pow(armLength, 2.0f)) + ((weightMass + ballMass) * ((1.0f / 2.0f) * Mathf.Pow(armLength, 2.0f))));
            angAccel *= Mathf.Rad2Deg;
            swingSpeed += angAccel;
            float z = pivot.transform.eulerAngles.z - swingSpeed;
            weight.transform.rotation = weightRotation;
            if (z <= 180.0f && z >= 1.0f)
            {
                swinging = false;
                moving = true;
                pauseButton.interactable = true;
                curveToggle.interactable = true;
                pivot.transform.eulerAngles = new Vector3(0.0f, 0.0f, -180.0f);
                x = transform.position.x;
                initialY = transform.position.y;
                if (audioOn) swing.Play();
                return;
            }
            else pivot.transform.eulerAngles = new Vector3(0.0f, 0.0f, z);
            swingTime += Time.unscaledDeltaTime;
        }
    }
    public void Launch()
    {
        launched = !launched;
        if (launched) 
        {
            //launch
            DisableUI();
            launchButton.interactable = true;
            startX = transform.position.x;
            startY = transform.position.y;
            launchButtonText.text = "Reset";
            pivotRotation = pivot.transform.rotation;
            weightPosition = weight.transform.position;
            swinging = true;
            if (audioOn) wind.Play();
        }
        else
        {
            //reset
            if(setFirstLaunch == 1)
            {
                setFirstLaunch = 2;
                firstLaunch = true;
            }
            swinging = false;
            pivot.transform.rotation = pivotRotation;
            transform.position = new Vector2(startX, startY);
            weight.transform.position = weightPosition;
            moving = false;
            launchButtonText.text = "Launch";
            time = 0.0f;
            swingTime = 0.0f;
            swingSpeed = 0.0f;
            timeText.text = "0.00";
            currentDistanceText.text = "0.00";
            EnableUI();
            pauseButton.interactable = false;
            pauseButtonText.text = "Pause";
            currentCurve.SetActive(true);
            currentCurve.GetComponent<LineRenderer>().startColor = Color.gray;
            currentCurve.GetComponent<LineRenderer>().endColor = Color.gray;
            currentCurve.transform.parent = oldCurves.transform;
            currentCurve = Instantiate(curvePrefab);
            DrawCurve();
            if (curveToggle.isOn) currentCurve.SetActive(true);
        }
    }
    public void Pause()
    {
        moving = !moving;
        if (!moving) pauseButtonText.text = "Unpause";
        else pauseButtonText.text = "Pause";
    }
    public void ToggleCurvePreview()
    {
        if (curveToggle.isOn) currentCurve.SetActive(true);
        else currentCurve.SetActive(false);
    }
	//Handling sliders
    public void InitialArmAngleChangeCheck()
    {
        initialArmAngle = Mathf.Ceil(initialArmAngleSlider.value * 178.0f) + 1.0f;
        StringBuilder temp = new StringBuilder(initialArmAngle.ToString("#.#"));
        if (initialArmAngle == 0.0f) temp.Append("0");
        temp.Append("°");
        initialArmAngleText.text = temp.ToString();
        GetVelocity();
        DrawCurve();
        pivot.transform.eulerAngles = new Vector3(0.0f, 0.0f, -initialArmAngle);
        weight.transform.rotation = weightRotation;
    }
    //farthest is 100 meters
    public void DistanceChangeCheck()
    {
        distance = (distanceSlider.value * 98.25f) + 1.75f;
        StringBuilder temp = new StringBuilder(distance.ToString("#.##") + " m");
        distanceText.text = temp.ToString();
        GetVelocity();
        DrawCurve();
    }
    //10 grams to 2 kg
    public void BallMassChangeCheck()
    {
        ballMass = (ballMassSlider.value * 1.99f) + 0.01f;
        if (ballMass < 1.0f)
        {
            float val = ballMass * 1000.0f;
            StringBuilder temp = new StringBuilder(val.ToString("#") + " g");
            ballMassText.text = temp.ToString();
        }
        else
        {
            StringBuilder temp = new StringBuilder(ballMass.ToString("#.##") + " kg");
            ballMassText.text = temp.ToString();
        }
        GetVelocity();
        DrawCurve();
    }
	//Draw a curve using CurveRenderer
    void DrawCurve()
    {
        currentCurve.transform.GetChild(0).position = new Vector2(0.0f, arm.transform.position.y + initialYOffset);
        float tempX = Mathf.Sqrt((-2.0f * (0.0f - (arm.transform.position.y + initialYOffset)) * Mathf.Pow(velocity, 2.0f)) / gravity);
        float tempY = (arm.transform.position.y + initialYOffset) - ((1.0f / 2.0f) * gravity * Mathf.Pow((tempX / 2.0f) / velocity, 2.0f));
        currentCurve.transform.GetChild(1).position = new Vector2(tempX / 2.0f, tempY);
        currentCurve.transform.GetChild(2).position = new Vector2(tempX, 0.0f);
    }
    //also calculates the weight of the mass, only called on slider drag up
    void GetVelocity()
    {
        weightMass = armLength * (1.0f + Mathf.Cos(initialArmAngle * Mathf.Deg2Rad)) * ballMass
         + ((Mathf.Pow(distance, 2.0f) / (2.0f * armLength)) * (((1.0f / 2.0f) * ballMass) + ((1.0f / 3.0f) * armMass)));
        weightMass /= ((armLength * (1.0f + Mathf.Cos(initialArmAngle * Mathf.Deg2Rad))) - ((1.0f / 4.0f) * ((Mathf.Pow(distance, 2.0f)) / (4.0f * armLength))));
        velocity = (ballMass * (-gravity) * ((armLength / 2.0f) + ((armLength / 2.0f) * Mathf.Cos(initialArmAngle * Mathf.Deg2Rad))));
        velocity -= (weightMass * (-gravity) * ((armLength / 2.0f) + ((armLength / 2.0f) * Mathf.Cos(initialArmAngle * Mathf.Deg2Rad))));
        velocity /= (((1.0f / 2.0f) * weightMass) + ((1.0f / 2.0f) * ballMass) + ((1.0f / 3.0f) * armMass));
        velocity = Mathf.Sqrt(velocity);
        if (funMode) velocity *= 5.0f;
        currentVelocityText.text = velocity.ToString("#.##");
        float temp = weightMass;
        if (temp < 0.0f) temp *= -1.0f;
        weightMassText.text = temp.ToString("#.##");
    }
    //optional sliders
    //between 1 and 5 feet (0.3048m - 1.524m), default is 2ft (0.6096m)
    public void ArmLengthChangeCheck()
    {
        armLength = (armLengthSlider.value * 1.2192f) + 0.3048f;
        GetVelocity();
        DrawCurve();
        StringBuilder temp = new StringBuilder(armLength.ToString("#.####") + " m");
        armLengthText.text = temp.ToString();
    }
    //between 0.12kg and 1.0kg, default is 0.12kg
    public void BeamMassChangeCheck()
    {
        armMass = (armMassSlider.value * 0.88f) + 0.12f;
        GetVelocity();
        DrawCurve();
        StringBuilder temp = new StringBuilder(armMass.ToString("#.##") + " kg");
        armMassText.text = temp.ToString();
    }
    //between 0m and 100m, default is 0m
    public void InitialHeightChangeCheck()
    {
        float y = (initialHeightSlider.value * 12.6f) + 3.53f;
        pivot.transform.position = new Vector2(pivot.transform.position.x, y);
        DrawCurve();
        y += 3.87f;
        backArm.transform.position = new Vector2(backArm.transform.position.x, (initialHeightSlider.value * 19.57f) - 8.53f);
        StringBuilder temp = new StringBuilder(y.ToString("#.#") + " m");
        pivotElevationText.text = temp.ToString();
    }
	//clear the line curves from the field
    public void ClearField()
    {
        foreach (Transform child in oldCurves.transform) Destroy(child.gameObject);
        firstLaunch = false;
        setFirstLaunch = 0;
    }
    public void AudioSwitch()
    {
        audioOn = !audioOn;
        if (audioOn)
        {
            audioButton.GetComponent<Image>().sprite = audioSprite;
            //if (!music.isPlaying) music.Play();
            //else music.UnPause();
        }
        else
        {
            audioButton.GetComponent<Image>().sprite = audioOffSprite;
            //music.Pause();
        }
    }
    public void EnableUI()
    {
        if(firstLaunch) clearFieldButton.interactable = true;
        initialArmAngleSlider.interactable = true;
        distanceSlider.interactable = true;
        ballMassSlider.interactable = true;
        curveToggle.interactable = true;
        moreOptionsButton.interactable = true;
        launchButton.interactable = true;
        helpButton.interactable = true;
        formulaeButton.interactable = true;
    }
    public void DisableUI()
    {
        clearFieldButton.interactable = false;
        initialArmAngleSlider.interactable = false;
        distanceSlider.interactable = false;
        ballMassSlider.interactable = false;
        curveToggle.interactable = false;
        moreOptionsButton.interactable = false;
        launchButton.interactable = false;
        helpButton.interactable = false;
        formulaeButton.interactable = false;
    }
}