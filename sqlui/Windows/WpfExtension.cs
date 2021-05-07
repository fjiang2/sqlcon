using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Data;

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

            return currentColumnNumber + 1;
        }


        /// <summary>
        /// get selected text or all text if nothing is selected
        /// </summary>
        /// <param name="textBox"></param>
        /// <returns></returns>
        public static string GetSelectionOrAllText(this RichTextBox textBox)
        {
            string text;
            int i;

            if (!string.IsNullOrEmpty(textBox.Selection.Text))
            {
                text = textBox.Selection.Text;

                i = 0;
                while (i < text.Length)
                {
                    if (char.IsLetterOrDigit(text[i++]))
                        return text;
                }
            }

            TextRange textRange = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd);
            text = textRange.Text;

            i = 0;
            while (i < text.Length)
            {
                if (char.IsLetterOrDigit(text[i++]))
                    return text;
            }

            return string.Empty;
        }

        public static string GetAllText(this RichTextBox textBox)
        {
            TextRange textRange = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd);
            return textRange.Text;
        }

        public static DataTable AddLineNumberColumn(this DataTable dt)
        {
            DataColumn line = new DataColumn("Line", typeof(int))
            {
                Caption = string.Empty,
            };
            dt.Columns.Add(line);

            line.SetOrdinal(0);

            int k = 1;
            foreach (DataRow row in dt.Rows)
            {
                row[line] = k++;
            }

            dt.AcceptChanges();
            return dt;
        }
    }
}
