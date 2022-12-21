using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MH
{
    public interface IGameObjectAction
    {
        void Invoke(GameObject gameObject);
    }
}
