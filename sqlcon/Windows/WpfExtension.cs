using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace sqlcon.Windows
{
    public static class WpfExtension
    {

        public static int LineNumber(this RichTextBox textBox)
        {
            TextPointer caretLineStart = textBox.CaretPosition.GetLineStartPosition(0);
            TextPointer p = textBox.Document.ContentStart.GetLineStartPosition(0);
            int currentLineNumber = 1;

            while (true)
            {
                if (caretLineStart.CompareTo(p) < 0)
                {
                    break;
                }
                int result;
                p = p.GetLineStartPosition(1, out result);
                if (result == 0)
                {
                    break;
                }
                currentLineNumber++;
            }
            return currentLineNumber;
        }

        public static int ColumnNumber(this RichTextBox textBox)
        {
            TextPointer caretPos = textBox.CaretPosition;
            TextPointer p = textBox.CaretPosition.GetLineStartPosition(0);
            int currentColumnNumber = Math.Max(p.GetOffsetToPosition(caretPos) - 1, 0);

            return currentColumnNumber;
        }

        public static string GetAllText(this RichTextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Selection.Text))
            {
                TextRange textRange = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd);
                return textRange.Text;
            }
            else
                return textBox.Selection.Text;
        }
    }
}
