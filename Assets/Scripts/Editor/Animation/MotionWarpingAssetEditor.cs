using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GP
{
    [CustomEditor(typeof(MotionWarpingAsset))]
    public class MotionWarpingAssetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var asset = (MotionWarpingAsset)target;
            if (GUILayout.Button("采样动画数据")) Sample(asset);
            if (GUILayout.Button("清理数据"))
            {
                Undo.RecordObject(asset, "Clear Motion Warping Data");
                foreach (var window in asset.WarpingWindows) window.ClearData();
                EditorUtility.SetDirty(asset);
            }
        }

        public static void Sample(MotionWarpingAsset asset)
        {
            if (!asset.SampleClip || !asset.SampleModel)
            {
                Debug.LogError("MotionWarpingAsset 缺少采样剪辑或模型，请在 Inspector 中配置后重新采样。", asset);
                return;
            }
            foreach (var window in asset.WarpingWindows)
                if (window == null || window.WarpWindow.x < 0 || window.WarpWindow.y < window.WarpWindow.x)
                    throw new ArgumentException("Warping 窗口必须满足 0 <= 开始帧 <= 结束帧。");

            var model = Instantiate(asset.SampleModel);
            model.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                var samples = new List<WarpFrameData[]>();
                foreach (var window in asset.WarpingWindows)
                    samples.Add(CalculatePhaseFrameData(model, asset.SampleClip, window.WarpWindow.x, window.WarpWindow.y));
                Undo.RecordObject(asset, "Sample Motion Warping Data");
                for (int i = 0; i < samples.Count; i++) asset.WarpingWindows[i].WriteData(i, samples[i]);
                EditorUtility.SetDirty(asset);
            }
            finally { DestroyImmediate(model); }
        }

        private static WarpFrameData[] CalculatePhaseFrameData(GameObject model, AnimationClip clip, int startFrame, int endFrame)
        {
            var range = endFrame + 1 - startFrame;
            var frameData = new WarpFrameData[range];

            for (int i = startFrame; i <= endFrame; ++i)
            {
                // 计算从开始窗口期到结束窗口期的运动量
                var frameMotionData =
                    model.transform.InverseTransformDirection(
                        CalculateFrameDeltaMotionData(model, clip, i, endFrame, clip.frameRate));

                frameData[i - startFrame] = new WarpFrameData(i, frameMotionData);
            }

            return frameData;
        }

        private static Vector3 CalculateFrameDeltaMotionData(GameObject model, AnimationClip clip, int currentFrame, int endFrame, float frameRate)
        {
            var position = Vector3.zero;

            for (int i = currentFrame; i < endFrame; ++i)
            {
                // 将帧转为时间
                var currentTime = i / frameRate;
                var nextTime = (i + 1) / frameRate;
                var deltaPosition = CalculateDeltaPosition(currentTime, nextTime);
                position += deltaPosition;
            }

            return position;

            Vector3 CalculateDeltaPosition(float currentTime, float nextTime)
            {
                var root = model.transform;
                var originPosition = root.position;
                var originRotation = root.rotation;

                // 采样当前时间点的数据
                clip.SampleAnimation(model, currentTime); // 采样模型这个时间点的动画数据
                var currentPos = root.position;
                var currentRot = root.rotation;

                // 采样下一个时间点的数据
                clip.SampleAnimation(model, nextTime);
                var deltaPosition = root.position - currentPos;

                root.SetPositionAndRotation(originPosition, originRotation);
                // 计算考虑旋转情况下的位置偏移量
                return currentRot * deltaPosition;
            }
        }
    }
}
