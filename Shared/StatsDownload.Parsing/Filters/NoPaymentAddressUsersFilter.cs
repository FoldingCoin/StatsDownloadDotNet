﻿namespace StatsDownload.Parsing.Filters
{
    using System.Linq;

    using StatsDownload.Core.Interfaces;
    using StatsDownload.Core.Interfaces.DataTransfer;

    public class NoPaymentAddressUsersFilter : IStatsFileParserService
    {
        private readonly IStatsFileParserService innerService;

        private readonly INoPaymentAddressUsersFilterSettings settings;

        public NoPaymentAddressUsersFilter(IStatsFileParserService innerService,
                                           INoPaymentAddressUsersFilterSettings settings)
        {
            this.innerService = innerService;
            this.settings = settings;
        }

        public ParseResults Parse(FilePayload filePayload)
        {
            ParseResults results = innerService.Parse(filePayload);

            if (settings.Enabled)
            {
                return new ParseResults(results.DownloadDateTime,
                    results.UsersData.Where(data => !string.IsNullOrWhiteSpace(data.BitcoinAddress)),
                    results.FailedUsersData);
            }

            return results;
        }
    }
}