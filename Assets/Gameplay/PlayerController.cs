//-----------------------------------------------------------------------
// REWIND COMPANY — Player Controller
// First-person movement with a held object, interact ray, and rewind
// position snapshot. Modular: the time-rewind system drives state,
// this controller only handles input and physics.
//-----------------------------------------------------------------------

using UnityEngine;
using RewindCompany.Systems;

namespace RewindCompany.Gameplay
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 4f;
        public float runSpeed = 7f;
        public float jumpForce = 8f;
        public float gravity = -20f;
        public float lookSpeed = 2.5f;
        public float lookXLimit = 60f;

        [Header("Cursor")]
        public float cursorSize = 1.5f;
        public LayerMask interactionMask;
        public float interactionRange = 5f;

        [Header("References")]
        public RewindSystem rewind;
        public RewindInputSystem input;

        // Raycast helpers
        public Transform cam;
        public Camera playerCam;

        // Input state
        public bool isWalking;
        public bool isCrouching;
        public bool isJumpPressed;

        // Physics
        public CharacterController controller;
        public Transform body;
        public Transform camRoot;

        // Held object
        public string heldObjectId;
        public Vector3 heldPosition;
        public Quaternion heldRotation;

        // Rewind snapshot
        Vector3 rewindPosition;
        Quaternion rewindRotation;

        void Awake()
        {
            if (playerCam == null) playerCam = GetComponentInChildren<Camera>();
            if (camRoot == null) camRoot = transform.GetChild(0);
            if (controller == null) controller = GetComponent<CharacterController>();
        }

        void OnEnable()
        {
            if (rewind != null && input != null)
            {
                rewind = RewindSystem.Instance;
                input = RewindInputSystem.Instance;
            }
        }

        void Update()
        {
            HandleCamera();
            HandleMovement();
            HandleHolding();
            HandleInteraction();
        }

        void HandleCamera()
        {
            if (cam == null) cam = Camera.main.transform;
            if (camRoot == null) camRoot = transform.GetChild(0);
            if (cam == null || camRoot == null) return;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            if (h != 0 || v != 0)
            {
                Vector3 dir = new Vector3(h, 0, v).normalized;
                if (dir.magnitude > 0.1f)
                {
                    dir = Vector3.Slerp(Vector3.forward, dir, 0.5f);
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.LookRotation(dir), Time.deltaTime * 10f);
                }
            }

            float dy = Input.GetAxisRaw("Mouse Y");
            float dx = Input.GetAxisRaw("Mouse X");
            camRoot.Rotate(0, dx * lookSpeed, 0);
            camRoot.Rotate(-dy * lookSpeed, 0, 0);

            Cursor.lockState = CursorLockMode.Locked;
        }

        void HandleMovement()
        {
            if (camRoot == null) return;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            isWalking = (h != 0 || v != 0) && !Input.GetKey(KeyCode.LeftShift);

            Vector3 forward = camRoot.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 move = (forward * v + right * h) * (isWalking ? moveSpeed : runSpeed);

            if (isCrouching)
            {
                controller.height = 1f;
                move *= 0.7f;
            }
            else
            {
                controller.height = 2f;
            }

            move.y += gravity * Time.deltaTime;
            controller.Move(move * Time.deltaTime);
        }

        void HandleHolding()
        {
            if (Input.GetKeyDown(KeyCode.F) && heldObjectId != null)
            {
                input.OnPickup(heldObjectId, heldPosition, heldRotation);
                heldObjectId = null;
                return;
            }
        }

        // Interaction / pick up
        void HandleInteraction()
        {
            if (cam == null)
            {
                if (playerCam != null) cam = playerCam.transform;
                else return;
            }

            RaycastHit hit;
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, interactionRange, interactionMask))
            {
                // Highlight / cursor update handled by UI system.
                if (Input.GetMouseButtonDown(0))
                {
                    if (heldObjectId == null && hit.transform != null && hit.transform.CompareTag("Pickupable"))
                    {
                        GrabObject(hit.transform);
                    }
                }
            }
        }

        void GrabObject(Transform obj)
        {
            heldObjectId = obj.gameObject.GetInstanceID().ToString();
            heldPosition = obj.position;
            heldRotation = obj.rotation;
            obj.SetParent(camRoot, true);
            obj.localPosition = Vector3.zero;
            obj.localRotation = Quaternion.identity;
            obj.localScale = Vector3.one;
            input.OnPickup(heldObjectId, heldPosition, heldRotation);
        }

        // Rewind snapshot
        public void RecordRewindPosition()
        {
            rewindPosition = transform.position;
            rewindRotation = transform.rotation;
        }
    }
}
