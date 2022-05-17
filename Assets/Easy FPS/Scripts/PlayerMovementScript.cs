using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementScript : MonoBehaviour
{
    Rigidbody _rigidbody;

    [Tooltip("Current players speed")] public float currentSpeed;
    
    private Transform _cameraMain;

    [FormerlySerializedAs("jumpForce")]
    [Tooltip("Force that moves player into jump")]
    [SerializeField]
    private float _jumpForce = 500;

    [Tooltip("Position of the camera inside the player")] [HideInInspector]
    public Vector3 cameraPosition;

    /*
     * Getting the Players rigidbody component.
     * And grabbing the mainCamera from Players child transform.
     */
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _cameraMain = transform.Find("Main Camera").transform;
        _bulletSpawn = _cameraMain.Find("BulletSpawn").transform;
        _ignoreLayer = 1 << LayerMask.NameToLayer("Player");
    }

    private Vector3 _slowdownV;

    private Vector2 _horizontalMovement;

    /*
    * Raycasting for meele attacks and input movement handling here.
    */
    void FixedUpdate()
    {
        RaycastForMeleeAttacks();

        PlayerMovementLogic();
    }

    /*
    * Accordingly to input adds force and if magnitude is bigger it will clamp it.
    * If player leaves keys it will deaccelerate
    */
    void PlayerMovementLogic()
    {
        var rigidbodyVelocity = _rigidbody.velocity;
        currentSpeed = rigidbodyVelocity.magnitude;
        _horizontalMovement = new Vector2(rigidbodyVelocity.x, rigidbodyVelocity.z);
        // you can not have a speed that is bigger than the maxSpeed
        if (_horizontalMovement.magnitude > maxSpeed)
        {
            _horizontalMovement = _horizontalMovement.normalized * maxSpeed;
        }

        _rigidbody.velocity = new Vector3(
            _horizontalMovement.x,
            _rigidbody.velocity.y,
            _horizontalMovement.y
        );
        if (grounded)
        {
            _rigidbody.velocity = Vector3.SmoothDamp(_rigidbody.velocity,
                new Vector3(0, rigidbodyVelocity.y, 0),
                ref _slowdownV,
                deaccelerationSpeed);
        }

        if (grounded)
        {
            _rigidbody.AddRelativeForce(Input.GetAxis("Horizontal") * accelerationSpeed * Time.deltaTime, 0,
                Input.GetAxis("Vertical") * accelerationSpeed * Time.deltaTime);
        }
        else
        {
            _rigidbody.AddRelativeForce(Input.GetAxis("Horizontal") * accelerationSpeed / 2 * Time.deltaTime, 0,
                Input.GetAxis("Vertical") * accelerationSpeed / 2 * Time.deltaTime);
        }

        /*
         * Slippery issues fixed here
         */
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            deaccelerationSpeed = 0.5f;
        }
        else
        {
            deaccelerationSpeed = 0.1f;
        }
    }

    /*
    * Handles jumping and ads the force and sounds.
    */
    void Jumping()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            _rigidbody.AddRelativeForce(Vector3.up * _jumpForce);
            if (_jumpSound)
                _jumpSound.Play();
            else
                print("Missig jump sound.");
            _walkSound.Stop();
            _runSound.Stop();
        }
    }

    /*
    * Update loop calling other stuff
    */
    void Update()
    {
        Jumping();

        Crouching();

        WalkingSound();
    } //end update

    /*
    * Checks if player is grounded and plays the sound accorindlgy to his speed
    */
    void WalkingSound()
    {
        if (_walkSound && _runSound)
        {
            if (RayCastGrounded())
            {
                //for walk sounsd using this because suraface is not straigh			
                if (currentSpeed > 1)
                {
                    //				print ("unutra sam");
                    if (maxSpeed == 3)
                    {
                        //	print ("tu sem");
                        if (!_walkSound.isPlaying)
                        {
                            //	print ("playam hod");
                            _walkSound.Play();
                            _runSound.Stop();
                        }
                    }
                    else if (maxSpeed == 5)
                    {
                        //	print ("NE tu sem");

                        if (!_runSound.isPlaying)
                        {
                            _walkSound.Stop();
                            _runSound.Play();
                        }
                    }
                }
                else
                {
                    _walkSound.Stop();
                    _runSound.Stop();
                }
            }
            else
            {
                _walkSound.Stop();
                _runSound.Stop();
            }
        }
        else
        {
            print("Missing walk and running sounds.");
        }
    }

    /*
    * Raycasts down to check if we are grounded along the gorunded method() because if the
    * floor is curvy it will go ON/OFF constatly this assures us if we are really grounded
    */
    private bool RayCastGrounded()
    {
        RaycastHit groundedInfo;
        if (Physics.Raycast(transform.position, transform.up * -1f, out groundedInfo, 1, ~_ignoreLayer))
        {
            Debug.DrawRay(transform.position, transform.up * -1f, Color.red, 0.0f);
            if (groundedInfo.transform != null)
            {
                //print ("vracam true");
                return true;
            }
            else
            {
                //print ("vracam false");
                return false;
            }
        }
        //print ("nisam if dosao");

        return false;
    }

    /*
    * If player toggle the crouch it will scale the player to appear that is crouching
    */
    void Crouching()
    {
        if (Input.GetKey(KeyCode.C))
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1, 0.6f, 1), Time.deltaTime * 15);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1, 1, 1), Time.deltaTime * 15);
        }
    }


    [Tooltip("The maximum speed you want to achieve")]
    public int maxSpeed = 5;

    [Tooltip("The higher the number the faster it will stop")]
    public float deaccelerationSpeed = 15.0f;


    [Tooltip("Force that is applied when moving forward or backward")]
    public float accelerationSpeed = 50000.0f;


    [Tooltip("Tells us weather the player is grounded or not.")]
    public bool grounded;

    /*
    * checks if our player is contacting the ground in the angle less than 60 degrees
    *	if it is, set groudede to true
    */
    void OnCollisionStay(Collision other)
    {
        foreach (ContactPoint contact in other.contacts)
        {
            if (Vector2.Angle(contact.normal, Vector3.up) < 60)
            {
                grounded = true;
            }
        }
    }

    /*
    * On collision exit set grounded to false
    */
    void OnCollisionExit()
    {
        grounded = false;
    }


    private RaycastHit _hitInfo;
    private float _meleeAttackCooldown;
    private string _currentWeapon;
    
    private LayerMask _ignoreLayer; //to ignore player layer

    Ray ray1, ray2, ray3, ray4, ray5, ray6, ray7, ray8, ray9;
    private float rayDetectorMeeleSpace = 0.15f;
    private float offsetStart = 0.05f;
    
    private Transform _bulletSpawn; //from here we shoot a ray to check where we hit him;
    /*
    * This method casts 9 rays in different directions. ( SEE scene tab and you will see 9 rays differently coloured).
    * Used to widley detect enemy infront and increase meele hit detectivity.
    * Checks for cooldown after last preformed meele attack.
    */


    public bool beenToMeeleAnim = false;

    private void RaycastForMeleeAttacks()
    {
        if (_meleeAttackCooldown > -5)
        {
            _meleeAttackCooldown -= 1 * Time.deltaTime;
        }


        if (GetComponent<GunInventory>().currentGun)
        {
            if (GetComponent<GunInventory>().currentGun.GetComponent<GunScript>())
                _currentWeapon = "gun";
        }

        //middle row
        var bulletSpawnPosition = _bulletSpawn.position;
        var bulletSpawnRight = _bulletSpawn.right;
        var bulletSpawnForward = _bulletSpawn.forward;
        ray1 = new Ray(bulletSpawnPosition + (bulletSpawnRight * offsetStart),
            bulletSpawnForward + (bulletSpawnRight * rayDetectorMeeleSpace));
        ray2 = new Ray(bulletSpawnPosition - (bulletSpawnRight * offsetStart),
            bulletSpawnForward - (bulletSpawnRight * rayDetectorMeeleSpace));
        ray3 = new Ray(bulletSpawnPosition, bulletSpawnForward);
        //upper row
        var bulletSpawnUp = _bulletSpawn.up;
        ray4 = new Ray(bulletSpawnPosition + (bulletSpawnRight * offsetStart) + (bulletSpawnUp * offsetStart),
            bulletSpawnForward + (bulletSpawnRight * rayDetectorMeeleSpace) +
            (bulletSpawnUp * rayDetectorMeeleSpace));
        ray5 = new Ray(bulletSpawnPosition - (bulletSpawnRight * offsetStart) + (bulletSpawnUp * offsetStart),
            bulletSpawnForward - (bulletSpawnRight * rayDetectorMeeleSpace) +
            (bulletSpawnUp * rayDetectorMeeleSpace));
        ray6 = new Ray(bulletSpawnPosition + (bulletSpawnUp * offsetStart),
            bulletSpawnForward + (bulletSpawnUp * rayDetectorMeeleSpace));
        //bottom row
        ray7 = new Ray(bulletSpawnPosition + (bulletSpawnRight * offsetStart) - (bulletSpawnUp * offsetStart),
            bulletSpawnForward + (bulletSpawnRight * rayDetectorMeeleSpace) -
            (bulletSpawnUp * rayDetectorMeeleSpace));
        ray8 = new Ray(bulletSpawnPosition - (bulletSpawnRight * offsetStart) - (bulletSpawnUp * offsetStart),
            bulletSpawnForward - (bulletSpawnRight * rayDetectorMeeleSpace) -
            (bulletSpawnUp * rayDetectorMeeleSpace));
        ray9 = new Ray(bulletSpawnPosition - (bulletSpawnUp * offsetStart),
            bulletSpawnForward - (bulletSpawnUp * rayDetectorMeeleSpace));

        Debug.DrawRay(ray1.origin, ray1.direction, Color.cyan);
        Debug.DrawRay(ray2.origin, ray2.direction, Color.cyan);
        Debug.DrawRay(ray3.origin, ray3.direction, Color.cyan);
        Debug.DrawRay(ray4.origin, ray4.direction, Color.red);
        Debug.DrawRay(ray5.origin, ray5.direction, Color.red);
        Debug.DrawRay(ray6.origin, ray6.direction, Color.red);
        Debug.DrawRay(ray7.origin, ray7.direction, Color.yellow);
        Debug.DrawRay(ray8.origin, ray8.direction, Color.yellow);
        Debug.DrawRay(ray9.origin, ray9.direction, Color.yellow);

        if (GetComponent<GunInventory>().currentGun)
        {
            if (!GetComponent<GunInventory>().currentGun.GetComponent<GunScript>().meeleAttack)
            {
                beenToMeeleAnim = false;
            }

            if (GetComponent<GunInventory>().currentGun.GetComponent<GunScript>().meeleAttack&&
                !beenToMeeleAnim)
            {
                beenToMeeleAnim = true;
                //	if (isRunning == false) {
                StartCoroutine("MeeleAttackWeaponHit");
                //	}
            }
        }
    }

    /*
     *Method that is called if the waepon hit animation has been triggered the first time via Q input
     *and if is, it will search for target and make damage
     */
    IEnumerator MeeleAttackWeaponHit()
    {
        if (Physics.Raycast(ray1, out _hitInfo, 2f, ~_ignoreLayer) ||
            Physics.Raycast(ray2, out _hitInfo, 2f, ~_ignoreLayer) || Physics.Raycast(ray3, out _hitInfo, 2f, ~_ignoreLayer)
            || Physics.Raycast(ray4, out _hitInfo, 2f, ~_ignoreLayer) ||
            Physics.Raycast(ray5, out _hitInfo, 2f, ~_ignoreLayer) || Physics.Raycast(ray6, out _hitInfo, 2f, ~_ignoreLayer)
            || Physics.Raycast(ray7, out _hitInfo, 2f, ~_ignoreLayer) ||
            Physics.Raycast(ray8, out _hitInfo, 2f, ~_ignoreLayer) ||
            Physics.Raycast(ray9, out _hitInfo, 2f, ~_ignoreLayer))
        {
            //Debug.DrawRay (bulletSpawn.position, bulletSpawn.forward + (bulletSpawn.right*0.2f), Color.green, 0.0f);
            if (_hitInfo.transform.CompareTag("Dummie"))
            {
                Transform other = _hitInfo.transform.root.transform;
                if (other.transform.CompareTag("Dummie"))
                {
                    print("hit a dummie");
                }

                InstantiateBlood(_hitInfo, false);
            }
        }

        yield return new WaitForEndOfFrame();
    }

    RaycastHit _hit; //stores info of hit;

    [Tooltip("Put your particle blood effect here.")]
    [SerializeField]
    private GameObject bloodEffect; //blood effect prefab;

    /*
    * Upon hitting enemy it calls this method, gives it raycast hit info 
    * and at that position it creates our blood prefab.
    */
    void InstantiateBlood(RaycastHit hitPos, bool swordHitWithGunOrNot)
    {
        if (_currentWeapon != "gun") return;
        GunScript.HitMarkerSound();

        if (_hitSound)
            _hitSound.Play();
        else
            print("Missing hit sound");

        if (swordHitWithGunOrNot) return;
        if (bloodEffect)
            Instantiate(bloodEffect, hitPos.point, Quaternion.identity);
        else
            print("Missing blood effect prefab in the inspector.");
    }

    [Header("Player SOUNDS")] [Tooltip("Jump sound when player jumps.")]
    public AudioSource _jumpSound;

    [Tooltip("Sound while player makes when successfully reloads weapon.")]
    public AudioSource _freakingZombiesSound;

    [Tooltip("Sound Bullet makes when hits target.")]
    public AudioSource _hitSound;

    [Tooltip("Walk sound player makes.")]
    public AudioSource _walkSound;
    
    [Tooltip("Run Sound player makes.")]
    public AudioSource _runSound;
}