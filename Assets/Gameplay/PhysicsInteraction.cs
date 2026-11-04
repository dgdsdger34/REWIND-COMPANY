//-----------------------------------------------------------------------
// REWIND COMPANY — Physics Interaction
// Handles grab/release, hold-to-move, and interact raycasting.
// Kept separate from PlayerController so it can be reused for puzzles.
//-----------------------------------------------------------------------

using UnityEngine;

namespace RewindCompany.Gameplay
{
    public class PhysicsInteraction : MonoBehaviour
    {
        [Header("Grab")]
        public float grabDistance = 1.5f;
        public float grabForce = 10f;
        public float holdMoveSpeed = 3f;

        [Header("Input")]
        public KeyCode grabKey = KeyCode.Mouse0;
        public KeyCode releaseKey = KeyCode.Mouse0;
        public KeyCode interactKey = KeyCode.E;

        [Header("References")]
        public PlayerController player;
        public RewindInputSystem rewindInput;

        public Transform heldObject;
        public Rigidbody heldRigidbody;

        public bool isHolding { get; private set; }

        void OnEnable() { }

        void FixedUpdate()
        {
            if (isHolding && heldObject != null && heldRigidbody != null)
            {
                // Pull held object toward camera.
                Vector3 dir = (player.camRoot.position - heldObject.position).normalized;
                heldObject.position += dir * holdMoveSpeed * Time.fixedDeltaTime;
                heldObject.rotation = Quaternion.Slerp(heldObject.rotation,
                    player.camRoot.rotation, 10f * Time.fixedDeltaTime);
            }
        }

        void Update()
        {
            if (heldObject == null) return;

            if (Input.GetKeyDown(releaseKey))
            {
                Drop();
            }
            if (Input.GetMouseButton(grabKey))
            {
                MoveHeldObject();
            }
        }

        void MoveHeldObject()
        {
            if (heldRigidbody != null)
            {
                // Direct physics: apply force toward camera.
                Vector3 dir = (player.camRoot.position - heldObject.position).normalized;
                heldRigidbody.AddForce(dir * grabForce, ForceMode.Impulse);
            }
            else
            {
                // Kinematic / childed: move with camera.
                heldObject.position += (player.camRoot.position - heldObject.position).normalized *
                    holdMoveSpeed * Time.deltaTime;
            }
        }

        public void Grab(Transform obj, Rigidbody rigid)
        {
            heldObject = obj;
            heldRigidbody = rigid;
            obj.SetParent(player.camRoot);
            obj.localPosition = Vector3.zero;
            obj.localRotation = Quaternion.identity;
            isHolding = true;
        }

        void Drop()
        {
            if (heldObject == null) return;
            if (heldRigidbody != null)
            {
                heldRigidbody.isKinematic = false;
                heldRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }
            heldObject.SetParent(null);
            heldObject = null;
            heldRigidbody = null;
            isHolding = false;
            rewindInput.OnDrop();
        }
    }
}
