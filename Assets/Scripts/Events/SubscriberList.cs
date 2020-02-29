using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SubscriberList : List<IEventSubscriber>
{
	public void Subscribe()
	{
		foreach (IEventSubscriber current in this)
		{
			current.Subscribe();
		}
	}

	public void Unsubscribe()
	{
		foreach (IEventSubscriber current in this)
		{
			current.Unsubscribe();
		}
	}
}