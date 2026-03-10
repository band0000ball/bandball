using Items.ItemData;
using Ricimi;
using UnityEngine;

namespace UI
{
    public class ItemOpener : PopupOpener
    {
        public OwnedItemData item;
        
        public override void OpenPopup()
        {
            var popup = Instantiate(popupPrefab, m_canvas.transform, false);
            popup.SetActive(true);
            popup.transform.localScale = Vector3.zero;
            var popupComponent = popup.GetComponent<ItemPopup>();
            popupComponent.Set(item);
            popupComponent.Open();
        }
    }
}