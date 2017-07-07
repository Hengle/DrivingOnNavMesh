﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public abstract class AbstractRoutetedInterceptor : RootMotionInterceptor {
        public const float RADIUS = 10f;
        public enum StateEnum { Idol = 0, Navigation }

        public event System.Action<AbstractRoutetedInterceptor> NavigationStart;
        public event System.Action<AbstractRoutetedInterceptor> NavigationComplete;
        public event System.Action<AbstractRoutetedInterceptor> NavigationAbort;
        public event System.Action<AbstractRoutetedInterceptor> NavigationExit;

        protected StateEnum state;
        protected AbstractRouter router;

        public AbstractRoutetedInterceptor(
            Animator anim, Transform tr, DrivingSetting drivingSetting, AbstractRouter router)
             : base(anim, tr, drivingSetting) {
            this.router = router;
        }

        #region Abstract
        protected abstract bool TryToStartNavigationTo (Vector3 destination);
        protected abstract void UpdateTarget (Vector3 pointFrom, float t);
        #endregion

        public virtual bool NavigateTo(Vector3 destination) {
            AbortNavigation ();
            var result = TryToStartNavigationTo (destination);
            if (result)
                StartNavigation ();
            return result;
        }
        public virtual void UpdateNavigation () {
            if (state != StateEnum.Navigation)
                return;

            var pointFrom = _tr.position;
            var t = router.ClosestT (pointFrom);
            if (t < 0f) {
                AbortNavigation ();
                return;
            }

            router.ActiveRange.SetRangeBegin (t);
            if (router.ActiveRange.Length <= Mathf.Epsilon) {
                CompleteNavigation ();
                return;
            }

            UpdateTarget (pointFrom, t);
        }

        public void DrawPath() {
            if (router != null || state == StateEnum.Navigation)
                router.DrawGizmos();
        }
        public void DrawTarget() {
            if (ActiveAndValid)
                Gizmos.DrawLine (_tr.position, Destination);
        }

        protected virtual void GotoNavigationState(StateEnum nextState) {
            state = nextState;
        }

        #region Action
        protected virtual void StartNavigation() {
            if (state == StateEnum.Idol) {
                GotoNavigationState (StateEnum.Navigation);
                NotifyNavigationStart ();
            }
        }
        protected virtual void CompleteNavigation() {
            if (state == StateEnum.Navigation) {
                GotoNavigationState(StateEnum.Idol);
                ResetTarget ();
                NotifyNavigationComplete ();
                NotifyNavigationExit ();
            }
        }
        protected virtual void AbortNavigation() {
            if (state == StateEnum.Navigation) {
                GotoNavigationState(StateEnum.Idol);
                ResetTarget ();
                NotifyNavigationAbort ();
                NotifyNavigationExit ();
            }
        }
        #endregion

        #region Event
        protected void NotifyNavigationStart() {
            if (NavigationStart != null)
                NavigationStart (this);
        }
        protected void NotifyNavigationAbort() {
            if (NavigationAbort != null)
                NavigationAbort (this);
        }
        protected void NotifyNavigationComplete() {
            if (NavigationComplete != null)
                NavigationComplete (this);
        }
        protected void NotifyNavigationExit() {
            if (NavigationExit != null)
                NavigationExit (this);
        }
        #endregion

    }
}