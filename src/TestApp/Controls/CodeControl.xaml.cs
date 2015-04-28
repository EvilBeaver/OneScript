/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Search;

using System.Windows.Threading;

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

            editor.TextArea.DefaultInputHandler.NestedInputHandlers.Add(new SearchInputHandler(editor.TextArea));
            editor.ShowLineNumbers = true;

            foldingManager = FoldingManager.Install(editor.TextArea);

            editor.TextChanged += editor_TextChanged;
            editor.TextArea.Options.EnableHyperlinks = false;
            editor.TextArea.Options.EnableVirtualSpace = true;
            editor.TextArea.Options.EnableRectangularSelection = true;
            editor.TextArea.SelectionCornerRadius = 0;
            editor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            
            foldingUpdateTimer = new DispatcherTimer(DispatcherPriority.ContextIdle);
            foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            foldingUpdateTimer.Tick += foldingUpdateTimer_Tick;
            foldingUpdateTimer.Start();

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
            if (foldingUpdateTimer!= null && !foldingUpdateTimer.IsEnabled)
            {
                foldingUpdateTimer.Start();
            }
        }

        void foldingUpdateTimer_Tick(object sender, EventArgs e)
        {
            ((DispatcherTimer)sender).Stop();
            foldingStrategy.UpdateFoldings(foldingManager, editor.Document);
            _procList = ((V8ModuleFoldingStrategy)foldingStrategy).ProcedureList;

            if (_procList != null)
            {
                cbProcList.Items.Clear();

                var Names = from lst in _procList select lst.Name;
                foreach (var item in Names)
                {
                    cbProcList.Items.Add(item);
                }
                
                UpdateCurrentProc();
            }

            //m_ModifyFlag = false;
        }

        public String Text 
        { 
            get 
            {
                return editor.Text; 
            }
            set
            {
                editor.Text = value;
            }
        }

        FoldingManager foldingManager;
        AbstractFoldingStrategy foldingStrategy = new V8ModuleFoldingStrategy();
        //bool m_ModifyFlag;
        DispatcherTimer foldingUpdateTimer;
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
            if (foldingManager != null)
            {
                foreach (var folding in foldingManager.AllFoldings)
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

    class V8ModuleFoldingStrategy : AbstractFoldingStrategy
	{
		
        public V8ModuleFoldingStrategy()
		{
			
		}
		
		/// <summary>
		/// Create <see cref="NewFolding"/>s for the specified document.
		/// </summary>
		public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
		{
			firstErrorOffset = -1;
			return CreateNewFoldings(document);
		}
		
		/// <summary>
		/// Create <see cref="NewFolding"/>s for the specified document.
		/// </summary>

        private struct TextFragment
        {
            public int offset;
            public int len;
        }

        private List<ProcListItem> _procList;

        public List<ProcListItem> ProcedureList
        {
            get
            {
                return _procList;
            }
        }

		public IEnumerable<NewFolding> CreateNewFoldings(ITextSource document)
		{
			List<NewFolding> newFoldings = new List<NewFolding>();
            _procList = new List<ProcListItem>();

            int startPos = 0;
            int len = document.TextLength;

            int currentStart = 0;

            bool MethodIsOpen = false;
            string MethodName ="";
            string EndToken = null;

            int PreCommentStart = -1;

            //var Reader = document.CreateReader();

            string FullText = document.Text;
            int DocLine = 0;
            int MethodStart = 0;

            const string kProcStart = "ПРОЦЕДУРА";
            const string kProcEnd = "КОНЕЦПРОЦЕДУРЫ";
            const string kFuncStart = "ФУНКЦИЯ";
            const string kFuncEnd = "КОНЕЦФУНКЦИИ";

            char[] trimArr = new char[]{' ','\t'};

            do
            {
                int prev_start = startPos;
                string lineText = ReadLine(FullText, ref startPos);
                
                DocLine++;

                if (lineText == null)
                {
                    break;
                }
                
                TextFragment tf = new TextFragment();
                tf.offset = prev_start;
                tf.len = lineText.Length;

                //startPos += lineText.Length + 2;

                if (!MethodIsOpen)
                {
                    bool CommentBreak = false;
                    
                    if (lineText.StartsWith("//"))
                    {
                        if (PreCommentStart < 0)
                        {
                            PreCommentStart = tf.offset + tf.len;
                        }
                    }
                    else
                    {
                        CommentBreak = true;
                    }
                    
                    if (LineIsKeyword(lineText.TrimStart(trimArr), kProcStart))
                    {
                        MethodIsOpen = true;
                        MethodName = ScanForParamList(FullText, prev_start+kProcStart.Length);
                        EndToken = kProcEnd;
                        MethodStart = DocLine;
                    }
                    else if(LineIsKeyword(lineText.TrimStart(trimArr), kFuncStart))
                    {
                        MethodIsOpen = true;
                        MethodName = ScanForParamList(FullText, prev_start + kFuncStart.Length);
                        EndToken = kFuncEnd;
                        MethodStart = DocLine;
                    }

                    if (MethodIsOpen)
                    {
                        currentStart = tf.offset + tf.len;

                        if (PreCommentStart >= 0)
                        {
                            var Folding = new NewFolding(PreCommentStart, tf.offset - 2);
                            newFoldings.Add(Folding);
                            PreCommentStart = -1;
                        }
                    }
                    else if(CommentBreak)
                    {
                        PreCommentStart = -1;
                    }
                    
                }
                else if (LineIsKeyword(lineText.TrimStart(trimArr), EndToken))
                {
                    var Folding = new NewFolding(currentStart, tf.offset + tf.len);
                    newFoldings.Add(Folding);

                    if (MethodName != "")
                    {
                        ProcListItem pli = new ProcListItem();
                        pli.Name = MethodName;
                        pli.StartLine = MethodStart;
                        pli.EndLine = DocLine;
                        pli.ListIndex = _procList.Count;

                        _procList.Add(pli);
                        
                        MethodName = "";
                    }

                    MethodIsOpen = false;
                }
                
            }
            while (true);

			return newFoldings;
		}

        private string ScanForParamList(string FullText, int Start)
        {
            int nameLen = 0;
            int i = Start;
            bool found = false;
            bool ltrFound = false;

            while (i < FullText.Length)
            {
                nameLen++;
                char currentLtr = FullText[i++];

                if (!Char.IsLetterOrDigit(currentLtr))
                {
                    if (ltrFound && !Char.IsWhiteSpace(currentLtr) && currentLtr != '(')
                    {
                        break;
                    }
                }
                else
                {
                    ltrFound = true;
                }

                if (currentLtr == '(')
                {
                    found = true;
                    nameLen--;
                    break;
                }

            }

            if (found)
            {
                return FullText.Substring(Start, nameLen).Trim();
            }
            else
            {
                return "";
            }
        }

        private bool LineIsKeyword(string Line, string Keyword)
        {
            if (Line.StartsWith(Keyword, StringComparison.OrdinalIgnoreCase))
            {
                if (Line.Length > Keyword.Length)
                {
                    return (Char.IsWhiteSpace(Line[Keyword.Length]));
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        private string ReadLine(string Content, ref int Position)
        {
            if (Position >= Content.Length)
            {
                Position = -1;
                return null;
            }
            
            int StartPoint = Position;
            int EndPoint = Position;

            while (Position < Content.Length)
            {
                if (Content[Position] == '\n')
                {
                    Position++;
                    break;
                }
                
                if (Content[Position] != '\r')
                {
                    EndPoint = Position;
                }

                Position++;

            }

            int len = EndPoint == StartPoint ? 0 : EndPoint - StartPoint + 1;
            
            string result = Content.Substring(StartPoint, len);
            return result;

        }

        



	}

}
