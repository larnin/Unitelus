using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;


class FxManager : MonoBehaviour
{
    Dictionary<int, FxInstance> m_playingFx = new Dictionary<int, FxInstance>();
    int m_nextFxID = 0;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<StartFxEvent>.Subscriber(StartFx));
        m_subscriberList.Add(new Event<StopFxEvent>.Subscriber(StopFx));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Update()
    {
        List<int> toDestroyFx = new List<int>();

        foreach(var instance in m_playingFx)
        {
            instance.Value.Update();

            if (instance.Value.NeedToDestroy())
            {
                toDestroyFx.Add(instance.Key);
                instance.Value.BeforeDestroy();
            }
        }

        foreach (var id in toDestroyFx)
            m_playingFx.Remove(id);
    }

    void StopFx(int id)
    {
        FxInstance instance = null;
        m_playingFx.TryGetValue(id, out instance);
        if (instance == null)
            return;

        instance.BeforeDestroy();

        m_playingFx.Remove(id);
    }

    void StartFx(StartFxEvent e)
    {
        int id = m_nextFxID++;

        e.outFxID = id;

        FxInstance instance = new FxInstance(id, e.skillUID, e.skillStep, gameObject);
        m_playingFx.Add(id, instance);

        instance.InitCaster(e.caster, e.casterPos, e.casterRot);
        instance.InitTarget(e.target, e.targetPos, e.targetRot);

        instance.OnInit();
    }

    void StopFx(StopFxEvent e)
    {
        StopFx(e.fxID);
    }
}
