using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class ClickToTranscribe : MonoBehaviour
{
    private void OnMouseDown()
    {
        TypewriterKey.CanType = true;
    }
}
