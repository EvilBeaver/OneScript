/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace V8Reader.Controls
{
    /// <summary>
    /// Логика взаимодействия для CodeControl.xaml
    /// </summary>
    public partial class CodeControl : UserControl
    {
        public CodeControl()
        {
            var Res = Application.GetResourceStream(new Uri("pack://application:,,,/TestApp;component/controls/1CV8Syntax.xshd", UriKind.Absolute));

            IHighlightingDefinition v8Highlighting;

            using (var s = Res.Stream)
            {
                using (XmlReader reader = new XmlTextReader(s))
                {
                    v8Highlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }

            HighlightingManager.Instance.RegisterHighlighting("1CV8", new string[] { ".v8m" }, v8Highlighting);

            InitializeComponent();

            SearchPanel.Install(editor.TextArea);
            editor.ShowLineNumbers = true;

            _foldingManager = FoldingManager.Install(editor.TextArea);

            editor.TextChanged += editor_TextChanged;
            editor.TextArea.Options.EnableHyperlinks = false;
            editor.TextArea.Options.EnableVirtualSpace = true;
            editor.TextArea.Options.EnableRectangularSelection = true;
            editor.TextArea.SelectionCornerRadius = 0;
            editor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            
            _foldingUpdateTimer = new DispatcherTimer(DispatcherPriority.ContextIdle);
            _foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            _foldingUpdateTimer.Tick += foldingUpdateTimer_Tick;
            _foldingUpdateTimer.Start();

        }

        void Caret_PositionChanged(object sender, EventArgs e)
        {
            UpdateCurrentProc();
        }

        private void UpdateCurrentProc()
        {
            if (_procList != null && _procList.Count > 0 && editor.SelectionLength == 0)
            {
                int curLine = editor.TextArea.Caret.Line;
                int selectedIndex = GetCurrentProcIndex(curLine);

                //cbProcList.SelectionChanged -= cbProcList_SelectionChanged;
                cbProcList.SelectedIndex = selectedIndex;
                //cbProcList.SelectionChanged += cbProcList_SelectionChanged;
            }
        }

        private int GetCurrentProcIndex(int curLine)
        {

            int chunkStart = 0;
            int chunkEnd = _procList.Count-1;
            
            return FindInChunk(curLine, chunkStart, chunkEnd);

        }

        private int FindInChunk(int curLine, int chunkStart, int chunkEnd)
        {
            int chunkLen = chunkEnd - chunkStart;

            if (chunkLen < 0)
            {
                return -1;
            }
            else if (chunkLen == 1 || chunkLen == 0)
            {
                for (int i = chunkStart; i <= chunkEnd; i++)
                {
                    var li = _procList[i];
                    if (curLine >= li.StartLine && curLine <= li.EndLine)
                    {
                        return li.ListIndex;
                    }
                }

                return -1;
            }

            int middle = chunkLen / 2;
            if (middle == 0)
            {
                return -1;
            }

            int middleIdx = chunkStart + middle;

            if (middleIdx < 0 || middleIdx >= _procList.Count)
            {
                return -1;
            }

            var item = _procList[middleIdx];
            if (curLine < item.StartLine)
            {
                return FindInChunk(curLine, chunkStart, middleIdx);
            }
            else if (curLine > item.EndLine)
            {
                return FindInChunk(curLine, middleIdx, chunkEnd);
            }
            else if (curLine >= item.StartLine && curLine <= item.EndLine)
            {
                return item.ListIndex;
            }
            else
            {
                return -1;
            }

        }

        void editor_TextChanged(object sender, EventArgs e)
        {
           // m_ModifyFlag = true;
            if (_foldingUpdateTimer!= null && !_foldingUpdateTimer.IsEnabled)
            {
                _foldingUpdateTimer.Start();
            }
        }

        void foldingUpdateTimer_Tick(object sender, EventArgs e)
        {
            ((DispatcherTimer)sender).Stop();
            _foldingStrategy.UpdateFoldings(_foldingManager, editor.Document);
            _procList = _foldingStrategy.ProcedureList;

            if (_procList == null) return;
            cbProcList.Items.Clear();

            var names = from lst in _procList select lst.Name;
            foreach (var item in names)
            {
                cbProcList.Items.Add(item);
            }
                
            UpdateCurrentProc();

            //m_ModifyFlag = false;
        }

        public string Text 
        { 
            get => editor.Text;
            set => editor.Text = value;
        }

        FoldingManager _foldingManager;
        V8ModuleFoldingStrategy _foldingStrategy = new V8ModuleFoldingStrategy();
        //bool m_ModifyFlag;
        DispatcherTimer _foldingUpdateTimer;
        List<ProcListItem> _procList;

        bool _userMethodSelect = false;

        void OnMethodSelected()
        {
            int si = cbProcList.SelectedIndex;
            if (si >= 0 && si < _procList.Count)
            {
                var item = _procList[si];
                if (item.StartLine <= editor.LineCount)
                {
                    SetCurrentMethod(item);
                }
            }

        }

        private void Method_MouseClick(object sender, MouseButtonEventArgs e)
        {
            _userMethodSelect = true;
        }

        private void btnProcList_Click(object sender, RoutedEventArgs e)
        {
            if (_procList != null && _procList.Count > 0)
            {
                var wnd = new ProcedureListWnd(_procList);
                wnd.Owner = Window.GetWindow(this);
                var answer = wnd.ShowDialog();
                if (answer == true)
                {
                    var item = wnd.SelectedItem;
                    SetCurrentMethod(item);
                }

            }
        }

        private void SetCurrentMethod(ProcListItem item)
        {
            editor.Focus();
            editor.ScrollToLine(item.StartLine);
            editor.TextArea.Caret.Line = item.StartLine;
            editor.TextArea.Caret.Column = 0;
        }

        private void btnCollapseNodes_Click(object sender, RoutedEventArgs e)
        {
            PerformNodeFolding(true);
        }

        private void btnExpandNodes_Click(object sender, RoutedEventArgs e)
        {
            PerformNodeFolding(false);
        }

        private void PerformNodeFolding(bool SetCollapsed)
        {
            if (_foldingManager != null)
            {
                foreach (var folding in _foldingManager.AllFoldings)
                {
                    folding.IsFolded = SetCollapsed;
                }
            }
        }

        private void cbProcList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_userMethodSelect)
            {
                _userMethodSelect = false;
                OnMethodSelected();
            }
        }

        private void cbProcList_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void cbProcList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        public static readonly RoutedEvent TextChangedEvent =
        EventManager.RegisterRoutedEvent(
            "TextChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(CodeControl));

        public event RoutedEventHandler TextChanged
        {
            add { AddHandler(TextChangedEvent, value); }
            remove { RemoveHandler(TextChangedEvent, value); }
        }

        private void editor_TextChanged_1(object sender, EventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(TextChangedEvent, sender));
        }

    }

    class ProcListItem
    {
        public int StartLine;
        public int EndLine;
        public int ListIndex;
        public string Name;

        public override string ToString()
        {
            return Name;
        }
    }
}
