using UnityEngine;
using babel.extensions.context.api;
using babel.extensions.context.impl;
using babel.extensions.mediation.api;

namespace babel.extensions.mediation.impl
{
	public class View : MonoBehaviour
	{
		//Leave this value true most of the time. If for some reason you want
		//a view to exist outside a context you can set it to false. The only
		//difference is whether an error gets generated.
		public bool requiresContext = true;
		
		protected bool registeredWithContext = false;

		protected virtual void Awake ()
		{
			bubbleToContext(this, true, false);
		}

		protected virtual void Start ()
		{
			if (!registeredWithContext)
				bubbleToContext(this, true, true);
		}

		protected virtual void OnDestroy ()
		{
			bubbleToContext(this, false, false);
		}
		
		virtual protected void bubbleToContext(MonoBehaviour view, bool toAdd, bool finalTry)
		{
			const int LOOP_MAX = 100;
			int loopLimiter = 0;
			Transform trans = view.gameObject.transform;
			while(trans.parent != null && loopLimiter < LOOP_MAX)
			{
				loopLimiter ++;
				trans = trans.parent;
				Debug.Log(trans.gameObject.name);
				
				if (trans.gameObject.GetComponent<ContextView>() != null)
				{
					ContextView contextView = trans.gameObject.GetComponent<ContextView>() as ContextView;
					if (contextView.context != null)
					{
						IContext context = contextView.context;
						if (toAdd)
						{
							context.AddView(view);
							registeredWithContext = true;
							return;
						}
						else
						{
							context.RemoveView(view);
							return;
						}
					}
				}
			}
			if (requiresContext && finalTry)
			{
				string msg = (loopLimiter == LOOP_MAX) ?
					msg = "A view couldn't find a context. Loop limit reached." :
					msg = "A view was added with no context. Views must be added into the hierarchy of their ContextView lest all hell break loose.";
				msg += "\nView: " + view.ToString();
				
				
				throw new MediationException(msg,
					MediationExceptionType.NO_CONTEXT);
			}
		}
	}
}

