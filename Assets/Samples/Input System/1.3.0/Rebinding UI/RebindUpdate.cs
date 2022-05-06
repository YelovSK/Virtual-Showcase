namespace UnityEngine.InputSystem.Samples.RebindUI
{
    public class RebindUpdate : MonoBehaviour
    {
        public void UpdateBindsInUI()
        {
            RebindActionUI[] rebinds = GetComponentsInChildren<RebindActionUI>();
            foreach (RebindActionUI rebind in rebinds)
                rebind.UpdateBindingDisplay();
        }
    }
}