﻿
namespace Notepads.Controls.TextEditor
{
    using Notepads.Commands;
    using Notepads.Extensions;
    using Notepads.Services;
    using Notepads.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Storage;
    using Windows.System;
    using Windows.UI.Core;
    using Windows.UI.Text;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public enum TextEditorMode
    {
        Editing,
        DiffPreview
    }

    public sealed partial class TextEditor : UserControl
    {
        public FileType FileType { get; private set; }

        private StorageFile _editingFile;

        public StorageFile EditingFile
        {
            get => _editingFile;
            private set
            {
                FileType = value == null ? FileType.TextFile : FileTypeUtility.GetFileTypeByFileName(value.Name);
                _editingFile = value;
            }
        }

        public INotepadsExtensionProvider ExtensionProvider;

        private readonly IKeyboardCommandHandler<KeyRoutedEventArgs> _keyboardCommandHandler;

        private IContentPreviewExtension _contentPreviewExtension;

        public event EventHandler ModeChanged;

        public event EventHandler ModifyStateChanged;

        public event RoutedEventHandler SelectionChanged;

        public TextFile OriginalSnapshot { get; private set; }

        public LineEnding? TargetLineEnding { get; private set; }

        public Encoding TargetEncoding { get; private set; }

        private bool _isModified;

        public bool IsModified
        {
            get => _isModified;
            private set
            {
                if (_isModified != value)
                {
                    _isModified = value;
                    ModifyStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private TextEditorMode _textEditorMode = TextEditorMode.Editing;

        public TextEditorMode TextEditorMode
        {
            get => _textEditorMode;
            private set
            {
                if (_textEditorMode != value)
                {
                    _textEditorMode = value;
                    ModeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public TextEditor()
        {
            InitializeComponent();

            TextEditorCore.TextChanging += TextEditorCore_OnTextChanging;
            TextEditorCore.SelectionChanged += (sender, args) => { SelectionChanged?.Invoke(this, args); };
            TextEditorCore.KeyDown += TextEditorCore_OnKeyDown;
            TextEditorCore.ContextFlyout = new TextEditorContextFlyout(this);

            // Init shortcuts
            _keyboardCommandHandler = GetKeyboardCommandHandler();

            ThemeSettingsService.OnThemeChanged += (sender, theme) =>
            {
                if (SideBySideDiffViewer != null && SideBySideDiffViewer.Visibility == Visibility.Visible)
                {
                    SideBySideDiffViewer.RenderDiff(OriginalSnapshot.Content, TextEditorCore.GetText());
                    Task.Factory.StartNew(
                        () => Dispatcher.RunAsync(CoreDispatcherPriority.Low,
                            () => SideBySideDiffViewer.Focus()));
                }
            };
        }

        private KeyboardCommandHandler GetKeyboardCommandHandler()
        {
            return new KeyboardCommandHandler(new List<IKeyboardCommand<KeyRoutedEventArgs>>
            {
                new KeyboardShortcut<KeyRoutedEventArgs>(true, false, false, VirtualKey.P, (args) => ShowHideContentPreview()),
                new KeyboardShortcut<KeyRoutedEventArgs>(false, true, false, VirtualKey.D, (args) => ShowHideSideBySideDiffViewer()),
                new KeyboardShortcut<KeyRoutedEventArgs>(false, false, false, VirtualKey.Escape, (args) =>
                {
                    if (SplitPanel != null && SplitPanel.Visibility == Visibility.Visible)
                    {
                        _contentPreviewExtension.IsExtensionEnabled = false;
                        CloseSplitView();
                    }
                }),
            });
        }

        public void Init(TextFile textFile, StorageFile file)
        {
            EditingFile = file;
            TargetEncoding = null;
            TargetLineEnding = null;
            TextEditorCore.SetText(textFile.Content);
            OriginalSnapshot = new TextFile(TextEditorCore.GetText(), textFile.Encoding, textFile.LineEnding);
            TextEditorCore.ClearUndoQueue();
            IsModified = false;
        }

        public bool TryChangeEncoding(Encoding encoding)
        {
            if (encoding == null) return false;

            if (!EncodingUtility.Equals(OriginalSnapshot.Encoding, encoding))
            {
                TargetEncoding = encoding;
                IsModified = true;
                return true;
            }

            if (TargetEncoding != null && EncodingUtility.Equals(OriginalSnapshot.Encoding, encoding))
            {
                TargetEncoding = null;
                IsModified = !IsInOriginalState();
                return true;
            }
            return false;
        }

        public bool TryChangeLineEnding(LineEnding lineEnding)
        {
            if (OriginalSnapshot.LineEnding != lineEnding)
            {
                TargetLineEnding = lineEnding;
                IsModified = true;
                return true;
            }

            if (TargetLineEnding != null && OriginalSnapshot.LineEnding == lineEnding)
            {
                TargetLineEnding = null;
                IsModified = !IsInOriginalState();
                return true;
            }
            return false;
        }

        public LineEnding GetLineEnding()
        {
            return TargetLineEnding ?? OriginalSnapshot.LineEnding;
        }

        public Encoding GetEncoding()
        {
            return TargetEncoding ?? OriginalSnapshot.Encoding;
        }

        public void OpenSplitView(IContentPreviewExtension extension)
        {
            SplitPanel.Content = extension;
            SplitPanelColumnDefinition.Width = new GridLength(ActualWidth / 2.0f);
            SplitPanelColumnDefinition.MinWidth = 100.0f;
            SplitPanel.Visibility = Visibility.Visible;
            GridSplitter.Visibility = Visibility.Visible;
        }

        public void CloseSplitView()
        {
            SplitPanelColumnDefinition.Width = new GridLength(0);
            SplitPanelColumnDefinition.MinWidth = 0.0f;
            SplitPanel.Visibility = Visibility.Collapsed;
            GridSplitter.Visibility = Visibility.Collapsed;
            TextEditorCore.Focus(FocusState.Programmatic);
        }

        public void ShowHideContentPreview()
        {
            if (_contentPreviewExtension == null)
            {
                _contentPreviewExtension = ExtensionProvider?.GetContentPreviewExtension(FileType);
                if (_contentPreviewExtension == null) return;
                _contentPreviewExtension.Bind(this);
            }

            if (SplitPanel == null) LoadSplitView();

            if (SplitPanel.Visibility == Visibility.Collapsed)
            {
                _contentPreviewExtension.IsExtensionEnabled = true;
                OpenSplitView(_contentPreviewExtension);
            }
            else
            {
                _contentPreviewExtension.IsExtensionEnabled = false;
                CloseSplitView();
            }
        }

        public void OpenSideBySideDiffViewer()
        {
            TextEditorMode = TextEditorMode.DiffPreview;
            TextEditorCore.IsEnabled = false;
            EditorRowDefinition.Height = new GridLength(0);
            SideBySideDiffViewRowDefinition.Height = new GridLength(1, GridUnitType.Star);
            SideBySideDiffViewer.Visibility = Visibility.Visible;
            SideBySideDiffViewer.RenderDiff(OriginalSnapshot.Content, TextEditorCore.GetText());
            Task.Factory.StartNew(
                () => Dispatcher.RunAsync(CoreDispatcherPriority.Low,
                    () => SideBySideDiffViewer.Focus()));
        }

        public void CloseSideBySideDiffViewer()
        {
            TextEditorMode = TextEditorMode.Editing;
            TextEditorCore.IsEnabled = true;
            EditorRowDefinition.Height = new GridLength(1, GridUnitType.Star);
            SideBySideDiffViewRowDefinition.Height = new GridLength(0);
            SideBySideDiffViewer.Visibility = Visibility.Collapsed;
            SideBySideDiffViewer.StopRenderingAndClearCache();
            TextEditorCore.Focus(FocusState.Programmatic);
        }

        public void ShowHideSideBySideDiffViewer()
        {
            if (SideBySideDiffViewer == null) LoadSideBySideDiffViewer();

            if (SideBySideDiffViewer.Visibility == Visibility.Collapsed)
            {
                if (!string.Equals(OriginalSnapshot.Content, TextEditorCore.GetText()))
                {
                    OpenSideBySideDiffViewer();
                }
            }
            else
            {
                CloseSideBySideDiffViewer();
            }
        }

        public void GetCurrentLineColumn(out int lineIndex, out int columnIndex, out int selectedCount)
        {
            TextEditorCore.GetCurrentLineColumn(out int line, out int column, out int selected);
            lineIndex = line;
            columnIndex = column;
            selectedCount = selected;
        }

        public bool IsEditorEnabled()
        {
            return TextEditorCore.IsEnabled;
        }

        public async Task SaveToFile(StorageFile file)
        {
            if (SideBySideDiffViewer != null && SideBySideDiffViewer.Visibility == Visibility.Visible)
            {
                CloseSideBySideDiffViewer();
            }
            var text = TextEditorCore.GetText();
            var encoding = TargetEncoding ?? OriginalSnapshot.Encoding;
            var lineEnding = TargetLineEnding ?? OriginalSnapshot.LineEnding;
            await FileSystemUtility.WriteToFile(LineEndingUtility.ApplyLineEnding(text, lineEnding), encoding, file);
            Init(new TextFile(text, encoding, lineEnding), file);
        }

        public string GetContentForSharing()
        {
            string content;

            if (TextEditorCore.Document.Selection.StartPosition == TextEditorCore.Document.Selection.EndPosition)
            {
                content = TextEditorCore.GetText();
            }
            else
            {
                content = TextEditorCore.Document.Selection.Text;
            }

            return content;
        }

        public void TypeTab()
        {
            if (TextEditorCore.IsEnabled)
            {
                var tabStr = EditorSettingsService.EditorDefaultTabIndents == -1 ? "\t" : new string(' ', EditorSettingsService.EditorDefaultTabIndents);
                TextEditorCore.Document.Selection.TypeText(tabStr);
            }
        }

        public bool FindNextAndReplace(string searchText, string replaceText, bool matchCase, bool matchWholeWord)
        {
            if (FindNextAndSelect(searchText, matchCase, matchWholeWord))
            {
                TextEditorCore.Document.Selection.SetText(TextSetOptions.None, replaceText);
                return true;
            }

            return false;
        }

        public bool FindAndReplaceAll(string searchText, string replaceText, bool matchCase, bool matchWholeWord)
        {
            var found = false;

            var pos = 0;
            var searchTextLength = searchText.Length;
            var replaceTextLength = replaceText.Length;

            var text = TextEditorCore.GetText();

            StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            pos = matchWholeWord ? IndexOfWholeWord(text, pos, searchText, comparison) : text.IndexOf(searchText, pos, comparison);

            while (pos != -1)
            {
                found = true;
                text = text.Remove(pos, searchTextLength).Insert(pos, replaceText);
                pos += replaceTextLength;
                pos = matchWholeWord ? IndexOfWholeWord(text, pos, searchText, comparison) : text.IndexOf(searchText, pos, comparison);
            }

            if (found)
            {
                TextEditorCore.SetText(text);
                TextEditorCore.Document.Selection.StartPosition = Int32.MaxValue;
                TextEditorCore.Document.Selection.EndPosition = TextEditorCore.Document.Selection.StartPosition;
            }

            return found;
        }

        public bool FindNextAndSelect(string searchText, bool matchCase, bool matchWholeWord, bool stopAtEof = true)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return false;
            }

            var text = TextEditorCore.GetText();

            StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            var index = matchWholeWord ? IndexOfWholeWord(text, TextEditorCore.Document.Selection.EndPosition, searchText, comparison) : text.IndexOf(searchText, TextEditorCore.Document.Selection.EndPosition, comparison);

            if (index != -1)
            {
                TextEditorCore.Document.Selection.StartPosition = index;
                TextEditorCore.Document.Selection.EndPosition = index + searchText.Length;
            }
            else
            {
                if (!stopAtEof)
                {
                    index = matchWholeWord ? IndexOfWholeWord(text, 0, searchText, comparison) : text.IndexOf(searchText, 0, comparison);

                    if (index != -1)
                    {
                        TextEditorCore.Document.Selection.StartPosition = index;
                        TextEditorCore.Document.Selection.EndPosition = index + searchText.Length;
                    }
                }
            }

            if (index == -1)
            {
                TextEditorCore.Document.Selection.StartPosition = TextEditorCore.Document.Selection.EndPosition;
                return false;
            }

            return true;
        }

        public void Focus()
        {
            if (SideBySideDiffViewer != null && SideBySideDiffViewer.Visibility == Visibility.Visible)
            {
                SideBySideDiffViewer.Focus();
            }
            else
            {
                TextEditorCore.Focus(FocusState.Programmatic);
            }
        }

        private bool IsInOriginalState(bool compareTextOnly = false)
        {
            if (OriginalSnapshot == null) return true;

            if (!compareTextOnly)
            {
                if (TargetLineEnding != null)
                {
                    return false;
                }

                if (TargetEncoding != null)
                {
                    return false;
                }
            }
            if (!string.Equals(OriginalSnapshot.Content, TextEditorCore.GetText()))
            {
                return false;
            }
            return true;
        }

        private void LoadSplitView()
        {
            FindName("SplitPanel");
            FindName("GridSplitter");
            SplitPanel.Visibility = Visibility.Collapsed;
            GridSplitter.Visibility = Visibility.Collapsed;
            SplitPanel.KeyDown += SplitPanel_OnKeyDown;
        }

        private void LoadSideBySideDiffViewer()
        {
            FindName("SideBySideDiffViewer");
            SideBySideDiffViewer.Visibility = Visibility.Collapsed;
            SideBySideDiffViewer.OnCloseEvent += (sender, args) => CloseSideBySideDiffViewer();
        }

        private void TextEditorCore_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            _keyboardCommandHandler.Handle(e);
        }

        private void SplitPanel_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            _keyboardCommandHandler.Handle(e);
        }

        private void TextEditorCore_OnTextChanging(RichEditBox textEditor, RichEditBoxTextChangingEventArgs args)
        {
            if (!args.IsContentChanging) return;
            if (IsModified)
            {
                IsModified = !IsInOriginalState();
            }
            else
            {
                IsModified = !IsInOriginalState(compareTextOnly: true);
            }
        }

        private static int IndexOfWholeWord(string target, int startIndex, string value, StringComparison comparison)
        {
            int pos = startIndex;
            while (pos < target.Length && (pos = target.IndexOf(value, pos, comparison)) != -1)
            {
                bool startBoundary = true;
                if (pos > 0)
                    startBoundary = !Char.IsLetterOrDigit(target[pos - 1]);

                bool endBoundary = true;
                if (pos + value.Length < target.Length)
                    endBoundary = !Char.IsLetterOrDigit(target[pos + value.Length]);

                if (startBoundary && endBoundary)
                    return pos;

                pos++;
            }
            return -1;
        }
    }
}