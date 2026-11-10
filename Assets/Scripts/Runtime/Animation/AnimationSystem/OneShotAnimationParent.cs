using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GP
{
    public class OneShotAnimationParent : PlayableBehaviour
    {
        private PlayableGraph m_Graph;
        private List<IAnimationState> m_ActiveAnimationStates;
        private Playable m_Self;
        private ScriptPlayable<AnimationNotifyPlayable> m_NotifyPlayable;
        private GameObject m_AnimationTarget;

        public IAnimationState RunningAnimationState { get; private set; }
        public bool IsRunning => !RunningAnimationState?.IsDone ?? false;
        public Playable ParentPlayable => m_Self;
        public INotificationReceiver NotifyReceiver => m_NotifyPlayable.IsValid() ? m_NotifyPlayable.GetBehaviour() : null;
        public GameObject AnimationTarget => m_AnimationTarget;

        public float Weight
        {
            get
            {
                var weight = 0f;

                foreach (var state in m_ActiveAnimationStates)
                {
                    weight = Mathf.Max(weight, state.Weight);
                }

                return weight;
            }
        }

        public static ScriptPlayable<OneShotAnimationParent> Create(PlayableGraph graph, Animator animator)
        {
            var handle = ScriptPlayable<OneShotAnimationParent>.Create(graph, 1);
            handle.GetBehaviour().Init(graph, animator);
            return handle;
        }

        public override void OnPlayableCreate(Playable playable)
        {
            base.OnPlayableCreate(playable);
            m_Self = playable;
        }

        public void PlayAnimation(AnimationAsset animationAsset)
        {
            BuildAndPlayState(animationAsset, true);
        }

        public bool InterruptAnimation(InterruptRequest interruptRequest)
        {
            return RunningAnimationState?.InterruptAnimation(interruptRequest) ?? false;
        }

        public bool InterruptAnimation(string animationName, InterruptRequest interruptRequest)
        {
            if (RunningAnimationState == null) return false;

            return RunningAnimationState.MatchesAnimationName(animationName) && InterruptAnimation(interruptRequest);
        }

        public void ForceInterruptAnimation(float easeOutDuration = .1f)
        {
            RunningAnimationState?.ForceInterruptAnimation(easeOutDuration);
        }

        public void RemoveAnimationState(IAnimationState animationState)
        {
            if (animationState == null) return;
            if (!animationState.IsDone) return;
            if (!m_ActiveAnimationStates.Contains(animationState)) return;

            if (RunningAnimationState == animationState) RunningAnimationState = null;
            m_ActiveAnimationStates.Remove(animationState);

        }

        public void Destroy()
        {
            if (m_NotifyPlayable.IsValid())
                m_NotifyPlayable.GetBehaviour().ClearAnimationNotifies();

            for (var i = m_ActiveAnimationStates.Count - 1; i >= 0; i--)
            {
                m_ActiveAnimationStates[i]?.OnDestroy();
            }

            m_ActiveAnimationStates.Clear();
        }

        public void InterruptAnimationNotify(IAnimationState source)
        {
            if (source != RunningAnimationState || !m_NotifyPlayable.IsValid()) return;

            m_NotifyPlayable.GetBehaviour().InterruptNotifies();
        }

        private void Init(PlayableGraph graph, Animator animator)
        {
            m_Graph = graph;
            m_AnimationTarget = animator.transform.root.gameObject;
            m_ActiveAnimationStates = new List<IAnimationState>();

            BuildNotifyPlayable();
        }

        private void BuildAndPlayState(AnimationAsset animationAsset, bool interruptLastAnimation)
        {
            if (animationAsset == null || animationAsset.AnimationClip == null) return;

            if (interruptLastAnimation)
            {
                RunningAnimationState?.ForceInterruptAnimation(.15f);
            }

            var animationHandle = OneShotAnimationState.Create(m_Graph, animationAsset);

            if (animationHandle.GetBehaviour() == null) return;

            animationHandle.GetBehaviour().BuildAnimationState(this);

            m_ActiveAnimationStates.Add(animationHandle.GetBehaviour());

            RunningAnimationState = animationHandle.GetBehaviour();

            // 即使新动画没有通知，也必须清理上一次播放的通知
            m_NotifyPlayable.GetBehaviour().SetAnimationNotifies(animationAsset, RunningAnimationState);
        }

        private void BuildNotifyPlayable()
        {
            m_NotifyPlayable = AnimationNotifyPlayable.Create(m_Graph, m_AnimationTarget);
            m_Self.ConnectInput(0, m_NotifyPlayable, 0);
            m_Self.SetInputWeight(0, 1f);
        }
    }
}
