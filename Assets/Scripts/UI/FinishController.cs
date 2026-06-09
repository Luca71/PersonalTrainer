using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace AllenamentoPersonale
{
    /// <summary>Controls the Finish / congratulations screen.</summary>
    public class FinishController
    {
        public VisualElement Root { get; }

        public FinishController(
            VisualElement root,
            Action onRepeat,
            Action onHome)
        {
            Root = root;

            root.Q<Button>("btn-repeat")
                ?.RegisterCallback<ClickEvent>(_ => onRepeat?.Invoke());

            root.Q<Button>("btn-home")
                ?.RegisterCallback<ClickEvent>(_ => onHome?.Invoke());

#if UNITY_ANDROID || UNITY_IOS
            // Hide or disable exit button on mobile – use device back gesture instead
            var btnExit = root.Q<Button>("btn-exit");
            if (btnExit != null) btnExit.style.display = DisplayStyle.None;
#else
            root.Q<Button>("btn-exit")
                ?.RegisterCallback<ClickEvent>(_ =>
                {
                    UnityEngine.Application.Quit();
                });
#endif
        }
    }
}
