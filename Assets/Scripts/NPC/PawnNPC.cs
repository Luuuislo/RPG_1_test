using UnityEngine;
using UnityEngine.EventSystems;

// Añadir a Pawn_NPC_Base. Requiere Collider2D + Physics2D Raycaster en la cámara.
public class PawnNPC : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData) => PawnShopUI.Instance?.Open();
}
