using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FPController : MonoBehaviour
{
    public GameObject cam;
    public GameObject stevePrefeb;
    public Slider healthbar;
    public Text ammoReserves;
    public Text ammoClipAmount;
    public Text scoreText;
    public Text highScoreText;
    public Transform shotDirection;
    public Animator anim;
    public AudioClip shot;
    public AudioClip victory;
    public AudioClip gameOver;
    public GameObject bloodPrefeb;
    public ParticleSystem muzzleFlash;

    public GameObject uiBloodPrefab;
    public GameObject gameOverPrefeb;
    public GameObject winTextPrefeb;
    public GameObject canvas;
    public Button restartButton;

    float cWidth;
    float cHeight;

    float speed = 0.1f;
    float Xsensitivity = 2;
    float Ysensitivity = 2;
    float minX = -90;
    float maxX = 90;

    int ammo = 50;
    int maxAmmo = 50;
    public float health = 100.0f;
    float maxHealth = 100.0f;
    int ammoClip = 10;
    int ammoClipMax = 10;
    int score = 0;

    Rigidbody rb;
    CapsuleCollider capsule;
    private AudioSource audioSource;

    Quaternion cameraRot;
    Quaternion characterRot;

    bool cursorIsLocked = true;
    bool lockCursor = true;

    public void TakeHit(float amount)
    {
        health = Mathf.Clamp(health - amount, 0, maxHealth);
        healthbar.value = health;

        GameObject bloodSplatter = Instantiate(uiBloodPrefab);
        bloodSplatter.transform.SetParent(canvas.transform);
        bloodSplatter.transform.position = new Vector3(Random.Range(0, cWidth), Random.Range(0, cHeight), 0);

        Destroy(bloodSplatter, 2.2f);
        
        if(health <= 0)
        {
            Vector3 pos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
            GameObject steve = Instantiate(stevePrefeb, pos, this.transform.rotation);
            steve.GetComponent<Animator>().SetTrigger("Death");
            GameStats.gameOver = true;
            audioSource.PlayOneShot(gameOver, 1.0f);
            if (!audioSource.isPlaying)
            {
                Destroy(this.gameObject);
            }

            GameObject gameOverText = Instantiate(gameOverPrefeb);
            gameOverText.transform.SetParent(canvas.transform);
            gameOverText.transform.localPosition = Vector3.zero;

            ShowRestart();

            SaveHighScore();
        }
    }

    public void SaveHighScore()
    {
        if(score > PlayerPrefs.GetInt("HighScore", 0))
        {
            PlayerPrefs.SetInt("HighScore", score);
            highScoreText.text = "High Score: " + score.ToString();
        }
    }

    public void ShowRestart()
    {
        restartButton.gameObject.SetActive(true);
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("Home"))
        {
            Vector3 pos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
            GameObject steve = Instantiate(stevePrefeb, pos, this.transform.rotation);
            steve.GetComponent<Animator>().SetTrigger("Dance");
            GameStats.gameOver = true;
            audioSource.PlayOneShot(victory, 1.0f);
            if (!audioSource.isPlaying)
            {
                Destroy(this.gameObject);
            }
            GameObject winText = Instantiate(winTextPrefeb);
            winText.transform.SetParent(canvas.transform);
            winText.transform.localPosition = Vector3.zero;

            ShowRestart();

            SaveHighScore();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        GameStats.gameOver = false;

        rb = this.GetComponent<Rigidbody>();
        capsule = this.GetComponent<CapsuleCollider>();

        cameraRot = cam.transform.localRotation;
        characterRot = this.transform.localRotation;
        
        highScoreText.text = "High Score: " + PlayerPrefs.GetInt("HighScore", 0).ToString();

        health = maxHealth;
        healthbar.value = health;

        ammoReserves.text = ammo + "";
        ammoClipAmount.text = ammoClip + "";

        cWidth = canvas.GetComponent<RectTransform>().rect.width;
        cHeight = canvas.GetComponent<RectTransform>().rect.height;
    }

    void ProcessZombieHit()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(shotDirection.position, shotDirection.forward, out hitInfo, 200))
        {
            GameObject hitZombie = hitInfo.collider.gameObject;
            if (hitZombie.gameObject.CompareTag("Zombie"))
            {
                GameObject blood = Instantiate(bloodPrefeb, hitInfo.point, Quaternion.identity);
                blood.transform.LookAt(this.transform.position);
                Destroy(blood, 1.5f);

                hitZombie.GetComponent<ZombieController>().shotsTaken++;
                if (hitZombie.GetComponent<ZombieController>().shotsTaken ==
                hitZombie.GetComponent<ZombieController>().shotsRequired)
                {
                    if (Random.Range(0, 10) < 5)
                    {
                        GameObject rdPrefab = hitZombie.GetComponent<ZombieController>().ragdoll;
                        GameObject newRD = Instantiate(rdPrefab, hitZombie.transform.position, hitZombie.transform.rotation);
                        newRD.transform.Find("Hips").GetComponent<Rigidbody>().AddForce(shotDirection.forward * 10000);
                        Destroy(hitZombie);
                    }

                    else
                    {
                        hitZombie.GetComponent<ZombieController>().KillZombie();
                    }

                    score += 10;
                    scoreText.text = "Score: " + score;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            
            if (ammoClip > 0 && health > 0)
            {
                anim.SetTrigger("fire");
                audioSource.PlayOneShot(shot, 1.0f);
                muzzleFlash.Play();
                ProcessZombieHit();
                ammoClip--;
                ammoClipAmount.text = ammoClip + "";
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            anim.SetTrigger("reload");
            int amountNeed = ammoClipMax - ammoClip;
            int ammoAvailable = amountNeed < ammo ? amountNeed : ammo;
            ammo -= ammoAvailable;
            ammoClip += ammoAvailable;
            ammoReserves.text = ammo + "";
            ammoClipAmount.text = ammoClip + "";
        }
    }

    void FixedUpdate()
    {
        float yRot = Input.GetAxis("Mouse X") * Ysensitivity;
        float xRot = Input.GetAxis("Mouse Y") * Xsensitivity;

        cameraRot *= Quaternion.Euler(-xRot, 0, 0);
        characterRot *= Quaternion.Euler(0, yRot, 0);

        cameraRot = ClampRotationAroundXAxis(cameraRot);

        this.transform.localRotation = characterRot;
        cam.transform.localRotation = cameraRot;

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.AddForce(0, 300, 0);
        }

        float x = Input.GetAxis("Horizontal") * speed;
        float z = Input.GetAxis("Vertical") * speed;

        transform.position += cam.transform.forward * z + cam.transform.right * x;

        UpdateCursorLock();
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, minX, maxX);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    bool IsGrounded()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, capsule.radius, Vector3.down, out hitInfo, (capsule.height / 2f - capsule.radius + 0.1f)))
        {
            return true;
        }
        return false;
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.CompareTag("Ammo") && ammo < maxAmmo)
        {
            ammo = Mathf.Clamp(ammo + 10, 0, maxAmmo);
            ammoReserves.text = ammo + "";
            Destroy(col.gameObject);
        }

        else if (col.gameObject.CompareTag("MedKit") && health < maxHealth)
        {
            health = Mathf.Clamp(health + 25, 0, maxHealth);
            healthbar.value = health;
            Destroy(col.gameObject);
        }
    }

    public void SetCursorLock(bool value)
    {
        lockCursor = value;
        if (!lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void UpdateCursorLock()
    {
        if (lockCursor)
        {
            InternalLockUpdate();
        }
    }

    public void InternalLockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            cursorIsLocked = false;
        }

        else if (Input.GetMouseButtonUp(0))
        {
            cursorIsLocked = true;
        }

        if (cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        else if (!cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
