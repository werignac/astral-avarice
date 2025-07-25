using AstralAvarice.Backend;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace AstralAvarice.Frontend
{
    public class HowToPlayUI : MonoBehaviour
    {
		private class HowToPlayButtonColorManipulator : PointerManipulator
		{
			private Color _mainColor;
			private Color _unhoveredTint;
			private Color _hoveredTint;
			private Color _pressedTint;
			private Color _disabledTint;

			private bool _pointerHovering = false;
			private bool _pointerPressing = false;

			public HowToPlayButtonColorManipulator(Color mainColor, Color unhoveredTint, Color hoveredTint, Color pressedTint, Color disabledTint)
			{
				_mainColor = mainColor;
				_unhoveredTint = unhoveredTint;
				_hoveredTint = hoveredTint;
				_pressedTint = pressedTint;
				_disabledTint = disabledTint;

				// No activation filters. Always active?
			}

			protected override void RegisterCallbacksOnTarget()
			{
				target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
				target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
				target.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
				target.RegisterCallback<PointerUpEvent>(OnPointerUp);
			}

			protected override void UnregisterCallbacksFromTarget()
			{
				target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
				target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
				target.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
				target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
			}

			private void OnPointerUp(PointerUpEvent evt)
			{
				_pointerPressing = false;
				UpdateTint();
			}

			private void OnPointerDown(PointerDownEvent evt)
			{
				_pointerPressing = true;
				UpdateTint();
			}

			private void OnPointerLeave(PointerLeaveEvent evt)
			{
				_pointerHovering = false;
				UpdateTint();
			}

			private void OnPointerEnter(PointerEnterEvent evt)
			{
				_pointerHovering = true;
				UpdateTint();
			}

			public void UpdateTint()
			{
				SetTint(GetTint());
			}

			private Color GetTint()
			{
				if (!target.enabledSelf)
					return _disabledTint;

				if (!_pointerHovering)
					return _unhoveredTint;

				if (!_pointerPressing)
					return _hoveredTint;

				return _pressedTint;
			}

			private void SetTint(Color tint)
			{
				Color tintedColor = _mainColor * tint;
				target.style.unityBackgroundImageTintColor = tintedColor;
			}
		}

		private class HowToPlayButtonColorManipulatorFactory
		{
			private Color _mainColor;
			private Color _unhoveredColor;
			private Color _hoveredColor;
			private Color _pressedColor;
			private Color _disabledColor;

			public HowToPlayButtonColorManipulatorFactory(
				Color mainColor,
				Color unhoveredColor,
				Color hoveredColor,
				Color pressedColor,
				Color disabledColor
				)
			{
				_mainColor = mainColor;
				_unhoveredColor = unhoveredColor;
				_hoveredColor = hoveredColor;
				_pressedColor = pressedColor;
				_disabledColor = disabledColor;
			}

			public HowToPlayButtonColorManipulator Make()
			{
				return new HowToPlayButtonColorManipulator(
					_mainColor,
					_unhoveredColor,
					_hoveredColor,
					_pressedColor,
					_disabledColor
				);
			}

		}

		private class HowToMenuButtonBinding
		{
			private const string TIP_BUTTON_ELEMENT_NAME = "TipButton";

			protected VisualElement _visualElement;
			protected Button _button;
			protected HowToPlayButtonColorManipulator _colorManipulator;

			public HowToMenuButtonBinding(VisualElement visualElement, HowToPlayButtonColorManipulatorFactory manipulatorFactory)
			{
				_visualElement = visualElement;
				_button = visualElement.Q<Button>(TIP_BUTTON_ELEMENT_NAME);

				_colorManipulator = manipulatorFactory.Make();
				_button.AddManipulator(_colorManipulator);
				_colorManipulator.UpdateTint();
			}
		}

		private class CategoryButtonBinding : HowToMenuButtonBinding
		{
			public HowToPlayCategory_SO Category { get; private set; }
			private List<TipButtonBinding> _tips = new List<TipButtonBinding>(); // NOTE: Circular reference.
			public bool IsOpen { get; private set; } = true;

			public UnityEvent<CategoryButtonBinding> OnClick = new UnityEvent<CategoryButtonBinding>();

			public CategoryButtonBinding(
				HowToPlayCategory_SO category,
				VisualElement visualElement,
				HowToPlayButtonColorManipulatorFactory manipulatorFactory
				) : base(visualElement, manipulatorFactory)
			{
				Category = category;
				_button.text = Category.categoryName;
				_button.RegisterCallback<ClickEvent>(OnButtonClicked);
			}

			private void OnButtonClicked(ClickEvent evt)
			{
				OnClick.Invoke(this);
			}

			public void AddChild(TipButtonBinding tip)
			{
				_tips.Add(tip);
				tip.SetParent(this);
			}

			public void Open()
			{
				IsOpen = true;

				foreach (TipButtonBinding tip in _tips)
					tip.Show();
			}

			public void Close()
			{
				IsOpen = false;

				foreach (TipButtonBinding tip in _tips)
					tip.Hide();
			}

		}

		private class TipButtonBinding : HowToMenuButtonBinding
		{
			private const string TABBED_CLASS_NAME = "tabbed";

			public HowToPlayTip Tip { get; private set; }
			public CategoryButtonBinding Parent { get; private set; } // NOTE: Circular reference.
			public int Id { get; private set; }

			public UnityEvent<TipButtonBinding> OnClick = new UnityEvent<TipButtonBinding>();

			public TipButtonBinding(
				HowToPlayTip tip,
				VisualElement visualElement,
				HowToPlayButtonColorManipulatorFactory manipulatorFactory,
				int id
				) : base(visualElement, manipulatorFactory)
			{
				Tip = tip;
				_button.AddToClassList(TABBED_CLASS_NAME);
				_button.text = tip.title;
				_button.RegisterCallback<ClickEvent>(OnButtonClicked);

				Id = id;
			}

			private void OnButtonClicked(ClickEvent evt)
			{
				OnClick.Invoke(this);
			}

			public void SetParent(CategoryButtonBinding parent)
			{
				Parent = parent;
			}

			public void Show()
			{
				_visualElement.style.display = DisplayStyle.Flex;
			}

			public void Hide()
			{
				_visualElement.style.display = DisplayStyle.None;
			}

			public void SetEnabled(bool enabled)
			{
				_button.SetEnabled(enabled);
				_colorManipulator.UpdateTint();
			}
		}

		[SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RenderTexture videoRenderTexture;
        [SerializeField] private UIDocument howToPlayUI;
        [SerializeField] private HowToPlayData data;
        [SerializeField] private VisualTreeAsset tipButtonPrefab;

        [HideInInspector] public UnityEvent OnPlayerClosed = new UnityEvent();

        private VisualElement tipContent;

        private int currentIndex = 0;

		private TipButtonBinding[] _tips;
		private CategoryButtonBinding[] _categories;

        public void Start()
        {
            Button prev = howToPlayUI.rootVisualElement.Q<Button>("Prev");
            Button next = howToPlayUI.rootVisualElement.Q<Button>("Next");
            Button exit = howToPlayUI.rootVisualElement.Q<Button>("BackButton");
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
			int categoryCount = data.categories.Length;
			int tipCount = 0;

			foreach (HowToPlayCategory_SO category in data.categories)
				tipCount += category.tips.Length;

			_categories = new CategoryButtonBinding[categoryCount];
			_tips = new TipButtonBinding[tipCount];

            tipContent.Clear();

			int categoryIndex = 0;
			int tipIndex = 0;

			for (; categoryIndex < categoryCount; categoryIndex++)
			{
				HowToPlayCategory_SO category = data.categories[categoryIndex];
				var manipulatorFactory = new HowToPlayButtonColorManipulatorFactory(
					category.Color,
					data.UnhoveredTint,
					data.HoveredTint,
					data.PressedTint,
					data.DisabledTint
				);
				VisualElement categoryButton = CreateTipButton();
				CategoryButtonBinding categoryButtonBinding = new CategoryButtonBinding(category, categoryButton, manipulatorFactory);

				for (int tipSubindex = 0; tipSubindex < category.tips.Length; tipSubindex++)
				{
					HowToPlayTip tip = category.tips[tipSubindex];
					VisualElement tipButton = CreateTipButton();
					TipButtonBinding tipButtonBinding = new TipButtonBinding(tip, tipButton, manipulatorFactory, tipIndex + tipSubindex);

					categoryButtonBinding.AddChild(tipButtonBinding);
					_tips[tipIndex + tipSubindex] = tipButtonBinding;
					tipButtonBinding.OnClick.AddListener(OnTipClicked);
				}

				tipIndex += category.tips.Length;
				_categories[categoryIndex] = categoryButtonBinding;
				categoryButtonBinding.OnClick.AddListener(OnCategoryClicked);
			}
        }

		private void OnCategoryClicked(CategoryButtonBinding category)
		{
			if (!category.IsOpen)
				category.Open();
			else
				category.Close();
		}

		private void OnTipClicked(TipButtonBinding tip)
		{
			SetUp(tip.Id);
		}

		public void Show()
        {
            howToPlayUI.rootVisualElement.style.display = DisplayStyle.Flex;
            howToPlayUI.sortingOrder = 128;
            SetUp(currentIndex);

			foreach (CategoryButtonBinding category in _categories)
			{
				if (category == _tips[currentIndex].Parent)
					category.Open();
				else
					category.Close();
			}
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
			// TODO: Get these ahead of time?
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
            
			if (index >= _tips.Length - 1)
            {
                next.style.visibility = Visibility.Hidden;
            }
            else
            {
                next.style.visibility = Visibility.Visible;
            }

			// Disable the button for the current tip.
			TipButtonBinding lastTipButtonBinding = _tips[currentIndex];
			TipButtonBinding tipButtonBinding = _tips[index];

			lastTipButtonBinding.SetEnabled(true);
			tipButtonBinding.SetEnabled(false);

			// Open the tip's category if closed.
			if (!tipButtonBinding.Parent.IsOpen)
				tipButtonBinding.Parent.Open();

			HowToPlayTip tip = tipButtonBinding.Tip;

			description.text = tip.description;
            title.text = tip.title;
            currentIndex = index;
            videoPlayer.Stop();
            videoPlayer.clip = tip.videoClip;
            videoPlayer.Play();

			currentIndex = index;
        }

        private void PrevClicked(ClickEvent evt)
        {
            SetUp(currentIndex - 1);
        }
        private void NextClicked(ClickEvent evt)
        {
            SetUp(currentIndex + 1);
        }

        private VisualElement CreateTipButton()
        {
            VisualElement tipButtonElement = tipButtonPrefab.Instantiate();
			tipContent.Add(tipButtonElement);
			return tipButtonElement;
        }

    }
}
