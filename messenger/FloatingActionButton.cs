using System;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace messenger
{
	public class FloatingActionButton : FrameLayout
	{
		// The View that is revealed
		protected View revealView;

		// The coordinates of a touch action
		protected Point touchPoint;

		// A GestureDetector to detect touch actions
		private GestureDetector gestureDetector;

		public FloatingActionButton(IntPtr a, Android.Runtime.JniHandleOwnership b) : base(a,b)
		{
		}

		public FloatingActionButton (Context context) : this(context,null,0,0)
		{
		}

		public FloatingActionButton(Context context, IAttributeSet attrs) : this(context,attrs,0,0)
		{
		}

		public FloatingActionButton(Context context, IAttributeSet attrs, int defStyleAttr) : this(context,attrs,defStyleAttr,0)
		{
		}

		public FloatingActionButton(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context,attrs,defStyleAttr,defStyleRes)
		{
			// When a view is clickable it will change its state to "pressed" on every click.
			Clickable = true;

			// Create a GestureDetector to detect single taps
			gestureDetector = new GestureDetector (context, new MySimpleOnGestureListener (this));

			//A new View is created
			revealView = new View (context);
			AddView (revealView, ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
		}

		private class MySimpleOnGestureListener : GestureDetector.SimpleOnGestureListener
		{
			FloatingActionButton b;
			public MySimpleOnGestureListener(FloatingActionButton bu)
			{
				b = bu;
			}
				
			public override bool OnSingleTapConfirmed (MotionEvent e)
			{
				b.touchPoint = new Point ((int)e.GetX (), (int)e.GetY ());

				return true;
			}
		}

		private class MyAnimatorListenerAdapter: AnimatorListenerAdapter
		{
			FloatingActionButton b;
			public MyAnimatorListenerAdapter(FloatingActionButton bu)
			{
				b = bu;
			}
			public override void OnAnimationEnd (Animator animation)
			{
				b.RefreshDrawableState ();

				b.revealView.Visibility = ViewStates.Gone;
				// Reset the touch point as the next call to setChecked might not come
				// from a tap.
				b.touchPoint = null;
			}
		}

		public override bool OnTouchEvent (MotionEvent e)
		{
			if (gestureDetector.OnTouchEvent (e))
				return true;

			return base.OnTouchEvent (e);
		}
			
		protected override void OnSizeChanged (int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged (w, h, oldw, oldh);

			var outline = new Outline ();
			OutlineProvider = new OutlineProv ();
			outline.SetOval (0, 0, w, h);
			OutlineProvider.GetOutline (this, outline);
			ClipToOutline = true;
		}

		private class OutlineProv : ViewOutlineProvider
		{
			public override void GetOutline (View view, Outline outline)
			{
				outline.SetOval (0, 0, view.Width, view.Height);
			}
		}

		protected override int[] OnCreateDrawableState (int extraSpace)
		{
			int[] drawableState = base.OnCreateDrawableState (extraSpace + 1);

			return drawableState;
		}


	}
}

