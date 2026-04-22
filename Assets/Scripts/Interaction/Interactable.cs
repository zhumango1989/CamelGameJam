using UnityEngine;
using UnityEngine.Events;

namespace GameJam.Interaction
{
    public interface IInteractable
    {
        void Interact(GameObject interactor);
        string GetInteractionPrompt();
        bool CanInteract(GameObject interactor);
    }

    public class Interactable : MonoBehaviour, IInteractable
    {
        [SerializeField] protected string interactionPrompt = "Press E to interact";
        [SerializeField] protected bool singleUse = false;
        [SerializeField] protected UnityEvent onInteract;

        protected bool hasBeenUsed;

        public virtual void Interact(GameObject interactor)
        {
            if (singleUse && hasBeenUsed) return;

            onInteract?.Invoke();
            hasBeenUsed = true;

            OnInteract(interactor);
        }

        protected virtual void OnInteract(GameObject interactor) { }

        public virtual string GetInteractionPrompt()
        {
            return interactionPrompt;
        }

        public virtual bool CanInteract(GameObject interactor)
        {
            if (singleUse && hasBeenUsed) return false;
            return true;
        }
    }

    public class InteractionTrigger : MonoBehaviour
    {
        [SerializeField] private float interactionRadius = 2f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private IInteractable currentTarget;

        private void Update()
        {
            CheckForInteractables();

            if (currentTarget != null && UnityEngine.Input.GetKeyDown(interactKey))
            {
                if (currentTarget.CanInteract(gameObject))
                {
                    currentTarget.Interact(gameObject);
                }
            }
        }

        private void CheckForInteractables()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRadius, interactableLayer);

            IInteractable closest = null;
            float closestDistance = float.MaxValue;

            foreach (var col in colliders)
            {
                if (col.TryGetComponent<IInteractable>(out var interactable))
                {
                    if (interactable.CanInteract(gameObject))
                    {
                        float distance = Vector3.Distance(transform.position, col.transform.position);
                        if (distance < closestDistance)
                        {
                            closest = interactable;
                            closestDistance = distance;
                        }
                    }
                }
            }

            currentTarget = closest;
        }

        public string GetCurrentPrompt()
        {
            return currentTarget?.GetInteractionPrompt() ?? string.Empty;
        }

        public bool HasTarget => currentTarget != null;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }

    public class PickupItem : Interactable
    {
        [SerializeField] private string itemId;
        [SerializeField] private int quantity = 1;

        public string ItemId => itemId;
        public int Quantity => quantity;

        protected override void OnInteract(GameObject interactor)
        {
            Core.EventSystem.Emit(Core.GameEvents.ItemCollected, this);
            Destroy(gameObject);
        }
    }
}
