using System;
using UnityEngine;

namespace TEngine
{
    public sealed partial class Debugger
    {
        private sealed class SettingsWindow : ScrollableDebuggerWindowBase
        {
            private Debugger _debugger = null;
            private float m_LastIconX = 0f;
            private float m_LastIconY = 0f;
            private float m_LastWindowX = 0f;
            private float m_LastWindowY = 0f;
            private float m_LastWindowWidth = 0f;
            private float m_LastWindowHeight = 0f;
            private float m_LastWindowScale = 0f;

            public override void Initialize(params object[] args)
            {
                _debugger = Debugger.Instance;
                if (_debugger == null)
                {
                    Log.Fatal("Debugger component is invalid.");
                    return;
                }

                m_LastIconX = PlayerPrefs.GetFloat("Debugger.Icon.X", DefaultIconRect.x);
                m_LastIconY = PlayerPrefs.GetFloat("Debugger.Icon.Y", DefaultIconRect.y);
                m_LastWindowX = PlayerPrefs.GetFloat("Debugger.Window.X", DefaultWindowRect.x);
                m_LastWindowY = PlayerPrefs.GetFloat("Debugger.Window.Y", DefaultWindowRect.y);
                m_LastWindowWidth = PlayerPrefs.GetFloat("Debugger.Window.Width", DefaultWindowRect.width);
                m_LastWindowHeight = PlayerPrefs.GetFloat("Debugger.Window.Height", DefaultWindowRect.height);
                
                _debugger.WindowScale = m_LastWindowScale = PlayerPrefs.GetFloat("Debugger.Window.Scale", DefaultWindowScale);
                _debugger.IconRect = new Rect(m_LastIconX, m_LastIconY, DefaultIconRect.width, DefaultIconRect.height);
                _debugger.WindowRect = new Rect(m_LastWindowX, m_LastWindowY, m_LastWindowWidth, m_LastWindowHeight);
            }

            public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
                if (Math.Abs(m_LastIconX - _debugger.IconRect.x) > 0.01f)
                {
                    m_LastIconX = _debugger.IconRect.x;
                    PlayerPrefs.SetFloat("Debugger.Icon.X", _debugger.IconRect.x);
                }

                if (Math.Abs(m_LastIconY - _debugger.IconRect.y) > 0.01f)
                {
                    m_LastIconY = _debugger.IconRect.y;
                    PlayerPrefs.SetFloat("Debugger.Icon.Y", _debugger.IconRect.y);
                }

                if (Math.Abs(m_LastWindowX - _debugger.WindowRect.x) > 0.01f)
                {
                    m_LastWindowX = _debugger.WindowRect.x;
                    PlayerPrefs.SetFloat("Debugger.Window.X", _debugger.WindowRect.x);
                }

                if (Math.Abs(m_LastWindowY - _debugger.WindowRect.y) > 0.01f)
                {
                    m_LastWindowY = _debugger.WindowRect.y;
                    PlayerPrefs.SetFloat("Debugger.Window.Y", _debugger.WindowRect.y);
                }

                if (Math.Abs(m_LastWindowWidth - _debugger.WindowRect.width) > 0.01f)
                {
                    m_LastWindowWidth = _debugger.WindowRect.width;
                    PlayerPrefs.SetFloat("Debugger.Window.Width", _debugger.WindowRect.width);
                }

                if (Math.Abs(m_LastWindowHeight - _debugger.WindowRect.height) > 0.01f)
                {
                    m_LastWindowHeight = _debugger.WindowRect.height;
                    PlayerPrefs.SetFloat("Debugger.Window.Height", _debugger.WindowRect.height);
                }

                if (Math.Abs(m_LastWindowScale - _debugger.WindowScale) > 0.01f)
                {
                    m_LastWindowScale = _debugger.WindowScale;
                    PlayerPrefs.SetFloat("Debugger.Window.Scale", _debugger.WindowScale);
                }
            }

            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Window Settings</b>");
                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Position:", GUILayout.Width(60f));
                        GUILayout.Label("Drag window caption to move position.");
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        float width = _debugger.WindowRect.width;
                        GUILayout.Label("Width:", GUILayout.Width(60f));
                        if (GUILayout.RepeatButton("-", GUILayout.Width(30f)))
                        {
                            width--;
                        }

                        width = GUILayout.HorizontalSlider(width, 100f, Screen.width - 20f);
                        if (GUILayout.RepeatButton("+", GUILayout.Width(30f)))
                        {
                            width++;
                        }

                        width = Mathf.Clamp(width, 100f, Screen.width - 20f);
                        if (Math.Abs(width - _debugger.WindowRect.width) > 0.01f)
                        {
                            _debugger.WindowRect = new Rect(_debugger.WindowRect.x, _debugger.WindowRect.y, width,
                                _debugger.WindowRect.height);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        float height = _debugger.WindowRect.height;
                        GUILayout.Label("Height:", GUILayout.Width(60f));
                        if (GUILayout.RepeatButton("-", GUILayout.Width(30f)))
                        {
                            height--;
                        }

                        height = GUILayout.HorizontalSlider(height, 100f, Screen.height - 20f);
                        if (GUILayout.RepeatButton("+", GUILayout.Width(30f)))
                        {
                            height++;
                        }

                        height = Mathf.Clamp(height, 100f, Screen.height - 20f);
                        if (Math.Abs(height - _debugger.WindowRect.height) > 0.01f)
                        {
                            _debugger.WindowRect = new Rect(_debugger.WindowRect.x, _debugger.WindowRect.y,
                                _debugger.WindowRect.width, height);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        float scale = _debugger.WindowScale;
                        GUILayout.Label("Scale:", GUILayout.Width(60f));
                        if (GUILayout.RepeatButton("-", GUILayout.Width(30f)))
                        {
                            scale -= 0.01f;
                        }

                        scale = GUILayout.HorizontalSlider(scale, 0.5f, 4f);
                        if (GUILayout.RepeatButton("+", GUILayout.Width(30f)))
                        {
                            scale += 0.01f;
                        }

                        scale = Mathf.Clamp(scale, 0.5f, 4f);
                        if (Math.Abs(scale - _debugger.WindowScale) > 0.01f)
                        {
                            _debugger.WindowScale = scale;
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("0.5x", GUILayout.Height(60f)))
                        {
                            _debugger.WindowScale = 0.5f;
                        }

                        if (GUILayout.Button("1.0x", GUILayout.Height(60f)))
                        {
                            _debugger.WindowScale = 1f;
                        }

                        if (GUILayout.Button("1.5x", GUILayout.Height(60f)))
                        {
                            _debugger.WindowScale = 1.5f;
                        }

                        if (GUILayout.Button("2.0x", GUILayout.Height(60f)))
                        {
                            _debugger.WindowScale = 2f;
                        }

                        if (GUILayout.Button("2.5x", GUILayout.Height(60f)))
                        {
                            _debugger.WindowScale = 2.5f;
                        }

                        if (GUILayout.Button("3.0x", GUILayout.Height(60f)))
                        {
                            _debugger.WindowScale = 3f;
                        }

                        if (GUILayout.Button("3.5x", GUILayout.Height(60f)))
                        {
                            _debugger.WindowScale = 3.5f;
                        }

                        if (GUILayout.Button("4.0x", GUILayout.Height(60f)))
                        {
                            _debugger.WindowScale = 4f;
                        }
                    }
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Reset Layout", GUILayout.Height(30f)))
                    {
                        _debugger.ResetLayout();
                    }
                }
                GUILayout.EndVertical();
            }

            public override void OnLeave()
            {
                PlayerPrefs.Save();
            }
        }
    }
}