namespace ProjectBase.UI.Editor
{
    internal static class UIScriptTemplates
    {
        public static string GetScreenViewTemplate(string className, string namespaceName)
        {
            return $@"using UnityEngine;
using ProjectBase.UI.Core;

namespace {namespaceName}
{{
    public class {className} : BaseScreen
    {{
        public override void OnNavigatedTo(object parameter = null)
        {{
            base.OnNavigatedTo(parameter);
        }}

        public override void OnNavigatedFrom()
        {{
            base.OnNavigatedFrom();
        }}
    }}
}}
";
        }

        public static string GetPopupViewTemplate(string className, string namespaceName)
        {
            return $@"using UnityEngine;
using ProjectBase.UI.Core;

namespace {namespaceName}
{{
    public class {className} : BasePopup
    {{
        public override void SetupData(object data = null)
        {{
            base.SetupData(data);
        }}
    }}
}}
";
        }

        public static string GetOverlayViewTemplate(string className, string namespaceName)
        {
            return $@"using UnityEngine;
using ProjectBase.UI.Core;

namespace {namespaceName}
{{
    public class {className} : BaseView
    {{
    }}
}}
";
        }

        public static string GetPresenterTemplate(string presenterName, string viewName,
            string namespaceName)
        {
            return $@"using ProjectBase.UI.Core;

namespace {namespaceName}
{{
    public class {presenterName} : BasePresenter<{viewName}>
    {{
        public {presenterName}({viewName} view) : base(view) {{ }}

        public override void Initialize(object parameter = null)
        {{
        }}

        public override void Dispose()
        {{
        }}
    }}
}}
";
        }
    }
}
