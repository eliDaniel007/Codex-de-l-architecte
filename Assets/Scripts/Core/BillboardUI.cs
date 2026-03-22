using UnityEngine;

namespace Codex.Core
{
    public class BillboardUI : MonoBehaviour
    {
        Transform _cam;

        void Start()
        {
            _cam = Camera.main != null ? Camera.main.transform : null;
        }

        void LateUpdate()
        {
            if (_cam == null)
            {
                if (Camera.main != null) _cam = Camera.main.transform;
                else return;
            }
            transform.LookAt(transform.position + _cam.forward);
        }
    }
}
