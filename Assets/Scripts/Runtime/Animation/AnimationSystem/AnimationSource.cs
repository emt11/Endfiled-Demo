using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GP
{
    public class AnimationSource : MonoBehaviour
    {
        private const string AnimationOutputName = "AnimateOut";
        [Space, SerializeField, BoxGroup("Set", showLabel: false)]
        private bool m_AutoCreate = true;

        [SerializeField, BoxGroup("Set", showLabel: false)]
        private bool m_PlayOnAwake = true;

        [SerializeField, BoxGroup("Set", showLabel: false)]
        private bool m_RootMotionByAnimator;

        [SerializeField, BoxGroup("Set", showLabel: false)]
        private string m_GraphName = "自定义 Playable";
        public AnimationAsset AnimationAsset;

        private Animator m_Animator;
        private AnimationMotionWarpingControl m_WarpingControl;
        private bool m_IsCreated;

        // Playable
        private PlayableGraph m_Graph;
        private AnimationPlayableOutput m_AnimationOutput;
        private AnimatorControllerPlayable m_ControllerPlayable;
        private ScriptPlayable<OneShotAnimationParent> m_OneShotAnimationParent;

        public Animator SourceAnimator => m_Animator;

        public bool IsAnimationRunning
        {
            get
            {
                if (!m_OneShotAnimationParent.IsValid()) return false;

                return m_OneShotAnimationParent.GetBehaviour()?.IsRunning ?? false;
            }
        }

        public IAnimationState RunningState
        {
            get
            {
                if (!m_OneShotAnimationParent.IsValid()) return null;

                return m_OneShotAnimationParent.GetBehaviour()?.RunningAnimationState;
            }
        }

        public float RootMotionWeight { get; private set; }

        public (Vector3 PositionDelta, Quaternion RotationDelta) RootMotion { get; private set; }
            = (Vector3.zero, Quaternion.identity);
        public bool WarpingRunning => m_WarpingControl?.WarpingStatus == WarpingStatus.Running;

        private void Awake()
        {
            m_Animator = GetComponentInChildren<Animator>();
            m_WarpingControl = new AnimationMotionWarpingControl(transform.root);

            if (m_AutoCreate)
                CreateGraph();
        }

        private void OnDisable()
        {
            ResetRootMotion();
            m_WarpingControl?.InterruptWarping();
        }

        private void OnDestroy()
        {
            if (m_OneShotAnimationParent.IsValid())
                m_OneShotAnimationParent.GetBehaviour().Destroy();

            if (m_Graph.IsValid())
                m_Graph.Destroy();

            m_IsCreated = false;
        }

        public void CreateGraph()
        {
            if (m_IsCreated) return;

            if (m_Graph.IsValid())
                m_Graph.Destroy();

            if (!m_Animator) return;

            m_Graph = PlayableGraph.Create(m_GraphName);

            if (m_Animator.runtimeAnimatorController)
                m_ControllerPlayable = AnimatorControllerPlayable.Create(m_Graph, m_Animator.runtimeAnimatorController);

            m_OneShotAnimationParent = OneShotAnimationParent.Create(m_Graph, m_Animator);

            var notifyPlayable = m_OneShotAnimationParent.GetInput(0);
            if (m_ControllerPlayable.IsValid())
            {
                notifyPlayable.ConnectInput(0, m_ControllerPlayable, 0);
                notifyPlayable.SetInputWeight(0, 1f);
            }

            m_AnimationOutput = AnimationPlayableOutput.Create(m_Graph, AnimationOutputName, m_Animator);
            m_AnimationOutput.SetSourcePlayable(m_OneShotAnimationParent); // 设置播放源
            m_AnimationOutput.AddNotificationReceiver(m_OneShotAnimationParent.GetBehaviour().NotifyReceiver);
            m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime); // 设置 Playable 时间更新模式

            m_Animator.runtimeAnimatorController = null;

            if (m_PlayOnAwake)
                m_Graph.Play();

            m_IsCreated = true;
        }

        public void PlayAnimation(AnimationAsset animationAsset)
        {
            if (!animationAsset) return;
            if (!m_OneShotAnimationParent.IsValid()) return;

            ResetRootMotion();
            if (m_WarpingControl.EnableWarping) m_WarpingControl.InterruptWarping();
            m_OneShotAnimationParent.GetBehaviour().PlayAnimation(animationAsset);
        }

        public void CancelAnimation(AnimationAsset animationAsset)
        {
            if (!animationAsset) return;
            if (!m_OneShotAnimationParent.IsValid()) return;

            if (m_OneShotAnimationParent.GetBehaviour().InterruptAnimation(
                new InterruptRequest(animationAsset.AnimationPriority, animationAsset.EaseOutDuration)))
            {
                m_WarpingControl.InterruptWarping();
                ResetRootMotion();
            }
        }

        public void ForceStopAnimation()
        {
            if (!m_OneShotAnimationParent.IsValid()) return;

            m_WarpingControl.InterruptWarping();
            ResetRootMotion();
            m_OneShotAnimationParent.GetBehaviour().ForceInterruptAnimation();
        }

        public bool Warp(IWarpInteraction interaction)
        {
            return m_WarpingControl.Interact(interaction);
        }

        private void OnAnimatorMove()
        {
            ResetRootMotion();
            var state = RunningState;
            if (state == null || state.IsDone)
            {
                m_WarpingControl.InterruptWarping();
                return;
            }
            m_WarpingControl.ProcessWarpingStatus(state.Frame);
            if (m_WarpingControl.WarpingStatus == WarpingStatus.Running)
            {
                do
                {
                    transform.root.position = m_WarpingControl.WarpingTranslate(m_Animator.deltaPosition, state.Frame);
                    transform.root.rotation = m_WarpingControl.WarpRotation(state.Frame);
                } while (m_WarpingControl.CompleteFrame(state.Frame));
                Physics.SyncTransforms();
                return;
            }
            if (!state.UseRootMotion) return;
            if (m_RootMotionByAnimator)
            {
                m_Animator.ApplyBuiltinRootMotion();
                return;
            }
            RootMotionWeight = m_OneShotAnimationParent.GetBehaviour().Weight;
            RootMotion = (m_Animator.deltaPosition, m_Animator.deltaRotation);
        }

        private void ResetRootMotion()
        {
            RootMotion = (Vector3.zero, Quaternion.identity);
            RootMotionWeight = 0f;
        }
    }
}
