using Azure;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.AI;
using Microsoft.JSInterop;
using System.Linq.Expressions;
using System.Reflection;

namespace WebApp.Components
{
    public partial class AIChat : IDisposable
    {
        #region Fields
        private string? _text;
        private List<ChatMessage> _historyChat = [];
        private bool _loading = false;
        private IChatClient? _chatClient;
        private bool _changed = true;
        #endregion

        #region Properties
        [Inject] internal IServiceProvider? ServiceProvider { get; set; }
        [Inject] internal IJSRuntime? JS { get; set; }

        [Parameter] public string? ChatClientKey { get; set; }
        [Parameter] public RenderFragment? Suggestions { get; set; }
        [Parameter] public object? Tool { get; set; }
        #endregion

        #region ComponentBase Methods
        public override Task SetParametersAsync(ParameterView parameters)
        {
            if (parameters.TryGetValue(nameof(Tool), out object? tool) && !EqualityComparer<object>.Default.Equals(Tool, tool))
                _changed = true;
            return base.SetParametersAsync(parameters);
        }

        protected override void OnParametersSet()
        {
            if (!_changed) return;
            _changed = false;

            if (string.IsNullOrEmpty(ChatClientKey))
            {
                _chatClient = ServiceProvider?.GetService<IChatClient>();
                if (Tool != null)
                {
                    var methods = Tool.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(AIChatAttribute), false).Length > 0).ToArray();
                    _chatClient = _chatClient?.AsBuilder()
                        .ConfigureOptions(x =>
                        {
                            x.Tools = methods.Select(m => AIFunctionFactory.Create(CreateDelegateFromMethod(m, Tool), m.Name)).ToArray();
                        })
                        .UseFunctionInvocation().Build();
                }
            }
            else
                _chatClient = ServiceProvider?.GetKeyedService<IChatClient>(ChatClientKey);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var module = await JS!.InvokeAsync<IJSObjectReference>("import", "./Components/AIChat.razor.js");
                if (module != null)
                {
                    await module.InvokeVoidAsync("init");
                    try
                    {
                        await module.DisposeAsync();
                    }
                    catch (JSDisconnectedException) { }
                }
            }
        }
        #endregion

        #region Internal & Private Methods
        private async Task HandleSendAsync()
        {
            if (_chatClient == null) return;
            
            _loading = true;
            var message = new ChatMessage(ChatRole.User, _text ?? string.Empty);
            _historyChat.Add(message);
            var options = new ChatOptions() { MaxOutputTokens = 2048 };
            var response = await _chatClient.GetResponseAsync(_historyChat.Where(m => m.Role == ChatRole.User || m.Role == ChatRole.Assistant), options);
            if (response != null)
            {
                _historyChat.AddMessages(response);
            }
            _text = null;
            _loading = false;
        }

        private string GetMessageCssClass(ChatMessage message)
        {
            if (message.Role == ChatRole.User)
                return "user-message";
            else if (message.Role == ChatRole.Assistant)
                return "assistant-message";
            else if (message.Role == ChatRole.System)
                return "system-message";
            else if (message.Role == ChatRole.Tool)
                return "tool-message";
            else
                return "other-message";
        }

        internal async Task SendPrompt(string prompt)
        {
            _text = prompt;
            StateHasChanged();
            await HandleSendAsync();
            StateHasChanged();
        }

        private void HandleClear()
        {
            _historyChat.Clear();
        }

        private static Delegate CreateDelegateFromMethod(MethodInfo method, object target)
        {
            // Sceglie il delegate giusto in base alla firma del metodo
            var parameters = method.GetParameters();
            Type delegateType;
            if (method.ReturnType == typeof(void))
            {
                // Action, Action<T>, Action<T1, T2>, ...
                delegateType = Expression.GetActionType(parameters.Select(p => p.ParameterType).ToArray());
            }
            else
            {
                // Func<T>, Func<T1, T2, TResult>, ...
                var typeArgs = parameters.Select(p => p.ParameterType).Concat(new[] { method.ReturnType }).ToArray();
                delegateType = Expression.GetFuncType(typeArgs);
            }
            return target == null
                ? Delegate.CreateDelegate(delegateType, method)
                : Delegate.CreateDelegate(delegateType, target, method);
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            if (_chatClient != null) 
                _chatClient.Dispose();
        }
        #endregion
    }
}
