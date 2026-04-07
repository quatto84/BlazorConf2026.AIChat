using Microsoft.AspNetCore.Components;

namespace WebApp.Components
{
    public partial class AIChatSuggestion
    {
        #region Fields

        #endregion

        #region Properties
        [CascadingParameter] public AIChat? Parent { get; set; }
        [Parameter, EditorRequired] public string? DisplayName { get; set; }
        [Parameter, EditorRequired] public string? Prompt { get; set; }
        #endregion

        #region ComponentBase Methods

        #endregion

        #region Internal & Private Methods
        private async Task HandleClick()
        {
            if (Parent == null || string.IsNullOrEmpty(Prompt)) return;
            await Parent.SendPrompt(Prompt);
        }
        #endregion

        #region Public Methods

        #endregion
    }
}
