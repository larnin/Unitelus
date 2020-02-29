using System;

class Event<T>
{
	public class Subscriber : IEventSubscriber
	{
		private Action<T> func_;

		public Subscriber(Action<T> func)
		{
			func_ = func;
		}

		void IEventSubscriber.Subscribe()
		{
			Event<T>.Subscribe(func_);
		}

		void IEventSubscriber.Unsubscribe()
		{
			Event<T>.Unsubscribe(func_);
		}
	}

	private static event Action<T> _event;

	public static void Subscribe(Action<T> func)
	{
		_event += func;
	}

	public static void Unsubscribe(Action<T> func)
	{
		_event -= func;
	}

	public static void Broadcast(T data)
	{
		if (_event != null)
		{
			_event(data);
		}
	}
}