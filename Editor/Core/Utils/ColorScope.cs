using System;
using UnityEngine;

namespace Wireframe
{
    internal struct ColorScope : IDisposable
    {
        private bool m_Disposed;
        private Color m_PreviousColor;

        public ColorScope(Color newColor)
        {
            this.m_Disposed = false;
            this.m_PreviousColor = GUI.color;
            GUI.color = newColor;
        }

        public ColorScope(float r, float g, float b, float a = 1f)
            : this(new Color(r, g, b, a))
        {
        }

        public void Dispose()
        {
            if (this.m_Disposed)
                return;
            this.m_Disposed = true;
            GUI.color = this.m_PreviousColor;
        }
    }
}