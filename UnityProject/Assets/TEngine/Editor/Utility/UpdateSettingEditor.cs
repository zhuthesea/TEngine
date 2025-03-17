using HybridCLR.Editor.Settings;
using UnityEditor;

namespace TEngine.Editor
{
    [CustomEditor(typeof(UpdateSetting), true)]
    public class UpdateSettingEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // 记录对象修改前的状态
            EditorGUI.BeginChangeCheck();

            // 绘制默认的 Inspector 界面
            base.OnInspectorGUI();

            // 检测是否有字段被修改
            if (EditorGUI.EndChangeCheck())
            {
                // 获取当前编辑的 ScriptableObject 实例
                UpdateSetting so = (UpdateSetting)target;

                // 标记对象为“已修改”，确保修改能被保存
                EditorUtility.SetDirty(so);

                HybridCLRSettings.Instance.hotUpdateAssemblies = so.HotUpdateAssemblies.ToArray();
                HybridCLRSettings.Instance.patchAOTAssemblies = so.AOTMetaAssemblies.ToArray();
            }
        }
    }
}