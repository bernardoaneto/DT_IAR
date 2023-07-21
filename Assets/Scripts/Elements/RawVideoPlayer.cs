using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class RawVideoPlayer : MonoBehaviour
{
    [SerializeField] private string ip;
    [SerializeField] private int port;
    [SerializeField] private int width;
    [SerializeField] private int height;

    private Thread thread;
    private static bool closing;
    private static readonly ConcurrentQueue<Color32[]> frameQueue = new ConcurrentQueue<Color32[]>();

    private Texture2D texture;

    private void Start()
    {
        texture = new Texture2D(width, height);
        GetComponent<Renderer>().material.mainTexture = texture;
        (thread = new Thread(ReceiveFrame)).Start();
        Debug.Log($"Video connected @ {ip}:{port}");
    }

    private void OnApplicationQuit()
    {
        closing = true;
        thread.Join();
    }

    private void FixedUpdate()
    {
        if (!frameQueue.TryDequeue(out var frame)) return;
        texture.SetPixels32(frame);
        texture.Apply();
    }

    private void ReceiveFrame()
    {
        using var client = new TcpClient(ip, port);
        var bufferSize = width * height;
        var bytesLen = bufferSize * 3;
        var bytes = new byte[bytesLen];
        var buffer = new Color32[bufferSize];
        var stream = client.GetStream();

        while (!closing && client.Connected)
        {
            try
            {
                var i = 0;
                while (i < bytesLen) i += stream.Read(bytes, i, bytesLen - i);
            }
            catch (Exception)
            {
                return;
            }

            var b = 0;
            while (b < bufferSize)
            {
                buffer[b] = new Color32(bytes[b * 3], bytes[b * 3 + 1], bytes[b * 3 + 2], byte.MaxValue);
                b++;
            }

            frameQueue.Enqueue(buffer);
        }
    }
}