﻿using HtmlAgilityPack;

namespace MainCore.Commands.Features.Step.StartAdventure.ExploreAdventureCommandHandler
{
    [RegisterAsTransient(Common.Enums.ServerEnums.TTWars)]
    public class TTWars : ICommandHandler<ExploreAdventureCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public TTWars(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(ExploreAdventureCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var adventure = _heroParser.GetAdventure(html);
            if (adventure is null) return Retry.ButtonNotFound("adventure place");

            Result result;
            result = await chromeBrowser.Click(By.XPath(adventure.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            bool startShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var startButton = _heroParser.GetContinueButton(doc);
                return startButton is not null;
            };

            result = await chromeBrowser.Wait(startShow, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            html = chromeBrowser.Html;
            var startAdventureButton = _heroParser.GetContinueButton(html);

            result = await chromeBrowser.Click(By.XPath(startAdventureButton.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            bool continueShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var continueButton = _heroParser.GetContinueButton(doc);
                return continueButton is not null;
            };

            result = await chromeBrowser.Wait(continueShow, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}