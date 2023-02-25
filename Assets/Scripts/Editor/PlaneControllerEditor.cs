using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(PlaneController))]
public class PlaneControllerEditor : Editor
{
    public VisualTreeAsset m_InspectorXML;
    public override VisualElement CreateInspectorGUI()
    {
        // root of inspector
        VisualElement inspector = new VisualElement();

        // clone visual tree from UXML
        m_InspectorXML.CloneTree(inspector);

        return inspector;
    }
}
