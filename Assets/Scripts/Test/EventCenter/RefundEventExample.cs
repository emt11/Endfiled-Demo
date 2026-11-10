using UnityEngine;
using UnityEngine.InputSystem;

namespace GP
{
    public class RefundEventExample : MonoBehaviour
    {
        [SerializeField] private int m_RefundAmount = 100;

        private int m_Balance;
        private IEventToken m_RefundToken;

        private void OnEnable()
        {
            // 1. 订阅：HandleRefund 是回调方法，RefundEvent 是它接收的数据类型
            // 编译器会把 HandleRefund 转换成 EventCenter<RefundEvent>.EventDelegate
            // 返回的 Token 用于取消本次订阅
            m_RefundToken = EventCenter<RefundEvent>.Subscribe(HandleRefund);
        }

        private void Start()
        {
            EMTLog.Log("退款演示：先自动退款一次，之后点击 Game 窗口并按空格再次退款。");
            PublishRefund();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
            {
                PublishRefund();
            }
        }

        private void OnDisable()
        {
            // 4. 注销：与 OnEnable 配对，避免重新启用组件时重复订阅
            m_RefundToken?.Unsubscribe();
            m_RefundToken = null;
        }

        private void PublishRefund()
        {
            // 2. 发布：只发送“退款多少”的数据，由事件中心通知订阅者
            var refund = new RefundEvent(m_RefundAmount);
            EMTLog.Log($"[发布通知] 退款 {refund.Amount} 元。");
            EventCenter<RefundEvent>.Publish(refund);
        }

        private void HandleRefund(RefundEvent refund)
        {
            // 3. 回调：接收数据，并执行实际逻辑
            m_Balance += refund.Amount;
            EMTLog.Log($"[钱包收到通知] 增加 {refund.Amount} 元，当前余额：{m_Balance} 元。");
        }
    }

    // 事件数据：只描述“发生了什么”，不负责执行退款逻辑
    public readonly struct RefundEvent : IEventContext
    {
        public readonly int Amount;

        public RefundEvent(int amount)
        {
            Amount = amount;
        }
    }
}
