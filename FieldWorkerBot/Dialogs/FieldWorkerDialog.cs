using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.FormFlow;
using System.Collections.Generic;
using FieldWorkerBot.DataAccess;
using FieldWorkerBot.Services;
using System.Linq;
using System.Globalization;
using Newtonsoft.Json;

namespace FieldWorkerBot.Dialogs
{
    [Serializable]
    internal class FieldWorkerDialog : LuisDialog<object>
    {
        Random random = new Random();
        IAwaitable<IMessageActivity> messageActivity;
        string substationName = null;
        List<DiscrepancyFormDialogBuilder> discrepancyFormBuilders = new List<DiscrepancyFormDialogBuilder>();

        public FieldWorkerDialog(LuisService service) : base(service)
        {
        }

        protected override Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> messageActivity)
        {
            this.messageActivity = messageActivity;
            return base.MessageReceived(context, messageActivity);
        }


        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm sorry, I didn't understand that.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("greet")]
        public async Task Greet(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hello, I'm André. How may I assist you today?");
            context.Wait(MessageReceived);
        }

        [LuisIntent("help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm here to help you register discrepancies found in substations during inspections. I also have a very soothing voice for you to enjoy.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("angry")]
        public async Task Angry(IDialogContext context, LuisResult result)
        {
            var responses = new[]
            {
                "Hey, be careful there!",
                "I'm only a bot you know.",
                "Do you kiss your grandma with that mouth?",
                "Is that all you've got? I've heard toddlers throwing worse insults than that!"
            };
            var message = responses[random.Next(responses.Length)];
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("happy")]
        public async Task Happy(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("That's so kind of you!");
            context.Wait(MessageReceived);
        }

        [LuisIntent("getDiscrepancyType")]
        public async Task SelectControlPoints(IDialogContext context, LuisResult result)
        {
            if (String.IsNullOrEmpty(substationName))
            {
                await context.PostAsync($"You need to tell what substation you want to select.");
                context.Wait(MessageReceived);
            }
            else
            {
                
                DiscrepancyTypeService discrepancyService = new DiscrepancyTypeService(new DiscrepancyTypeRepository(new InspectionRepository(new AssetRepository()), new AssetRepository()));
                AssetService assetService = new AssetService(new AssetRepository());
                InspectionService inspectionService = new InspectionService(new InspectionRepository(new AssetRepository()));
                var asset = await assetService.GetByName(substationName);
                var inspection = await inspectionService.GetNotPerformedByAssetId(asset.ObjectId);
                var discrepancyTypes = await discrepancyService.Get(asset.ObjectId, inspection.Id);
       
                await context.PostAsync($"You have your discrepancy types here:\n {JsonConvert.SerializeObject(discrepancyTypes)}");
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("assetAllGood")]
        public async Task AssetAllGood(IDialogContext context, LuisResult result)
        {
            if (String.IsNullOrEmpty(substationName))
            {
                await context.PostAsync($"You need to tell what substation you want to select.");
                context.Wait(MessageReceived);
            }
            else
            {
                PromptDialog.Confirm(context, AfterConfirmAsync, "Are you sure that you want to finish this inspection?");
                await Task.FromResult<Object>(null);
            }
        }

        [LuisIntent("reportDiscrepancy")]
        public async Task ReportDiscrepancy(IDialogContext context, LuisResult result)
        {
            EntityRecommendation discrepancyTypeEntity;
            var discrepancyDialog = new DiscrepancyFormDialogBuilder();

            if (result.TryFindEntity("ControlPoint", out discrepancyTypeEntity))
            {
                var discrepancyType = discrepancyTypeEntity.Entity.ToUpper();
                discrepancyDialog = new DiscrepancyFormDialogBuilder { DiscrepancyType = discrepancyType };
            }

            var form = new FormDialog<DiscrepancyFormDialogBuilder>(
                    discrepancyDialog,
                    DiscrepancyFormDialogBuilder.BuildForm,
                    FormOptions.PromptInStart,
                    result.Entities);

            context.Call(form, AfterDiscrepancyAsync);
        }

        [LuisIntent("commitReport")]
        public async Task CommitReport(IDialogContext context, LuisResult result)
        {
            if (String.IsNullOrEmpty(substationName))
            {
                await context.PostAsync($"You need to tell what substation you want to select.");
            }
            else
            {
                await context.PostAsync($"Committing inspection on substation {substationName} with {discrepancyFormBuilders.Count} discrepancies.");
                discrepancyFormBuilders.Clear();
            }

            context.Wait(MessageReceived);
        }

        [LuisIntent("selectSubstation")]
        public async Task SelectSubstation(IDialogContext context, LuisResult result)
        {
            AssetService assetService = new AssetService(new AssetRepository());
            string extractedName;
            string message = "I'm sorry, I didn't catch the name of the substation.";

            if (TryExtractingSubstationName(result, out extractedName))
            {
                var asset = await assetService.GetByName(extractedName);
                substationName = asset != null ? extractedName : null;
                message = asset != null ? $"The substation {substationName} has been chosen." : $"Sorry, I didn't find a substation called '{extractedName}'.";
            }

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("checkMyReport")]
        public async Task CheckMyReport(IDialogContext context, LuisResult result)
        {
            if (!String.IsNullOrEmpty(substationName))
            {
                await context.PostAsync($"You have selected substation {substationName}.");
            }

            await context.PostAsync($"You have logged {discrepancyFormBuilders.Count} discrepancies.");

            foreach (var discrepancy in discrepancyFormBuilders)
                await context.PostAsync($"Type {discrepancy.DiscrepancyType}. Comment: \"{discrepancy.Comment}\"");

            context.Wait(MessageReceived);
        }

        public async Task AfterConfirmAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                AssetService assetService = new AssetService(new AssetRepository());
                InspectionService inspectionService = new InspectionService(new InspectionRepository(new AssetRepository()));
                var asset = await assetService.GetByName(substationName);
                var inspection = await inspectionService.GetNotPerformedByAssetId(asset.ObjectId);
                inspection.InspectionDate = DateTime.Now;
                inspection.StatusCode = 0;
                await inspectionService.Commit(inspection);

                await context.PostAsync($"Committing inspection on substation {substationName} as everything is ok");
            }
            else
            {
                await context.PostAsync($"Committing inspection on substation {substationName} is canceled");
            }
            context.Wait(MessageReceived);
        }

        private async Task AfterDiscrepancyAsync(IDialogContext context, IAwaitable<DiscrepancyFormDialogBuilder> result)
        {
            var discrepancy = await result;
            discrepancyFormBuilders.Add(discrepancy);
            await context.PostAsync($"OK, registering a discrepancy of type {discrepancy.DiscrepancyType} with the following comment: {discrepancy.Comment}");
            context.Wait(MessageReceived);
        }

        bool TryExtractingSubstationName(LuisResult result, out string substationName)
        {
            EntityRecommendation substationEntity;

            if (result.TryFindEntity("SubstationName", out substationEntity))
            {
                substationName = substationEntity.Entity.ToUpper().Replace(" ", "");

                if (ContainsDigits(substationName))
                    return true;
            }

            if (TryExtractingSubstationName(result.Query, out substationName))
            {
                substationName = substationName.Replace(" ", "");

                if (ContainsDigits(substationName))
                    return true;
            }

            substationName = "";
            return false;
        }

        private bool TryExtractingSubstationName(string query, out string substationName)
        {
            substationName = "";
            var matches = Regex.Matches(query, "\\w{1,5}\\s?\\d{1,5}");
            if (matches.Count == 0) return false;
            substationName = matches[0].Value;
            return true;
        }

        private bool ContainsDigits(string s)
        {
            foreach (char c in s)
                if (char.IsDigit(c))
                    return true;

            return false;
        }

    }
}