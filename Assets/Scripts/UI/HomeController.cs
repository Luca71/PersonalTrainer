using System;
using UnityEngine.UIElements;

namespace AllenamentoPersonale
{
    /// <summary>Controls the Home screen visual tree.</summary>
    public class HomeController
    {
        public VisualElement Root { get; }

        public HomeController(
            VisualElement root,
            int exerciseCount,
            Action onStart,
            Action onOpenEditor)
        {
            Root = root;

            // Subtitle
            var subtitle = root.Q<Label>("lbl-subtitle");
            if (subtitle != null)
                subtitle.text = $"{exerciseCount} esercizi · riscaldamento + forza";

            // Start button
            var btnStart = root.Q<Button>("btn-start");
            btnStart?.RegisterCallback<ClickEvent>(_ => onStart?.Invoke());

            // Editor button
            var btnEditor = root.Q<Button>("btn-editor");
            btnEditor?.RegisterCallback<ClickEvent>(_ => onOpenEditor?.Invoke());
        }
    }
}
