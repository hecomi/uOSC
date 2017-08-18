using UnityEngine;
using System.Threading;

namespace uOSC
{

public class Thread
{
    System.Threading.Thread thread_;
    bool isRunning_ = false;
    System.Action loopFunc_ = null;

    public void Start(System.Action loopFunc)
    {
        if (isRunning_) return;

        isRunning_ = true;
        loopFunc_ = loopFunc;

        thread_ = new System.Threading.Thread(() => 
        {
            while (isRunning_)
            {
                if (loopFunc_ != null) 
                {
                    loopFunc_();
                }
                System.Threading.Thread.Sleep(0);
            }
        });
        thread_.Start();
    }

    public void Stop(int timeoutMilliseconds = 3000)
    {
        if (!isRunning_) return;

        isRunning_ = false;

        if (thread_.IsAlive)
        {
            thread_.Join(timeoutMilliseconds);
            if (thread_.IsAlive)
            {
                thread_.Abort();
            }
        }
    }
}

}