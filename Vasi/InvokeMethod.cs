using System;
using HutongGames.PlayMaker;

namespace Vasi
{
    public class InvokeMethod : FsmStateAction
    {
        private readonly Action _action;

        public InvokeMethod(Action a)
        {
            _action = a;
        }

        public override void OnEnter()
        {
            _action?.Invoke();
            
            Finish();
        }
    }
}