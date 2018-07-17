﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using PiP_Tool.Helpers;
using PiP_Tool.Native;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace PiP_Tool.DataModel
{
    public class WindowInfo
    {

        #region public

        public IntPtr Handle { get; }
        public string Program { get; private set; }
        public string Title { get; private set; }
        public Point Position { get; private set; }
        public Size Size { get; private set; }
        public NativeStructs.Rect Rect { get; private set; }
        public NativeStructs.Rect Border { get; set; }

        public NativeStructs.Rect RectNoBorder => new NativeStructs.Rect(
            Position.X + Border.Left,
            Position.Y + Border.Top,
            Rect.Width - (Border.Left + Border.Right) + Position.X + Border.Left,
            Rect.Height - (Border.Top + Border.Bottom) + Position.Y + Border.Top
        );

        /// <summary>
        /// Gets if window is minimized
        /// </summary>
        public bool IsMinimized => (_winInfo.dwStyle & (uint)WindowStyles.WS_MINIMIZE) == (uint)WindowStyles.WS_MINIMIZE;

        #endregion

        #region private

        private NativeStructs.WINDOWINFO _winInfo;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handle">Handle of the window</param>
        public WindowInfo(IntPtr handle)
        {
            Handle = handle;
            RefreshInfo();
        }

        /// <summary>
        /// Refresh all window informations (size, position, title, style, border...)
        /// </summary>
        public void RefreshInfo()
        {
            GetSizeAndPosition();
            GetProgram();
            GetTitle();
            GetWinInfo();
            GetBorder();
        }

        /// <summary>
        /// set this window as foreground window
        /// </summary>
        public void SetAsForegroundWindow()
        {
            RefreshInfo();
            if (IsMinimized)
                NativeMethods.ShowWindow(Handle, ShowWindowCommands.Restore);
            RefreshInfo();
            NativeMethods.SetForegroundWindow(Handle);
        }

        /// <summary>
        /// Get window size and position
        /// </summary>
        private void GetSizeAndPosition()
        {
            if (!NativeMethods.GetWindowRect(Handle, out var rct)) return;
            Rect = rct;
            Position = new Point(rct.Left, rct.Top);
            Size = new Size(rct.Right - rct.Left + 1, rct.Bottom - rct.Top + 1);
        }

        /// <summary>
        /// Get window program
        /// </summary>
        private void GetProgram()
        {
            NativeMethods.GetWindowThreadProcessId(Handle, out var processId);
            if (processId == 0)
                return;
            Program = Process.GetProcessById((int)processId).ProcessName;
        }

        /// <summary>
        /// Get window title
        /// </summary>
        private void GetTitle()
        {
            var length = NativeMethods.GetWindowTextLength(Handle);
            if (length == 0) return;

            var builder = new StringBuilder(length);
            NativeMethods.GetWindowText(Handle, builder, length + 1);
            Title = builder.ToString();
        }

        /// <summary>
        /// Get window info (window style...)
        /// </summary>
        private void GetWinInfo()
        {
            _winInfo = new NativeStructs.WINDOWINFO();
            _winInfo.cbSize = (uint)Marshal.SizeOf(_winInfo);
            NativeMethods.GetWindowInfo(Handle, ref _winInfo);
        }

        /// <summary>
        /// Get window border size
        /// </summary>
        private void GetBorder()
        {
            NativeMethods.DwmGetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out NativeStructs.Rect frame, Marshal.SizeOf(typeof(NativeStructs.Rect)));

            Border = new NativeStructs.Rect(
                (int)(Math.Floor(frame.Left / ScaleHelper.ScalingFactor) - Rect.Left),
                (int)(Math.Floor(frame.Top / ScaleHelper.ScalingFactor) - Rect.Top),
                (int)(Rect.Right - Math.Ceiling(frame.Right / ScaleHelper.ScalingFactor)),
                (int)(Rect.Bottom - Math.Ceiling(frame.Bottom / ScaleHelper.ScalingFactor))
            );
        }

        /// <summary>
        /// Check if obj if is WindowInfo and compare handle
        /// </summary>
        /// <param name="obj">object to compare</param>
        /// <returns>Handles are equals</returns>
        public override bool Equals(object obj)
        {
            return obj is WindowInfo windowInfo && Handle.Equals(windowInfo.Handle);
        }

        /// <summary>
        /// Compare handle
        /// </summary>
        /// <param name="other">WindowInfo to compare</param>
        /// <returns>Handles are equals</returns>
        protected bool Equals(WindowInfo other)
        {
            return Handle.Equals(other.Handle);
        }

        /// <summary>
        /// Get hashcode of the Handle
        /// </summary>
        /// <returns>Hashcode of the Handle</returns>
        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }

        /// <summary>
        /// Equality operator override
        /// </summary>
        /// <param name="left">Left member of the comparison</param>
        /// <param name="right">Right member of the comparison</param>
        /// <returns>Handles are equals</returns>
        public static bool operator ==(WindowInfo left, WindowInfo right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Inequality operator override
        /// </summary>
        /// <param name="left">Left member of the comparison</param>
        /// <param name="right">Right member of the comparison</param>
        /// <returns>Handles are not equals</returns>
        public static bool operator !=(WindowInfo left, WindowInfo right)
        {
            return !Equals(left, right);
        }

    }
}
