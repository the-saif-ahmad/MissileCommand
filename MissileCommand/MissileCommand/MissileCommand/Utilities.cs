using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MissileCommand
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;


    class Utilities
    {
        public static void update()
        {
            EventMouse.update();
            KB.update();
            Defer.update();
        }
    }
    class EventMouse
    {


        private static MouseState oldMouse = Mouse.GetState();


        private static List<Action<Point>> leftClick = new List<Action<Point>>();
        private static List<Action<Point>> rightClick = new List<Action<Point>>();
        private static List<Action<Point>> middleClick = new List<Action<Point>>();


        public static void onLeftClick(Action<Point> a)
        {
            leftClick.Add(a);
        }


        public static void onRightClick(Action<Point> a)
        {
            rightClick.Add(a);
        }


        public static void onMiddleClick(Action<Point> a)
        {
            middleClick.Add(a);
        }


        public static void update()
        {
            var state = Mouse.GetState();
            if (state.LeftButton == ButtonState.Pressed && oldMouse.LeftButton != ButtonState.Pressed) leftClick.ForEach(a => a.Invoke(new Point(state.X, state.Y)));
            if (state.RightButton == ButtonState.Pressed && oldMouse.RightButton != ButtonState.Pressed) rightClick.ForEach(a => a.Invoke(new Point(state.X, state.Y)));
            if (state.MiddleButton == ButtonState.Pressed && oldMouse.MiddleButton != ButtonState.Pressed) middleClick.ForEach(a => a.Invoke(new Point(state.X, state.Y)));
            oldMouse = state;
        }


    }
    class KB
    {


        static private HashSet<Keys> last = new HashSet<Keys>();


        static private Dictionary<Keys, List<Action>> pressActions = new Dictionary<Keys, List<Action>>();
        static private Dictionary<Keys, List<Action>> depressActions = new Dictionary<Keys, List<Action>>();


        public static void update()
        {
            var p = Keyboard.GetState().GetPressedKeys();
            HashSet<Keys> next = new HashSet<Keys>(p);
            next.Except(last).ToList().ForEach(k => { if (pressActions.ContainsKey(k)) pressActions[k].ForEach(a => a.Invoke()); });
            last.Except(next).ToList().ForEach(k => { if (depressActions.ContainsKey(k)) depressActions[k].ForEach(a => a.Invoke()); });
            last = next;
        }


        public static void onPress(Keys k, Action a)
        {
            if (pressActions.ContainsKey(k)) pressActions[k].Add(a);
            else { var l = new List<Action>(); l.Add(a); pressActions[k] = l; }
        }


        public static void onDepress(Keys k, Action a)
        {
            if (depressActions.ContainsKey(k)) depressActions[k].Add(a);
            else { var l = new List<Action>(); l.Add(a); depressActions[k] = l; }
        }

        public static void removeOnPress(Keys k, Action a)
        {
            pressActions[k].Remove(a);
        }


    }
    class Defer
    {
        private class DeferEntry
        {
            public int ticksLeft;
            public Action action;
            public DeferEntry(int ticks, Action action)
            {
                this.ticksLeft = ticks;
                this.action = action;
            }
        }


        private static List<DeferEntry> entries = new List<DeferEntry>();


        public static void defer(int ticks, Action action)
        {
            entries.Add(new DeferEntry(ticks, action));
        }


        public static void update()
        {
            entries.ForEach(d => { if (--d.ticksLeft == 0) d.action.Invoke(); });
            entries.RemoveAll(d => d.ticksLeft == 0);
        }
    }

}
