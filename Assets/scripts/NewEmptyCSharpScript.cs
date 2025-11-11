using UnityEngine;
using TMPro;

public class ContextSubtitle : MonoBehaviour
{
    [Header("Scene References")]
    public Transform playerCamera;           // XR Rig's Main Camera
    public Transform speaker;                // Your NPC/model/“speaker”
    public TextMeshProUGUI subtitleText;     // TMP text on this canvas

    [Header("Placement")]
    public float verticalOffset = 1.4f;      // Height above speaker
    public Vector3 worldOffset = Vector3.zero; // Fine-tune lateral offset
    public float maxDistance = 8f;           // Fade or clamp beyond this (optional)

    [Header("Fallback (optional)")]
    public Canvas headMountedCanvas;         // Small canvas parented to camera
    public TextMeshProUGUI headMountedText;  // TMP on fallback canvas
    public float showFallbackWhenDotBelow = 0f; // 0 = when speaker behind player

    [Header("Demo Line")]
    [TextArea]
    public string demoLine = "Hello! Welcome to our VR world.";

    CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
        if (subtitleText != null && !string.IsNullOrEmpty(demoLine))
            subtitleText.text = demoLine;

        if (headMountedCanvas != null && headMountedText != null)
            headMountedText.text = demoLine;
        
        // Ensure world-space
        var canvas = GetComponent<Canvas>();
        if (canvas) canvas.renderMode = RenderMode.WorldSpace;
    }

    void LateUpdate()
    {
        if (playerCamera == null || speaker == null) return;

        // 1) Position above speaker + optional offset
        Vector3 targetPos = speaker.position + Vector3.up * verticalOffset + worldOffset;
        transform.position = targetPos;

        // 2) Billboard: face the camera (without pitch/roll if preferred)
        Vector3 toCam = (playerCamera.position - transform.position).normalized;
        toCam.y = 0f; // keep upright; comment this out to face fully
        if (toCam.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(toCam);

        // 3) Optional: fade if too far
        if (cg != null && maxDistance > 0f)
        {
            float d = Vector3.Distance(playerCamera.position, speaker.position);
            cg.alpha = Mathf.Clamp01(1f - Mathf.InverseLerp(maxDistance * 0.6f, maxDistance, d));
        }

        // 4) Fallback caption when user looks away
        if (headMountedCanvas != null && headMountedText != null)
        {
            Vector3 toSpeaker = (speaker.position - playerCamera.position).normalized;
            float dot = Vector3.Dot(playerCamera.forward, toSpeaker);
            bool facingAway = dot < showFallbackWhenDotBelow;
            headMountedCanvas.enabled = facingAway;
        }
    }

    // Helper to set text at runtime
    public void SetSubtitle(string line)
    {
        if (subtitleText != null) subtitleText.text = line;
        if (headMountedText != null) headMountedText.text = line;
    }
}
