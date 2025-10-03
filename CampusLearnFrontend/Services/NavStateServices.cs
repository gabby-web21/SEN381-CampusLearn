using System; 
using System.Linq;
using System.Collections.Generic;

namespace CampusLearnFrontend.Services
{
    // This manages the dock/undock & expanded/collapsed state so multiple components can react
    public class NavStateService
    {
        public bool IsExpanded { get; private set; } = true;
        public bool IsDocked { get; private set; } = true; // docked true = fixed bar; false = undocked floating

        public event Action OnChange;

        public void ToggleExpand()
        {
            IsExpanded = !IsExpanded;
            Notify();
        }
        public void ToggleDock()
        {
            IsDocked = !IsDocked;
            Notify();
        }
        private void Notify() => OnChange?.Invoke();
    }
}

