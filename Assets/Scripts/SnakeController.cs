using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SnakeController : MonoBehaviour
{
    Rigidbody rigidBody;
    Vector3 moveAmount;
    Touch theTouch;
    Vector2 touchPosition;
    public bool movingForward; //Automatically move forward
    public LayerMask layerMask;
    [SerializeField] float forwardSpeed;
    [SerializeField] float feverSpeed;
    [SerializeField] float sidewaySpeed;
    List<GameObject> tailSegments = new List<GameObject>();
    [SerializeField] int tailSegmentAmount;
    float tSA;
    int foodEaten;
    [SerializeField] Text foodEatenText;
    [SerializeField] GameObject tailSegmentPrefab;
    [SerializeField] float maxTailSegmentDistance;  //Max value
    [SerializeField] Collider eatCone;
    [SerializeField] int maxCrystalAmount;
    [SerializeField] int crystalAmount;
    bool fever;
    public float feverTimer;
    bool invincible;
    int colorIndex;     //Used in ColorChange();
    [SerializeField] float colorChangeTimer;
    Vector3 snakeZPosDif;   //difference between position of snake and camera
    [SerializeField] Transform SnakeHitPos;
    [SerializeField] Transform SnakeMoveTo;
    [SerializeField] Text crystalText;
    bool snakeMoveToSet; //SnakeMoveTo position has been set
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        snakeZPosDif = transform.position - Camera.main.transform.position;
        tSA = tailSegmentAmount;
        tailSegments.Add(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (tailSegments.Count < tailSegmentAmount) {
            GameObject obj = Instantiate(tailSegmentPrefab,
                tailSegments[tailSegments.Count - 1].transform.position
                - tailSegments[tailSegments.Count - 1].transform.forward * maxTailSegmentDistance,
                transform.rotation);
            obj.SetActive(true);
            obj.GetComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
            tailSegments.Add(obj);
        }
        if (invincible)
        {
            GetComponent<BoxCollider>().enabled = false;
        } else
        {
            GetComponent <BoxCollider>().enabled = true;
        }
        if (Input.GetMouseButton(0) && !fever)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, layerMask))
            {
                SnakeHitPos.transform.position = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                transform.LookAt(tailSegments[1].transform.position + tailSegments[1].transform.forward * 10f);
                if (!snakeMoveToSet)
                {
                    SnakeMoveTo.position = transform.position;
                    snakeMoveToSet = true;
                }
                SnakeMoveTo.position = new Vector3(SnakeMoveTo.position.x,transform.position.y,
                    Camera.main.transform.position.z + snakeZPosDif.z);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            snakeMoveToSet = false;
        }
    }
    private void FixedUpdate()
    {
        Vector3 curVel = Vector3.zero;
        Vector3 camCurVel = Vector3.zero;
        if (snakeMoveToSet)
        {
            rigidBody.MovePosition(Vector3.SmoothDamp(transform.position, SnakeMoveTo.position,
               ref curVel, 0.05f, sidewaySpeed));
        } else
        {
            transform.LookAt(transform.position + Vector3.forward * 5f);
        }
        if (movingForward && !fever)
        {
            Camera.main.transform.position =
                    Vector3.SmoothDamp(Camera.main.transform.position,
                    Camera.main.transform.position + Vector3.forward * forwardSpeed * Time.fixedDeltaTime,
                    ref camCurVel, 0.05f, forwardSpeed);
            //Vector3 snakeZpos = Camera.main.transform.position + snakeZPosDif;
            rigidBody.MovePosition(
                Vector3.SmoothDamp(transform.position,
                transform.position + Vector3.forward * forwardSpeed * Time.fixedDeltaTime,
                ref curVel, 0.05f, forwardSpeed));
        }
        else if (movingForward && fever)
        {
            rigidBody.MovePosition(Vector3.SmoothDamp(transform.position, new Vector3(0f, transform.position.y, transform.position.z), ref curVel, 0.05f, forwardSpeed));
            rigidBody.MovePosition(
                Vector3.SmoothDamp(transform.position,
                transform.position + Vector3.forward * feverSpeed * Time.fixedDeltaTime, ref curVel, 0.05f, forwardSpeed));
            Camera.main.transform.position =
                    Vector3.SmoothDamp(Camera.main.transform.position,
                    Camera.main.transform.position + Vector3.forward * feverSpeed * Time.fixedDeltaTime, ref camCurVel, 0.05f, forwardSpeed);
            transform.LookAt(transform.position + Vector3.forward * 5f);
        }
        TailLogic();
    }
    void TailLogic()
    {
        for (int i = 0; i < tailSegments.Count; i++)
        {
            if (i != 0)
            {
                float tailSegDst =
                    Vector3.Distance(tailSegments[i].transform.position, tailSegments[i - 1].transform.position); //Current value
                if (tailSegDst > maxTailSegmentDistance)
                {
                    tailSegments[i].transform.LookAt(tailSegments[i - 1].transform.position);
                    Rigidbody rb = tailSegments[i].GetComponent<Rigidbody>();
                    Vector3 curVel = Vector3.zero;
                    rb.MovePosition(rb.transform.position + tailSegments[i].transform.forward * (tailSegDst - maxTailSegmentDistance));
                }
            }
        }
    }

    IEnumerator FeverTimer()
    {
        while (fever)
        {
            print("Fever Start");
            yield return new WaitForSeconds(feverTimer);
            crystalAmount = 0;
            fever = false;
            crystalText.text = crystalAmount.ToString();
            invincible = true;
            print("Fever End");
            StartCoroutine(FeverShuttingDown());
        }
    }
    IEnumerator FeverShuttingDown()
    {
        while (true)
        {
            yield return new WaitForSeconds(feverTimer / feverTimer);
            invincible = false;
        }
    }
    void Die()
    {
        gameObject.SetActive(false);
        SceneManager.LoadScene(0);
    }
    private void OnCollisionEnter(Collision collision)  //Eating mechanic, snake eats when food touches head, if not food -> die
    {
        if (collision.gameObject.tag != "crystal" && !fever)
        {
            Color ourColor = GetComponent<MeshRenderer>().material.color;
            Color foodColor = collision.gameObject.GetComponent<MeshRenderer>().material.color;
            if (collision.gameObject.tag == "edible" && ourColor == foodColor)
            {
                collision.gameObject.SetActive(false);
                tSA += 0.35f;
                foodEaten++;
                foodEatenText.text = foodEaten.ToString();
                tailSegmentAmount = (int)tSA;
            }
            else
            {
                Die();
            }
        } else if (collision.gameObject.tag == "crystal" && !fever)      //If you're eating a crystal
        {
            collision.gameObject.SetActive(false);
            crystalAmount++;
            if(crystalAmount >= maxCrystalAmount && !fever)   //When you ate enough crystals, activate fever
            {
                fever = true;
                StartCoroutine(FeverTimer());
            }
            crystalText.text = crystalAmount.ToString();
        }
        if (fever)
        {   
            if(collision.gameObject.tag == "edible")
            {
                foodEaten++;
                foodEatenText.text = foodEaten.ToString();
            }
            collision.gameObject.SetActive(false);
            tSA += 0.35f;
            tailSegmentAmount = (int)tSA;
        }
    }
    private void OnTriggerStay(Collider other)  //Sucking mechanic, sucks food towards head on "eat_cone" trigger stay
    {
        Color ourColor = GetComponent<MeshRenderer>().material.color;
        Color foodColor = other.GetComponent<MeshRenderer>().material.color;
        if (other.tag == "edible" && ourColor == foodColor)
        {
            if (other.transform.parent != null)
            {
                Vector3 curVel = Vector3.zero;
                other.transform.parent.position =
                    Vector3.SmoothDamp(other.gameObject.transform.position, transform.position, ref curVel, 0.1f);
            }
            else
            {
                Vector3 curVel = Vector3.zero;
                other.gameObject.transform.position =
                    Vector3.SmoothDamp(other.gameObject.transform.position, transform.position, ref curVel, 0.1f);
            }
        }
    }
    IEnumerator ColorChange() //Change the color of each segment individually with a delay
    {
        yield return new WaitForSeconds(colorChangeTimer);
        colorIndex++;
        if (colorIndex < tailSegments.Count)
        {
            MeshRenderer meshRen = tailSegments[colorIndex].GetComponent<MeshRenderer>();
            Material newMaterial = GetComponent<MeshRenderer>().material;
            meshRen.material = newMaterial;
            StartCoroutine(ColorChange());
        } else
        {
            colorIndex = 0;
        }
    }
    private void OnTriggerExit(Collider other)  //Change colors when on "eat_cone" trigger exit
    {
        if(other.tag == "ColorChanger")
        {
            MeshRenderer meshRen = GetComponent<MeshRenderer>();
            Material newMaterial = other.GetComponent<MeshRenderer>().material;
            meshRen.material = newMaterial;
            StartCoroutine(ColorChange());
        }
    }
}
