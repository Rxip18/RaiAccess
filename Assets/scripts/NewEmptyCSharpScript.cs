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

    // --- New Section for Multiple Lines ---
    public string[] lines = {
        "Hello! Welcome to our VR world.",
        "We have a lot of things to see today.",
        "Follow me when you're ready."
    };

    int lineIndex = 0;
    CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();

        if (subtitleText != null && lines.Length > 0)
            SetSubtitle(lines[0]);

        // Auto-find camera if not assigned
        if (playerCamera == null)
        {
            var cam = Camera.main;
            if (cam != null) playerCamera = cam.transform;
        }

        var canvas = GetComponent<Canvas>();
        if (canvas) canvas.renderMode = RenderMode.WorldSpace;
    }

    void LateUpdate()
    {
        if (playerCamera == null || speaker == null) return;

        // Position above the speaker
        Vector3 targetPos = speaker.position + Vector3.up * verticalOffset + worldOffset;
        transform.position = targetPos;

        // Make canvas face the player camera correctly
        Vector3 forward = (transform.position - playerCamera.position).normalized;
        forward.y = 0f; // keep upright
        transform.rotation = Quaternion.LookRotation(forward);

        // Optional: fade with distance
        if (cg != null && maxDistance > 0f)
        {
            float d = Vector3.Distance(playerCamera.position, speaker.position);
            cg.alpha = Mathf.Clamp01(1f - Mathf.InverseLerp(maxDistance * 0.6f, maxDistance, d));
        }
    }

    // --- Switch subtitles manually with keyboard ---
    void Update()
    {
        // Press SPACE to go to the next subtitle
        if (Input.GetKeyDown(KeyCode.Space))
        {
            lineIndex = (lineIndex + 1) % lines.Length;
            SetSubtitle(lines[lineIndex]);
        }
    }

    // --- Function to update subtitle text ---
    public void SetSubtitle(string line)
    {
        if (subtitleText != null)
            subtitleText.text = line;

        if (headMountedText != null)
            headMountedText.text = line;
    }
}
