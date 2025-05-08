using AstralAvarice.Backend;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace AstralAvarice.Frontend
{
    public class HowToPlayUI : MonoBehaviour
    {
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RenderTexture videoRenderTexture;
        [SerializeField] private UIDocument howToPlayUI;
        [SerializeField] private HowToPlayData data;

        [HideInInspector] public UnityEvent OnPlayerClosed = new UnityEvent();

        private int currentIndex = 0;

        public void Start()
        {
            Button prev = howToPlayUI.rootVisualElement.Q<Button>("Prev");
            Button next = howToPlayUI.rootVisualElement.Q<Button>("Next");
            Button exit = howToPlayUI.rootVisualElement.Q<Button>("Exit");
            prev.RegisterCallback<ClickEvent>(PrevClicked);
            next.RegisterCallback<ClickEvent>(NextClicked);
            exit.RegisterCallback<ClickEvent>(BackButton_OnClick);
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
        }

        public void Show()
        {
            howToPlayUI.rootVisualElement.style.display = DisplayStyle.Flex;
            howToPlayUI.sortingOrder = 128;
            SetUp(currentIndex);
        }

        public void Hide()
        {
            howToPlayUI.rootVisualElement.style.display = DisplayStyle.None;
        }

        private void BackButton_OnClick(ClickEvent evt)
        {
            Hide();
            OnPlayerClosed?.Invoke();
        }

        public void SetUp(int index)
        {
            VisualElement video = howToPlayUI.rootVisualElement.Q("Video");
            Label description = howToPlayUI.rootVisualElement.Q<Label>("Description");
            Label title = howToPlayUI.rootVisualElement.Q<Label>("Title");
            Button prev = howToPlayUI.rootVisualElement.Q<Button>("Prev");
            Button next = howToPlayUI.rootVisualElement.Q<Button>("Next");

            if (index <= 0)
            {
                prev.style.display = DisplayStyle.None;
            }
            else
            {
                prev.style.display = DisplayStyle.Flex;
            }
            if (index >= data.descriptions.Length - 1)
            {
                next.style.display = DisplayStyle.None;
            }
            else
            {
                next.style.display = DisplayStyle.Flex;
            }
            description.text = data.descriptions[index];
            title.text = data.titles[index];
            currentIndex = index;
            videoPlayer.Stop();
            videoPlayer.clip = data.videoClips[index];
            videoPlayer.Play();
        }

        private void PrevClicked(ClickEvent evt)
        {
            SetUp(currentIndex - 1);
        }
        private void NextClicked(ClickEvent evt)
        {
            SetUp(currentIndex + 1);
        }
    }
}
