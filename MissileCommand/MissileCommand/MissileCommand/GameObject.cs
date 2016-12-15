using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace MissileCommand
{
    public abstract class GameObject
    {

        public bool toDestroy = false;

        public virtual void Destroy()
        {
            toDestroy = true;
        }

        public abstract void Update(List<GameObject> objects);
        public abstract void Draw(SpriteBatch sb);

    }
}
