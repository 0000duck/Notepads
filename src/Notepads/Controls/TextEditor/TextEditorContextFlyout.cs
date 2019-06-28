﻿
namespace Notepads.Controls.TextEditor
{
    using System;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;

    public class TextEditorContextFlyout : MenuFlyout
    {
        private MenuFlyoutItem _cut;
        private MenuFlyoutItem _copy;
        private MenuFlyoutItem _paste;
        private MenuFlyoutItem _undo;
        private MenuFlyoutItem _redo;
        private MenuFlyoutItem _selectAll;
        private MenuFlyoutItem _wordWrap;
        private MenuFlyoutItem _share;

        private readonly TextEditor _textEditor;

        public TextEditorContextFlyout(TextEditor editor)
        {
            _textEditor = editor;
            Items.Add(Cut);
            Items.Add(Copy);
            Items.Add(Paste);
            Items.Add(Undo);
            Items.Add(Redo);
            Items.Add(SelectAll);
            Items.Add(new MenuFlyoutSeparator());
            Items.Add(WordWrap);
            Items.Add(Share);

            Opening += TextEditorContextFlyout_Opening;

        }

        private void TextEditorContextFlyout_Opening(object sender, object e)
        {
            if (_textEditor.Document.Selection.StartPosition == _textEditor.Document.Selection.EndPosition)
            {
                PrepareForInsertionMode();
            }
            else
            {
                PrepareForSelectionMode();
            }

            Undo.IsEnabled = _textEditor.Document.CanUndo();
            Redo.IsEnabled = _textEditor.Document.CanRedo();

            WordWrap.Icon.Visibility = (_textEditor.TextWrapping == TextWrapping.Wrap) ? Visibility.Visible : Visibility.Collapsed;
        }

        public void PrepareForInsertionMode()
        {
            Cut.Visibility = Visibility.Collapsed;
            Copy.Visibility = Visibility.Collapsed;
            Share.Text = "Share";
        }

        public void PrepareForSelectionMode()
        {
            Cut.Visibility = Visibility.Visible;
            Copy.Visibility = Visibility.Visible;
            Share.Text = "Share Selected";
        }

        public MenuFlyoutItem Cut
        {
            get
            {
                if (_cut == null)
                {
                    _cut = new MenuFlyoutItem { Icon = new SymbolIcon(Symbol.Cut), Text = "Cut" };
                    _cut.KeyboardAccelerators.Add(new KeyboardAccelerator()
                    {
                        Modifiers = VirtualKeyModifiers.Control,
                        Key = VirtualKey.X,
                        IsEnabled = false,
                    });
                    _cut.Click += (sender, args) => { _textEditor.Document.Selection.Cut(); };
                }
                return _cut;
            }
        }

        public MenuFlyoutItem Copy
        {
            get
            {
                if (_copy == null)
                {
                    _copy = new MenuFlyoutItem { Icon = new SymbolIcon(Symbol.Copy), Text = "Copy" };
                    _copy.KeyboardAccelerators.Add(new KeyboardAccelerator()
                    {
                        Modifiers = VirtualKeyModifiers.Control,
                        Key = VirtualKey.C,
                        IsEnabled = false,
                    });
                    _copy.Click += (sender, args) =>
                    {
                        DataPackage dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
                        dataPackage.SetText(_textEditor.Document.Selection.Text);
                        Clipboard.SetContent(dataPackage);
                    };
                }
                return _copy;
            }
        }

        public MenuFlyoutItem Paste
        {
            get
            {
                if (_paste == null)
                {
                    _paste = new MenuFlyoutItem { Icon = new SymbolIcon(Symbol.Paste), Text = "Paste" };
                    _paste.KeyboardAccelerators.Add(new KeyboardAccelerator()
                    {
                        Modifiers = VirtualKeyModifiers.Control,
                        Key = VirtualKey.V,
                        IsEnabled = false,
                    });
                    _paste.Click += async (sender, args) => { await _textEditor.PastePlainTextFromWindowsClipboard(null); };
                }
                return _paste;
            }
        }

        public MenuFlyoutItem Undo
        {
            get
            {
                if (_undo == null)
                {
                    _undo = new MenuFlyoutItem { Icon = new SymbolIcon(Symbol.Undo), Text = "Undo" };
                    _undo.KeyboardAccelerators.Add(new KeyboardAccelerator()
                    {
                        Modifiers = VirtualKeyModifiers.Control,
                        Key = VirtualKey.Z,
                        IsEnabled = false,
                    });
                    _undo.Click += (sender, args) => { _textEditor.Document.Undo(); };
                }
                return _undo;
            }
        }

        public MenuFlyoutItem Redo
        {
            get
            {
                if (_redo == null)
                {
                    _redo = new MenuFlyoutItem { Icon = new SymbolIcon(Symbol.Redo), Text = "Redo" };
                    _redo.KeyboardAccelerators.Add(new KeyboardAccelerator()
                    {
                        Modifiers = (VirtualKeyModifiers.Control & VirtualKeyModifiers.Shift),
                        Key = VirtualKey.Z,
                        IsEnabled = false,

                    });
                    _redo.KeyboardAcceleratorTextOverride = "Ctrl+Shift+Z";
                    _redo.Click += (sender, args) => { _textEditor.Document.Redo(); };
                }
                return _redo;
            }
        }

        public MenuFlyoutItem SelectAll
        {
            get
            {
                if (_selectAll == null)
                {
                    _selectAll = new MenuFlyoutItem { Icon = new SymbolIcon(Symbol.SelectAll), Text = "Select All" };
                    _selectAll.KeyboardAccelerators.Add(new KeyboardAccelerator()
                    {
                        Modifiers = VirtualKeyModifiers.Control,
                        Key = VirtualKey.A,
                        IsEnabled = false,
                    });
                    _selectAll.Click += (sender, args) =>
                    {
                        _textEditor.Document.Selection.SetRange(0, Int32.MaxValue);
                    };
                }
                return _selectAll;
            }
        }

        public MenuFlyoutItem Share
        {
            get
            {
                if (_share == null)
                {
                    _share = new MenuFlyoutItem { Icon = new SymbolIcon(Symbol.Share), Text = "Share" };
                    _share.Click += (sender, args) =>
                    {
                        Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
                    };
                }
                return _share;
            }
        }

        public MenuFlyoutItem WordWrap
        {
            get
            {
                if (_wordWrap != null) return _wordWrap;

                _wordWrap = new MenuFlyoutItem
                {
                    Text = "Word Wrap",
                    Icon = new FontIcon()
                    {
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        Glyph = "\uE73E"
                    }
                };

                _wordWrap.Icon.Visibility = _textEditor.TextWrapping == TextWrapping.Wrap ? Visibility.Visible : Visibility.Collapsed;
                _wordWrap.Click += _wordWrap_Click;
                return _wordWrap;
            }
        }

        private void _wordWrap_Click(object sender, RoutedEventArgs e)
        {
            _wordWrap.Icon.Visibility = _textEditor.TextWrapping == TextWrapping.Wrap ? Visibility.Visible : Visibility.Collapsed;
            _textEditor.TextWrapping = _textEditor.TextWrapping == TextWrapping.Wrap ? TextWrapping.NoWrap : TextWrapping.Wrap;
        }
    }
}
