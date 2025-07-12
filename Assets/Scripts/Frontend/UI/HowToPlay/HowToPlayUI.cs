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
        [SerializeField] private VisualTreeAsset tipButtonPrefab;

        [HideInInspector] public UnityEvent OnPlayerClosed = new UnityEvent();

        private VisualElement tipContent;

        private int currentIndex = 0;

        public void Start()
        {
            Button prev = howToPlayUI.rootVisualElement.Q<Button>("Prev");
            Button next = howToPlayUI.rootVisualElement.Q<Button>("Next");
            Button exit = howToPlayUI.rootVisualElement.Q<Button>("Exit");
            prev.RegisterCallback<ClickEvent>(PrevClicked);
            next.RegisterCallback<ClickEvent>(NextClicked);
            exit.RegisterCallback<ClickEvent>(BackButton_OnClick);

            tipContent = howToPlayUI.rootVisualElement.Q("TipContent");

            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;

            PopulateTips();
        }

        private void PopulateTips()
        {
            tipContent.Clear();
            for (int i = 0; i < data.descriptions.Length; ++i)
            {
                VisualElement button = CreateTipButton(i);
                tipContent.Add(button);
            }
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
                prev.style.visibility = Visibility.Hidden;
            }
            else
            {
                prev.style.visibility = Visibility.Visible;
            }
            
			if (index >= data.descriptions.Length - 1)
            {
                next.style.visibility = Visibility.Hidden;
            }
            else
            {
                next.style.visibility = Visibility.Visible;
            }

            description.text = data.descriptions[index];
            title.text = data.titles[index];
            currentIndex = index;
            videoPlayer.Stop();
            videoPlayer.clip = data.videoClips[index];
            videoPlayer.Play();
			
			// Disable the button for the current tip.
			int childIndex = 0;
			foreach (VisualElement tipButtonTemplate in tipContent.Children())
			{
				Button tipButton = tipButtonTemplate.Q<Button>("TipButton");
				tipButton.SetEnabled(childIndex != index);
				childIndex++;
			}
        }

        private void PrevClicked(ClickEvent evt)
        {
            SetUp(currentIndex - 1);
        }
        private void NextClicked(ClickEvent evt)
        {
            SetUp(currentIndex + 1);
        }

        private void TipButtonClicked(ClickEvent evt, int index)
        {
            SetUp(index);
        }

        private VisualElement CreateTipButton(int index)
        {
            PtUUISettings uiSettings = PtUUISettings.GetOrCreateSettings();

            string tipName = data.titles[index];
            VisualElement tipButtonElement = tipButtonPrefab.Instantiate();
            Button tipButton = tipButtonElement.Q<Button>("TipButton");
            tipButton.text = tipName;
            tipButton.RegisterCallback<ClickEvent, int>(TipButtonClicked, index);
            return (tipButtonElement);
        }
    }
}
