using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf : Node
{
    public delegate Status Tick();
    public Tick ProcessMethod;

    public delegate Status TickM(int val);
    public TickM ProcessMethodM;

    public int index;

    private Action onFinish;

    public Leaf() { }

    public Leaf(string n, Tick pm, Action onComplete = null)
    {
        name = n;
        ProcessMethod = pm;
        onFinish = onComplete;
    }

    public Leaf(string n, int i, TickM pm, Action onComplete = null)
    {
        name = n;
        ProcessMethodM = pm;
        index = i;
        onFinish = onComplete;
    }

    public Leaf(string n, Tick pm, int order, Action onComplete = null)
    {
        name = n;
        ProcessMethod = pm;
        sortOrder = order;
        onFinish = onComplete;
    }

    public override Status Process()
    {
        Node.Status s;
        if(ProcessMethod != null)
            s = ProcessMethod();
        else if (ProcessMethodM != null)
            s = ProcessMethodM(index);
        else
            s = Status.FAILURE;

        if (s == Status.SUCCESS)
        {
            onFinish?.Invoke();
           // return s;
        }

        Debug.Log(name + " " + s);
        return s;
    }

}
