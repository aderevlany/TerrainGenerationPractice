using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ThreadedDataRequester : MonoBehaviour
{
    static ThreadedDataRequester instance;
    Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

    private void Awake()
    {
        instance = FindObjectOfType<ThreadedDataRequester>();
    }

    // generateData is the function that is called to generate the data that you want
    public static void RequestData(Func<object> generateData, Action<object> callback)
    {
        // starts the thread for generating heightMap
        ThreadStart threadStart = delegate {
            instance.DataThread(generateData, callback); };

        new Thread(threadStart).Start();
    }

    // is started in a new thread
    void DataThread(Func<object> generateData, Action<object> callback)
    {
        //HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, center);
        object data = generateData();
        // Lock the heightMap queue so that multiple threads cannot access it at the same time
        lock (dataQueue)
        {
            dataQueue.Enqueue(new ThreadInfo(callback, data));
        }
    }

    private void Update()
    {
        if (dataQueue.Count > 0)
        {
            for (int i = 0; i < dataQueue.Count; i++)
            {
                ThreadInfo threadInfo = dataQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);  // call the passed function with the appropriate parameter
            }
        }
    }

    struct ThreadInfo
    {
        public readonly Action<object> callback;
        public readonly object parameter;

        public ThreadInfo(Action<object> callback, object parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
