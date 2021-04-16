using System;
using System.Collections;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;

namespace Vasi
{
    [PublicAPI]
    public static class FsmUtil
    {
        public static void RemoveAction(this FsmState state, int index)
        {
            state.Actions = state.Actions.Where((_, ind) => ind != index).ToArray();
        }

        public static void RemoveAction<T>(this FsmState state) where T : FsmStateAction
        {
            state.Actions = state.Actions.RemoveFirst(x => x is T).ToArray();
        }

        public static void RemoveAllOfType<T>(this FsmState state) where T : FsmStateAction
        {
            state.Actions = state.Actions.Where(x => x is not T).ToArray();
        }

        public static void RemoveAnim(this PlayMakerFSM fsm, string stateName, int index)
        {
            var anim = fsm.GetAction<Tk2dPlayAnimationWithEvents>(stateName, index);

            var @event = new FsmEvent(anim.animationCompleteEvent ?? anim.animationTriggerEvent);

            FsmState state = fsm.GetState(stateName);

            state.RemoveAction(index);

            state.InsertAction
            (
                index,
                new NextFrameEvent
                {
                    sendEvent = @event,
                    Active = true,
                    Enabled = true
                }
            );
        }

        public static FsmState GetState(this PlayMakerFSM fsm, string stateName)
        {
            return fsm.FsmStates.First(t => t.Name == stateName);
        }

        public static bool TryGetState(this PlayMakerFSM fsm, string stateName, out FsmState state)
        {
            state = fsm.FsmStates.FirstOrDefault(t => t.Name == stateName);

            return state != null;
        }

        public static FsmState CopyState(this PlayMakerFSM fsm, string stateName, string newState)
        {
            FsmState orig = fsm.GetState(stateName);

            var state = new FsmState(orig)
            {
                Name = newState,
                Transitions = orig
                              .Transitions
                              .Select(x => new FsmTransition(x) { ToFsmState = x.ToFsmState })
                              .ToArray(),
            };


            fsm.Fsm.States = fsm.FsmStates.Append(stte).ToArray();

            return state;
        }

        public static T GetAction<T>(this FsmState state, int index) where T : FsmStateAction
        {
            FsmStateAction act = state.Actions[index];

            return (T) act;
        }

        public static T GetAction<T>(this PlayMakerFSM fsm, string stateName, int index) where T : FsmStateAction
        {
            FsmStateAction act = fsm.GetState(stateName).Actions[index];

            return (T) act;
        }

        public static T GetAction<T>(this FsmState state) where T : FsmStateAction
        {
            return state.Actions.OfType<T>().First();
        }

        public static T GetAction<T>(this PlayMakerFSM fsm, string stateName) where T : FsmStateAction
        {
            return fsm.GetState(stateName).GetAction<T>();
        }

        public static void AddAction(this FsmState state, FsmStateAction action)
        {
            state.Actions = state.Actions.Append(action).ToArray();
        }

        public static void InsertAction(this FsmState state, int index, FsmStateAction action)
        {
            state.Actions = state.Actions.Insert(index, action).ToArray();

            action.Init(state);
        }

        public static void ChangeTransition(this PlayMakerFSM self, string state, string eventName, string toState)
        {
            self.GetState(state).ChangeTransition(eventName, toState);
        }

        public static void ChangeTransition(this FsmState state, string eventName, string toState)
        {
            state.Transitions.First(tr => tr.EventName == eventName).ToFsmState = state.Fsm.FsmComponent.GetState(toState);
        }

        public static void AddTransition(this FsmState state, FsmEvent @event, string toState)
        {
            state.Transitions = state.Transitions.Append
            (
                new FsmTransition
                {
                    FsmEvent = @event,
                    ToFsmState = state.Fsm.GetState(toState)
                }
            )
            .ToArray();
        }

        [PublicAPI]
        public static void AddTransition(this FsmState state, string eventName, string toState)
        {
            state.AddTransition(FsmEvent.GetFsmEvent(eventName) ?? new FsmEvent(eventName), toState);
        }

        [PublicAPI]
        public static void RemoveTransition(this FsmState state, string transition)
        {
            state.Transitions = state.Transitions.Where(trans => transition != trans.ToFsmState.Name).ToArray();
        }

        [PublicAPI]
        public static void AddCoroutine(this FsmState state, Func<IEnumerator> method)
        {
            state.InsertCoroutine(state.Actions.Length, method);
        }

        [PublicAPI]
        public static void AddMethod(this FsmState state, Action method)
        {
            state.InsertMethod(state.Actions.Length, method);
        }

        [PublicAPI]
        public static void InsertMethod(this FsmState state, int index, Action method)
        {
            state.InsertAction(index, new InvokeMethod(method));
        }

        [PublicAPI]
        public static void InsertCoroutine(this FsmState state, int index, Func<IEnumerator> coro, bool wait = true)
        {
            state.InsertAction(index, new InvokeCoroutine(coro, wait));
        }

        [PublicAPI]
        public static FsmInt GetOrCreateInt(this PlayMakerFSM fsm, string intName)
        {
            FsmInt prev = fsm.FsmVariables.IntVariables.FirstOrDefault(x => x.Name == intName);

            if (prev != null)
                return prev;

            var @new = new FsmInt(intName);

            fsm.FsmVariables.IntVariables = fsm.FsmVariables.IntVariables.Append(@new).ToArray();

            return @new;
        }

        [PublicAPI]
        public static FsmBool CreateBool(this PlayMakerFSM fsm, string boolName)
        {
            var @new = new FsmBool(boolName);

            fsm.FsmVariables.BoolVariables = fsm.FsmVariables.BoolVariables.Append(@new).ToArray();

            return @new;
        }

        [PublicAPI]
        public static void AddToSendRandomEventV3
        (
            this SendRandomEventV3 sre,
            string toState,
            float weight,
            int eventMaxAmount,
            int missedMaxAmount,
            [CanBeNull] string eventName = null,
            bool createInt = true
        )
        {
            var fsm = sre.Fsm.Owner as PlayMakerFSM;

            string state = sre.State.Name;

            eventName ??= toState.Split(' ').First();

            fsm.GetState(state).AddTransition(eventName, toState);

            sre.events = sre.events.Append(fsm.GetState(state).Transitions.Single(x => x.FsmEvent.Name == eventName).FsmEvent).ToArray();
            sre.weights = sre.weights.Append(weight).ToArray();
            sre.trackingInts = sre.trackingInts.Append(fsm.GetOrCreateInt($"Ms {eventName}")).ToArray();
            sre.eventMax = sre.eventMax.Append(eventMaxAmount).ToArray();
            sre.trackingIntsMissed = sre.trackingIntsMissed.Append(fsm.GetOrCreateInt($"Ct {eventName}")).ToArray();
            sre.missedMax = sre.missedMax.Append(missedMaxAmount).ToArray();
        }

        [PublicAPI]
        public static FsmState CreateState(this PlayMakerFSM fsm, string stateName)
        {
            var state = new FsmState(fsm.Fsm)
            {
                Name = stateName
            };

            fsm.Fsm.States = fsm.FsmStates.Append(state).ToArray();

            return state;
        }
    }
}