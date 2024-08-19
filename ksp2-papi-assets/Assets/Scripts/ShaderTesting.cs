using UnityEngine;

public class ShaderTesting : MonoBehaviour
{
    public Camera renderCam;
    public Shader unlit;
    public Shader add;
    public Shader depthCopy;
    public Shader generatePattern;

    public float width = 256;
    public float height = 96;

    private RenderTexture rt1;
    private RenderTexture rt2;
    private RenderTexture rt3;
    private Material matUnlit;
    private Material matAdd;
    private Material matDepthCopy;
    private Material matGeneratePattern;

    private void Awake()
    {
        matUnlit = new Material(unlit);
        matAdd = new Material(add);
        matDepthCopy = new Material(depthCopy);
        matGeneratePattern = new Material(generatePattern);

        rt1 = new RenderTexture(Screen.width, Screen.height, 24);
        rt2 = new RenderTexture(rt1);
        rt3 = new RenderTexture(rt1);
        rt1.Create();
        rt2.Create();
        rt3.Create();

        renderCam.targetTexture = rt1;
        renderCam.depthTextureMode |= DepthTextureMode.Depth;
    }

    private void LateUpdate()
    {
        renderCam.worldToCameraMatrix = Camera.main.worldToCameraMatrix;
        renderCam.projectionMatrix = Camera.main.projectionMatrix;
        // renderCam.Render();
        renderCam.RenderWithShader(unlit, "RenderType");
        Graphics.Blit(null, rt2, matGeneratePattern);
        matAdd.SetTexture("_Tex1", rt1, UnityEngine.Rendering.RenderTextureSubElement.Depth);
        matAdd.SetTexture("_Tex2", rt2, UnityEngine.Rendering.RenderTextureSubElement.Depth);
        Graphics.Blit(null, rt3, matAdd);
    }

    private void OnGUI()
    {
        GUILayout.Box(rt1, GUILayout.Width(width), GUILayout.Height(height));
        GUILayout.Box(rt2, GUILayout.Width(width), GUILayout.Height(height));
        GUILayout.Box(rt3, GUILayout.Width(width), GUILayout.Height(height));
    }

    private void OnDestroy()
    {
        rt1.Release();
        rt2.Release();
        rt3.Release();
    }
}
