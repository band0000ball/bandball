using DG.Tweening;
using UnityEngine;

namespace CameraComponent
{
    public class MainCamera : MonoBehaviour
    {
        /// <summary>
        /// 振動演出
        /// </summary>
        /// <param name="width">触れ幅</param>
        /// <param name="count">往復回数</param>
        /// <param name="duration">時間</param>
        public void Shake(float width, int count, float duration)
        {
            var cameraTransform = Camera.main?.transform;
            var seq = DOTween.Sequence();
            // 振れ演出の片道の揺れ分の時間
            var partDuration = duration / count / 2f;
            // 振れ幅の半分の値
            var widthHalf = width / 2f;
            // 往復回数-1回分の振動演出を作る
            for (int i = 0; i < count - 1; i++)
            {
                seq.Append(cameraTransform.DOLocalRotate(new Vector3(-widthHalf, 0f), partDuration));
                seq.Append(cameraTransform.DOLocalRotate(new Vector3(widthHalf, 0f), partDuration));
            }
            // 最後の揺れは元の角度に戻す工程とする
            seq.Append(cameraTransform.DOLocalRotate(new Vector3(-widthHalf, 0f), partDuration));
            seq.Append(cameraTransform.DOLocalRotate(Vector3.zero, partDuration));
        }

    }
}