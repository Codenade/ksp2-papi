// modified version of https://forum.unity.com/threads/solved-how-can-i-count-the-number-of-pixels-above-a-threshold-with-a-shader.763232/#post-5084408

using UnityEngine;

namespace ksp2_papi
{
    public static class CSFlareUtil
    {
        public static int[] AnalyzeImage(ComputeShader cs, RenderTexture rt, RenderTexture mask)
        {
            ComputeBuffer cb;
            int[] analysisResult;
            int kernelInit, kernelMain;
            kernelInit = cs.FindKernel("CSInit");
            kernelMain = cs.FindKernel("CSMain");
            cb = new ComputeBuffer(1, sizeof(int) * 2);
            analysisResult = new int[2];
            cs.SetTexture(kernelInit, "InputImage", rt);
            cs.SetTexture(kernelMain, "InputImage", rt);
            cs.SetTexture(kernelInit, "Mask", mask);
            cs.SetTexture(kernelMain, "Mask", mask);
            cs.SetBuffer(kernelInit, "ResultBuffer", cb);
            cs.SetBuffer(kernelMain, "ResultBuffer", cb);
            cs.Dispatch(kernelInit, 1, 1, 1);
            cs.Dispatch(kernelMain, rt.width / 8, rt.height / 8, 1);
            cb.GetData(analysisResult);
            cb.Release();
            return analysisResult;
        }
    }
}