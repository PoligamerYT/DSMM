using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DSMM.UI
{
    // Code Extracted From https://www.youtube.com/watch?v=cY2cHP8rnPk
    public class ColorPicker : MonoBehaviour, IPointerClickHandler
    {
        public Color output;

        public void OnPointerClick(PointerEventData eventData)
        {
            output = Pick(CameraController.Instance._cam.gameObject.GetComponent<Camera>().WorldToScreenPoint(eventData.position), GetComponent<Image>());
            UIManager.Instance.PlayerViewer.color = output;
        }

        Color Pick(Vector2 screenPoint, Image imageToPick)
        {
            Vector2 point;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(imageToPick.rectTransform, screenPoint, CameraController.Instance._cam.gameObject.GetComponent<Camera>(), out point);
            point += imageToPick.rectTransform.sizeDelta / 2;
            Texture2D t = GetComponent<Image>().sprite.texture;
            Vector2Int m_point = new Vector2Int((int)((t.width * point.x) / imageToPick.rectTransform.sizeDelta.x), (int)((t.height * point.y) / imageToPick.rectTransform.sizeDelta.y));
            return t.GetPixel(m_point.x, m_point.y);
        }
    }
}
