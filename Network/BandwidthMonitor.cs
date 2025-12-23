using DSMM.Common;
using UnityEngine;

namespace DSMM.Network
{
    public class BandwidthMonitor : MonoBehaviour
    {
        public static BandwidthMonitor Instance;

        private long bytesSent;
        private long bytesReceived;

        private float timer;

        public float SendKBps { get; private set; }
        public float ReceiveKBps { get; private set; }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void Update()
        {
            timer += Time.deltaTime;

            if (timer >= 1f)
            {
                SendKBps = bytesSent / 1024f;
                ReceiveKBps = bytesReceived / 1024f;

                bytesSent = 0;
                bytesReceived = 0;
                timer = 0f;
            }
        }

        void OnGUI()
        {
            if(!Utils.HasArg("--mp-debug") || NetworkManager.Instance == null || !NetworkManager.Instance.IsConnected())
                return;

            const float width = 200f;
            const float height = 50f;

            GUILayout.BeginArea(new Rect(
                10,
                Screen.height - height,
                width,
                height
            ));

            GUILayout.Label($"Send: {BandwidthMonitor.Instance.SendKBps:F2} KB/s");
            GUILayout.Label($"Recv: {BandwidthMonitor.Instance.ReceiveKBps:F2} KB/s");

            GUILayout.EndArea();
        }

        public void AddSent(int bytes)
        {
            bytesSent += bytes;
        }

        public void AddReceived(int bytes)
        {
            bytesReceived += bytes;
        }
    }
}
