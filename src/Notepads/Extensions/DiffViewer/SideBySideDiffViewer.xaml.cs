﻿
namespace Notepads.Extensions.DiffViewer
{
    using Notepads.Commands;
    using System;
    using System.Collections.Generic;
    using Windows.System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;


    public sealed partial class SideBySideDiffViewer : Page, ISideBySideDiffViewer
    {
        private readonly TextBoxDiffRenderer _diffRenderer;
        private readonly ScrollViewerSynchronizer _scrollSynchronizer;
        private readonly IKeyboardCommandHandler<KeyRoutedEventArgs> _keyboardCommandHandler;

        public event EventHandler OnCloseEvent;

        public SideBySideDiffViewer()
        {
            InitializeComponent();
            _scrollSynchronizer = new ScrollViewerSynchronizer(new List<ScrollViewer> { LeftScroller, RightScroller });
            _diffRenderer = new TextBoxDiffRenderer();
            _keyboardCommandHandler = GetKeyboardCommandHandler();

            LeftBox.SelectionHighlightColor = Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush;
            RightBox.SelectionHighlightColor = Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush;

            LayoutRoot.KeyDown += OnKeyDown;
            KeyDown += OnKeyDown;
            LeftBox.KeyDown += OnKeyDown;
            RightBox.KeyDown += OnKeyDown;
        }

        private KeyboardCommandHandler GetKeyboardCommandHandler()
        {
            return new KeyboardCommandHandler(new List<IKeyboardCommand<KeyRoutedEventArgs>>
            {
                new KeyboardShortcut<KeyRoutedEventArgs>(false, false, false, VirtualKey.Escape, (args) =>
                {
                    OnCloseEvent?.Invoke(this, EventArgs.Empty);
                }),
            });
        }

        private void OnKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs args)
        {
            _keyboardCommandHandler.Handle(args);
        }

        public void Focus()
        {
            RightBox.Focus(FocusState.Programmatic);
        }

        public void RenderDiff(string left, string right)
        {
            var diffData = _diffRenderer.GenerateDiffViewData(left, right);
            var leftData = diffData.Item1;
            var rightData = diffData.Item2;
            LeftBox.Blocks.Clear();
            RightBox.Blocks.Clear();

            foreach (var block in leftData.Blocks)
            {
                LeftBox.Blocks.Add(block);
            }

            foreach (var textHighlighter in leftData.TextHighlighters)
            {
                LeftBox.TextHighlighters.Add(textHighlighter);
            }

            foreach (var block in rightData.Blocks)
            {
                RightBox.Blocks.Add(block);
            }

            foreach (var textHighlighter in rightData.TextHighlighters)
            {
                RightBox.TextHighlighters.Add(textHighlighter);
            }
        }
    }
}
