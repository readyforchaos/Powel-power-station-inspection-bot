using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FieldWorkerBot.Dialogs
{
    [Serializable]
    public class DiscrepancyFormDialogBuilder
    {
        public string DiscrepancyType;
        public string Comment;

        public static IForm<DiscrepancyFormDialogBuilder> BuildForm()
        {
            return new FormBuilder<DiscrepancyFormDialogBuilder>()
                .AddRemainingFields()
                .Build();
        }

        public static IFormDialog<DiscrepancyFormDialogBuilder> Build(FormOptions options = FormOptions.PromptInStart)
        {
            // Generated a new FormDialog<T> based on IForm<BasicForm>
            return FormDialog.FromForm(BuildForm, options);
        }
    };
}