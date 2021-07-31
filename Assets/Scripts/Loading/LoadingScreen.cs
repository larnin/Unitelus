using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class LoadingScreen : MonoBehaviour
{
    SubscriberList m_subscriberList = new SubscriberList();

    TMP_Text m_text = null;

    private void Awake()
    {
        m_subscriberList.Add(new Event<GameLoaderStateEvent>.Subscriber(LoadingChangeState));
        m_subscriberList.Subscribe();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void Start()
    {
        var obj = transform.Find("Loading text");
        if (obj != null)
            m_text = obj.GetComponent<TMP_Text>();
    }
    
    void Update()
    {
        GetLoadingState state = new GetLoadingState();
        Event<GetLoadingState>.Broadcast(state);

        m_text.text = state.stateText;
    }

    void LoadingChangeState(GameLoaderStateEvent e)
    {
        if(!gameObject.activeSelf && e.newState != GameLoadingState.loaded)
            gameObject.SetActive(true);

        if(e.newState == GameLoadingState.loaded)
        {
            DOVirtual.DelayedCall(0.5f, ()=>
                {
                    if (this != null)
                        gameObject.SetActive(false);
            });
        }
    }
}
