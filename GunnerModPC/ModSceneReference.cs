using System.Reflection;

namespace GHCPMissionsMod
{
    public class ModSceneReference : Eflatun.SceneReference.SceneReference
    {
        public string UniqueModMissionName;
        public string Name
        {
            get { return UniqueModMissionName; }
        }

        public ModSceneReference(Eflatun.SceneReference.SceneReference template)
        {
            FieldInfo guidField = typeof(Eflatun.SceneReference.SceneReference).GetField("sceneAssetGuidHex", BindingFlags.Instance | BindingFlags.NonPublic);
            guidField.SetValue(this, guidField.GetValue(template));
        }
    }
}
