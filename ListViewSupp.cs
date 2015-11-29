// ============================================================================
// ListViewSupp
// ============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace TCore.ListViewSupp
{
    // ============================================================================
    // H E A D E R  S U P P
    // ============================================================================
    public class HeaderSupp
    {
        // The area occupied by the ListView header. 
        private Rectangle m_rcHeader;

        // Delegate that is called for each child window of the ListView. 
        private delegate bool EnumWinCallBack(IntPtr hwnd, IntPtr lParam);

        // Calls EnumWinCallBack for each child window of hWndParent (i.e. the ListView).
        [DllImport("user32.Dll")]
        private static extern int EnumChildWindows(
            IntPtr hWndParent,
            EnumWinCallBack callBackFunc,
            IntPtr lParam);

        // Gets the bounding rectangle of the specified window (ListView header bar). 
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private bool EnumWindowCallBack(IntPtr hwnd, IntPtr lParam)
        {
            // Determine the rectangle of the ListView header bar and save it in m_rcHeader.
            RECT rc;
            if (!GetWindowRect(hwnd, out rc))
                {
                m_rcHeader = Rectangle.Empty;
                }
            else
                {
                m_rcHeader = new Rectangle(rc.Left, rc.Top, rc.Right - rc.Left, rc.Bottom - rc.Top);
                }
            return false; // Stop the enum
        }

        public static ColumnHeader[] GetOrderedHeaders(ListView lv)
        {
            ColumnHeader[] rgColumnHeader = new ColumnHeader[lv.Columns.Count];

            foreach (ColumnHeader header in lv.Columns)
                {
                rgColumnHeader[header.DisplayIndex] = header;
                }

            return rgColumnHeader;
        }

        public ColumnHeader ColumnHeaderFromContextOpening(ListView lv, object sender, CancelEventArgs e)
        {
            // This call indirectly calls EnumWindowCallBack which sets _headerRect
            // to the area occupied by the ListView's header bar.
            EnumChildWindows(lv.Handle, new EnumWinCallBack(EnumWindowCallBack), IntPtr.Zero);

            // If the mouse position is in the header bar, cancel the display
            // of the regular context menu and display the column header context 
            // menu instead.
            if (m_rcHeader.Contains(Control.MousePosition))
                {
                e.Cancel = true;

                return ColumnFromPoint(lv, Control.MousePosition.X - m_rcHeader.Left);
                }
            return null; // either not in the header area, or couldn't find the header
        }

        /* C O L U M N  F R O M  P O I N T */
        /*----------------------------------------------------------------------------
        	%%Function: ColumnFromPoint
        	%%Qualified: TCore.ListViewSupp.HeaderSupp.ColumnFromPoint
        	%%Contact: rlittle
        	
            Given an X coordinate (in control relative units), return the columnheader
            for the column that the x is within.
        ----------------------------------------------------------------------------*/
        public static ColumnHeader ColumnFromPoint(ListView lv, int dx)
        {
            // The xoffset is how far the mouse is from the left edge of the header.

            // Iterate through the column headers in the order they are displayed, 
            // adding up their widths as we go.  When the sum exceeds the xoffset, 
            // we know the mouse is on the current header. 
            int dxSum = 0;

            foreach (ColumnHeader ch in GetOrderedHeaders(lv))
                {
                dxSum += ch.Width;
                if (dxSum > dx)
                    {
                    return ch;
                    }
                }
            return null;
        }
    }
}