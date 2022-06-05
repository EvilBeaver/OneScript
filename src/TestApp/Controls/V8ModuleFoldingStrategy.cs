/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace V8Reader.Controls
{
    class V8ModuleFoldingStrategy
    {
		
        public V8ModuleFoldingStrategy()
        {
			
        }
		
        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            int firstErrorOffset;
            var foldings = CreateNewFoldings(document, out firstErrorOffset);
            manager.UpdateFoldings(foldings, firstErrorOffset);
        }
        
        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
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
                            var Folding = new NewFolding(PreCommentStart, tf.offset-1);
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